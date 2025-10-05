using System.Drawing;
using Xunit;
using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.SharedKernel;

namespace Yanets.Core.Tests
{
    public class NetworkDeviceTests
    {
        [Fact]
        public void NetworkDevice_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var device = new TestDevice();

            // Assert
            Assert.NotEqual(Guid.Empty, device.Id);
            Assert.Empty(device.Name);
            Assert.Empty(device.Hostname);
            Assert.Equal(DeviceType.Router, device.Type);
            Assert.NotNull(device.Interfaces);
            Assert.False(device.State.IsSimulationRunning);
        }

        [Fact]
        public void AddInterface_ShouldAddInterfaceToDevice()
        {
            // Arrange
            var device = new TestDevice();
            var initialCount = device.Interfaces.Count; // Device has default interfaces
            var interfaceObj = new NetworkInterface
            {
                Name = "Ethernet10", // Use a name that doesn't conflict with defaults
                Type = InterfaceType.Ethernet,
                Speed = 100
            };

            // Act
            device.AddInterface(interfaceObj);

            // Assert
            Assert.Equal(initialCount + 1, device.Interfaces.Count);
            Assert.Equal("Ethernet10", device.Interfaces.Last().Name);
        }

        [Fact]
        public void AddInterface_ShouldThrowException_WhenInterfaceIsNull()
        {
            // Arrange
            var device = new TestDevice();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => device.AddInterface(null!));
        }

        [Fact]
        public void AddInterface_ShouldThrowException_WhenInterfaceNameAlreadyExists()
        {
            // Arrange
            var device = new TestDevice();
            var interface1 = new NetworkInterface { Name = "Ethernet0" };
            var interface2 = new NetworkInterface { Name = "Ethernet0" };

            device.AddInterface(interface1);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => device.AddInterface(interface2));
        }

        [Fact]
        public void GetInterface_ShouldReturnCorrectInterface()
        {
            // Arrange
            var device = new TestDevice();
            var interfaceObj = new NetworkInterface { Name = "Ethernet0" };
            device.AddInterface(interfaceObj);

            // Act
            var result = device.GetInterface("Ethernet0");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Ethernet0", result.Name);
        }

        [Fact]
        public void GetUpInterfaces_ShouldReturnOnlyUpInterfaces()
        {
            // Arrange
            var device = new TestDevice();
            var initialUpCount = device.GetUpInterfaces().Count(); // Count existing up interfaces

            var upInterface = new NetworkInterface { Name = "Ethernet10", IsUp = true };
            var downInterface = new NetworkInterface { Name = "Ethernet11", IsUp = false };

            device.AddInterface(upInterface);
            device.AddInterface(downInterface);

            // Act
            var result = device.GetUpInterfaces();

            // Assert
            Assert.Equal(initialUpCount + 1, result.Count()); // Should have one more up interface
            Assert.Contains(result, i => i.Name == "Ethernet10");
        }

        [Fact]
        public void IsValid_ShouldReturnFalse_ForInvalidDevice()
        {
            // Arrange
            var device = new TestDevice
            {
                Name = "", // Invalid: empty name
                Hostname = "invalid-device",
                Vendor = new TestVendorProfile()
            };

            // Act
            var result = device.IsValid();

            // Assert
            Assert.False(result); // Name is empty
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_ForValidDevice()
        {
            // Arrange
            var device = new TestDevice
            {
                Name = "Test Device",
                Hostname = "test-device-valid",
                Vendor = new TestVendorProfile()
            };

            // Act
            var result = device.IsValid();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Clone_ShouldCreateDeepCopy()
        {
            // Arrange
            var device = new TestDevice
            {
                Name = "Test Device",
                Hostname = "test-device-clone",
                Position = new Point(100, 100)
            };

            var originalInterfaceCount = device.Interfaces.Count;

            // Act
            var clone = device.Clone();

            // Assert
            Assert.NotEqual(device.Id, clone.Id);
            Assert.Equal(device.Name, clone.Name);
            Assert.Equal(device.Hostname, clone.Hostname);
            Assert.Equal(device.Position.X, clone.Position.X);
            Assert.Equal(device.Position.Y, clone.Position.Y);
            Assert.Equal(originalInterfaceCount, clone.Interfaces.Count); // Same number of interfaces
            Assert.NotEqual(device.Interfaces.First().GetHashCode(), clone.Interfaces.First().GetHashCode());
        }

        // Test implementation of NetworkDevice for testing purposes
        private class TestDevice : NetworkDevice
        {
            public TestDevice() : base(DeviceType.Router, "TestVendor") { }

            public override bool IsValid() => base.IsValid();
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
    }
}
