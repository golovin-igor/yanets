using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Simulation
{
    public class NetworkStack
    {
        public string HostId { get; }
        public List<NetworkInterface> Interfaces { get; set; }
        public RoutingTable RoutingTable { get; set; }
        public ArpTable ArpTable { get; set; }
        public MacAddressTable MacAddressTable { get; set; }

        public NetworkStack(string hostId)
        {
            HostId = hostId ?? throw new ArgumentNullException(nameof(hostId));
            Interfaces = new List<NetworkInterface>();
            RoutingTable = new RoutingTable();
            ArpTable = new ArpTable();
            MacAddressTable = new MacAddressTable();
        }

        public NetworkInterface GetInterfaceByName(string name)
        {
            return Interfaces.FirstOrDefault(i =>
                i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public NetworkInterface GetInterfaceByIp(string ipAddress)
        {
            return Interfaces.FirstOrDefault(i => i.IpAddress == ipAddress);
        }

        public void AddInterface(NetworkInterface networkInterface)
        {
            if (networkInterface == null)
                throw new ArgumentNullException(nameof(networkInterface));

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

        public void AddArpEntry(string ipAddress, string macAddress)
        {
            ArpTable.AddEntry(ipAddress, macAddress);
        }

        public void AddMacEntry(int vlanId, string macAddress, string interfaceName)
        {
            MacAddressTable.AddEntry(vlanId, macAddress, interfaceName);
        }

        public string GetInterfaceForMac(string macAddress, int vlanId = 1)
        {
            return MacAddressTable.GetInterfaceForMac(macAddress, vlanId);
        }

        public bool IsLocalIp(string ipAddress)
        {
            return Interfaces.Any(i => i.IpAddress == ipAddress);
        }

        public NetworkInterface GetInterfaceForDestination(string destinationIp)
        {
            // Find the interface that would be used to reach the destination
            var route = RoutingTable.GetRoute(destinationIp);
            if (route != null)
            {
                return GetInterfaceByName(route.InterfaceName);
            }

            // Default route
            return Interfaces.FirstOrDefault(i => i.IsManagement);
        }

        public void UpdateInterfaceStatus(string interfaceName, InterfaceStatus status)
        {
            var interfaceInfo = GetInterfaceByName(interfaceName);
            if (interfaceInfo != null)
            {
                interfaceInfo.Status = status;
            }
        }

        public Dictionary<string, NetworkInterface> GetInterfaceDictionary()
        {
            return Interfaces.ToDictionary(i => i.Name, i => i);
        }

        public List<string> GetInterfaceNames()
        {
            return Interfaces.Select(i => i.Name).ToList();
        }

        public List<NetworkInterface> GetUpInterfaces()
        {
            return Interfaces.Where(i => i.Status == InterfaceStatus.Up).ToList();
        }

        public NetworkInterface GetManagementInterface()
        {
            return Interfaces.FirstOrDefault(i => i.IsManagement);
        }
    }
}