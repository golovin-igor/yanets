# YANETS Architecture Document

## System Architecture Overview

YANETS employs a **Clean Architecture** approach with distinct layers and clear separation of concerns. The architecture is designed to support realistic device simulation including CLI (Telnet/SSH) and SNMP communication with vendor-specific behaviors.

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                      │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────┐    │
│  │   UI (WPF/  │  │   CLI        │  │   SNMP           │    │
│  │   Avalonia) │  │   Server     │  │   Agent          │    │
│  └─────────────┘  └──────────────┘  └──────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────────┐
│                     Application Layer                       │
│  ┌──────────────────┐  ┌──────────────────────────────┐     │
│  │  Topology        │  │  Device Simulation           │     │
│  │  Management      │  │  Services                    │     │
│  │  Services        │  │  - CLI Handler               │     │
│  └──────────────────┘  │  - SNMP Responder            │     │
│                        │  - Command Parser            │     │
│                        └──────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────────┐
│                       Domain Layer                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │   Topology   │  │   Device     │  │   Protocol       │   │
│  │   Models     │  │   Models     │  │   Models         │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │   Vendor     │  │   Command    │  │   MIB            │   │
│  │   Profiles   │  │   Definitions│  │   Definitions    │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │  File I/O    │  │  Network     │  │  Plugin          │   │
│  │  (JSON/YAML) │  │  Sockets     │  │  Loader          │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Detailed Layer Architecture

### 1. Domain Layer (Core)

The heart of YANETS containing all business logic and domain models.

#### 1.1 Core Models

```csharp
// Yanets.Core/Models/

namespace Yanets.Core.Models
{
    // Topology representation
    public class NetworkTopology
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<NetworkDevice> Devices { get; set; }
        public List<Connection> Connections { get; set; }
        public TopologyMetadata Metadata { get; set; }
    }

    // Base device abstraction
    public abstract class NetworkDevice
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Hostname { get; set; }
        public DeviceType Type { get; set; }
        public VendorProfile Vendor { get; set; }
        public Point Position { get; set; }
        public List<NetworkInterface> Interfaces { get; set; }
        public DeviceState State { get; set; }
        public IDeviceSimulator Simulator { get; set; }
    }

    // Device state management
    public class DeviceState
    {
        public string RunningConfig { get; set; }
        public string StartupConfig { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public RoutingTable RoutingTable { get; set; }
        public ArpTable ArpTable { get; set; }
        public MacAddressTable MacTable { get; set; }
        public List<VlanConfiguration> Vlans { get; set; }
        public DateTime Uptime { get; set; }
        public SystemResources Resources { get; set; }
    }

    // Network interface representation
    public class NetworkInterface
    {
        public string Name { get; set; }
        public InterfaceType Type { get; set; }
        public string IpAddress { get; set; }
        public string SubnetMask { get; set; }
        public string MacAddress { get; set; }
        public InterfaceStatus Status { get; set; }
        public int Speed { get; set; } // Mbps
        public bool IsUp { get; set; }
    }
}
```

#### 1.2 Vendor Profiles

