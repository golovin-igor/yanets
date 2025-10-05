using System;
using Yanets.Core.Commands;
using Yanets.Core.Models;
using Yanets.Application.Services.Vendors;

namespace Yanets.Application.Services
{
    /// <summary>
    /// Simple test class to verify routing commands functionality
    /// </summary>
    public static class RoutingCommandsTest
    {
        public static void TestRoutingCommands()
        {
            Console.WriteLine("Testing YANETS Routing Commands");
            Console.WriteLine("=================================");

            // Create a Cisco IOS vendor profile
            var ciscoProfile = new CiscoIosVendorProfile();

            // Test BGP commands
            Console.WriteLine("\n1. Testing Cisco BGP Commands:");
            TestCiscoBgpCommands(ciscoProfile);

            // Test OSPF commands
            Console.WriteLine("\n2. Testing Cisco OSPF Commands:");
            TestCiscoOspfCommands(ciscoProfile);

            // Test other routing commands
            Console.WriteLine("\n3. Testing Cisco Other Routing Commands:");
            TestCiscoOtherRoutingCommands(ciscoProfile);

            Console.WriteLine("\nAll tests completed!");
        }

        private static void TestCiscoBgpCommands(CiscoIosVendorProfile profile)
        {
            var commands = new[]
            {
                "router bgp 65001",
                "show ip bgp",
                "show bgp summary",
                "neighbor 192.168.2.2 remote-as 65002",
                "network 10.0.0.0 mask 255.255.255.0",
                "redistribute ospf 1"
            };

            foreach (var command in commands)
            {
                var commandDef = profile.GetCommand(command);
                if (commandDef != null)
                {
                    Console.WriteLine($"✓ Command '{command}' - Handler: {commandDef.Handler.Method.Name}");
                }
                else
                {
                    Console.WriteLine($"✗ Command '{command}' - Not found");
                }
            }
        }

        private static void TestCiscoOspfCommands(CiscoIosVendorProfile profile)
        {
            var commands = new[]
            {
                "router ospf 1",
                "show ip ospf",
                "area 0 range 10.0.0.0 255.255.255.0"
            };

            foreach (var command in commands)
            {
                var commandDef = profile.GetCommand(command);
                if (commandDef != null)
                {
                    Console.WriteLine($"✓ Command '{command}' - Handler: {commandDef.Handler.Method.Name}");
                }
                else
                {
                    Console.WriteLine($"✗ Command '{command}' - Not found");
                }
            }
        }

        private static void TestCiscoOtherRoutingCommands(CiscoIosVendorProfile profile)
        {
            var commands = new[]
            {
                "router eigrp 100",
                "show ip eigrp",
                "router rip",
                "ip route 192.168.3.0 255.255.255.0 192.168.2.1",
                "show ip protocols",
                "show ip route"
            };

            foreach (var command in commands)
            {
                var commandDef = profile.GetCommand(command);
                if (commandDef != null)
                {
                    Console.WriteLine($"✓ Command '{command}' - Handler: {commandDef.Handler.Method.Name}");
                }
                else
                {
                    Console.WriteLine($"✗ Command '{command}' - Not found");
                }
            }
        }
    }
}
