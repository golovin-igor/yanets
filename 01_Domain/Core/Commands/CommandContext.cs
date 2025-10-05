using Yanets.Core.Models;
using Yanets.SharedKernel;

namespace Yanets.Core.Commands
{
    /// <summary>
    /// Represents the context for command execution
    /// </summary>
    public class CommandContext
    {
        public NetworkDevice Device { get; set; } = null!;
        public Models.DeviceState State { get; set; } = null!;
        public string RawCommand { get; set; } = string.Empty;
        public Dictionary<string, string> ParsedArguments { get; set; } = new();
        public CliSession Session { get; set; } = null!;
        public int CurrentPrivilegeLevel { get; set; }

        /// <summary>
        /// Gets the current CLI mode from the session
        /// </summary>
        public CliMode CurrentMode => Session.CurrentMode;

        /// <summary>
        /// Gets a parsed argument value
        /// </summary>
        public string? GetArgument(string key)
        {
            return ParsedArguments.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Gets a parsed argument with a default value
        /// </summary>
        public string GetArgument(string key, string defaultValue)
        {
            return ParsedArguments.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Gets a parsed argument as integer
        /// </summary>
        public int GetIntArgument(string key, int defaultValue = 0)
        {
            var value = GetArgument(key);
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Validates that required arguments are present
        /// </summary>
        public bool HasRequiredArguments(IEnumerable<string> requiredArgs)
        {
            return requiredArgs.All(arg => ParsedArguments.ContainsKey(arg));
        }

        /// <summary>
        /// Creates a clone of the context for state isolation
        /// </summary>
        public CommandContext Clone()
        {
            return new CommandContext
            {
                Device = Device,
                State = State.Clone(),
                RawCommand = RawCommand,
                ParsedArguments = new Dictionary<string, string>(ParsedArguments),
                Session = Session,
                CurrentPrivilegeLevel = CurrentPrivilegeLevel
            };
        }
    }
}
