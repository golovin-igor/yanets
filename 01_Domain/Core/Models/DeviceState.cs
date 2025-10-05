using Yanets.SharedKernel;

namespace Yanets.Core.Models
{
    /// <summary>
    /// Represents the runtime state of a network device
    /// </summary>
    public class DeviceState
    {
        public string RunningConfig { get; set; } = string.Empty;
        public string StartupConfig { get; set; } = string.Empty;
        public Dictionary<string, object> Variables { get; set; } = new();
        public List<RoutingTableEntry> RoutingTable { get; set; } = new();
        public List<ArpEntry> ArpTable { get; set; } = new();
        public List<MacAddressEntry> MacTable { get; set; } = new();
        public List<VlanConfiguration> Vlans { get; set; } = new();
        public DateTime Uptime { get; set; } = DateTime.Now;
        public SystemResources Resources { get; set; } = new();
        public bool IsSimulationRunning { get; set; }

        /// <summary>
        /// Gets the uptime as a TimeSpan
        /// </summary>
        public TimeSpan CurrentUptime => DateTime.Now - Uptime;

        /// <summary>
        /// Gets or sets a variable value
        /// </summary>
        public object? GetVariable(string key)
        {
            return Variables.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Sets a variable value
        /// </summary>
        public void SetVariable(string key, object value)
        {
            Variables[key] = value;
        }

        /// <summary>
        /// Gets a strongly-typed variable value
        /// </summary>
        public T? GetVariable<T>(string key) where T : class
        {
            var value = GetVariable(key);
            return value as T;
        }

        /// <summary>
        /// Gets a variable value with a default fallback
        /// </summary>
        public T GetVariable<T>(string key, T defaultValue) where T : notnull
        {
            var value = GetVariable(key);
            return value != null ? (T)value : defaultValue;
        }

        /// <summary>
        /// Adds a routing table entry
        /// </summary>
        public void AddRoute(RoutingTableEntry route)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));

            RoutingTable.Add(route);
        }

        /// <summary>
        /// Removes routing table entries matching the criteria
        /// </summary>
        public int RemoveRoutes(string? destination = null, string? interfaceName = null)
        {
            return RoutingTable.RemoveAll(route =>
                (destination == null || route.Destination == destination) &&
                (interfaceName == null || route.Interface == interfaceName));
        }

        /// <summary>
        /// Gets routing table entries for a specific destination
        /// </summary>
        public IEnumerable<RoutingTableEntry> GetRoutes(string destination)
        {
            return RoutingTable.Where(route => route.Destination == destination);
        }

        /// <summary>
        /// Adds an ARP entry
        /// </summary>
        public void AddArpEntry(ArpEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            ArpTable.Add(entry);
        }

        /// <summary>
        /// Removes ARP entries for a specific IP address
        /// </summary>
        public bool RemoveArpEntry(string ipAddress)
        {
            return ArpTable.RemoveAll(entry => entry.IpAddress == ipAddress) > 0;
        }

        /// <summary>
        /// Gets ARP entry for a specific IP address
        /// </summary>
        public ArpEntry? GetArpEntry(string ipAddress)
        {
            return ArpTable.FirstOrDefault(entry => entry.IpAddress == ipAddress);
        }

        /// <summary>
        /// Adds a MAC address table entry
        /// </summary>
        public void AddMacEntry(MacAddressEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            MacTable.Add(entry);
        }

        /// <summary>
        /// Removes MAC address entries for a specific MAC address
        /// </summary>
        public bool RemoveMacEntry(string macAddress)
        {
            return MacTable.RemoveAll(entry => entry.MacAddress == macAddress) > 0;
        }

        /// <summary>
        /// Gets MAC address entries for a specific MAC address
        /// </summary>
        public IEnumerable<MacAddressEntry> GetMacEntries(string macAddress)
        {
            return MacTable.Where(entry => entry.MacAddress == macAddress);
        }

        /// <summary>
        /// Adds a VLAN configuration
        /// </summary>
        public void AddVlan(VlanConfiguration vlan)
        {
            if (vlan == null)
                throw new ArgumentNullException(nameof(vlan));

            if (Vlans.Any(v => v.Id == vlan.Id))
                throw new InvalidOperationException($"VLAN {vlan.Id} already exists");

            Vlans.Add(vlan);
        }

        /// <summary>
        /// Removes a VLAN configuration
        /// </summary>
        public bool RemoveVlan(int vlanId)
        {
            return Vlans.RemoveAll(v => v.Id == vlanId) > 0;
        }

        /// <summary>
        /// Gets a VLAN configuration by ID
        /// </summary>
        public VlanConfiguration? GetVlan(int vlanId)
        {
            return Vlans.FirstOrDefault(v => v.Id == vlanId);
        }

        /// <summary>
        /// Updates system resources
        /// </summary>
        public void UpdateResources(SystemResources resources)
        {
            Resources = resources ?? throw new ArgumentNullException(nameof(resources));
        }

        /// <summary>
        /// Creates a deep clone of the device state
        /// </summary>
        public DeviceState Clone()
        {
            return new DeviceState
            {
                RunningConfig = RunningConfig,
                StartupConfig = StartupConfig,
                Variables = new Dictionary<string, object>(Variables),
                RoutingTable = RoutingTable.Select(r => new RoutingTableEntry
                {
                    Destination = r.Destination,
                    Gateway = r.Gateway,
                    Interface = r.Interface,
                    Metric = r.Metric,
                    Protocol = r.Protocol
                }).ToList(),
                ArpTable = ArpTable.Select(a => new ArpEntry
                {
                    IpAddress = a.IpAddress,
                    MacAddress = a.MacAddress,
                    Interface = a.Interface,
                    Type = a.Type
                }).ToList(),
                MacTable = MacTable.Select(m => new MacAddressEntry
                {
                    MacAddress = m.MacAddress,
                    Interface = m.Interface,
                    Vlan = m.Vlan,
                    Type = m.Type
                }).ToList(),
                Vlans = Vlans.Select(v => new VlanConfiguration
                {
                    Id = v.Id,
                    Name = v.Name,
                    Ports = new List<int>(v.Ports),
                    IsActive = v.IsActive
                }).ToList(),
                Uptime = Uptime,
                Resources = new SystemResources
                {
                    CpuUtilization = Resources.CpuUtilization,
                    MemoryTotal = Resources.MemoryTotal,
                    MemoryUsed = Resources.MemoryUsed
                },
                IsSimulationRunning = IsSimulationRunning
            };
        }

        /// <summary>
        /// Validates the device state
        /// </summary>
        public bool IsValid()
        {
            // Basic validation - can be extended based on requirements
            return Resources != null &&
                   Uptime <= DateTime.Now &&
                   Variables != null &&
                   RoutingTable != null &&
                   ArpTable != null &&
                   MacTable != null &&
                   Vlans != null;
        }

        /// <summary>
        /// Resets the device state to initial values
        /// </summary>
        public void Reset()
        {
            RunningConfig = string.Empty;
            StartupConfig = string.Empty;
            Variables.Clear();
            RoutingTable.Clear();
            ArpTable.Clear();
            MacTable.Clear();
            Vlans.Clear();
            Uptime = DateTime.Now;
            Resources = new SystemResources();
            IsSimulationRunning = false;
        }
    }
}
