using Yanets.Core.Snmp;

namespace Yanets.Core.Interfaces
{
    /// <summary>
    /// Interface for SNMP MIB providers
    /// </summary>
    public interface IMibProvider
    {
        /// <summary>
        /// Gets all supported MIB definitions
        /// </summary>
        Dictionary<string, MibDefinition> GetSupportedMibs();

        /// <summary>
        /// Gets an OID handler for a specific OID
        /// </summary>
        OidHandler? GetOidHandler(string oid);

        /// <summary>
        /// Checks if an OID is supported
        /// </summary>
        bool SupportsOid(string oid);
    }
}
