using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeckmanCoulter.SwaggerPrototype.Extensions
{
  public static class AuthenticationExtensions
  {
    public static void AddAzureADB2C(this IServiceCollection services, IConfiguration configuration)
    {
      services.AddAuthentication(options => { options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
        .AddJwtBearer(jwtOptions =>
        {
          jwtOptions.Authority = string.Format(configuration["AzureADB2C:Authority"], configuration["AzureADB2C:Domain"], configuration["AzureADB2C:Policy"]);
          jwtOptions.Audience = configuration["AzureADB2C:ClientId"];
        });
    }

    public static void AddAzureAD(this IServiceCollection services, IConfiguration configuration)
    {
      services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme).AddAzureADBearer(options => configuration.Bind("AzureAD", options));
    }
  }
}