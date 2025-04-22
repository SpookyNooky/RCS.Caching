using CachingLibrary.Infrastructure.Redis.Stores;
using CachingLibrary.Tests.Models;
using StackExchange.Redis;

namespace CachingLibrary.Tests;

/// <summary>
/// Unit tests for <see cref="RedisCacheStore{TKey, TValue}"/> using Redis as the backend.
/// Validates persistence, TTL handling, and key-based cache operations.
/// </summary>
public class RedisCacheStoreTests
{
    /// <summary>
    /// Instantiates a new Redis-backed cache store using a local Redis instance.
    /// </summary>
    private static RedisCacheStore<string, SomeTestDto> CreateStore()
    {
        var connection = ConnectionMultiplexer.Connect("localhost:6379");
        return new RedisCacheStore<string, SomeTestDto>(connection);
    }

    /// <summary>
    /// Generates a unique cache key for test isolation to prevent key collisions between test runs.
    /// </summary>
    private static string GetKey() => $"test:{Guid.NewGuid()}";

    /// <summary>
    /// Verifies that a value set into the Redis cache can be accurately retrieved by key.
    /// </summary>
    [Fact]
    public async Task SetAndGet_ShouldReturnSameValue()
    {
        var store = CreateStore();
        var key = GetKey();
        var value = new SomeTestDto { Name = "redis", Count = 10 };

        await store.SetAsync(key, value);
        var result = await store.GetAsync(key);

        Assert.NotNull(result);
        Assert.Equal(value.Name, result.Name);
        Assert.Equal(value.Count, result.Count);
    }

    /// <summary>
    /// Confirms that a key-value entry can be removed from Redis and no longer exists afterward.
    /// </summary>
    [Fact]
    public async Task Remove_ShouldDeleteValue()
    {
        var store = CreateStore();
        var key = GetKey();
        await store.SetAsync(key, new SomeTestDto());

        var removed = await store.RemoveAsync(key);
        var result = await store.GetAsync(key);

        Assert.True(removed);
        Assert.Null(result);
    }

    /// <summary>
    /// Validates that <c>ExistsAsync</c> accurately reflects the presence of a key in the cache.
    /// </summary>
    [Fact]
    public async Task Exists_ShouldReturnTrueIfKeyExists()
    {
        var store = CreateStore();
        var key = GetKey();
        await store.SetAsync(key, new SomeTestDto());

        var exists = await store.ExistsAsync(key);

        Assert.True(exists);
    }

    /// <summary>
    /// Ensures that keys with a short TTL expire as expected and are no longer retrievable.
    /// </summary>
    [Fact]
    public async Task ExpiredKey_ShouldNotBeAvailable()
    {
        var store = CreateStore();
        var key = GetKey();
        await store.SetAsync(key, new SomeTestDto(), TimeSpan.FromMilliseconds(300));

        await Task.Delay(500); // Wait for TTL to elapse
        var result = await store.GetAsync(key);

        Assert.Null(result);
    }
}
