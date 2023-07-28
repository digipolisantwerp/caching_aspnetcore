using Digipolis.Caching.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Digipolis.Caching.Startup
{
	public static class ConfigureStartupExtensions
	{
		public static void UseDisableCacheRequestHeaderMiddleWare(this IApplicationBuilder app)
		{
			app.UseMiddleware<DisableCacheRequestHeaderMiddleware>();
		}
	}
}
