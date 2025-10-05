using Yanets.Core.Models;

namespace Yanets.Core.Commands
{
    /// <summary>
    /// Represents a command definition with syntax and handler
    /// </summary>
    public class CommandDefinition
    {
        public string Syntax { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PrivilegeLevel { get; set; }
        public List<CommandParameter> Parameters { get; set; } = new();
        public Func<CommandContext, CommandResult> Handler { get; set; } = null!;
        public List<string> Aliases { get; set; } = new();
        public bool RequiresConfirmation { get; set; }

        /// <summary>
        /// Validates that the command definition is properly configured
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Syntax) &&
                   Handler != null &&
                   PrivilegeLevel >= 0;
        }

        /// <summary>
        /// Checks if this command matches the given syntax
        /// </summary>
        public bool MatchesSyntax(string commandSyntax)
        {
            return Syntax.Equals(commandSyntax, StringComparison.OrdinalIgnoreCase) ||
                   Aliases.Contains(commandSyntax, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the parameter by name
        /// </summary>
        public CommandParameter? GetParameter(string name)
        {
            return Parameters.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Represents a command parameter
    /// </summary>
    public class CommandParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; } = string.Empty;
        public List<string> AllowedValues { get; set; } = new();

        /// <summary>
        /// Validates a parameter value
        /// </summary>
        public bool IsValidValue(string value)
        {
            if (IsRequired && string.IsNullOrWhiteSpace(value))
                return false;

            if (AllowedValues.Count > 0 && !AllowedValues.Contains(value))
                return false;

            return true;
        }
    }
}
