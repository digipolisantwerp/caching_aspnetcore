using Digipolis.Caching.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Digipolis.Caching.Handlers
{
    internal class DistributedCacheHandler : ICacheHandler
    {
        public DistributedCacheHandler(
            IDistributedCache cache,
            ILogger<DistributedCacheHandler> logger)
        {
            _cache = cache ?? throw new ArgumentNullException($"{GetType().Name}.Ctr - Argument {nameof(cache)} cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = false, // Disable unnecessary spacing
                ReferenceHandler = ReferenceHandler.Preserve
            };
        }

        private readonly IDistributedCache _cache;
        private readonly ILogger<DistributedCacheHandler> _logger;
        private readonly JsonSerializerOptions _serializerOptions;

        public async Task<(bool succeeded, CacheObject<T> value)> GetFromCacheAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var result = await _cache.GetStringAsync(key, cancellationToken);

            if (result != null)
            {
                try
                {
                    var deserialized = JsonSerializer.Deserialize<CacheObject<T>>(result, _serializerOptions);
                    return (succeeded: true, value: deserialized);
                }
                catch (Exception)
                {
                    // Log here, probably mismatch in type failure while deserializing
                    _logger.LogError($"{GetType().Name}.GetFromCacheAsync - deserializing cache entry '{key}' to type 'CacheObject<{typeof(T).Name}>' failed");
                }
            }
            return (succeeded: false, value: default(CacheObject<T>));
        }

        public Task SaveToCacheAsync<T>(string key, CacheObject<T> value, int minutesToCache, CancellationToken cancellationToken = default)
        {
            var relativeTime = new TimeSpan(hours: 0, minutes: minutesToCache, seconds: 0);

            // Choosing JSON over Binary will result in slower (de)serialization but the resulting file will be much smaller and transfer faster over network
            // This means better scaling, because more pods is more seialization power, without saturating network
            // https://dejanstojanovic.net/aspnet/2018/june/how-to-boost-application-performance-by-choosing-the-right-serialization/
            // Performance could be better with UTF8Json but found too many bugs to take the risk at this time
            var json = JsonSerializer.Serialize(value, _serializerOptions);

            return _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = relativeTime }, cancellationToken);
        }

        public async Task RemoveFromCacheAsync(params string[] keys)
        {
            if (keys == null || keys.Length < 1) return;

            var numberOfKeys = keys.Length;
            var tasks = new List<Task>();

            for (int i = 0; i < numberOfKeys; i++)
            {
                tasks.Add(_cache.RemoveAsync(keys[i]));
            }

            await Task.WhenAll(tasks);
        }
    }
}
