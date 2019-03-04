using BeckmanCoulter.SwaggerPrototype.Extensions;
using BeckmanCoulter.SwaggerPrototype.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeckmanCoulter.SwaggerPrototype
{
  public class Startup
  {
    public Startup(IConfiguration configuration, IHostingEnvironment environment)
    {
      _configuration = configuration;
      _environment = environment;
    }

    private readonly IConfiguration _configuration;
    private readonly IHostingEnvironment _environment;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));

      if (_environment.IsDevelopment())
      {
        services.AddAzureADB2C(_configuration);
      }
      else
      {
        services.AddAzureAD(_configuration);
      }


      services.AddMvc(config =>
      {
        var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
        config.Filters.Add(new AuthorizeFilter(policy));
      }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      // Register the Swagger generator, defining 1 or more Swagger documents
      if (_environment.IsDevelopment())
      {
        services.AddSwaggerWithAzureADB2C(_configuration);
      }
      else
      {
        services.AddSwaggerWithAzureAD(_configuration);
      }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (_environment.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseHsts();
      }

      app.UseStaticFiles();

      if (_environment.IsDevelopment())
      {
        app.UseSwaggerWithAzureADB2C(_configuration);
      }
      else
      {
        app.UseSwaggerWithAzureAD(_configuration);
      }

      app.UseHttpsRedirection();

      app.UseAuthentication();
      app.UseMvc();
    }
  }
}
