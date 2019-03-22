using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Reflection;

namespace BeckmanCoulter.BookStore.Extensions
{
    /// <summary>
    /// Cache Configurations for <see cref="IConfigurationRoot"/>.
    /// </summary>
    public static class AppConfiguration
    {
        private static readonly ConcurrentDictionary<string, IConfigurationRoot> s_configurationCache;

        static AppConfiguration()
        {
            s_configurationCache = new ConcurrentDictionary<string, IConfigurationRoot>();
        }

        /// <summary>
        /// Get configuration.
        /// </summary>
        /// <param name="path">Configuration file path</param>
        /// <param name="environmentName">Hosting environment name.</param>
        /// <param name="addUserSecrets">Adds the user secrets configuration source.</param>
        /// <returns></returns>
        public static IConfigurationRoot Get(string path, string environmentName = null, bool addUserSecrets = false)
        {
            var cacheKey = $"{path}#{environmentName}#{addUserSecrets}";
            return s_configurationCache.GetOrAdd(
                cacheKey,
                _ => BuildConfiguration(path, environmentName, addUserSecrets)
            );
        }

        private static IConfigurationRoot BuildConfiguration(string path, string environmentName = null, bool addUserSecrets = false)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", true, true);

            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                builder = builder.AddJsonFile($"appsettings.{environmentName}.json", true);
            }

            builder = builder.AddEnvironmentVariables();

            if (addUserSecrets)
            {
                builder.AddUserSecrets(Assembly.GetEntryAssembly());
            }

            return builder.Build();
        }
    }
}