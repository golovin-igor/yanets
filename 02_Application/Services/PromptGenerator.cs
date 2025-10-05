using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.SharedKernel;

namespace Yanets.Application.Services
{
    /// <summary>
    /// Implementation of prompt generator for the application layer
    /// </summary>
    public class PromptGenerator : IPromptGenerator
    {
        public string GeneratePrompt(NetworkDevice device, CliMode currentMode, int privilegeLevel)
        {
            if (device == null)
                return ">";

            var hostname = device.Hostname ?? "unknown";

            return currentMode switch
            {
                CliMode.UserExec => $"{hostname}>",
                CliMode.PrivilegedExec => $"{hostname}#",
                CliMode.GlobalConfig => $"{hostname}(config)#",
                CliMode.InterfaceConfig => $"{hostname}(config-if)#",
                CliMode.RouterConfig => $"{hostname}(config-router)#",
                CliMode.LineConfig => $"{hostname}(config-line)#",
                CliMode.VlanConfig => $"{hostname}(config-vlan)#",
                CliMode.VlanDatabase => $"{hostname}(vlan)#",
                CliMode.Diagnostic => $"{hostname}*",
                _ => $"{hostname}>"
            };
        }

        public string GenerateWelcomeBanner(VendorProfile vendor)
        {
            if (vendor == null)
                return string.Empty;

            return vendor.WelcomeBanner;
        }

        public string GetLoginPrompt(VendorProfile vendor)
        {
            return vendor?.LoginPrompt ?? "Username: ";
        }

        public string GetPasswordPrompt(VendorProfile vendor)
        {
            return vendor?.PasswordPrompt ?? "Password: ";
        }

        /// <summary>
        /// Generates a mode-specific prompt with additional context
        /// </summary>
        public string GenerateContextualPrompt(NetworkDevice device, CliMode currentMode, int privilegeLevel, string? context = null)
        {
            var basePrompt = GeneratePrompt(device, currentMode, privilegeLevel);

            if (!string.IsNullOrEmpty(context))
            {
                return $"{basePrompt} ({context})";
            }

            return basePrompt;
        }
    }
}
