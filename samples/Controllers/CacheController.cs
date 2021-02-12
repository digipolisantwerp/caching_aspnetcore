using System.Threading.Tasks;
using Digipolis.Caching.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Example.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService _service;
        public CacheController(ICacheService service)
        {
            _service = service;
        }
        
        [HttpPost("[action]/{cacheTime}")]
        public async Task<bool> PostToCache(int cacheTime, [FromBody] CacheData data)
        {
            await _service.SaveToCacheAsync(data.Key, data, cacheTime, cacheTime);
            return true;
        }
        
        [HttpGet("[action]/{key}")]
        public async Task<string> GetFromCache(string key)
        {
            var result = await _service.GetFromCacheAsync<CacheData>(key);
            return result.succeeded ? result.value.Data : "No entry in cache";
        }
        
        [HttpGet("[action]/{cacheTime}")]
        public async Task<CacheData[]> GetFromCacheOrFunction(int cacheTime)
        {
            // this will get data from function and persist the result in the cache
            var result = await _service.GetFromCacheOrFuncAsync(async () =>
            {
                return new [] {new CacheData
                    {
                        Key = "1",
                        Data = "Function result 1"
                    }, new CacheData
                        {
                        Key = "2",
                        Data = "Function result 2"
                    }
                    };
            }, "Function", cacheTime, cacheTime);

            return result;
        }
        
        [HttpDelete("[action]/{key}")]
        public async Task DeleteFromCache(string key)
        {
            await _service.RemoveFromCacheAsync(key);
        }
        
    }

    public class CacheData
    {
        public string Key { get; set; }
        public string Data { get; set; }
    }
}