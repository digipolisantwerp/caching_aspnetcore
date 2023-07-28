using Digipolis.Caching.Handlers;
using Digipolis.Caching.Options;
using Digipolis.Caching.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Caching.Distributed;
using Polly.Registry;

namespace Digipolis.Caching.Startup
{
    public static class DependencyRegistration
    {
        public static void AddCache(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment,
            IConfigurationSection customConfigurationSection = null)
        {
            IConfigurationSection configurationSection;
            if (customConfigurationSection != null)
                configurationSection = customConfigurationSection;
            else
                configurationSection = configuration.GetSection(Constants.Config.ConfigurationSection.DataAccess).GetSection(Constants.Config.ConfigurationSection.Cache);

            CacheSettings.RegisterConfiguration(services, configurationSection, environment);

            CacheSettings cacheSettings;
            using (var provider = services.BuildServiceProvider())
            {
                cacheSettings = provider.GetService<IOptions<CacheSettings>>().Value;
            }

            if (cacheSettings == null || !cacheSettings.CacheEnabled) return;

            AddMultiTierCaches(services, cacheSettings, environment);

            RegisterDependencies(services, cacheSettings, environment);
        }


        private static void AddMultiTierCaches(
            this IServiceCollection services,
            CacheSettings cacheSettings,
            IHostEnvironment environment)
        {
            services.AddMemoryCache();

            if (!environment.IsDevelopment() && cacheSettings.Tier2Enabled)
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = cacheSettings.Configuration;
                });
            }
        }

        private static void RegisterDependencies(
            this IServiceCollection services,
            CacheSettings cacheSettings,
            IHostEnvironment environment)
        {
            AddHandlers(services, cacheSettings, environment);
            AddServices(services);
            AddPolicies(services);
        }

        private static void AddHandlers(
            this IServiceCollection services,
            CacheSettings cacheSettings,
            IHostEnvironment environment)
        {
            if (!environment.IsDevelopment() && cacheSettings.Tier2Enabled)
            {
                services.AddSingleton<DistributedCacheHandler>();
            }
            else if (environment.IsDevelopment() && cacheSettings.Tier2Enabled)
            {
                services.AddDistributedMemoryCache();
                services.AddSingleton<DistributedCacheHandler>();
            }

            services.AddSingleton<LocalCacheHandler>();
        }

        private static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<ICacheService, CacheService>();
            services.AddScoped<CacheControlOptions>();
        }

        private static void AddPolicies(this IServiceCollection services)
        {
            services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IDistributedCache>().AsAsyncCacheProvider<string>());
            services.AddSingleton<IReadOnlyPolicyRegistry<string>, PolicyRegistry>((serviceProvider) =>
            {
                PolicyRegistry registry = new PolicyRegistry();
                registry.Add("myCachePolicy", Policy.TimeoutAsync<string>(20));
                return registry;
            });
        }
    }
}
