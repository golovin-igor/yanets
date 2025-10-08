using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Core.Interfaces
{
    public interface IConnectionRouter
    {
        Task RouteTcpConnectionAsync(TcpClient client, int localPort);
        Task RouteUdpPacketAsync(UdpReceiveResult packet, int localPort);
        void RegisterHostPorts(IVirtualHost host, Dictionary<ProtocolType, int> portMappings);
        void UnregisterHostPorts(IVirtualHost host);
        Dictionary<ProtocolType, int> GetHostPortMappings(IVirtualHost host);
        bool IsPortRegistered(int port);
        IVirtualHost GetHostByPort(int port);
    }
}