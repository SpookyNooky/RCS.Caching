using System.Linq;

namespace CachingLibrary.Abstractions.Helpers;

/// <summary>
/// Provides a consistent way to construct cache keys by joining segments with a colon separator.
/// Ensures normalization for cross-platform cache usage across Redis, SQL, or in-memory stores.
/// </summary>
public static class KeyFormatter
{
    /// <summary>
    /// Constructs a cache key from one or more segments using ":" as a delimiter.
    /// All segments are trimmed and converted to lower-case.
    /// </summary>
    /// <param name="segments">The individual key segments to combine.</param>
    /// <returns>A normalized, colon-separated cache key string.</returns>
    /// <example>
    /// <code>
    /// var key1 = KeyFormatter.Format("auth", "token", "user", 12345);
    /// // Result: "auth:token:user:12345"
    ///
    /// var key2 = KeyFormatter.Format("Shop", "Catalog", "Product", "456", "Price", "USD");
    /// // Result: "shop:catalog:product:456:price:usd"
    ///
    /// var key3 = KeyFormatter.Format("prod", "tenantA", "invoice", 2024, "inv-001");
    /// // Result: "prod:tenanta:invoice:2024:inv-001"
    /// </code>
    /// </example>
    public static string Format(params object[] segments)
    {
        return string.Join(":", segments
            .Select(s => s?.ToString()?.ToLowerInvariant().Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}