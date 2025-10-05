using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Yanets.Core.Models;

namespace Yanets.WebUI.Tests.Integration
{
    public class PerformanceTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        public PerformanceTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ApiResponseTime_ShouldBeUnder100ms()
        {
            // Arrange
            var topologyRequest = new
            {
                Name = "Performance Test Topology",
                Description = "Testing API response times"
            };

            // Act
            var stopwatch = Stopwatch.StartNew();
            var response = await _client.PostAsJsonAsync("/api/topology", topologyRequest);
            stopwatch.Stop();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.True(stopwatch.ElapsedMilliseconds < 100, $"API response time {stopwatch.ElapsedMilliseconds}ms exceeded 100ms threshold");
        }

        [Fact]
        public async Task MultipleTopologyOperations_ShouldHandleLoad()
        {
            // Arrange
            var operations = new List<Task<HttpResponseMessage>>();
            var stopwatch = Stopwatch.StartNew();

            // Create 10 topologies concurrently
            for (int i = 0; i < 10; i++)
            {
                var request = new
                {
                    Name = $"Load Test Topology {i}",
                    Description = $"Performance testing topology {i}"
                };

                operations.Add(_client.PostAsJsonAsync("/api/topology", request));
            }

            // Act
            var responses = await Task.WhenAll(operations);
            stopwatch.Stop();

            // Assert
            foreach (var response in responses)
            {
                response.EnsureSuccessStatusCode();
            }

            // Should complete all operations within reasonable time
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"10 topology creations took {stopwatch.ElapsedMilliseconds}ms, exceeded 5s threshold");

            // Cleanup
            var topologies = await Task.WhenAll(
                responses.Select(r => r.Content.ReadFromJsonAsync<NetworkTopology>())
            );

