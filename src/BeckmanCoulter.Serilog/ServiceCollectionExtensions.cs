using Castle.MicroKernel.Registration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BeckmanCoulter.Serilog
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Serilog to the logging pipeline.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="env">Hosting Environment</param>
        public static void AddSerilog(this IServiceCollection services, IHostingEnvironment env)
        {
            //Register dependent  components
            //IIocManager iocManager = IocManager.Instance;
            //iocManager.IocContainer.Register(
            //  Component.For(typeof(IHttpContextAccessor))
            //    .ImplementedBy(typeof(HttpContextAccessor))
            //    .LifestyleSingleton(),
            //  Component.For<HttpContextEnricher>()
            //    .LifestyleTransient());

            //Read configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true);

            if (!string.IsNullOrWhiteSpace(env.EnvironmentName))
            {
                builder = builder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);
            }

            builder = builder.AddEnvironmentVariables();
            var configuration = builder.Build();

            //build logger
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.FromLogContext()
                .Enrich.WithAspNetCoreHttpcontext()
                .CreateLogger();
            Log.Logger = logger;

            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(dispose: true));
        }
    }
}
