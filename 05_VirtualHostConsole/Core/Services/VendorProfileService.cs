using Microsoft.Extensions.Logging;
using Yanets.Core.Vendors;
using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Core.Services
{
    public class VendorProfileService
    {
        private readonly ILogger<VendorProfileService> _logger;
        private readonly Dictionary<string, VendorProfile> _vendorProfiles;

        public VendorProfileService(ILogger<VendorProfileService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vendorProfiles = new Dictionary<string, VendorProfile>();

            // Initialize built-in vendor profiles
            InitializeVendorProfiles();
        }

        public VendorProfile GetVendorProfile(string vendorName)
        {
            if (string.IsNullOrEmpty(vendorName))
            {
                throw new ArgumentNullException(nameof(vendorName));
            }

            var normalizedName = vendorName.ToLower();

            if (_vendorProfiles.TryGetValue(normalizedName, out var profile))
            {
                _logger.LogDebug("Returning vendor profile for {VendorName}", vendorName);
                return profile;
            }

            _logger.LogWarning("Vendor profile for {VendorName} not found, returning default", vendorName);
            return _vendorProfiles["cisco"]; // Return Cisco as default
        }

        public List<string> GetAvailableVendors()
        {
            return _vendorProfiles.Keys.ToList();
        }

        public VendorProfile CreateCustomVendorProfile(string name, string os, string version)
        {
            try
            {
                _logger.LogInformation("Creating custom vendor profile: {Name}", name);

                var profile = new VendorProfile
                {
                    VendorName = name,
                    Os = os,
                    Version = version
                };

                _vendorProfiles[name.ToLower()] = profile;

                _logger.LogInformation("Created custom vendor profile {Name}", name);
                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create custom vendor profile {Name}", name);
                throw;
            }
        }

        private void InitializeVendorProfiles()
        {
            try
            {
                _logger.LogInformation("Initializing built-in vendor profiles");

                // Cisco IOS Profile
                var ciscoProfile = new VendorProfile
                {
                    VendorName = "Cisco",
                    Os = "IOS",
                    Version = "15.0"
                };

                // Juniper JunOS Profile
                var juniperProfile = new VendorProfile
                {
                    VendorName = "Juniper",
                    Os = "JunOS",
                    Version = "18.4"
                };

                // Add to collection
                _vendorProfiles["cisco"] = ciscoProfile;
                _vendorProfiles["juniper"] = juniperProfile;

                _logger.LogInformation("Initialized {Count} vendor profiles", _vendorProfiles.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize vendor profiles");
                throw;
            }
        }

        public async Task LoadVendorProfilesFromDirectoryAsync(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    _logger.LogWarning("Vendor profile directory {DirectoryPath} does not exist", directoryPath);
                    return;
                }

                var profileFiles = Directory.GetFiles(directoryPath, "*.json");

                foreach (var file in profileFiles)
                {
                    try
                    {
                        await LoadVendorProfileFromFileAsync(file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to load vendor profile from {File}", file);
                    }
                }

                _logger.LogInformation("Loaded vendor profiles from {DirectoryPath}", directoryPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load vendor profiles from directory {DirectoryPath}", directoryPath);
            }
        }

        private async Task LoadVendorProfileFromFileAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var profile = System.Text.Json.JsonSerializer.Deserialize<VendorProfile>(json);

                if (profile != null)
                {
                    var vendorName = profile.VendorName?.ToLower() ?? Path.GetFileNameWithoutExtension(filePath).ToLower();
                    _vendorProfiles[vendorName] = profile;

                    _logger.LogInformation("Loaded vendor profile {VendorName} from {FilePath}",
                        profile.VendorName, filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load vendor profile from {FilePath}", filePath);
                throw;
            }
        }

        public async Task SaveVendorProfileAsync(string vendorName, string filePath)
        {
            try
            {
                var profile = GetVendorProfile(vendorName);

                var json = System.Text.Json.JsonSerializer.Serialize(profile, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(filePath, json);

                _logger.LogInformation("Saved vendor profile {VendorName} to {FilePath}", vendorName, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save vendor profile {VendorName}", vendorName);
                throw;
            }
        }
    }
}