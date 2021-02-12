using System;
using System.Threading.Tasks;

namespace Digipolis.Caching.Services
{
    public interface ICacheService
    {
        Task<T> GetFromCacheOrFuncAsync<T>(Func<Task<T>> cacheFunc, string cacheEntryName, int? localCacheMinutes = null, int? distributedCacheMinutes = null);

        Task<(bool succeeded, T value)> GetFromCacheAsync<T>(string key, int? minutesToCacheLocally = null);

        Task SaveToCacheAsync<T>(string key, T value, int? minutesToCacheLocally = null, int? minutesToCacheDistributed = null);

        Task RemoveFromCacheAsync(params string[] keys);
    }
}
