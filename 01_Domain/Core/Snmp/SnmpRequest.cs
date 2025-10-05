using Yanets.SharedKernel;

namespace Yanets.Core.Snmp
{
    /// <summary>
    /// Represents an SNMP request
    /// </summary>
    public class SnmpRequest
    {
        public int RequestId { get; set; }
        public SnmpRequestType Type { get; set; }
        public string Community { get; set; } = string.Empty;
        public SnmpVersion Version { get; set; } = SnmpVersion.V2c;
        public List<string> Oids { get; set; } = new();
        public List<SnmpVarBind> VarBinds { get; set; } = new();

        /// <summary>
        /// Validates the SNMP request
        /// </summary>
        public bool IsValid()
        {
            return RequestId >= 0 &&
                   !string.IsNullOrWhiteSpace(Community) &&
                   Oids.Count > 0;
        }

        /// <summary>
        /// Creates a GET request
        /// </summary>
        public static SnmpRequest Get(int requestId, string community, params string[] oids)
        {
            return new SnmpRequest
            {
                RequestId = requestId,
                Type = SnmpRequestType.Get,
                Community = community,
                Oids = oids.ToList()
            };
        }

        /// <summary>
        /// Creates a GETNEXT request
        /// </summary>
        public static SnmpRequest GetNext(int requestId, string community, params string[] oids)
        {
            return new SnmpRequest
            {
                RequestId = requestId,
                Type = SnmpRequestType.GetNext,
                Community = community,
                Oids = oids.ToList()
            };
        }

        /// <summary>
        /// Creates a SET request
        /// </summary>
        public static SnmpRequest Set(int requestId, string community, params SnmpVarBind[] varBinds)
        {
            return new SnmpRequest
            {
                RequestId = requestId,
                Type = SnmpRequestType.Set,
                Community = community,
                VarBinds = varBinds.ToList()
            };
        }

        /// <summary>
        /// Returns a string representation of the request
        /// </summary>
        public override string ToString()
        {
            return $"{Type} Request ID: {RequestId}, Community: {Community}, OIDs: {string.Join(", ", Oids)}";
        }
    }

    /// <summary>
    /// Represents an SNMP variable binding
    /// </summary>
    public class SnmpVarBind
    {
        public string Oid { get; set; } = string.Empty;
        public SnmpValue? Value { get; set; }
        public SnmpError Error { get; set; } = SnmpError.NoError;
        public int ErrorIndex { get; set; }

        /// <summary>
        /// Creates a variable binding with a value
        /// </summary>
        public static SnmpVarBind WithValue(string oid, SnmpValue value)
        {
            return new SnmpVarBind { Oid = oid, Value = value };
        }

        /// <summary>
        /// Creates a variable binding with an error
        /// </summary>
        public static SnmpVarBind WithError(string oid, SnmpError error, int errorIndex = 0)
        {
            return new SnmpVarBind { Oid = oid, Error = error, ErrorIndex = errorIndex };
        }

        /// <summary>
        /// Checks if this variable binding has an error
        /// </summary>
        public bool HasError => Error != SnmpError.NoError;

        /// <summary>
        /// Returns a string representation of the variable binding
        /// </summary>
        public override string ToString()
        {
            if (HasError)
                return $"{Oid} - Error: {Error} (Index: {ErrorIndex})";

            return $"{Oid} = {Value}";
        }
    }
}
