using System;
using System.Linq;
using System.Threading.Tasks;
using Digipolis.Caching.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Digipolis.Caching.Middleware
{
	internal class DisableCacheRequestHeaderMiddleware
	{
		private readonly RequestDelegate _next;
		private CacheControlOptions _headerOptions;
		private readonly ILogger<DisableCacheRequestHeaderMiddleware> _logger;

		public DisableCacheRequestHeaderMiddleware(RequestDelegate next, ILogger<DisableCacheRequestHeaderMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			const string disableCacheHeaderKey = "x-cache-disable";

			_headerOptions = context.RequestServices.GetRequiredService<CacheControlOptions>();

			context.Request.Headers.TryGetValue(disableCacheHeaderKey, out var disableCacheHeaderValue);

			if (disableCacheHeaderValue != StringValues.Empty && disableCacheHeaderValue.Any())
			{
				try
				{
					_headerOptions.DisableCacheFromHeader = Convert.ToBoolean(disableCacheHeaderValue.FirstOrDefault());
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Could not set disable caching from header. Cache not disabled.");
				}
			}

			await _next(context);
		}

	}
}
