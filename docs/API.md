# YANETS API Reference

## Overview

The YANETS API provides RESTful endpoints for managing network topologies and devices. All endpoints return JSON responses and use standard HTTP status codes.

**Base URL**: `http://localhost:5000`

## Authentication

Currently, the API does not require authentication for demonstration purposes. In a production environment, consider implementing proper authentication and authorization.

## Common Response Formats

### Success Response (200 OK)
```json
{
  "id": "guid-here",
  "name": "Topology Name",
  "description": "Topology Description",
  "devices": [...],
  "connections": [...],
  "metadata": {
    "version": "1.0",
    "createdAt": "2025-01-01T00:00:00Z",
    "updatedAt": "2025-01-01T00:00:00Z"
  }
}
```

### Error Response (400 Bad Request)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid request format"
}
```

### Not Found Response (404 Not Found)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Resource not found"
}
```

## Topology Management

### List All Topologies

**GET** `/api/topology`

Returns a list of all network topologies.

**Response**: Array of `NetworkTopology` objects

**Status Codes**:
- 200: Success

### Get Topology

**GET** `/api/topology/{id}`

Returns a specific topology by ID.

**Parameters**:
- `id` (GUID): The topology identifier

**Response**: `NetworkTopology` object

**Status Codes**:
- 200: Success
- 404: Topology not found

### Create Topology

**POST** `/api/topology`

Creates a new network topology.

**Request Body**:
```json
{
  "name": "Production Network",
  "description": "Main production network"
}
```

**Response**: Created `NetworkTopology` object with 201 status

**Status Codes**:
- 201: Created
- 400: Invalid request data

### Update Topology

**PUT** `/api/topology/{id}`

Updates an existing topology.

**Parameters**:
- `id` (GUID): The topology identifier

**Request Body**:
```json
{
  "name": "Updated Network Name",
  "description": "Updated description"
}
```

**Status Codes**:
- 204: Success (No Content)
- 404: Topology not found
- 400: Invalid request data

### Delete Topology

**DELETE** `/api/topology/{id}`

Deletes a topology and all its associated devices.

**Parameters**:
- `id` (GUID): The topology identifier

**Status Codes**:
- 204: Success (No Content)
- 404: Topology not found

## Device Management

### Add Device to Topology

**POST** `/api/topology/{topologyId}/devices`

Adds a new device to an existing topology.

**Parameters**:
- `topologyId` (GUID): The topology identifier

**Request Body**:
```json
{
  "name": "Branch Router",
  "hostname": "branch-r1",
  "vendorName": "Cisco",
  "positionX": 200,
  "positionY": 150
}
```

**Supported Vendors**:
- `Cisco` - Cisco IOS devices
- `Juniper` - Juniper JunOS devices

**Response**: Created `NetworkDevice` object with 201 status

**Status Codes**:
- 201: Created
- 404: Topology not found
- 400: Invalid request data

### Get Devices in Topology

**GET** `/api/topology/{topologyId}/devices`

Returns all devices in a specific topology.

**Parameters**:
- `topologyId` (GUID): The topology identifier

**Response**: Array of `NetworkDevice` objects

**Status Codes**:
- 200: Success
- 404: Topology not found

## Health and Monitoring

### Health Check

**GET** `/health`

Returns the current health status of the YANETS system.

