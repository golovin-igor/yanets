using Yanets.Core.Commands;
using Yanets.Core.Interfaces;
using Yanets.Core.Snmp;

namespace Yanets.Core.Vendors
{
    /// <summary>
    /// Abstract base class for vendor-specific device profiles
    /// </summary>
    public abstract class VendorProfile
    {
        public abstract string VendorName { get; }
        public abstract string Os { get; }
        public abstract string Version { get; }

        public abstract ICommandParser CommandParser { get; }
        public abstract IPromptGenerator PromptGenerator { get; }
        public abstract IMibProvider MibProvider { get; }

        // CLI-specific properties
        public virtual string WelcomeBanner { get; set; } = string.Empty;
        public virtual string LoginPrompt { get; set; } = "Username: ";
        public virtual string PasswordPrompt { get; set; } = "Password: ";
        public virtual Dictionary<string, CommandDefinition> Commands { get; set; } = new();

        // SNMP-specific properties
        public virtual string SysObjectId { get; set; } = string.Empty;
        public virtual Dictionary<string, OidHandler> OidHandlers { get; set; } = new();

        /// <summary>
        /// Gets a command definition by syntax
        /// </summary>
        public CommandDefinition? GetCommand(string syntax)
        {
            return Commands.TryGetValue(syntax, out var command) ? command : null;
        }

        /// <summary>
        /// Registers a command definition
        /// </summary>
        public void RegisterCommand(CommandDefinition command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            Commands[command.Syntax] = command;
        }

        /// <summary>
        /// Gets all available commands
        /// </summary>
        public IEnumerable<CommandDefinition> GetAllCommands()
        {
            return Commands.Values;
        }

        /// <summary>
        /// Gets commands available at a specific privilege level
        /// </summary>
        public IEnumerable<CommandDefinition> GetCommandsByPrivilegeLevel(int privilegeLevel)
        {
            return Commands.Values.Where(c => c.PrivilegeLevel <= privilegeLevel);
        }

        /// <summary>
        /// Gets an OID handler by OID string
        /// </summary>
        public OidHandler? GetOidHandler(string oid)
        {
            return OidHandlers.TryGetValue(oid, out var handler) ? handler : null;
        }

        /// <summary>
        /// Registers an OID handler
        /// </summary>
        public void RegisterOidHandler(OidHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            OidHandlers[handler.Oid] = handler;
        }

        /// <summary>
        /// Validates that the vendor profile is properly configured
        /// </summary>
        public virtual bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(VendorName) &&
                   !string.IsNullOrWhiteSpace(Os) &&
                   !string.IsNullOrWhiteSpace(Version) &&
                   CommandParser != null &&
                   PromptGenerator != null &&
                   MibProvider != null &&
                   Commands.Count > 0;
        }
    }
}
