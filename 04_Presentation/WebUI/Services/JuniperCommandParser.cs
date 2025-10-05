using Yanets.Core.Interfaces;
using Yanets.Core.Commands;
using Yanets.Core.Vendors;

namespace Yanets.WebUI.Services
{
    public class JuniperCommandParser : ICommandParser
    {
        public CommandDefinition? Parse(string commandLine, VendorProfile vendor)
        {
            return vendor.GetCommand(commandLine.Trim());
        }

        public bool ValidateSyntax(string commandLine, CommandDefinition definition)
        {
            return definition.Syntax.Equals(commandLine.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public Dictionary<string, string> ExtractArguments(string commandLine)
        {
            return new Dictionary<string, string>();
        }
    }
}
