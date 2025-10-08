using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Yanets.VirtualHostConsole.Core.Interfaces;
using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Networking
{
    public class ConnectionRouter : IConnectionRouter
    {
        private readonly Dictionary<int, IVirtualHost> _portToHostMapping;
        private readonly Dictionary<string, Dictionary<ProtocolType, int>> _hostToPortMapping;
        private readonly ProtocolDetector _protocolDetector;
        private readonly ILogger<ConnectionRouter> _logger;
        private readonly List<Task> _connectionTasks;
        private CancellationTokenSource _cancellationTokenSource;

        public ConnectionRouter(ILogger<ConnectionRouter> logger)
        {
            _portToHostMapping = new Dictionary<int, IVirtualHost>();
            _hostToPortMapping = new Dictionary<string, Dictionary<ProtocolType, int>>();
            _protocolDetector = new ProtocolDetector();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionTasks = new List<Task>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task RouteTcpConnectionAsync(TcpClient client, int localPort)
        {
            try
            {
                if (!_portToHostMapping.TryGetValue(localPort, out var host))
                {
                    _logger.LogWarning("No host registered for port {Port}", localPort);
                    client.Close();
                    return;
                }

                // Detect protocol
                var protocol = await _protocolDetector.DetectProtocolAsync(client.InnerClient);

                _logger.LogInformation("Routing {Protocol} connection on port {Port} to host {HostId}",
                    protocol, localPort, host.Id);

                // Create session for the connection
                var session = new CliSession
                {
                    HostId = host.Id,
                    Host = host,
                    RemoteEndPoint = client.RemoteEndPoint,
                    IsAuthenticated = false,
                    PrivilegeLevel = 0,
                    CurrentMode = CliMode.UserExec
                };

                // Handle the connection based on protocol
                var handler = CreateProtocolHandler(protocol, host);
                if (handler != null)
                {
                    var connectionTask = handler.HandleConnectionAsync(client.GetStream());
                    _connectionTasks.Add(connectionTask);

                    // Clean up completed tasks
                    _ = CleanupCompletedTasksAsync();
                }
                else
                {
                    _logger.LogWarning("No handler available for protocol {Protocol}", protocol);
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error routing TCP connection on port {Port}", localPort);
                client.Close();
            }
        }

        public async Task RouteUdpPacketAsync(UdpReceiveResult packet, int localPort)
        {
            try
            {
                if (!_portToHostMapping.TryGetValue(localPort, out var host))
                {
                    _logger.LogWarning("No host registered for UDP port {Port}", localPort);
                    return;
                }

                // Parse SNMP request
                var snmpRequest = SnmpMessageParser.Parse(packet.Buffer);
                if (snmpRequest == null)
                {
                    _logger.LogWarning("Failed to parse SNMP request from {RemoteEndPoint}", packet.RemoteEndPoint);
                    return;
                }

                _logger.LogInformation("Routing SNMP request to host {HostId}", host.Id);

                // Handle SNMP request
                var response = await host.HandleSnmpRequestAsync(snmpRequest);

                if (response != null)
                {
                    // Send SNMP response
                    var responseBytes = SnmpMessageEncoder.Encode(response);
                    // Note: In a real implementation, you'd need UDP client to send response
                    _logger.LogDebug("SNMP response ready for host {HostId}", host.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error routing UDP packet on port {Port}", localPort);
            }
        }

        public void RegisterHostPorts(IVirtualHost host, Dictionary<ProtocolType, int> portMappings)
        {
            try
            {
                _logger.LogInformation("Registering ports for host {HostId}", host.Id);

                // Remove existing mappings for this host
                UnregisterHostPorts(host);

                // Register new port mappings
                foreach (var (protocol, port) in portMappings)
                {
                    _portToHostMapping[port] = host;
                    host.PortMappings[port] = protocol;

                    _logger.LogDebug("Registered {Protocol} on port {Port} for host {HostId}",
                        protocol, port, host.Id);
                }

                // Store host-to-port mapping
                _hostToPortMapping[host.Id] = new Dictionary<ProtocolType, int>(portMappings);

                _logger.LogInformation("Registered {PortCount} ports for host {HostId}",
                    portMappings.Count, host.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register ports for host {HostId}", host.Id);
                throw;
            }
        }

        public void UnregisterHostPorts(IVirtualHost host)
        {
            try
            {
                _logger.LogInformation("Unregistering ports for host {HostId}", host.Id);

                // Remove from port-to-host mapping
                var portsToRemove = _portToHostMapping
                    .Where(kvp => kvp.Value.Id == host.Id)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var port in portsToRemove)
                {
                    _portToHostMapping.Remove(port);
                }

                // Remove from host-to-port mapping
                _hostToPortMapping.Remove(host.Id);

                // Clear host port mappings
                host.PortMappings.Clear();

                _logger.LogInformation("Unregistered {PortCount} ports for host {HostId}",
                    portsToRemove.Count, host.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unregister ports for host {HostId}", host.Id);
            }
        }

        public Dictionary<ProtocolType, int> GetHostPortMappings(IVirtualHost host)
        {
            return _hostToPortMapping.TryGetValue(host.Id, out var mappings)
                ? new Dictionary<ProtocolType, int>(mappings)
                : new Dictionary<ProtocolType, int>();
        }

        public bool IsPortRegistered(int port)
        {
            return _portToHostMapping.ContainsKey(port);
        }

        public IVirtualHost GetHostByPort(int port)
        {
            return _portToHostMapping.TryGetValue(port, out var host) ? host : null;
        }

        public async Task StartAsync()
        {
            _logger.LogInformation("Starting ConnectionRouter");

            // Start TCP listeners for all registered ports
            foreach (var (port, host) in _portToHostMapping)
            {
                await StartTcpListenerAsync(port);
            }

            // Start UDP listeners for SNMP
            var snmpPorts = _portToHostMapping.Keys.Where(p => p % 1000 == 161); // SNMP ports
            foreach (var port in snmpPorts)
            {
                await StartUdpListenerAsync(port);
            }

            _logger.LogInformation("ConnectionRouter started with {PortCount} registered ports",
                _portToHostMapping.Count);
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Stopping ConnectionRouter");

            // Cancel all connection tasks
            _cancellationTokenSource.Cancel();

            // Wait for tasks to complete
            await Task.WhenAll(_connectionTasks.Where(t => !t.IsCompleted));
            _connectionTasks.Clear();

            _logger.LogInformation("ConnectionRouter stopped");
        }

        private async Task StartTcpListenerAsync(int port)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();

                _logger.LogDebug("Started TCP listener on port {Port}", port);

                // Start accepting connections
                _ = Task.Run(async () =>
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var client = await listener.AcceptTcpClientAsync();
                            var connectionTask = RouteTcpConnectionAsync(new TcpClient(client), port);
                            _connectionTasks.Add(connectionTask);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error accepting TCP connection on port {Port}", port);
                        }
                    }

                    listener.Stop();
                }, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start TCP listener on port {Port}", port);
            }
        }

        private async Task StartUdpListenerAsync(int port)
        {
            try
            {
                var udpClient = new UdpClient(port);
                _logger.LogDebug("Started UDP listener on port {Port}", port);

                // Start receiving UDP packets
                _ = Task.Run(async () =>
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var receiveResult = await udpClient.ReceiveAsync();
                            var packetTask = RouteUdpPacketAsync(new UdpReceiveResult
                            {
                                Buffer = receiveResult.Buffer,
                                RemoteEndPoint = receiveResult.RemoteEndPoint
                            }, port);
                            _connectionTasks.Add(packetTask);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error receiving UDP packet on port {Port}", port);
                        }
                    }

                    udpClient.Close();
                }, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start UDP listener on port {Port}", port);
            }
        }

        private IProtocolHandler CreateProtocolHandler(ProtocolType protocol, IVirtualHost host)
        {
            return protocol switch
            {
                ProtocolType.Telnet => new TelnetProtocolHandler(host, _logger),
                ProtocolType.Snmp => new SnmpProtocolHandler(host, _logger),
                _ => null
            };
        }

        private async Task CleanupCompletedTasksAsync()
        {
            var completedTasks = _connectionTasks.Where(t => t.IsCompleted).ToList();
            foreach (var task in completedTasks)
            {
                _connectionTasks.Remove(task);

                // Log any exceptions from completed tasks
                if (task.IsFaulted && task.Exception != null)
                {
                    _logger.LogError(task.Exception, "Connection task failed");
                }
            }

            // Prevent memory leaks by limiting task history
            if (_connectionTasks.Count > 1000)
            {
                _logger.LogWarning("Too many connection tasks, cleaning up");
                _connectionTasks.RemoveAll(t => t.IsCompleted);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }

    // Supporting classes for protocol detection and message parsing
    public class ProtocolDetector
    {
        public async Task<ProtocolType> DetectProtocolAsync(Socket socket)
        {
            try
            {
                // Read first few bytes to detect protocol
                var buffer = new byte[10];
                var received = await socket.ReceiveAsync(buffer, SocketFlags.Peek);

                if (received == 0)
                    return ProtocolType.Telnet; // Default

                // Simple protocol detection based on initial bytes
                if (buffer[0] == 0x30) // SNMP message starts with 0x30
                    return ProtocolType.Snmp;

                return ProtocolType.Telnet; // Default to Telnet
            }
            catch
            {
                return ProtocolType.Telnet;
            }
        }
    }

    // Placeholder classes for SNMP message handling
    public static class SnmpMessageParser
    {
        public static SnmpRequest Parse(byte[] buffer)
        {
            // Simplified SNMP parsing - in real implementation would properly parse ASN.1
            return new SnmpRequest
            {
                RequestId = 1,
                Type = SnmpRequestType.Get,
                Community = "public",
                Oids = new List<string>()
            };
        }
    }

    public static class SnmpMessageEncoder
    {
        public static byte[] Encode(SnmpResponse response)
        {
            // Simplified SNMP encoding - in real implementation would properly encode ASN.1
            return Array.Empty<byte>();
        }
    }
}