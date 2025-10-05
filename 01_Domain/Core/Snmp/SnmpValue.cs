using Yanets.SharedKernel;

namespace Yanets.Core.Snmp
{
    /// <summary>
    /// Represents an SNMP value with type information
    /// </summary>
    public class SnmpValue
    {
        public SnmpDataType Type { get; set; }
        public object Value { get; set; } = null!;

        /// <summary>
        /// Creates an integer SNMP value
        /// </summary>
        public static SnmpValue Integer(int value)
        {
            return new SnmpValue { Type = SnmpDataType.Integer, Value = value };
        }

        /// <summary>
        /// Creates an octet string SNMP value
        /// </summary>
        public static SnmpValue OctetString(string value)
        {
            return new SnmpValue { Type = SnmpDataType.OctetString, Value = value ?? string.Empty };
        }

        /// <summary>
        /// Creates an object identifier SNMP value
        /// </summary>
        public static SnmpValue ObjectIdentifier(string oid)
        {
            return new SnmpValue { Type = SnmpDataType.ObjectIdentifier, Value = oid ?? string.Empty };
        }

        /// <summary>
        /// Creates an IP address SNMP value
        /// </summary>
        public static SnmpValue IpAddress(string ipAddress)
        {
            return new SnmpValue { Type = SnmpDataType.IpAddress, Value = ipAddress ?? string.Empty };
        }

        /// <summary>
        /// Creates a counter32 SNMP value
        /// </summary>
        public static SnmpValue Counter32(uint value)
        {
            return new SnmpValue { Type = SnmpDataType.Counter32, Value = value };
        }

        /// <summary>
        /// Creates a gauge32 SNMP value
        /// </summary>
        public static SnmpValue Gauge32(uint value)
        {
            return new SnmpValue { Type = SnmpDataType.Gauge32, Value = value };
        }

        /// <summary>
        /// Creates a time ticks SNMP value
        /// </summary>
        public static SnmpValue TimeTicks(uint value)
        {
            return new SnmpValue { Type = SnmpDataType.TimeTicks, Value = value };
        }

        /// <summary>
        /// Creates a counter64 SNMP value
        /// </summary>
        public static SnmpValue Counter64(ulong value)
        {
            return new SnmpValue { Type = SnmpDataType.Counter64, Value = value };
        }

        /// <summary>
        /// Gets the value as a specific type
        /// </summary>
        public T? As<T>()
        {
            try
            {
                return (T)Value;
            }
            catch (InvalidCastException)
            {
                return default;
            }
        }

        /// <summary>
        /// Gets the value as an integer
        /// </summary>
        public int AsInt()
        {
            var value = As<int?>();
            return value ?? 0;
        }

        /// <summary>
        /// Gets the value as a string
        /// </summary>
        public string AsString()
        {
            var value = As<string>();
            return value ?? string.Empty;
        }

        /// <summary>
        /// Gets the value as an unsigned integer
        /// </summary>
        public uint AsUint()
        {
            var value = As<uint?>();
            return value ?? 0u;
        }

        /// <summary>
        /// Gets the value as an unsigned long
        /// </summary>
        public ulong AsUlong()
        {
            var value = As<ulong?>();
            return value ?? 0ul;
        }

        /// <summary>
        /// Validates that the value matches the expected type
        /// </summary>
        public bool IsValidType()
        {
            return Type switch
            {
                SnmpDataType.Integer => Value is int,
                SnmpDataType.OctetString => Value is string,
                SnmpDataType.ObjectIdentifier => Value is string,
                SnmpDataType.IpAddress => Value is string,
                SnmpDataType.Counter32 => Value is uint,
                SnmpDataType.Gauge32 => Value is uint,
                SnmpDataType.TimeTicks => Value is uint,
                SnmpDataType.Counter64 => Value is ulong,
                _ => false
            };
        }

        /// <summary>
        /// Creates a null SNMP value
        /// </summary>
        public static SnmpValue Null()
        {
            return new SnmpValue { Type = SnmpDataType.Null, Value = string.Empty };
        }

        /// <summary>
        /// Checks if this value represents a null
        /// </summary>
        public bool IsNull => Type == SnmpDataType.Null || Value == null;

        /// <summary>
        /// Returns a string representation of the value
        /// </summary>
        public override string ToString()
        {
            if (IsNull)
                return "NULL";

            return Type switch
            {
                SnmpDataType.Integer => $"Integer: {AsInt()}",
                SnmpDataType.OctetString => $"OctetString: \"{AsString()}\"",
                SnmpDataType.ObjectIdentifier => $"OID: {AsString()}",
                SnmpDataType.IpAddress => $"IPAddress: {AsString()}",
                SnmpDataType.Counter32 => $"Counter32: {AsUint()}",
                SnmpDataType.Gauge32 => $"Gauge32: {AsUint()}",
                SnmpDataType.TimeTicks => $"TimeTicks: {AsUint()}",
                SnmpDataType.Counter64 => $"Counter64: {AsUlong()}",
                _ => $"{Type}: {Value}"
            };
        }

        /// <summary>
        /// Compares two SNMP values for equality
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is not SnmpValue other)
                return false;

            return Type == other.Type && Equals(Value, other.Value);
        }

        /// <summary>
        /// Gets the hash code for this SNMP value
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }
    }
}
