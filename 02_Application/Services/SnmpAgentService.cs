using System.Net;
using System.Net.Sockets;
using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Core.Snmp;

namespace Yanets.Application.Services
{
    /// <summary>
    /// Service for managing SNMP agent connections (UDP)
    /// </summary>
    public class SnmpAgentService
    {
        private readonly Dictionary<Guid, UdpClient> _deviceAgents;
        private readonly ITopologyService _topologyService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SnmpAgentService> _logger;
        private bool _isRunning;

        public SnmpAgentService(
            ITopologyService topologyService,
            IServiceProvider serviceProvider,
            ILogger<SnmpAgentService> logger)
        {
            _deviceAgents = new Dictionary<Guid, UdpClient>();
            _topologyService = topologyService ?? throw new ArgumentNullException(nameof(topologyService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _isRunning = false;
        }

        public bool IsRunning => _isRunning;

        /// <summary>
        /// Starts the SNMP agent for a specific device
        /// </summary>
        public void StartSnmpAgent(NetworkDevice device, int port = 161)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            if (_deviceAgents.ContainsKey(device.Id))
                throw new InvalidOperationException($"SNMP agent already running for device {device.Id}");

            try
            {
                var agent = new UdpClient(port);
                _deviceAgents[device.Id] = agent;

                _logger.LogInformation("Started SNMP agent for device {DeviceId} on port {Port}", device.Id, port);

                Task.Run(() => ListenForSnmpRequests(device, agent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start SNMP agent for device {DeviceId}", device.Id);
                throw;
            }
        }

        /// <summary>
        /// Stops the SNMP agent for a specific device
        /// </summary>
        public void StopSnmpAgent(Guid deviceId)
        {
            if (_deviceAgents.TryGetValue(deviceId, out var agent))
            {
                try
                {
                    agent.Close();
                    _deviceAgents.Remove(deviceId);
                    _logger.LogInformation("Stopped SNMP agent for device {DeviceId}", deviceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping SNMP agent for device {DeviceId}", deviceId);
                }
            }
        }

        /// <summary>
        /// Stops all SNMP agents
        /// </summary>
        public void StopAllAgents()
        {
            foreach (var deviceId in _deviceAgents.Keys.ToList())
            {
                StopSnmpAgent(deviceId);
            }
        }

        private async Task ListenForSnmpRequests(NetworkDevice device, UdpClient agent)
        {
            while (_isRunning)
            {
                try
                {
                    var receiveResult = await agent.ReceiveAsync();
                    _logger.LogDebug("Received SNMP request for device {DeviceId} from {RemoteEndPoint}",
                        device.Id, receiveResult.RemoteEndPoint);

                    _ = Task.Run(() => ProcessSnmpRequest(device, agent, receiveResult));
                }
                catch (ObjectDisposedException)
                {
                    // Agent was stopped
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error listening for SNMP requests for device {DeviceId}", device.Id);
                }
            }
        }

        private async Task ProcessSnmpRequest(
            NetworkDevice device,
            UdpClient agent,
            UdpReceiveResult receiveResult)
        {
            try
            {
                // Parse SNMP request (simplified for demo)
                var request = ParseSnmpRequest(receiveResult.Buffer);

                // Validate community string
                if (!ValidateCommunity(request.Community, device))
                {
                    _logger.LogWarning("Invalid community string from {RemoteEndPoint}", receiveResult.RemoteEndPoint);
                    return; // Silently drop as per SNMP standard
                }

                // Get device simulator
                var simulator = _serviceProvider.GetService<IDeviceSimulator>();
                if (simulator == null)
                {
                    _logger.LogError("Device simulator not available for device {DeviceId}", device.Id);
                    return;
                }

                // Handle request
                var response = await simulator.HandleSnmpRequest(request);

                // Encode and send response (simplified for demo)
                var responseBytes = EncodeSnmpResponse(response);
                await agent.SendAsync(responseBytes, responseBytes.Length, receiveResult.RemoteEndPoint);

                _logger.LogDebug("Sent SNMP response for device {DeviceId} to {RemoteEndPoint}",
                    device.Id, receiveResult.RemoteEndPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SNMP request for device {DeviceId}", device.Id);
            }
        }

        private SnmpRequest ParseSnmpRequest(byte[] buffer)
        {
            // Simplified SNMP parsing for demo purposes
            // In a real implementation, this would use a proper SNMP library

            var request = new SnmpRequest
            {
                RequestId = BitConverter.ToInt32(buffer, 4),
                Type = (SnmpRequestType)buffer[2],
                Community = Encoding.ASCII.GetString(buffer, 8, buffer.Length - 8).Trim('\0'),
                Version = (SnmpVersion)buffer[3],
                Oids = new List<string>()
            };

            // Extract OIDs from request (simplified)
            if (request.Type == SnmpRequestType.Get || request.Type == SnmpRequestType.GetNext)
            {
                // Parse OID list from buffer
                request.Oids.Add("1.3.6.1.2.1.1.1.0"); // sysDescr as example
            }

            return request;
        }

        private byte[] EncodeSnmpResponse(SnmpResponse response)
        {
            // Simplified SNMP encoding for demo purposes
            // In a real implementation, this would use a proper SNMP library

            var buffer = new List<byte>();

            // SNMP message header (simplified)
            buffer.AddRange(new byte[] { 0x30, 0x00 }); // Sequence
            buffer.AddRange(BitConverter.GetBytes(response.RequestId));
            buffer.Add((byte)SnmpRequestType.Response);
            buffer.Add((byte)SnmpVersion.V2c);

            // Community string
            var communityBytes = Encoding.ASCII.GetBytes("public");
            buffer.AddRange(communityBytes);

            // Response PDU (simplified)
            foreach (var varBind in response.VarBinds)
            {
                // Add OID and value (simplified encoding)
                buffer.AddRange(Encoding.ASCII.GetBytes(varBind.Oid));
                buffer.AddRange(new byte[] { 0x02, 0x01, 0x01 }); // Integer value 1
            }

            return buffer.ToArray();
        }

        private bool ValidateCommunity(string community, NetworkDevice device)
        {
            // Check against device configuration
            var snmpConfig = device.State?.Variables?.GetValueOrDefault("snmp_communities") as Dictionary<string, string>;

            return snmpConfig?.ContainsKey(community) ?? community == "public"; // Default read-only
        }
    }
}
