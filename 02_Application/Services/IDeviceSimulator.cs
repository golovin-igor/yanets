using Yanets.Core.Commands;
using Yanets.Core.Interfaces;
using Yanets.Core.Models;

namespace Yanets.Application.Services
{
    /// <summary>
    /// Interface for device simulation services that orchestrate CLI and SNMP operations
    /// </summary>
    public interface IDeviceSimulator
    {
        /// <summary>
        /// Executes a CLI command on the device
        /// </summary>
        Task<CommandResult> ExecuteCommand(CommandContext context);

        /// <summary>
        /// Handles an SNMP request
        /// </summary>
        Task<SnmpResponse> HandleSnmpRequest(SnmpRequest request);

        /// <summary>
        /// Gets the current device state
        /// </summary>
        DeviceState GetCurrentState();

        /// <summary>
        /// Updates the device state
        /// </summary>
        void UpdateState(DeviceState newState);

        /// <summary>
        /// Initializes the simulator with device configuration
        /// </summary>
        Task InitializeAsync(NetworkDevice device);

        /// <summary>
        /// Shuts down the simulator and releases resources
        /// </summary>
        Task ShutdownAsync();

        /// <summary>
        /// Gets the associated network device
        /// </summary>
        NetworkDevice Device { get; }

        /// <summary>
        /// Gets whether the simulator is currently running
        /// </summary>
        bool IsRunning { get; }
    }
}
