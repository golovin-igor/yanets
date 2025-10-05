using System.Drawing;

namespace Yanets.SharedKernel
{
    /// <summary>
    /// Represents different types of network devices
    /// </summary>
    public enum DeviceType
    {
        Router,
        Switch,
        Firewall,
        AccessPoint,
        Server,
        Workstation
    }

    /// <summary>
    /// Represents different types of network interfaces
    /// </summary>
    public enum InterfaceType
    {
        Ethernet,
        FastEthernet,
        GigabitEthernet,
        TenGigabitEthernet,
        Serial,
        Loopback,
        Tunnel,
        Vlan
    }

    /// <summary>
    /// Represents the operational status of a network interface
    /// </summary>
    public enum InterfaceStatus
    {
        Up,
        Down,
        AdministrativelyDown,
        Testing,
        Unknown,
        Dormant,
        NotPresent,
        LowerLayerDown
    }

    /// <summary>
    /// Represents CLI modes for device interaction
    /// </summary>
    public enum CliMode
    {
        UserExec,
        PrivilegedExec,
        GlobalConfig,
        InterfaceConfig,
        RouterConfig,
        LineConfig,
        VlanConfig,
        VlanDatabase,
        Diagnostic
    }

    /// <summary>
    /// Represents SNMP data types
    /// </summary>
    public enum SnmpDataType
    {
        Integer,
        OctetString,
        ObjectIdentifier,
        IpAddress,
        Counter32,
        Gauge32,
        TimeTicks,
        Counter64,
        Opaque,
        Null
    }

    /// <summary>
    /// Represents SNMP request types
    /// </summary>
    public enum SnmpRequestType
    {
        Get,
        GetNext,
        GetBulk,
        Set,
        Trap,
        Inform,
        Response
    }

    /// <summary>
    /// Represents SNMP error codes
    /// </summary>
    public enum SnmpError
    {
        NoError = 0,
        TooBig = 1,
        NoSuchName = 2,
        BadValue = 3,
        ReadOnly = 4,
        GenErr = 5,
        NoAccess = 6,
        WrongType = 7,
        WrongLength = 8,
        WrongEncoding = 9,
        WrongValue = 10,
        NoCreation = 11,
        InconsistentValue = 12,
        ResourceUnavailable = 13,
        CommitFailed = 14,
        UndoFailed = 15,
        AuthorizationError = 16,
        NotWritable = 17,
        InconsistentName = 18
    }

    /// <summary>
    /// Represents SNMP versions
    /// </summary>
    public enum SnmpVersion
    {
        V1 = 0,
        V2c = 1,
        V3 = 3
    }

    /// <summary>
    /// Represents device states for simulation
    /// </summary>
    public enum DeviceSimulationState
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Error
    }

    /// <summary>
    /// Represents connection types between devices
    /// </summary>
    public enum ConnectionType
    {
        Ethernet,
        Serial,
        Fiber,
        Wireless,
        Virtual
    }

    /// <summary>
    /// Represents access modes for SNMP MIB objects
    /// </summary>
    public enum AccessMode
    {
        ReadOnly,
        ReadWrite,
        ReadCreate,
        NotAccessible,
        AccessibleForNotify
    }

    /// <summary>
    /// Represents system resources for device simulation
    /// </summary>
    public class SystemResources
    {
        public double CpuUtilization { get; set; }
        public long MemoryTotal { get; set; }
        public long MemoryUsed { get; set; }
        public long MemoryFree => MemoryTotal - MemoryUsed;
        public double MemoryUtilization => MemoryTotal > 0 ? (double)MemoryUsed / MemoryTotal * 100 : 0;
    }

    /// <summary>
    /// Represents VLAN configuration
    /// </summary>
    public class VlanConfiguration
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<int> Ports { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Represents routing table entry
    /// </summary>
    public class RoutingTableEntry
    {
        public string Destination { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public string Interface { get; set; } = string.Empty;
        public int Metric { get; set; }
        public string Protocol { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents ARP table entry
    /// </summary>
    public class ArpEntry
    {
        public string IpAddress { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string Interface { get; set; } = string.Empty;
        public ArpType Type { get; set; }
    }

    /// <summary>
    /// Represents ARP entry types
    /// </summary>
    public enum ArpType
    {
        Dynamic,
        Static,
        Invalid
    }

    /// <summary>
    /// Represents MAC address table entry
    /// </summary>
    public class MacAddressEntry
    {
        public string MacAddress { get; set; } = string.Empty;
        public string Interface { get; set; } = string.Empty;
        public int Vlan { get; set; }
        public MacEntryType Type { get; set; }
    }

    /// <summary>
    /// Represents MAC address entry types
    /// </summary>
    public enum MacEntryType
    {
        Dynamic,
        Static,
        Permanent,
        Self
    }

    /// <summary>
    /// Represents topology metadata
    /// </summary>
    public class TopologyMetadata
    {
        public string Version { get; set; } = "1.0";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a connection between two network devices
    /// </summary>
    public class Connection
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SourceDeviceId { get; set; }
        public string SourceInterface { get; set; } = string.Empty;
        public Guid TargetDeviceId { get; set; }
        public string TargetInterface { get; set; } = string.Empty;
        public ConnectionType Type { get; set; }
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
    }
}
