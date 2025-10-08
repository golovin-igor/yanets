using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Core.Interfaces
{
    public interface ISubnetManager
    {
        Task<SubnetDefinition> CreateSubnetAsync(string cidr, string name, string gateway = null);
        Task<bool> RemoveSubnetAsync(string name);
        SubnetDefinition GetSubnet(string name);
        IEnumerable<SubnetDefinition> GetAllSubnets();
        string AllocateIpAddress(string subnetName);
        bool ReleaseIpAddress(string subnetName, string ipAddress);
        bool IsIpAvailable(string subnetName, string ipAddress);
        int GetAvailableIpCount(string subnetName);
        Dictionary<string, string> GetAllocatedIps(string subnetName);
    }
}