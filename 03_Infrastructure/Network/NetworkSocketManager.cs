using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Yanets.Core.Interfaces;
using Yanets.Core.Models;

namespace Yanets.Infrastructure.Network
{
    /// <summary>
    /// Manages network socket connections for all devices
    /// </summary>
    public class NetworkSocketManager
    {
        private readonly Dictionary<Guid, DeviceNetworkContext> _deviceContexts;
        private readonly ILogger<NetworkSocketManager> _logger;
        private bool _isRunning;

        public NetworkSocketManager(ILogger<NetworkSocketManager> logger)
        {
            _deviceContexts = new Dictionary<Guid, DeviceNetworkContext>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _isRunning = false;
        }

        public bool IsRunning => _isRunning;

        /// <summary>
        /// Starts all network services for a device
        /// </summary>
        public async Task StartAllServices(NetworkDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            var context = new DeviceNetworkContext
            {
                Device = device,
                ActiveSessions = new Dictionary<int, TcpClient>()
            };

            // Get management interface IP
            var managementIp = GetDeviceIpAddress(device);

            // Start Telnet (port 23)
            context.TelnetListener = new TcpListener(managementIp, 23);
            context.TelnetListener.Start();

            // Start SSH (port 22) - simplified for demo
            context.SshListener = new TcpListener(managementIp, 22);
            context.SshListener.Start();

            // Start SNMP (port 161)
            context.SnmpAgent = new UdpClient(new IPEndPoint(managementIp, 161));

            _deviceContexts[device.Id] = context;

            // Start listening tasks
            _ = Task.Run(() => AcceptTelnetConnections(context));
            _ = Task.Run(() => AcceptSshConnections(context));
            _ = Task.Run(() => HandleSnmpRequests(context));

            _logger.LogInformation("Started network services for device {DeviceId} at {ManagementIp}", device.Id, managementIp);
        }

        /// <summary>
        /// Stops all network services for a device
        /// </summary>
        public void StopAllServices(Guid deviceId)
        {
            if (_deviceContexts.TryGetValue(deviceId, out var context))
            {
                try
                {
                    context.TelnetListener?.Stop();
                    context.SshListener?.Stop();
                    context.SnmpAgent?.Close();

                    _deviceContexts.Remove(deviceId);
                    _logger.LogInformation("Stopped network services for device {DeviceId}", deviceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping network services for device {DeviceId}", deviceId);
                }
            }
        }

        /// <summary>
        /// Stops all network services
        /// </summary>
        public void StopAllServices()
        {
            foreach (var deviceId in _deviceContexts.Keys.ToList())
            {
                StopAllServices(deviceId);
            }
        }

        private IPAddress GetDeviceIpAddress(NetworkDevice device)
        {
            // Get management interface IP
            var mgmtInterface = device.Interfaces
                .FirstOrDefault(i => !string.IsNullOrEmpty(i.IpAddress));

            return mgmtInterface != null
                ? IPAddress.Parse(mgmtInterface.IpAddress)
                : IPAddress.Loopback;
        }

        private async Task AcceptTelnetConnections(DeviceNetworkContext context)
        {
            while (context.TelnetListener?.Server.IsBound == true)
            {
                try
                {
                    var client = await context.TelnetListener.AcceptTcpClientAsync();
                    var sessionId = Guid.NewGuid();
                    context.ActiveSessions[sessionId.GetHashCode()] = client;

                    _logger.LogInformation("Accepted Telnet connection for device {DeviceId} from {RemoteEndPoint}",
                        context.Device.Id, client.Client.RemoteEndPoint);

                    // Handle the connection (simplified for demo)
                    _ = Task.Run(() => HandleTelnetSession(context.Device, client));
                }
                catch (ObjectDisposedException)
                {
                    // Listener was stopped
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting Telnet connection for device {DeviceId}", context.Device.Id);
                }
            }
        }

        private async Task AcceptSshConnections(DeviceNetworkContext context)
        {
            while (context.SshListener?.Server.IsBound == true)
            {
                try
                {
                    var client = await context.SshListener.AcceptTcpClientAsync();
                    _logger.LogInformation("Accepted SSH connection for device {DeviceId} from {RemoteEndPoint}",
                        context.Device.Id, client.Client.RemoteEndPoint);

                    // SSH implementation would go here (simplified for demo)
                    client.Close();
                }
                catch (ObjectDisposedException)
                {
                    // Listener was stopped
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting SSH connection for device {DeviceId}", context.Device.Id);
                }
            }
        }

        private async Task HandleSnmpRequests(DeviceNetworkContext context)
        {
            while (context.SnmpAgent != null)
            {
                try
                {
                    var receiveResult = await context.SnmpAgent.ReceiveAsync();
                    _logger.LogDebug("Received SNMP request for device {DeviceId} from {RemoteEndPoint}",
                        context.Device.Id, receiveResult.RemoteEndPoint);

                    // Process SNMP request (simplified for demo)
                    _ = Task.Run(() => ProcessSnmpRequest(context.Device, context.SnmpAgent, receiveResult));
                }
                catch (ObjectDisposedException)
                {
                    // Agent was stopped
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling SNMP requests for device {DeviceId}", context.Device.Id);
                }
            }
        }

        private async Task HandleTelnetSession(NetworkDevice device, TcpClient client)
        {
            try
            {
                // Telnet session handling would be implemented here
                // For demo purposes, just close the connection
                await Task.Delay(100);
            }
            finally
            {
                client.Close();
            }
        }

        private async Task ProcessSnmpRequest(NetworkDevice device, UdpClient agent, UdpReceiveResult receiveResult)
        {
            try
            {
                // SNMP request processing would be implemented here
                // For demo purposes, just send a basic response
                var response = new byte[] { 0x30, 0x00 }; // Simplified SNMP response
                await agent.SendAsync(response, response.Length, receiveResult.RemoteEndPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SNMP request for device {DeviceId}", device.Id);
            }
        }
    }

    /// <summary>
    /// Network context for a single device
    /// </summary>
    public class DeviceNetworkContext
    {
        public NetworkDevice Device { get; set; } = null!;
        public TcpListener? TelnetListener { get; set; }
        public TcpListener? SshListener { get; set; }
        public UdpClient? SnmpAgent { get; set; }
        public Dictionary<int, TcpClient> ActiveSessions { get; set; } = new();
    }
}
