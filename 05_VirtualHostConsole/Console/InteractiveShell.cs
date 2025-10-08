using Microsoft.Extensions.Logging;
using Yanets.VirtualHostConsole.Core.Interfaces;

namespace Yanets.VirtualHostConsole.Console
{
    public class InteractiveShell
    {
        private readonly IVirtualNetworkManager _networkManager;
        private readonly ILogger<InteractiveShell> _logger;
        private readonly Dictionary<string, Action<string[]>> _commands;

        public InteractiveShell(IVirtualNetworkManager networkManager, ILogger<InteractiveShell> logger)
        {
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _commands = new Dictionary<string, Action<string[]>>
            {
                ["help"] = args => ShowHelp(),
                ["status"] = args => ShowStatus(),
                ["list"] = args => ListHosts(),
                ["create"] = args => HandleCreate(args),
                ["start"] = args => StartNetwork(),
                ["stop"] = args => StopNetwork(),
                ["connect"] = args => ConnectToHost(args),
                ["clear"] = args => Console.Clear(),
                ["exit"] = args => Environment.Exit(0)
            };
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Interactive mode started. Type 'help' for commands.");
            Console.WriteLine();
        }

        public async Task ExecuteCommandAsync(string commandLine)
        {
            try
            {
                var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    return;

                var command = parts[0].ToLower();
                var args = parts.Skip(1).ToArray();

                if (_commands.TryGetValue(command, out var action))
                {
                    action(args);
                }
                else
                {
                    Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command: {CommandLine}", commandLine);
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("Available Commands:");
            Console.WriteLine("==================");
            Console.WriteLine("help                    - Show this help");
            Console.WriteLine("status                  - Show network status");
            Console.WriteLine("list                    - List all hosts");
            Console.WriteLine("create host <name>      - Create a new host");
            Console.WriteLine("create subnet <cidr>    - Create a new subnet");
            Console.WriteLine("start                   - Start the network");
            Console.WriteLine("stop                    - Stop the network");
            Console.WriteLine("connect <hostname>      - Show connection info");
            Console.WriteLine("clear                   - Clear the screen");
            Console.WriteLine("exit                    - Exit the application");
            Console.WriteLine();
        }

        private void ShowStatus()
        {
            var stats = _networkManager.GetNetworkStatistics();

            Console.WriteLine("Network Status:");
            Console.WriteLine($"Total Hosts: {stats.TotalHosts}");
            Console.WriteLine($"Active Hosts: {stats.ActiveHosts}");
            Console.WriteLine($"Total Connections: {stats.TotalConnections}");
            Console.WriteLine($"Last Updated: {stats.LastUpdated}");
            Console.WriteLine();
        }

        private void ListHosts()
        {
            var hosts = _networkManager.GetAllHosts();

            if (!hosts.Any())
            {
                Console.WriteLine("No hosts found.");
                return;
            }

            Console.WriteLine($"{"Hostname",-20} {"IP Address",-15} {"Status",-10}");
            Console.WriteLine(new string('-', 50));

            foreach (var host in hosts)
            {
                Console.WriteLine($"{host.Hostname,-20} {host.IpAddress,-15} {host.Status,-10}");
            }
            Console.WriteLine();
        }

        private void HandleCreate(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: create <host|subnet> <name>");
                return;
            }

            var type = args[0].ToLower();
            var name = args.Length > 1 ? args[1] : string.Empty;

            switch (type)
            {
                case "host":
                    if (string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine("Host name is required");
                        return;
                    }
                    Console.WriteLine($"Creating host '{name}'...");
                    // TODO: Implement host creation
                    break;

                case "subnet":
                    if (string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine("Subnet CIDR is required");
                        return;
                    }
                    Console.WriteLine($"Creating subnet '{name}'...");
                    // TODO: Implement subnet creation
                    break;

                default:
                    Console.WriteLine($"Unknown create type: {type}");
                    break;
            }
        }

        private void StartNetwork()
        {
            Console.WriteLine("Starting network...");
            // TODO: Implement network start
        }

        private void StopNetwork()
        {
            Console.WriteLine("Stopping network...");
            // TODO: Implement network stop
        }

        private void ConnectToHost(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: connect <hostname>");
                return;
            }

            var hostname = args[0];
            var host = _networkManager.GetAllHosts()
                .FirstOrDefault(h => h.Hostname.Equals(hostname, StringComparison.OrdinalIgnoreCase));

            if (host == null)
            {
                Console.WriteLine($"Host '{hostname}' not found");
                return;
            }

            Console.WriteLine($"Connection information for {hostname}:");
            Console.WriteLine($"IP Address: {host.IpAddress}");
            Console.WriteLine($"Telnet: {host.IpAddress}:23");
            Console.WriteLine($"SNMP: {host.IpAddress}:161");
            Console.WriteLine();
        }
    }
}