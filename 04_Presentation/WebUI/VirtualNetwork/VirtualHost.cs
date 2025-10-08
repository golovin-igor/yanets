using Microsoft.Extensions.Logging;
using Yanets.WebUI.VirtualNetwork.Models;

namespace Yanets.WebUI.VirtualNetwork
{
    public class VirtualHost : IVirtualHost
    {
        private readonly ILogger<VirtualHost> _logger;
        private readonly List<IProtocolHandler> _protocolHandlers;
        private readonly Dictionary<ProtocolType, int> _portMappings;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _listeningTask;

        public string Id { get; }
        public string Hostname { get; set; }
        public string IpAddress { get; set; }
        public string SubnetName { get; }
        public VendorProfile VendorProfile { get; set; }
        public NetworkStack NetworkStack { get; }
        public DeviceState CurrentState { get; set; }
        public HostStatus Status { get; set; } = HostStatus.Stopped;
        public DateTime CreatedAt { get; }
        public Dictionary<int, ProtocolType> PortMappings { get; }

        public event EventHandler<VirtualNetworkManager.HostStatusEventArgs> StatusChanged;

        public VirtualHost(
            string id,
            string hostname,
            string ipAddress,
            string subnetName,
            string vendor,
            ILogger<VirtualHost> logger)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Hostname = hostname ?? throw new ArgumentNullException(nameof(hostname));
            IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            SubnetName = subnetName ?? throw new ArgumentNullException(nameof(subnetName));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            CreatedAt = DateTime.UtcNow;
            _protocolHandlers = new List<IProtocolHandler>();
            _portMappings = new Dictionary<ProtocolType, int>();
            PortMappings = new Dictionary<int, ProtocolType>();
            NetworkStack = new NetworkStack(Id);

            // Initialize device state
            CurrentState = new DeviceState
            {
                RunningConfiguration = GenerateDefaultConfiguration(),
                StartupConfiguration = GenerateDefaultConfiguration(),
                Uptime = DateTime.UtcNow,
                Resources = new SystemResources
                {
                    CpuUtilization = 5,
                    MemoryTotal = 65536,
                    MemoryUsed = 12000
                }
            };

            // Initialize network stack
            InitializeNetworkStack();

            // Load vendor profile
            LoadVendorProfile(vendor);

            _logger.LogInformation("Created VirtualHost {Id} with IP {IpAddress}", Id, IpAddress);
        }

