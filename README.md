# YANETS - Yet Another NEtwork Topology Simulator

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)](https://github.com/golovin-igor/yanets)

A modern, cross-platform network topology simulator designed for network architects, engineers, students, and IT professionals. Built on the robust .NET platform, YANETS provides an intuitive environment for designing, visualizing, and analyzing complex network topologies.

![YANETS Screenshot](docs/images/screenshot.png)

## 🎯 Overview

YANETS focuses on the structural aspects of networking - helping you design and visualize how network devices connect and interact. Unlike full-stack network simulators that emulate actual traffic and protocols, YANETS specializes in topology design, validation, and documentation, making it lightweight, fast, and accessible.

## ✨ Key Features

- **🎨 Visual Topology Designer**: Drag-and-drop interface for creating network diagrams with industry-standard device representations
- **🔧 Multi-Vendor Support**: Define and simulate topologies with devices from Cisco, Juniper, Arista, HP, Dell, and custom vendors
- **✅ Topology Validation**: Automated checks for common design issues, redundancy paths, and best practices
- **📚 Device Library**: Extensive catalog of routers, switches, firewalls, load balancers, servers, and network appliances
- **📄 Export & Documentation**: Generate professional network diagrams, topology reports, and technical documentation
- **🏗️ Hierarchical Design**: Support for campus, data center, WAN, and cloud network architectures
- **💻 Cross-Platform**: Runs on Windows, Linux, and macOS thanks to .NET's cross-platform capabilities
- **🔌 Extensible**: Plugin architecture for custom device types and validation rules

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- Windows 10+, Linux, or macOS 10.15+

### Installation

#### Option 1: Download Release
Download the latest release from the [Releases](https://github.com/golovin-igor/yanets/releases) page.

#### Option 2: Build from Source
```bash
# Clone the repository
git clone https://github.com/golovin-igor/yanets.git
cd yanets

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project src/Yanets.UI
```

### Quick Start

1. Launch YANETS
2. Create a new topology project
3. Drag devices from the toolbox onto the canvas
4. Connect devices by clicking and dragging between ports
5. Configure device properties in the properties panel
6. Validate your topology using the built-in validator
7. Export your design to PNG, SVG, or PDF

## 📖 Documentation

- [User Guide](docs/UserGuide.md)
- [Architecture Overview](docs/Architecture.md)
- [API Reference](docs/API.md)
- [Plugin Development](docs/PluginDevelopment.md)
- [Contributing Guide](CONTRIBUTING.md)

## 🏗️ Architecture

YANETS follows Clean Architecture principles with clear separation of concerns:

```
src/
├── Yanets.Core/              # Domain models and business logic
├── Yanets.Application/       # Use cases and application services
├── Yanets.Infrastructure/    # External services, file I/O, persistence
├── Yanets.UI/               # User interface (WPF/Avalonia/MAUI/Blazor)
└── Yanets.Plugins/          # Plugin system and extensibility
```

### Technology Stack

- **.NET 9**: Core framework with latest performance improvements
- **C# 13**: Programming language with modern features
- **[UI Framework]**: Cross-platform UI (Avalonia/MAUI/Blazor/WPF)
- **Graph Theory Libraries**: Topology analysis and path computation
- **JSON/YAML**: Topology file formats
- **xUnit**: Testing framework

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
