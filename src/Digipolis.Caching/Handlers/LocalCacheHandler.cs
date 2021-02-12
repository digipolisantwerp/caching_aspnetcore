using Digipolis.Caching.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Digipolis.Caching.Handlers
{
    internal class LocalCacheHandler : ICacheHandler
    {
        public LocalCacheHandler(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException($"{GetType().Name}.Ctr - Argument {nameof(cache)} cannot be null.");
        }

        private readonly IMemoryCache _cache;

        public Task<(bool succeeded, CacheObject<T> value)> GetFromCacheAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var succeeded = _cache.TryGetValue(key, out CacheObject<T> result);

            if (succeeded)
                return Task.FromResult((succeeded, value: succeeded ? result : default));
            else
                return Task.FromResult((succeeded, value: default(CacheObject<T>)));
        }

        public Task SaveToCacheAsync<T>(string key, CacheObject<T> value, int minutesToCache, CancellationToken cancellationToken = default)
        {
            var relativeTime = new TimeSpan(hours: 0, minutes: minutesToCache, seconds: 0);
            _cache.Set(key, value, relativeTime);
            return Task.CompletedTask;
        }

        public Task RemoveFromCacheAsync(params string[] keys)
        {
            if (keys == null || keys.Length < 1) return Task.CompletedTask;

            var numberOfKeys = keys.Length;
            for (int i = 0; i < numberOfKeys; i++)
            {
                _cache.Remove(keys[i]);
            }

            return Task.CompletedTask;
        }
    }
}
