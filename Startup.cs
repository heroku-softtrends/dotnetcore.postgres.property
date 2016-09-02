using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostgresSampleApp.Model;
using System.IO;

namespace PostgresSampleApp
{
    public class Startup
    {
        public static string appRoutePath = string.Empty;
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            //if (env.IsDevelopment())
            //{
            //    // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
            //    builder.AddApplicationInsightsSettings(developerMode: true);
            //}
            Configuration = builder.Build();

           
    }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            //services.AddApplicationInsightsTelemetry(Configuration);
            services.AddMemoryCache(options =>
            {
                options.ExpirationScanFrequency = TimeSpan.FromMinutes(300);
            });
            services.AddSession();

            services.AddMvc();

            //Get Database Connection 
            string _connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
            _connectionString.Replace("//", "");

            char[] delimiterChars = { '/', ':', '@', '?' };
            string[] strConn = _connectionString.Split(delimiterChars);
            strConn = strConn.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            Config.User = strConn[1];
            Config.Pass = strConn[2];
            Config.Server = strConn[3];
            Config.Database = strConn[5];
            Config.Port = strConn[4];
            Config.ConnectionString = "host=" + Config.Server + ";port=" + Config.Port + ";database=" + Config.Database + ";uid=" + Config.User + ";pwd=" + Config.Pass + ";sslmode=Require;Trust Server Certificate=true;Timeout=1000";
            //Config.ConnectionString = "Host=ec2-54-243-210-223.compute-1.amazonaws.com;Port=5432;User Id=bpqmfsatgaabpb;Password=TIpfRzH32iobV_8Q8Fi--bx9IV;Database=d6kdk6sivm8mpe;sslmode=Require;Trust Server Certificate=true;Timeout=1000";
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            appRoutePath = Path.Combine(env.ContentRootPath, "Data", "property.sql");

            app.UseSession();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            //app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //app.UseApplicationInsightsExceptionTelemetry();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Property}/{action=Index}/{id?}");
            });
        }
    }
}
