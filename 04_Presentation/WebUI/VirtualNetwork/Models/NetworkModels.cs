using System.Net;
using System.Net.Sockets;

namespace Yanets.WebUI.VirtualNetwork.Models
{
    public enum ProtocolType
    {
        Telnet,
        Ssh,
        Snmp,
        Http,
        Https
    }

    public enum HostStatus
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Error
    }

    public enum InterfaceStatus
    {
        Up,
        Down,
        AdministrativelyDown
    }

    public enum NetworkEventType
    {
        HostCreated,
        HostStarted,
        HostStopped,
        HostRemoved,
        ConnectionEstablished,
        ConnectionClosed,
        SubnetCreated,
        SubnetRemoved
    }

    public class NetworkEventArgs : EventArgs
    {
        public NetworkEventType EventType { get; set; }
        public string HostId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class NetworkStatistics
    {
        public int TotalHosts { get; set; }
        public int ActiveHosts { get; set; }
        public int TotalConnections { get; set; }
        public int ActiveConnections { get; set; }
        public Dictionary<ProtocolType, int> ProtocolCounts { get; set; } = new();
        public Dictionary<string, HostStatistics> PerHostStats { get; set; } = new();
        public NetworkTrafficStatistics TrafficStats { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class HostStatistics
    {
        public string HostId { get; set; }
        public int ActiveConnections { get; set; }
        public int TotalCommandsExecuted { get; set; }
        public int TotalSnmpRequests { get; set; }
        public DateTime Uptime { get; set; }
        public double CpuUtilization { get; set; }
        public long MemoryUsed { get; set; }
        public Dictionary<ProtocolType, int> ProtocolCounts { get; set; } = new();
    }

    public class NetworkTrafficStatistics
    {
        public long TotalBytesReceived { get; set; }
        public long TotalBytesSent { get; set; }
        public int PacketsReceived { get; set; }
        public int PacketsSent { get; set; }
        public double AverageResponseTime { get; set; }
    }

    public class SubnetDefinition
    {
        public string Name { get; set; }
        public string Cidr { get; set; }
        public IPAddress NetworkAddress { get; set; }
        public IPAddress BroadcastAddress { get; set; }
        public IPAddress Gateway { get; set; }
        public byte PrefixLength { get; set; }
        public List<string> DnsServers { get; set; } = new();
        public HashSet<string> ExcludedIps { get; set; } = new();
        public HashSet<string> AllocatedIps { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class NetworkInterface
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string SubnetMask { get; set; }
        public string MacAddress { get; set; }
        public InterfaceStatus Status { get; set; }
        public int Mtu { get; set; } = 1500;
        public bool IsManagement { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new();
    }

    public class RoutingTable
    {
        public List<RouteEntry> Entries { get; set; } = new();

        public void AddRoute(string destination, string gateway, string interfaceName, int metric = 1)
        {
            Entries.Add(new RouteEntry
            {
                Destination = destination,
                Gateway = gateway,
                InterfaceName = interfaceName,
                Metric = metric,
                Timestamp = DateTime.UtcNow
            });
        }

        public RouteEntry GetRoute(string destination)
        {
            return Entries
                .Where(e => IsNetworkMatch(destination, e.Destination))
                .OrderBy(e => e.Metric)
                .FirstOrDefault();
        }

        private bool IsNetworkMatch(string ipAddress, string network)
        {
            // Simplified network matching logic
            return true;
        }
    }

    public class RouteEntry
    {
        public string Destination { get; set; }
        public string Gateway { get; set; }
        public string InterfaceName { get; set; }
        public int Metric { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ArpTable
    {
        public Dictionary<string, ArpEntry> Entries { get; set; } = new();

        public void AddEntry(string ipAddress, string macAddress)
        {
            Entries[ipAddress] = new ArpEntry
            {
                IpAddress = ipAddress,
                MacAddress = macAddress,
                Timestamp = DateTime.UtcNow,
                Type = ArpEntryType.Dynamic
            };
        }

        public string GetMacAddress(string ipAddress)
        {
            return Entries.TryGetValue(ipAddress, out var entry) ? entry.MacAddress : null;
        }
    }

    public class ArpEntry
    {
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public ArpEntryType Type { get; set; }
    }

    public enum ArpEntryType
    {
        Dynamic,
        Static,
        Incomplete
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
        public ProtocolType ProtocolType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    // Network client wrappers for compatibility
    public class TcpClient
    {
        public System.Net.Sockets.TcpClient InnerClient { get; }

        public TcpClient(System.Net.Sockets.TcpClient innerClient)
        {
            InnerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));
        }

        public NetworkStream GetStream() => new NetworkStream(InnerClient.GetStream());
        public bool Connected => InnerClient.Connected;
        public EndPoint RemoteEndPoint => InnerClient.Client.RemoteEndPoint;
        public EndPoint LocalEndPoint => InnerClient.Client.LocalEndPoint;
        public void Close() => InnerClient.Close();
    }

    public class UdpReceiveResult
    {
        public byte[] Buffer { get; set; }
        public EndPoint RemoteEndPoint { get; set; }
    }

    public class NetworkStream
    {
        private System.IO.Stream _innerStream;

        public NetworkStream(System.IO.Stream innerStream)
        {
            _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        }

        public bool CanRead => _innerStream.CanRead;
        public bool CanWrite => _innerStream.CanWrite;
        public bool DataAvailable => (_innerStream as System.Net.Sockets.NetworkStream)?.DataAvailable ?? false;

        public Task<int> ReadAsync(byte[] buffer, int offset, int count) =>
            _innerStream.ReadAsync(buffer, offset, count);

        public Task WriteAsync(byte[] buffer, int offset, int count) =>
            _innerStream.WriteAsync(buffer, offset, count);

        public void Close() => _innerStream.Close();
        public async Task<string> ReadLineAsync() => await new StreamReader(_innerStream).ReadLineAsync();
        public async Task WriteLineAsync(string line) => await new StreamWriter(_innerStream).WriteLineAsync(line);
    }
}