using Yanets.Core.Commands;
using Yanets.Core.Models;
using Yanets.Core.Snmp;

namespace Yanets.Core.Interfaces
{
    /// <summary>
    /// Interface for device simulation services
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
        Task InitializeAsync();

        /// <summary>
        /// Shuts down the simulator and releases resources
        /// </summary>
        Task ShutdownAsync();
    }
}
