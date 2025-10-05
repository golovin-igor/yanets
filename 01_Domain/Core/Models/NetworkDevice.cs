using System.Drawing;
using Yanets.Core.Interfaces;
using Yanets.Core.Vendors;
using Yanets.SharedKernel;

namespace Yanets.Core.Models
{
    /// <summary>
    /// Abstract base class representing a network device in the simulation
    /// </summary>
    public abstract class NetworkDevice
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public DeviceType Type { get; set; }
        public VendorProfile Vendor { get; set; } = null!;
        public Point Position { get; set; }
        public List<NetworkInterface> Interfaces { get; set; } = new();
        public Models.DeviceState State { get; set; } = new Models.DeviceState();
        public IDeviceSimulator? Simulator { get; set; }

        /// <summary>
        /// Gets the management interface (first interface with an IP address)
        /// </summary>
        public NetworkInterface? ManagementInterface =>
            Interfaces.FirstOrDefault(i => !string.IsNullOrEmpty(i.IpAddress));

        /// <summary>
        /// Gets the primary IP address for management access
        /// </summary>
        public string? ManagementIpAddress => ManagementInterface?.IpAddress;

        /// <summary>
        /// Initializes a new network device with default interfaces
        /// </summary>
        protected NetworkDevice(DeviceType deviceType, string vendorName)
        {
            Type = deviceType;
            InitializeDefaultInterfaces();
            InitializeInterfaceConnectivity();
        }

        /// <summary>
        /// Initializes default interfaces based on device type
        /// </summary>
        private void InitializeDefaultInterfaces()
        {
            switch (Type)
            {
                case DeviceType.Router:
                    AddDefaultRouterInterfaces();
                    break;
                case DeviceType.Switch:
                    AddDefaultSwitchInterfaces();
                    break;
                case DeviceType.Firewall:
                    AddDefaultFirewallInterfaces();
                    break;
                default:
                    AddDefaultEthernetInterfaces();
                    break;
            }
        }

        /// <summary>
        /// Initializes interface connectivity tracking for all interfaces
        /// </summary>
        private void InitializeInterfaceConnectivity()
        {
            foreach (var iface in Interfaces.Where(i => i.IsUp))
            {
                State.InterfaceConnectivity[iface.Name] = true;
            }
        }

        private void AddDefaultRouterInterfaces()
        {
            // Add loopback interface
            AddInterface(new NetworkInterface
            {
                Name = "Loopback0",
                Type = InterfaceType.Loopback,
                IsUp = true,
                Status = InterfaceStatus.Up
            });

            // Add GigabitEthernet interfaces
            for (int i = 0; i < 4; i++)
            {
                AddInterface(new NetworkInterface
                {
                    Name = $"GigabitEthernet0/{i}",
                    Type = InterfaceType.GigabitEthernet,
                    Speed = 1000,
                    IsUp = true,
                    Status = InterfaceStatus.Up
                });
            }
        }

        private void AddDefaultSwitchInterfaces()
        {
            // Add VLAN interface
            AddInterface(new NetworkInterface
            {
                Name = "Vlan1",
                Type = InterfaceType.Vlan,
                IsUp = true,
                Status = InterfaceStatus.Up
            });

            // Add FastEthernet interfaces
            for (int i = 0; i < 24; i++)
            {
                AddInterface(new NetworkInterface
                {
                    Name = $"FastEthernet0/{i}",
                    Type = InterfaceType.FastEthernet,
                    Speed = 100,
                    IsUp = true,
                    Status = InterfaceStatus.Up
                });
            }

            // Add GigabitEthernet interfaces
            for (int i = 0; i < 4; i++)
            {
                AddInterface(new NetworkInterface
                {
                    Name = $"GigabitEthernet0/{i}",
                    Type = InterfaceType.GigabitEthernet,
                    Speed = 1000,
                    IsUp = true,
                    Status = InterfaceStatus.Up
                });
            }
        }

        private void AddDefaultFirewallInterfaces()
        {
            // Add management interface
            AddInterface(new NetworkInterface
            {
                Name = "Management0/0",
                Type = InterfaceType.GigabitEthernet,
                Speed = 1000,
                IsUp = true,
                Status = InterfaceStatus.Up
            });

            // Add inside and outside interfaces
            AddInterface(new NetworkInterface
            {
                Name = "GigabitEthernet0/0",
                Type = InterfaceType.GigabitEthernet,
                Speed = 1000,
                IsUp = true,
                Status = InterfaceStatus.Up
            });

            AddInterface(new NetworkInterface
            {
                Name = "GigabitEthernet0/1",
                Type = InterfaceType.GigabitEthernet,
                Speed = 1000,
                IsUp = true,
                Status = InterfaceStatus.Up
            });
        }

        private void AddDefaultEthernetInterfaces()
        {
            // Add default Ethernet interfaces
            for (int i = 0; i < 4; i++)
            {
                AddInterface(new NetworkInterface
                {
                    Name = $"Ethernet{i}",
                    Type = InterfaceType.Ethernet,
                    Speed = 10,
                    IsUp = true,
                    Status = InterfaceStatus.Up
                });
            }
        }

        /// <summary>
        /// Adds an interface to the device
        /// </summary>
        public void AddInterface(NetworkInterface interfaceObj)
        {
            if (interfaceObj == null)
                throw new ArgumentNullException(nameof(interfaceObj));

            if (Interfaces.Any(i => i.Name.Equals(interfaceObj.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Interface '{interfaceObj.Name}' already exists on device");

            Interfaces.Add(interfaceObj);
        }

        /// <summary>
        /// Removes an interface from the device
        /// </summary>
        public bool RemoveInterface(string interfaceName)
        {
            return Interfaces.RemoveAll(i =>
                i.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase)) > 0;
        }

        /// <summary>
        /// Gets an interface by name
        /// </summary>
        public NetworkInterface? GetInterface(string interfaceName)
        {
            return Interfaces.FirstOrDefault(i =>
                i.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all interfaces that are currently up
        /// </summary>
        public IEnumerable<NetworkInterface> GetUpInterfaces()
        {
            return Interfaces.Where(i => i.IsUp);
        }

        /// <summary>
        /// Validates the device configuration
        /// </summary>
        public virtual bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            if (string.IsNullOrWhiteSpace(Hostname))
                return false;

            if (Vendor == null)
                return false;

            if (Interfaces.Count == 0)
                return false;

            // Check for duplicate interface names
            var interfaceNames = Interfaces.Select(i => i.Name).ToList();
            if (interfaceNames.Count != interfaceNames.Distinct().Count())
                return false;

            return true;
        }

        /// <summary>
        /// Initializes interface connectivity tracking for all interfaces
        /// </summary>
        private void InitializeInterfaceConnectivity()
        {
            foreach (var iface in Interfaces.Where(i => i.IsUp))
            {
                State.InterfaceConnectivity[iface.Name] = true;
            }
        }

        /// <summary>
        /// Creates a deep clone of the device
        /// </summary>
        public NetworkDevice Clone()
        {
            var clone = (NetworkDevice)MemberwiseClone();
            clone.Id = Guid.NewGuid();
            clone.Interfaces = Interfaces.Select(i => i.Clone()).ToList();
            clone.Position = new Point(Position.X, Position.Y);
            return clone;
        }
    }
}
