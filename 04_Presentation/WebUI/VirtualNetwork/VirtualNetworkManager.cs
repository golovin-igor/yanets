using Microsoft.Extensions.Logging;
using System.Timers;
using Yanets.WebUI.VirtualNetwork.Models;

namespace Yanets.WebUI.VirtualNetwork
{
    public class VirtualNetworkManager : IVirtualNetworkManager, IDisposable
    {
        private readonly ILogger<VirtualNetworkManager> _logger;
        private readonly List<IVirtualHost> _hosts;
        private readonly Dictionary<string, SubnetDefinition> _subnets;
        private bool _isNetworkRunning;
        private Timer _statisticsTimer;

        public event EventHandler<NetworkEventArgs> NetworkEvent;

        public VirtualNetworkManager(ILogger<VirtualNetworkManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hosts = new List<IVirtualHost>();
            _subnets = new Dictionary<string, SubnetDefinition>();
            _isNetworkRunning = false;

            // Initialize default subnet
            InitializeDefaultSubnet();
        }

        public async Task<IVirtualHost> CreateHostAsync(string hostname, string vendor, string subnetName = "default")
        {
            try
            {
                _logger.LogInformation("Creating virtual host: {Hostname} with vendor: {Vendor}", hostname, vendor);

                // Check if subnet exists
                if (!_subnets.ContainsKey(subnetName))
                {
                    throw new ArgumentException($"Subnet '{subnetName}' does not exist");
                }

                // Allocate IP address from subnet
                var subnet = _subnets[subnetName];
                var ipAddress = AllocateIpFromSubnet(subnet);
                if (string.IsNullOrEmpty(ipAddress))
                {
                    throw new InvalidOperationException($"No available IP addresses in subnet '{subnetName}'");
                }

                // Generate unique host ID
                var hostId = Guid.NewGuid().ToString();

                // Create virtual host instance
                var host = new VirtualHost(hostId, hostname, ipAddress, subnetName, vendor, _logger);

                // Add to collections
                _hosts.Add(host);

                // Set up event handlers
                host.StatusChanged += OnHostStatusChanged;

                _logger.LogInformation("Created virtual host {HostId} with IP {IpAddress}", hostId, ipAddress);

                // Raise network event
                RaiseNetworkEvent(NetworkEventType.HostCreated, hostId,
                    $"Host {hostname} created with IP {ipAddress}");

                return host;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create virtual host {Hostname}", hostname);
                throw;
            }
        }

        public async Task<bool> StartNetworkAsync()
        {
            if (_isNetworkRunning)
            {
                _logger.LogWarning("Network is already running");
                return true;
            }

            try
            {
                _logger.LogInformation("Starting virtual network with {HostCount} hosts", _hosts.Count);

                // Start all hosts
                var startTasks = _hosts.Select(host => host.StartAsync());
                await Task.WhenAll(startTasks);

                // Start statistics collection
                StartStatisticsCollection();

                _isNetworkRunning = true;

                _logger.LogInformation("Virtual network started successfully");
                RaiseNetworkEvent(NetworkEventType.HostStarted, null, "Virtual network started");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start virtual network");
                return false;
            }
        }

        public async Task<bool> StopNetworkAsync()
        {
            if (!_isNetworkRunning)
            {
                _logger.LogWarning("Network is not running");
                return true;
            }

            try
            {
                _logger.LogInformation("Stopping virtual network");

                // Stop statistics collection
                StopStatisticsCollection();

                // Stop all hosts
                var stopTasks = _hosts.Select(host => host.StopAsync());
                await Task.WhenAll(stopTasks);

                _isNetworkRunning = false;

                _logger.LogInformation("Virtual network stopped successfully");
                RaiseNetworkEvent(NetworkEventType.HostStopped, null, "Virtual network stopped");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop virtual network");
                return false;
            }
        }

        public async Task<bool> RemoveHostAsync(string hostId)
        {
            var host = _hosts.FirstOrDefault(h => h.Id == hostId);
            if (host == null)
            {
                _logger.LogWarning("Host {HostId} not found", hostId);
                return false;
            }

            try
            {
                _logger.LogInformation("Removing virtual host {HostId}", hostId);

                // Stop the host if it's running
                if (host.Status == HostStatus.Running)
                {
                    await host.StopAsync();
                }

                // Release IP address
                if (_subnets.TryGetValue(host.SubnetName, out var subnet))
                {
                    subnet.AllocatedIps.Remove(host.IpAddress);
                }

                // Remove from collections
                _hosts.Remove(host);

                // Clean up event handlers
                host.StatusChanged -= OnHostStatusChanged;

                _logger.LogInformation("Virtual host {HostId} removed successfully", hostId);
                RaiseNetworkEvent(NetworkEventType.HostRemoved, hostId, $"Host {host.Hostname} removed");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove virtual host {HostId}", hostId);
                return false;
            }
        }

        public NetworkStatistics GetNetworkStatistics()
        {
            var stats = new NetworkStatistics
            {
                TotalHosts = _hosts.Count,
                ActiveHosts = _hosts.Count(h => h.Status == HostStatus.Running),
                LastUpdated = DateTime.UtcNow
            };

            // Aggregate per-host statistics
            foreach (var host in _hosts)
            {
                var hostStats = host.GetStatistics();
                stats.PerHostStats[host.Id] = hostStats;

                // Update connection counts
                stats.TotalConnections += hostStats.ActiveConnections;
                if (host.Status == HostStatus.Running)
                {
                    stats.ActiveConnections += hostStats.ActiveConnections;
                }

                // Update protocol counts
                foreach (var protocolCount in hostStats.ProtocolCounts)
                {
                    stats.ProtocolCounts[protocolCount.Key] =
                        stats.ProtocolCounts.GetValueOrDefault(protocolCount.Key) + protocolCount.Value;
                }
            }

            return stats;
        }

