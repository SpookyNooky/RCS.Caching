using RCS.Caching.Abstractions.Interfaces;
using RCS.Caching.Infrastructure.Memory.Stores;
using RCS.Caching.Infrastructure.Redis.Stores;
using RCS.Caching.Infrastructure.Sql.Stores;

namespace RCS.Caching.WebApi.Resolvers;

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