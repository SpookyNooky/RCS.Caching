using RCS.Caching.Abstractions.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace RCS.Caching.Infrastructure.Redis.Stores;

/// <summary>
/// Redis-backed implementation of the <see cref="ICacheStore{TKey, TValue}"/> interface.
/// Leverages <see cref="StackExchange.Redis"/> for distributed, persistent, and scalable caching.
/// </summary>
/// <typeparam name="TKey">
/// Type used for cache keys. Must be non-nullable and support meaningful <c>ToString()</c> conversion for Redis keying.
/// </typeparam>
/// <typeparam name="TValue">
/// Type of the objects to be stored and retrieved from Redis.
/// </typeparam>
public class RedisCacheStore<TKey, TValue>(IConnectionMultiplexer redis) : ICacheStore<TKey, TValue>
    where TKey : notnull
{
    // Reference to the Redis logical database (default: DB 0)
    private readonly IDatabase _db = redis.GetDatabase();

    /// <summary>
    /// Serializes and stores the given value in Redis under the specified key, with optional expiration.
    /// </summary>
    /// <param name="key">Cache key used for lookup and deletion.</param>
    /// <param name="value">The object to cache, which will be JSON serialized.</param>
    /// <param name="ttl">Optional time-to-live (absolute expiration).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetAsync(TKey key, TValue value, TimeSpan? ttl = null)
    {
        var serialized = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key.ToString(), serialized, ttl);
    }

    /// <summary>
    /// Retrieves and deserializes a cached value from Redis using the given key.
    /// </summary>
    /// <param name="key">The cache key to look up.</param>
    /// <returns>
    /// A task returning the deserialized object if present, or <c>default</c> if not found.
    /// </returns>
    public async Task<TValue?> GetAsync(TKey key)
    {
        var result = await _db.StringGetAsync(key.ToString());
        return result.HasValue ? JsonSerializer.Deserialize<TValue>(result!) : default;
    }

    /// <summary>
    /// Removes the cached entry associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the entry to remove.</param>
    /// <returns>
    /// <c>true</c> if the key existed and was deleted; otherwise <c>false</c>.
    /// </returns>
    public async Task<bool> RemoveAsync(TKey key)
    {
        return await _db.KeyDeleteAsync(key.ToString());
    }

    /// <summary>
    /// Checks whether a cache entry exists for the specified key.
    /// </summary>
    /// <param name="key">The key to check for existence.</param>
    /// <returns>
    /// <c>true</c> if the key exists in Redis; otherwise <c>false</c>.
    /// </returns>
    public async Task<bool> ExistsAsync(TKey key)
    {
        return await _db.KeyExistsAsync(key.ToString());
    }
}
