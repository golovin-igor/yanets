using Yanets.SharedKernel;

namespace Yanets.Core.Snmp
{
    /// <summary>
    /// Represents an SNMP response
    /// </summary>
    public class SnmpResponse
    {
        public int RequestId { get; set; }
        public SnmpError ErrorStatus { get; set; } = SnmpError.NoError;
        public int ErrorIndex { get; set; }
        public List<SnmpVarBind> VarBinds { get; set; } = new();

        /// <summary>
        /// Checks if the response contains an error
        /// </summary>
        public bool HasError => ErrorStatus != SnmpError.NoError;

        /// <summary>
        /// Gets the error description
        /// </summary>
        public string ErrorDescription => ErrorStatus switch
        {
            SnmpError.NoError => "No error",
            SnmpError.TooBig => "Response too big",
            SnmpError.NoSuchName => "No such name",
            SnmpError.BadValue => "Bad value",
            SnmpError.ReadOnly => "Read only",
            SnmpError.GenErr => "General error",
            SnmpError.NoAccess => "No access",
            SnmpError.WrongType => "Wrong type",
            SnmpError.WrongLength => "Wrong length",
            SnmpError.WrongEncoding => "Wrong encoding",
            SnmpError.WrongValue => "Wrong value",
            SnmpError.NoCreation => "No creation",
            SnmpError.InconsistentValue => "Inconsistent value",
            SnmpError.ResourceUnavailable => "Resource unavailable",
            SnmpError.CommitFailed => "Commit failed",
            SnmpError.UndoFailed => "Undo failed",
            SnmpError.AuthorizationError => "Authorization error",
            SnmpError.NotWritable => "Not writable",
            SnmpError.InconsistentName => "Inconsistent name",
            _ => "Unknown error"
        };

        /// <summary>
        /// Creates a successful response
        /// </summary>
        public static SnmpResponse Success(int requestId, params SnmpVarBind[] varBinds)
        {
            return new SnmpResponse
            {
                RequestId = requestId,
                VarBinds = varBinds.ToList()
            };
        }

        /// <summary>
        /// Creates an error response
        /// </summary>
        public static SnmpResponse Error(int requestId, SnmpError error, int errorIndex = 0)
        {
            return new SnmpResponse
            {
                RequestId = requestId,
                ErrorStatus = error,
                ErrorIndex = errorIndex
            };
        }

        /// <summary>
        /// Adds a variable binding to the response
        /// </summary>
        public void AddVarBind(SnmpVarBind varBind)
        {
            VarBinds.Add(varBind);
        }

        /// <summary>
        /// Gets a variable binding by OID
        /// </summary>
        public SnmpVarBind? GetVarBind(string oid)
        {
            return VarBinds.FirstOrDefault(vb => vb.Oid == oid);
        }

        /// <summary>
        /// Validates the response
        /// </summary>
        public bool IsValid()
        {
            return RequestId >= 0 &&
                   (ErrorStatus == SnmpError.NoError || VarBinds.Count == 0);
        }

        /// <summary>
        /// Returns a string representation of the response
        /// </summary>
        public override string ToString()
        {
            if (HasError)
                return $"Response ID: {RequestId}, Error: {ErrorDescription} (Index: {ErrorIndex})";

            return $"Response ID: {RequestId}, VarBinds: {VarBinds.Count}";
        }
    }
}
