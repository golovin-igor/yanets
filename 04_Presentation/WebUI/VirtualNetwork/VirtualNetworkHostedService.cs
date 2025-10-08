using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Yanets.WebUI.VirtualNetwork
{
    public class VirtualNetworkHostedService : BackgroundService
    {
        private readonly IVirtualNetworkManager _networkManager;
        private readonly ILogger<VirtualNetworkHostedService> _logger;
        private readonly IConfiguration _configuration;

        public VirtualNetworkHostedService(
            IVirtualNetworkManager networkManager,
            ILogger<VirtualNetworkHostedService> logger,
            IConfiguration configuration)
        {
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // No recurring loop needed; startup work is done in StartAsync.
            await Task.CompletedTask;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("VirtualNetworkHostedService starting...");

                // Optionally create a default host on startup (config-driven)
                var autoCreateDefaultHost = _configuration.GetValue<bool>("VirtualNetwork:AutoCreateDefaultHost", true);
                var defaultVendor = _configuration.GetValue<string>("VirtualNetwork:DefaultVendor", "cisco");

                if (autoCreateDefaultHost)
                {
                    // Only create a default host if there are none
                    if (!_networkManager.GetAllHosts().Any())
                    {
                        var host = await _networkManager.CreateHostAsync("router1", defaultVendor, "default");
                        _logger.LogInformation("Created default virtual host {Hostname} with IP {Ip}", host.Hostname, host.IpAddress);
                    }
                }

                var autoStart = _configuration.GetValue<bool>("VirtualNetwork:AutoStart", true);
                if (autoStart)
                {
                    var started = await _networkManager.StartNetworkAsync();
                    if (started)
                    {
                        _logger.LogInformation("Virtual network started successfully on application start");
                    }
                    else
                    {
                        _logger.LogWarning("Virtual network failed to start on application start");
                    }
                }
                else
                {
                    _logger.LogInformation("Virtual network auto-start is disabled by configuration");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during VirtualNetworkHostedService StartAsync");
            }

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("VirtualNetworkHostedService stopping...");

                var stopped = await _networkManager.StopNetworkAsync();
                if (stopped)
                {
                    _logger.LogInformation("Virtual network stopped successfully on application stop");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during VirtualNetworkHostedService StopAsync");
            }

            await base.StopAsync(cancellationToken);
        }
    }
}