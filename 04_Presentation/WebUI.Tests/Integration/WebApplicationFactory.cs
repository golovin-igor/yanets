using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Application.Services;
using Yanets.Application.Services.Vendors;
using System.Linq;

namespace Yanets.WebUI.Tests.Integration
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace real services with test implementations
                services.AddScoped<ITopologyService, TestTopologyService>();
                services.AddScoped<ICommandParser>(sp =>
                    new Application.Services.CommandParser());
                services.AddScoped<IMibProvider>(sp =>
                    new Application.Services.MibProvider());
                services.AddScoped<IPromptGenerator>(sp =>
                    new Application.Services.PromptGenerator());

                // Remove the problematic IDeviceSimulator registration that requires NetworkDevice
                var deviceSimulatorDescriptor = services.FirstOrDefault(s =>
                    s.ServiceType == typeof(Application.Services.IDeviceSimulator));
                if (deviceSimulatorDescriptor != null)
                {
                    services.Remove(deviceSimulatorDescriptor);
                }

                // Add test logger
                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                });

                // Configure JSON serialization to handle Func delegates
                services.Configure<JsonOptions>(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new FuncConverter());
                });
            });
        }
    }

    /// <summary>
    /// Custom JSON converter to handle Func delegates during serialization
    /// </summary>
    public class FuncConverter : JsonConverter<object>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(Delegate).IsAssignableFrom(typeToConvert);
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // For deserialization, we'll skip Func delegates
            reader.Skip();
            return null!;
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            // For serialization, we'll write a placeholder or skip
            writer.WriteStringValue("[Delegate]");
        }
    }

    /// <summary>
    /// Test implementation of topology service for integration testing
    /// </summary>
    public class TestTopologyService : ITopologyService
    {
        private readonly List<NetworkTopology> _topologies;

        public TestTopologyService()
        {
            _topologies = new List<NetworkTopology>();
        }

        public NetworkTopology? GetTopology(Guid id)
        {
            return _topologies.FirstOrDefault(t => t.Id == id);
        }

        public IEnumerable<NetworkTopology> GetAllTopologies()
        {
            return _topologies;
        }

        public void SaveTopology(NetworkTopology topology)
        {
            var existing = _topologies.FirstOrDefault(t => t.Id == topology.Id);
            if (existing != null)
            {
                _topologies.Remove(existing);
            }
            _topologies.Add(topology);
        }

        public void DeleteTopology(Guid id)
        {
            var topology = _topologies.FirstOrDefault(t => t.Id == id);
            if (topology != null)
            {
                _topologies.Remove(topology);
            }
        }

        public Task StartTopologySimulationAsync(Guid topologyId)
        {
            return Task.CompletedTask;
        }

        public Task StopTopologySimulationAsync(Guid topologyId)
        {
            return Task.CompletedTask;
        }

        public bool IsTopologyRunning(Guid topologyId)
        {
            return false;
        }
    }
}
