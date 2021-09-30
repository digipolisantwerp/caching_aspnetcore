using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Digipolis.Caching.Options._Base
{
    internal abstract class SettingsBase
    {
        public static string GetValue(string value, string configKey, IWebHostEnvironment env)
        {
            var configKeyValue = Environment.GetEnvironmentVariable(configKey);
            return configKeyValue ?? ((env.IsDevelopment() || env.IsEnvironment(Constants.Config.Environment.IntegrationTesting)) ? value : throw new ArgumentException($"Configuration error: invalid parameter '{configKey}'"));
        }

        public static int GetValue(int value, string configKey, IWebHostEnvironment env)
        {
            return int.TryParse(Environment.GetEnvironmentVariable(configKey), out int configKeyValue) ?
                  configKeyValue : ((env.IsDevelopment() || env.IsEnvironment(Constants.Config.Environment.IntegrationTesting)) ? value : throw new ArgumentException($"Configuration error: invalid parameter '{configKey}'"));
        }

        public static bool GetValue(bool value, string configKey, IWebHostEnvironment env)
        {
            return bool.TryParse(Environment.GetEnvironmentVariable(configKey), out bool configKeyValue) ?
                              configKeyValue : ((env.IsDevelopment() || env.IsEnvironment(Constants.Config.Environment.IntegrationTesting)) ? value : throw new ArgumentException($"Configuration error: invalid parameter '{configKey}'"));
        }
    }
}
