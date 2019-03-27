
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace BeckmanCoulter.BookStore.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IHostingEnvironment"/>.
    /// </summary>
    public static class HostingEnvironmentExtensions
    {
        /// <summary>
        /// Get configuration from IHostingEnvironment.
        /// </summary>
        /// <param name="env">Hosting Environment</param>
        /// <returns></returns>
        public static IConfigurationRoot GetAppConfiguration(this IHostingEnvironment env)
        {
            return AppConfiguration.Get(env.ContentRootPath, env.EnvironmentName);
        }
    }
}