        public IEnumerable<IVirtualHost> GetAllHosts() => _hosts.AsReadOnly();

        public IVirtualHost GetHostById(string hostId) => _hosts.FirstOrDefault(h => h.Id == hostId);

        public IVirtualHost GetHostByIp(string ipAddress) => _hosts.FirstOrDefault(h => h.IpAddress == ipAddress);

        public async Task<bool> SaveConfigurationAsync(string filePath)
        {
            try
            {
                var config = new NetworkConfiguration
                {
                    Subnets = _subnets.Values.ToList(),
                    Hosts = _hosts.Select(h => new HostConfiguration
                    {
                        Id = h.Id,
                        Hostname = h.Hostname,
                        IpAddress = h.IpAddress,
                        SubnetName = h.SubnetName,
                        Vendor = h.VendorProfile?.VendorName ?? "Unknown",
                        Status = h.Status
                    }).ToList(),
                    SavedAt = DateTime.UtcNow
                };

                var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(filePath, json);
                _logger.LogInformation("Network configuration saved to {FilePath}", filePath);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save network configuration");
                return false;
            }
        }

        public async Task<bool> LoadConfigurationAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Configuration file {FilePath} not found", filePath);
                    return false;
                }

                var json = await File.ReadAllTextAsync(filePath);
                var config = System.Text.Json.JsonSerializer.Deserialize<NetworkConfiguration>(json);

                if (config == null)
                {
                    _logger.LogError("Failed to deserialize configuration from {FilePath}", filePath);
                    return false;
                }

                _logger.LogInformation("Network configuration loaded from {FilePath}", filePath);

                // TODO: Implement configuration loading logic
                // This would recreate subnets and hosts based on the loaded configuration

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load network configuration");
                return false;
            }
        }

        private void InitializeDefaultSubnet()
        {
            try
            {
                var subnet = new SubnetDefinition
                {
                    Name = "default",
                    Cidr = "192.168.1.0/24",
                    NetworkAddress = System.Net.IPAddress.Parse("192.168.1.0"),
                    Gateway = System.Net.IPAddress.Parse("192.168.1.1"),
                    PrefixLength = 24,
                    ExcludedIps = new HashSet<string>
                    {
                        "192.168.1.0",  // Network address
                        "192.168.1.1",  // Gateway (will be allocated to first host)
                        "192.168.1.255" // Broadcast
                    }
                };

                _subnets["default"] = subnet;
                _logger.LogInformation("Initialized default subnet {Cidr}", subnet.Cidr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize default subnet");
            }
        }

        private string AllocateIpFromSubnet(SubnetDefinition subnet)
        {
            // Simple IP allocation - find next available IP
            for (int i = 2; i < 255; i++) // Start from .2 to avoid gateway
            {
                var ip = $"192.168.1.{i}";
                if (!subnet.AllocatedIps.Contains(ip) && !subnet.ExcludedIps.Contains(ip))
                {
                    subnet.AllocatedIps.Add(ip);
                    return ip;
                }
            }

            return null; // No available IPs
        }

        private void OnHostStatusChanged(object sender, HostStatusEventArgs e)
        {
            var host = sender as IVirtualHost;
            _logger.LogInformation("Host {HostId} status changed to {Status}", host?.Id, e.Status);

            RaiseNetworkEvent(
                e.Status == HostStatus.Running ? NetworkEventType.HostStarted : NetworkEventType.HostStopped,
                host?.Id,
                $"Host {host?.Hostname} status changed to {e.Status}");
        }

        private void RaiseNetworkEvent(NetworkEventType eventType, string hostId, string message)
        {
            NetworkEvent?.Invoke(this, new NetworkEventArgs
            {
                EventType = eventType,
                HostId = hostId,
                Message = message
            });
        }

        private void StartStatisticsCollection()
        {
            _statisticsTimer = new Timer(_ => UpdateNetworkStatistics(), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        private void StopStatisticsCollection()
        {
            _statisticsTimer?.Dispose();
            _statisticsTimer = null;
        }

        private void UpdateNetworkStatistics()
        {
            try
            {
                var stats = GetNetworkStatistics();
                // Could emit statistics update events here for real-time monitoring
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update network statistics");
            }
        }

        public void Dispose()
        {
            StopStatisticsCollection();
            _ = StopNetworkAsync(); // Fire and forget
            GC.SuppressFinalize(this);
        }

        // Supporting classes for configuration and events
        public class NetworkConfiguration
        {
            public List<SubnetDefinition> Subnets { get; set; } = new();
            public List<HostConfiguration> Hosts { get; set; } = new();
            public DateTime SavedAt { get; set; }
        }

        public class HostConfiguration
        {
            public string Id { get; set; }
            public string Hostname { get; set; }
            public string IpAddress { get; set; }
            public string SubnetName { get; set; }
            public string Vendor { get; set; }
            public HostStatus Status { get; set; }
        }

        public class HostStatusEventArgs : EventArgs
        {
            public HostStatus Status { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }
    }
}