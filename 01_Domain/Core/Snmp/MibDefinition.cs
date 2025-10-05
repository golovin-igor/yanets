using Yanets.SharedKernel;

namespace Yanets.Core.Snmp
{
    /// <summary>
    /// Represents an SNMP MIB object definition
    /// </summary>
    public class MibDefinition
    {
        public string Oid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public SnmpDataType DataType { get; set; }
        public AccessMode Access { get; set; }
        public string Description { get; set; } = string.Empty;
        public int MaxAccess { get; set; }
        public string Status { get; set; } = "current";
        public Dictionary<string, string> Indexes { get; set; } = new();

        /// <summary>
        /// Validates the MIB definition
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Oid) &&
                   !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Description) &&
                   IsValidOid(Oid);
        }

        /// <summary>
        /// Validates OID format (basic validation)
        /// </summary>
        private static bool IsValidOid(string oid)
        {
            if (string.IsNullOrWhiteSpace(oid))
                return false;

            var parts = oid.Split('.');
            if (parts.Length < 2)
                return false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out var value) || value < 0 || value > int.MaxValue)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the parent OID (removes last component)
        /// </summary>
        public string GetParentOid()
        {
            if (string.IsNullOrEmpty(Oid))
                return string.Empty;

            var parts = Oid.Split('.');
            if (parts.Length <= 1)
                return string.Empty;

            return string.Join(".", parts.Take(parts.Length - 1));
        }

        /// <summary>
        /// Checks if this OID is a child of the specified parent OID
        /// </summary>
        public bool IsChildOf(string parentOid)
        {
            return Oid.StartsWith(parentOid + ".", StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the last component of the OID (the specific identifier)
        /// </summary>
        public string GetLastComponent()
        {
            if (string.IsNullOrEmpty(Oid))
                return string.Empty;

            var parts = Oid.Split('.');
            return parts.Length > 0 ? parts[parts.Length - 1] : string.Empty;
        }

        /// <summary>
        /// Returns a string representation of the MIB definition
        /// </summary>
        public override string ToString()
        {
            return $"{Oid} ({Name}) - {DataType} - {Access}";
        }
    }
}