```csharp
// Yanets.Core/Vendors/

namespace Yanets.Core.Vendors
{
    public abstract class VendorProfile
    {
        public string VendorName { get; set; }
        public string Os { get; set; }
        public string Version { get; set; }
        
        public abstract ICommandParser CommandParser { get; }
        public abstract IPromptGenerator PromptGenerator { get; }
        public abstract IMibProvider MibProvider { get; }
        
        // CLI-specific
        public virtual string WelcomeBanner { get; set; }
        public virtual string LoginPrompt { get; set; }
        public virtual string PasswordPrompt { get; set; }
        public virtual Dictionary<string, CommandDefinition> Commands { get; set; }
        
        // SNMP-specific
        public virtual string SysObjectId { get; set; }
        public virtual Dictionary<string, OidHandler> OidHandlers { get; set; }
    }

    // Cisco IOS implementation
    public class CiscoIosVendorProfile : VendorProfile
    {
        public override string VendorName => "Cisco";
        public override string Os => "IOS";
        
        public override string WelcomeBanner => 
            "User Access Verification\n";
        
        public override string LoginPrompt => "Username: ";
        public override string PasswordPrompt => "Password: ";
        
        public CiscoIosVendorProfile()
        {
            Commands = new Dictionary<string, CommandDefinition>
            {
                ["show version"] = new CommandDefinition
                {
                    Syntax = "show version",
                    PrivilegeLevel = 1,
                    Handler = ShowVersionHandler
                },
                ["show ip interface brief"] = new CommandDefinition
                {
                    Syntax = "show ip interface brief",
                    PrivilegeLevel = 1,
                    Handler = ShowIpInterfaceBriefHandler
                },
                ["show running-config"] = new CommandDefinition
                {
                    Syntax = "show running-config",
                    PrivilegeLevel = 15,
                    Handler = ShowRunningConfigHandler
                },
                // ... hundreds more commands
            };
            
            OidHandlers = new Dictionary<string, OidHandler>
            {
                ["1.3.6.1.2.1.1.1.0"] = SysDescrHandler, // sysDescr
                ["1.3.6.1.2.1.1.3.0"] = SysUptimeHandler, // sysUpTime
                ["1.3.6.1.2.1.2.2.1.2"] = IfDescrHandler, // ifDescr table
                // ... complete MIB-II and vendor MIBs
            };
        }
    }

    // Juniper JunOS implementation
    public class JuniperJunosVendorProfile : VendorProfile
    {
        public override string VendorName => "Juniper";
        public override string Os => "JunOS";
        
        public override string LoginPrompt => "login: ";
        
        public JuniperJunosVendorProfile()
        {
            Commands = new Dictionary<string, CommandDefinition>
            {
                ["show version"] = new CommandDefinition
                {
                    Syntax = "show version",
                    Handler = ShowVersionHandler
                },
                ["show interfaces terse"] = new CommandDefinition
                {
                    Syntax = "show interfaces terse",
                    Handler = ShowInterfacesTerseHandler
                },
                ["show configuration"] = new CommandDefinition
                {
                    Syntax = "show configuration",
                    Handler = ShowConfigurationHandler
                },
                // ... JunOS-specific commands
            };
        }
    }

    // Similar implementations for:
    // - AristaEosVendorProfile
    // - HpComwareVendorProfile
    // - DellOs10VendorProfile
    // - MikrotikRouterOsVendorProfile
    // etc.
}
```

#### 1.3 Command System

```csharp
// Yanets.Core/Commands/

namespace Yanets.Core.Commands
{
    public class CommandDefinition
    {
        public string Syntax { get; set; }
        public string Description { get; set; }
        public int PrivilegeLevel { get; set; }
        public List<CommandParameter> Parameters { get; set; }
        public Func<CommandContext, CommandResult> Handler { get; set; }
        public List<string> Aliases { get; set; }
        public bool RequiresConfirmation { get; set; }
    }

    public class CommandContext
    {
        public NetworkDevice Device { get; set; }
        public DeviceState State { get; set; }
        public string RawCommand { get; set; }
        public Dictionary<string, string> ParsedArguments { get; set; }
        public CliSession Session { get; set; }
        public int CurrentPrivilegeLevel { get; set; }
    }

    public class CommandResult
    {
        public bool Success { get; set; }
        public string Output { get; set; }
        public string ErrorMessage { get; set; }
        public DeviceState UpdatedState { get; set; }
    }

    public interface ICommandParser
    {
        CommandDefinition Parse(string commandLine, VendorProfile vendor);
        bool ValidateSyntax(string commandLine, CommandDefinition definition);
        Dictionary<string, string> ExtractArguments(string commandLine);
    }
}
```

#### 1.4 SNMP System

```csharp
// Yanets.Core/Snmp/

namespace Yanets.Core.Snmp
{
    public class MibDefinition
    {
        public string Oid { get; set; }
        public string Name { get; set; }
        public SnmpDataType DataType { get; set; }
        public AccessMode Access { get; set; }
        public string Description { get; set; }
    }

    public class OidHandler
    {
        public string Oid { get; set; }
        public Func<DeviceState, SnmpValue> GetHandler { get; set; }
        public Func<DeviceState, SnmpValue, bool> SetHandler { get; set; }
        public bool IsTable { get; set; }
        public Func<DeviceState, List<string>> GetTableIndices { get; set; }
    }

    public interface IMibProvider
    {
        Dictionary<string, MibDefinition> GetSupportedMibs();
        OidHandler GetOidHandler(string oid);
        bool SupportsOid(string oid);
    }

    public class SnmpValue
    {
        public SnmpDataType Type { get; set; }
        public object Value { get; set; }
        
        public static SnmpValue Integer(int value) => 
            new SnmpValue { Type = SnmpDataType.Integer, Value = value };
        
        public static SnmpValue OctetString(string value) => 
            new SnmpValue { Type = SnmpDataType.OctetString, Value = value };
        
        public static SnmpValue ObjectIdentifier(string oid) => 
            new SnmpValue { Type = SnmpDataType.ObjectIdentifier, Value = oid };
    }

    public enum SnmpDataType
    {
        Integer,
        OctetString,
        ObjectIdentifier,
        IpAddress,
        Counter32,
        Gauge32,
        TimeTicks,
        Counter64
    }
}
```

