# Virtual Host Simulation Console Application - Architecture Document

## Overview

This document describes the architecture for a cross-platform console application that simulates network hosts with virtual IP addresses using a Docker-inspired networking approach. The system enables users to create and manage multiple virtual network devices, each with their own IP addresses from configurable subnets, all running on a single machine without requiring administrative privileges.

## 🎯 Objectives

- **Cross-platform Compatibility**: Single codebase supporting Windows, Linux, and macOS
- **Virtual Networking**: Docker-like subnet management with automatic IP allocation
- **No Admin Privileges**: Port-based IP simulation using localhost ports
- **Realistic Simulation**: Authentic vendor-specific network device behaviors
- **Interactive Management**: Real-time CLI for network configuration and monitoring
- **Scalable Architecture**: Support hundreds of virtual hosts on single machine

## 🏗️ System Architecture

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    Console Application                          │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │              Interactive CLI Shell                      │    │
│  │  ┌─────────────────────────────────────────────────────┐  │    │
│  │  │            Command Parser & Executor                │  │    │
│  │  └─────────────────────────────────────────────────────┘  │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────────────────────────┐
│                 Virtual Network Manager                         │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │              Subnet Manager                             │    │
│  │  ┌─────────────────────────────────────────────────────┐  │    │
│  │  │         IP Address Pool & Allocator                │  │    │
│  │  └─────────────────────────────────────────────────────┘  │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────────────────────────┐
│                 Connection Router                               │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │              Protocol Detector                          │    │
│  │  ┌─────────────────────────────────────────────────────┐  │    │
│  │  │         Session Manager & Pool                      │  │    │
│  │  └─────────────────────────────────────────────────────┘  │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────────────────────────┐
│                 Host Simulation Engine                          │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │              Virtual Host Array                         │    │
│  │  ┌─────────────────────────────────────────────────────┐  │    │
│  │  │  ┌─────────────────────────────────────────────────┐  │  │    │
│  │  │  │           Individual Network Stacks             │  │  │    │
│  │  │  │  ┌─────────────────────────────────────────────┐  │  │  │    │
│  │  │  │  │        Protocol Handlers                    │  │  │  │    │
│  │  │  │  │  ┌─────────────────────────────────────────┐  │  │  │  │    │
│  │  │  │  │  │     Device State Management             │  │  │  │  │    │
│  │  │  │  │  └─────────────────────────────────────────┘  │  │  │  │    │
│  │  │  │  └─────────────────────────────────────────────┘  │  │  │    │
│  │  │  └─────────────────────────────────────────────────┘  │  │    │
│  │  └─────────────────────────────────────────────────────┘  │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

## 🏢 Core Components

### 1. Virtual Network Manager

**Primary Responsibility**: Central orchestration of the virtual network simulation.

**Key Features**:
- Network lifecycle management (start/stop)
- Host registration and tracking
- Resource allocation coordination
- Statistics aggregation

**Core Interfaces**:
```csharp
public interface IVirtualNetworkManager
{
    Task<IVirtualHost> CreateHost(string hostname, string vendor, string subnet);
    Task<bool> StartNetworkAsync();
    Task<bool> StopNetworkAsync();
    NetworkStatistics GetNetworkStatistics();
    IEnumerable<IVirtualHost> GetAllHosts();
}
```

### 2. Subnet Management System

**Primary Responsibility**: Docker-like subnet management and IP address allocation.

**Key Features**:
- CIDR subnet definitions (192.168.1.0/24, 10.0.0.0/16)
- Automatic IP address allocation
- Manual IP assignment support
- Duplicate IP detection and prevention
- Configuration persistence

**Core Classes**:
```csharp
public class SubnetDefinition
{
    public string Cidr { get; set; }           // "192.168.1.0/24"
    public string Gateway { get; set; }        // "192.168.1.1"
    public string Name { get; set; }           // "default"
    public List<string> DnsServers { get; set; }
    public HashSet<string> ExcludedIps { get; set; }
}

public class IpAddressPool
{
    public SubnetDefinition Subnet { get; }
    public ConcurrentHashSet<string> AllocatedIps { get; }
    public ConcurrentHashSet<string> AvailableIps { get; }

    public string AllocateNextAvailableIp();
    public bool AllocateSpecificIp(string ip);
    public void ReleaseIp(string ip);
    public bool IsIpAvailable(string ip);
    public int GetAvailableCount();
}
```

