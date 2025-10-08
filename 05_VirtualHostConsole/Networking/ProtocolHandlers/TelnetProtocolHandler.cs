using System.Text;
using Microsoft.Extensions.Logging;
using Yanets.VirtualHostConsole.Core.Interfaces;
using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Networking.ProtocolHandlers
{
    public class TelnetProtocolHandler : IProtocolHandler
    {
        private readonly IVirtualHost _host;
        private readonly ILogger<TelnetProtocolHandler> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _listeningTask;

        public ProtocolType Type => ProtocolType.Telnet;
        public IVirtualHost Host => _host;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public TelnetProtocolHandler(IVirtualHost host, ILogger<TelnetProtocolHandler> logger)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleConnectionAsync(NetworkStream stream)
        {
            try
            {
                _logger.LogInformation("Handling Telnet connection for host {HostId}", _host.Id);

                // Create CLI session
                var session = new CliSession
                {
                    HostId = _host.Id,
                    Host = _host,
                    RemoteEndPoint = stream.RemoteEndPoint,
                    IsAuthenticated = false,
                    PrivilegeLevel = 0,
                    CurrentMode = CliMode.UserExec
                };

                // Send welcome banner
                await SendWelcomeBannerAsync(stream);

                // Authentication
                if (!await AuthenticateUserAsync(stream, session))
                {
                    await stream.WriteLineAsync("Authentication failed");
                    return;
                }

                session.IsAuthenticated = true;
                session.PrivilegeLevel = 1;

                // Main command loop
                await RunCommandLoopAsync(stream, session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Telnet connection for host {HostId}", _host.Id);
            }
            finally
            {
                stream.Close();
            }
        }

        public async Task<byte[]> ProcessDataAsync(byte[] data)
        {
            // Telnet is handled synchronously in HandleConnectionAsync
            return Array.Empty<byte>();
        }

        public async Task StartListeningAsync(int port)
        {
            _logger.LogInformation("Starting Telnet listener on port {Port} for host {HostId}", port, _host.Id);

            // Telnet listening is handled by the ConnectionRouter
            // This method is called to indicate the handler is ready
        }

        public async Task StopListeningAsync()
        {
            _logger.LogInformation("Stopping Telnet listener for host {HostId}", _host.Id);

            _cancellationTokenSource?.Cancel();
            if (_listeningTask != null)
            {
                await _listeningTask;
            }
        }

        private async Task SendWelcomeBannerAsync(NetworkStream stream)
        {
            var banner = $@"Welcome to {_host.Hostname}

User Access Verification

";
            await stream.WriteAsync(Encoding.ASCII.GetBytes(banner));
        }

        private async Task<bool> AuthenticateUserAsync(NetworkStream stream, CliSession session)
        {
            try
            {
                // Send username prompt
                await stream.WriteAsync(Encoding.ASCII.GetBytes("Username: "));
                var username = await stream.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(username))
                {
                    return false;
                }

                // Send password prompt
                await stream.WriteAsync(Encoding.ASCII.GetBytes("Password: "));
                var password = await stream.ReadLineAsync();

                // Simple authentication - in real implementation would validate against user database
                if (username == "admin" && password == "password")
                {
                    _logger.LogInformation("User {Username} authenticated for host {HostId}", username, _host.Id);
                    return true;
                }

                _logger.LogWarning("Authentication failed for user {Username} on host {HostId}", username, _host.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for host {HostId}", _host.Id);
                return false;
            }
        }

        private async Task RunCommandLoopAsync(NetworkStream stream, CliSession session)
        {
            try
            {
                while (stream.CanRead)
                {
                    // Update session activity
                    session.UpdateActivity();

                    // Send prompt
                    var prompt = GeneratePrompt(session);
                    await stream.WriteAsync(Encoding.ASCII.GetBytes(prompt));

                    // Read command
                    var commandLine = await stream.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(commandLine))
                        continue;

                    // Handle special commands
                    if (commandLine.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        if (session.CurrentMode == CliMode.UserExec)
                            break;

                        session.CurrentMode = ExitMode(session.CurrentMode);
                        continue;
                    }

                    if (commandLine.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    // Execute command
                    var result = await _host.ExecuteCommandAsync(commandLine, session);

                    // Send output
                    if (!string.IsNullOrEmpty(result.Output))
                    {
                        await stream.WriteAsync(Encoding.ASCII.GetBytes(result.Output));
                    }

                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        await stream.WriteLineAsync(result.ErrorMessage);
                    }

                    // Update session mode if changed
                    if (result.UpdatedState != null)
                    {
                        // Mode changes would be handled here
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in command loop for host {HostId}", _host.Id);
            }
        }

        private string GeneratePrompt(CliSession session)
        {
            return session.CurrentMode switch
            {
                CliMode.UserExec => $"{_host.Hostname}>",
                CliMode.PrivilegedExec => $"{_host.Hostname}#",
                CliMode.GlobalConfig => $"{_host.Hostname}(config)#",
                CliMode.InterfaceConfig => $"{_host.Hostname}(config-if)#",
                CliMode.RouterConfig => $"{_host.Hostname}(config-router)#",
                CliMode.LineConfig => $"{_host.Hostname}(config-line)#",
                CliMode.VlanConfig => $"{_host.Hostname}(config-vlan)#",
                _ => $"{_host.Hostname}>"
            };
        }

        private CliMode ExitMode(CliMode currentMode)
        {
            return currentMode switch
            {
                CliMode.PrivilegedExec => CliMode.UserExec,
                CliMode.GlobalConfig => CliMode.PrivilegedExec,
                CliMode.InterfaceConfig => CliMode.GlobalConfig,
                CliMode.RouterConfig => CliMode.GlobalConfig,
                CliMode.LineConfig => CliMode.GlobalConfig,
                CliMode.VlanConfig => CliMode.GlobalConfig,
                _ => CliMode.UserExec
            };
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}