### 2. Application Layer

Orchestrates business logic and coordinates between layers.

#### 2.1 Device Simulation Services

```csharp
// Yanets.Application/Services/

namespace Yanets.Application.Services
{
    public interface IDeviceSimulator
    {
        Task<CommandResult> ExecuteCommand(CommandContext context);
        Task<SnmpResponse> HandleSnmpRequest(SnmpRequest request);
        DeviceState GetCurrentState();
        void UpdateState(DeviceState newState);
    }

    public class DeviceSimulatorService : IDeviceSimulator
    {
        private readonly ICommandParser _commandParser;
        private readonly IMibProvider _mibProvider;
        private readonly NetworkDevice _device;
        private DeviceState _currentState;

        public async Task<CommandResult> ExecuteCommand(CommandContext context)
        {
            // Parse command
            var commandDef = _commandParser.Parse(
                context.RawCommand, 
                _device.Vendor
            );

            if (commandDef == null)
            {
                return CommandResult.Error(
                    $"% Invalid input detected at '^' marker.\n" +
                    $"% Unknown command or incomplete command."
                );
            }

            // Check privilege level
            if (context.CurrentPrivilegeLevel < commandDef.PrivilegeLevel)
            {
                return CommandResult.Error("% Command authorization failed");
            }

            // Execute handler
            context.State = _currentState;
            var result = await Task.Run(() => commandDef.Handler(context));

            // Update state if changed
            if (result.UpdatedState != null)
            {
                _currentState = result.UpdatedState;
            }

            return result;
        }

        public async Task<SnmpResponse> HandleSnmpRequest(SnmpRequest request)
        {
            switch (request.Type)
            {
                case SnmpRequestType.Get:
                    return await HandleGetRequest(request);
                
                case SnmpRequestType.GetNext:
                    return await HandleGetNextRequest(request);
                
                case SnmpRequestType.GetBulk:
                    return await HandleGetBulkRequest(request);
                
                case SnmpRequestType.Set:
                    return await HandleSetRequest(request);
                
                default:
                    return SnmpResponse.Error(SnmpError.GenErr);
            }
        }

        private async Task<SnmpResponse> HandleGetRequest(SnmpRequest request)
        {
            var results = new List<SnmpVarBind>();

            foreach (var oid in request.Oids)
            {
                var handler = _mibProvider.GetOidHandler(oid);
                
                if (handler == null)
                {
                    results.Add(new SnmpVarBind
                    {
                        Oid = oid,
                        Error = SnmpError.NoSuchName
                    });
                    continue;
                }

                var value = handler.GetHandler(_currentState);
                results.Add(new SnmpVarBind
                {
                    Oid = oid,
                    Value = value
                });
            }

            return new SnmpResponse
            {
                RequestId = request.RequestId,
                VarBinds = results
            };
        }
    }
}
```

#### 2.2 CLI Server

