using System.Net;
using Microsoft.Extensions.Logging;
using Yanets.VirtualHostConsole.Core.Interfaces;
using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Core.Services
{
    public class SubnetManager : ISubnetManager
    {
        private readonly Dictionary<string, SubnetDefinition> _subnets;
        private readonly ILogger<SubnetManager> _logger;

        public SubnetManager(ILogger<SubnetManager> logger)
        {
            _subnets = new Dictionary<string, SubnetDefinition>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Create default subnet
            _ = CreateSubnetAsync("192.168.1.0/24", "default", "192.168.1.1");
        }

        public async Task<SubnetDefinition> CreateSubnetAsync(string cidr, string name, string gateway = null)
        {
            try
            {
                _logger.LogInformation("Creating subnet {Name} with CIDR {Cidr}", name, cidr);

                if (_subnets.ContainsKey(name))
                {
                    throw new ArgumentException($"Subnet '{name}' already exists");
                }

                // Parse CIDR notation
                var subnet = ParseCidr(cidr);
                subnet.Name = name;
                subnet.Gateway = !string.IsNullOrEmpty(gateway) ?
                    IPAddress.Parse(gateway) : subnet.NetworkAddress;

                // Add to collection
                _subnets[name] = subnet;

                _logger.LogInformation("Created subnet {Name} with available IPs", name);

                return subnet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create subnet {Name}", name);
                throw;
            }
        }

        public async Task<bool> RemoveSubnetAsync(string name)
        {
            try
            {
                if (!_subnets.ContainsKey(name))
                {
                    _logger.LogWarning("Subnet {Name} not found", name);
                    return false;
                }

                // Check if subnet has allocated IPs
                var subnet = _subnets[name];
                if (subnet.AllocatedIps.Any())
                {
                    _logger.LogWarning("Cannot remove subnet {Name} with allocated IPs", name);
                    return false;
                }

                _subnets.Remove(name);
                _logger.LogInformation("Removed subnet {Name}", name);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove subnet {Name}", name);
                return false;
            }
        }

        public SubnetDefinition GetSubnet(string name)
        {
            return _subnets.TryGetValue(name, out var subnet) ? subnet : null;
        }

        public IEnumerable<SubnetDefinition> GetAllSubnets() => _subnets.Values;

        public string AllocateIpAddress(string subnetName)
        {
            try
            {
                if (!_subnets.TryGetValue(subnetName, out var subnet))
                {
                    _logger.LogError("Subnet {SubnetName} not found", subnetName);
                    return null;
                }

                // Find next available IP
                var availableIp = FindNextAvailableIp(subnet);
                if (string.IsNullOrEmpty(availableIp))
                {
                    _logger.LogWarning("No available IP addresses in subnet {SubnetName}", subnetName);
                    return null;
                }

                // Mark as allocated
                subnet.AllocatedIps.Add(availableIp);

                _logger.LogInformation("Allocated IP {IpAddress} in subnet {SubnetName}", availableIp, subnetName);

                return availableIp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to allocate IP address in subnet {SubnetName}", subnetName);
                return null;
            }
        }

        public bool ReleaseIpAddress(string subnetName, string ipAddress)
        {
            try
            {
                if (!_subnets.TryGetValue(subnetName, out var subnet))
                {
                    _logger.LogWarning("Subnet {SubnetName} not found", subnetName);
                    return false;
                }

                if (subnet.AllocatedIps.Remove(ipAddress))
                {
                    _logger.LogInformation("Released IP {IpAddress} from subnet {SubnetName}", ipAddress, subnetName);
                    return true;
                }

                _logger.LogWarning("IP {IpAddress} not found in subnet {SubnetName}", ipAddress, subnetName);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to release IP {IpAddress} from subnet {SubnetName}", ipAddress, subnetName);
                return false;
            }
        }

        public bool IsIpAvailable(string subnetName, string ipAddress)
        {
            if (!_subnets.TryGetValue(subnetName, out var subnet))
            {
                return false;
            }

            return !subnet.AllocatedIps.Contains(ipAddress) && !subnet.ExcludedIps.Contains(ipAddress);
        }

        public int GetAvailableIpCount(string subnetName)
        {
            if (!_subnets.TryGetValue(subnetName, out var subnet))
            {
                return 0;
            }

            return subnet.AllocatedIps.Count;
        }

        public Dictionary<string, string> GetAllocatedIps(string subnetName)
        {
            if (!_subnets.TryGetValue(subnetName, out var subnet))
            {
                return new Dictionary<string, string>();
            }

            return subnet.AllocatedIps.ToDictionary(ip => ip, ip => subnetName);
        }

        private SubnetDefinition ParseCidr(string cidr)
        {
            try
            {
                var parts = cidr.Split('/');
                if (parts.Length != 2)
                {
                    throw new ArgumentException("Invalid CIDR format");
                }

                var networkAddress = IPAddress.Parse(parts[0]);
                var prefixLength = byte.Parse(parts[1]);

                if (prefixLength > 32)
                {
                    throw new ArgumentException("Invalid prefix length");
                }

                var subnet = new SubnetDefinition
                {
                    Cidr = cidr,
                    NetworkAddress = networkAddress,
                    PrefixLength = prefixLength,
                    BroadcastAddress = CalculateBroadcastAddress(networkAddress, prefixLength),
                    ExcludedIps = new HashSet<string>()
                };

                // Calculate available IP range and exclude network/broadcast
                var (firstIp, lastIp) = CalculateIpRange(networkAddress, prefixLength);
                subnet.ExcludedIps.Add(networkAddress.ToString()); // Network address
                subnet.ExcludedIps.Add(subnet.BroadcastAddress.ToString()); // Broadcast address

                return subnet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse CIDR {Cidr}", cidr);
                throw;
            }
        }

        private string FindNextAvailableIp(SubnetDefinition subnet)
        {
            var (firstIp, lastIp) = CalculateIpRange(subnet.NetworkAddress, subnet.PrefixLength);

            for (var current = firstIp; current <= lastIp; current++)
            {
                var ipString = current.ToString();
                if (!subnet.AllocatedIps.Contains(ipString) && !subnet.ExcludedIps.Contains(ipString))
                {
                    return ipString;
                }
            }

            return null; // No available IPs
        }

        private (uint firstIp, uint lastIp) CalculateIpRange(IPAddress networkAddress, byte prefixLength)
        {
            var ipBytes = networkAddress.GetAddressBytes();
            var ipAddress = BitConverter.ToUInt32(ipBytes.Reverse().ToArray(), 0);

            var hostBits = 32 - prefixLength;
            var networkMask = uint.MaxValue << hostBits;

            var networkAddressInt = ipAddress & networkMask;
            var firstIp = networkAddressInt + 1; // Skip network address
            var broadcastAddress = networkAddressInt | (uint.MaxValue >> prefixLength);
            var lastIp = broadcastAddress - 1; // Skip broadcast address

            return (firstIp, lastIp);
        }

        private IPAddress CalculateBroadcastAddress(IPAddress networkAddress, byte prefixLength)
        {
            var ipBytes = networkAddress.GetAddressBytes();
            var ipAddress = BitConverter.ToUInt32(ipBytes.Reverse().ToArray(), 0);

            var hostBits = 32 - prefixLength;
            var broadcastAddress = ipAddress | (uint.MaxValue >> prefixLength);

            var broadcastBytes = BitConverter.GetBytes(broadcastAddress).Reverse().ToArray();
            return new IPAddress(broadcastBytes);
        }
    }
}