using Digipolis.Caching.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Digipolis.Caching.Handlers
{
    internal interface ICacheHandler
    {
        Task<(bool succeeded, CacheObject<T> value)> GetFromCacheAsync<T>(string key, CancellationToken cancellationToken = default);

        Task SaveToCacheAsync<T>(string key, CacheObject<T> value, int minutesToCache, CancellationToken cancellationToken = default);

        Task RemoveFromCacheAsync(params string[] keys);
    }
}
