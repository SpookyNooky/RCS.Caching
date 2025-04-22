using System;

namespace CachingLibrary.Abstractions.Helpers
{
    public class CacheEntry<T>
    {
        public T Value { get; set; } = default!;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresOn { get; set; }
    }
}
