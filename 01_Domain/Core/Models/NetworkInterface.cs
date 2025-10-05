using Yanets.SharedKernel;

namespace Yanets.Core.Models
{
    /// <summary>
    /// Represents a network interface on a device
    /// </summary>
    public class NetworkInterface : ICloneable
    {
        public string Name { get; set; } = string.Empty;
        public InterfaceType Type { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string SubnetMask { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public InterfaceStatus Status { get; set; } = InterfaceStatus.Down;
        public int Speed { get; set; } = 10; // Mbps, default to Ethernet speed
        public bool IsUp { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Mtu { get; set; } = 1500;
        public bool IsShutdown { get; set; }

        /// <summary>
        /// Gets the network portion of the IP address
        /// </summary>
        public string NetworkAddress => CalculateNetworkAddress();

        /// <summary>
        /// Gets the broadcast address for this interface
        /// </summary>
        public string BroadcastAddress => CalculateBroadcastAddress();

        /// <summary>
        /// Gets whether this interface has a valid IP configuration
        /// </summary>
        public bool HasIpConfiguration => !string.IsNullOrEmpty(IpAddress) && !string.IsNullOrEmpty(SubnetMask);

        /// <summary>
        /// Gets the operational status based on administrative and link status
        /// </summary>
        public InterfaceStatus OperationalStatus
        {
            get
            {
                if (IsShutdown || !IsUp)
                    return InterfaceStatus.AdministrativelyDown;

                return Status;
            }
        }

        /// <summary>
        /// Validates the interface configuration
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            // Validate IP address format if provided
            if (!string.IsNullOrEmpty(IpAddress) && !IsValidIpAddress(IpAddress))
                return false;

            // Validate subnet mask format if provided
            if (!string.IsNullOrEmpty(SubnetMask) && !IsValidSubnetMask(SubnetMask))
                return false;

            // Validate MAC address format if provided
            if (!string.IsNullOrEmpty(MacAddress) && !IsValidMacAddress(MacAddress))
                return false;

            // Validate MTU
            if (Mtu < 64 || Mtu > 9216)
                return false;

            return true;
        }

        /// <summary>
        /// Generates a random MAC address for this interface
        /// </summary>
        public void GenerateMacAddress()
        {
            var random = new Random();
            var bytes = new byte[6];
            random.NextBytes(bytes);

            // Set the locally administered bit and unicast bit
            bytes[0] = (byte)((bytes[0] & 0xFC) | 0x02);

            MacAddress = string.Join(":", bytes.Select(b => b.ToString("X2")));
        }

        /// <summary>
        /// Sets the IP address and subnet mask
        /// </summary>
        public void SetIpConfiguration(string ipAddress, string subnetMask)
        {
            if (!IsValidIpAddress(ipAddress))
                throw new ArgumentException("Invalid IP address format", nameof(ipAddress));

            if (!IsValidSubnetMask(subnetMask))
                throw new ArgumentException("Invalid subnet mask format", nameof(subnetMask));

            IpAddress = ipAddress;
            SubnetMask = subnetMask;
        }

        /// <summary>
        /// Calculates the network address from IP and subnet mask
        /// </summary>
        private string CalculateNetworkAddress()
        {
            if (!HasIpConfiguration)
                return string.Empty;

            var ipBytes = IpAddressToBytes(IpAddress);
            var maskBytes = IpAddressToBytes(SubnetMask);

            var networkBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
            }

            return BytesToIpAddress(networkBytes);
        }

        /// <summary>
        /// Calculates the broadcast address from IP and subnet mask
        /// </summary>
        private string CalculateBroadcastAddress()
        {
            if (!HasIpConfiguration)
                return string.Empty;

            var ipBytes = IpAddressToBytes(IpAddress);
            var maskBytes = IpAddressToBytes(SubnetMask);

            var broadcastBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                broadcastBytes[i] = (byte)(ipBytes[i] | (maskBytes[i] ^ 0xFF));
            }

            return BytesToIpAddress(broadcastBytes);
        }

        /// <summary>
        /// Converts IP address string to byte array
        /// </summary>
        private static byte[] IpAddressToBytes(string ipAddress)
        {
            return ipAddress.Split('.').Select(byte.Parse).ToArray();
        }

        /// <summary>
        /// Converts byte array to IP address string
        /// </summary>
        private static string BytesToIpAddress(byte[] bytes)
        {
            return string.Join(".", bytes);
        }

        /// <summary>
        /// Validates IP address format
        /// </summary>
        private static bool IsValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            var parts = ipAddress.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (!byte.TryParse(part, out var value) || value > 255)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validates subnet mask format
        /// </summary>
        private static bool IsValidSubnetMask(string subnetMask)
        {
            if (!IsValidIpAddress(subnetMask))
                return false;

            var bytes = IpAddressToBytes(subnetMask);
            var binary = string.Join("", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

            // Valid subnet masks should be contiguous 1s followed by 0s
            var firstZero = binary.IndexOf('0');
            if (firstZero == -1)
                return true; // All 1s (255.255.255.255)

            var afterFirstZero = binary.Substring(firstZero);
            return !afterFirstZero.Contains('1'); // No 1s after first 0
        }

        /// <summary>
        /// Validates MAC address format
        /// </summary>
        private static bool IsValidMacAddress(string macAddress)
        {
            if (string.IsNullOrWhiteSpace(macAddress))
                return false;

            var parts = macAddress.Split(':');
            if (parts.Length != 6)
                return false;

            foreach (var part in parts)
            {
                if (part.Length != 2 || !int.TryParse(part, System.Globalization.NumberStyles.HexNumber, null, out var value) || value > 255)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a clone of this interface
        /// </summary>
        public NetworkInterface Clone()
        {
            return new NetworkInterface
            {
                Name = Name,
                Type = Type,
                IpAddress = IpAddress,
                SubnetMask = SubnetMask,
                MacAddress = MacAddress,
                Status = Status,
                Speed = Speed,
                IsUp = IsUp,
                Description = Description,
                Mtu = Mtu,
                IsShutdown = IsShutdown
            };
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Returns a string representation of the interface
        /// </summary>
        public override string ToString()
        {
            var status = IsUp ? "up" : "down";
            var config = HasIpConfiguration ? $"{IpAddress}/{SubnetMask}" : "no ip address";
            return $"{Name} - {config} - {status}";
        }
    }
}
