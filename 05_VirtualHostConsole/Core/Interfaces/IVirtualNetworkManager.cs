using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Core.Interfaces
{
    public interface IVirtualNetworkManager
    {
        Task<IVirtualHost> CreateHostAsync(string hostname, string vendor, string subnetName = "default");
        Task<bool> StartNetworkAsync();
        Task<bool> StopNetworkAsync();
        Task<bool> RemoveHostAsync(string hostId);
        NetworkStatistics GetNetworkStatistics();
        IEnumerable<IVirtualHost> GetAllHosts();
        IVirtualHost GetHostById(string hostId);
        IVirtualHost GetHostByIp(string ipAddress);
        Task<bool> SaveConfigurationAsync(string filePath);
        Task<bool> LoadConfigurationAsync(string filePath);
        event EventHandler<NetworkEventArgs> NetworkEvent;
    }
}