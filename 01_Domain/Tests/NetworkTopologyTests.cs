using System.Drawing;
using Xunit;
using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.SharedKernel;

namespace Yanets.Core.Tests
{
    public class NetworkTopologyTests
    {
        [Fact]
        public void NetworkTopology_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var topology = new NetworkTopology();

            // Assert
            Assert.NotEqual(Guid.Empty, topology.Id);
            Assert.Empty(topology.Name);
            Assert.Empty(topology.Description);
            Assert.Empty(topology.Devices);
            Assert.Empty(topology.Connections);
            Assert.NotNull(topology.Metadata);
        }

        [Fact]
        public void AddDevice_ShouldAddDeviceToTopology()
        {
            // Arrange
            var topology = new NetworkTopology();
            var device = CreateTestDevice();

            // Act
            topology.AddDevice(device);

            // Assert
            Assert.Single(topology.Devices);
            Assert.Equal(device.Id, topology.Devices.First().Id);
        }

        [Fact]
        public void AddDevice_ShouldThrowException_WhenDeviceIsNull()
        {
            // Arrange
            var topology = new NetworkTopology();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => topology.AddDevice(null!));
        }

        [Fact]
        public void AddDevice_ShouldThrowException_WhenDeviceIdAlreadyExists()
        {
            // Arrange
            var topology = new NetworkTopology();
            var device1 = CreateTestDevice();
            var device2 = CreateTestDevice();
            device2.Id = device1.Id; // Same ID

            topology.AddDevice(device1);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => topology.AddDevice(device2));
        }

        [Fact]
        public void RemoveDevice_ShouldRemoveDeviceAndConnections()
        {
            // Arrange
            var topology = new NetworkTopology();
            var device1 = CreateTestDevice("device1");
            var device2 = CreateTestDevice("device2");
            var connection = new Connection
            {
                SourceDeviceId = device1.Id,
                TargetDeviceId = device2.Id,
                SourceInterface = "GigabitEthernet0/0", // Use existing interface name
                TargetInterface = "GigabitEthernet0/0"  // Use existing interface name
            };

            topology.AddDevice(device1);
            topology.AddDevice(device2);
            topology.AddConnection(connection);

            // Act
            var removed = topology.RemoveDevice(device1.Id);

            // Assert
            Assert.True(removed);
            Assert.Single(topology.Devices);
            Assert.Empty(topology.Connections);
        }

        [Fact]
        public void GetDevice_ShouldReturnCorrectDevice()
        {
            // Arrange
            var topology = new NetworkTopology();
            var device = CreateTestDevice();
            topology.AddDevice(device);

            // Act
            var result = topology.GetDevice(device.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(device.Id, result.Id);
        }

        [Fact]
        public void GetDeviceByHostname_ShouldReturnCorrectDevice()
        {
            // Arrange
            var topology = new NetworkTopology();
            var device = CreateTestDevice();
            topology.AddDevice(device);

            // Act
            var result = topology.GetDeviceByHostname(device.Hostname);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(device.Hostname, result.Hostname);
        }

        [Fact]
        public void IsValid_ShouldReturnFalse_ForInvalidTopology()
        {
            // Arrange
            var topology = new NetworkTopology();
            var device = CreateTestDevice("invalid-device");
            // Don't add device to topology, so it should be invalid

            // Act
            var result = topology.IsValid();

            // Assert
            Assert.False(result); // Empty topology should be invalid
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_ForValidTopology()
        {
            // Arrange
            var topology = new NetworkTopology();
            var device1 = CreateTestDevice("device1");
            var device2 = CreateTestDevice("device2");
            var connection = new Connection
            {
                SourceDeviceId = device1.Id,
                TargetDeviceId = device2.Id,
                SourceInterface = "GigabitEthernet0/0", // Use existing interface name
                TargetInterface = "GigabitEthernet0/0"  // Use existing interface name
            };


            topology.AddDevice(device1);
            topology.AddDevice(device2);
            topology.AddConnection(connection);

            // Act
            var result = topology.IsValid();

            // Assert
            Assert.True(result);
        }

        private static NetworkDevice CreateTestDevice(string hostname = "test-device")
        {
            var device = new TestDevice
            {
                Name = "Test Device",
                Hostname = hostname,
                Type = DeviceType.Router,
                Position = new Point(100, 100),
                Vendor = new TestVendorProfile()
            };
            return device;
        }

        // Test implementation of VendorProfile for testing purposes
        private class TestVendorProfile : VendorProfile
        {
            public override string VendorName => "TestVendor";
            public override string Os => "TestOS";
            public override string Version => "1.0";

            public override ICommandParser CommandParser => throw new NotImplementedException();
            public override IPromptGenerator PromptGenerator => throw new NotImplementedException();
            public override IMibProvider MibProvider => throw new NotImplementedException();
        }

        // Test implementation of NetworkDevice for testing purposes
        private class TestDevice : NetworkDevice
        {
            public TestDevice() : base(DeviceType.Router, "TestVendor") { }

            public override bool IsValid() => true;
        }
    }
}
