using CachingLibrary.Abstractions.Interfaces;
using CachingLibrary.Infrastructure.Memory.Stores;
using CachingLibrary.Infrastructure.Redis.Stores;
using CachingLibrary.Infrastructure.Sql.Stores;

namespace CachingLibrary.WebApi.Resolvers;

public class CacheStoreResolver<TKey, TValue>(IServiceProvider provider)
    where TKey : notnull
{
    public ICacheStore<TKey, TValue> Resolve(string store)
    {
        return store.ToLowerInvariant() switch
        {
            "memory" => provider.GetRequiredService<MemoryCacheStore<TKey, TValue>>(),
            "sql"    => provider.GetRequiredService<SqlCacheStore<TKey, TValue>>(),
            "redis"  => provider.GetRequiredService<RedisCacheStore<TKey, TValue>>(),
            _ => throw new ArgumentException($"Unknown cache store: {store}")
        };
    }
}