### 3. Host Simulation Engine

**Primary Responsibility**: Individual network stacks for each virtual host.

**Key Features**:
- Isolated network state per host
- Vendor-specific command implementations
- Multi-protocol support (Telnet, SSH, SNMP)
- Real-time state management
- Configuration persistence

**Core Architecture**:
```csharp
public class VirtualHost : IVirtualHost
{
    public string Id { get; }
    public string Hostname { get; set; }
    public string IpAddress { get; set; }
    public VendorProfile VendorProfile { get; set; }
    public NetworkStack NetworkStack { get; }
    public DeviceState CurrentState { get; set; }
    public HostStatus Status { get; set; }

    // Lifecycle Management
    public Task StartAsync();
    public Task StopAsync();

    // Command Execution
    public Task<CommandResult> ExecuteCommandAsync(string command, CliSession session);

    // Network Operations
    public Task<SnmpResponse> HandleSnmpRequestAsync(SnmpRequest request);
    public NetworkInterface GetInterface(string name);
    public void UpdateInterfaceState(string interfaceName, InterfaceStatus status);
}
```

### 4. Connection Handling System

**Primary Responsibility**: Route connections and manage protocol sessions.

**Key Features**:
- Multi-protocol connection handling
- Automatic protocol detection
- Session lifecycle management
- Load distribution across ports
- Connection pooling and cleanup

**Core Components**:
```csharp
public class ConnectionRouter
{
    private readonly Dictionary<int, IVirtualHost> _portToHostMapping;
    private readonly ProtocolDetector _protocolDetector;
    private readonly SessionManager _sessionManager;

    public async Task RouteTcpConnectionAsync(TcpClient client, int localPort);
    public async Task RouteUdpPacketAsync(UdpReceiveResult packet, int localPort);
    public void RegisterHostPorts(IVirtualHost host, Dictionary<ProtocolType, int> portMappings);
    public void UnregisterHostPorts(IVirtualHost host);
}

public class ProtocolHandler
{
    public ProtocolType Type { get; }
    public IVirtualHost Host { get; }

    public Task HandleConnectionAsync(NetworkStream stream);
    public Task<byte[]> ProcessDataAsync(byte[] data);
    public void CloseConnection();
}
```

### 5. Console Application Interface

**Primary Responsibility**: Interactive command-line interface for network management.

**Key Features**:
- Real-time command parsing and execution
- Interactive shell with tab completion
- Real-time network monitoring display
- Configuration save/load functionality
- Comprehensive logging integration

**Command Structure**:
```bash
# Network Management
create subnet 192.168.1.0/24 --gateway 192.168.1.1 --name default
create host router1 --vendor cisco --subnet default --hostname R1
create host switch1 --vendor cisco --subnet default --hostname SW1
start network
stop network

# Host Configuration
connect router1
show running-config
configure terminal
interface gigabitethernet 0/1
  ip address 192.168.1.1 255.255.255.0
  no shutdown
exit

# Network Monitoring
show hosts
show connections
show statistics
show interfaces
monitor traffic --host router1
```

## 🌐 Virtual IP Address Mapping Strategy

### Port-Based IP Simulation

The system uses a port-mapping strategy to simulate virtual IP addresses:

```
Virtual Network Space          │     Physical Implementation
═══════════════════════════════│════════════════════════════════
192.168.1.1:23 (Telnet)       →  127.0.0.1:23001
192.168.1.1:161 (SNMP)        →  127.0.0.1:16101
192.168.1.1:22 (SSH)          →  127.0.0.1:22001
192.168.1.2:23 (Telnet)       →  127.0.0.1:23002
192.168.1.2:161 (SNMP)        →  127.0.0.1:16102
192.168.1.2:22 (SSH)          →  127.0.0.1:22002
...
```

### IP Address Allocation Algorithm

1. **Subnet Validation**: Parse CIDR notation and calculate valid IP range
2. **Exclusion Processing**: Remove reserved IPs (network, broadcast, gateway)
3. **Availability Check**: Scan for port conflicts on localhost
4. **Sequential Allocation**: Assign next available IP from pool
5. **Port Mapping**: Calculate unique localhost ports for each protocol
6. **Registration**: Register host with connection router

## 🔄 Data Flow Architecture

