using System;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Configuration;

namespace BeckmanCoulter.Serilog
{
  /// <summary>
  /// Extends <see cref="LoggerConfiguration"/> to add enrichers for <see cref="Microsoft.AspNetCore.Http.HttpContext"/>.
  /// capabilities.
  /// </summary>
  public static class LoggerEnrichmentConfigurationExtensions
  {
    /// <summary>
    /// Enrich log events with Aspnetcore httpContext properties.
    /// </summary>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration WithAspNetCoreHttpcontext(this LoggerEnrichmentConfiguration enrichmentConfiguration,
      Func<IHttpContextAccessor, object> optionAction = null)
    {
      if (enrichmentConfiguration == null)
        throw new ArgumentNullException(nameof(enrichmentConfiguration));
            //IIocManager iocManager = IocManager.Instance;
            //var enricher = iocManager.IocContainer.Resolve<HttpContextEnricher>();
            var enricher = new HttpContextEnricher(null);
            if (optionAction != null)
      {
        enricher.SetCustomAction(optionAction);
      }

      return enrichmentConfiguration.With(enricher);
    }
  }
}
