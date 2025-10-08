# YANETS Virtual Host Console Application

A cross-platform console application for simulating network hosts with virtual IP addresses using a Docker-inspired networking approach.

## 🚀 Features

- **Cross-platform**: Runs on Windows, Linux, and macOS
- **Virtual Networking**: Docker-like subnet management with automatic IP allocation
- **No Admin Privileges**: Port-based IP simulation using localhost ports
- **Multi-Vendor Support**: Cisco IOS, Juniper JunOS, and custom vendor profiles
- **Interactive CLI**: Real-time network management and monitoring
- **Protocol Simulation**: Telnet, SNMP, and SSH protocol handling
- **Automatic Startup**: Configurable automatic network initialization

## 🏗️ Architecture

The application follows a clean architecture pattern with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    Console Application                      │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              Interactive CLI Shell                  │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────────────────────┐
│                 Virtual Network Manager                     │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              Subnet Manager                         │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────────────────────┐
│                 Connection Router                           │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              Protocol Handlers                      │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────────────────────┐
│                 Host Simulation Engine                      │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              Virtual Host Array                     │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

## 🛠️ Technology Stack

- **Runtime**: .NET 9+ for cross-platform compatibility
- **Language**: C# 13 with modern language features
- **CLI Framework**: System.CommandLine for command parsing
- **Networking**: System.Net.Sockets for TCP/UDP handling
- **Configuration**: Microsoft.Extensions.Configuration
- **Logging**: Microsoft.Extensions.Logging

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later

### Running the Application

#### Option 1: Interactive Mode (Recommended)
```bash
# Navigate to the project directory
cd 05_VirtualHostConsole

# Run in interactive mode
dotnet run

# The application will start and show:
# YANETS Virtual Host Console
# ==========================
# Type 'help' for commands or 'exit' to quit
```

#### Option 2: Command Line Mode
```bash
# Create a host
dotnet run -- create-host router1 --vendor cisco

# Start the network
dotnet run -- start

# Show status
dotnet run -- status
```

### Basic Usage Examples

#### 1. Create a Virtual Network
```bash
# The application automatically creates a default subnet (192.168.1.0/24)
# when the first host is created
```

#### 2. Create Virtual Hosts
```bash
# Create a Cisco router
create host router1 --vendor cisco

# Create a Juniper switch
create host switch1 --vendor juniper

# Create multiple hosts at once
create host router2 --vendor cisco
create host switch2 --vendor juniper
```

#### 3. Start Network Simulation
```bash
# Start all virtual hosts
start

# Check status
status

# List all hosts
list
```

#### 4. Connect to Virtual Hosts
```bash
# Show connection information
connect router1

# The output will show:
# Connection information for router1:
# IP Address: 192.168.1.1
# Telnet: 192.168.1.1:23
# SNMP: 192.168.1.1:161
```

#### 5. Use Telnet to Connect
```bash
# In a separate terminal, connect via telnet
telnet 192.168.1.1

# Login with default credentials
Username: admin
Password: password

# You should see the router prompt
router1>
```

## 📋 Command Reference

### Network Management Commands

| Command | Description | Example |
|---------|-------------|---------|
| `create subnet <cidr>` | Create a virtual subnet | `create subnet 10.0.0.0/24` |
| `create host <name>` | Create a virtual host | `create host router1 --vendor cisco` |
| `start` | Start the virtual network | `start` |
| `stop` | Stop the virtual network | `stop` |
| `status` | Show network status | `status` |
| `list` | List all virtual hosts | `list` |
| `save <filename>` | Save network configuration | `save network.json` |
| `load <filename>` | Load network configuration | `load network.json` |

### Host Management Commands

| Command | Description | Example |
|---------|-------------|---------|
| `connect <hostname>` | Show connection info for host | `connect router1` |
| `remove <hostname>` | Remove a virtual host | `remove router1` |
| `show interfaces <hostname>` | Show host interfaces | `show interfaces router1` |
| `configure <hostname>` | Enter configuration mode | `configure router1` |

### Interactive Mode Commands

When running in interactive mode, use these commands:

```bash
help                    # Show available commands
status                  # Show network status
list                    # List all hosts
create host <name>      # Create a new host
start                   # Start the network
stop                    # Stop the network
connect <hostname>      # Show connection info
clear                   # Clear the screen
exit                    # Exit the application
```

## 🌐 Virtual IP Address Mapping