### Connection Establishment Flow

```
External Client Request
        │
        ▼
[Localhost Port] → [Connection Router] → [Protocol Detection]
        │                     │
        ▼                     ▼
[Port-to-Host Mapping] ← [Session Manager] → [Virtual Host Lookup]
        │                     │
        ▼                     ▼
[Virtual Host] → [Protocol Handler] → [Vendor Command Processing]
        │                     │
        ▼                     ▼
[Device State] → [Response Generation] → [Client Response]
```

### Host Creation Flow

```
User Command → [CLI Parser] → [VirtualNetworkManager.CreateHost()]
        │
        ▼
[SubnetManager] → [IP Allocation] → [Port Mapping Calculation]
        │
        ▼
[VirtualHost] → [NetworkStack Init] → [Protocol Server Startup]
        │
        ▼
[ConnectionRouter] → [Port Registration] → [Ready for Connections]
```

### Command Execution Flow

```
CLI Input → [Command Parser] → [VirtualHost.ExecuteCommand()]
        │
        ▼
[VendorProfile] → [Command Definition] → [Privilege Check]
        │
        ▼
[Command Handler] → [Device State Update] → [Response Generation]
        │
        ▼
[CLI Output] → [User Display]
```

## 💾 State Management Architecture

### Device State Model

```csharp
public class DeviceState
{
    public string RunningConfiguration { get; set; }
    public string StartupConfiguration { get; set; }
    public RoutingTable RoutingTable { get; set; }
    public ArpTable ArpTable { get; set; }
    public MacAddressTable MacAddressTable { get; set; }
    public List<VlanConfiguration> Vlans { get; set; }
    public SystemResources Resources { get; set; }
    public Dictionary<string, object> Variables { get; set; }
    public DateTime LastUpdated { get; set; }
    public DeviceStatus Status { get; set; }
}
```

### State Persistence Strategy

- **In-Memory State**: Primary state storage for performance
- **Configuration Files**: JSON/YAML for startup configurations
- **Change Tracking**: Delta-based state updates
- **Snapshot Support**: Point-in-time state capture
- **Atomic Updates**: Transaction-like state changes

## 🔧 Protocol Implementation Architecture

### Supported Protocols

| Protocol | Default Port | Implementation Status |
|----------|--------------|----------------------|
| Telnet   | 23          | ✅ Complete         |
| SSH      | 22          | 🔄 Planned          |
| SNMP     | 161         | ✅ Complete         |
| HTTP     | 80          | 🔄 Planned          |
| HTTPS    | 443         | 🔄 Planned          |

### Protocol Handler Architecture

```csharp
public abstract class ProtocolHandlerBase : IProtocolHandler
{
    public ProtocolType Type { get; }
    public IVirtualHost Host { get; }
    public ILogger Logger { get; }

    public abstract Task HandleConnectionAsync(Stream stream);
    public abstract Task<byte[]> ProcessDataAsync(byte[] data);
    protected virtual void OnConnectionClosed() { }
}

public class TelnetProtocolHandler : ProtocolHandlerBase
{
    private CliSession _currentSession;
    private readonly IPromptGenerator _promptGenerator;

    public override async Task HandleConnectionAsync(Stream stream)
    {
        // Telnet negotiation and session establishment
        await NegotiateTelnetOptions(stream);

        // Authentication flow
        var authResult = await AuthenticateUser(stream);
        if (!authResult) return;

        // Command loop
        await RunCommandLoop(stream);
    }
}
```

## 📊 Monitoring and Statistics Architecture

### Real-Time Monitoring

```csharp
public class NetworkStatistics
{
    public int TotalHosts { get; set; }
    public int ActiveHosts { get; set; }
    public int TotalConnections { get; set; }
    public int ActiveConnections { get; set; }
    public Dictionary<ProtocolType, int> ProtocolCounts { get; set; }
    public Dictionary<string, HostStatistics> PerHostStats { get; set; }
    public NetworkTrafficStatistics TrafficStats { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class HostStatistics
{
    public string HostId { get; set; }
    public int ActiveConnections { get; set; }
    public int TotalCommandsExecuted { get; set; }
    public int TotalSnmpRequests { get; set; }
    public DateTime Uptime { get; set; }
    public double CpuUtilization { get; set; }
    public long MemoryUsed { get; set; }
}
```

