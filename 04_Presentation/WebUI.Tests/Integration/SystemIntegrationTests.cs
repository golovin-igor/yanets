using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Yanets.Core.Models;
using Yanets.SharedKernel;

namespace Yanets.WebUI.Tests.Integration
{
    public class SystemIntegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _httpClient;
        private readonly TestWebApplicationFactory _factory;

        public SystemIntegrationTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _httpClient = factory.CreateClient();
        }

        [Fact]
        public async Task EndToEndTopologyWorkflow_ShouldWorkCompletely()
        {
            // 1. Create a topology via API
            var topologyRequest = new
            {
                Name = "E2E Test Topology",
                Description = "End-to-end integration test"
            };

            var topologyResponse = await _httpClient.PostAsJsonAsync("/api/topology", topologyRequest);
            topologyResponse.EnsureSuccessStatusCode();
            var topology = await topologyResponse.Content.ReadFromJsonAsync<NetworkTopology>();

            // 2. Add a device via API
            var deviceRequest = new
            {
                Name = "E2E Router",
                Hostname = "e2e-router",
                VendorName = "Cisco",
                PositionX = 100,
                PositionY = 100
            };

            var deviceResponse = await _httpClient.PostAsJsonAsync($"/api/topology/{topology!.Id}/devices", deviceRequest);
            deviceResponse.EnsureSuccessStatusCode();

            // 3. Verify topology contains device
            var devicesResponse = await _httpClient.GetAsync($"/api/topology/{topology.Id}/devices");
            devicesResponse.EnsureSuccessStatusCode();
            var devices = await devicesResponse.Content.ReadFromJsonAsync<NetworkDevice[]>();
            Assert.NotNull(devices);
            Assert.Single(devices);
            Assert.Equal("E2E Router", devices[0].Name);
            Assert.Equal("e2e-router", devices[0].Hostname);

            // 4. Test CLI connectivity (simplified)
            var device = devices[0];
            var telnetPort = GetTelnetPort(device);
            await TestTelnetConnection(device.Hostname, telnetPort);

            // 5. Test SNMP connectivity (simplified)
            var snmpPort = GetSnmpPort(device);
            await TestSnmpConnection(device.Hostname, snmpPort);

            // 6. Cleanup - Delete topology
            var deleteResponse = await _httpClient.DeleteAsync($"/api/topology/{topology.Id}");
            deleteResponse.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task MultiDeviceTopology_ShouldHandleMultipleDevices()
        {
            // Create topology
            var topologyRequest = new { Name = "Multi-Device Topology", Description = "Testing multiple devices" };
            var topologyResponse = await _httpClient.PostAsJsonAsync("/api/topology", topologyRequest);
            var topology = await topologyResponse.Content.ReadFromJsonAsync<NetworkTopology>();

            // Add multiple devices
            var devices = new[]
            {
                new { Name = "Router1", Hostname = "router1", VendorName = "Cisco", PositionX = 100, PositionY = 100 },
                new { Name = "Router2", Hostname = "router2", VendorName = "Cisco", PositionX = 200, PositionY = 100 },
                new { Name = "Switch1", Hostname = "switch1", VendorName = "Cisco", PositionX = 150, PositionY = 200 }
            };

            foreach (var deviceRequest in devices)
            {
                var response = await _httpClient.PostAsJsonAsync($"/api/topology/{topology!.Id}/devices", deviceRequest);
                response.EnsureSuccessStatusCode();
            }

            // Verify all devices are present
            var devicesResponse = await _httpClient.GetAsync($"/api/topology/{topology!.Id}/devices");
            var retrievedDevices = await devicesResponse.Content.ReadFromJsonAsync<NetworkDevice[]>();
            Assert.NotNull(retrievedDevices);
            Assert.Equal(3, retrievedDevices.Length);

            // Test connectivity for each device
            foreach (var device in retrievedDevices)
            {
                var telnetPort = GetTelnetPort(device);
                var snmpPort = GetSnmpPort(device);

                await TestTelnetConnection(device.Hostname, telnetPort);
                await TestSnmpConnection(device.Hostname, snmpPort);
            }

            // Cleanup
            await _httpClient.DeleteAsync($"/api/topology/{topology.Id}");
        }

        [Fact]
        public async Task JuniperDevice_ShouldWorkWithDifferentVendor()
        {
            // Create topology
            var topologyRequest = new { Name = "Juniper Topology", Description = "Testing Juniper vendor" };
            var topologyResponse = await _httpClient.PostAsJsonAsync("/api/topology", topologyRequest);
            var topology = await topologyResponse.Content.ReadFromJsonAsync<NetworkTopology>();

            // Add Juniper device
            var deviceRequest = new
            {
                Name = "Juniper Router",
                Hostname = "juniper-router",
                VendorName = "Juniper",
                PositionX = 100,
                PositionY = 100
            };

            var deviceResponse = await _httpClient.PostAsJsonAsync($"/api/topology/{topology!.Id}/devices", deviceRequest);
            deviceResponse.EnsureSuccessStatusCode();

            var device = await deviceResponse.Content.ReadFromJsonAsync<NetworkDevice>();
            Assert.Equal(DeviceType.Router, device!.Type);

            // Test connectivity
            var telnetPort = GetTelnetPort(device);
            var snmpPort = GetSnmpPort(device);

            await TestTelnetConnection(device.Hostname, telnetPort);
            await TestSnmpConnection(device.Hostname, snmpPort);

            // Cleanup
            await _httpClient.DeleteAsync($"/api/topology/{topology.Id}");
        }

        [Fact]
        public async Task ConcurrentOperations_ShouldHandleMultipleRequests()
        {
            // Create multiple topologies concurrently
            var topologyTasks = Enumerable.Range(1, 5).Select(i =>
                _httpClient.PostAsJsonAsync("/api/topology", new
                {
                    Name = $"Concurrent Topology {i}",
                    Description = $"Created concurrently {i}"
                })
            ).ToArray();

            var topologyResponses = await Task.WhenAll(topologyTasks);

            // Verify all topologies were created
            foreach (var response in topologyResponses)
            {
                response.EnsureSuccessStatusCode();
            }

            var topologies = await Task.WhenAll(
                topologyResponses.Select(r => r.Content.ReadFromJsonAsync<NetworkTopology>())
            );

            Assert.Equal(5, topologies.Length);

            // Test concurrent device creation
            var deviceTasks = topologies.Select((topology, index) =>
                _httpClient.PostAsJsonAsync($"/api/topology/{topology!.Id}/devices", new
                {
                    Name = $"Device {index + 1}",
                    Hostname = $"device{index + 1}",
                    VendorName = "Cisco",
                    PositionX = index * 50,
                    PositionY = index * 50
                })
            );

            var deviceResponses = await Task.WhenAll(deviceTasks);
            foreach (var response in deviceResponses)
            {
                response.EnsureSuccessStatusCode();
            }

            // Cleanup all topologies
            var cleanupTasks = topologies.Select(t => _httpClient.DeleteAsync($"/api/topology/{t!.Id}"));
            await Task.WhenAll(cleanupTasks);
        }

        private int GetTelnetPort(NetworkDevice device)
        {
            // Use device ID hash to distribute ports
            return 23000 + (Math.Abs(device.Id.GetHashCode()) % 1000);
        }

        private int GetSnmpPort(NetworkDevice device)
        {
            // Use device ID hash to distribute ports
            return 16100 + (Math.Abs(device.Id.GetHashCode()) % 1000);
        }

        private async Task TestTelnetConnection(string hostname, int port)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync("localhost", port);

                // Send a simple command and expect a response
                using var stream = client.GetStream();
                var buffer = new byte[1024];

                // Send "show version" command
                var command = "show version\r\n";
                var commandBytes = Encoding.ASCII.GetBytes(command);
                await stream.WriteAsync(commandBytes);

                // Read response (simplified)
                var bytesRead = await stream.ReadAsync(buffer);
                var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // Should contain hostname or device info
                Assert.Contains(hostname, response, StringComparison.OrdinalIgnoreCase);

                client.Close();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Telnet connection test failed: {ex.Message}");
            }
        }

        private async Task TestSnmpConnection(string hostname, int port)
        {
            try
            {
                // For integration testing, we'll just test that the port is accessible
                using var client = new TcpClient();
                await client.ConnectAsync("localhost", port);
                client.Close();
            }
            catch (Exception ex)
            {
                Assert.Fail($"SNMP connection test failed: {ex.Message}");
            }
        }
    }
}
