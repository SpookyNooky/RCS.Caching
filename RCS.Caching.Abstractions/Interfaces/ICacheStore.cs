using System;
using System.Threading.Tasks;

namespace RCS.Caching.Abstractions.Interfaces;

/// <summary>
/// Generic key-value cache interface.
/// </summary>
/// <typeparam name="TKey">Type of the key.</typeparam>
/// <typeparam name="TValue">Type of the value.</typeparam>
public interface ICacheStore<in TKey, TValue>
{
    Task SetAsync(TKey key, TValue value, TimeSpan? ttl = null);
    Task<TValue?> GetAsync(TKey key);
    Task<bool> RemoveAsync(TKey key);
    Task<bool> ExistsAsync(TKey key);
}