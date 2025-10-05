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

- **Network Architects**: Design and validate network architectures before implementation
- **IT Students**: Learn network topology concepts and prepare for certifications (CCNA, CCNP, etc.)
- **System Administrators**: Document existing networks and plan infrastructure changes
- **Technical Leads**: Communicate network designs to stakeholders with clear visualizations
- **Consultants**: Rapidly prototype network solutions for client proposals

## 🗺️ Roadmap

### Phase 1 - Foundation (Current)
- [x] Project setup and architecture
- [ ] Core domain models
- [ ] Basic UI framework
- [ ] Device library with common network devices
- [ ] Canvas with drag-and-drop functionality

### Phase 2 - Core Features
- [ ] Connection management (cables, links)
- [ ] Property panels and device configuration
- [ ] File save/load (JSON format)
- [ ] Topology validation engine
- [ ] Export to image formats (PNG, SVG)

### Phase 3 - Advanced Features
- [ ] Multi-vendor device support
- [ ] Advanced topology validation rules
- [ ] Network documentation generator
- [ ] Layer 2/Layer 3 path analysis
- [ ] Redundancy and SPOF detection

### Phase 4 - Extensibility
- [ ] Plugin system architecture
- [ ] Custom device type support
- [ ] Import/export from other tools (Visio, Draw.io)
- [ ] API for automation
- [ ] Template library

### Future Considerations
- [ ] Collaboration features (multi-user editing)
- [ ] Cloud topology integration (Azure, AWS, GCP)
- [ ] IP address management (IPAM)
- [ ] Basic configuration generation
- [ ] Integration with network management tools

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

```bash
# Clone the repository
git clone https://github.com/golovin-igor/yanets.git
cd yanets

# Install development dependencies
dotnet restore

# Run tests
dotnet test

# Run the application in development mode
dotnet run --project src/Yanets.UI
```

### Coding Standards

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Write unit tests for all business logic
- Document public APIs with XML comments
- Use meaningful commit messages

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by tools like Cisco Packet Tracer, GNS3, and Draw.io
- Built with the amazing .NET ecosystem
- Thanks to all contributors and the open-source community

## 📧 Contact
- **GitHub**: [@golovin-igor](https://github.com/golovin-igor)
- **Issues**: [GitHub Issues](https://github.com/golovin-igor/yanets/issues)

## 🌟 Support

If you find YANETS useful, please consider:
- ⭐ Starring the repository
- 🐛 Reporting bugs and requesting features
- 📖 Improving documentation
- 💻 Contributing code

---

**YANETS** - *Design Your Network, Visualize Your Infrastructure*

Made with ❤️ using .NET
