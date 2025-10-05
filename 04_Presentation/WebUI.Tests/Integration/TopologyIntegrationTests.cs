using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.SharedKernel;

namespace Yanets.WebUI.Tests.Integration
{
    public class TopologyIntegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        public TopologyIntegrationTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task HealthCheck_ShouldReturnHealthy()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var healthResponse = JsonSerializer.Deserialize<JsonElement>(content);

            Assert.Equal("Healthy", healthResponse.GetProperty("status").GetString());
        }

        [Fact]
        public async Task GetTopologies_EmptyList_ShouldReturnEmptyArray()
        {
            // Act
            var response = await _client.GetAsync("/api/topology");

            // Assert
            response.EnsureSuccessStatusCode();
            var topologies = await response.Content.ReadFromJsonAsync<NetworkTopology[]>();
            Assert.NotNull(topologies);
            Assert.Empty(topologies);
        }

        [Fact]
        public async Task CreateTopology_ShouldReturnCreatedTopology()
        {
            // Arrange
            var createRequest = new
            {
                Name = "Test Topology",
                Description = "Integration test topology"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/topology", createRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var createdTopology = await response.Content.ReadFromJsonAsync<NetworkTopology>();
            Assert.NotNull(createdTopology);
            Assert.Equal("Test Topology", createdTopology.Name);
            Assert.Equal("Integration test topology", createdTopology.Description);
            Assert.NotEqual(Guid.Empty, createdTopology.Id);
        }

        [Fact]
        public async Task CreateTopology_And_GetTopology_ShouldReturnSameTopology()
        {
            // Arrange
            var createRequest = new
            {
                Name = "Test Topology 2",
                Description = "Second integration test topology"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/topology", createRequest);
            var createdTopology = await createResponse.Content.ReadFromJsonAsync<NetworkTopology>();
            var topologyId = createdTopology!.Id;

            // Act
            var getResponse = await _client.GetAsync($"/api/topology/{topologyId}");

            // Assert
            getResponse.EnsureSuccessStatusCode();
            var retrievedTopology = await getResponse.Content.ReadFromJsonAsync<NetworkTopology>();
            Assert.NotNull(retrievedTopology);
            Assert.Equal(createdTopology.Id, retrievedTopology.Id);
            Assert.Equal(createdTopology.Name, retrievedTopology.Name);
            Assert.Equal(createdTopology.Description, retrievedTopology.Description);
        }

        [Fact]
        public async Task CreateTopology_WithDevices_ShouldWork()
        {
            // Arrange - Create topology first
            var createTopologyRequest = new
            {
                Name = "Topology with Devices",
                Description = "Testing device creation"
            };

            var createTopologyResponse = await _client.PostAsJsonAsync("/api/topology", createTopologyRequest);
            var createdTopology = await createTopologyResponse.Content.ReadFromJsonAsync<NetworkTopology>();
            var topologyId = createdTopology!.Id;

            // Arrange - Create device
            var createDeviceRequest = new
            {
                Name = "Test Router",
                Hostname = "test-router",
                VendorName = "Cisco",
                PositionX = 100,
                PositionY = 200
            };

            // Act
            var createDeviceResponse = await _client.PostAsJsonAsync($"/api/topology/{topologyId}/devices", createDeviceRequest);

            // Assert
            createDeviceResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, createDeviceResponse.StatusCode);

            var createdDevice = await createDeviceResponse.Content.ReadFromJsonAsync<NetworkDevice>();
            Assert.NotNull(createdDevice);
            Assert.Equal("Test Router", createdDevice.Name);
            Assert.Equal("test-router", createdDevice.Hostname);
            Assert.Equal(DeviceType.Router, createdDevice.Type);
        }

        [Fact]
        public async Task GetDevices_ShouldReturnDevicesInTopology()
        {
            // Arrange - Create topology and device
            var topologyRequest = new { Name = "Device Test Topology", Description = "Testing device retrieval" };
            var topologyResponse = await _client.PostAsJsonAsync("/api/topology", topologyRequest);
            var topology = await topologyResponse.Content.ReadFromJsonAsync<NetworkTopology>();

            var deviceRequest = new
            {
                Name = "Test Switch",
                Hostname = "test-switch",
                VendorName = "Cisco",
                PositionX = 50,
                PositionY = 150
            };

            await _client.PostAsJsonAsync($"/api/topology/{topology!.Id}/devices", deviceRequest);

            // Act
            var devicesResponse = await _client.GetAsync($"/api/topology/{topology.Id}/devices");

            // Assert
            devicesResponse.EnsureSuccessStatusCode();
            var devices = await devicesResponse.Content.ReadFromJsonAsync<NetworkDevice[]>();
            Assert.NotNull(devices);
            Assert.Single(devices);
            Assert.Equal("Test Switch", devices[0].Name);
            Assert.Equal("test-switch", devices[0].Hostname);
        }

        [Fact]
        public async Task UpdateTopology_ShouldModifyTopology()
        {
            // Arrange - Create topology
            var createRequest = new { Name = "Original Name", Description = "Original Description" };
            var createResponse = await _client.PostAsJsonAsync("/api/topology", createRequest);
            var topology = await createResponse.Content.ReadFromJsonAsync<NetworkTopology>();

            // Arrange - Update request
            var updateRequest = new
            {
                Name = "Updated Name",
                Description = "Updated Description"
            };

            // Act
            var updateResponse = await _client.PutAsJsonAsync($"/api/topology/{topology!.Id}", updateRequest);

            // Assert
            updateResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            // Verify update
            var getResponse = await _client.GetAsync($"/api/topology/{topology.Id}");
            var updatedTopology = await getResponse.Content.ReadFromJsonAsync<NetworkTopology>();
            Assert.Equal("Updated Name", updatedTopology!.Name);
            Assert.Equal("Updated Description", updatedTopology.Description);
        }

        [Fact]
        public async Task DeleteTopology_ShouldRemoveTopology()
        {
            // Arrange - Create topology
            var createRequest = new { Name = "To Be Deleted", Description = "Will be deleted" };
            var createResponse = await _client.PostAsJsonAsync("/api/topology", createRequest);
            var topology = await createResponse.Content.ReadFromJsonAsync<NetworkTopology>();

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/topology/{topology!.Id}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/topology/{topology.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task GetNonExistentTopology_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/topology/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateDevice_InNonExistentTopology_ShouldReturnNotFound()
        {
            // Arrange
            var deviceRequest = new
            {
                Name = "Test Device",
                Hostname = "test-device",
                VendorName = "Cisco",
                PositionX = 100,
                PositionY = 100
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/topology/{Guid.NewGuid()}/devices", deviceRequest);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
