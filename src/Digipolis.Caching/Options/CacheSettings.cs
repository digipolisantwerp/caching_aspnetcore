using Digipolis.Caching.Constants;
using Digipolis.Caching.Constants.Config;
using Digipolis.Caching.Options._Base;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Digipolis.Caching.Options
{
    internal class CacheSettings : SettingsBase
    {
        public string Configuration { get; set; }
        public bool CacheEnabled { get; set; }
        public bool Tier2Enabled { get; set; }
        public int DefaultMinutesToCacheTier1 { get; set; }
        public int DefaultMinutesToCacheTier2 { get; set; }
        public int TimeoutAsyncAfterSeconds { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HAA0302:Display class allocation to capture closure", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HAA0301:Closure Allocation Source", Justification = "<Pending>")]
        public static void RegisterConfiguration(IServiceCollection services, IConfigurationSection section, IHostingEnvironment env)
        {
            services.Configure<CacheSettings>(settings =>
            {
                settings.LoadFromConfigSection(section);
                settings.OverrideFromEnvironmentVariables(env);
            });
        }

        private void LoadFromConfigSection(IConfigurationSection section)
        {
            section.Bind(this);
        }

        private void OverrideFromEnvironmentVariables(IHostingEnvironment env)
        {
            Configuration = GetValue(Configuration, CacheSettingsConfigKey.Configuration, env);
            CacheEnabled = GetValue(CacheEnabled, CacheSettingsConfigKey.CacheEnabled, env);
            DefaultMinutesToCacheTier1 = GetValue(DefaultMinutesToCacheTier1, CacheSettingsConfigKey.DefaultMinutesToCacheTier1, env);
            DefaultMinutesToCacheTier2 = GetValue(DefaultMinutesToCacheTier2, CacheSettingsConfigKey.DefaultMinutesToCacheTier2, env);
            Tier2Enabled = GetValue(Tier2Enabled, CacheSettingsConfigKey.Tier2Enabled, env);
            TimeoutAsyncAfterSeconds = GetValue(TimeoutAsyncAfterSeconds, CacheSettingsConfigKey.TimeoutAsyncAfterSeconds, env);
        }
    }
}
