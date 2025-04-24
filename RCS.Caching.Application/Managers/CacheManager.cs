using CachingLibrary.Abstractions.Interfaces;

namespace CachingLibrary.Application.Managers;

/// <summary>
/// High-level generic cache manager.
/// </summary>
public class CacheManager<TKey, TValue>(ICacheStore<TKey, TValue> store)
    where TKey : notnull
{
    public Task SetAsync(TKey key, TValue value, TimeSpan? ttl = null) => store.SetAsync(key, value, ttl);

    public Task<TValue?> GetAsync(TKey key) => store.GetAsync(key);

    public Task<bool> RemoveAsync(TKey key) => store.RemoveAsync(key);

    public Task<bool> ExistsAsync(TKey key) => store.ExistsAsync(key);
}