### Statistics Collection Strategy

- **Connection Events**: Track connection establishment/termination
- **Command Execution**: Monitor CLI command processing
- **SNMP Operations**: Track MIB access patterns
- **Resource Usage**: Monitor memory and CPU per host
- **Error Tracking**: Collect and categorize errors

## 🔒 Security Architecture

### Connection Security

- **Protocol-Level Security**: Telnet/SSH authentication mechanisms
- **SNMP Community Strings**: Configurable read/write community validation
- **Session Management**: Automatic session timeout and cleanup
- **Input Validation**: Comprehensive command and parameter validation

### Access Control

```csharp
public class AccessControlManager
{
    public bool ValidateSnmpCommunity(string community, SnmpAccessType accessType);
    public bool ValidateCliCredentials(string username, string password);
    public bool CheckCommandPrivilege(CommandDefinition command, CliSession session);
    public void AuditSecurityEvent(SecurityEvent securityEvent);
}
```

## 📈 Scalability Architecture

### Performance Considerations

- **Connection Pooling**: Reuse connections and sessions where possible
- **Async Operations**: All I/O operations are fully asynchronous
- **Resource Limits**: Configurable limits per host and globally
- **Memory Management**: Efficient state storage and cleanup
- **Thread Management**: Proper thread pool configuration

### Scaling Targets

| Metric | Target | Implementation Strategy |
|--------|--------|------------------------|
| Virtual Hosts | 100+ | Efficient state management |
| Concurrent Connections | 500+ | Async I/O and connection pooling |
| SNMP Requests/sec | 1000+ | Optimized MIB handling |
| CLI Commands/sec | 100+ | Cached command definitions |
| Memory Usage | < 500MB | Compact state representation |

## 🛠️ Implementation Architecture

### Project Structure

```
yanets-console/
├── src/
│   ├── Yanets.VirtualHost/                    # Main application
│   │   ├── Core/                             # Core abstractions
│   │   │   ├── Interfaces/                   # Service interfaces
│   │   │   ├── Models/                       # Core data models
│   │   │   └── Services/                     # Core services
│   │   ├── Networking/                       # Network simulation
│   │   │   ├── VirtualNetworkManager.cs     # Network orchestration
│   │   │   ├── SubnetManager.cs             # Subnet management
│   │   │   ├── ConnectionRouter.cs          # Connection routing
│   │   │   └── ProtocolHandlers/            # Protocol implementations
│   │   ├── Simulation/                       # Host simulation
│   │   │   ├── VirtualHost.cs               # Host implementation
│   │   │   ├── NetworkStack.cs              # Network stack
│   │   │   ├── DeviceState.cs               # Device state
│   │   │   └── VendorProfiles/              # Vendor behaviors
│   │   ├── Console/                          # CLI interface
│   │   │   ├── ConsoleApplication.cs        # Main application
│   │   │   ├── CommandParser.cs             # Command parsing
│   │   │   ├── InteractiveShell.cs          # Interactive shell
│   │   │   └── StatusDisplay.cs             # Real-time display
│   │   └── Shared/                           # Shared utilities
│   │       ├── Logging/                      # Logging framework
│   │       ├── Configuration/                # Configuration management
│   │       └── Extensions/                   # Utility extensions
│   ├── Yanets.VirtualHost.Tests/             # Test projects
│   │   ├── UnitTests/                        # Unit tests
│   │   ├── IntegrationTests/                 # Integration tests
│   │   └── PerformanceTests/                 # Performance tests
│   └── Yanets.VirtualHost.CLI/               # CLI executable
├── docs/                                     # Documentation
├── scripts/                                  # Build scripts
├── build/                                    # Build output
└── config/                                   # Configuration files
```

### Development Phases

#### Phase 1: Core Infrastructure (Week 1-2)
- [x] Project setup and structure
- [x] Basic VirtualNetworkManager implementation
- [x] Subnet management and IP allocation
- [x] Simple console interface

#### Phase 2: Host Simulation (Week 3-4)
- [x] VirtualHost implementation
- [x] NetworkStack with routing tables
- [x] Basic device state management
- [x] Vendor profile integration

#### Phase 3: Protocol Implementation (Week 5-6)
- [x] Telnet protocol handler
- [x] SNMP protocol handler
- [x] Connection routing system
- [x] Session management

