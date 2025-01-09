using Digipolis.Caching.Handlers;
using Digipolis.Caching.Models;
using Digipolis.Caching.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Digipolis.Caching.Services
{
    // #CACHING: Global cache setup with the possibility of multiple cache layers
    // At the moment, tier 1 is always local (short-lived) cache, only available to the specific instance of the application
    // Tier 2 is a distributed (long-lived) cache backed by Redis.
    internal class CacheService : ICacheService
    {
        private readonly CacheSettings _settings;
        private readonly IReadOnlyCollection<CacheHandlerWithOptions> _cacheHandlers;
        private readonly ILogger<CacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private CacheControlOptions _cacheControlOptions;

        public CacheService(
            IServiceProvider serviceProvider,
            IOptions<CacheSettings> options,
            ILogger<CacheService> logger)
        {
            _settings = options?.Value ?? throw new ArgumentNullException($"{GetType().Name}.Ctr - Argument {nameof(options)} cannot be null.");
            

            if (!IsCacheEnabled())
            {
                _cacheHandlers = new List<CacheHandlerWithOptions>(0);
                return;
            }

            //Important: add cache handlers in order of cache lifetime (from min to max)
            var cacheHandlers = new List<CacheHandlerWithOptions>();
            //Add to cache tier 1 (local cache)
            RetrieveCacheImplementationAndAddToList<LocalCacheHandler>(serviceProvider, cacheHandlers, _settings.DefaultMinutesToCacheTier1);

            if (IsTier2Enabled())
            {
                var timeoutInSeconds = _settings.TimeoutAsyncAfterSeconds > 0 ? _settings.TimeoutAsyncAfterSeconds : 5;
                //Add to cache tier 2 (distributed cache)
                RetrieveCacheImplementationAndAddToList<DistributedCacheHandler>(serviceProvider, cacheHandlers, _settings.DefaultMinutesToCacheTier2, timeoutInSeconds);
            }

            _cacheHandlers = cacheHandlers;
            _serviceProvider = serviceProvider;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> GetFromCacheOrFuncAsync<T>(
            Func<Task<T>> cacheFunc,
            string cacheEntryName,
            int? localCacheMinutes = null,
            int? distributedCacheMinutes = null)
        {
            if (string.IsNullOrWhiteSpace(cacheEntryName))
                throw new ArgumentException($"{GetType().Name}.Cache parameter {nameof(cacheEntryName)} cannot be null or whitespace.");

            var (succeeded, cacheEntry) = await GetFromCacheAsync<T>(cacheEntryName, localCacheMinutes).ConfigureAwait(false);
            if (succeeded)
                return cacheEntry;

            _logger.LogInformation($"{GetType().Name}.Cache({cacheEntryName}) - no cache entry found. Invoking cache func");

            try
            {
                cacheEntry = await cacheFunc.Invoke().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Cache func invocation failed", ex);
            }

            if (cacheEntry != null)
                await SaveToCacheAsync(cacheEntryName, cacheEntry, localCacheMinutes, distributedCacheMinutes).ConfigureAwait(false);
            else
                _logger.LogDebug($"{GetType().Name}.Cache({cacheEntryName}) - no cache entry set. Cache func resulted in null or default value");

            return cacheEntry;
        }

        /// <summary>
        /// Try to retrieve the value from one or multiple caches
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="minutesToCacheLocally"></param>
        /// <returns></returns>
        public async Task<(bool succeeded, T value)> GetFromCacheAsync<T>(string key, int? minutesToCacheLocally = null)
        {
            CacheObject<T> result = default;
            if (!IsCacheEnabled() || _cacheHandlers == null || _cacheHandlers.Count < 1 || IsCacheScopeDisabled()) return (succeeded: false, value: default);

            var itemFoundInCacheTierNumber = -1;

            for (int i = 0; i < _cacheHandlers.Count; i++)
            {
                var cacheHandlerWrapper = _cacheHandlers.ElementAt(i);

                // Skip if cache handler is missing
                if (cacheHandlerWrapper.CacheHandler == null) continue;

                // Cache retrieval may never crash the application
                try
                {
	                using var cancellationTokenSource = new CancellationTokenSource(cacheHandlerWrapper.MilliSecondsBeforeTimeout);
	                var (succeeded, cacheResult) = await cacheHandlerWrapper.CacheHandler.GetFromCacheAsync<T>(key, cancellationTokenSource.Token);
	                if (succeeded)
	                {
		                itemFoundInCacheTierNumber = i;
		                result = cacheResult;
		                break;
	                }
                }
                catch (OperationCanceledException)
                {
                    // Timeout occurred and task was cancelled
                    // Log error for statistics and early problem detection
                    _logger.LogError($"{GetType().Name}.GetFromCacheAsync - operation cancelled while retrieving cache entry '{key}'");
                }
                catch (Exception ex)
                {
                    // Log the cache retrieval failure due to errors
                    _logger.LogError($"{GetType().Name}.GetFromCacheAsync - cache retrieval failed for entry '{key}'. Exception message: {ex.Message}");
                }
            }

            // If cached item not found, stop here
            if (itemFoundInCacheTierNumber == -1)
                return (succeeded: false, value: default(T));

            // If item found in longer living cache, add it to shorter living cache
            if (itemFoundInCacheTierNumber > 0 && result?.CacheUntil > DateTime.UtcNow)
            {
                var cacheEntryLifeTimeRemaining = Convert.ToInt32(Math.Floor((result.CacheUntil - DateTime.UtcNow).TotalMinutes));
                if (cacheEntryLifeTimeRemaining > 0)
                {
                    for (int i = 0; i < itemFoundInCacheTierNumber; i++)
                    {
                        var cacheHandlerWrapper = _cacheHandlers.ElementAt(i);
                        //check if cache object lifetime is smaller than minutes to cache locally, if so => only cache object lifetime
                        var minutesToCache = minutesToCacheLocally < cacheEntryLifeTimeRemaining ? minutesToCacheLocally : cacheEntryLifeTimeRemaining;
                        await SaveToSpecificCacheAsync(key, result.Value, cacheHandlerWrapper, minutesToCache);
                    }
                }
            }

            return (succeeded: true, value: result!.Value);
        }

        /// <summary>
        /// Save value to one or multiple caches
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="minutesToCacheLocally"></param>
        /// <param name="minutesToCacheDistributed"></param>
        /// <returns></returns>
        public async Task SaveToCacheAsync<T>(string key, T value, int? minutesToCacheLocally = null, int? minutesToCacheDistributed = null)
        {
            if (!IsCacheEnabled() || _cacheHandlers == null || _cacheHandlers.Count < 1 || IsCacheScopeDisabled()) return;

            foreach (var cacheHandlerWrapper in _cacheHandlers)
            {
                try
                {
                    if (cacheHandlerWrapper.CacheHandler is LocalCacheHandler)
                    {
                        await SaveToSpecificCacheAsync(key, value, cacheHandlerWrapper, minutesToCacheLocally);
                    }
                    else if (cacheHandlerWrapper.CacheHandler is DistributedCacheHandler)
                    {
                        await SaveToSpecificCacheAsync(key, value, cacheHandlerWrapper, minutesToCacheDistributed);
                    }
                    else
                    {
                        //unknown cache handler => using default cache duration
                        await SaveToSpecificCacheAsync(key, value, cacheHandlerWrapper);
                    }
                }
                catch (Exception)
                {
                    // Catch underlying error and act if necessary

                }
            }
        }

        /// <summary>
        /// Remove cached items based on given keys
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public async Task RemoveFromCacheAsync(params string[] keys)
        {
            if (!IsCacheEnabled() || _cacheHandlers == null || _cacheHandlers.Count < 1 || IsCacheScopeDisabled()) return;

            //reverse: remove longest living cache first to prevent race conditions (possibly refreshing shorter living cache entries while delete is in progress)
            var reversed = _cacheHandlers.Reverse();
            foreach (var cacheHandlerWrapper in reversed)
            {
                await cacheHandlerWrapper.CacheHandler?.RemoveFromCacheAsync(keys)!;
            }
        }

        private static async Task SaveToSpecificCacheAsync<T>(string key, T value, CacheHandlerWithOptions cacheHandlerWrapper, int? minutesToCache = null)
        {
            if (!minutesToCache.HasValue)
            {
                minutesToCache = cacheHandlerWrapper.MinutesToCache;
            }

            if (minutesToCache.Value <= 0)
            {
                //invalid cache duration
                return;
            }

            try
            {
                var cacheObject = new CacheObject<T>
                {
                    CacheUntil = DateTime.UtcNow.AddMinutes(minutesToCache.Value),
                    Value = value
                };

                using var cancellationTokenSource = new CancellationTokenSource(cacheHandlerWrapper.MilliSecondsBeforeTimeout);
                // Write to cache even if key already exist, 
                // OK to overwrite existing data because data is probably retrieved recently
                await cacheHandlerWrapper.CacheHandler?.SaveToCacheAsync<T>(key, cacheObject, minutesToCache.Value, cancellationTokenSource.Token)!;
            }
            catch (OperationCanceledException)
            {
                // Timeout occurred and task ask cancelled
                // Log error for statistics and early problem detection
            }
            catch (Exception)
            {
                // Log error but continue trying to cache

            }
        }

        private bool IsCacheEnabled()
        {
            if (_settings != null && _settings.CacheEnabled)
                return true;
            return false;
        }

        private bool IsCacheScopeDisabled()
        {
	        _cacheControlOptions =_serviceProvider.GetRequiredService<CacheControlOptions>();
	        return _cacheControlOptions != null && _cacheControlOptions.DisableCacheFromHeader;
        }

        private bool IsTier2Enabled()
        {
            if (_settings?.Tier2Enabled ?? false)
                return true;
            return false;
        }

        private void RetrieveCacheImplementationAndAddToList<T>(IServiceProvider serviceProvider, List<CacheHandlerWithOptions> cacheHandlers, int minutesToCache = 15, int asyncTimoutInSeconds = 5)
            where T : ICacheHandler
        {
            var cacheTier = serviceProvider.GetService(typeof(T))
                ?? throw new ArgumentNullException($"{GetType().Name}.Ctr - Argument {typeof(T).Name} cannot be null.");
            cacheHandlers.Add(new CacheHandlerWithOptions { CacheHandler = cacheTier as ICacheHandler, MinutesToCache = minutesToCache, MilliSecondsBeforeTimeout = asyncTimoutInSeconds * 1000 });
        }
    }
}