```csharp
// Yanets.Application/Services/CliServer.cs

namespace Yanets.Application.Services
{
    public class CliServerService
    {
        private readonly Dictionary<Guid, TcpListener> _deviceListeners;
        private readonly ITopologyService _topologyService;

        public void StartCliServer(NetworkDevice device, int port = 23)
        {
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            _deviceListeners[device.Id] = listener;

            Task.Run(() => AcceptCliConnections(device, listener));
        }

        private async Task AcceptCliConnections(
            NetworkDevice device, 
            TcpListener listener)
        {
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleCliSession(device, client));
            }
        }

        private async Task HandleCliSession(
            NetworkDevice device, 
            TcpClient client)
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            var session = new CliSession
            {
                Device = device,
                PrivilegeLevel = 0,
                IsAuthenticated = false,
                CurrentMode = CliMode.UserExec
            };

            // Send welcome banner
            await writer.WriteAsync(device.Vendor.WelcomeBanner);

            // Authentication flow
            if (!await AuthenticateUser(session, reader, writer, device.Vendor))
            {
                await writer.WriteLineAsync("Authentication failed");
                return;
            }

            session.IsAuthenticated = true;
            session.PrivilegeLevel = 1;

            // Main command loop
            while (client.Connected)
            {
                // Display prompt
                var prompt = GeneratePrompt(device, session);
                await writer.WriteAsync(prompt);

                // Read command
                var commandLine = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(commandLine))
                    continue;

                // Handle special commands
                if (commandLine.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    if (session.CurrentMode == CliMode.UserExec)
                        break;
                    
                    session.CurrentMode = ExitMode(session.CurrentMode);
                    continue;
                }

                // Execute command
                var context = new CommandContext
                {
                    Device = device,
                    State = device.State,
                    RawCommand = commandLine,
                    Session = session,
                    CurrentPrivilegeLevel = session.PrivilegeLevel
                };

                var result = await device.Simulator.ExecuteCommand(context);
                
                if (!string.IsNullOrEmpty(result.Output))
                {
                    await writer.WriteAsync(result.Output);
                }

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    await writer.WriteLineAsync(result.ErrorMessage);
                }
            }
        }

        private string GeneratePrompt(NetworkDevice device, CliSession session)
        {
            return session.CurrentMode switch
            {
                CliMode.UserExec => $"{device.Hostname}>",
                CliMode.PrivilegedExec => $"{device.Hostname}#",
                CliMode.GlobalConfig => $"{device.Hostname}(config)#",
                CliMode.InterfaceConfig => 
                    $"{device.Hostname}(config-if)#",
                _ => $"{device.Hostname}>"
            };
        }
    }

    public class CliSession
    {
        public NetworkDevice Device { get; set; }
        public bool IsAuthenticated { get; set; }
        public int PrivilegeLevel { get; set; }
        public CliMode CurrentMode { get; set; }
        public Stack<CliMode> ModeStack { get; set; } = new();
        public Dictionary<string, object> SessionVariables { get; set; } = new();
    }

    public enum CliMode
    {
        UserExec,
        PrivilegedExec,
        GlobalConfig,
        InterfaceConfig,
        RouterConfig,
        LineConfig,
        VlanConfig
    }
}
```

#### 2.3 SNMP Agent

```csharp
// Yanets.Application/Services/SnmpAgent.cs

namespace Yanets.Application.Services
{
    public class SnmpAgentService
    {
        private readonly Dictionary<Guid, UdpClient> _deviceAgents;
        private readonly ITopologyService _topologyService;

        public void StartSnmpAgent(NetworkDevice device, int port = 161)
        {
            var agent = new UdpClient(port);
            _deviceAgents[device.Id] = agent;

            Task.Run(() => ListenForSnmpRequests(device, agent));
        }

        private async Task ListenForSnmpRequests(
            NetworkDevice device, 
            UdpClient agent)
        {
            while (true)
            {
                var receiveResult = await agent.ReceiveAsync();
                _ = Task.Run(() => ProcessSnmpRequest(
                    device, 
                    agent, 
                    receiveResult
                ));
            }
        }

        private async Task ProcessSnmpRequest(
            NetworkDevice device,
            UdpClient agent,
            UdpReceiveResult receiveResult)
        {
            try
            {
                // Parse SNMP request
                var request = SnmpMessageParser.Parse(receiveResult.Buffer);

                // Validate community string
                if (!ValidateCommunity(request.Community, device))
                {
                    // Silently drop (SNMP standard behavior)
                    return;
                }

                // Handle request
                var response = await device.Simulator.HandleSnmpRequest(request);

                // Encode and send response
                var responseBytes = SnmpMessageEncoder.Encode(response);
                await agent.SendAsync(
                    responseBytes, 
                    responseBytes.Length, 
                    receiveResult.RemoteEndPoint
                );
            }
            catch (Exception ex)
            {
                // Log error but don't crash the agent
                Console.WriteLine($"SNMP processing error: {ex.Message}");
            }
        }

        private bool ValidateCommunity(string community, NetworkDevice device)
        {
            // Check against device configuration
            var snmpConfig = device.State.Variables
                .GetValueOrDefault("snmp_communities") as Dictionary<string, string>;
            
            return snmpConfig?.ContainsKey(community) ?? 
                   community == "public"; // Default read-only
        }
    }

    public class SnmpRequest
    {
        public int RequestId { get; set; }
        public SnmpRequestType Type { get; set; }
        public string Community { get; set; }
        public SnmpVersion Version { get; set; }
        public List<string> Oids { get; set; }
        public List<SnmpVarBind> VarBinds { get; set; }
    }

    public class SnmpResponse
    {
        public int RequestId { get; set; }
        public SnmpError ErrorStatus { get; set; }
        public int ErrorIndex { get; set; }
        public List<SnmpVarBind> VarBinds { get; set; }
    }

    public enum SnmpRequestType
    {
        Get,
        GetNext,
        GetBulk,
        Set
    }

    public enum SnmpError
    {
        NoError = 0,
        TooBig = 1,
        NoSuchName = 2,
        BadValue = 3,
        ReadOnly = 4,
        GenErr = 5
    }
}
```

