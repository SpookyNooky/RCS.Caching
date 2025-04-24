using Microsoft.Extensions.Caching.Memory;
using RCS.Caching.Abstractions.Interfaces;

namespace RCS.Caching.Infrastructure.Memory.Stores;

/// <summary>
/// Provides a generic, thread-safe in-memory caching mechanism using <see cref="IMemoryCache"/>.
/// This class implements <see cref="ICacheStore{TKey, TValue}"/> to abstract the underlying cache strategy,
/// allowing for consistent behavior across different storage backends.
/// </summary>
/// <typeparam name="TKey">
/// The type used as a unique identifier for each cached item. Must be non-nullable due to internal dictionary-based storage constraints.
/// </typeparam>
/// <typeparam name="TValue">
/// The type of the value to be cached.
/// </typeparam>
public class MemoryCacheStore<TKey, TValue>(IMemoryCache memoryCache) : ICacheStore<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Asynchronously sets a value in the in-memory cache with an optional time-to-live (TTL).
    /// </summary>
    /// <param name="key">The unique key identifying the cache entry.</param>
    /// <param name="value">The value to be stored in the cache.</param>
    /// <param name="ttl">Optional absolute expiration time span for the cache entry.</param>
    /// <returns>A completed <see cref="Task"/> representing the operation.</returns>
    public Task SetAsync(TKey key, TValue value, TimeSpan? ttl = null)
    {
        var options = ttl.HasValue
            ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }
            : null;

        memoryCache.Set(key, value, options);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously retrieves a value from the cache using the specified key.
    /// </summary>
    /// <param name="key">The key of the cache entry to retrieve.</param>
    /// <returns>
    /// A task containing the value associated with the key, or <c>null</c> if not found.
    /// </returns>
    public Task<TValue?> GetAsync(TKey key)
    {
        memoryCache.TryGetValue(key, out TValue? value);
        return Task.FromResult(value);
    }

    /// <summary>
    /// Asynchronously removes a cache entry by key.
    /// </summary>
    /// <param name="key">The key of the cache entry to remove.</param>
    /// <returns>
    /// A task that returns <c>true</c> when the removal request completes.
    /// </returns>
    public Task<bool> RemoveAsync(TKey key)
    {
        memoryCache.Remove(key);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Asynchronously checks whether a cache entry exists for the specified key.
    /// </summary>
    /// <param name="key">The key of the cache entry to check.</param>
    /// <returns>
    /// A task returning <c>true</c> if the cache entry exists; otherwise <c>false</c>.
    /// </returns>
    public Task<bool> ExistsAsync(TKey key)
    {
        return Task.FromResult(memoryCache.TryGetValue(key, out _));
    }
}
