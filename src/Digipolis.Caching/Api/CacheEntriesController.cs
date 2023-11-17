using Digipolis.Caching.Models;
using Digipolis.Caching.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Digipolis.Caching.Api
{
    //Ideally this shouldn't be abstract, however integrating our API versioning 
    //requires a dependency to another NuGet package, which we want to avoid
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class CacheEntriesController : Controller
    {
        private readonly ICacheService _service;
        public CacheEntriesController(ICacheService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Remove cache entries by key. When running multi-instance, memory cache will stay alive on other instances.
        /// </summary>
        /// <param name="cacheEntryWrapper">Contains a list of keys to remove from the cache</param>
        /// <returns></returns>
        [HttpPost("remove")]
        [Consumes("application/json")]
        [ProducesResponseType(204)]
        [AllowAnonymous]
        public virtual async Task<IActionResult> RemoveCacheEntries([FromBody] CacheEntryWrapper cacheEntryWrapper)
        {
            if (cacheEntryWrapper?.Keys?.Any() ?? false)
                await _service.RemoveFromCacheAsync(cacheEntryWrapper.Keys.ToArray());
            return NoContent();
        }
    }
}