### 3. Infrastructure Layer

Handles external concerns and technical implementations.

#### 3.1 Network Socket Management

```csharp
// Yanets.Infrastructure/Network/

namespace Yanets.Infrastructure.Network
{
    public class NetworkSocketManager
    {
        private readonly Dictionary<Guid, DeviceNetworkContext> _deviceContexts;

        public class DeviceNetworkContext
        {
            public NetworkDevice Device { get; set; }
            public TcpListener TelnetListener { get; set; }
            public TcpListener SshListener { get; set; }
            public UdpClient SnmpAgent { get; set; }
            public Dictionary<int, TcpClient> ActiveSessions { get; set; }
        }

        public async Task StartAllServices(NetworkDevice device)
        {
            var context = new DeviceNetworkContext
            {
                Device = device,
                ActiveSessions = new Dictionary<int, TcpClient>()
            };

            // Start Telnet (port 23)
            context.TelnetListener = new TcpListener(
                GetDeviceIpAddress(device), 
                23
            );
            context.TelnetListener.Start();

            // Start SSH (port 22)
            context.SshListener = new TcpListener(
                GetDeviceIpAddress(device), 
                22
            );
            context.SshListener.Start();

            // Start SNMP (port 161)
            context.SnmpAgent = new UdpClient(new IPEndPoint(
                GetDeviceIpAddress(device), 
                161
            ));

            _deviceContexts[device.Id] = context;

            // Start listening tasks
            _ = Task.Run(() => AcceptTelnetConnections(context));
            _ = Task.Run(() => AcceptSshConnections(context));
            _ = Task.Run(() => HandleSnmpRequests(context));
        }

        private IPAddress GetDeviceIpAddress(NetworkDevice device)
        {
            // Get management interface IP
            var mgmtInterface = device.Interfaces
                .FirstOrDefault(i => !string.IsNullOrEmpty(i.IpAddress));
            
            return mgmtInterface != null 
                ? IPAddress.Parse(mgmtInterface.IpAddress)
                : IPAddress.Loopback;
        }
    }
}
```

#### 3.2 Command Handlers Implementation

