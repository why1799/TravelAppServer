using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using TravelAppServer.Configs;
using TravelAppServer.Cookies;
using TravelAppStorage.Implementations;
using TravelAppStorage.Interfaces;
using TravelAppStorage.Settings;

namespace TravelAppServer
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; set; }
        public Startup()
        {
            var builder = new ConfigurationBuilder()
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

            services.AddOptions();

            services.Configure<Settings>(Configuration.GetSection("Settings"));
            services.Configure<DBConnection>(Configuration.GetSection("DBConnection"));

            services.AddSingleton(typeof(IStorage), typeof(MongoStorage));


            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TravelApp API", Version = "v1" });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.ConfigureSwaggerGen(options => 
            {
                options.CustomSchemaIds(x => x.FullName);
            });

            services.AddRazorPages();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "TravelApp.AuthCookie";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Для https : CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Strict; //Lax если есть Auth02
                    options.EventsType = typeof(TravelAppCookieAuthenticationEvents);
                });
            services.AddScoped<TravelAppCookieAuthenticationEvents>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TravelApp API V1");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //HttpConfiguration config = new HttpConfiguration();
            //WebApiConfig.Register(config);
            //appowin.UseWebApi(config);

            app.UseCookiePolicy(new CookiePolicyOptions {
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.None, // Для https : CookieSecurePolicy.Always;
                MinimumSameSitePolicy = SameSiteMode.Strict //Lax если есть Auth02
            });

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
