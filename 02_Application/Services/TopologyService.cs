using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Core.Vendors;

namespace Yanets.Application.Services
{
    /// <summary>
    /// Implementation of topology management service
    /// </summary>
    public class TopologyService : ITopologyService
    {
        private readonly Dictionary<Guid, NetworkTopology> _topologies;
        private readonly Dictionary<Guid, bool> _simulationStatus;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TopologyService> _logger;

        public TopologyService(IServiceProvider serviceProvider, ILogger<TopologyService> logger)
        {
            _topologies = new Dictionary<Guid, NetworkTopology>();
            _simulationStatus = new Dictionary<Guid, bool>();
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public NetworkTopology? GetTopology(Guid id)
        {
            return _topologies.TryGetValue(id, out var topology) ? topology : null;
        }

        public IEnumerable<NetworkTopology> GetAllTopologies()
        {
            return _topologies.Values;
        }

        public void SaveTopology(NetworkTopology topology)
        {
            if (topology == null)
                throw new ArgumentNullException(nameof(topology));

            if (!topology.IsValid())
                throw new ArgumentException("Invalid topology", nameof(topology));

            _topologies[topology.Id] = topology;
            _simulationStatus[topology.Id] = false; // Default to not running

            _logger.LogInformation("Saved topology {TopologyId} with {DeviceCount} devices",
                topology.Id, topology.Devices.Count);
        }

        public void DeleteTopology(Guid id)
        {
            if (_topologies.TryGetValue(id, out var topology))
            {
                // Stop simulation if running
                if (_simulationStatus.TryGetValue(id, out var isRunning) && isRunning)
                {
                    StopTopologySimulationAsync(id).Wait();
                }

                _topologies.Remove(id);
                _simulationStatus.Remove(id);

                _logger.LogInformation("Deleted topology {TopologyId}", id);
            }
        }

        public async Task StartTopologySimulationAsync(Guid topologyId)
        {
            var topology = GetTopology(topologyId);
            if (topology == null)
                throw new ArgumentException("Topology not found", nameof(topologyId));

            if (_simulationStatus.TryGetValue(topologyId, out var isRunning) && isRunning)
                throw new InvalidOperationException("Simulation already running for topology");

            try
            {
                // Initialize all devices in the topology
                foreach (var device in topology.Devices)
                {
                    await InitializeDeviceForSimulation(device);
                }

                // Start CLI and SNMP servers for each device
                var cliServer = _serviceProvider.GetService<CliServerService>();
                var snmpAgent = _serviceProvider.GetService<SnmpAgentService>();

                if (cliServer != null && snmpAgent != null)
                {
                    foreach (var device in topology.Devices)
                    {
                        // Start CLI server (Telnet)
                        cliServer.StartCliServer(device, GetTelnetPort(device));

                        // Start SNMP agent
                        snmpAgent.StartSnmpAgent(device, GetSnmpPort(device));
                    }
                }

                _simulationStatus[topologyId] = true;
                _logger.LogInformation("Started simulation for topology {TopologyId}", topologyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start simulation for topology {TopologyId}", topologyId);
                await StopTopologySimulationAsync(topologyId); // Cleanup
                throw;
            }
        }

        public async Task StopTopologySimulationAsync(Guid topologyId)
        {
            if (!_simulationStatus.TryGetValue(topologyId, out var isRunning) || !isRunning)
                return;

            try
            {
                // Stop all servers for devices in this topology
                var topology = GetTopology(topologyId);
                if (topology != null)
                {
                    var cliServer = _serviceProvider.GetService<CliServerService>();
                    var snmpAgent = _serviceProvider.GetService<SnmpAgentService>();

                    if (cliServer != null && snmpAgent != null)
                    {
                        foreach (var device in topology.Devices)
                        {
                            cliServer.StopCliServer(device.Id);
                            snmpAgent.StopSnmpAgent(device.Id);
                        }
                    }
                }

                _simulationStatus[topologyId] = false;
                _logger.LogInformation("Stopped simulation for topology {TopologyId}", topologyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping simulation for topology {TopologyId}", topologyId);
            }
        }

        public bool IsTopologyRunning(Guid topologyId)
        {
            return _simulationStatus.TryGetValue(topologyId, out var isRunning) && isRunning;
        }

        private async Task InitializeDeviceForSimulation(NetworkDevice device)
        {
            // Initialize device state
            if (device.State == null)
            {
                device.State = new DeviceState
                {
                    Uptime = DateTime.Now,
                    Resources = new SystemResources
                    {
                        CpuUtilization = 5,
                        MemoryTotal = 65536,
                        MemoryUsed = 12000
                    },
                    IsSimulationRunning = true
                };
            }
            else
            {
                device.State.IsSimulationRunning = true;
                device.State.Uptime = DateTime.Now;
            }

            // Ensure vendor is properly set
            if (device.Vendor == null)
            {
                device.Vendor = CreateDefaultVendorProfile(device.Type);
            }

            // Create and initialize device simulator
            var simulator = _serviceProvider.GetService<IDeviceSimulator>();
            if (simulator != null)
            {
                await simulator.InitializeAsync(device);
            }
        }

        private VendorProfile CreateDefaultVendorProfile(DeviceType deviceType)
        {
            return deviceType switch
            {
                DeviceType.Router => new CiscoIosVendorProfile(),
                DeviceType.Switch => new CiscoIosVendorProfile(),
                _ => new CiscoIosVendorProfile()
            };
        }

        private int GetTelnetPort(NetworkDevice device)
        {
            // Use device ID hash to distribute ports
            return 23000 + (Math.Abs(device.Id.GetHashCode()) % 1000);
        }

        private int GetSnmpPort(NetworkDevice device)
        {
            // Use device ID hash to distribute ports
            return 16100 + (Math.Abs(device.Id.GetHashCode()) % 1000);
        }
    }
}
