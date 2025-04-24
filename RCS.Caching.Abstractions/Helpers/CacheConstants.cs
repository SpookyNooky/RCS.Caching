using System;

namespace RCS.Caching.Abstractions.Helpers
{
    public static class CacheConstants
    {
        public static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(10);
        public const int DefaultRedisBucketCount = 10000;
    }
}
