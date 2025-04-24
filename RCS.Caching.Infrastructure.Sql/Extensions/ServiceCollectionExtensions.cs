using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using RCS.Caching.Abstractions.Interfaces;
using RCS.Caching.Infrastructure.Sql.Stores;

namespace RCS.Caching.Infrastructure.Sql.Extensions;

/// <summary>
/// Provides extension methods for configuring SQL Server-backed caching using memory-optimized tables.
/// Registers required services and store abstractions with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a SQL Server-backed cache store for the specified key-value types.
    /// Uses a scoped <see cref="IDbConnection"/> and injects <see cref="SqlCacheStore{TKey, TValue}"/> as the backing implementation.
    /// </summary>
    /// <typeparam name="TKey">
    /// The cache key type. Must be non-nullable to support table indexing and lookup.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type of the cached value.
    /// </typeparam>
    /// <param name="services">
    /// The service collection into which SQL caching services will be registered.
    /// </param>
    /// <param name="connectionString">
    /// A valid SQL Server connection string targeting a database with memory-optimized table support.
    /// </param>
    /// <returns>
    /// The modified <see cref="IServiceCollection"/> with SQL cache dependencies registered.
    /// </returns>
    /// <remarks>
    /// This method ensures the SQL connection is opened immediately upon scope creation to support
    /// memory-optimized table operations that do not tolerate lazy connection handling.
    /// The <see cref="ICacheStore{TKey, TValue}"/> interface can then be used to access a consistent caching abstraction.
    /// </remarks>
    public static IServiceCollection AddSqlCacheStore<TKey, TValue>(
        this IServiceCollection services,
        string connectionString)
        where TKey : notnull
    {
        services.AddScoped<IDbConnection>(_ =>
        {
            var connection = new SqlConnection(connectionString);
            connection.Open(); // Required to avoid deferred connection issues with memory-optimized tables
            return connection;
        });

        services.AddScoped<ICacheStore<TKey, TValue>, SqlCacheStore<TKey, TValue>>();
        return services;
    }
}
