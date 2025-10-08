using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using Yanets.VirtualHostConsole.Core.Interfaces;
using Yanets.VirtualHostConsole.Core.Services;
using Yanets.VirtualHostConsole.Networking;
using Yanets.VirtualHostConsole.Simulation;
using Yanets.VirtualHostConsole.Console;

namespace Yanets.VirtualHostConsole
{
    public class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Set up dependency injection
            var serviceProvider = ConfigureServices(configuration);

            // Create logger
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Starting YANETS Virtual Host Console");

                // Create main application
                var app = new VirtualHostConsoleApp(
                    serviceProvider.GetRequiredService<IVirtualNetworkManager>(),
                    serviceProvider.GetRequiredService<ICommandParser>(),
                    serviceProvider.GetRequiredService<ILogger<VirtualHostConsoleApp>>()
                );

                // Set up command line interface
                var rootCommand = CommandLineInterface.CreateRootCommand(app);

                // Run the application
                return await rootCommand.InvokeAsync(args);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Application failed to start");
                return 1;
            }
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
                builder.AddDebug();
            });

            // Add configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Register core services
            services.AddSingleton<IVirtualNetworkManager, VirtualNetworkManager>();
            services.AddSingleton<ISubnetManager, SubnetManager>();
            services.AddSingleton<IConnectionRouter, ConnectionRouter>();
            services.AddSingleton<ISessionManager, SessionManager>();

            // Register protocol handlers
            services.AddTransient<IProtocolHandler, TelnetProtocolHandler>();
            services.AddTransient<IProtocolHandler, SnmpProtocolHandler>();

            // Register command parser
            services.AddSingleton<ICommandParser, CommandParser>();

            // Register console services
            services.AddSingleton<InteractiveShell>();
            services.AddSingleton<StatusDisplay>();

            return services.BuildServiceProvider();
        }
    }
}