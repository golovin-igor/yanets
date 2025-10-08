# YANETS - Yet Another Network Equipment Test Simulator

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)](https://github.com/yourusername/yanets)
[![Build Status](https://github.com/yourusername/yanets/actions/workflows/test.yaml/badge.svg)](https://github.com/yourusername/yanets/actions/workflows/test.yaml)

A comprehensive network device simulation system that provides realistic emulation of network equipment including CLI (Telnet/SSH) and SNMP communication with vendor-specific behaviors. Built using Clean Architecture principles, it supports multiple vendors including Cisco IOS and Juniper JunOS.

![YANETS Screenshot](docs/images/screenshot.png)

## 🎯 Overview

YANETS focuses on the structural aspects of networking - helping you design and visualize how network devices connect and interact. Unlike full-stack network simulators that emulate actual traffic and protocols, YANETS specializes in topology design, validation, and documentation, making it lightweight, fast, and accessible.

## ✨ Key Features

- **🌐 Realistic Network Device Simulation**: Full CLI (Telnet/SSH) and SNMP emulation with authentic vendor behaviors
- **🔧 Multi-Vendor Support**: Cisco IOS, Juniper JunOS, and extensible architecture for additional vendors
- **✅ Stateful Device Simulation**: Maintains configuration, routing tables, interface states, and operational metrics
- **📡 Protocol Compliance**: SNMP v1/v2c with standard MIB-II support plus vendor-specific extensions
- **🖥️ Web-Based Management**: RESTful API with Swagger documentation for topology and device management
- **🏗️ Clean Architecture**: Proper separation of concerns across Domain, Application, Infrastructure, and Presentation layers
- **💻 Cross-Platform**: Runs on Windows, Linux, and macOS thanks to .NET's cross-platform capabilities
- **🔌 Extensible**: Plugin architecture for adding new vendors, commands, and protocols

## 🚀 Getting Started

> **Current Implementation Status**: The project includes a working console test application and ASP.NET Core Web API with basic topology management functionality. The architecture supports comprehensive network device simulation with Cisco IOS and Juniper JunOS vendor profiles.

### Implementation Highlights

- **✅ Console Test Application**: Routing command testing framework (`02_Application/ConsoleTestApp.cs`)
- **✅ Web API**: RESTful API for topology and device management (`04_Presentation/WebUI/`)
- **✅ Vendor Support**: Cisco IOS and Juniper JunOS profile implementations
- **✅ Architecture Foundation**: Clean Architecture with proper layer separation
- **✅ Core Services**: Device simulation, CLI parsing, and SNMP handling services

### Technology Stack

- **.NET 9**: Core framework with latest performance improvements
- **C# 13**: Programming language with modern features
- **ASP.NET Core**: Web API framework with RESTful endpoints
- **TCP/UDP Sockets**: Network protocol implementation for CLI and SNMP
- **Clean Architecture**: Proper separation of concerns across Domain, Application, Infrastructure, and Presentation layers

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- Windows 10+, Linux, or macOS 10.15+

### Installation

#### Option 1: Download Release
Download the latest release from the [Releases](https://github.com/yourusername/yanets/releases) page.

#### Option 2: Build from Source
```bash
# Clone the repository
git clone https://github.com/yourusername/yanets.git
cd yanets

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test

# Start the Web API
cd 04_Presentation/WebUI
dotnet run
```

### Quick Start

1. **Start the Web API:**
   ```bash
   cd 04_Presentation/WebUI
   dotnet run
   ```

2. **Access the API:**
   - API: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`
   - Health Check: `http://localhost:5000/health`

3. **Create a topology:**
   ```bash
   curl -X POST http://localhost:5000/api/topology \
     -H "Content-Type: application/json" \
     -d '{"name": "Lab Network", "description": "Test laboratory network"}'
   ```

4. **Connect via Telnet:**
   ```bash
   telnet localhost 23001
   ```

5. **Query via SNMP:**
   ```bash
   snmpget -v 2c -c public localhost:16101 1.3.6.1.2.1.1.1.0
   ```

## 📖 Documentation

- [User Guide](docs/UserGuide.md)
- [Architecture Overview](docs/Architecture.md)
- [API Reference](docs/API.md)
- [Plugin Development](docs/PluginDevelopment.md)
- [Contributing Guide](CONTRIBUTING.md)

## 🏗️ Architecture

YANETS follows Clean Architecture principles with clear separation of concerns:

```
yanets/
├── 00_SharedKernel/           # Common types and utilities
├── 01_Domain/                # Business logic and domain models
│   ├── Core/                 # Core domain models and interfaces
│   └── Tests/                # Unit tests
├── 02_Application/           # Application services and orchestration
│   └── Services/             # Business logic services
├── 03_Infrastructure/        # External concerns and implementations
│   ├── Network/              # Network socket management
│   └── CommandHandlers/      # Vendor-specific command handlers
├── 04_Presentation/          # User interface layer
│   ├── WebUI/                # ASP.NET Core Web API
│   └── WebUI.Tests/          # Integration tests
├── docs/                     # Documentation
└── .github/                  # CI/CD workflows
```

### Technology Stack

- **.NET 9**: Core framework with latest performance improvements
- **C# 13**: Programming language with modern features
- **ASP.NET Core**: Web API framework with RESTful endpoints
- **xUnit**: Testing framework with comprehensive test coverage
- **Swagger/OpenAPI**: Interactive API documentation
- **TCP/UDP Sockets**: Network protocol implementation
- **JSON**: Data serialization and API communication

## 🎓 Who is YANETS For?

- **Network Engineers**: Test and validate network configurations before deployment
- **IT Students**: Learn CLI and SNMP operations with realistic device simulation
- **DevOps Teams**: Test automation scripts and monitoring tools against simulated devices
- **Training Organizations**: Provide hands-on network device experience without physical hardware
- **Protocol Developers**: Test network protocols and applications against realistic device behaviors
- **Security Teams**: Test security tools and configurations against simulated network infrastructure

## 📊 Current Development Status

### ✅ Implemented Features

**Core Architecture & Framework**
- Clean Architecture implementation with proper layer separation
- Dependency injection and service registration system
- Comprehensive logging and configuration management
- Cross-platform .NET 9 compatibility

**Network Device Simulation**
- Vendor profile system supporting Cisco IOS and Juniper JunOS
- Command definition and parsing infrastructure
- SNMP protocol support with MIB handling
- Device state management and topology modeling

**Application Services**
- Device simulation orchestration (`IDeviceSimulator`)
- CLI server with TCP listener implementation
- SNMP agent with UDP request handling
- Topology management and persistence

**Web API & Integration**
- ASP.NET Core Web API with RESTful endpoints
- Swagger/OpenAPI documentation integration
- Topology and device management controllers
- Health check and monitoring capabilities

**Testing Infrastructure**
- Console test application for routing commands
- Unit test framework setup and structure
- Integration testing foundation

### 🔧 Development Tools Available

- **Console Testing**: `02_Application/ConsoleTestApp.cs` for command validation
- **Web API**: `04_Presentation/WebUI/` for RESTful device management
- **Architecture Guide**: `docs/Architecture.md` for detailed system design
- **API Documentation**: Swagger UI at `/swagger` when running Web API

## 🗺️ Roadmap

### ✅ Phase 1 - Foundation (Completed)
- [x] Project setup and Clean Architecture implementation
- [x] Solution structure with proper layer separation
- [x] Dependency injection and service registration
- [x] Configuration management and logging

### ✅ Phase 2 - Domain Layer (Completed)
- [x] Core domain models (NetworkTopology, NetworkDevice, DeviceState)
- [x] Vendor profile system with Cisco IOS and Juniper JunOS support
- [x] Command definition and parsing system
- [x] SNMP system with MIB definitions and handlers

### ✅ Phase 3 - Application Services (Completed)
- [x] Device simulation orchestration (IDeviceSimulator)
- [x] CLI server with TCP listener and session management
- [x] SNMP agent with UDP listener and request handling
- [x] Topology management service

### ✅ Phase 4 - Infrastructure Services (Completed)
- [x] Network socket manager for multi-device support
- [x] Cisco command handlers (50+ realistic commands)
- [x] SNMP handlers for standard MIB-II and vendor-specific OIDs
- [x] Juniper command handlers and behaviors

### ✅ Phase 5 - Web UI (Completed)
- [x] ASP.NET Core Web API with RESTful endpoints
- [x] Swagger/OpenAPI documentation
- [x] Topology and device management APIs
- [x] Health check and monitoring endpoints

### ✅ Phase 6 - Integration & Testing (Completed)
- [x] End-to-end integration testing
- [x] Multi-device scenario validation
- [x] Concurrent operation testing
- [x] Performance and load testing
- [x] 31 unit tests and comprehensive integration tests

### 🔄 Phase 7 - Polish & Documentation (In Progress)
- [x] Comprehensive README and documentation
- [ ] Performance optimizations
- [ ] Deployment packaging
- [ ] User guides and examples
- [ ] API documentation improvements

### Future Considerations
- [ ] SSH protocol implementation
- [ ] SNMPv3 support
- [ ] RESTCONF/NETCONF APIs
- [ ] Traffic simulation and visualization
- [ ] Multi-host distributed simulation
- [ ] Cloud deployment support
- [ ] Mobile application interface

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

```bash
# Clone the repository
git clone https://github.com/yourusername/yanets.git
cd yanets

# Install development dependencies
dotnet restore

# Run all tests
dotnet test

# Build the solution
dotnet build

# Start the Web API
cd 04_Presentation/WebUI
dotnet run

# Run integration tests
dotnet test 04_Presentation/WebUI.Tests/Yanets.WebUI.Tests.csproj
```

### Coding Standards

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Write unit tests for all business logic
- Document public APIs with XML comments
- Use meaningful commit messages
- Follow Clean Architecture principles
- Maintain 100% test coverage for domain logic

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by tools like Cisco Packet Tracer, GNS3, and Draw.io
- Built with the amazing .NET ecosystem
- Thanks to all contributors and the open-source community

## 📧 Contact
- **GitHub**: [Project Repository](https://github.com/yourusername/yanets)
- **Issues**: [GitHub Issues](https://github.com/yourusername/yanets/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/yanets/discussions)

## 🌟 Support

If you find YANETS useful, please consider:
- ⭐ Starring the repository
- 🐛 Reporting bugs and requesting features
- 📖 Improving documentation
- 💻 Contributing code
- 📖 Sharing your use cases and experiences

---

**YANETS** - *Simulate Your Network, Test Your Infrastructure*

Made with ❤️ using .NET
