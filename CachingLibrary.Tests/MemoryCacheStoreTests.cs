using CachingLibrary.Infrastructure.Memory.Stores;
using CachingLibrary.Tests.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CachingLibrary.Tests;

/// <summary>
/// Unit tests for <see cref="MemoryCacheStore{TKey, TValue}"/> using string keys and <see cref="SomeTestDto"/> values.
/// Validates correctness of caching behavior, expiration logic, and contract conformance.
/// </summary>
public class MemoryCacheStoreTests
{
    /// <summary>
    /// Helper method to create a fresh instance of the memory cache store for test isolation.
    /// </summary>
    private static MemoryCacheStore<string, SomeTestDto> CreateStore()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        return new MemoryCacheStore<string, SomeTestDto>(memoryCache);
    }

    /// <summary>
    /// Generates a unique test key to prevent cross-test collisions in the shared memory cache.
    /// </summary>
    private static string GetKey() => $"test:{Guid.NewGuid()}";

    /// <summary>
    /// Verifies that values set and retrieved via the cache maintain consistency and object fidelity.
    /// </summary>
    [Fact]
    public async Task SetAndGet_ShouldReturnSameValue()
    {
        var store = CreateStore();
        var key = GetKey();
        var dto = new SomeTestDto { Name = "sample", Count = 10 };

        await store.SetAsync(key, dto);
        var result = await store.GetAsync(key);

        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Count, result.Count);
    }

    /// <summary>
    /// Ensures that the <c>ExistsAsync</c> method accurately detects the presence of a cache entry.
    /// </summary>
    [Fact]
    public async Task Exists_ShouldReturnTrueIfPresent()
    {
        var store = CreateStore();
        var key = GetKey();
        await store.SetAsync(key, new SomeTestDto());

        var exists = await store.ExistsAsync(key);

        Assert.True(exists);
    }

    /// <summary>
    /// Confirms that a cache entry is fully removed when <c>RemoveAsync</c> is invoked.
    /// </summary>
    [Fact]
    public async Task Remove_ShouldDeleteEntry()
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
    /// Validates that an item with a TTL expires as expected and becomes unavailable after expiration.
    /// </summary>
    [Fact]
    public async Task ExpiredItem_ShouldNotBeAvailable()
    {
        var store = CreateStore();
        var key = GetKey();
        await store.SetAsync(key, new SomeTestDto(), TimeSpan.FromMilliseconds(300));

        await Task.Delay(500); // Wait past the TTL
        var result = await store.GetAsync(key);

        Assert.Null(result);
    }
}
