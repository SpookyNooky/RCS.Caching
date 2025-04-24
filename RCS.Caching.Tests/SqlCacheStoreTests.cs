using Microsoft.Data.SqlClient;
using RCS.Caching.Infrastructure.Sql.Stores;
using RCS.Caching.Tests.Models;

namespace RCS.Caching.Tests;

/// <summary>
/// Unit tests for the <see cref="SqlCacheStore{TKey, TValue}"/> implementation,
/// verifying persistence, expiration, and concurrency-safe behavior via snapshot isolation.
/// </summary>
public class SqlCacheStoreTests
{
    /// <summary>
    /// SQL Server connection string pointing to a test database with memory-optimized table support.
    /// </summary>
    private const string ConnectionString = "Server=localhost;Database=RCS.Caching;User Id=sa;Password=Spooky58466!;TrustServerCertificate=True;";

    /// <summary>
    /// Instantiates a new <see cref="SqlCacheStore{TKey, TValue}"/> for isolated test runs.
    /// </summary>
    private static SqlCacheStore<string, SomeTestDto> CreateStore()
    {
        return new SqlCacheStore<string, SomeTestDto>(new SqlConnection(ConnectionString));
    }

    /// <summary>
    /// Generates a unique test key to avoid key collisions and side effects between tests.
    /// </summary>
    private static string GetUniqueKey() => $"test:{Guid.NewGuid()}";

    /// <summary>
    /// Ensures that values written to SQL cache are accurately retrievable, with data integrity preserved.
    /// </summary>
    [Fact]
    public async Task SetAndGet_ShouldReturnSameValue()
    {
        var store = CreateStore();
        var key = GetUniqueKey();
        var value = new SomeTestDto { Name = "test", Count = 123 };

        await store.SetAsync(key, value);
        var fetched = await store.GetAsync(key, useSnapshotHint: true);

        Assert.NotNull(fetched);
        Assert.Equal(value.Name, fetched.Name);
        Assert.Equal(value.Count, fetched.Count);
    }

    /// <summary>
    /// Verifies that the <c>RemoveAsync</c> method deletes an entry and that it is no longer retrievable afterward.
    /// </summary>
    [Fact]
    public async Task Remove_ShouldDeleteValue()
    {
        var store = CreateStore();
        var key = GetUniqueKey();
        await store.SetAsync(key, new SomeTestDto { Name = "remove", Count = 1 });

        var deleted = await store.RemoveAsync(key);
        var result = await store.GetAsync(key, useSnapshotHint: true);

        Assert.True(deleted);
        Assert.Null(result);
    }

    /// <summary>
    /// Confirms that the <c>ExistsAsync</c> method detects the presence of a valid entry in the cache.
    /// </summary>
    [Fact]
    public async Task Exists_ShouldReturnTrueIfExists()
    {
        var store = CreateStore();
        var key = GetUniqueKey();
        await store.SetAsync(key, new SomeTestDto { Name = "exists", Count = 42 });

        var exists = await store.ExistsAsync(key, useSnapshotHint: true);

        Assert.True(exists);
    }

    /// <summary>
    /// Validates that a cache entry with a TTL expires and is no longer returned after expiration time passes.
    /// </summary>
    [Fact]
    public async Task ExpiredKey_ShouldNotBeReturned()
    {
        var store = CreateStore();
        var key = GetUniqueKey();
        await store.SetAsync(key, new SomeTestDto { Name = "expire", Count = 999 }, TimeSpan.FromSeconds(1));

        await Task.Delay(1500); // Wait for TTL to elapse
        var result = await store.GetAsync(key, useSnapshotHint: true);

        Assert.Null(result);
    }
}
