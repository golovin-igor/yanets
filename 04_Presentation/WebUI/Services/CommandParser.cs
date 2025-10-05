using Yanets.Core.Interfaces;
using Yanets.Core.Commands;
using Yanets.Core.Vendors;

namespace Yanets.WebUI.Services
{
    public class CommandParser : ICommandParser
    {
        public CommandDefinition? Parse(string commandLine, VendorProfile vendor)
        {
            // Basic implementation - find command by exact match
            return vendor.GetCommand(commandLine.Trim());
        }

        public bool ValidateSyntax(string commandLine, CommandDefinition definition)
        {
            return definition.Syntax.Equals(commandLine.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public Dictionary<string, string> ExtractArguments(string commandLine)
        {
            // Basic implementation - no argument extraction for now
            return new Dictionary<string, string>();
        }
    }
}
