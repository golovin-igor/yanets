using Yanets.Core.Interfaces;
using Yanets.Core.Snmp;
using Yanets.SharedKernel;

namespace Yanets.WebUI.Services
{
    public class CiscoMibProvider : IMibProvider
    {
        private readonly Dictionary<string, MibDefinition> _mibs = new();

        public CiscoMibProvider()
        {
            // Cisco-specific MIBs
            _mibs["1.3.6.1.4.1.9.2.1.1.0"] = new MibDefinition
            {
                Oid = "1.3.6.1.4.1.9.2.1.1.0",
                Name = "ciscoSystemVersion",
                DataType = SnmpDataType.OctetString,
                Access = AccessMode.ReadOnly
            };

            _mibs["1.3.6.1.4.1.9.9.13.1.1.1.0"] = new MibDefinition
            {
                Oid = "1.3.6.1.4.1.9.9.13.1.1.1.0",
                Name = "ciscoPingCount",
                DataType = SnmpDataType.Counter32,
                Access = AccessMode.ReadOnly
            };
        }

        public Dictionary<string, MibDefinition> GetSupportedMibs()
        {
            return _mibs;
        }

        public OidHandler? GetOidHandler(string oid)
        {
            return null; // Not implemented for this demo
        }

        public bool SupportsOid(string oid)
        {
            return _mibs.ContainsKey(oid);
        }
    }
}
