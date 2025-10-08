using Microsoft.Extensions.Logging;
using Yanets.VirtualHostConsole.Core.Interfaces;
using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Networking.ProtocolHandlers
{
    public class SnmpProtocolHandler : IProtocolHandler
    {
        private readonly IVirtualHost _host;
        private readonly ILogger<SnmpProtocolHandler> _logger;
        private CancellationTokenSource _cancellationTokenSource;

        public ProtocolType Type => ProtocolType.Snmp;
        public IVirtualHost Host => _host;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public SnmpProtocolHandler(IVirtualHost host, ILogger<SnmpProtocolHandler> logger)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleConnectionAsync(NetworkStream stream)
        {
            // SNMP is UDP-based, so this method is primarily for compatibility
            // Real SNMP handling is done in ProcessDataAsync
            await Task.CompletedTask;
        }

        public async Task<byte[]> ProcessDataAsync(byte[] data)
        {
            try
            {
                _logger.LogDebug("Processing SNMP data for host {HostId}", _host.Id);

                // Parse SNMP request
                var request = SnmpMessageParser.Parse(data);
                if (request == null)
                {
                    _logger.LogWarning("Failed to parse SNMP request for host {HostId}", _host.Id);
                    return CreateSnmpErrorResponse(SnmpError.GenErr);
                }

                // Validate community string
                if (!ValidateCommunity(request.Community))
                {
                    _logger.LogWarning("Invalid SNMP community '{Community}' for host {HostId}",
                        request.Community, _host.Id);
                    return CreateSnmpErrorResponse(SnmpError.GenErr);
                }

                // Handle SNMP request
                var response = await _host.HandleSnmpRequestAsync(request);

                // Encode response
                var responseBytes = SnmpMessageEncoder.Encode(response);

                _logger.LogDebug("Processed SNMP request for host {HostId}, response size: {Size}",
                    _host.Id, responseBytes.Length);

                return responseBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SNMP data for host {HostId}", _host.Id);
                return CreateSnmpErrorResponse(SnmpError.GenErr);
            }
        }

        public async Task StartListeningAsync(int port)
        {
            _logger.LogInformation("Starting SNMP listener on port {Port} for host {HostId}", port, _host.Id);
            // SNMP listening is handled by the ConnectionRouter
        }

        public async Task StopListeningAsync()
        {
            _logger.LogInformation("Stopping SNMP listener for host {HostId}", _host.Id);
            _cancellationTokenSource?.Cancel();
        }

        private bool ValidateCommunity(string community)
        {
            // Check against device configuration
            var snmpConfig = _host.CurrentState.Variables
                .GetValueOrDefault("snmp_communities") as Dictionary<string, string>;

            return snmpConfig?.ContainsKey(community) ?? community == "public";
        }

        private byte[] CreateSnmpErrorResponse(SnmpError error)
        {
            // Simplified error response - in real implementation would properly encode ASN.1
            return Array.Empty<byte>();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}