**Response**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-01-01T00:00:00Z"
}
```

**Status Codes**:
- 200: System is healthy

## Data Models

### NetworkTopology

```json
{
  "id": "guid",
  "name": "string",
  "description": "string",
  "devices": [
    {
      "id": "guid",
      "name": "string",
      "hostname": "string",
      "type": "Router|Switch|Firewall|AccessPoint|Server|Workstation",
      "vendor": {
        "vendorName": "string",
        "os": "string",
        "version": "string"
      },
      "position": {
        "x": 0,
        "y": 0
      },
      "interfaces": [
        {
          "name": "string",
          "type": "Ethernet|FastEthernet|GigabitEthernet|Serial|Loopback|Tunnel|Vlan",
          "ipAddress": "string",
          "subnetMask": "string",
          "macAddress": "string",
          "status": "Up|Down|AdministrativelyDown|Testing|Unknown|Dormant|NotPresent|LowerLayerDown",
          "speed": 0,
          "isUp": true,
          "description": "string",
          "mtu": 1500,
          "isShutdown": false
        }
      ],
      "state": {
        "runningConfig": "string",
        "startupConfig": "string",
        "variables": {},
        "routingTable": [],
        "arpTable": [],
        "macTable": [],
        "vlans": [],
        "uptime": "2025-01-01T00:00:00Z",
        "resources": {
          "cpuUtilization": 0.0,
          "memoryTotal": 0,
          "memoryUsed": 0
        },
        "isSimulationRunning": true
      }
    }
  ],
  "connections": [
    {
      "id": "guid",
      "sourceDeviceId": "guid",
      "sourceInterface": "string",
      "targetDeviceId": "guid",
      "targetInterface": "string",
      "type": "Ethernet|Serial|Fiber|Wireless|Virtual",
      "startPoint": {
        "x": 0,
        "y": 0
      },
      "endPoint": {
        "x": 0,
        "y": 0
      }
    }
  ],
  "metadata": {
    "version": "string",
    "createdAt": "2025-01-01T00:00:00Z",
    "updatedAt": "2025-01-01T00:00:00Z",
    "author": "string",
    "description": "string"
  }
}
```

### NetworkDevice

```json
{
  "id": "guid",
  "name": "string",
  "hostname": "string",
  "type": "Router|Switch|Firewall|AccessPoint|Server|Workstation",
  "vendor": {
    "vendorName": "string",
    "os": "string",
    "version": "string"
  },
  "position": {
    "x": 0,
    "y": 0
  },
  "interfaces": [...],
  "state": {...}
}
```

### NetworkInterface

```json
{
  "name": "string",
  "type": "Ethernet|FastEthernet|GigabitEthernet|Serial|Loopback|Tunnel|Vlan",
  "ipAddress": "string",
  "subnetMask": "string",
  "macAddress": "string",
  "status": "Up|Down|AdministrativelyDown|Testing|Unknown|Dormant|NotPresent|LowerLayerDown",
  "speed": 0,
  "isUp": true,
  "description": "string",
  "mtu": 1500,
  "isShutdown": false
}
```

## Error Codes

| Status Code | Description |
|-------------|-------------|
| 200 | Success |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request |
| 404 | Not Found |
| 500 | Internal Server Error |

## Rate Limiting

The API does not currently implement rate limiting, but this should be considered for production deployments.

## Versioning

API versioning is not currently implemented but can be added using URL versioning (`/api/v1/topology`) or header-based versioning.

## Examples

### Complete Workflow Example

```bash
# 1. Check system health
curl http://localhost:5000/health

# 2. Create a new topology
curl -X POST http://localhost:5000/api/topology \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Lab Network",
    "description": "Network simulation laboratory"
  }'

# 3. Add a router
curl -X POST http://localhost:5000/api/topology/{topologyId}/devices \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Core Router",
    "hostname": "core-r1",
    "vendorName": "Cisco",
    "positionX": 100,
    "positionY": 100
  }'

# 4. Connect via CLI
telnet localhost 23001

# 5. Query via SNMP
snmpget -v 2c -c public localhost:16101 1.3.6.1.2.1.1.1.0
```

### Using the API with JavaScript

```javascript
const baseUrl = 'http://localhost:5000/api';

// Create a topology
async function createTopology() {
  const response = await fetch(`${baseUrl}/topology`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      name: 'JavaScript Network',
      description: 'Created via JavaScript API'
    })
  });

  const topology = await response.json();
  return topology;
}

// Get all topologies
async function getTopologies() {
  const response = await fetch(`${baseUrl}/topology`);
  const topologies = await response.json();
  return topologies;
}
```

## CLI Integration

### Telnet Connection

```bash
# Connect to device
telnet localhost 23001

# Login (any credentials accepted)
Username: admin
Password: password

