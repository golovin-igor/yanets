using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Core.Interfaces
{
    public interface IVirtualHost
    {
        string Id { get; }
        string Hostname { get; set; }
        string IpAddress { get; set; }
        string SubnetName { get; }
        VendorProfile VendorProfile { get; set; }
        NetworkStack NetworkStack { get; }
        DeviceState CurrentState { get; set; }
        HostStatus Status { get; set; }
        DateTime CreatedAt { get; }
        Dictionary<int, ProtocolType> PortMappings { get; }

        Task StartAsync();
        Task StopAsync();
        Task<CommandResult> ExecuteCommandAsync(string command, CliSession session);
        Task<SnmpResponse> HandleSnmpRequestAsync(SnmpRequest request);
        NetworkInterface GetInterface(string name);
        void UpdateInterfaceState(string interfaceName, InterfaceStatus status);
        HostStatistics GetStatistics();
        Task<bool> SaveStateAsync();
        Task<bool> LoadStateAsync();
        event EventHandler<HostStatusEventArgs> StatusChanged;
    }
}