#### Phase 4: Advanced Features (Week 7-8)
- [x] Interactive CLI shell
- [x] Real-time monitoring
- [x] Configuration persistence
- [x] Error handling and logging

#### Phase 5: Polish and Testing (Week 9-10)
- [x] Cross-platform testing
- [x] Performance optimization
- [x] Documentation completion
- [x] User acceptance testing

## 🔧 Technology Stack

### Runtime and Framework
- **.NET 9+**: Cross-platform runtime with latest performance improvements
- **C# 13**: Modern language features and pattern matching
- **ASP.NET Core**: Web API foundation (for future web interface)
- **System.CommandLine**: Modern command-line parsing

### Networking
- **System.Net.Sockets**: TCP/UDP socket management
- **System.IO.Pipelines**: High-performance I/O for protocol handling
- **Microsoft.AspNetCore.Connections**: Advanced connection management

### Utilities and Infrastructure
- **Microsoft.Extensions.DependencyInjection**: IoC container
- **Microsoft.Extensions.Logging**: Comprehensive logging framework
- **Microsoft.Extensions.Configuration**: Configuration management
- **System.Text.Json**: High-performance JSON serialization

### Development and Testing
- **xUnit**: Modern testing framework
- **Moq**: Mocking framework for unit tests
- **Coverlet**: Code coverage analysis
- **BenchmarkDotNet**: Performance benchmarking

## ✅ Benefits and Use Cases

### Key Benefits

1. **Zero-Configuration Networking**: No network setup or admin privileges required
2. **Docker-Compatible**: Familiar subnet and IP management approach
3. **Cross-Platform**: Single binary works on Windows, Linux, macOS
4. **Realistic Simulation**: Authentic vendor behaviors and responses
5. **Easy Scaling**: Support hundreds of devices on modest hardware
6. **Development Friendly**: Hot-reload configuration and real-time monitoring
7. **Production Ready**: Comprehensive logging, error handling, and monitoring

### Primary Use Cases

#### Network Training and Education
- Learn network device configuration without physical hardware
- Practice CLI commands and troubleshooting scenarios
- Understand network protocols and behaviors

#### Development and Testing
- Test network management tools and scripts
- Validate network automation workflows
- Develop monitoring and alerting systems

#### Protocol Development
- Test SNMP MIB implementations
- Validate network protocol compliance
- Develop custom network applications

#### Security Testing
- Test network security tools and configurations
- Validate firewall and access control rules
- Security assessment and penetration testing

## 🚀 Getting Started

### Basic Usage

```bash
# Start the application
dotnet run --project src/Yanets.VirtualHost.CLI

# Create a virtual network
create subnet 192.168.1.0/24 --gateway 192.168.1.1

# Create virtual hosts
create host router1 --vendor cisco --hostname R1
create host switch1 --vendor cisco --hostname SW1

# Start network simulation
start network

# Connect to hosts
telnet 192.168.1.1
snmpwalk -v 2c -c public 192.168.1.1
```

### Configuration Example

```json
{
  "Network": {
    "DefaultSubnet": "192.168.1.0/24",
    "Gateway": "192.168.1.1",
    "DnsServers": ["8.8.8.8", "1.1.1.1"]
  },
  "Hosts": {
    "Defaults": {
      "Vendor": "cisco",
      "DeviceType": "router"
    }
  },
  "Logging": {
    "LogLevel": "Information",
    "LogFile": "logs/virtualhost.log"
  }
}
```

## 🔮 Future Enhancements

### Planned Features
- **SSH Protocol Support**: Secure shell connectivity
- **Web Interface**: Browser-based management interface
- **REST API**: Programmatic access to simulation features
- **Traffic Simulation**: Basic packet flow between hosts
- **SNMPv3 Support**: Advanced SNMP with authentication and encryption
- **Custom Vendor Profiles**: Easy creation of new device types

### Advanced Capabilities
- **Multi-Host Networking**: Communication between virtual hosts
- **VLAN Support**: Virtual LAN configuration and isolation
- **QoS Simulation**: Quality of service policy testing
- **Network Events**: Link up/down and failure simulation
- **Performance Monitoring**: Detailed metrics and graphing

---

This architecture provides a solid foundation for creating a powerful, cross-platform network simulation tool that brings enterprise-grade network testing capabilities to any development environment without requiring special network configurations or administrative privileges.