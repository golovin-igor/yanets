using System.CommandLine;
using Yanets.VirtualHostConsole.Console;

namespace Yanets.VirtualHostConsole.Console
{
    public static class CommandLineInterface
    {
        public static RootCommand CreateRootCommand(VirtualHostConsoleApp app)
        {
            var rootCommand = new RootCommand("YANETS Virtual Host Console - Network device simulation");

            // Create subnet command
            var createSubnetCommand = new Command("create-subnet", "Create a virtual subnet")
            {
                new Argument<string>("cidr", "Subnet CIDR (e.g., 192.168.1.0/24)"),
                new Option<string>(["--name", "-n"], "Subnet name"),
                new Option<string>(["--gateway", "-g"], "Gateway IP address")
            };

            createSubnetCommand.SetHandler(async (cidr, name, gateway) =>
            {
                var subnetName = string.IsNullOrEmpty(name) ? "default" : name;
                Console.WriteLine($"Creating subnet '{subnetName}' with CIDR {cidr}...");
                // TODO: Implement subnet creation through app
                await Task.CompletedTask();
            }, createSubnetCommand.Arguments[0], createSubnetCommand.Options[0], createSubnetCommand.Options[1]);

            // Create host command
            var createHostCommand = new Command("create-host", "Create a virtual host")
            {
                new Argument<string>("hostname", "Host name"),
                new Option<string>(["--vendor", "-v"], () => "cisco", "Vendor type"),
                new Option<string>(["--subnet", "-s"], () => "default", "Subnet name")
            };

            createHostCommand.SetHandler(async (hostname, vendor, subnet) =>
            {
                await app.CreateHostAsync(hostname, vendor, subnet);
            }, createHostCommand.Arguments[0], createHostCommand.Options[0], createHostCommand.Options[1]);

            // Start network command
            var startCommand = new Command("start", "Start the virtual network");
            startCommand.SetHandler(async () =>
            {
                await app.StartNetworkAsync();
            });

            // Stop network command
            var stopCommand = new Command("stop", "Stop the virtual network");
            stopCommand.SetHandler(async () =>
            {
                await app.StopNetworkAsync();
            });

            // Status command
            var statusCommand = new Command("status", "Show network status");
            statusCommand.SetHandler(async () =>
            {
                await app.ShowStatusAsync();
            });

            // List hosts command
            var listHostsCommand = new Command("list-hosts", "List all virtual hosts");
            listHostsCommand.SetHandler(async () =>
            {
                await app.ListHostsAsync();
            });

            // Connect command
            var connectCommand = new Command("connect", "Show connection information for a host")
            {
                new Argument<string>("hostname", "Host name")
            };

            connectCommand.SetHandler(async (hostname) =>
            {
                await app.ConnectToHostAsync(hostname);
            }, connectCommand.Arguments[0]);

            // Add commands to root
            rootCommand.AddCommand(createSubnetCommand);
            rootCommand.AddCommand(createHostCommand);
            rootCommand.AddCommand(startCommand);
            rootCommand.AddCommand(stopCommand);
            rootCommand.AddCommand(statusCommand);
            rootCommand.AddCommand(listHostsCommand);
            rootCommand.AddCommand(connectCommand);

            return rootCommand;
        }
    }
}