The application uses a port-based mapping strategy to simulate virtual IP addresses:

| Virtual IP | Protocol | Maps To | Description |
|------------|----------|---------|-------------|
| 192.168.1.1 | Telnet (23) | 127.0.0.1:23001 | Primary management interface |
| 192.168.1.1 | SNMP (161) | 127.0.0.1:16101 | SNMP agent |
| 192.168.1.1 | SSH (22) | 127.0.0.1:22001 | Secure shell (planned) |
| 192.168.1.2 | Telnet (23) | 127.0.0.1:23002 | Second host |
| ... | ... | ... | ... |

## ⚙️ Configuration

The application uses `appsettings.json` for configuration:

```json
{
  "Network": {
    "DefaultSubnet": "192.168.1.0/24",
    "Gateway": "192.168.1.1",
    "AutoStart": false,
    "MaxHosts": 100
  },
  "Hosts": {
    "Defaults": {
      "Vendor": "cisco",
      "DeviceType": "router"
    }
  },
  "Security": {
    "DefaultUsername": "admin",
    "DefaultPassword": "password",
    "SnmpCommunity": "public"
  }
}
```

## 🔧 Advanced Usage

### Custom Subnet Creation
```bash
# Create a custom subnet
create subnet 10.0.0.0/16 --gateway 10.0.0.1

# Create hosts in the custom subnet
create host server1 --vendor linux --subnet custom
```

### Configuration Persistence
```bash
# Save current network configuration
save my-network.json

# Load configuration on startup
load my-network.json
```

### Real-time Monitoring
```bash
# Enable real-time status display
monitor on

# Disable monitoring
monitor off
```

## 🔒 Security Features

- **Authentication**: Telnet/SSH authentication with configurable credentials
- **SNMP Security**: Community string validation for SNMP access
- **Session Management**: Automatic session timeout and cleanup
- **Input Validation**: Comprehensive command and parameter validation

## 📊 Monitoring and Statistics

The application provides real-time monitoring capabilities:

```bash
# Show detailed statistics
show statistics

# Monitor specific host
monitor host router1

# Show connection details
show connections
```

## 🚨 Troubleshooting

### Common Issues

1. **Port Conflicts**
   - Error: "Port already in use"
   - Solution: Check if ports 23000-24000 are available

2. **Host Creation Fails**
   - Error: "No available IP addresses"
   - Solution: Check subnet configuration or create a larger subnet

3. **Connection Refused**
   - Error: "Connection refused" when connecting via telnet
   - Solution: Ensure the virtual network is started with `start` command

### Debug Mode

Enable debug logging by modifying `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

## 🤝 Integration with YANETS

This console application integrates with the main YANETS ecosystem:

- **Shared Kernel**: Uses common types and utilities
- **Domain Models**: Leverages existing device and network models
- **Vendor Profiles**: Compatible with existing vendor implementations
- **Protocol Handlers**: Extends current CLI and SNMP handling

## 📈 Performance

### Tested Configurations

| Configuration | Max Hosts | Concurrent Connections | Memory Usage |
|---------------|-----------|----------------------|--------------|
| Development | 10 | 50 | ~50MB |
| Small Lab | 50 | 200 | ~200MB |
| Medium Lab | 100 | 500 | ~500MB |

### Performance Tuning

- **Connection Pooling**: Automatic cleanup of idle connections
- **Memory Management**: Efficient state storage and garbage collection
- **Async Operations**: All I/O operations are fully asynchronous

## 🔮 Future Enhancements

### Planned Features
- **SSH Protocol Support**: Secure shell connectivity
- **Web Interface**: Browser-based management interface
- **REST API**: Programmatic access to simulation features
- **Traffic Simulation**: Basic packet flow between hosts
- **SNMPv3 Support**: Advanced SNMP with authentication

### Advanced Capabilities
- **Multi-Host Communication**: Inter-host network traffic
- **VLAN Support**: Virtual LAN configuration
- **QoS Simulation**: Quality of service testing
- **Network Events**: Link failure simulation

## 📚 Related Documentation

- [VirtualHostArchitecture.md](docs/VirtualHostArchitecture.md) - Detailed architecture guide
- [Architecture.md](../docs/Architecture.md) - Main YANETS architecture
- [UserGuide.md](../docs/UserGuide.md) - User manual

---

**YANETS Virtual Host Console** - *Simulate Network Devices, Test Your Infrastructure*