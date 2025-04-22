using System;

namespace CachingLibrary.Abstractions.Helpers
{
    public static class TimeHelper
    {
        public static DateTime? CalculateExpiry(TimeSpan? ttl)
            => ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null;

        public static bool IsExpired(DateTime? expiresOn)
            => expiresOn.HasValue && expiresOn.Value <= DateTime.UtcNow;
    }
}
