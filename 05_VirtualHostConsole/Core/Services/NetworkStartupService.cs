using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yanets.VirtualHostConsole.Core.Interfaces;

namespace Yanets.VirtualHostConsole.Core.Services
{
    public class NetworkStartupService
    {
        private readonly IVirtualNetworkManager _networkManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NetworkStartupService> _logger;

        public NetworkStartupService(
            IVirtualNetworkManager networkManager,
            IConfiguration configuration,
            ILogger<NetworkStartupService> logger)
        {
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> InitializeNetworkAsync()
        {
            try
            {
                _logger.LogInformation("Initializing virtual network");

                // Create default subnet if it doesn't exist
                await EnsureDefaultSubnetAsync();

                // Load saved configuration if available
                await LoadSavedConfigurationAsync();

                // Auto-start network if configured
                var autoStart = _configuration.GetValue<bool>("Network:AutoStart");
                if (autoStart)
                {
                    _logger.LogInformation("Auto-start enabled, starting network");
                    return await _networkManager.StartNetworkAsync();
                }

                _logger.LogInformation("Network initialization completed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize network");
                return false;
            }
        }

        public async Task<bool> CreateDefaultHostAsync()
        {
            try
            {
                var defaultVendor = _configuration.GetValue<string>("Hosts:Defaults:Vendor");
                var defaultDeviceType = _configuration.GetValue<string>("Hosts:Defaults:DeviceType");

                _logger.LogInformation("Creating default host with vendor {Vendor}", defaultVendor);

                var host = await _networkManager.CreateHostAsync("default-router", defaultVendor);

                if (host != null)
                {
                    _logger.LogInformation("Created default host {Hostname} with IP {IpAddress}",
                        host.Hostname, host.IpAddress);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create default host");
                return false;
            }
        }

        public async Task<bool> CreateHostFromConfigurationAsync(HostConfiguration config)
        {
            try
            {
                _logger.LogInformation("Creating host from configuration: {Hostname}", config.Hostname);

                var host = await _networkManager.CreateHostAsync(config.Hostname, config.Vendor);

                if (host != null && !string.IsNullOrEmpty(config.IpAddress))
                {
                    // In a full implementation, this would handle specific IP assignment
                    _logger.LogInformation("Host {Hostname} created with IP {IpAddress}",
                        config.Hostname, config.IpAddress);
                }

                return host != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create host from configuration {Hostname}", config.Hostname);
                return false;
            }
        }

        private async Task EnsureDefaultSubnetAsync()
        {
            try
            {
                var defaultCidr = _configuration.GetValue<string>("Network:DefaultSubnet");
                var defaultGateway = _configuration.GetValue<string>("Network:Gateway");

                _logger.LogInformation("Ensuring default subnet {Cidr} exists", defaultCidr);

                // Check if default subnet exists
                var existingSubnet = _networkManager.GetAllHosts()
                    .Select(h => h.SubnetName)
                    .Distinct()
                    .Any();

                if (!existingSubnet)
                {
                    _logger.LogInformation("Default subnet not found, it will be created when first host is added");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ensure default subnet");
            }
        }

        private async Task LoadSavedConfigurationAsync()
        {
            try
            {
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "network-config.json");

                if (File.Exists(configPath))
                {
                    _logger.LogInformation("Loading saved configuration from {ConfigPath}", configPath);

                    var loaded = await _networkManager.LoadConfigurationAsync(configPath);

                    if (loaded)
                    {
                        _logger.LogInformation("Configuration loaded successfully");
                    }
                    else
                    {
                        _logger.LogWarning("Failed to load configuration");
                    }
                }
                else
                {
                    _logger.LogInformation("No saved configuration found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading saved configuration");
            }
        }

        public async Task<bool> SaveCurrentConfigurationAsync()
        {
            try
            {
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "network-config.json");

                _logger.LogInformation("Saving current configuration to {ConfigPath}", configPath);

                var saved = await _networkManager.SaveConfigurationAsync(configPath);

                if (saved)
                {
                    _logger.LogInformation("Configuration saved successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to save configuration");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration");
                return false;
            }
        }
    }

    // Supporting configuration classes
    public class HostConfiguration
    {
        public string Hostname { get; set; }
        public string Vendor { get; set; }
        public string IpAddress { get; set; }
        public string SubnetName { get; set; }
        public HostStatus Status { get; set; }
    }
}