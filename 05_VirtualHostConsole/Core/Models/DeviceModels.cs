using Yanets.Core.Models;
using Yanets.Core.Vendors;

namespace Yanets.VirtualHostConsole.Core.Models
{
    public class DeviceState
    {
        public string RunningConfiguration { get; set; } = string.Empty;
        public string StartupConfiguration { get; set; } = string.Empty;
        public RoutingTable RoutingTable { get; set; } = new();
        public ArpTable ArpTable { get; set; } = new();
        public MacAddressTable MacAddressTable { get; set; } = new();
        public List<VlanConfiguration> Vlans { get; set; } = new();
        public SystemResources Resources { get; set; } = new();
        public Dictionary<string, object> Variables { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public DeviceStatus Status { get; set; } = DeviceStatus.Normal;

        public DeviceState Clone()
        {
            return new DeviceState
            {
                RunningConfiguration = RunningConfiguration,
                StartupConfiguration = StartupConfiguration,
                RoutingTable = new RoutingTable { Entries = new List<RouteEntry>(RoutingTable.Entries) },
                ArpTable = new ArpTable { Entries = new Dictionary<string, ArpEntry>(ArpTable.Entries) },
                MacAddressTable = new MacAddressTable { Entries = new Dictionary<int, MacEntry>(MacAddressTable.Entries) },
                Vlans = new List<VlanConfiguration>(Vlans),
                Resources = new SystemResources
                {
                    CpuUtilization = Resources.CpuUtilization,
                    MemoryUsed = Resources.MemoryUsed,
                    MemoryTotal = Resources.MemoryTotal
                },
                Variables = new Dictionary<string, object>(Variables),
                LastUpdated = LastUpdated,
                Status = Status
            };
        }
    }

    public class MacAddressTable
    {
        public Dictionary<int, MacEntry> Entries { get; set; } = new();

        public void AddEntry(int vlanId, string macAddress, string interfaceName)
        {
            Entries[vlanId] = new MacEntry
            {
                MacAddress = macAddress,
                InterfaceName = interfaceName,
                VlanId = vlanId,
                Timestamp = DateTime.UtcNow,
                Type = MacEntryType.Dynamic
            };
        }

        public string GetInterfaceForMac(string macAddress, int vlanId = 1)
        {
            return Entries.TryGetValue(vlanId, out var entry) && entry.MacAddress == macAddress
                ? entry.InterfaceName : null;
        }
    }

    public class MacEntry
    {
        public string MacAddress { get; set; }
        public string InterfaceName { get; set; }
        public int VlanId { get; set; }
        public DateTime Timestamp { get; set; }
        public MacEntryType Type { get; set; }
    }

    public enum MacEntryType
    {
        Dynamic,
        Static,
        Permanent
    }

    public class VlanConfiguration
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> InterfaceMembers { get; set; } = new();
        public Dictionary<string, string> Properties { get; set; } = new();
    }

    public class SystemResources
    {
        public double CpuUtilization { get; set; }
        public long MemoryUsed { get; set; }
        public long MemoryTotal { get; set; }
        public long DiskUsed { get; set; }
        public long DiskTotal { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public double MemoryUtilization => MemoryTotal > 0 ? (double)MemoryUsed / MemoryTotal * 100 : 0;
        public double DiskUtilization => DiskTotal > 0 ? (double)DiskUsed / DiskTotal * 100 : 0;
    }

    public enum DeviceStatus
    {
        Normal,
        Warning,
        Error,
        Maintenance
    }

    public class NetworkStack
    {
        public string HostId { get; }
        public List<NetworkInterface> Interfaces { get; set; } = new();
        public RoutingTable RoutingTable { get; set; } = new();
        public ArpTable ArpTable { get; set; } = new();
        public MacAddressTable MacAddressTable { get; set; } = new();

        public NetworkStack(string hostId)
        {
            HostId = hostId;
        }

        public NetworkInterface GetInterfaceByName(string name)
        {
            return Interfaces.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public NetworkInterface GetInterfaceByIp(string ipAddress)
        {
            return Interfaces.FirstOrDefault(i => i.IpAddress == ipAddress);
        }

        public void AddInterface(NetworkInterface networkInterface)
        {
            Interfaces.Add(networkInterface);
        }

        public void RemoveInterface(string name)
        {
            Interfaces.RemoveAll(i => i.Name == name);
        }

        public string GetNextHop(string destinationIp)
        {
            var route = RoutingTable.GetRoute(destinationIp);
            return route?.Gateway;
        }

        public string ResolveMacAddress(string ipAddress)
        {
            return ArpTable.GetMacAddress(ipAddress);
        }
    }

    public class CliSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string HostId { get; set; }
        public IVirtualHost Host { get; set; }
        public bool IsAuthenticated { get; set; }
        public int PrivilegeLevel { get; set; }
        public CliMode CurrentMode { get; set; } = CliMode.UserExec;
        public Stack<CliMode> ModeStack { get; set; } = new();
        public Dictionary<string, object> SessionVariables { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public EndPoint RemoteEndPoint { get; set; }

        public void UpdateActivity()
        {
            LastActivity = DateTime.UtcNow;
        }

        public bool IsExpired(int timeoutMinutes = 30)
        {
            return DateTime.UtcNow - LastActivity > TimeSpan.FromMinutes(timeoutMinutes);
        }
    }

    public enum CliMode
    {
        UserExec,
        PrivilegedExec,
        GlobalConfig,
        InterfaceConfig,
        RouterConfig,
        LineConfig,
        VlanConfig
    }

    public class CommandResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public DeviceState UpdatedState { get; set; }
        public Dictionary<string, object> ResultData { get; set; } = new();

        public static CommandResult Success(string output = "")
        {
            return new CommandResult { Success = true, Output = output };
        }

        public static CommandResult Error(string errorMessage)
        {
            return new CommandResult { Success = false, ErrorMessage = errorMessage };
        }

        public static CommandResult SuccessWithStateUpdate(string output, DeviceState newState)
        {
            return new CommandResult
            {
                Success = true,
                Output = output,
                UpdatedState = newState
            };
        }
    }

    public class SnmpRequest
    {
        public int RequestId { get; set; }
        public SnmpRequestType Type { get; set; }
        public string Community { get; set; } = "public";
        public SnmpVersion Version { get; set; } = SnmpVersion.V2C;
        public List<string> Oids { get; set; } = new();
        public List<SnmpVarBind> VarBinds { get; set; } = new();
        public EndPoint RemoteEndPoint { get; set; }
    }

    public class SnmpResponse
    {
        public int RequestId { get; set; }
        public SnmpError ErrorStatus { get; set; }
        public int ErrorIndex { get; set; }
        public List<SnmpVarBind> VarBinds { get; set; } = new();

        public static SnmpResponse Success(int requestId, List<SnmpVarBind> varBinds)
        {
            return new SnmpResponse
            {
                RequestId = requestId,
                ErrorStatus = SnmpError.NoError,
                VarBinds = varBinds
            };
        }

        public static SnmpResponse Error(int requestId, SnmpError error, int errorIndex = 0)
        {
            return new SnmpResponse
            {
                RequestId = requestId,
                ErrorStatus = error,
                ErrorIndex = errorIndex
            };
        }
    }

    public class SnmpVarBind
    {
        public string Oid { get; set; }
        public SnmpValue Value { get; set; }
        public SnmpError Error { get; set; } = SnmpError.NoError;
    }

    public enum SnmpRequestType
    {
        Get,
        GetNext,
        GetBulk,
        Set
    }

    public enum SnmpVersion
    {
        V1,
        V2C,
        V3
    }

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
}