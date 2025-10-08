using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.Application.Services;
using Yanets.WebUI.Services;
using Yanets.SharedKernel;

namespace Yanets.WebUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "YANETS - Yet Another Network Equipment Test Simulator",
                    Version = "v1",
                    Description = "A comprehensive network device simulation system with CLI and SNMP support"
                });
            });

            // Register YANETS services
            services.AddSingleton<Yanets.Application.Services.ITopologyService, Yanets.Application.Services.TopologyService>();
            services.AddTransient<Yanets.Application.Services.IDeviceSimulator, Yanets.Application.Services.DeviceSimulatorService>();
            services.AddSingleton<Yanets.Core.Interfaces.ICommandParser, Yanets.Application.Services.CommandParser>();
            services.AddSingleton<Yanets.Core.Interfaces.IMibProvider, Yanets.Application.Services.MibProvider>();
            services.AddSingleton<Yanets.Core.Interfaces.IPromptGenerator, Yanets.Application.Services.PromptGenerator>();

            // Register infrastructure services
            services.AddSingleton<CliServerService>();
            services.AddSingleton<SnmpAgentService>();
            // Register Virtual Network services
            // Hosted service to auto-start the virtual network on app startup
            services.AddHostedService<Yanets.WebUI.VirtualNetwork.VirtualNetworkHostedService>();
            services.AddSingleton<Yanets.WebUI.VirtualNetwork.IVirtualNetworkManager, Yanets.WebUI.VirtualNetwork.VirtualNetworkManager>();
            services.AddTransient<Yanets.WebUI.VirtualNetwork.IProtocolHandler, Yanets.WebUI.VirtualNetwork.ProtocolHandlers.TelnetProtocolHandler>();
            services.AddTransient<Yanets.WebUI.VirtualNetwork.IProtocolHandler, Yanets.WebUI.VirtualNetwork.ProtocolHandlers.SnmpProtocolHandler>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
            
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "YANETS API v1");
                    c.RoutePrefix = string.Empty; // Serve Swagger UI at root
                });
            }

            app.UseHttpsRedirection();

                // Redirect root to the Virtual Network dashboard
                endpoints.MapGet("/", async context =>
                {
                    context.Response.Redirect("/virtual-network");
                    await Task.CompletedTask;
                });
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Health check endpoint
                endpoints.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));
            });
        }
    }
}
