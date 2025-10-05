using System.Net;
using System.Net.Sockets;
using System.Text;
using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.SharedKernel;

namespace Yanets.Application.Services
{
    /// <summary>
    /// Service for managing CLI server connections (Telnet/SSH)
    /// </summary>
    public class CliServerService
    {
        private readonly Dictionary<Guid, TcpListener> _deviceListeners;
        private readonly ITopologyService _topologyService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CliServerService> _logger;
        private bool _isRunning;

        public CliServerService(
            ITopologyService topologyService,
            IServiceProvider serviceProvider,
            ILogger<CliServerService> logger)
        {
            _deviceListeners = new Dictionary<Guid, TcpListener>();
            _topologyService = topologyService ?? throw new ArgumentNullException(nameof(topologyService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _isRunning = false;
        }

        public bool IsRunning => _isRunning;

        /// <summary>
        /// Starts the CLI server for a specific device
        /// </summary>
        public void StartCliServer(NetworkDevice device, int port = 23)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            if (_deviceListeners.ContainsKey(device.Id))
                throw new InvalidOperationException($"CLI server already running for device {device.Id}");

            try
            {
                var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                _deviceListeners[device.Id] = listener;

                _logger.LogInformation("Started CLI server for device {DeviceId} on port {Port}", device.Id, port);

                Task.Run(() => AcceptCliConnections(device, listener));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start CLI server for device {DeviceId}", device.Id);
                throw;
            }
        }

        /// <summary>
        /// Stops the CLI server for a specific device
        /// </summary>
        public void StopCliServer(Guid deviceId)
        {
            if (_deviceListeners.TryGetValue(deviceId, out var listener))
            {
                try
                {
                    listener.Stop();
                    _deviceListeners.Remove(deviceId);
                    _logger.LogInformation("Stopped CLI server for device {DeviceId}", deviceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping CLI server for device {DeviceId}", deviceId);
                }
            }
        }

        /// <summary>
        /// Stops all CLI servers
        /// </summary>
        public void StopAllServers()
        {
            foreach (var deviceId in _deviceListeners.Keys.ToList())
            {
                StopCliServer(deviceId);
            }
        }

        private async Task AcceptCliConnections(NetworkDevice device, TcpListener listener)
        {
            while (listener.Server.IsBound)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync();
                    _logger.LogInformation("Accepted CLI connection for device {DeviceId} from {RemoteEndPoint}",
                        device.Id, client.Client.RemoteEndPoint);

                    _ = Task.Run(() => HandleCliSession(device, client));
                }
                catch (ObjectDisposedException)
                {
                    // Listener was stopped
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting CLI connection for device {DeviceId}", device.Id);
                }
            }
        }

        private async Task HandleCliSession(NetworkDevice device, TcpClient client)
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII);
            using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

            var session = new CliSession
            {
                Device = device,
                PrivilegeLevel = 0,
                IsAuthenticated = false,
                CurrentMode = CliMode.UserExec,
                ClientAddress = client.Client.RemoteEndPoint?.ToString() ?? "unknown"
            };

