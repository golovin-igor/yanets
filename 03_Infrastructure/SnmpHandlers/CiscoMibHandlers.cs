using Yanets.Core.Models;
using Yanets.Core.Snmp;
using Yanets.SharedKernel;

namespace Yanets.Infrastructure.SnmpHandlers
{
    /// <summary>
    /// Cisco-specific SNMP MIB handlers
    /// </summary>
    public static class CiscoMibHandlers
    {
        // Cisco system MIB (1.3.6.1.4.1.9.2)
        public static SnmpValue CiscoSystemVersionHandler(DeviceState state)
        {
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null)
                return SnmpValue.OctetString("15.2(4)E8");

            return SnmpValue.OctetString(device.Vendor.Version);
        }

        public static SnmpValue CiscoCpuUtilizationHandler(DeviceState state)
        {
            var resources = state.Resources;
            return SnmpValue.Gauge32((uint)resources.CpuUtilization);
        }

        public static SnmpValue CiscoMemoryUsedHandler(DeviceState state)
        {
            var resources = state.Resources;
            return SnmpValue.Gauge32((uint)resources.MemoryUsed);
        }

        public static SnmpValue CiscoMemoryFreeHandler(DeviceState state)
        {
            var resources = state.Resources;
            return SnmpValue.Gauge32((uint)resources.MemoryFree);
        }

        // Cisco interface extensions (1.3.6.1.4.1.9.9.23)
        public static SnmpValue CiscoIfInOctetsHandler(DeviceState state, string index)
        {
            // For demo, return interface-specific counter
            return SnmpValue.Counter32(123456789 + int.Parse(index));
        }

        public static SnmpValue CiscoIfOutOctetsHandler(DeviceState state, string index)
        {
            // For demo, return interface-specific counter
            return SnmpValue.Counter32(987654321 + int.Parse(index));
        }

        public static SnmpValue CiscoIfInUcastPktsHandler(DeviceState state, string index)
        {
            return SnmpValue.Counter32(100000 + int.Parse(index) * 100);
        }

        public static SnmpValue CiscoIfOutUcastPktsHandler(DeviceState state, string index)
        {
            return SnmpValue.Counter32(90000 + int.Parse(index) * 100);
        }

        public static SnmpValue CiscoIfInDiscardsHandler(DeviceState state, string index)
        {
            return SnmpValue.Counter32(10 + int.Parse(index));
        }

        public static SnmpValue CiscoIfOutDiscardsHandler(DeviceState state, string index)
        {
            return SnmpValue.Counter32(5 + int.Parse(index));
        }

        public static SnmpValue CiscoIfInErrorsHandler(DeviceState state, string index)
        {
            return SnmpValue.Counter32(2 + int.Parse(index));
        }

        public static SnmpValue CiscoIfOutErrorsHandler(DeviceState state, string index)
        {
            return SnmpValue.Counter32(1 + int.Parse(index));
        }

        // Cisco VLAN MIB (1.3.6.1.4.1.9.9.46)
        public static SnmpValue CiscoVlanNameHandler(DeviceState state, string index)
        {
            var vlanId = int.Parse(index);
            var vlan = state.Vlans.FirstOrDefault(v => v.Id == vlanId);

            if (vlan == null)
                return SnmpValue.OctetString($"VLAN{vlanId}");

            return SnmpValue.OctetString(vlan.Name);
        }

        public static SnmpValue CiscoVlanStateHandler(DeviceState state, string index)
        {
            var vlanId = int.Parse(index);
            var vlan = state.Vlans.FirstOrDefault(v => v.Id == vlanId);

            if (vlan == null)
                return SnmpValue.Integer(1); // Operational

            return SnmpValue.Integer(vlan.IsActive ? 1 : 2); // 1=operational, 2=suspended
        }

        public static SnmpValue CiscoVlanPortsHandler(DeviceState state, string index)
        {
            var vlanId = int.Parse(index);
            var vlan = state.Vlans.FirstOrDefault(v => v.Id == vlanId);

            if (vlan == null)
                return SnmpValue.OctetString("");

            // Convert port list to SNMP octet string format
            var portBytes = vlan.Ports.Select(p => (byte)p).ToArray();
            return SnmpValue.OctetString(string.Join("", portBytes.Select(b => b.ToString("X2"))));
        }

