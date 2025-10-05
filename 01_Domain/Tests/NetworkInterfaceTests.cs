using Xunit;
using Yanets.Core.Models;
using Yanets.SharedKernel;

namespace Yanets.Core.Tests
{
    public class NetworkInterfaceTests
    {
        [Fact]
        public void NetworkInterface_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var interfaceObj = new NetworkInterface();

            // Assert
            Assert.Empty(interfaceObj.Name);
            Assert.Equal(InterfaceType.Ethernet, interfaceObj.Type);
            Assert.Empty(interfaceObj.IpAddress);
            Assert.Empty(interfaceObj.SubnetMask);
            Assert.Empty(interfaceObj.MacAddress);
            Assert.Equal(InterfaceStatus.Down, interfaceObj.Status);
            Assert.Equal(10, interfaceObj.Speed); // Ethernet default is 10 Mbps
            Assert.False(interfaceObj.IsUp);
            Assert.Equal(1500, interfaceObj.Mtu);
            Assert.False(interfaceObj.IsShutdown);
        }

        [Fact]
        public void SetIpConfiguration_ShouldSetIpAndSubnetMask()
        {
            // Arrange
            var interfaceObj = new NetworkInterface();

            // Act
            interfaceObj.SetIpConfiguration("192.168.1.1", "255.255.255.0");

            // Assert
            Assert.Equal("192.168.1.1", interfaceObj.IpAddress);
            Assert.Equal("255.255.255.0", interfaceObj.SubnetMask);
            Assert.True(interfaceObj.HasIpConfiguration);
        }

        [Fact]
        public void SetIpConfiguration_ShouldThrowException_ForInvalidIpAddress()
        {
            // Arrange
            var interfaceObj = new NetworkInterface();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => interfaceObj.SetIpConfiguration("invalid-ip", "255.255.255.0"));
        }

        [Fact]
        public void SetIpConfiguration_ShouldThrowException_ForInvalidSubnetMask()
        {
            // Arrange
            var interfaceObj = new NetworkInterface();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => interfaceObj.SetIpConfiguration("192.168.1.1", "invalid-mask"));
        }

        [Fact]
        public void GenerateMacAddress_ShouldGenerateValidMacAddress()
        {
            // Arrange
            var interfaceObj = new NetworkInterface();

            // Act
            interfaceObj.GenerateMacAddress();

            // Assert
            Assert.NotEmpty(interfaceObj.MacAddress);
            Assert.Equal(17, interfaceObj.MacAddress.Length); // XX:XX:XX:XX:XX:XX format
            Assert.Contains(":", interfaceObj.MacAddress);

            // Validate MAC address format
            var parts = interfaceObj.MacAddress.Split(':');
            Assert.Equal(6, parts.Length);
            foreach (var part in parts)
            {
                Assert.Equal(2, part.Length);
                Assert.True(int.TryParse(part, System.Globalization.NumberStyles.HexNumber, null, out _));
            }
        }

        [Fact]
        public void NetworkAddress_ShouldCalculateCorrectNetworkAddress()
        {
            // Arrange
            var interfaceObj = new NetworkInterface();
            interfaceObj.SetIpConfiguration("192.168.1.100", "255.255.255.0");

            // Act
            var networkAddress = interfaceObj.NetworkAddress;

            // Assert
            Assert.Equal("192.168.1.0", networkAddress);
        }

        [Fact]
        public void BroadcastAddress_ShouldCalculateCorrectBroadcastAddress()
        {
            // Arrange
            var interfaceObj = new NetworkInterface();
            interfaceObj.SetIpConfiguration("192.168.1.100", "255.255.255.0");

            // Act
            var broadcastAddress = interfaceObj.BroadcastAddress;

            // Assert
            Assert.Equal("192.168.1.255", broadcastAddress);
        }

        [Fact]
        public void IsValid_ShouldReturnFalse_ForInvalidInterface()
        {
            // Arrange
            var interfaceObj = new NetworkInterface(); // Name is empty

            // Act
            var result = interfaceObj.IsValid();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_ForValidInterface()
        {
            // Arrange
            var interfaceObj = new NetworkInterface
            {
                Name = "Ethernet0",
                IpAddress = "192.168.1.1",
                SubnetMask = "255.255.255.0",
                MacAddress = "AA:BB:CC:DD:EE:FF",
                Mtu = 1500
            };

            // Act
            var result = interfaceObj.IsValid();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_ShouldReturnFalse_ForInvalidIpAddress()
        {
            // Arrange
            var interfaceObj = new NetworkInterface
            {
                Name = "Ethernet0",
                IpAddress = "invalid-ip",
                SubnetMask = "255.255.255.0"
            };

            // Act
            var result = interfaceObj.IsValid();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValid_ShouldReturnFalse_ForInvalidMacAddress()
        {
            // Arrange
            var interfaceObj = new NetworkInterface
            {
                Name = "Ethernet0",
                MacAddress = "invalid-mac"
            };

            // Act
            var result = interfaceObj.IsValid();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Clone_ShouldCreateDeepCopy()
        {
            // Arrange
            var interfaceObj = new NetworkInterface
            {
                Name = "Ethernet0",
                IpAddress = "192.168.1.1",
                SubnetMask = "255.255.255.0",
                MacAddress = "AA:BB:CC:DD:EE:FF",
                Speed = 1000,
                IsUp = true,
                Description = "Test interface"
            };

            // Act
            var clone = interfaceObj.Clone();

            // Assert
            Assert.Equal(interfaceObj.Name, clone.Name);
            Assert.Equal(interfaceObj.IpAddress, clone.IpAddress);
            Assert.Equal(interfaceObj.SubnetMask, clone.SubnetMask);
            Assert.Equal(interfaceObj.MacAddress, clone.MacAddress);
            Assert.Equal(interfaceObj.Speed, clone.Speed);
            Assert.Equal(interfaceObj.IsUp, clone.IsUp);
            Assert.Equal(interfaceObj.Description, clone.Description);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var interfaceObj = new NetworkInterface
            {
                Name = "Ethernet0",
                IpAddress = "192.168.1.1",
                SubnetMask = "255.255.255.0",
                IsUp = true
            };

            // Act
            var result = interfaceObj.ToString();

            // Assert
            Assert.Contains("Ethernet0", result);
            Assert.Contains("192.168.1.1/255.255.255.0", result);
            Assert.Contains("up", result);
        }
    }
}