            try
            {
                // Get device simulator service
                var simulator = _serviceProvider.GetService<IDeviceSimulator>();
                if (simulator == null)
                {
                    await writer.WriteLineAsync("Device simulator not available");
                    return;
                }

                // Send welcome banner
                var welcomeBanner = device.Vendor.WelcomeBanner;
                if (!string.IsNullOrEmpty(welcomeBanner))
                {
                    await writer.WriteAsync(welcomeBanner);
                }

                // Authentication flow
                if (!await AuthenticateUser(session, reader, writer, device.Vendor))
                {
                    await writer.WriteLineAsync("Authentication failed");
                    return;
                }

                session.IsAuthenticated = true;
                session.PrivilegeLevel = 1;

                // Main command loop
                while (client.Connected && _isRunning)
                {
                    try
                    {
                        // Display prompt
                        var prompt = GeneratePrompt(device, session);
                        await writer.WriteAsync(prompt);

                        // Read command
                        var commandLine = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(commandLine))
                            continue;

                        session.UpdateActivity();

                        // Handle special commands
                        if (await HandleSpecialCommands(commandLine, session, writer))
                            continue;

                        // Execute command
                        var context = new CommandContext
                        {
                            Device = device,
                            State = device.State,
                            RawCommand = commandLine,
                            Session = session,
                            CurrentPrivilegeLevel = session.PrivilegeLevel
                        };

                        var result = await simulator.ExecuteCommand(context);

                        if (!string.IsNullOrEmpty(result.Output))
                        {
                            await writer.WriteAsync(result.Output);
                        }

                        if (!string.IsNullOrEmpty(result.ErrorMessage))
                        {
                            await writer.WriteLineAsync(result.ErrorMessage);
                        }
                    }
                    catch (IOException)
                    {
                        // Client disconnected
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling CLI session for device {DeviceId}", device.Id);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in CLI session for device {DeviceId}", device.Id);
            }
            finally
            {
                client.Close();
            }
        }

        private async Task<bool> AuthenticateUser(
            CliSession session,
            StreamReader reader,
            StreamWriter writer,
            VendorProfile vendor)
        {
            try
            {
                // Send login prompt
                await writer.WriteAsync(vendor.LoginPrompt);

                // Read username (for now, accept any username)
                var username = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(username))
                    return false;

                // Send password prompt
                await writer.WriteAsync(vendor.PasswordPrompt);

                // Read password (for now, accept any password)
                var password = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(password))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication error for device {DeviceId}", session.Device.Id);
                return false;
            }
        }

        private async Task<bool> HandleSpecialCommands(string commandLine, CliSession session, StreamWriter writer)
        {
            var command = commandLine.Trim().ToLower();

            switch (command)
            {
                case "exit":
                    if (session.CurrentMode == CliMode.UserExec)
                        return true; // Exit session

                    session.CurrentMode = session.ModeStack.Count > 0 ? session.ModeStack.Pop() : CliMode.UserExec;
                    return true;

                case "enable":
                    if (session.CurrentMode == CliMode.UserExec)
                    {
                        session.CurrentMode = CliMode.PrivilegedExec;
                        session.PrivilegeLevel = 15;
                        await writer.WriteLineAsync("Password: ");
                        // For demo, accept any password
                        return true;
                    }
                    break;

                case "configure terminal":
                case "conf t":
                    if (session.CurrentMode == CliMode.PrivilegedExec)
                    {
                        session.CurrentMode = CliMode.GlobalConfig;
                        session.ModeStack.Push(CliMode.PrivilegedExec);
                        await writer.WriteLineAsync(
                            $"Enter configuration commands, one per line. End with CNTL/Z.\n{session.Device.Hostname}(config)#"
                        );
                        return true;
                    }
                    break;

                case "end":
                case "\u001a": // Ctrl+Z
                    if (session.ModeStack.Count > 0)
                    {
                        session.CurrentMode = session.ModeStack.Pop();
                        await writer.WriteLineAsync($"{session.Device.Hostname}#");
                        return true;
                    }
                    break;
            }

            return false;
        }

        private string GeneratePrompt(NetworkDevice device, CliSession session)
        {
            return session.CurrentMode switch
            {
                CliMode.UserExec => $"{device.Hostname}>",
                CliMode.PrivilegedExec => $"{device.Hostname}#",
                CliMode.GlobalConfig => $"{device.Hostname}(config)#",
                CliMode.InterfaceConfig => $"{device.Hostname}(config-if)#",
                CliMode.RouterConfig => $"{device.Hostname}(config-router)#",
                CliMode.LineConfig => $"{device.Hostname}(config-line)#",
                CliMode.VlanConfig => $"{device.Hostname}(config-vlan)#",
                _ => $"{device.Hostname}>"
            };
        }
    }
}
