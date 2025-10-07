using System;
using System.Collections.Generic;
using Yanets.Core.Commands;
using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.Application.Services.Vendors;
using Yanets.SharedKernel;

namespace Yanets.Application.Services
{
    /// <summary>
    /// Test implementation of NetworkDevice for testing purposes
    /// </summary>
    public class TestDevice : NetworkDevice
    {
        public TestDevice(DeviceType deviceType, string vendorName) : base(deviceType, vendorName) { }

        public override bool IsValid() => true;
    }

    /// <summary>
    /// Test class to demonstrate connectivity and routing functionality
    /// </summary>
    public static class ConnectivityTest
    {
        public static void TestConnectivityFeatures()
        {
            Console.WriteLine("YANETS Connectivity and Routing Test");
            Console.WriteLine("====================================");

            // Create a simple network topology for testing
            var topology = CreateTestTopology();

            // Test interface connectivity tracking
            Console.WriteLine("\n1. Testing Interface Connectivity:");
            TestInterfaceConnectivity(topology);

            // Test ping functionality
            Console.WriteLine("\n2. Testing Ping Functionality:");
            TestPingFunctionality(topology);

            // Test route recalculation
            Console.WriteLine("\n3. Testing Route Recalculation:");
            TestRouteRecalculation(topology);

            Console.WriteLine("\nAll connectivity tests completed!");
        }

        private static NetworkTopology CreateTestTopology()
        {
            var topology = new NetworkTopology
            {
                Name = "Test Topology",
                Description = "Simple test topology for connectivity testing"
            };

            // Create Router1
            var router1 = new TestDevice(DeviceType.Router, "Cisco")
            {
                Name = "Router1",
                Hostname = "Router1"
            };
            router1.AddInterface(new NetworkInterface
            {
                Name = "GigabitEthernet0/0",
                IpAddress = "192.168.1.1",
                SubnetMask = "255.255.255.0",
                IsUp = true,
                Status = InterfaceStatus.Up
            });
            router1.AddInterface(new NetworkInterface
            {
                Name = "GigabitEthernet0/1",
                IpAddress = "192.168.2.1",
                SubnetMask = "255.255.255.0",
                IsUp = true,
                Status = InterfaceStatus.Up
            });

            // Create Router2
            var router2 = new TestDevice(DeviceType.Router, "Cisco")
            {
                Name = "Router2",
                Hostname = "Router2"
            };
            router2.AddInterface(new NetworkInterface
            {
                Name = "GigabitEthernet0/0",
                IpAddress = "192.168.2.2",
                SubnetMask = "255.255.255.0",
                IsUp = true,
                Status = InterfaceStatus.Up
            });
            router2.AddInterface(new NetworkInterface
            {
                Name = "GigabitEthernet0/1",
                IpAddress = "192.168.3.1",
                SubnetMask = "255.255.255.0",
                IsUp = true,
                Status = InterfaceStatus.Up
            });

            topology.AddDevice(router1);
            topology.AddDevice(router2);

            // Add static routes
            router1.State.AddRoute(new RoutingTableEntry
            {
                Destination = "192.168.3.0/24",
                Gateway = "192.168.2.2",
                Interface = "GigabitEthernet0/1",
                Metric = 1,
                Protocol = "Static"
            });

            router2.State.AddRoute(new RoutingTableEntry
            {
                Destination = "192.168.1.0/24",
                Gateway = "192.168.2.1",
                Interface = "GigabitEthernet0/0",
                Metric = 1,
                Protocol = "Static"
            });

            return topology;
        }

        private static void TestInterfaceConnectivity(NetworkTopology topology)
        {
            var router1 = topology.GetDeviceByHostname("Router1");
            if (router1 != null)
            {
                Console.WriteLine($"Router1 GigabitEthernet0/0 connectivity: {router1.State.IsInterfaceReachable("GigabitEthernet0/0")}");
                Console.WriteLine($"Router1 GigabitEthernet0/1 connectivity: {router1.State.IsInterfaceReachable("GigabitEthernet0/1")}");

                // Test shutting down an interface
                router1.State.UpdateInterfaceConnectivity("GigabitEthernet0/0", false);
                Console.WriteLine($"After shutdown - Router1 GigabitEthernet0/0 connectivity: {router1.State.IsInterfaceReachable("GigabitEthernet0/0")}");
                Console.WriteLine($"Routes after shutdown: {router1.State.RoutingTable.Count}");
            }
        }

        private static void TestPingFunctionality(NetworkTopology topology)
        {
            var router1 = topology.GetDeviceByHostname("Router1");
            if (router1 != null)
            {
                // Test ping to reachable host
                var vendorProfile = new CiscoIosVendorProfile();
                var commandContext = new CommandContext
                {
                    Device = router1,
                    State = router1.State,
                    ParsedArguments = new Dictionary<string, string> { { "ip-address", "192.168.2.2" } },
                    Session = new CliSession(),
                    CurrentPrivilegeLevel = 1
                };

                var pingResult = vendorProfile.GetCommand("ping 192.168.2.2")?.Handler(commandContext);
                Console.WriteLine($"Ping to 192.168.2.2: {pingResult?.Output?.Split('\n')[0]}");

                // Test ping to unreachable host
                commandContext.ParsedArguments["ip-address"] = "192.168.3.2";
                pingResult = vendorProfile.GetCommand("ping 192.168.3.2")?.Handler(commandContext);
                Console.WriteLine($"Ping to 192.168.3.2: {pingResult?.Output?.Split('\n')[0]}");
            }
        }

        private static void TestRouteRecalculation(NetworkTopology topology)
        {
            var router1 = topology.GetDeviceByHostname("Router1");
            if (router1 != null)
            {
                Console.WriteLine($"Routes before interface shutdown: {router1.State.RoutingTable.Count}");

                // Shut down interface that routes depend on
                router1.State.UpdateInterfaceConnectivity("GigabitEthernet0/1", false);

                Console.WriteLine($"Routes after interface shutdown: {router1.State.RoutingTable.Count}");
                Console.WriteLine("Routes that remain:");
                foreach (var route in router1.State.RoutingTable)
                {
                    Console.WriteLine($"  {route.Destination} via {route.Interface}");
                }
            }
        }
    }
}
