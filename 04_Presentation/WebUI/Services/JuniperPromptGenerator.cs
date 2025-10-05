using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.SharedKernel;

namespace Yanets.WebUI.Services
{
    public class JuniperPromptGenerator : IPromptGenerator
    {
        public string GeneratePrompt(NetworkDevice device, CliMode currentMode, int privilegeLevel)
        {
            return currentMode switch
            {
                CliMode.UserExec => $"{device.Hostname}>",
                CliMode.PrivilegedExec => $"{device.Hostname}#",
                CliMode.GlobalConfig => $"{device.Hostname}#",
                _ => $"{device.Hostname}>"
            };
        }

        public string GenerateWelcomeBanner(VendorProfile vendor)
        {
            return $"JUNOS {vendor.Version} built {DateTime.Now:yyyy-MM-dd}\n";
        }

        public string GetLoginPrompt(VendorProfile vendor)
        {
            return vendor.LoginPrompt;
        }

        public string GetPasswordPrompt(VendorProfile vendor)
        {
            return vendor.PasswordPrompt;
        }
    }
}
