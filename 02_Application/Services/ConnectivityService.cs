using Yanets.Core.Models;

namespace Yanets.Application.Services
{
    /// <summary>
    /// Service to handle network connectivity between devices
    /// </summary>
    public class ConnectivityService
    {
        private readonly NetworkTopology _topology;

        public ConnectivityService(NetworkTopology topology)
        {
            _topology = topology ?? throw new ArgumentNullException(nameof(topology));
        }

        /// <summary>
        /// Checks if a target IP address is reachable from a source device
        /// </summary>
        public bool IsReachable(string sourceDeviceName, string targetIpAddress)
        {
            var sourceDevice = _topology.GetDeviceByHostname(sourceDeviceName);
            if (sourceDevice == null)
                return false;

            // Find the target device that owns the target IP
            var targetDevice = FindDeviceByIpAddress(targetIpAddress);
            if (targetDevice == null)
                return false;

            // Check if target device has any reachable interfaces
            var reachableInterfaces = targetDevice.Interfaces
                .Where(iface => iface.IsUp && !iface.IsShutdown && !string.IsNullOrEmpty(iface.IpAddress))
                .ToList();

            if (!reachableInterfaces.Any())
                return false;

            // For now, if target device has any up interface, consider it reachable
            // In a more sophisticated implementation, we would check actual network topology
            return true;
        }

        /// <summary>
        /// Finds a device that owns the specified IP address
        /// </summary>
        private NetworkDevice? FindDeviceByIpAddress(string ipAddress)
        {
            foreach (var device in _topology.Devices)
            {
                var matchingInterface = device.Interfaces
                    .FirstOrDefault(iface => iface.IpAddress == ipAddress);

                if (matchingInterface != null)
                    return device;
            }

            return null;
        }

        /// <summary>
        /// Updates connectivity for all devices when an interface changes state
        /// </summary>
        public void UpdateConnectivityForInterface(string deviceName, string interfaceName, bool isConnected)
        {
            var device = _topology.GetDeviceByHostname(deviceName);
            if (device == null)
                return;

            device.State.UpdateInterfaceConnectivity(interfaceName, isConnected);

            // In a more sophisticated implementation, we would:
            // 1. Find all routes that depend on this interface
            // 2. Update ARP tables for devices that were using this interface
            // 3. Trigger route recalculation across the network
        }

        /// <summary>
        /// Gets all reachable devices from a source device
        /// </summary>
        public IEnumerable<string> GetReachableDevices(string sourceDeviceName)
        {
            var sourceDevice = _topology.GetDeviceByHostname(sourceDeviceName);
            if (sourceDevice == null)
                return Enumerable.Empty<string>();

            var reachableDevices = new List<string>();

            foreach (var device in _topology.Devices)
            {
                if (device.Hostname == sourceDeviceName)
                    continue;

                // Check if any interface on the target device is reachable
                var reachableInterfaces = device.Interfaces
                    .Where(iface => iface.IsUp && !iface.IsShutdown && !string.IsNullOrEmpty(iface.IpAddress))
                    .ToList();

                if (reachableInterfaces.Any())
                    reachableDevices.Add(device.Hostname);
            }

            return reachableDevices;
        }
    }
}
