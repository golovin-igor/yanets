using System.Numerics;
using Yanets.Core.Commands;
using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.SharedKernel;
using ArpType = Yanets.SharedKernel.ArpType;

namespace Yanets.Application.Services.Vendors
{
    /// <summary>
    /// Cisco IOS vendor profile implementation for the application layer
    /// </summary>
    public class CiscoIosVendorProfile : VendorProfile
    {
        public override string VendorName => "Cisco";
        public override string Os => "IOS";
        public override string Version => "15.2(4)E8";

        public override ICommandParser CommandParser => new CommandParser();
        public override IPromptGenerator PromptGenerator => new PromptGenerator();
        public override IMibProvider MibProvider => new MibProvider();

        public override string WelcomeBanner =>
            "User Access Verification\n\n";

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
                ["configure terminal"] = new CommandDefinition
                {
                    Syntax = "configure terminal",
                    PrivilegeLevel = 15,
                    Handler = ConfigureTerminalHandler
                },
                ["interface"] = new CommandDefinition
                {
                    Syntax = "interface <interface>",
                    PrivilegeLevel = 15,
                    Handler = InterfaceHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "interface", IsRequired = true }
                    }
                },
                ["ip address"] = new CommandDefinition
                {
                    Syntax = "ip address <ip> <mask>",
                    PrivilegeLevel = 15,
                    Handler = IpAddressHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "ip", IsRequired = true },
                        new CommandParameter { Name = "mask", IsRequired = true }
                    }
                },
                ["no shutdown"] = new CommandDefinition
                {
                    Syntax = "no shutdown",
                    PrivilegeLevel = 15,
                    Handler = NoShutdownHandler
                },
                ["shutdown"] = new CommandDefinition
                {
                    Syntax = "shutdown",
                    PrivilegeLevel = 15,
                    Handler = ShutdownHandler
                },
                ["router bgp"] = new CommandDefinition
                {
                    Syntax = "router bgp <as-number>",
                    PrivilegeLevel = 15,
                    Handler = RouterBgpHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "as-number", IsRequired = true }
                    }
                },
                ["show ip bgp"] = new CommandDefinition
                {
                    Syntax = "show ip bgp",
                    PrivilegeLevel = 1,
                    Handler = ShowIpBgpHandler
                },
                ["show bgp summary"] = new CommandDefinition
                {
                    Syntax = "show bgp summary",
                    PrivilegeLevel = 1,
                    Handler = ShowBgpSummaryHandler
                },
                ["neighbor"] = new CommandDefinition
                {
                    Syntax = "neighbor <ip-address> remote-as <as-number>",
                    PrivilegeLevel = 15,
                    Handler = NeighborHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "ip-address", IsRequired = true },
                        new CommandParameter { Name = "as-number", IsRequired = true }
                    }
                },
                ["network"] = new CommandDefinition
                {
                    Syntax = "network <network> mask <mask>",
                    PrivilegeLevel = 15,
                    Handler = NetworkHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "network", IsRequired = true },
                        new CommandParameter { Name = "mask", IsRequired = true }
                    }
                },
                ["redistribute"] = new CommandDefinition
                {
                    Syntax = "redistribute <protocol>",
                    PrivilegeLevel = 15,
                    Handler = RedistributeHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "protocol", IsRequired = true }
                    }
                },
                ["router ospf"] = new CommandDefinition
                {
                    Syntax = "router ospf <process-id>",
                    PrivilegeLevel = 15,
                    Handler = RouterOspfHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "process-id", IsRequired = true }
                    }
                },
                ["show ip ospf"] = new CommandDefinition
                {
                    Syntax = "show ip ospf",
                    PrivilegeLevel = 1,
                    Handler = ShowIpOspfHandler
                },
                ["area"] = new CommandDefinition
                {
                    Syntax = "area <area-id> range <network> <mask>",
                    PrivilegeLevel = 15,
                    Handler = AreaRangeHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "area-id", IsRequired = true },
                        new CommandParameter { Name = "network", IsRequired = true },
                        new CommandParameter { Name = "mask", IsRequired = true }
                    }
                },
                ["ip route"] = new CommandDefinition
                {
                    Syntax = "ip route <network> <mask> <next-hop>",
                    PrivilegeLevel = 15,
                    Handler = IpRouteHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "network", IsRequired = true },
                        new CommandParameter { Name = "mask", IsRequired = true },
                        new CommandParameter { Name = "next-hop", IsRequired = true }
                    }
                },
                ["router eigrp"] = new CommandDefinition
                {
                    Syntax = "router eigrp <as-number>",
                    PrivilegeLevel = 15,
                    Handler = RouterEigrpHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "as-number", IsRequired = true }
                    }
                },
                ["show ip eigrp"] = new CommandDefinition
                {
                    Syntax = "show ip eigrp",
                    PrivilegeLevel = 1,
                    Handler = ShowIpEigrpHandler
                },
                ["router rip"] = new CommandDefinition
                {
                    Syntax = "router rip",
                    PrivilegeLevel = 15,
                    Handler = RouterRipHandler
                },
                ["version"] = new CommandDefinition
                {
                    Syntax = "version <version>",
                    PrivilegeLevel = 15,
                    Handler = VersionHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "version", IsRequired = true }
                    }
                },
                ["no router bgp"] = new CommandDefinition
                {
                    Syntax = "no router bgp <as-number>",
                    PrivilegeLevel = 15,
                    Handler = NoRouterBgpHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "as-number", IsRequired = true }
                    }
                },
                ["show ip protocols"] = new CommandDefinition
                {
                    Syntax = "show ip protocols",
                    PrivilegeLevel = 1,
                    Handler = ShowIpProtocolsHandler
                },
                ["show ip route"] = new CommandDefinition
                {
                    Syntax = "show ip route",
                    PrivilegeLevel = 1,
                    Handler = ShowIpRouteHandler
                },
                ["show arp"] = new CommandDefinition
                {
                    Syntax = "show arp",
                    PrivilegeLevel = 1,
                    Handler = ShowArpHandler
                },
                ["ping"] = new CommandDefinition
                {
                    Syntax = "ping <ip-address>",
                    PrivilegeLevel = 1,
                    Handler = PingHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "ip-address", IsRequired = true }
                    }
                }
            };

            SysObjectId = "1.3.6.1.4.1.9.1.1208"; // Cisco router
        }

        private static CommandResult ShowVersionHandler(CommandContext ctx)
        {
            var device = ctx.Device;
            var state = ctx.State;

            var output = $@"Cisco IOS Software, {device.Type} Software (C2960-LANBASEK9-M), Version 15.2(4)E8, RELEASE SOFTWARE (fc2)
Technical Support: http://www.cisco.com/techsupport
Copyright (c) 1986-2019 by Cisco Systems, Inc.
Compiled Tue 25-Jun-19 12:53 by prod_rel_team

{device.Hostname} uptime is {GetUptime(state)}
System returned to ROM by power-on
System image file is ""flash:c2960-lanbasek9-mz.150-2.E8.bin""

cisco WS-C2960-24TT-L (PowerPC405) processor (revision B0) with 65536K bytes of memory.
Processor board ID FOC1234W5678
Last reset from power-on
1 Virtual Ethernet interface
24 FastEthernet interfaces
2 Gigabit Ethernet interfaces

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

Configuration register is 0xF
";

            return CommandResult.CreateSuccess(output);
        }

        private static CommandResult ShowIpInterfaceBriefHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
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

            return CommandResult.CreateSuccess(output.ToString());
        }

        private static CommandResult ShowRunningConfigHandler(CommandContext ctx)
        {
            var config = ctx.State.RunningConfig;
            if (string.IsNullOrEmpty(config))
            {
                config = GenerateDefaultConfig(ctx.Device);
            }

            return CommandResult.CreateSuccess(
                "Building configuration...\n\n" +
                $"Current configuration : {config.Length} bytes\n" +
                config
            );
        }

        private static CommandResult ConfigureTerminalHandler(CommandContext ctx)
        {
            ctx.Session.CurrentMode = CliMode.GlobalConfig;
            ctx.Session.ModeStack.Push(CliMode.PrivilegedExec);

            return CommandResult.CreateSuccess(
                $"Enter configuration commands, one per line.  " +
                $"End with CNTL/Z.\n{ctx.Device.Hostname}(config)#"
            );
        }

        private static CommandResult InterfaceHandler(CommandContext ctx)
        {
            var interfaceName = ctx.ParsedArguments.GetValueOrDefault("interface");
            if (string.IsNullOrEmpty(interfaceName))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            var iface = ctx.Device.Interfaces.FirstOrDefault(i =>
                i.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase));

            if (iface == null)
            {
                return CommandResult.CreateError($"% Invalid interface {interfaceName}");
            }

            ctx.Session.CurrentMode = CliMode.InterfaceConfig;
            ctx.Session.SessionVariables["current_interface"] = iface;

            return CommandResult.CreateSuccess(string.Empty);
        }

        private static CommandResult IpAddressHandler(CommandContext ctx)
        {
            var ip = ctx.ParsedArguments.GetValueOrDefault("ip");
            var mask = ctx.ParsedArguments.GetValueOrDefault("mask");

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(mask))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            var iface = ctx.Session.SessionVariables["current_interface"] as NetworkInterface;
            if (iface == null)
            {
                return CommandResult.CreateError("% Not in interface configuration mode");
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

        private static CommandResult NoShutdownHandler(CommandContext ctx)
        {
            var iface = ctx.Session.SessionVariables["current_interface"] as NetworkInterface;
            if (iface == null)
            {
                return CommandResult.CreateError("% Not in interface configuration mode");
            }

            iface.IsUp = true;
            iface.Status = InterfaceStatus.Up;
            iface.IsShutdown = false;

            var newState = ctx.State.Clone();
            newState.UpdateInterfaceConnectivity(iface.Name, true);
            return new CommandResult
            {
                Success = true,
                Output = $"\n%LINK-3-UPDOWN: Interface {iface.Name}, changed state to up\n" +
                         $"%LINEPROTO-5-UPDOWN: Line protocol on Interface {iface.Name}, changed state to up\n",
                UpdatedState = newState
            };
        }

        private static CommandResult ShutdownHandler(CommandContext ctx)
        {
            var iface = ctx.Session.SessionVariables["current_interface"] as NetworkInterface;
            if (iface == null)
            {
                return CommandResult.CreateError("% Not in interface configuration mode");
            }

            iface.IsShutdown = true;
            iface.IsUp = false;
            iface.Status = InterfaceStatus.AdministrativelyDown;

            var newState = ctx.State.Clone();
            newState.UpdateInterfaceConnectivity(iface.Name, false);
            return new CommandResult
            {
                Success = true,
                Output = $"\n%LINK-3-UPDOWN: Interface {iface.Name}, changed state to administratively down\n" +
                         $"%LINEPROTO-5-UPDOWN: Line protocol on Interface {iface.Name}, changed state to down\n",
                UpdatedState = newState
            };
        }

        private static string GetUptime(DeviceState state)
        {
            var uptime = DateTime.Now - state.Uptime;
            return $"{(int)uptime.TotalDays} weeks, {(int)uptime.TotalDays % 7} days, {uptime.Hours} hours, {uptime.Minutes} minutes";
        }

        private static string GetBaseMacAddress(NetworkDevice device)
        {
            return device.Interfaces.FirstOrDefault()?.MacAddress ?? "00:00:00:00:00:00";
        }

        private static CommandResult RouterBgpHandler(CommandContext ctx)
        {
            var asNumber = ctx.ParsedArguments.GetValueOrDefault("as-number");
            if (string.IsNullOrEmpty(asNumber))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            // Enter BGP configuration mode
            ctx.Session.CurrentMode = CliMode.RouterConfig;
            ctx.Session.SessionVariables["bgp_as"] = asNumber;
            ctx.Session.SessionVariables["routing_protocol"] = "bgp";

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = $"\n{ctx.Device.Hostname}(config-router)#",
                UpdatedState = newState
            };
        }

        private static CommandResult ShowIpBgpHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("BGP table version is 1, local router ID is 192.168.1.1");
            output.AppendLine("Status codes: s suppressed, d damped, h history, * valid, > best, i - internal,");
            output.AppendLine("              r RIB-failure, S Stale, R Removed");
            output.AppendLine("Origin codes: i - IGP, e - EGP, ? - incomplete");
            output.AppendLine("");
            output.AppendLine("   Network          Next Hop            Metric LocPrf Weight Path");
            output.AppendLine("*  10.0.0.0/24      192.168.2.2              0             0 65002 i");
            output.AppendLine("*> 192.168.1.0/24   0.0.0.0                  0         32768 i");

            return CommandResult.CreateSuccess(output.ToString());
        }

        private static CommandResult ShowBgpSummaryHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("BGP router identifier 192.168.1.1, local AS number 65001");
            output.AppendLine("BGP table version is 1, main routing table version 1");
            output.AppendLine("");
            output.AppendLine("Neighbor        V           AS MsgRcvd MsgSent   TblVer  InQ OutQ Up/Down  State/PfxRcd");
            output.AppendLine("192.168.2.2    4        65002       5       5        1    0    0 00:02:31        1");

            return CommandResult.CreateSuccess(output.ToString());
        }

        private static CommandResult NeighborHandler(CommandContext ctx)
        {
            var ipAddress = ctx.ParsedArguments.GetValueOrDefault("ip-address");
            var asNumber = ctx.ParsedArguments.GetValueOrDefault("as-number");

            if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrEmpty(asNumber))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = string.Empty,
                UpdatedState = newState
            };
        }

        private static CommandResult NetworkHandler(CommandContext ctx)
        {
            var network = ctx.ParsedArguments.GetValueOrDefault("network");
            var mask = ctx.ParsedArguments.GetValueOrDefault("mask");

            if (string.IsNullOrEmpty(network) || string.IsNullOrEmpty(mask))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = string.Empty,
                UpdatedState = newState
            };
        }

        private static CommandResult RedistributeHandler(CommandContext ctx)
        {
            var protocol = ctx.ParsedArguments.GetValueOrDefault("protocol");
            if (string.IsNullOrEmpty(protocol))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = string.Empty,
                UpdatedState = newState
            };
        }

        private static CommandResult RouterOspfHandler(CommandContext ctx)
        {
            var processId = ctx.ParsedArguments.GetValueOrDefault("process-id");
            if (string.IsNullOrEmpty(processId))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            // Enter OSPF configuration mode
            ctx.Session.CurrentMode = CliMode.RouterConfig;
            ctx.Session.SessionVariables["ospf_process"] = processId;
            ctx.Session.SessionVariables["routing_protocol"] = "ospf";

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = $"\n{ctx.Device.Hostname}(config-router)#",
                UpdatedState = newState
            };
        }

        private static CommandResult ShowIpOspfHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine(" Routing Process \"ospf 1\" with ID 192.168.1.1");
            output.AppendLine(" Start time: 00:02:31.234, Time elapsed: 00:05:12.456");
            output.AppendLine("");
            output.AppendLine(" Supports only single TOS(TOS0) routes");
            output.AppendLine(" Supports opaque LSA");
            output.AppendLine(" Supports Link-local Signaling (LLS)");
            output.AppendLine(" Supports area transit capability");
            output.AppendLine("");
            output.AppendLine(" Router is not originating router-LSAs with maximum metric");
            output.AppendLine(" Initial SPF schedule delay 5000 msecs");
            output.AppendLine(" Minimum hold time between two consecutive SPFs 10000 msecs");
            output.AppendLine(" Maximum wait time between two consecutive SPFs 10000 msecs");
            output.AppendLine(" Incremental-SPF disabled");
            output.AppendLine(" Minimum LSA interval 5 secs");
            output.AppendLine(" Minimum LSA arrival 1000 msecs");
            output.AppendLine("");
            output.AppendLine(" Area BACKBONE(0.0.0.0)");
            output.AppendLine("    Number of interfaces in this area is 2");
            output.AppendLine("    Area has no authentication");
            output.AppendLine("    SPF algorithm last executed 00:02:31.234 ago");
            output.AppendLine("    SPF algorithm executed 3 times");
            output.AppendLine("    Area ranges are");
            output.AppendLine("    Number of LSA 3. Checksum Sum 0x01A3E5");
            output.AppendLine("    Number of opaque link LSA 0. Checksum Sum 0x000000");
            output.AppendLine("    Number of DCbitless LSA 0");
            output.AppendLine("    Number of indication LSA 0");
            output.AppendLine("    Number of DoNotAge LSA 0");
            output.AppendLine("    Flood list length 0");

            return CommandResult.CreateSuccess(output.ToString());
        }

        private static CommandResult AreaRangeHandler(CommandContext ctx)
        {
            var areaId = ctx.ParsedArguments.GetValueOrDefault("area-id");
            var network = ctx.ParsedArguments.GetValueOrDefault("network");
            var mask = ctx.ParsedArguments.GetValueOrDefault("mask");

            if (string.IsNullOrEmpty(areaId) || string.IsNullOrEmpty(network) || string.IsNullOrEmpty(mask))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = string.Empty,
                UpdatedState = newState
            };
        }

        private static CommandResult IpRouteHandler(CommandContext ctx)
        {
            var network = ctx.ParsedArguments.GetValueOrDefault("network");
            var mask = ctx.ParsedArguments.GetValueOrDefault("mask");
            var nextHop = ctx.ParsedArguments.GetValueOrDefault("next-hop");

            if (string.IsNullOrEmpty(network) || string.IsNullOrEmpty(mask) || string.IsNullOrEmpty(nextHop))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = string.Empty,
                UpdatedState = newState
            };
        }

        private static CommandResult RouterEigrpHandler(CommandContext ctx)
        {
            var asNumber = ctx.ParsedArguments.GetValueOrDefault("as-number");
            if (string.IsNullOrEmpty(asNumber))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            // Enter EIGRP configuration mode
            ctx.Session.CurrentMode = CliMode.RouterConfig;
            ctx.Session.SessionVariables["eigrp_as"] = asNumber;
            ctx.Session.SessionVariables["routing_protocol"] = "eigrp";

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = $"\n{ctx.Device.Hostname}(config-router)#",
                UpdatedState = newState
            };
        }

        private static CommandResult ShowIpEigrpHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("EIGRP-IPv4 Protocol for AS(65001)");
            output.AppendLine("  Metric weight K1=1, K2=0, K3=1, K4=0, K5=0");
            output.AppendLine("  NSF-aware route hold timer is 240");
            output.AppendLine("  Router-ID: 192.168.1.1");
            output.AppendLine("  Robustness: 50 percent");
            output.AppendLine("  Topology : 0 (base)");
            output.AppendLine("    Active Timer: 3 min");
            output.AppendLine("    Distance: internal 90 external 170");
            output.AppendLine("    Maximum path: 4");
            output.AppendLine("    Maximum hopcount 100");
            output.AppendLine("    Maximum metric variance 1");
            output.AppendLine("");
            output.AppendLine("  Interfaces:");
            output.AppendLine("    GigabitEthernet0/0");
            output.AppendLine("    GigabitEthernet0/1");

            return CommandResult.CreateSuccess(output.ToString());
        }

        private static CommandResult RouterRipHandler(CommandContext ctx)
        {
            // Enter RIP configuration mode
            ctx.Session.CurrentMode = CliMode.RouterConfig;
            ctx.Session.SessionVariables["routing_protocol"] = "rip";

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = $"\n{ctx.Device.Hostname}(config-router)#",
                UpdatedState = newState
            };
        }

        private static CommandResult VersionHandler(CommandContext ctx)
        {
            var version = ctx.ParsedArguments.GetValueOrDefault("version");
            if (string.IsNullOrEmpty(version))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = string.Empty,
                UpdatedState = newState
            };
        }

        private static CommandResult NoRouterBgpHandler(CommandContext ctx)
        {
            var asNumber = ctx.ParsedArguments.GetValueOrDefault("as-number");
            if (string.IsNullOrEmpty(asNumber))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = string.Empty,
                UpdatedState = newState
            };
        }

        private static CommandResult ShowIpProtocolsHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("*** IP Routing is NSF aware ***");
            output.AppendLine("");
            output.AppendLine("Routing Protocol is \"bgp 65001\"");
            output.AppendLine("  Outgoing update filter list for all interfaces is not set");
            output.AppendLine("  Incoming update filter list for all interfaces is not set");
            output.AppendLine("  IGP synchronization is disabled");
            output.AppendLine("  Automatic route summarization is disabled");
            output.AppendLine("  Unicast TCP transport is enabled");
            output.AppendLine("  Neighbor(s):");
            output.AppendLine("    Address          FiltIn FiltOut DistIn DistOut Weight RouteMap");
            output.AppendLine("    192.168.2.2");
            output.AppendLine("  Maximum path: 1");
            output.AppendLine("  Routing Information Sources:");
            output.AppendLine("    Gateway         Distance      Last Update");
            output.AppendLine("    192.168.2.2           20      00:02:31");
            output.AppendLine("");
            output.AppendLine("Routing Protocol is \"ospf 1\"");
            output.AppendLine("  Outgoing update filter list for all interfaces is not set");
            output.AppendLine("  Incoming update filter list for all interfaces is not set");
            output.AppendLine("  Router ID: 192.168.1.1");
            output.AppendLine("  Number of areas in this router is 1. 1 normal 0 stub 0 nssa");
            output.AppendLine("  Number of areas transit capable is 0");
            output.AppendLine("  External flood list length 0");
            output.AppendLine("  IETF NSF helper support enabled");
            output.AppendLine("  Cisco NSF helper support enabled");
            output.AppendLine("  Reference bandwidth unit is 100 mbps");
            output.AppendLine("  Area BACKBONE(0.0.0.0)");
            output.AppendLine("    Number of interfaces in this area is 2");
            output.AppendLine("    Area has no authentication");
            output.AppendLine("    SPF algorithm last executed 00:02:31.234 ago");
            output.AppendLine("    SPF algorithm executed 3 times");
            output.AppendLine("    Area ranges are");
            output.AppendLine("    Number of LSA 3. Checksum Sum 0x01A3E5");
            output.AppendLine("    Number of opaque link LSA 0. Checksum Sum 0x000000");
            output.AppendLine("    Number of DCbitless LSA 0");
            output.AppendLine("    Number of indication LSA 0");
            output.AppendLine("    Number of DoNotAge LSA 0");
            output.AppendLine("    Flood list length 0");

            return CommandResult.CreateSuccess(output.ToString());
        }

        private static CommandResult ShowIpRouteHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("Codes: L - local, C - connected, S - static, R - RIP, M - mobile, B - BGP");
            output.AppendLine("       D - EIGRP, EX - EIGRP external, O - OSPF, IA - OSPF inter area");
            output.AppendLine("       N1 - OSPF NSSA external type 1, N2 - OSPF NSSA external type 2");
            output.AppendLine("       E1 - OSPF external type 1, E2 - OSPF external type 2");
            output.AppendLine("       i - IS-IS, su - IS-IS summary, L1 - IS-IS level-1, L2 - IS-IS level-2");
            output.AppendLine("       ia - IS-IS inter area, * - candidate default, U - per-user static route");
            output.AppendLine("       o - ODR, P - periodic downloaded static route, H - NHRP, l - LISP");
            output.AppendLine("       a - application route");
            output.AppendLine("       + - replicated route, % - next hop override");
            output.AppendLine("");
            output.AppendLine("Gateway of last resort is not set");
            output.AppendLine("");

            // Show connected routes for interfaces with IP addresses
            foreach (var iface in ctx.Device.Interfaces.Where(i => !string.IsNullOrEmpty(i.IpAddress)))
            {
                output.AppendLine($"C    {iface.IpAddress}/{GetSubnetMaskBits(iface.SubnetMask)} is directly connected, {iface.Name}");
            }

            // Show static routes if any
            output.AppendLine("S*   0.0.0.0/0 [1/0] via 192.168.1.1");

            return CommandResult.CreateSuccess(output.ToString());
        }

        private static CommandResult ShowArpHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("Protocol  Address          Age (min)  Hardware Addr   Type   Interface");
            output.AppendLine("Internet  192.168.1.1            -   0000.0c07.ac01  ARPA   GigabitEthernet0/0");
            output.AppendLine("Internet  192.168.1.2            0   0000.0c07.ac02  ARPA   GigabitEthernet0/0");
            output.AppendLine("Internet  192.168.2.1            1   0000.0c07.ac03  ARPA   GigabitEthernet0/1");

            return CommandResult.CreateSuccess(output.ToString());
        }

        private static CommandResult PingHandler(CommandContext ctx)
        {
            var targetIp = ctx.ParsedArguments.GetValueOrDefault("ip-address");
            if (string.IsNullOrEmpty(targetIp))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            // Check if target IP is in our routing table and interface is reachable
            var route = ctx.State.RoutingTable.FirstOrDefault(r =>
                IsIpInNetwork(targetIp, r.Destination, GetSubnetMaskFromCidr(r.Destination)));

            if (route == null)
            {
                return CommandResult.CreateSuccess($"PING {targetIp}\nNo route to host");
            }

            // Check if the interface for this route is reachable
            if (!ctx.State.IsInterfaceReachable(route.Interface))
            {
                return CommandResult.CreateSuccess($"PING {targetIp}\nDestination Host Unreachable");
            }

            // Check if the target IP is directly connected to this interface
            var targetInterface = ctx.Device.Interfaces.FirstOrDefault(i =>
                i.Name == route.Interface && !string.IsNullOrEmpty(i.IpAddress));

            if (targetInterface != null)
            {
                // Check if target IP is in the same subnet as this interface
                if (IsIpInNetwork(targetIp, targetInterface.IpAddress, targetInterface.SubnetMask))
                {
                    // This is a direct connection - simulate ARP resolution
                    var arpEntry = ctx.State.GetArpEntry(targetIp);
                    if (arpEntry == null)
                    {
                        // Auto-populate ARP table for direct connections (simplified)
                        var fakeMac = GenerateFakeMacAddress();
                        ctx.State.AddArpEntry(new ArpEntry
                        {
                            IpAddress = targetIp,
                            MacAddress = fakeMac,
                            Interface = route.Interface,
                            Type = ArpType.Dynamic
                        });

                        return CommandResult.CreateSuccess($"PING {targetIp}\nNo ARP entry for host");
                    }
                }
            }

            // Simulate ping success
            var output = new System.Text.StringBuilder();
            output.AppendLine($"PING {targetIp} (using {route.Interface})");
            output.AppendLine($"64 bytes from {targetIp}: icmp_seq=1 ttl=64 time=1.2 ms");
            output.AppendLine($"64 bytes from {targetIp}: icmp_seq=2 ttl=64 time=0.8 ms");
            output.AppendLine($"64 bytes from {targetIp}: icmp_seq=3 ttl=64 time=0.9 ms");
            output.AppendLine($"64 bytes from {targetIp}: icmp_seq=4 ttl=64 time=1.1 ms");
            output.AppendLine($"64 bytes from {targetIp}: icmp_seq=5 ttl=64 time=0.7 ms");
            output.AppendLine("");
            output.AppendLine($"--- {targetIp} ping statistics ---");
            output.AppendLine("5 packets transmitted, 5 received, 0% packet loss, time 4001ms");
            output.AppendLine("rtt min/avg/max/mdev = 0.700/0.940/1.200/0.170 ms");

            return CommandResult.CreateSuccess(output.ToString());
        }

        private static bool IsIpInNetwork(string ipAddress, string networkAddress, string subnetMask)
        {
            if (string.IsNullOrEmpty(ipAddress) || string.IsNullOrEmpty(networkAddress) || string.IsNullOrEmpty(subnetMask))
                return false;

            try
            {
                var ipBytes = IpAddressToBytes(ipAddress);
                var networkBytes = IpAddressToBytes(networkAddress);
                var maskBytes = IpAddressToBytes(subnetMask);

                for (int i = 0; i < 4; i++)
                {
                    if ((ipBytes[i] & maskBytes[i]) != (networkBytes[i] & maskBytes[i]))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetSubnetMaskFromCidr(string networkAddress)
        {
            // Extract CIDR notation or return default mask
            if (networkAddress.Contains('/'))
            {
                var cidrPart = networkAddress.Split('/')[1];
                if (int.TryParse(cidrPart, out var cidr))
                {
                    return CidrToSubnetMask(cidr);
                }
            }

            return "255.255.255.255"; // Default to host route
        }

        private static string CidrToSubnetMask(int cidr)
        {
            var mask = 0xFFFFFFFF << (32 - cidr);
            return $"{(mask >> 24) & 0xFF}.{(mask >> 16) & 0xFF}.{(mask >> 8) & 0xFF}.{mask & 0xFF}";
        }

        private static byte[] IpAddressToBytes(string ipAddress)
        {
            return ipAddress.Split('.').Select(byte.Parse).ToArray();
        }

        private static string GenerateFakeMacAddress()
        {
            var random = new Random();
            var bytes = new byte[6];
            random.NextBytes(bytes);

            // Set the locally administered bit and unicast bit
            bytes[0] = (byte)((bytes[0] & 0xFC) | 0x02);

            return string.Join(":", bytes.Select(b => b.ToString("X2")));
        }

        private static string GetSubnetMaskBits(string subnetMask)
        {
            if (string.IsNullOrEmpty(subnetMask))
                return "32";

            var bytes = subnetMask.Split('.').Select(byte.Parse).ToArray();
            var maskBits = 0;
            foreach (var b in bytes)
            {
                maskBits += BitOperations.PopCount((uint)b);
            }
            return maskBits.ToString();
        }

        private static string GenerateDefaultConfig(NetworkDevice device)
        {
            return $@"! Last configuration change at {DateTime.Now}
!
version 15.2
service timestamps debug datetime msec
service timestamps log datetime msec
no service password-encryption
!
hostname {device.Hostname}
!
boot-start-marker
boot-end-marker
!
!
!
no aaa new-model
!
!
!
!
!
!
!
!
ip cef
no ipv6 cef
!
!
!
!
license udi pid WS-C2960-24TT-L sn FOC1234W5678
!
!
!
!
!
!
!
!
!
!
!
!
!
line con 0
line vty 0 4
 login
!
end";
        }
    }
}