```csharp
// Yanets.Infrastructure/CommandHandlers/Cisco/

namespace Yanets.Infrastructure.CommandHandlers.Cisco
{
    public static class CiscoCommandHandlers
    {
        public static CommandResult ShowVersionHandler(CommandContext ctx)
        {
            var device = ctx.Device;
            var state = ctx.State;
            
            var output = $@"Cisco IOS Software, {device.Type} Software (C2960-LANBASEK9-M), Version 15.0(2)SE, RELEASE SOFTWARE (fc1)
Technical Support: http://www.cisco.com/techsupport
Copyright (c) 1986-2012 by Cisco Systems, Inc.
Compiled Sat 28-Jul-12 00:29 by prod_rel_team

ROM: Bootstrap program is C2960 boot loader
BOOTLDR: C2960 Boot Loader (C2960-HBOOT-M) Version 12.2(53r)SEY3, RELEASE SOFTWARE (fc1)

{device.Hostname} uptime is {GetUptime(state)}
System returned to ROM by power-on
System image file is ""flash:c2960-lanbasek9-mz.150-2.SE.bin""

cisco WS-C2960-24TT-L (PowerPC405) processor (revision B0) with 65536K bytes of memory.
Processor board ID FOC1234W5678
Last reset from power-on
1 Virtual Ethernet interface
24 FastEthernet interfaces
2 Gigabit Ethernet interfaces
The password-recovery mechanism is enabled.

64K bytes of flash-simulated non-volatile configuration memory.
Base ethernet MAC Address       : {GetBaseMacAddress(device)}
Motherboard assembly number     : 73-10390-03
Power supply part number        : 341-0097-02
Motherboard serial number       : FOC12345678
Power supply serial number      : AZS12345678
Model revision number           : B0
Motherboard revision number     : B0
Model number                    : WS-C2960-24TT-L
System serial number            : FOC1234W5678
Top Assembly Part Number        : 800-27221-02
Top Assembly Revision Number    : A0
Version ID                      : V02
CLEI Code Number                : COM3L00BRA
Hardware Board Revision Number  : 0x01


Switch Ports Model              SW Version            SW Image                 
------ ----- -----              ----------            ----------               
*    1 26    WS-C2960-24TT-L    15.0(2)SE             C2960-LANBASEK9-M        


Configuration register is 0xF
";
            return CommandResult.Success(output);
        }

        public static CommandResult ShowIpInterfaceBriefHandler(CommandContext ctx)
        {
            var output = new StringBuilder();
            output.AppendLine("Interface              IP-Address      OK? Method Status                Protocol");
            
            foreach (var iface in ctx.Device.Interfaces)
            {
                var status = iface.IsUp ? "up" : "administratively down";
                var protocol = iface.IsUp ? "up" : "down";
                
                output.AppendLine(
                    $"{iface.Name,-22} " +
                    $"{iface.IpAddress ?? "unassigned",-15} " +
                    $"{"YES",-3} " +
                    $"{"manual",-6} " +
                    $"{status,-21} " +
                    $"{protocol}"
                );
            }
            
            return CommandResult.Success(output.ToString());
        }

        public static CommandResult ShowRunningConfigHandler(CommandContext ctx)
        {
            if (string.IsNullOrEmpty(ctx.State.RunningConfig))
            {
                return CommandResult.Success(GenerateDefaultConfig(ctx.Device));
            }
            
            return CommandResult.Success(
                "Building configuration...\n\n" +
                "Current configuration : 4001 bytes\n" +
                ctx.State.RunningConfig
            );
        }

        public static CommandResult ConfigureTerminalHandler(CommandContext ctx)
        {
            ctx.Session.CurrentMode = CliMode.GlobalConfig;
            ctx.Session.ModeStack.Push(CliMode.PrivilegedExec);
            
            return CommandResult.Success(
                $"Enter configuration commands, one per line.  " +
                $"End with CNTL/Z.\n"
            );
        }

        public static CommandResult InterfaceHandler(CommandContext ctx)
        {
            var interfaceName = ctx.ParsedArguments["interface"];
            var iface = ctx.Device.Interfaces
                .FirstOrDefault(i => i.Name.Equals(
                    interfaceName, 
                    StringComparison.OrdinalIgnoreCase
                ));
            
            if (iface == null)
            {
                return CommandResult.Error(
                    $"% Invalid input detected at '^' marker."
                );
            }
            
            ctx.Session.CurrentMode = CliMode.InterfaceConfig;
            ctx.Session.SessionVariables["current_interface"] = iface;
            
            return CommandResult.Success(string.Empty);
        }

        public static CommandResult IpAddressHandler(CommandContext ctx)
        {
            var ip = ctx.ParsedArguments["ip"];
            var mask = ctx.ParsedArguments["mask"];
            var iface = ctx.Session.SessionVariables["current_interface"] 
                as NetworkInterface;
            
            if (iface == null)
            {
                return CommandResult.Error("% Not in interface configuration mode");
            }
            
            iface.IpAddress = ip;
            iface.SubnetMask = mask;
            
            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = string.Empty,
                UpdatedState = newState
            };
        }

        public static CommandResult NoShutdownHandler(CommandContext ctx)
        {
            var iface = ctx.Session.SessionVariables["current_interface"] 
                as NetworkInterface;
            
            if (iface == null)
            {
                return CommandResult.Error("% Not in interface configuration mode");
            }
            
            iface.IsUp = true;
            iface.Status = InterfaceStatus.Up;
            
            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = $"\n%LINK-3-UPDOWN: Interface {iface.Name}, changed state to up\n" +
                         $"%LINEPROTO-5-UPDOWN: Line protocol on Interface {iface.Name}, changed state to up\n",
                UpdatedState = newState
            };
        }
    }
}
```

#### 3.3 SNMP MIB Handlers

