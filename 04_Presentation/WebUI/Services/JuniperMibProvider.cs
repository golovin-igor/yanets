using Yanets.Core.Interfaces;
using Yanets.Core.Snmp;
using Yanets.SharedKernel;

namespace Yanets.WebUI.Services
{
    public class JuniperMibProvider : IMibProvider
    {
        private readonly Dictionary<string, MibDefinition> _mibs = new();

        public JuniperMibProvider()
        {
            // Juniper-specific MIBs
            _mibs["1.3.6.1.4.1.2636.3.1.1.0"] = new MibDefinition
            {
                Oid = "1.3.6.1.4.1.2636.3.1.1.0",
                Name = "jnxOperatingState",
                DataType = SnmpDataType.Integer,
                Access = AccessMode.ReadOnly
            };

            _mibs["1.3.6.1.4.1.2636.3.1.2.0"] = new MibDefinition
            {
                Oid = "1.3.6.1.4.1.2636.3.1.2.0",
                Name = "jnxOperatingTemp",
                DataType = SnmpDataType.Integer,
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
