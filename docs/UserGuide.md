# YANETS User Guide

## Introduction

YANETS (Yet Another Network Equipment Test Simulator) is a comprehensive network device simulation system that provides realistic emulation of network equipment including CLI (Telnet/SSH) and SNMP communication with vendor-specific behaviors.

This guide will help you get started with YANETS, understand its capabilities, and make the most of its features for network testing and development.

## Quick Start

### 1. Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/yanets.git
cd yanets

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests to verify everything works
dotnet test
```

### 2. Start the Simulator

```bash
# Start the Web API
cd 04_Presentation/WebUI
dotnet run
```

The API will be available at:
- **API Base**: `http://localhost:5000`
- **Swagger UI**: `http://localhost:5000/swagger`
- **Health Check**: `http://localhost:5000/health`

### 3. Create Your First Topology

```bash
# Create a topology
curl -X POST http://localhost:5000/api/topology \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My First Network",
    "description": "A simple network topology for testing"
  }'
```

### 4. Add Network Devices

```bash
# Add a Cisco router
curl -X POST http://localhost:5000/api/topology/{topologyId}/devices \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Core Router",
    "hostname": "core-r1",
    "vendorName": "Cisco",
    "positionX": 100,
    "positionY": 100
  }'
```

### 5. Connect via CLI

```bash
# Connect to the device (port will be automatically assigned)
telnet localhost 23001

# Login (any credentials accepted)
Username: admin
Password: password

# Execute commands
core-r1> show version
core-r1> show ip interface brief
core-r1> configure terminal
```

### 6. Query via SNMP

```bash
# Get system description
snmpget -v 2c -c public localhost:16101 1.3.6.1.2.1.1.1.0

# Get system uptime
snmpget -v 2c -c public localhost:16101 1.3.6.1.2.1.1.3.0

# Walk interface table
snmpwalk -v 2c -c public localhost:16101 1.3.6.1.2.1.2.2.1
```

## Supported Vendors

### Cisco IOS

**Available Commands:**
- `show version` - System information and hardware details
- `show ip interface brief` - Interface status and IP configuration
- `show running-config` - Current configuration display
- `show ip route` - Routing table information
- `show processes cpu` - CPU utilization statistics
- `show memory statistics` - Memory usage information
- `configure terminal` - Enter configuration mode
- `interface <name>` - Configure specific interface
- `ip address <ip> <mask>` - Set interface IP address
- `no shutdown` - Enable interface

**SNMP Support:**
- Standard MIB-II (RFC 1213)
- Cisco-specific OIDs
- Interface statistics
- System information

### Juniper JunOS

**Available Commands:**
- `show version` - JunOS system information
- `show interfaces terse` - Interface status (Juniper format)
- `show configuration` - Configuration display
- `show route summary` - Routing table summary
- `show bgp summary` - BGP neighbor status
- `configure` - Enter configuration mode

**SNMP Support:**
- Standard MIB-II
- Juniper-specific OIDs
- Chassis and hardware information

## API Reference

### Topology Management

#### Create Topology
```http
POST /api/topology
Content-Type: application/json

{
  "name": "Production Network",
  "description": "Main production network topology"
}
```

**Response:**
```json
{
  "id": "guid-here",
  "name": "Production Network",
  "description": "Main production network topology",
  "devices": [],
  "connections": [],
  "metadata": {
    "version": "1.0",
    "createdAt": "2025-01-01T00:00:00Z",
    "updatedAt": "2025-01-01T00:00:00Z"
  }
}
```

#### Get Topology
```http
GET /api/topology/{id}
```

#### Update Topology
```http
PUT /api/topology/{id}
Content-Type: application/json

{
  "name": "Updated Network Name",
  "description": "Updated description"
}
```

#### Delete Topology
```http
DELETE /api/topology/{id}
```

### Device Management

#### Add Device
```http
POST /api/topology/{topologyId}/devices
Content-Type: application/json

{
  "name": "Branch Router",
  "hostname": "branch-r1",
  "vendorName": "Cisco",
  "positionX": 200,
  "positionY": 150
}
```

**Supported Vendors:**
- `Cisco` - Cisco IOS devices
- `Juniper` - Juniper JunOS devices

#### Get Devices
```http
GET /api/topology/{topologyId}/devices
```

## CLI Usage Examples

### Basic Device Configuration