```csharp
// Yanets.Infrastructure/SnmpHandlers/

namespace Yanets.Infrastructure.SnmpHandlers
{
    public class Mib2Handlers
    {
        // System group (1.3.6.1.2.1.1)
        public static SnmpValue SysDescrHandler(DeviceState state)
        {
            var device = state.Variables["device"] as NetworkDevice;
            return SnmpValue.OctetString(
                $"{device.Vendor.VendorName} {device.Type} " +
                $"running {device.Vendor.Os} {device.Vendor.Version}"
            );
        }

        public static SnmpValue SysObjectIdHandler(DeviceState state)
        {
            var device = state.Variables["device"] as NetworkDevice;
            return SnmpValue.ObjectIdentifier(device.Vendor.SysObjectId);
        }

        public static SnmpValue SysUptimeHandler(DeviceState state)
        {
            var uptime = DateTime.Now - state.Uptime;
            var timeticks = (int)(uptime.TotalMilliseconds / 10);
            return SnmpValue.TimeTicks(timeticks);
        }

        public static SnmpValue SysNameHandler(DeviceState state)
        {
            var device = state.Variables["device"] as NetworkDevice;
            return SnmpValue.OctetString(device.Hostname);
        }

        // Interfaces group (1.3.6.1.2.1.2)
        public static SnmpValue IfNumberHandler(DeviceState state)
        {
            var device = state.Variables["device"] as NetworkDevice;
            return SnmpValue.Integer(device.Interfaces.Count);
        }

        public static SnmpValue IfDescrHandler(DeviceState state, string index)
        {
            var device = state.Variables["device"] as NetworkDevice;
            var ifIndex = int.Parse(index);
            
            if (ifIndex < 1 || ifIndex > device.Interfaces.Count)
                return null;
            
            var iface = device.Interfaces[ifIndex - 1];
            return SnmpValue.OctetString(iface.Name);
        }

        public static SnmpValue IfTypeHandler(DeviceState state, string index)
        {
            var device = state.Variables["device"] as NetworkDevice;
            var ifIndex = int.Parse(index);
            
            if (ifIndex < 1 || ifIndex > device.Interfaces.Count)
                return null;
            
            var iface = device.Interfaces[ifIndex - 1];
            return SnmpValue.Integer((int)iface.Type);
        }

        public static SnmpValue IfSpeedHandler(DeviceState state, string index)
        {
            var device = state.Variables["device"] as NetworkDevice;
            var ifIndex = int.Parse(index);
            
            if (ifIndex < 1 || ifIndex > device.Interfaces.Count)
                return null;
            
            var iface = device.Interfaces[ifIndex - 1];
            return SnmpValue.Gauge32(iface.Speed * 1000000); // Convert Mbps to bps
        }

        public static SnmpValue IfOperStatusHandler(DeviceState state, string index)
        {
            var device = state.Variables["device"] as NetworkDevice;
            var ifIndex = int.Parse(index);
            
            if (ifIndex < 1 || ifIndex > device.Interfaces.Count)
                return null;
            
            var iface = device.Interfaces[ifIndex - 1];
            return SnmpValue.Integer(iface.IsUp ? 1 : 2); // 1=up, 2=down
        }

        // IP group (1.3.6.1.2.1.4)
        public static SnmpValue IpForwardingHandler(DeviceState state)
        {
            // 1 = forwarding, 2 = not-forwarding
            return SnmpValue.Integer(1);
        }

        public static SnmpValue IpAddrTableHandler(DeviceState state, string index)
        {
            var device = state.Variables["device"] as NetworkDevice;
            var iface = device.Interfaces
                .FirstOrDefault(i => i.IpAddress == index);
            
            if (iface == null)
                return null;
            
            return SnmpValue.IpAddress(iface.IpAddress);
        }
    }

    public class CiscoMibHandlers
    {
        // Cisco-specific OIDs
        public static SnmpValue CiscoCpuHandler(DeviceState state)
        {
            var resources = state.Resources;
            return SnmpValue.Gauge32(resources.CpuUtilization);
        }

        public static SnmpValue CiscoMemoryHandler(DeviceState state)
        {
            var resources = state.Resources;
            return SnmpValue.Gauge32(resources.MemoryUsed);
        }

        public static SnmpValue CiscoVlanHandler(DeviceState state, string index)
        {
            var vlanId = int.Parse(index);
            var vlan = state.Vlans.FirstOrDefault(v => v.Id == vlanId);
            
            if (vlan == null)
                return null;
            
            return SnmpValue.OctetString(vlan.Name);
        }
    }
}
```

