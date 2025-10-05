# YANETS Deployment Guide

## Overview

This guide provides instructions for deploying YANETS in various environments, from development to production.

## System Requirements

### Minimum Requirements
- **OS**: Windows 10+, Linux (Ubuntu 20.04+), macOS 11+
- **CPU**: 1 GHz dual-core processor
- **RAM**: 2 GB (4 GB recommended)
- **Disk**: 500 MB free space
- **Network**: TCP ports 5000, 23000-23999, 16100-16199

### Recommended Requirements
- **CPU**: 2 GHz quad-core processor
- **RAM**: 8 GB
- **Disk**: 1 GB free space (for logs and configurations)

## Installation Methods

### Option 1: Download Pre-built Release

1. Download the latest release from [GitHub Releases](https://github.com/yourusername/yanets/releases)
2. Extract to your desired directory
3. Run `YANETS.exe` (Windows) or `./YANETS` (Linux/macOS)

### Option 2: Build from Source

```bash
# Clone repository
git clone https://github.com/yourusername/yanets.git
cd yanets

# Build release version
dotnet publish 04_Presentation/WebUI/Yanets.WebUI.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained \
  --output publish

# Deploy the published files
```

### Option 3: Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish 04_Presentation/WebUI/Yanets.WebUI.csproj \
  --configuration Release \
  --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000 23000-23999 16100-16199
ENTRYPOINT ["dotnet", "Yanets.WebUI.dll"]
```

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | Development |
| `ASPNETCORE_URLS` | API server URLs | http://localhost:5000 |
| `LOG_LEVEL` | Logging level | Information |
| `YANETS_DATA_PATH` | Data storage path | ./data |

### Application Settings

Create an `appsettings.json` file:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Yanets": {
    "DataPath": "./data",
    "MaxConcurrentConnections": 100,
    "CommandTimeout": 30000
  }
}
```

## Network Configuration

### Port Requirements

YANETS uses the following ports:
- **5000**: Web API (HTTP)
- **23000-23999**: Telnet connections (distributed per device)
- **16100-16199**: SNMP agents (distributed per device)

### Firewall Configuration

```bash
# Allow Web API
sudo ufw allow 5000

# Allow Telnet range
sudo ufw allow 23000:23999/tcp

# Allow SNMP range
sudo ufw allow 16100:16199/udp
```

## Deployment Scenarios

### Development Environment

```bash
# Start with debug logging
LOG_LEVEL=Debug dotnet run --project 04_Presentation/WebUI

# Or use Visual Studio/VS Code
dotnet run --project 04_Presentation/WebUI --launch-profile https
```

### Production Environment

```bash
# Use reverse proxy (nginx example)
server {
    listen 80;
    server_name yanets.yourdomain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}

# Start with production configuration
ASPNETCORE_ENVIRONMENT=Production dotnet run --project 04_Presentation/WebUI
```

### Docker Compose

```yaml
version: '3.8'
services:
  yanets:
    build: .
    ports:
      - "5000:5000"
      - "23000-23999:23000-23999"
      - "16100-16199:16100-16199"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5000
    volumes:
      - yanets_data:/app/data
    restart: unless-stopped

volumes:
  yanets_data:
```

## Monitoring and Health Checks

### Health Endpoints

```bash
# Basic health check
curl http://localhost:5000/health

# Detailed system status (if implemented)
curl http://localhost:5000/api/diagnostics
```

### Logging

YANETS uses structured logging with the following levels:
- **Trace**: Detailed diagnostic information
- **Debug**: Development debugging information
- **Information**: General information (default)
- **Warning**: Warning conditions
- **Error**: Error conditions
- **Critical**: Critical errors

### Metrics

Monitor these key metrics:
- API response times
- Active CLI connections
- SNMP request rates
- Memory usage
- CPU utilization

## Security Considerations

### Network Security
- Use HTTPS in production
- Configure firewall rules appropriately
- Monitor for unauthorized access attempts
- Consider VPN for remote access

### Authentication (Future)
- Currently accepts any credentials for demo purposes
- Implement proper authentication for production
- Consider OAuth2/OpenID Connect
- Use secure password storage

### Data Protection
- Encrypt sensitive configuration data
- Use secure communication protocols
- Implement audit logging
- Regular security updates

## Performance Optimization

### Memory Management
- Monitor garbage collection
- Configure appropriate memory limits
- Use connection pooling where applicable

### Concurrent Operations
- Configure maximum concurrent connections
- Monitor thread pool usage
- Implement proper async/await patterns

### Database Optimization
- Use efficient data structures for in-memory storage
- Implement proper indexing for large topologies
- Consider database persistence for large-scale deployments

## Backup and Recovery

### Data Backup
- Export topology configurations regularly
- Backup application logs
- Save device configurations

### Recovery Procedures
1. Restore topology configurations
2. Restart network services
3. Verify device connectivity
4. Validate system functionality

## Troubleshooting

### Common Issues

**High Memory Usage**
- Check for memory leaks in long-running simulations
- Monitor garbage collection
- Consider reducing concurrent connections

**Slow Response Times**
- Check system resources (CPU, memory, disk I/O)
- Monitor network connectivity
- Review logging for performance bottlenecks

**Connection Failures**
- Verify port availability
- Check firewall configuration
- Review network interface configuration

### Debug Logging

Enable debug logging:

```bash
export LOG_LEVEL=Debug
./YANETS
```

### Performance Profiling

Use dotnet tools for profiling:

```bash
dotnet trace collect --process-id <pid>
dotnet trace collect --providers Microsoft-DotNETCore-SampleProfiler
```

## Scaling Considerations

### Single Server Deployment
- Suitable for small to medium deployments
- Supports 50-100 concurrent devices
- 1000+ SNMP requests per second

### Multi-Server Deployment (Future)
- Distribute devices across multiple servers
- Load balancing for API requests
- Shared configuration management

### Cloud Deployment
- Use container orchestration (Kubernetes)
- Auto-scaling based on load
- Cloud-native monitoring and logging

## Support and Maintenance

### Regular Updates
- Monitor for security updates
- Update .NET runtime as needed
- Review and update dependencies

### Community Support
- GitHub Issues for bug reports
- GitHub Discussions for questions
- Documentation contributions welcome

### Professional Support
- Contact the development team for enterprise support
- Custom feature development
- Training and consulting services

## License and Legal

- YANETS is released under the MIT License
- See LICENSE file for full license text
- No warranties provided for production use

---

**For issues and support, visit [GitHub Issues](https://github.com/yourusername/yanets/issues)**
