using Yanets.Core.Interfaces;

namespace Yanets.WebUI.VirtualNetwork.Models
{
    public class VendorProfile
    {
        public string VendorName { get; set; }
        public string Os { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new();

        // Integration with existing YANETS interfaces
        public ICommandParser CommandParser { get; set; }
        public IPromptGenerator PromptGenerator { get; set; }
        public IMibProvider MibProvider { get; set; }

        public override string ToString()
        {
            return $"{VendorName} {Os} {Version}";
        }
    }
}