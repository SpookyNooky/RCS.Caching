# Set up project folder
$solutionName = "CachingLibrary"
$apiProject = "$solutionName.WebApi"
New-Item -ItemType Directory -Force -Path $solutionName | Out-Null
Set-Location $solutionName

# Create solution
dotnet new sln -n $solutionName

# Create Web API project
dotnet new web -n $apiProject --no-https
dotnet sln add "$apiProject/$apiProject.csproj"

# Create DTO directory and file
$dtoPath = "$apiProject/Models"
New-Item -ItemType Directory -Force -Path $dtoPath | Out-Null
@"
namespace $apiProject.Models;

public class SomeTestDto
{
    public string Name { get; set; } = default!;
    public int Count { get; set; }
}
"@ | Set-Content -Encoding UTF8 "$dtoPath/SomeTestDto.cs"

# Add NuGet packages
Set-Location $apiProject
dotnet add package Microsoft.Extensions.Caching.Memory
dotnet add package Microsoft.Data.SqlClient
dotnet add package StackExchange.Redis
dotnet add package Swashbuckle.AspNetCore

# Replace Program.cs
@"
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using $apiProject.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Caching API", Version = "v1" }));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheStore<string, SomeTestDto>, MemoryCacheStore<string, SomeTestDto>>();
builder.Services.AddSingleton(provider => ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddSingleton<ICacheStore<string, SomeTestDto>>(provider => new RedisCacheStore<string, SomeTestDto>(provider.GetRequiredService<ConnectionMultiplexer>()));
builder.Services.AddScoped(provider => new SqlConnection("Server=localhost;Database=RCS.Caching;User Id=sa;Password=Spooky58466!;TrustServerCertificate=True;"));
builder.Services.AddScoped<ICacheStore<string, SomeTestDto>, SqlCacheStore<string, SomeTestDto>>();
builder.Services.AddSingleton<CacheStoreResolver>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/cache/{store}/{key}", async (string store, string key, SomeTestDto dto, CacheStoreResolver resolver) =>
{
    var cache = resolver.Resolve(store);
    await cache.SetAsync(key, dto);
    return Results.Ok("Stored.");
});

app.MapGet("/cache/{store}/{key}", async (string store, string key, CacheStoreResolver resolver) =>
{
    var cache = resolver.Resolve(store);
    var value = await cache.GetAsync(key);
    return value is not null ? Results.Ok(value) : Results.NotFound();
});

app.MapDelete("/cache/{store}/{key}", async (string store, string key, CacheStoreResolver resolver) =>
{
    var cache = resolver.Resolve(store);
    var removed = await cache.RemoveAsync(key);
    return removed ? Results.Ok("Removed.") : Results.NotFound();
});

app.MapMethods("/cache/{store}/{key}", new[] { "HEAD" }, async (string store, string key, CacheStoreResolver resolver) =>
{
    var cache = resolver.Resolve(store);
    var exists = await cache.ExistsAsync(key);
    return exists ? Results.Ok() : Results.NotFound();
});

app.Run();

interface ICacheStore<TKey, TValue>
{
    Task SetAsync(TKey key, TValue value, TimeSpan? ttl = null);
    Task<TValue?> GetAsync(TKey key);
    Task<bool> RemoveAsync(TKey key);
    Task<bool> ExistsAsync(TKey key);
}

class MemoryCacheStore<TKey, TValue>(IMemoryCache memoryCache) : ICacheStore<TKey, TValue> where TKey : notnull
{
    public Task SetAsync(TKey key, TValue value, TimeSpan? ttl = null)
    {
        var options = ttl.HasValue ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl } : null;
        memoryCache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task<TValue?> GetAsync(TKey key) => Task.FromResult(memoryCache.TryGetValue(key, out TValue? value) ? value : default);
    public Task<bool> RemoveAsync(TKey key) { memoryCache.Remove(key); return Task.FromResult(true); }
    public Task<bool> ExistsAsync(TKey key) => Task.FromResult(memoryCache.TryGetValue(key, out _));
}

