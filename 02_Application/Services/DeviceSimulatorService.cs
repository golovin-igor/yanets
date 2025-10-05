using Yanets.Core.Commands;
using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Core.Snmp;
using Yanets.SharedKernel;

namespace Yanets.Application.Services
{
    /// <summary>
    /// Implementation of device simulation service that orchestrates CLI and SNMP operations
    /// </summary>
    public class DeviceSimulatorService : IDeviceSimulator
    {
        private readonly ICommandParser _commandParser;
        private readonly IMibProvider _mibProvider;
        private readonly IPromptGenerator _promptGenerator;
        private NetworkDevice _device;
        private DeviceState _currentState;
        private bool _isRunning;

        public DeviceSimulatorService(
            ICommandParser commandParser,
            IMibProvider mibProvider,
            IPromptGenerator promptGenerator,
            NetworkDevice device)
        {
            _commandParser = commandParser ?? throw new ArgumentNullException(nameof(commandParser));
            _mibProvider = mibProvider ?? throw new ArgumentNullException(nameof(mibProvider));
            _promptGenerator = promptGenerator ?? throw new ArgumentNullException(nameof(promptGenerator));
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _currentState = device.State ?? new DeviceState();
            _isRunning = false;
        }

        public NetworkDevice Device => _device;

        public bool IsRunning => _isRunning;

        public DeviceState GetCurrentState()
        {
            return _currentState;
        }

        public void UpdateState(DeviceState newState)
        {
            if (newState == null)
                throw new ArgumentNullException(nameof(newState));

            _currentState = newState;
            _device.State = newState;
        }

        public async Task InitializeAsync(NetworkDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _currentState = device.State ?? new DeviceState();

            // Initialize device state with default values
            if (_currentState.Uptime == default)
            {
                _currentState.Uptime = DateTime.Now;
            }

            if (_currentState.Resources == null)
            {
                _currentState.Resources = new SystemResources
                {
                    CpuUtilization = 5,
                    MemoryTotal = 65536,
                    MemoryUsed = 12000
                };
            }

            _isRunning = true;
        }

        public async Task ShutdownAsync()
        {
            _isRunning = false;
            // Clean up any resources
        }

        public async Task<CommandResult> ExecuteCommand(CommandContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!_isRunning)
                return CommandResult.CreateError("Device simulator is not running");

            try
            {
                // Parse command using vendor-specific parser
                var commandDef = _commandParser.Parse(context.RawCommand, _device.Vendor);

                if (commandDef == null)
                {
                    return CommandResult.CreateError(
                        $"% Invalid input detected at '^' marker.\n" +
                        $"% Unknown command or incomplete command."
                    );
                }

                // Check privilege level
                if (context.CurrentPrivilegeLevel < commandDef.PrivilegeLevel)
                {
                    return CommandResult.CreateError("% Command authorization failed");
                }

                // Validate command syntax
                if (!_commandParser.ValidateSyntax(context.RawCommand, commandDef))
                {
                    return CommandResult.CreateError("% Invalid input detected at '^' marker.");
                }

                // Execute the command handler
                context.State = _currentState;
                var result = await Task.Run(() => commandDef.Handler(context));

                // Update state if changed
                if (result.UpdatedState != null)
                {
                    _currentState = result.UpdatedState;
                    _device.State = result.UpdatedState;
                }

                return result;
            }
            catch (Exception ex)
            {
                return CommandResult.CreateError($"Command execution failed: {ex.Message}");
            }
        }

        public async Task<SnmpResponse> HandleSnmpRequest(SnmpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!_isRunning)
                return new SnmpResponse { RequestId = request.RequestId, ErrorStatus = SnmpError.GenErr };

            try
            {
                var response = new SnmpResponse { RequestId = request.RequestId };

                foreach (var oid in request.Oids)
                {
                    var handler = _mibProvider.GetOidHandler(oid);

                    if (handler == null)
                    {
                        response.VarBinds.Add(new SnmpVarBind
                        {
                            Oid = oid,
                            Error = SnmpError.NoSuchName
                        });
                        continue;
                    }

                    try
                    {
                        var value = handler.GetValue(_currentState);
                        response.VarBinds.Add(new SnmpVarBind
                        {
                            Oid = oid,
                            Value = value
                        });
                    }
                    catch (Exception ex)
                    {
                        response.VarBinds.Add(new SnmpVarBind
                        {
                            Oid = oid,
                            Error = SnmpError.GenErr
                        });
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                return new SnmpResponse
                {
                    RequestId = request.RequestId,
                    ErrorStatus = SnmpError.GenErr
                };
            }
        }
    }
}
