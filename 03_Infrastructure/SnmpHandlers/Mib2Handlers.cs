using Yanets.Core.Models;
using Yanets.Core.Snmp;
using Yanets.SharedKernel;

namespace Yanets.Infrastructure.SnmpHandlers
{
    /// <summary>
    /// SNMP handlers for standard MIB-II OIDs
    /// </summary>
    public static class Mib2Handlers
    {
        // System group (1.3.6.1.2.1.1)
        public static SnmpValue SysDescrHandler(DeviceState state)
        {
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null)
                return SnmpValue.OctetString("Unknown device");

            return SnmpValue.OctetString(
                $"{device.Vendor.VendorName} {device.Type} " +
                $"running {device.Vendor.Os} {device.Vendor.Version}"
            );
        }

        public static SnmpValue SysObjectIdHandler(DeviceState state)
        {
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null)
                return SnmpValue.ObjectIdentifier("1.3.6.1.4.1.9.1.1208"); // Default Cisco

            return SnmpValue.ObjectIdentifier(device.Vendor.SysObjectId);
        }

        public static SnmpValue SysUptimeHandler(DeviceState state)
        {
            var uptime = DateTime.Now - state.Uptime;
            var timeticks = (uint)(uptime.TotalMilliseconds / 10);
            return SnmpValue.TimeTicks(timeticks);
        }

        public static SnmpValue SysNameHandler(DeviceState state)
        {
            var device = state.Variables["device"] as NetworkDevice;
            return SnmpValue.OctetString(device?.Hostname ?? "unknown");
        }

        public static SnmpValue SysLocationHandler(DeviceState state)
        {
            return SnmpValue.OctetString(state.Variables.GetValueOrDefault("sysLocation") as string ?? "");
        }

        public static SnmpValue SysContactHandler(DeviceState state)
        {
            return SnmpValue.OctetString(state.Variables.GetValueOrDefault("sysContact") as string ?? "");
        }

        // Interfaces group (1.3.6.1.2.1.2)
        public static SnmpValue IfNumberHandler(DeviceState state)
        {
            var device = state.Variables["device"] as NetworkDevice;
            return SnmpValue.Integer(device?.Interfaces.Count ?? 0);
        }

        public static SnmpValue IfDescrHandler(DeviceState state, string index)
        {
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null || !int.TryParse(index, out var ifIndex))
                return SnmpValue.OctetString("");

            if (ifIndex < 1 || ifIndex > device.Interfaces.Count)
                return SnmpValue.OctetString("");

            var iface = device.Interfaces[ifIndex - 1];
            return SnmpValue.OctetString(iface.Name);
        }

        public static SnmpValue IfTypeHandler(DeviceState state, string index)
        {
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null || !int.TryParse(index, out var ifIndex))
                return SnmpValue.Integer(1); // Other

            if (ifIndex < 1 || ifIndex > device.Interfaces.Count)
                return SnmpValue.Integer(1); // Other

            var iface = device.Interfaces[ifIndex - 1];
            return SnmpValue.Integer((int)iface.Type);
        }

        public static SnmpValue IfMtuHandler(DeviceState state, string index)
        {
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null || !int.TryParse(index, out var ifIndex))
                return SnmpValue.Integer(1500);

            if (ifIndex < 1 || ifIndex > device.Interfaces.Count)
                return SnmpValue.Integer(1500);

            var iface = device.Interfaces[ifIndex - 1];
            return SnmpValue.Integer(iface.Mtu);
        }

        public static SnmpValue IfSpeedHandler(DeviceState state, string index)
        {
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null || !int.TryParse(index, out var ifIndex))
                return SnmpValue.Gauge32(0);

            if (ifIndex < 1 || ifIndex > device.Interfaces.Count)
                return SnmpValue.Gauge32(0);

            var iface = device.Interfaces[ifIndex - 1];
            return SnmpValue.Gauge32((uint)iface.Speed * 1000000); // Convert Mbps to bps
        }

        public static SnmpValue IfPhysAddressHandler(DeviceState state, string index)
        {
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null || !int.TryParse(index, out var ifIndex))
                return SnmpValue.OctetString("");

            if (ifIndex < 1 || ifIndex > device.Interfaces.Count)
                return SnmpValue.OctetString("");

            var iface = device.Interfaces[ifIndex - 1];
            if (string.IsNullOrEmpty(iface.MacAddress))
                return SnmpValue.OctetString("");

            // Convert MAC address from XX:XX:XX:XX:XX:XX to byte array
            var macBytes = iface.MacAddress.Split(':')
                .Select(hex => Convert.ToByte(hex, 16))
                .ToArray();

            return SnmpValue.OctetString(string.Join("", macBytes.Select(b => b.ToString("X2"))));
        }

        public static SnmpValue IfOperStatusHandler(DeviceState state, string index)
        {
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null || !int.TryParse(index, out var ifIndex))
                return SnmpValue.Integer(2); // Down

            if (ifIndex < 1 || ifIndex > device.Interfaces.Count)
                return SnmpValue.Integer(2); // Down

            var iface = device.Interfaces[ifIndex - 1];
            return SnmpValue.Integer(iface.IsUp ? 1 : 2); // 1=up, 2=down
        }

        public static SnmpValue IfInOctetsHandler(DeviceState state, string index)
        {
            // For demo, return a counter value
            return SnmpValue.Counter32(1234567890);
        }

        public static SnmpValue IfOutOctetsHandler(DeviceState state, string index)
        {
            // For demo, return a counter value
            return SnmpValue.Counter32(987654321);
        }

        // IP group (1.3.6.1.2.1.4)
        public static SnmpValue IpForwardingHandler(DeviceState state)
        {
            // 1 = forwarding (router), 2 = not-forwarding (host/switch)
            var device = state.Variables["device"] as NetworkDevice;
            return SnmpValue.Integer(device?.Type == DeviceType.Router ? 1 : 2);
        }

        public static SnmpValue IpAddrTableHandler(DeviceState state, string index)
        {
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null)
                return SnmpValue.IpAddress("0.0.0.0");

            var iface = device.Interfaces
                .FirstOrDefault(i => !string.IsNullOrEmpty(i.IpAddress));

            return SnmpValue.IpAddress(iface?.IpAddress ?? "0.0.0.0");
        }

        public static SnmpValue IpNetToMediaTableHandler(DeviceState state, string index)
        {
            // For demo, return a MAC address for the first interface
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null)
                return SnmpValue.OctetString("");

            var iface = device.Interfaces.FirstOrDefault();
            if (iface == null || string.IsNullOrEmpty(iface.MacAddress))
                return SnmpValue.OctetString("");

            var macBytes = iface.MacAddress.Split(':')
                .Select(hex => Convert.ToByte(hex, 16))
                .ToArray();

            return SnmpValue.OctetString(string.Join("", macBytes.Select(b => b.ToString("X2"))));
        }

        // TCP group (1.3.6.1.2.1.6)
        public static SnmpValue TcpConnStateHandler(DeviceState state, string index)
        {
            // For demo, return established state (1)
            return SnmpValue.Integer(5); // Established
        }

        public static SnmpValue TcpInSegsHandler(DeviceState state)
        {
            return SnmpValue.Counter32(1234567);
        }

        public static SnmpValue TcpOutSegsHandler(DeviceState state)
        {
            return SnmpValue.Counter32(7654321);
        }

        // UDP group (1.3.6.1.2.1.7)
        public static SnmpValue UdpInDatagramsHandler(DeviceState state)
        {
            return SnmpValue.Counter32(654321);
        }

        public static SnmpValue UdpOutDatagramsHandler(DeviceState state)
        {
            return SnmpValue.Counter32(321654);
        }

        // SNMP group (1.3.6.1.2.1.11)
        public static SnmpValue SnmpInPktsHandler(DeviceState state)
        {
            return SnmpValue.Counter32(100);
        }

        public static SnmpValue SnmpOutPktsHandler(DeviceState state)
        {
            return SnmpValue.Counter32(95);
        }

        public static SnmpValue SnmpInGetRequestsHandler(DeviceState state)
        {
            return SnmpValue.Counter32(50);
        }

        public static SnmpValue SnmpInSetRequestsHandler(DeviceState state)
        {
            return SnmpValue.Counter32(5);
        }
    }
}
