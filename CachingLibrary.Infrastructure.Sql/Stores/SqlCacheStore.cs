using CachingLibrary.Abstractions.Interfaces;
using Dapper;
using System.Data;
using System.Text.Json;

namespace CachingLibrary.Infrastructure.Sql.Stores;

/// <summary>
/// Provides a SQL Server-based implementation of <see cref="ICacheStore{TKey, TValue}"/> using memory-optimized tables,
/// Dapper for lightweight data access, and JSON serialization for flexible value persistence.
/// </summary>
/// <typeparam name="TKey">
/// The type used as the cache key. Must be non-nullable and capable of meaningful string representation for SQL indexing.
/// </typeparam>
/// <typeparam name="TValue">
/// The type of the value to cache. Objects are serialized as JSON before being persisted to SQL.
/// </typeparam>
public class SqlCacheStore<TKey, TValue>(IDbConnection connection) : ICacheStore<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Inserts or updates a cache entry for the specified key. Values are serialized as JSON and written to a SQL memory-optimized table.
    /// If a time-to-live (TTL) is provided, an expiration timestamp is also stored.
    /// </summary>
    /// <param name="key">The cache key, uniquely identifying the entry.</param>
    /// <param name="value">The value to be cached, which will be serialized.</param>
    /// <param name="ttl">Optional expiration interval.</param>
    public async Task SetAsync(TKey key, TValue value, TimeSpan? ttl = null)
    {
        var now = DateTime.UtcNow;
        var expiresOn = ttl.HasValue ? now.Add(ttl.Value) : (DateTime?)null;

        const string sql = """
            UPDATE dbo.CacheEntries
            SET [Value] = @Value, ExpiresOn = @ExpiresOn
            WHERE [Key] = @Key;

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO dbo.CacheEntries ([Key], [Value], ExpiresOn)
                VALUES (@Key, @Value, @ExpiresOn);
            END
        """;

        await connection.ExecuteAsync(sql, new
        {
            Key = key.ToString(),
            Value = JsonSerializer.Serialize(value),
            ExpiresOn = expiresOn
        });
    }

    /// <summary>
    /// Retrieves a cache entry by key if it exists and is not expired. Uses snapshot isolation by default.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The deserialized value if found; otherwise <c>default</c>.</returns>
    public Task<TValue?> GetAsync(TKey key) => GetAsync(key, useSnapshotHint: true);

    /// <summary>
    /// Removes a cache entry by key. Uses snapshot isolation by default.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns><c>true</c> if an entry was removed; otherwise <c>false</c>.</returns>
    public Task<bool> RemoveAsync(TKey key) => RemoveAsync(key, useSnapshotHint: true);

    /// <summary>
    /// Checks whether a cache entry exists and is not expired. Uses snapshot isolation by default.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns><c>true</c> if the key exists and has not expired; otherwise <c>false</c>.</returns>
    public Task<bool> ExistsAsync(TKey key) => ExistsAsync(key, useSnapshotHint: true);

    /// <summary>
    /// Retrieves a cache entry using the specified key with optional snapshot isolation.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="useSnapshotHint">Whether to use <c>WITH (SNAPSHOT)</c> SQL hint for concurrency consistency.</param>
    /// <returns>The deserialized cached value, or <c>default</c> if not found or expired.</returns>
    public async Task<TValue?> GetAsync(TKey key, bool useSnapshotHint)
    {
        var hint = useSnapshotHint ? " WITH (SNAPSHOT)" : string.Empty;

        var sql = $"""
            SELECT [Value]
            FROM dbo.CacheEntries{hint}
            WHERE [Key] = @Key AND (ExpiresOn IS NULL OR ExpiresOn > GETUTCDATE());
        """;

        var result = await connection.QuerySingleOrDefaultAsync<string>(sql, new { Key = key.ToString() });
        return result != null ? JsonSerializer.Deserialize<TValue>(result) : default;
    }

    /// <summary>
    /// Deletes a cache entry using the specified key, with optional snapshot isolation hint.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="useSnapshotHint">Whether to apply snapshot isolation for consistency.</param>
    /// <returns><c>true</c> if an entry was deleted; otherwise <c>false</c>.</returns>
    public async Task<bool> RemoveAsync(TKey key, bool useSnapshotHint)
    {
        var hint = useSnapshotHint ? " WITH (SNAPSHOT)" : string.Empty;

        var sql = $"""
            DELETE FROM dbo.CacheEntries{hint}
            WHERE [Key] = @Key;
        """;

        var affected = await connection.ExecuteAsync(sql, new { Key = key.ToString() });
        return affected > 0;
    }

    /// <summary>
    /// Determines whether a non-expired cache entry exists for the given key, using optional snapshot isolation.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="useSnapshotHint">Whether to apply snapshot isolation during the lookup.</param>
    /// <returns><c>true</c> if a valid entry exists; otherwise <c>false</c>.</returns>
    public async Task<bool> ExistsAsync(TKey key, bool useSnapshotHint)
    {
        var hint = useSnapshotHint ? " WITH (SNAPSHOT)" : string.Empty;

        var sql = $"""
            SELECT 1
            FROM dbo.CacheEntries{hint}
            WHERE [Key] = @Key AND (ExpiresOn IS NULL OR ExpiresOn > GETUTCDATE());
        """;

        var exists = await connection.QueryFirstOrDefaultAsync<int?>(sql, new { Key = key.ToString() });
        return exists.HasValue;
    }
}