            var cleanupTasks = topologies.Select(t => _client.DeleteAsync($"/api/topology/{t!.Id}"));
            await Task.WhenAll(cleanupTasks);
        }

        [Fact]
        public async Task DeviceCreationPerformance_ShouldScale()
        {
            // Arrange - Create a topology first
            var topologyRequest = new { Name = "Device Performance Test", Description = "Testing device creation performance" };
            var topologyResponse = await _client.PostAsJsonAsync("/api/topology", topologyRequest);
            var topology = await topologyResponse.Content.ReadFromJsonAsync<NetworkTopology>();

            var deviceOperations = new List<Task<HttpResponseMessage>>();
            var stopwatch = Stopwatch.StartNew();

            // Create 20 devices
            for (int i = 0; i < 20; i++)
            {
                var deviceRequest = new
                {
                    Name = $"Performance Device {i}",
                    Hostname = $"perf-device-{i}",
                    VendorName = "Cisco",
                    PositionX = i * 10,
                    PositionY = i * 10
                };

                deviceOperations.Add(_client.PostAsJsonAsync($"/api/topology/{topology!.Id}/devices", deviceRequest));
            }

            // Act
            var deviceResponses = await Task.WhenAll(deviceOperations);
            stopwatch.Stop();

            // Assert
            foreach (var response in deviceResponses)
            {
                response.EnsureSuccessStatusCode();
            }

            // Should complete device creation within reasonable time
            Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"20 device creations took {stopwatch.ElapsedMilliseconds}ms, exceeded 3s threshold");

            // Verify all devices were created
            var devicesResponse = await _client.GetAsync($"/api/topology/{topology!.Id}/devices");
            var devices = await devicesResponse.Content.ReadFromJsonAsync<NetworkDevice[]>();
            Assert.NotNull(devices);
            Assert.Equal(20, devices.Length);

            // Cleanup
            await _client.DeleteAsync($"/api/topology/{topology.Id}");
        }

        [Fact]
        public async Task ConcurrentReadOperations_ShouldNotInterfere()
        {
            // Arrange - Create multiple topologies
            var topologyTasks = Enumerable.Range(1, 5).Select(i =>
                _client.PostAsJsonAsync("/api/topology", new
                {
                    Name = $"Concurrent Read Test {i}",
                    Description = $"Testing concurrent reads {i}"
                })
            );

            var topologyResponses = await Task.WhenAll(topologyTasks);
            var topologies = await Task.WhenAll(
                topologyResponses.Select(r => r.Content.ReadFromJsonAsync<NetworkTopology>())
            );

            // Act - Perform concurrent reads
            var readTasks = topologies.Select(topology =>
                _client.GetAsync($"/api/topology/{topology!.Id}")
            );

            var readResponses = await Task.WhenAll(readTasks);
            var stopwatch = Stopwatch.StartNew();

            // Assert
            foreach (var response in readResponses)
            {
                response.EnsureSuccessStatusCode();
            }

            stopwatch.Stop();

            // Should handle concurrent reads efficiently
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"5 concurrent reads took {stopwatch.ElapsedMilliseconds}ms, exceeded 1s threshold");

            // Cleanup
            var cleanupTasks = topologies.Select(t => _client.DeleteAsync($"/api/topology/{t!.Id}"));
            await Task.WhenAll(cleanupTasks);
        }

        [Fact]
        public async Task MemoryUsage_ShouldRemainStable()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(false);
            var operations = new List<Task>();

            // Perform memory-intensive operations
            for (int i = 0; i < 50; i++)
            {
                operations.Add(CreateAndDeleteTopology(i));
            }

            // Act
            await Task.WhenAll(operations);

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncrease = finalMemory - initialMemory;

            // Assert - Memory increase should be reasonable (less than 50MB)
            Assert.True(memoryIncrease < 50 * 1024 * 1024, $"Memory increased by {memoryIncrease} bytes, exceeded 50MB threshold");
        }

        private async Task CreateAndDeleteTopology(int index)
        {
            // Create topology
            var topologyRequest = new
            {
                Name = $"Memory Test Topology {index}",
                Description = $"Testing memory usage {index}"
            };

            var topologyResponse = await _client.PostAsJsonAsync("/api/topology", topologyRequest);
            var topology = await topologyResponse.Content.ReadFromJsonAsync<NetworkTopology>();

            // Add some devices
            for (int i = 0; i < 3; i++)
            {
                var deviceRequest = new
                {
                    Name = $"Memory Device {index}-{i}",
                    Hostname = $"mem-device-{index}-{i}",
                    VendorName = "Cisco",
                    PositionX = index * 10,
                    PositionY = i * 10
                };

                await _client.PostAsJsonAsync($"/api/topology/{topology!.Id}/devices", deviceRequest);
            }

            // Delete topology
            await _client.DeleteAsync($"/api/topology/{topology!.Id}");
        }

        [Fact]
        public async Task ErrorHandling_ShouldReturnAppropriateStatusCodes()
        {
            // Test 404 for non-existent topology
            var notFoundResponse = await _client.GetAsync($"/api/topology/{Guid.NewGuid()}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, notFoundResponse.StatusCode);

            // Test 404 for device in non-existent topology
            var deviceNotFoundResponse = await _client.PostAsJsonAsync($"/api/topology/{Guid.NewGuid()}/devices", new
            {
                Name = "Test Device",
                Hostname = "test-device",
                VendorName = "Cisco",
                PositionX = 100,
                PositionY = 100
            });
            Assert.Equal(System.Net.HttpStatusCode.NotFound, deviceNotFoundResponse.StatusCode);

            // Test 400 for invalid topology data
            var invalidResponse = await _client.PostAsJsonAsync("/api/topology", new
            {
                // Missing required Name field
                Description = "Invalid topology"
            });

            // Should return 400 Bad Request or similar validation error
            Assert.True(
                invalidResponse.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                invalidResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity,
                $"Expected 400 or 422, got {invalidResponse.StatusCode}"
            );
        }
    }
}
