using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace BeckmanCoulter.SwaggerPrototype.Extensions
{
  public static class SwaggerExtensions
  {

    #region Use Azure AD B2C to do authentication

    public static void AddSwaggerWithAzureADB2C(this IServiceCollection services, IConfiguration configuration)
    {
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new Info
        {
          Version = "v1",
          Title = "ToDo API",
          Description = "A simple example ASP.NET Core Web API",
          TermsOfService = "None",
          Contact = new Contact
          {
            Name = "Shayne Boyer",
            Email = string.Empty,
            Url = "https://twitter.com/spboyer"
          },
          License = new License
          {
            Name = "Use under LICX",
            Url = "https://example.com/license"
          }
        });

        // Generate Scopes
        var scopes = configuration["SwaggerADB2C:Scopes"].Split(",", StringSplitOptions.RemoveEmptyEntries);
        var scopesDic = new Dictionary<string, string>();
        foreach (var scope in scopes)
        {
          scopesDic.Add(string.Format(configuration["AzureADB2C:AppIdUrl"], scope), "");
        }

        c.AddSecurityDefinition("oauth2", new OAuth2Scheme
        {
          Type = "oauth2",
          Flow = "implicit",
          TokenUrl = string.Format(configuration["SwaggerADB2C:TokenUrl"], configuration["AzureADB2C:Domain"], configuration["AzureADB2C:Policy"]),
          AuthorizationUrl = string.Format(configuration["SwaggerADB2C:AuthorizationUrl"], configuration["AzureADB2C:Domain"], configuration["AzureADB2C:Policy"]),
          Scopes = scopesDic
        });
        c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
        {
          { "oauth2", scopesDic.Keys.ToList() }
        });

        // Set the comments path for the Swagger JSON and UI.
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
      });
    }

    public static void UseSwaggerWithAzureADB2C(this IApplicationBuilder app, IConfiguration configuration)
    {
      // Enable middleware to serve generated Swagger as a JSON endpoint.
      app.UseSwagger();

      // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
      // specifying the Swagger JSON endpoint.
      app.UseSwaggerUI(options =>
      {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        options.OAuthClientId(configuration["SwaggerADB2C:ClientId"]);
        options.OAuthRealm(configuration["SwaggerADB2C:Realm"]);
        options.OAuthAppName("My API V1");
        options.OAuthScopeSeparator(" ");
      });
    }

    #endregion

    #region Use Azure AD to do authentication

    public static void AddSwaggerWithAzureAD(this IServiceCollection services, IConfiguration configuration)
    {
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new Info
        {
          Version = "v1",
          Title = "ToDo API",
          Description = "A simple example ASP.NET Core Web API",
          TermsOfService = "None",
          Contact = new Contact
          {
            Name = "Shayne Boyer",
            Email = string.Empty,
            Url = "https://twitter.com/spboyer"
          },
          License = new License
          {
            Name = "Use under LICX",
            Url = "https://example.com/license"
          }
        });

        c.AddSecurityDefinition("oauth2", new OAuth2Scheme
        {
          Type = "oauth2",
          Flow = "implicit",
          AuthorizationUrl = $"https://login.microsoftonline.com/{configuration["AzureAD:TenantId"]}/oauth2/authorize",
          Scopes = new Dictionary<string, string>
          {
            { "user_impersonation", "Access API" }
          }
        });
        c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
        {
          { "oauth2", new[] { "user_impersonation" } }
        });

        // Set the comments path for the Swagger JSON and UI.
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
      });
    }

    public static void UseSwaggerWithAzureAD(this IApplicationBuilder app, IConfiguration configuration)
    {
      // Enable middleware to serve generated Swagger as a JSON endpoint.
      app.UseSwagger();

      // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
      // specifying the Swagger JSON endpoint.
      app.UseSwaggerUI(c =>
      {
        c.OAuthClientId(configuration["SwaggerAD:ClientId"]);
        c.OAuthRealm(configuration["SwaggerAD:ClientId"]);
        c.OAuthAppName("My API V1");
        c.OAuthScopeSeparator(" ");
        c.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
        {
          {"resource", configuration["AzureAD:ClientId"]}
        });
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
      });
    }

    #endregion
  }
}