using Yanets.Core.Models;
using Yanets.SharedKernel;

namespace Yanets.Core.Snmp
{
    /// <summary>
    /// Represents a handler for SNMP OID operations
    /// </summary>
    public class OidHandler
    {
        public string Oid { get; set; } = string.Empty;
        public Func<DeviceState, SnmpValue> GetHandler { get; set; } = null!;
        public Func<DeviceState, SnmpValue, bool> SetHandler { get; set; } = null!;
        public bool IsTable { get; set; }
        public Func<DeviceState, List<string>> GetTableIndices { get; set; } = null!;
        public AccessMode Access { get; set; } = AccessMode.ReadOnly;

        /// <summary>
        /// Gets the value for this OID from the device state
        /// </summary>
        public SnmpValue GetValue(DeviceState state)
        {
            if (GetHandler == null)
                throw new InvalidOperationException($"No GET handler configured for OID {Oid}");

            try
            {
                return GetHandler(state);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting value for OID {Oid}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sets the value for this OID in the device state
        /// </summary>
        public bool SetValue(DeviceState state, SnmpValue value)
        {
            if (SetHandler == null)
                throw new InvalidOperationException($"No SET handler configured for OID {Oid}");

            if (Access == AccessMode.ReadOnly)
                throw new InvalidOperationException($"OID {Oid} is read-only");

            try
            {
                return SetHandler(state, value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error setting value for OID {Oid}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets table indices for this OID if it's a table
        /// </summary>
        public List<string> GetIndices(DeviceState state)
        {
            if (!IsTable)
                return new List<string>();

            if (GetTableIndices == null)
                throw new InvalidOperationException($"No table indices handler configured for OID {Oid}");

            try
            {
                return GetTableIndices(state);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting table indices for OID {Oid}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates the OID handler configuration
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Oid) &&
                   GetHandler != null &&
                   (Access != AccessMode.ReadOnly || SetHandler != null) &&
                   (!IsTable || GetTableIndices != null);
        }

        /// <summary>
        /// Creates a read-only OID handler
        /// </summary>
        public static OidHandler ReadOnly(string oid, Func<DeviceState, SnmpValue> getHandler)
        {
            return new OidHandler
            {
                Oid = oid,
                GetHandler = getHandler,
                Access = AccessMode.ReadOnly
            };
        }

        /// <summary>
        /// Creates a read-write OID handler
        /// </summary>
        public static OidHandler ReadWrite(string oid, Func<DeviceState, SnmpValue> getHandler, Func<DeviceState, SnmpValue, bool> setHandler)
        {
            return new OidHandler
            {
                Oid = oid,
                GetHandler = getHandler,
                SetHandler = setHandler,
                Access = AccessMode.ReadWrite
            };
        }

        /// <summary>
        /// Creates a table OID handler
        /// </summary>
        public static OidHandler Table(string oid, Func<DeviceState, SnmpValue> getHandler, Func<DeviceState, List<string>> getIndices)
        {
            return new OidHandler
            {
                Oid = oid,
                GetHandler = getHandler,
                GetTableIndices = getIndices,
                IsTable = true,
                Access = AccessMode.ReadOnly
            };
        }
    }
}
