using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Core.Snmp;
using Yanets.Core.Vendors;
using Yanets.SharedKernel;
using System.Linq;

namespace Yanets.Application.Services
{
    /// <summary>
    /// Implementation of MIB provider for the application layer
    /// </summary>
    public class MibProvider : IMibProvider
    {
        private readonly Dictionary<string, MibDefinition> _mibs;

        public MibProvider()
        {
            _mibs = new Dictionary<string, MibDefinition>();

            // Initialize with standard MIB-II definitions
            InitializeMib2Definitions();

            // Add vendor-specific MIBs
            InitializeVendorMibs();
        }

        public Dictionary<string, MibDefinition> GetSupportedMibs()
        {
            return _mibs;
        }

        public OidHandler? GetOidHandler(string oid)
        {
            // For demo purposes, return basic handlers
            return oid switch
            {
                "1.3.6.1.2.1.1.1.0" => OidHandler.ReadOnly(oid, state => SysDescrHandler(state)),
                "1.3.6.1.2.1.1.3.0" => OidHandler.ReadOnly(oid, state => SysUptimeHandler(state)),
                "1.3.6.1.2.1.1.5.0" => OidHandler.ReadOnly(oid, state => SysNameHandler(state)),
                "1.3.6.1.2.1.2.2.1.2" => OidHandler.Table("1.3.6.1.2.1.2.2.1.2", state => IfDescrHandler(state, "1"), state => GetInterfaceIndices(state)),
                _ => null
            };
        }

        public bool SupportsOid(string oid)
        {
            return _mibs.ContainsKey(oid) || oid.StartsWith("1.3.6.1.2.1.2.2.1"); // Interface table
        }

        private void InitializeMib2Definitions()
        {
            // System group (1.3.6.1.2.1.1)
            _mibs["1.3.6.1.2.1.1.1.0"] = new MibDefinition
            {
                Oid = "1.3.6.1.2.1.1.1.0",
                Name = "sysDescr",
                DataType = SharedKernel.SnmpDataType.OctetString,
                Access = SharedKernel.AccessMode.ReadOnly,
                Description = "System description"
            };

            _mibs["1.3.6.1.2.1.1.2.0"] = new MibDefinition
            {
                Oid = "1.3.6.1.2.1.1.2.0",
                Name = "sysObjectID",
                DataType = SharedKernel.SnmpDataType.ObjectIdentifier,
                Access = SharedKernel.AccessMode.ReadOnly,
                Description = "System object identifier"
            };

            _mibs["1.3.6.1.2.1.1.3.0"] = new MibDefinition
            {
                Oid = "1.3.6.1.2.1.1.3.0",
                Name = "sysUpTime",
                DataType = SharedKernel.SnmpDataType.TimeTicks,
                Access = SharedKernel.AccessMode.ReadOnly,
                Description = "System uptime"
            };

            _mibs["1.3.6.1.2.1.1.5.0"] = new MibDefinition
            {
                Oid = "1.3.6.1.2.1.1.5.0",
                Name = "sysName",
                DataType = SharedKernel.SnmpDataType.OctetString,
                Access = SharedKernel.AccessMode.ReadWrite,
                Description = "System name"
            };

            // Interface group (1.3.6.1.2.1.2)
            _mibs["1.3.6.1.2.1.2.1.0"] = new MibDefinition
            {
                Oid = "1.3.6.1.2.1.2.1.0",
                Name = "ifNumber",
                DataType = SharedKernel.SnmpDataType.Integer,
                Access = SharedKernel.AccessMode.ReadOnly,
                Description = "Number of interfaces"
            };
        }

        private void InitializeVendorMibs()
        {
            // Cisco-specific MIBs
            _mibs["1.3.6.1.4.1.9.2.1.1.0"] = new MibDefinition
            {
                Oid = "1.3.6.1.4.1.9.2.1.1.0",
                Name = "ciscoSystemVersion",
                DataType = SharedKernel.SnmpDataType.OctetString,
                Access = SharedKernel.AccessMode.ReadOnly,
                Description = "Cisco system version"
            };

            // Juniper-specific MIBs
            _mibs["1.3.6.1.4.1.2636.3.1.1.0"] = new MibDefinition
            {
                Oid = "1.3.6.1.4.1.2636.3.1.1.0",
                Name = "jnxOperatingState",
                DataType = SharedKernel.SnmpDataType.Integer,
                Access = SharedKernel.AccessMode.ReadOnly,
                Description = "Juniper operating state"
            };
        }

        private SnmpValue SysDescrHandler(DeviceState state)
        {
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null)
                return SnmpValue.OctetString("Unknown device");

            return SnmpValue.OctetString(
                $"{device.Vendor.VendorName} {device.Type} " +
                $"running {device.Vendor.Os} {device.Vendor.Version}"
            );
        }

        private SnmpValue SysUptimeHandler(DeviceState state)
        {
            var uptime = DateTime.Now - state.Uptime;
            var timeticks = (uint)(uptime.TotalMilliseconds / 10);
            return SnmpValue.TimeTicks(timeticks);
        }

        private SnmpValue SysNameHandler(DeviceState state)
        {
            var device = state.Variables["device"] as NetworkDevice;
            return SnmpValue.OctetString(device?.Hostname ?? "unknown");
        }

        private SnmpValue IfDescrHandler(DeviceState state, string index)
        {
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null || !int.TryParse(index, out var ifIndex))
                return SnmpValue.OctetString("");

            if (ifIndex < 1 || ifIndex > device.Interfaces.Count)
                return SnmpValue.OctetString("");

            var iface = device.Interfaces[ifIndex - 1];
            return SnmpValue.OctetString(iface.Name);
        }

        private List<string> GetInterfaceIndices(DeviceState state)
        {
            var device = state.Variables["device"] as NetworkDevice;
            if (device == null)
                return new List<string>();

            return Enumerable.Range(1, device.Interfaces.Count)
                .Select(i => i.ToString())
                .ToList();
        }
    }
}
