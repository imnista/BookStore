using BeckmanCoulter.BookStore.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace BeckmanCoulter.BookStore
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        public IConfiguration Configuration { get; }
        private readonly IHostingEnvironment _environment;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

      services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
        .AddAzureAD(options => Configuration.Bind("AzureAd", options));

      services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
      {
        options.Authority = options.Authority + "/v2.0/";

        // Per the code below, this application signs in users in any Work and School
        // accounts and any Microsoft Personal Accounts.
        // If you want to direct Azure AD to restrict the users that can sign-in, change 
        // the tenant value of the appsettings.json file in the following way:
        // - only Work and School accounts => 'organizations'
        // - only Microsoft Personal accounts => 'consumers'
        // - Work and School and Personal accounts => 'common'

        // If you want to restrict the users that can sign-in to only one tenant
        // set the tenant value in the appsettings.json file to the tenant ID of this
        // organization, and set ValidateIssuer below to true.

        // If you want to restrict the users that can sign-in to several organizations
        // Set the tenant value in the appsettings.json file to 'organizations', set
        // ValidateIssuer, above to 'true', and add the issuers you want to accept to the
        // options.TokenValidationParameters.ValidIssuers collection
        options.TokenValidationParameters.ValidateIssuer = false;
      });

      services.AddDbContext<ApplicationDbContext>(options =>
          options.UseSqlServer(
              Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            #region Add AddSerilog

            //Read configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(_environment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true);

            if (!string.IsNullOrWhiteSpace(_environment.EnvironmentName))
            {
                builder = builder.AddJsonFile($"appsettings.{_environment.EnvironmentName}.json", true);
            }

            builder = builder.AddEnvironmentVariables();
            var configuration = builder.Build();

            //build logger
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
            Log.Logger = logger;

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            #endregion Add AddSerilog
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}