        public async Task StartAsync()
        {
            if (Status == HostStatus.Running)
            {
                _logger.LogWarning("VirtualHost {Id} is already running", Id);
                return;
            }

            try
            {
                _logger.LogInformation("Starting VirtualHost {Id}", Id);

                Status = HostStatus.Starting;
                OnStatusChanged(new VirtualNetworkManager.HostStatusEventArgs { Status = Status });

                // Create cancellation token for managing the listening task
                _cancellationTokenSource = new CancellationTokenSource();

                // Allocate ports for each protocol
                AllocateProtocolPorts();

                // Start protocol handlers
                await StartProtocolHandlersAsync();

                // Start the main listening task
                _listeningTask = Task.Run(ListenForConnectionsAsync, _cancellationTokenSource.Token);

                Status = HostStatus.Running;
                OnStatusChanged(new VirtualNetworkManager.HostStatusEventArgs { Status = Status });

                _logger.LogInformation("VirtualHost {Id} started successfully", Id);
            }
            catch (Exception ex)
            {
                Status = HostStatus.Error;
                OnStatusChanged(new VirtualNetworkManager.HostStatusEventArgs { Status = Status });

                _logger.LogError(ex, "Failed to start VirtualHost {Id}", Id);
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (Status == HostStatus.Stopped)
            {
                _logger.LogWarning("VirtualHost {Id} is already stopped", Id);
                return;
            }

            try
            {
                _logger.LogInformation("Stopping VirtualHost {Id}", Id);

                Status = HostStatus.Stopping;
                OnStatusChanged(new VirtualNetworkManager.HostStatusEventArgs { Status = Status });

                // Cancel the listening task
                _cancellationTokenSource?.Cancel();

                // Stop all protocol handlers
                await StopProtocolHandlersAsync();

                // Wait for the listening task to complete
                if (_listeningTask != null)
                {
                    await _listeningTask;
                }

                Status = HostStatus.Stopped;
                OnStatusChanged(new VirtualNetworkManager.HostStatusEventArgs { Status = Status });

                _logger.LogInformation("VirtualHost {Id} stopped successfully", Id);
            }
            catch (Exception ex)
            {
                Status = HostStatus.Error;
                OnStatusChanged(new VirtualNetworkManager.HostStatusEventArgs { Status = Status });

                _logger.LogError(ex, "Failed to stop VirtualHost {Id}", Id);
                throw;
            }
        }

        public async Task<CommandResult> ExecuteCommandAsync(string command, CliSession session)
        {
            try
            {
                if (VendorProfile?.CommandParser == null)
                {
                    return CommandResult.Error("No command parser available");
                }

                // Update session activity
                session.UpdateActivity();

                // Parse and execute command using vendor profile
                var commandDefinition = VendorProfile.CommandParser.Parse(command, VendorProfile);

                if (commandDefinition == null)
                {
                    return CommandResult.Error("Invalid command or incomplete command");
                }

                // Check privilege level
                if (session.PrivilegeLevel < commandDefinition.PrivilegeLevel)
                {
                    return CommandResult.Error("Command authorization failed");
                }

                // Execute the command
                var context = new CommandContext
                {
                    Device = this,
                    State = CurrentState,
                    RawCommand = command,
                    Session = session,
                    CurrentPrivilegeLevel = session.PrivilegeLevel
                };

                var result = await Task.Run(() => commandDefinition.Handler(context));

                // Update state if changed
                if (result.UpdatedState != null)
                {
                    CurrentState = result.UpdatedState;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command '{Command}' on host {Id}", command, Id);
                return CommandResult.Error($"Command execution failed: {ex.Message}");
            }
        }

        public async Task<SnmpResponse> HandleSnmpRequestAsync(SnmpRequest request)
        {
            try
            {
                if (VendorProfile?.MibProvider == null)
                {
                    return SnmpResponse.Error(request.RequestId, SnmpError.GenErr);
                }

                var results = new List<SnmpVarBind>();

                foreach (var oid in request.Oids)
                {
                    var handler = VendorProfile.MibProvider.GetOidHandler(oid);

                    if (handler == null)
                    {
                        results.Add(new SnmpVarBind
                        {
                            Oid = oid,
                            Error = SnmpError.NoSuchName
                        });
                        continue;
                    }

                    var value = handler.GetHandler(CurrentState);
                    results.Add(new SnmpVarBind
                    {
                        Oid = oid,
                        Value = value
                    });
                }

                return SnmpResponse.Success(request.RequestId, results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling SNMP request on host {Id}", Id);
                return SnmpResponse.Error(request.RequestId, SnmpError.GenErr);
            }
        }

        public NetworkInterface GetInterface(string name)
        {
            return NetworkStack.GetInterfaceByName(name);
        }

        public void UpdateInterfaceState(string interfaceName, InterfaceStatus status)
        {
            NetworkStack.UpdateInterfaceStatus(interfaceName, status);
            _logger.LogInformation("Updated interface {InterfaceName} status to {Status}", interfaceName, status);
        }

        public HostStatistics GetStatistics()
        {
            return new HostStatistics
            {
                HostId = Id,
                ActiveConnections = _protocolHandlers.Sum(h => GetActiveConnectionCount(h)),
                TotalCommandsExecuted = GetTotalCommandCount(),
                TotalSnmpRequests = GetTotalSnmpRequestCount(),
                Uptime = DateTime.UtcNow - CreatedAt,
                CpuUtilization = CurrentState.Resources.CpuUtilization,
                MemoryUsed = CurrentState.Resources.MemoryUsed,
                ProtocolCounts = GetProtocolCounts()
            };
        }

        public async Task<bool> SaveStateAsync()
        {
            try
            {
                var stateFile = Path.Combine("states", $"{Id}_state.json");
                Directory.CreateDirectory("states");

                var json = System.Text.Json.JsonSerializer.Serialize(CurrentState, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(stateFile, json);
                _logger.LogInformation("Saved state for host {Id} to {StateFile}", Id, stateFile);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save state for host {Id}", Id);
                return false;
            }
        }

        public async Task<bool> LoadStateAsync()
        {
            try
            {
                var stateFile = Path.Combine("states", $"{Id}_state.json");

                if (!File.Exists(stateFile))
                {
                    _logger.LogWarning("State file {StateFile} not found for host {Id}", stateFile, Id);
                    return false;
                }

                var json = await File.ReadAllTextAsync(stateFile);
                var state = System.Text.Json.JsonSerializer.Deserialize<DeviceState>(json);

                if (state != null)
                {
                    CurrentState = state;
                    _logger.LogInformation("Loaded state for host {Id} from {StateFile}", Id, stateFile);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load state for host {Id}", Id);
                return false;
            }
        }

        private async Task StartProtocolHandlersAsync()
        {
            // Create and start protocol handlers
            foreach (var (protocol, port) in _portMappings)
            {
                var handler = CreateProtocolHandler(protocol, port);
                if (handler != null)
                {
                    _protocolHandlers.Add(handler);
                    await handler.StartListeningAsync(port);
                    _logger.LogInformation("Started {Protocol} handler on port {Port} for host {Id}", protocol, port, Id);
                }
            }
        }

        private async Task StopProtocolHandlersAsync()
        {
            foreach (var handler in _protocolHandlers)
            {
                await handler.StopListeningAsync();
                handler.Dispose();
            }
            _protocolHandlers.Clear();
        }

        private void AllocateProtocolPorts()
        {
            var basePort = 23000 + (Id.GetHashCode() % 1000); // Distribute across port range

            _portMappings[ProtocolType.Telnet] = basePort;
            _portMappings[ProtocolType.Snmp] = basePort + 1000;
            _portMappings[ProtocolType.Ssh] = basePort + 2000;

            // Update port mappings for external access
            PortMappings[basePort] = ProtocolType.Telnet;
            PortMappings[basePort + 1000] = ProtocolType.Snmp;
            PortMappings[basePort + 2000] = ProtocolType.Ssh;
        }

        private IProtocolHandler CreateProtocolHandler(ProtocolType protocol, int port)
        {
            return protocol switch
            {
                ProtocolType.Telnet => new TelnetProtocolHandler(this, _logger),
                ProtocolType.Snmp => new SnmpProtocolHandler(this, _logger),
                _ => null
            };
        }

        private async Task ListenForConnectionsAsync()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    // Main listening loop - protocol handlers manage their own connections
                    await Task.Delay(1000, _cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ListenForConnectionsAsync for host {Id}", Id);
            }
        }

        private void InitializeNetworkStack()
        {
            // Add management interface
            var managementInterface = new NetworkInterface
            {
                Name = "Management",
                IpAddress = IpAddress,
                SubnetMask = GetSubnetMask(SubnetName),
                MacAddress = GenerateMacAddress(),
                Status = InterfaceStatus.Up,
                IsManagement = true
            };

            NetworkStack.AddInterface(managementInterface);

            // Initialize routing table
            NetworkStack.RoutingTable.AddRoute("0.0.0.0", "0.0.0.0", "Management", 1);

            // Initialize ARP table
            // ARP entry will be added when first packet is sent
        }

        private void LoadVendorProfile(string vendor)
        {
            // This would integrate with the existing vendor profile system
            // For now, create a basic Cisco profile
            VendorProfile = new VendorProfile
            {
                VendorName = vendor,
                Os = "IOS",
                Version = "15.0"
            };

            _logger.LogInformation("Loaded vendor profile {Vendor} for host {Id}", vendor, Id);
        }

        private string GenerateDefaultConfiguration()
        {
            return $@"hostname {Hostname}
interface Management
 ip address {IpAddress} {GetSubnetMask(SubnetName)}
 no shutdown
end";
        }

        private string GetSubnetMask(string subnetName)
        {
            // Simplified subnet mask calculation
            return subnetName switch
            {
                "default" => "255.255.255.0",
                _ => "255.255.255.0"
            };
        }

        private string GenerateMacAddress()
        {
            var random = new Random(Id.GetHashCode());
            var bytes = new byte[6];
            random.NextBytes(bytes);
            bytes[0] = (byte)(bytes[0] & 0xFE); // Clear multicast bit
            bytes[0] = (byte)(bytes[0] | 0x02);  // Set local bit

            return string.Join(":", bytes.Select(b => b.ToString("X2")));
        }

        private int GetActiveConnectionCount(IProtocolHandler handler)
        {
            // This would be implemented based on the specific protocol handler's connection tracking
            return 0;
        }

        private int GetTotalCommandCount()
        {
            // This would track total commands executed
            return 0;
        }

        private int GetTotalSnmpRequestCount()
        {
            // This would track total SNMP requests
            return 0;
        }

        private Dictionary<ProtocolType, int> GetProtocolCounts()
        {
            return new Dictionary<ProtocolType, int>
            {
                [ProtocolType.Telnet] = GetActiveConnectionCount(_protocolHandlers.FirstOrDefault(h => h.Type == ProtocolType.Telnet) ?? new NullProtocolHandler()),
                [ProtocolType.Snmp] = GetActiveConnectionCount(_protocolHandlers.FirstOrDefault(h => h.Type == ProtocolType.Snmp) ?? new NullProtocolHandler())
            };
        }

        private void OnStatusChanged(VirtualNetworkManager.HostStatusEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        // Supporting classes
        private class NullProtocolHandler : IProtocolHandler
        {
            public ProtocolType Type => ProtocolType.Telnet;
            public IVirtualHost Host => null;
            public event EventHandler<DataReceivedEventArgs> DataReceived;
            public void Dispose() { }
            public Task HandleConnectionAsync(NetworkStream stream) => Task.CompletedTask;
            public Task<byte[]> ProcessDataAsync(byte[] data) => Task.FromResult(Array.Empty<byte>());
            public Task StartListeningAsync(int port) => Task.CompletedTask;
            public Task StopListeningAsync() => Task.CompletedTask;
        }
    }
}