### 4. Presentation Layer

User interface and external interfaces.

#### 4.1 Main UI Architecture

```csharp
// Yanets.UI/ViewModels/

namespace Yanets.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ITopologyService _topologyService;
        private readonly CliServerService _cliServer;
        private readonly SnmpAgentService _snmpAgent;

        public ObservableCollection<NetworkDevice> Devices { get; set; }
        public ObservableCollection<Connection> Connections { get; set; }

        public ICommand AddDeviceCommand { get; }
        public ICommand StartSimulationCommand { get; }
        public ICommand StopSimulationCommand { get; }
        public ICommand ConnectToDeviceCommand { get; }

        private async Task StartSimulation()
        {
            foreach (var device in Devices)
            {
                // Initialize device state
                device.State = new DeviceState
                {
                    Uptime = DateTime.Now,
                    Resources = new SystemResources
                    {
                        CpuUtilization = 5,
                        MemoryTotal = 65536,
                        MemoryUsed = 12000
                    }
                };

                // Create simulator
                device.Simulator = new DeviceSimulatorService(
                    new CommandParser(),
                    device.Vendor.MibProvider,
                    device
                );

                // Start network services
                _cliServer.StartCliServer(device, GetTelnetPort(device));
                _snmpAgent.StartSnmpAgent(device, GetSnmpPort(device));
            }

            IsSimulationRunning = true;
        }

        private async Task ConnectToDevice(NetworkDevice device)
        {
            // Open terminal window for CLI access
            var terminalWindow = new TerminalWindow(device);
            terminalWindow.Show();
        }
    }
}
```

## Key Design Patterns

### 1. **Strategy Pattern**
- Different vendor implementations via `VendorProfile` abstraction
- Pluggable command parsers and handlers

### 2. **Command Pattern**
- CLI commands as executable objects with context
- Undo/redo capability for configuration changes

### 3. **Observer Pattern**
- Device state changes notify UI
- Network events trigger appropriate handlers

### 4. **Factory Pattern**
- Device creation based on type and vendor
- Command handler instantiation

### 5. **Repository Pattern**
- Topology persistence and retrieval
- Device library management

## Data Flow

### CLI Command Execution Flow
```
User Input → CliServer → CommandParser → VendorProfile 
→ CommandDefinition → Handler → DeviceState → Response → User
```

### SNMP Request Flow
```
SNMP Client → SnmpAgent → SnmpParser → MibProvider 
→ OidHandler → DeviceState → SnmpResponse → SNMP Client
```

## Scalability Considerations

1. **Asynchronous I/O**: All network operations are async
2. **Connection Pooling**: Reuse TCP connections where possible
3. **Memory Management**: Device states are memory-efficient
4. **Concurrent Access**: Thread-safe device state management
5. **Plugin Architecture**: Extensible without core changes

## Security Considerations

1. **Authentication**: CLI and SNMP authentication
2. **Authorization**: Privilege levels for commands
3. **Input Validation**: All user input sanitized
4. **Community Strings**: SNMP access control
5. **Session Management**: Timeout and cleanup

## Testing Strategy

1. **Unit Tests**: Individual command handlers and parsers
2. **Integration Tests**: CLI and SNMP end-to-end scenarios
3. **Vendor Compatibility Tests**: Ensure accurate vendor behavior
4. **Performance Tests**: Handle multiple concurrent sessions
5. **Fuzz Testing**: Invalid command and SNMP request handling

## Performance Metrics

### Expected Performance Targets
- Support 100+ simulated devices simultaneously
- Handle 50+ concurrent CLI sessions
- Process 1000+ SNMP requests per second
- Response time < 100ms for CLI commands
- Response time < 50ms for SNMP queries

## Future Enhancements

1. **SSH Support**: Secure shell protocol implementation
2. **SNMPv3**: Advanced SNMP with encryption
3. **Syslog Server**: Centralized logging simulation
4. **NETCONF/RESTCONF**: Modern API support
5. **Traffic Simulation**: Basic packet flow visualization
6. **Performance Monitoring**: Real-time metrics dashboard
7. **Cluster Support**: Distributed simulation across multiple hosts

---

This architecture provides a solid, scalable foundation for YANETS with realistic device simulation capabilities!
