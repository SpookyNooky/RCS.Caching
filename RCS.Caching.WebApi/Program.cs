using Microsoft.AspNetCore.Mvc;
using RCS.Caching.Infrastructure.Memory.Extensions;
using RCS.Caching.Infrastructure.Memory.Stores;
using RCS.Caching.Infrastructure.Redis.Extensions;
using RCS.Caching.Infrastructure.Redis.Stores;
using RCS.Caching.Infrastructure.Sql.Extensions;
using RCS.Caching.Infrastructure.Sql.Stores;
using RCS.Caching.Tests.Models;
using RCS.Caching.WebApi.Resolvers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register all cache implementations (interfaces)
builder.Services.AddInMemoryCache<string, SomeTestDto>();
builder.Services.AddSqlCacheStore<string, SomeTestDto>("Server=localhost;Database=RCS.Caching;User Id=sa;Password=Spooky58466!;TrustServerCertificate=True;");
builder.Services.AddRedisCacheStore<string, SomeTestDto>("localhost:6379");

// Explicitly register concrete implementations for resolver
builder.Services.AddSingleton<MemoryCacheStore<string, SomeTestDto>>();
builder.Services.AddSingleton<RedisCacheStore<string, SomeTestDto>>();
builder.Services.AddScoped<SqlCacheStore<string, SomeTestDto>>();

// Register resolver (must be scoped to match SqlCacheStore lifetime)
builder.Services.AddScoped<CacheStoreResolver<string, SomeTestDto>>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Caching API v1");
});

// Redirect root URL to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

// Store a value under a specified key using the selected cache backend
app.MapPost("/cache/{store}/{key}", async (
    [FromRoute] string store,
    [FromRoute] string key,
    [FromBody] SomeTestDto dto,
    [FromServices] CacheStoreResolver<string, SomeTestDto> resolver) =>
{
    var cache = resolver.Resolve(store);
    await cache.SetAsync(key, dto);
    return Results.Ok("Stored.");
});

// Retrieve a cached value by key from the selected cache backend
app.MapGet("/cache/{store}/{key}", async (
    [FromRoute] string store,
    [FromRoute] string key,
    [FromServices] CacheStoreResolver<string, SomeTestDto> resolver) =>
{
    var cache = resolver.Resolve(store);
    var value = await cache.GetAsync(key);
    return value is not null ? Results.Ok(value) : Results.NotFound();
});

// Remove a cached entry by key from the selected cache backend
app.MapDelete("/cache/{store}/{key}", async (
    [FromRoute] string store,
    [FromRoute] string key,
    [FromServices] CacheStoreResolver<string, SomeTestDto> resolver) =>
{
    var cache = resolver.Resolve(store);
    var removed = await cache.RemoveAsync(key);
    return removed ? Results.Ok("Removed.") : Results.NotFound();
});

// Check if a key exists in the selected cache backend (no body returned)
app.MapMethods("/cache/{store}/{key}", ["HEAD"], async (
    [FromRoute] string store,
    [FromRoute] string key,
    [FromServices] CacheStoreResolver<string, SomeTestDto> resolver) =>
{
    var cache = resolver.Resolve(store);
    var exists = await cache.ExistsAsync(key);
    return exists ? Results.Ok() : Results.NotFound();
});

app.Run();

namespace RCS.Caching.WebApi
{
    public partial class Program { }
} // Needed for WebApplicationFactory support in integration tests