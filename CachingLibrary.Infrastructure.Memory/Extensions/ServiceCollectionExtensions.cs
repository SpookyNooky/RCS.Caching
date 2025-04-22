using CachingLibrary.Abstractions.Interfaces;
using CachingLibrary.Infrastructure.Memory.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace CachingLibrary.Infrastructure.Memory.Extensions;

/// <summary>
/// Provides extension methods for integrating in-memory caching into the .NET dependency injection system.
/// Designed to register a typed, generic memory-based cache store with scoped or singleton lifetimes.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers an in-memory cache implementation for the specified key-value types.
    /// This includes the underlying <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/> infrastructure
    /// as well as a strongly-typed cache store implementing <see cref="ICacheStore{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type used as the cache key. Must be non-nullable to satisfy dictionary-based memory cache constraints.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type of the values being cached.
    /// </typeparam>
    /// <param name="services">
    /// The dependency injection service collection to which the in-memory cache dependencies will be added.
    /// </param>
    /// <returns>
    /// The original <see cref="IServiceCollection"/> instance with memory cache services registered.
    /// </returns>
    /// <remarks>
    /// This extension method ensures both the default Microsoft in-memory cache and a custom abstraction
    /// are wired into the service container. Consumers of <see cref="ICacheStore{TKey, TValue}"/> can
    /// transparently use the memory cache with no knowledge of the underlying implementation.
    /// </remarks>
    public static IServiceCollection AddInMemoryCache<TKey, TValue>(this IServiceCollection services)
        where TKey : notnull
    {
        services.AddMemoryCache(); // Registers IMemoryCache as a singleton
        services.AddSingleton<ICacheStore<TKey, TValue>, MemoryCacheStore<TKey, TValue>>();
        return services;
    }
}
