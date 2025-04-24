using Microsoft.Extensions.DependencyInjection;
using RCS.Caching.Abstractions.Interfaces;
using RCS.Caching.Infrastructure.Redis.Stores;
using StackExchange.Redis;

namespace RCS.Caching.Infrastructure.Redis.Extensions;

/// <summary>
/// Provides an extension method for integrating Redis-backed caching into the .NET dependency injection system.
/// Registers a typed Redis-based cache store as an <see cref="ICacheStore{TKey, TValue}"/> implementation.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Redis cache store and its dependencies into the service collection.
    /// Uses <see cref="StackExchange.Redis.IConnectionMultiplexer"/> for efficient connection management.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type used as the cache key. Must be non-nullable for Redis key serialization safety.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type of the values to be stored in Redis.
    /// </typeparam>
    /// <param name="services">
    /// The service collection used for dependency injection.
    /// </param>
    /// <param name="connectionString">
    /// The connection string used to establish a connection to the Redis server.
    /// </param>
    /// <returns>
    /// The modified <see cref="IServiceCollection"/> for fluent configuration.
    /// </returns>
    /// <remarks>
    /// This method sets up a singleton Redis connection via <see cref="ConnectionMultiplexer"/>,
    /// and registers a strongly-typed Redis cache implementation. This promotes efficient reuse
    /// of the Redis connection and ensures thread-safe access to the distributed cache.
    /// </remarks>
    public static IServiceCollection AddRedisCacheStore<TKey, TValue>(
        this IServiceCollection services,
        string connectionString)
        where TKey : notnull
    {
        // Register a singleton connection to Redis to avoid connection pool bloat
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(connectionString));

        // Register the Redis cache store abstraction
        services.AddSingleton<ICacheStore<TKey, TValue>, RedisCacheStore<TKey, TValue>>();
        return services;
    }
}