# Execute commands
show version
show ip interface brief
configure terminal
interface GigabitEthernet0/0
ip address 192.168.1.1 255.255.255.0
no shutdown
```

### SSH Connection

SSH support is planned for future releases. Currently, only Telnet is supported.

## SNMP Integration

### SNMP v1/v2c Support

```bash
# Get system description
snmpget -v 2c -c public localhost:16101 1.3.6.1.2.1.1.1.0

# Get interface information
snmpwalk -v 2c -c public localhost:16101 1.3.6.1.2.1.2.2.1

# Get Cisco-specific information
snmpget -v 2c -c public localhost:16101 1.3.6.1.4.1.9.9.109.1.1.1.1.2.1
```

### Supported OIDs

#### Standard MIB-II (RFC 1213)
- `1.3.6.1.2.1.1.1.0` - sysDescr
- `1.3.6.1.2.1.1.3.0` - sysUpTime
- `1.3.6.1.2.1.1.5.0` - sysName
- `1.3.6.1.2.1.2.1.0` - ifNumber
- `1.3.6.1.2.1.2.2.1.2.*` - ifDescr table
- `1.3.6.1.2.1.2.2.1.3.*` - ifType table
- `1.3.6.1.2.1.2.2.1.5.*` - ifSpeed table
- `1.3.6.1.2.1.2.2.1.8.*` - ifOperStatus table
- `1.3.6.1.2.1.4.1.0` - ipForwarding
- `1.3.6.1.2.1.4.20.1.1.*` - ipAddrTable
- `1.3.6.1.2.1.6.1.0` - tcpConnState
- `1.3.6.1.2.1.6.6.0` - tcpInSegs
- `1.3.6.1.2.1.6.7.0` - tcpOutSegs
- `1.3.6.1.2.1.7.1.0` - udpInDatagrams
- `1.3.6.1.2.1.7.2.0` - udpOutDatagrams
- `1.3.6.1.2.1.11.1.0` - snmpInPkts
- `1.3.6.1.2.1.11.2.0` - snmpOutPkts

#### Cisco-Specific OIDs
- `1.3.6.1.4.1.9.2.1.1.0` - ciscoSystemVersion
- `1.3.6.1.4.1.9.9.109.1.1.1.1.2.*` - ciscoCpuUtilization
- `1.3.6.1.4.1.9.9.109.1.1.1.1.3.*` - ciscoMemoryUsed
- `1.3.6.1.4.1.9.9.23.1.2.1.1.*` - ciscoIfInOctets
- `1.3.6.1.4.1.9.9.23.1.2.1.2.*` - ciscoIfOutOctets

## Testing the API

### Using Swagger UI

1. Start the YANETS Web API
2. Navigate to `http://localhost:5000/swagger`
3. Use the interactive interface to test endpoints

### Using curl

```bash
# Health check
curl http://localhost:5000/health

# Create topology
curl -X POST http://localhost:5000/api/topology \
  -H "Content-Type: application/json" \
  -d '{"name": "Test Network", "description": "API testing"}'

# Get all topologies
curl http://localhost:5000/api/topology
```

### Using Postman

1. Import the OpenAPI specification from Swagger UI
2. Create a new collection for YANETS API testing
3. Test all endpoints with various scenarios

## Troubleshooting

### Common Issues

**Problem**: API returns 404 for existing resources
**Solution**: Verify the GUID format and ensure the resource exists

**Problem**: JSON serialization errors
**Solution**: Ensure request body is valid JSON and required fields are provided

**Problem**: Device connectivity issues
**Solution**: Check that devices are properly initialized and network services are running

**Problem**: Performance issues
**Solution**: Monitor system resources and consider reducing concurrent operations

### Debug Logging

Enable debug logging by setting environment variables:

```bash
export LOG_LEVEL=Debug
dotnet run
```

### API Validation

The API includes comprehensive validation:
- Required fields must be provided
- GUIDs must be valid format
- Vendor names must be supported
- Position coordinates must be valid

---

**For more examples and detailed usage, see the [User Guide](UserGuide.md)**
