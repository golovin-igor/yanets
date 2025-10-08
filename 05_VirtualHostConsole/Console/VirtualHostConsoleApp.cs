using System.CommandLine;
using Microsoft.Extensions.Logging;
using Yanets.VirtualHostConsole.Core.Interfaces;
using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Console
{
    public class VirtualHostConsoleApp
    {
        private readonly IVirtualNetworkManager _networkManager;
        private readonly ICommandParser _commandParser;
        private readonly ILogger<VirtualHostConsoleApp> _logger;
        private readonly InteractiveShell _shell;
        private bool _isRunning;

        public VirtualHostConsoleApp(
            IVirtualNetworkManager networkManager,
            ICommandParser commandParser,
            ILogger<VirtualHostConsoleApp> logger)
        {
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            _commandParser = commandParser ?? throw new ArgumentNullException(nameof(commandParser));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _shell = new InteractiveShell(_networkManager, _logger);
            _isRunning = false;

            // Subscribe to network events
            _networkManager.NetworkEvent += OnNetworkEvent;
        }

        public async Task<int> RunAsync(string[] args)
        {
            try
            {
                _logger.LogInformation("Starting YANETS Virtual Host Console");

                // Set up command line interface
                var rootCommand = CommandLineInterface.CreateRootCommand(this);

                // If no arguments provided, enter interactive mode
                if (args.Length == 0)
                {
                    return await RunInteractiveAsync();
                }

                // Otherwise, execute command line interface
                return await rootCommand.InvokeAsync(args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Application failed to start");
                return 1;
            }
        }

        private async Task<int> RunInteractiveAsync()
        {
            _isRunning = true;
            Console.WriteLine("YANETS Virtual Host Console");
            Console.WriteLine("==========================");
            Console.WriteLine("Type 'help' for commands or 'exit' to quit");
            Console.WriteLine();

            try
            {
                await _shell.RunAsync();

                while (_isRunning)
                {
                    var line = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                        line.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    await _shell.ExecuteCommandAsync(line);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in interactive mode");
                return 1;
            }
            finally
            {
                _isRunning = false;
                await CleanupAsync();
            }

            return 0;
        }

        public async Task ShowStatusAsync()
        {
            var stats = _networkManager.GetNetworkStatistics();

            Console.WriteLine("Network Status");
            Console.WriteLine("==============");
            Console.WriteLine($"Total Hosts: {stats.TotalHosts}");
            Console.WriteLine($"Active Hosts: {stats.ActiveHosts}");
            Console.WriteLine($"Total Connections: {stats.TotalConnections}");
            Console.WriteLine($"Active Connections: {stats.ActiveConnections}");
            Console.WriteLine($"Last Updated: {stats.LastUpdated}");
            Console.WriteLine();

            // Show per-host statistics
            if (stats.PerHostStats.Any())
            {
                Console.WriteLine("Host Details:");
                Console.WriteLine("-------------");
                foreach (var (hostId, hostStats) in stats.PerHostStats)
                {
                    var host = _networkManager.GetHostById(hostId);
                    Console.WriteLine($"- {host?.Hostname ?? hostId}: {hostStats.ActiveConnections} connections, {hostStats.Status}");
                }
                Console.WriteLine();
            }
        }

        public async Task CreateHostAsync(string hostname, string vendor, string subnet)
        {
            try
            {
                Console.WriteLine($"Creating host '{hostname}' with vendor '{vendor}' in subnet '{subnet}'...");

                var host = await _networkManager.CreateHostAsync(hostname, vendor, subnet);

                Console.WriteLine($"✓ Created host {host.Hostname} ({host.Id})");
                Console.WriteLine($"  IP Address: {host.IpAddress}");
                Console.WriteLine($"  Status: {host.Status}");
                Console.WriteLine($"  Created: {host.CreatedAt}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to create host: {ex.Message}");
                _logger.LogError(ex, "Failed to create host {Hostname}", hostname);
            }
        }

        public async Task StartNetworkAsync()
        {
            try
            {
                Console.WriteLine("Starting virtual network...");

                var success = await _networkManager.StartNetworkAsync();

                if (success)
                {
                    Console.WriteLine("✓ Virtual network started successfully");
                    await ShowStatusAsync();
                }
                else
                {
                    Console.WriteLine("✗ Failed to start virtual network");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error starting network: {ex.Message}");
                _logger.LogError(ex, "Failed to start network");
            }
        }

        public async Task StopNetworkAsync()
        {
            try
            {
                Console.WriteLine("Stopping virtual network...");

                var success = await _networkManager.StopNetworkAsync();

                if (success)
                {
                    Console.WriteLine("✓ Virtual network stopped successfully");
                }
                else
                {
                    Console.WriteLine("✗ Failed to stop virtual network");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error stopping network: {ex.Message}");
                _logger.LogError(ex, "Failed to stop network");
            }
        }

        public async Task ListHostsAsync()
        {
            var hosts = _networkManager.GetAllHosts();

            if (!hosts.Any())
            {
                Console.WriteLine("No hosts found. Use 'create host' to add hosts.");
                return;
            }

            Console.WriteLine("Virtual Hosts");
            Console.WriteLine("=============");
            Console.WriteLine($"{"Hostname",-20} {"IP Address",-15} {"Status",-10} {"Uptime",-10}");
            Console.WriteLine(new string('-', 60));

            foreach (var host in hosts)
            {
                var stats = host.GetStatistics();
                var uptime = DateTime.UtcNow - host.CreatedAt;

                Console.WriteLine($"{host.Hostname,-20} {host.IpAddress,-15} {host.Status,-10} {uptime:hh\\:mm\\:ss}");
            }
            Console.WriteLine();
        }

        public async Task ConnectToHostAsync(string hostname)
        {
            var host = _networkManager.GetAllHosts()
                .FirstOrDefault(h => h.Hostname.Equals(hostname, StringComparison.OrdinalIgnoreCase));

            if (host == null)
            {
                Console.WriteLine($"Host '{hostname}' not found");
                return;
            }

            Console.WriteLine($"Connecting to host {hostname} ({host.IpAddress})...");
            Console.WriteLine("Note: Use actual telnet client to connect:");
            Console.WriteLine($"  telnet {host.IpAddress} 23");
            Console.WriteLine($"  snmpwalk -v 2c -c public {host.IpAddress}");
            Console.WriteLine();
        }

        public async Task ShowHelpAsync()
        {
            Console.WriteLine("YANETS Virtual Host Console Commands");
            Console.WriteLine("===================================");
            Console.WriteLine();
            Console.WriteLine("Network Management:");
            Console.WriteLine("  create subnet <cidr> [--name <name>]    Create a virtual subnet");
            Console.WriteLine("  create host <name> [--vendor <vendor>]  Create a virtual host");
            Console.WriteLine("  start network                           Start the virtual network");
            Console.WriteLine("  stop network                            Stop the virtual network");
            Console.WriteLine("  status                                  Show network status");
            Console.WriteLine("  list hosts                              List all virtual hosts");
            Console.WriteLine();
            Console.WriteLine("Host Management:");
            Console.WriteLine("  connect <hostname>                      Show connection info for host");
            Console.WriteLine("  remove <hostname>                       Remove a virtual host");
            Console.WriteLine("  save <filename>                         Save network configuration");
            Console.WriteLine("  load <filename>                         Load network configuration");
            Console.WriteLine();
            Console.WriteLine("General:");
            Console.WriteLine("  help                                    Show this help");
            Console.WriteLine("  exit                                    Exit the application");
            Console.WriteLine();
        }

        private void OnNetworkEvent(object sender, NetworkEventArgs e)
        {
            Console.WriteLine($"[EVENT] {e.EventType}: {e.Message}");

            if (e.EventType == NetworkEventType.HostCreated)
            {
                Console.WriteLine($"       Host ID: {e.HostId}");
            }
        }

        private async Task CleanupAsync()
        {
            try
            {
                if (_networkManager.GetNetworkStatistics().ActiveHosts > 0)
                {
                    await _networkManager.StopNetworkAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup");
            }
        }

        public void Shutdown()
        {
            _isRunning = false;
        }
    }
}