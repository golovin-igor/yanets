using System.Text.RegularExpressions;
using Yanets.Core.Interfaces;
using Yanets.Core.Commands;
using Yanets.Core.Models;
using Yanets.Core.Vendors;

namespace Yanets.Application.Services
{
    /// <summary>
    /// Implementation of command parser for the application layer
    /// </summary>
    public class CommandParser : ICommandParser
    {
        public CommandDefinition? Parse(string commandLine, VendorProfile vendor)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                return null;

            var trimmedCommand = commandLine.Trim();
            var commandDef = vendor.GetCommand(trimmedCommand);

            if (commandDef != null)
                return commandDef;

            // Try to find command by partial match or alias
            return FindCommandByAlias(trimmedCommand, vendor);
        }

        public bool ValidateSyntax(string commandLine, CommandDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(commandLine) || definition == null)
                return false;

            var trimmedCommand = commandLine.Trim();

            // Exact match
            if (definition.Syntax.Equals(trimmedCommand, StringComparison.OrdinalIgnoreCase))
                return true;

            // Check aliases
            if (definition.Aliases.Any(alias => alias.Equals(trimmedCommand, StringComparison.OrdinalIgnoreCase)))
                return true;

            // For more complex validation, we could implement pattern matching
            // For now, basic validation is sufficient

            return false;
        }

        public Dictionary<string, string> ExtractArguments(string commandLine)
        {
            var arguments = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(commandLine))
                return arguments;

            // Simple argument extraction - split by spaces and look for key=value pairs
            var parts = commandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts.Skip(1)) // Skip the command itself
            {
                if (part.Contains('='))
                {
                    var keyValue = part.Split('=', 2);
                    if (keyValue.Length == 2)
                    {
                        arguments[keyValue[0]] = keyValue[1];
                    }
                }
                else if (part.StartsWith("no "))
                {
                    arguments["negate"] = part.Substring(3);
                }
                else
                {
                    // Positional argument
                    var argIndex = arguments.Count(k => !k.Key.Contains("="));
                    arguments[$"arg{argIndex}"] = part;
                }
            }

            return arguments;
        }

        private CommandDefinition? FindCommandByAlias(string commandLine, VendorProfile vendor)
        {
            // Try to match against command patterns
            foreach (var command in vendor.GetAllCommands())
            {
                if (MatchesCommandPattern(commandLine, command.Syntax))
                {
                    return command;
                }

                // Check aliases
                if (command.Aliases.Any(alias => MatchesCommandPattern(commandLine, alias)))
                {
                    return command;
                }
            }

            return null;
        }

        private bool MatchesCommandPattern(string commandLine, string pattern)
        {
            // Simple pattern matching for demo
            // In a real implementation, this would use more sophisticated parsing

            if (pattern.Equals(commandLine, StringComparison.OrdinalIgnoreCase))
                return true;

            // Check for abbreviations (e.g., "sh ver" for "show version")
            if (pattern.StartsWith(commandLine, StringComparison.OrdinalIgnoreCase))
            {
                var patternParts = pattern.Split(' ');
                var commandParts = commandLine.Split(' ');

                if (patternParts.Length >= commandParts.Length)
                {
                    for (int i = 0; i < commandParts.Length; i++)
                    {
                        if (!patternParts[i].StartsWith(commandParts[i], StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                    return true;
                }
            }

            return false;
        }
    }
}
