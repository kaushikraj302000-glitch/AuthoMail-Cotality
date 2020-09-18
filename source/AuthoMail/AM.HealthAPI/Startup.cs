using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.SqlServer;
using HealthChecks.System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AM.HealthAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private const string perf = "perf";
        private const string stage = "stage";
        private const string uat = "uat";
        private const string prod = "prod";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Config perf = new Config(nameof(perf), Configuration);
            //Config uat = new Config(nameof(uat), Configuration);
            //Config prod = new Config(nameof(prod), Configuration);


            //services.AddControllers();
            ConfigureHealthChecks(services, perf);
            ConfigureHealthChecks(services, stage);
            ConfigureHealthChecks(services, uat);
            ConfigureHealthChecks(services, prod);

            //services.AddControllers();
            //services.AddHealthChecks()
            //    .AddCheck("AMHealthCheck", new AMHealthCheck(Configuration["ConnectionStrings:AuthoMail"], Configuration.GetValue<string>("AMRootPath")));
        }

        private void MapHealthCheckEndPoints(string env, IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks($"/{env}", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains(env)
            });
        }

        private void ConfigureHealthChecks(IServiceCollection services, string env)
        {
            var config = new Config();
            Configuration.GetSection(env).Bind(config);

            services.AddHealthChecks()
                            .AddSqlServer(config.ConnectionString, name: $"{env}_SqlServer", tags: new[] { env })
                            .AddDiskStorageHealthCheck(s => s.AddDrive(config.AMRootPath, config.MinFreeMB), name: $"{env}_DiskStorage", tags: new[] { env })
                            .AddTypeActivatedCheck<AMHealthCheck>($"{env}_AMHealthCheck", HealthStatus.Unhealthy, args: new object[] { config.AccessCheckPath }, tags: new[] { env });
        }
        //
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllers();
                MapHealthCheckEndPoints(perf, endpoints);
                MapHealthCheckEndPoints(stage, endpoints);
                MapHealthCheckEndPoints(uat, endpoints);
                MapHealthCheckEndPoints(prod, endpoints);
            });
        }
    }

    public class AMHealthCheck : IHealthCheck
    {
        public string AccessCheckPath { get; set; }

        public AMHealthCheck(string rootPath)
        {
            AccessCheckPath = rootPath;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                ////Sql Access
                //SqlServerHealthCheck sqlServerHealthCheck = new SqlServerHealthCheck(ConnectionString, "SELECT * FROM Common.Clients");
                //await sqlServerHealthCheck.CheckHealthAsync(context);


                ////Disk storage check
                //DiskStorageOptions diskStorageOptions = new DiskStorageOptions();
                //diskStorageOptions.AddDrive("f:\\", 60240);
                //DiskStorageHealthCheck diskStorageHealthCheck = new DiskStorageHealthCheck(diskStorageOptions);

                string textWritten = "Writing process is working";

                string tempFilePath = Path.Combine(AccessCheckPath, Path.GetRandomFileName());
                //Write Access
                await File.WriteAllTextAsync(tempFilePath, textWritten);

                //Read Access
                var text = await File.ReadAllTextAsync(tempFilePath);

                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                //if (HealthCheckResult.Healthy().Status.ToString() == "Healthy")
                //    return HealthCheckResult.Healthy();
                //return HealthCheckResult.Unhealthy();

            }
            catch (IOException ex)
            {
                return new HealthCheckResult(status: context.Registration.FailureStatus, exception: ex);
            }
            return HealthCheckResult.Healthy();
        }
    }
}