```bash
# Connect to device
telnet localhost 23001

# Login
Username: admin
Password: password

# Check device status
core-r1> show version
core-r1> show ip interface brief

# Configure interface
core-r1> configure terminal
core-r1(config)# interface GigabitEthernet0/0
core-r1(config-if)# ip address 192.168.1.1 255.255.255.0
core-r1(config-if)# no shutdown
core-r1(config-if)# end

# Verify configuration
core-r1> show ip interface brief
```

### Routing Configuration

```bash
# Enter configuration mode
core-r1> configure terminal

# Configure static route
core-r1(config)# ip route 10.0.0.0 255.255.255.0 192.168.2.1

# Check routing table
core-r1> show ip route
```

### SNMP Monitoring

```bash
# Get system information
snmpget -v 2c -c public localhost:16101 1.3.6.1.2.1.1.1.0

# Get interface information
snmpwalk -v 2c -c public localhost:16101 1.3.6.1.2.1.2.2.1.2

# Get CPU utilization (Cisco specific)
snmpget -v 2c -c public localhost:16101 1.3.6.1.4.1.9.9.109.1.1.1.1.2.1
```

## Best Practices

### Network Design

1. **Start Simple**: Begin with basic topologies and add complexity gradually
2. **Use Realistic Names**: Use meaningful hostnames and IP addresses
3. **Document Changes**: Keep track of configuration changes and their purposes
4. **Test Connectivity**: Verify device connectivity before complex configurations

### CLI Usage

1. **Check Status First**: Always run `show version` and `show ip interface brief` first
2. **Save Configurations**: Use `write memory` or `copy running-config startup-config` regularly
3. **Use Help**: Most commands support `?` for help and auto-completion
4. **Understand Modes**: Learn the difference between user exec, privileged exec, and configuration modes

### SNMP Monitoring

1. **Use Correct OIDs**: Reference the appropriate MIB for the information you need
2. **Monitor Regularly**: Set up periodic polling for key metrics
3. **Handle Errors**: SNMP operations can fail - implement proper error handling
4. **Security**: Use appropriate community strings and consider SNMPv3 for production

## Troubleshooting

### Common Issues

**Problem**: Device doesn't respond to CLI commands
**Solution**: Check that the device is properly initialized and the correct port is being used

**Problem**: SNMP queries return "No Such Name" errors
**Solution**: Verify the OID is correct and supported by the device vendor

**Problem**: API returns 404 errors
**Solution**: Ensure the topology and device IDs are correct and exist

### Debug Logging

Enable detailed logging by setting the log level:

```bash
export LOG_LEVEL=Debug
dotnet run
```

### Performance Issues

If you experience slow response times:

1. Check system resources (CPU, memory)
2. Reduce the number of concurrent connections
3. Monitor for memory leaks in long-running tests
4. Consider using the in-memory topology service for testing

## Advanced Usage

### Custom Device Configuration

```csharp
// Create a custom device with specific interfaces
var device = new RouterDevice
{
    Name = "Custom Router",
    Hostname = "custom-r1",
    Vendor = new CiscoIosVendorProfile()
};

// Add custom interfaces
device.AddInterface(new NetworkInterface
{
    Name = "GigabitEthernet0/0",
    Type = InterfaceType.GigabitEthernet,
    IpAddress = "10.0.0.1",
    SubnetMask = "255.255.255.0",
    IsUp = true
});
```

### Multi-Device Topologies

```bash
# Create a topology with multiple interconnected devices
curl -X POST http://localhost:5000/api/topology \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Multi-Device Network",
    "description": "Network with multiple interconnected devices"
  }'

# Add multiple devices
# Add router1, router2, switch1, etc.
```

### Automated Testing

```csharp
// Use the API for automated testing
var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

// Create topology
var topology = await CreateTopologyAsync(client);

// Add devices
var router = await AddDeviceAsync(client, topology.Id, "Cisco");

// Execute CLI commands programmatically
var result = await ExecuteCommandAsync(router, "show version");
```

## Support and Community

- **GitHub Issues**: [Report bugs and request features](https://github.com/yourusername/yanets/issues)
- **GitHub Discussions**: [Ask questions and share ideas](https://github.com/yourusername/yanets/discussions)
- **Documentation**: [Full API reference and guides](docs/)

## Contributing

We welcome contributions! See our [Contributing Guide](CONTRIBUTING.md) for details on:
- Adding new vendors
- Implementing new commands
- Extending SNMP support
- Improving documentation

---

**Happy Network Testing with YANETS!** 🎉
