using System;

namespace Digipolis.Caching.Models
{
    internal class CacheObject<T>
    {
        public DateTime CacheUntil { get; set; }
        public T Value { get; set; }
    }
}
