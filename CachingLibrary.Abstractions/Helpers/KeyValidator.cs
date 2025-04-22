using System;

namespace CachingLibrary.Abstractions.Helpers
{
    public static class KeyValidator
    {
        public static void Validate(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be null or empty.");

            if (key.Length > 250)
                throw new ArgumentException("Cache key exceeds maximum length (250 chars).");

            // Add more rules if needed
        }
    }
}
