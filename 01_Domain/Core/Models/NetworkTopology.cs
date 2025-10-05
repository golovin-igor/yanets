using System.Text.Json;
using Yanets.SharedKernel;

namespace Yanets.Core.Models
{
    /// <summary>
    /// Represents a complete network topology with devices and connections
    /// </summary>
    public class NetworkTopology
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<NetworkDevice> Devices { get; set; } = new();
        public List<Connection> Connections { get; set; } = new();
        public TopologyMetadata Metadata { get; set; } = new();

        /// <summary>
        /// Validates the topology for consistency
        /// </summary>
        public bool IsValid()
        {
            // Empty topology is invalid
            if (Devices.Count == 0)
                return false;

            // Check for duplicate device IDs
            var deviceIds = Devices.Select(d => d.Id).ToList();
            if (deviceIds.Count != deviceIds.Distinct().Count())
                return false;

            // Check for duplicate device hostnames
            var hostnames = Devices.Select(d => d.Hostname).ToList();
            if (hostnames.Count != hostnames.Distinct().Count())
                return false;

            // Check that all connections reference valid devices
            foreach (var connection in Connections)
            {
                if (!Devices.Any(d => d.Id == connection.SourceDeviceId) ||
                    !Devices.Any(d => d.Id == connection.TargetDeviceId))
                    return false;

                // Check that interfaces exist on the devices
                var sourceDevice = Devices.First(d => d.Id == connection.SourceDeviceId);
                var targetDevice = Devices.First(d => d.Id == connection.TargetDeviceId);

                if (!sourceDevice.Interfaces.Any(i => i.Name == connection.SourceInterface) ||
                    !targetDevice.Interfaces.Any(i => i.Name == connection.TargetInterface))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Adds a device to the topology
        /// </summary>
        public void AddDevice(NetworkDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            if (Devices.Any(d => d.Id == device.Id))
                throw new InvalidOperationException($"Device with ID {device.Id} already exists in topology");

            if (Devices.Any(d => d.Hostname.Equals(device.Hostname, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Device with hostname '{device.Hostname}' already exists in topology");

            Devices.Add(device);
            UpdateMetadata();
        }

        /// <summary>
        /// Removes a device from the topology
        /// </summary>
        public bool RemoveDevice(Guid deviceId)
        {
            var device = Devices.FirstOrDefault(d => d.Id == deviceId);
            if (device == null)
                return false;

            // Remove all connections involving this device
            Connections.RemoveAll(c =>
                c.SourceDeviceId == deviceId || c.TargetDeviceId == deviceId);

            Devices.Remove(device);
            UpdateMetadata();
            return true;
        }

        /// <summary>
        /// Adds a connection between two devices
        /// </summary>
        public void AddConnection(Connection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (Connections.Any(c => c.Id == connection.Id))
                throw new InvalidOperationException($"Connection with ID {connection.Id} already exists in topology");

            if (connection.SourceDeviceId == connection.TargetDeviceId)
                throw new InvalidOperationException("Cannot connect a device to itself");

            Connections.Add(connection);
            UpdateMetadata();
        }

        /// <summary>
        /// Removes a connection from the topology
        /// </summary>
        public bool RemoveConnection(Guid connectionId)
        {
            var removed = Connections.RemoveAll(c => c.Id == connectionId) > 0;
            if (removed)
                UpdateMetadata();
            return removed;
        }

        /// <summary>
        /// Gets a device by ID
        /// </summary>
        public NetworkDevice? GetDevice(Guid deviceId)
        {
            return Devices.FirstOrDefault(d => d.Id == deviceId);
        }

        /// <summary>
        /// Gets a device by hostname
        /// </summary>
        public NetworkDevice? GetDeviceByHostname(string hostname)
        {
            return Devices.FirstOrDefault(d =>
                d.Hostname.Equals(hostname, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all connections for a specific device
        /// </summary>
        public IEnumerable<Connection> GetDeviceConnections(Guid deviceId)
        {
            return Connections.Where(c =>
                c.SourceDeviceId == deviceId || c.TargetDeviceId == deviceId);
        }

        /// <summary>
        /// Updates the topology metadata
        /// </summary>
        private void UpdateMetadata()
        {
            Metadata.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Serializes the topology to JSON
        /// </summary>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Creates a topology from JSON
        /// </summary>
        public static NetworkTopology FromJson(string json)
        {
            return JsonSerializer.Deserialize<NetworkTopology>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? throw new InvalidOperationException("Failed to deserialize topology");
        }
    }
}
