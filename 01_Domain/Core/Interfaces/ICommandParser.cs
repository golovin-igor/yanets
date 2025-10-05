using Yanets.Core.Commands;
using Yanets.Core.Vendors;

namespace Yanets.Core.Interfaces
{
    /// <summary>
    /// Interface for parsing CLI commands
    /// </summary>
    public interface ICommandParser
    {
        /// <summary>
        /// Parses a command line into a command definition
        /// </summary>
        CommandDefinition? Parse(string commandLine, VendorProfile vendor);

        /// <summary>
        /// Validates the syntax of a command
        /// </summary>
        bool ValidateSyntax(string commandLine, CommandDefinition definition);

        /// <summary>
        /// Extracts arguments from a command line
        /// </summary>
        Dictionary<string, string> ExtractArguments(string commandLine);
    }
}
