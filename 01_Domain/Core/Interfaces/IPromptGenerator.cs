using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.SharedKernel;

namespace Yanets.Core.Interfaces
{
    /// <summary>
    /// Interface for generating CLI prompts
    /// </summary>
    public interface IPromptGenerator
    {
        /// <summary>
        /// Generates a prompt based on current device state and CLI mode
        /// </summary>
        string GeneratePrompt(NetworkDevice device, CliMode currentMode, int privilegeLevel);

        /// <summary>
        /// Generates the welcome banner for initial connection
        /// </summary>
        string GenerateWelcomeBanner(VendorProfile vendor);

        /// <summary>
        /// Generates login prompts
        /// </summary>
        string GetLoginPrompt(VendorProfile vendor);

        /// <summary>
        /// Generates password prompts
        /// </summary>
        string GetPasswordPrompt(VendorProfile vendor);
    }
}
