using Yanets.Core.Interfaces;
using Yanets.Core.Snmp;
using Yanets.SharedKernel;

namespace Yanets.WebUI.Services
{
    public class MibProvider : IMibProvider
    {
        private readonly Dictionary<string, MibDefinition> _mibs = new();

        public MibProvider()
        {
            // Add basic MIB-II definitions
            _mibs["1.3.6.1.2.1.1.1.0"] = new MibDefinition
            {
                Oid = "1.3.6.1.2.1.1.1.0",
                Name = "sysDescr",
                DataType = SnmpDataType.OctetString,
                Access = AccessMode.ReadOnly
            };

            _mibs["1.3.6.1.2.1.1.3.0"] = new MibDefinition
            {
                Oid = "1.3.6.1.2.1.1.3.0",
                Name = "sysUpTime",
                DataType = SnmpDataType.TimeTicks,
                Access = AccessMode.ReadOnly
            };
        }

        public Dictionary<string, MibDefinition> GetSupportedMibs()
        {
            return _mibs;
        }

        public OidHandler? GetOidHandler(string oid)
        {
            // Basic implementation - return null for now
            return null;
        }

        public bool SupportsOid(string oid)
        {
            return _mibs.ContainsKey(oid);
        }
    }
}