        // Cisco CDP (Cisco Discovery Protocol) MIB (1.3.6.1.4.1.9.9.23)
        public static SnmpValue CiscoCdpCacheDeviceIdHandler(DeviceState state, string index)
        {
            // For demo, return a neighbor device ID
            return SnmpValue.OctetString($"Neighbor-{index}");
        }

        public static SnmpValue CiscoCdpCacheDevicePortHandler(DeviceState state, string index)
        {
            // For demo, return a neighbor port
            return SnmpValue.OctetString($"GigabitEthernet0/{index}");
        }

        public static SnmpValue CiscoCdpCachePlatformHandler(DeviceState state, string index)
        {
            // For demo, return a platform type
            return SnmpValue.OctetString("cisco WS-C2960-24TT-L");
        }

        // Cisco IP MIB (1.3.6.1.4.1.9.9.13)
        public static SnmpValue CiscoIpRouteCountHandler(DeviceState state)
        {
            return SnmpValue.Gauge32((uint)state.RoutingTable.Count);
        }

        public static SnmpValue CiscoIpRouteDestHandler(DeviceState state, string index)
        {
            var routeIndex = int.Parse(index) - 1;
            if (routeIndex < 0 || routeIndex >= state.RoutingTable.Count)
                return SnmpValue.IpAddress("0.0.0.0");

            var route = state.RoutingTable[routeIndex];
            return SnmpValue.IpAddress(route.Destination);
        }

        public static SnmpValue CiscoIpRouteNextHopHandler(DeviceState state, string index)
        {
            var routeIndex = int.Parse(index) - 1;
            if (routeIndex < 0 || routeIndex >= state.RoutingTable.Count)
                return SnmpValue.IpAddress("0.0.0.0");

            var route = state.RoutingTable[routeIndex];
            return SnmpValue.IpAddress(route.Gateway);
        }

        public static SnmpValue CiscoIpRouteMaskHandler(DeviceState state, string index)
        {
            var routeIndex = int.Parse(index) - 1;
            if (routeIndex < 0 || routeIndex >= state.RoutingTable.Count)
                return SnmpValue.IpAddress("255.255.255.255");

            var route = state.RoutingTable[routeIndex];
            return SnmpValue.IpAddress(GetSubnetMaskFromDestination(route.Destination));
        }

        public static SnmpValue CiscoIpRouteMetric1Handler(DeviceState state, string index)
        {
            var routeIndex = int.Parse(index) - 1;
            if (routeIndex < 0 || routeIndex >= state.RoutingTable.Count)
                return SnmpValue.Integer(0);

            var route = state.RoutingTable[routeIndex];
            return SnmpValue.Integer(route.Metric);
        }

        public static SnmpValue CiscoIpRouteProtoHandler(DeviceState state, string index)
        {
            var routeIndex = int.Parse(index) - 1;
            if (routeIndex < 0 || routeIndex >= state.RoutingTable.Count)
                return SnmpValue.Integer(1); // Other

            var route = state.RoutingTable[routeIndex];
            return SnmpValue.Integer(route.Protocol switch
            {
                "connected" => 2,  // Local
                "static" => 3,     // Static
                "rip" => 11,       // RIP
                "ospf" => 13,      // OSPF
                "bgp" => 14,       // BGP
                _ => 1             // Other
            });
        }

        private static string GetSubnetMaskFromDestination(string destination)
        {
            // Simplified subnet mask calculation
            if (destination.Contains("/"))
            {
                var parts = destination.Split('/');
                if (parts.Length == 2 && int.TryParse(parts[1], out var bits))
                {
                    return SubnetMaskFromBits(bits);
                }
            }

            return "255.255.255.255";
        }

        private static string SubnetMaskFromBits(int bits)
        {
            var mask = 0xFFFFFFFF << (32 - bits);
            var bytes = new[]
            {
                (byte)((mask >> 24) & 0xFF),
                (byte)((mask >> 16) & 0xFF),
                (byte)((mask >> 8) & 0xFF),
                (byte)(mask & 0xFF)
            };

            return string.Join(".", bytes);
        }
    }
}