class RedisCacheStore<TKey, TValue>(IConnectionMultiplexer redis) : ICacheStore<TKey, TValue> where TKey : notnull
{
    private readonly IDatabase db = redis.GetDatabase();
    public async Task SetAsync(TKey key, TValue value, TimeSpan? ttl = null) =>
        await db.StringSetAsync(key.ToString(), System.Text.Json.JsonSerializer.Serialize(value), ttl);

    public async Task<TValue?> GetAsync(TKey key)
    {
        var result = await db.StringGetAsync(key.ToString());
        return result.HasValue ? System.Text.Json.JsonSerializer.Deserialize<TValue>(result!) : default;
    }

    public Task<bool> RemoveAsync(TKey key) => db.KeyDeleteAsync(key.ToString());
    public Task<bool> ExistsAsync(TKey key) => db.KeyExistsAsync(key.ToString());
}

class SqlCacheStore<TKey, TValue>(SqlConnection connection) : ICacheStore<TKey, TValue> where TKey : notnull
{
    public async Task SetAsync(TKey key, TValue value, TimeSpan? ttl = null)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE dbo.CacheEntries SET [Value] = @Value, ExpiresOn = @ExpiresOn WHERE [Key] = @Key;
            IF @@ROWCOUNT = 0 INSERT INTO dbo.CacheEntries ([Key], [Value], ExpiresOn) VALUES (@Key, @Value, @ExpiresOn);
        """;
        cmd.Parameters.AddWithValue("@Key", key.ToString());
        cmd.Parameters.AddWithValue("@Value", System.Text.Json.JsonSerializer.Serialize(value));
        cmd.Parameters.AddWithValue("@ExpiresOn", ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : DBNull.Value);
        if (connection.State != 'Open') await connection.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<TValue?> GetAsync(TKey key)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT [Value] FROM dbo.CacheEntries WITH (SNAPSHOT) WHERE [Key] = @Key AND (ExpiresOn IS NULL OR ExpiresOn > GETUTCDATE())";
        cmd.Parameters.AddWithValue("@Key", key.ToString());
        if (connection.State != 'Open') await connection.OpenAsync();
        var result = await cmd.ExecuteScalarAsync() as string;
        return result != null ? System.Text.Json.JsonSerializer.Deserialize<TValue>(result) : default;
    }

    public async Task<bool> RemoveAsync(TKey key)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.CacheEntries WITH (SNAPSHOT) WHERE [Key] = @Key";
        cmd.Parameters.AddWithValue("@Key", key.ToString());
        if (connection.State != 'Open') await connection.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> ExistsAsync(TKey key)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM dbo.CacheEntries WITH (SNAPSHOT) WHERE [Key] = @Key AND (ExpiresOn IS NULL OR ExpiresOn > GETUTCDATE())";
        cmd.Parameters.AddWithValue("@Key", key.ToString());
        if (connection.State != 'Open') await connection.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();
        return result != null;
    }
}

class CacheStoreResolver
{
    private readonly IServiceProvider provider;
    public CacheStoreResolver(IServiceProvider provider) => this.provider = provider;

    public ICacheStore<string, SomeTestDto> Resolve(string name) => name.ToLower() switch
    {
        "memory" => provider.GetRequiredService<MemoryCacheStore<string, SomeTestDto>>(),
        "sql" => provider.GetRequiredService<SqlCacheStore<string, SomeTestDto>>(),
        "redis" => provider.GetRequiredService<RedisCacheStore<string, SomeTestDto>>(),
        _ => throw new ArgumentException($"Unknown store: {name}")
    };
}
"@ | Set-Content -Encoding UTF8 "Program.cs"

Write-Host "✅ Web API '$apiProject' is ready. Run with:"
Write-Host "cd $apiProject"
Write-Host "dotnet run"
