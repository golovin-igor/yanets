using Yanets.Core.Commands;
using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.SharedKernel;

namespace Yanets.Application.Services.Vendors
{
    /// <summary>
    /// Juniper JunOS vendor profile implementation for the application layer
    /// </summary>
    public class JuniperJunosVendorProfile : VendorProfile
    {
        public override string VendorName => "Juniper";
        public override string Os => "JunOS";
        public override string Version => "18.4R1.8";

        public override ICommandParser CommandParser => new CommandParser();
        public override IPromptGenerator PromptGenerator => new PromptGenerator();
        public override IMibProvider MibProvider => new MibProvider();

        public override string LoginPrompt => "login: ";

        public JuniperJunosVendorProfile()
        {
            Commands = new Dictionary<string, CommandDefinition>
            {
                ["show version"] = new CommandDefinition
                {
                    Syntax = "show version",
                    PrivilegeLevel = 1,
                    Handler = ShowVersionHandler
                },
                ["show interfaces terse"] = new CommandDefinition
                {
                    Syntax = "show interfaces terse",
                    PrivilegeLevel = 1,
                    Handler = ShowInterfacesTerseHandler
                },
                ["show configuration"] = new CommandDefinition
                {
                    Syntax = "show configuration",
                    PrivilegeLevel = 15,
                    Handler = ShowConfigurationHandler
                },
                ["configure"] = new CommandDefinition
                {
                    Syntax = "configure",
                    PrivilegeLevel = 15,
                    Handler = ConfigureHandler
                },
                ["set interfaces"] = new CommandDefinition
                {
                    Syntax = "set interfaces <interface> <property>",
                    PrivilegeLevel = 15,
                    Handler = SetInterfaceHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "interface", IsRequired = true },
                        new CommandParameter { Name = "property", IsRequired = true }
                    }
                },
                ["set protocols bgp"] = new CommandDefinition
                {
                    Syntax = "set protocols bgp <as-number>",
                    PrivilegeLevel = 15,
                    Handler = SetProtocolsBgpHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "as-number", IsRequired = true }
                    }
                },
                ["set protocols ospf area"] = new CommandDefinition
                {
                    Syntax = "set protocols ospf area <area-id> interface <interface>",
                    PrivilegeLevel = 15,
                    Handler = SetProtocolsOspfAreaHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "area-id", IsRequired = true },
                        new CommandParameter { Name = "interface", IsRequired = true }
                    }
                },
                ["show bgp summary"] = new CommandDefinition
                {
                    Syntax = "show bgp summary",
                    PrivilegeLevel = 1,
                    Handler = ShowBgpSummaryHandler
                },
                ["show ospf neighbor"] = new CommandDefinition
                {
                    Syntax = "show ospf neighbor",
                    PrivilegeLevel = 1,
                    Handler = ShowOspfNeighborHandler
                },
                ["show route protocol bgp"] = new CommandDefinition
                {
                    Syntax = "show route protocol bgp",
                    PrivilegeLevel = 1,
                    Handler = ShowRouteProtocolBgpHandler
                },
                ["set routing-options static route"] = new CommandDefinition
                {
                    Syntax = "set routing-options static route <network> next-hop <next-hop>",
                    PrivilegeLevel = 15,
                    Handler = SetStaticRouteHandler,
                    Parameters = new List<CommandParameter>
                    {
                        new CommandParameter { Name = "network", IsRequired = true },
                        new CommandParameter { Name = "next-hop", IsRequired = true }
                    }
                },
                ["show route"] = new CommandDefinition
                {
                    Syntax = "show route",
                    PrivilegeLevel = 1,
                    Handler = ShowRouteHandler
                }
            };

            SysObjectId = "1.3.6.1.4.1.2636.1.1.1.4"; // Juniper router
        }

        private static CommandResult ShowVersionHandler(CommandContext ctx)
        {
            var device = ctx.Device;

            var output = $@"Hostname: {device.Hostname}
Model: mx480
Junos: 18.4R1.8
JUNOS Software Release [18.4R1.8]

{device.Hostname} (ttyu0)

login: ";

            return CommandResult.CreateSuccess(output);
        }

        private static CommandResult ShowInterfacesTerseHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("Interface               Admin Link Proto    Local                 Remote");

            foreach (var iface in ctx.Device.Interfaces.Take(8)) // Show first 8 interfaces
            {
                var admin = iface.IsUp ? "up" : "down";
                var link = iface.IsUp ? "up" : "down";
                var proto = "inet";

                output.AppendLine(
                    $"{iface.Name,-23} " +
                    $"{admin,-5} " +
                    $"{link,-4} " +
                    $"{proto,-8} " +
                    $"{iface.IpAddress ?? "",-21}"
                );
            }

            return CommandResult.CreateSuccess(output.ToString());
        }

        private static CommandResult ShowConfigurationHandler(CommandContext ctx)
        {
            var config = ctx.State.RunningConfig;
            if (string.IsNullOrEmpty(config))
            {
                config = GenerateDefaultConfig(ctx.Device);
            }

            return CommandResult.CreateSuccess(config);
        }

        private static CommandResult ConfigureHandler(CommandContext ctx)
        {
            ctx.Session.CurrentMode = CliMode.GlobalConfig;
            ctx.Session.ModeStack.Push(CliMode.PrivilegedExec);

            return CommandResult.CreateSuccess(
                $"Entering configuration mode\n\n[edit]\n{ctx.Device.Hostname}# "
            );
        }

        private static CommandResult SetInterfaceHandler(CommandContext ctx)
        {
            var interfaceName = ctx.ParsedArguments.GetValueOrDefault("interface");
            var property = ctx.ParsedArguments.GetValueOrDefault("property");

            if (string.IsNullOrEmpty(interfaceName) || string.IsNullOrEmpty(property))
            {
                return CommandResult.CreateError("syntax error");
            }

            var iface = ctx.Device.Interfaces.FirstOrDefault(i =>
                i.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase));

            if (iface == null)
            {
                return CommandResult.CreateError($"Interface {interfaceName} not found");
            }

            // Handle different interface properties
            if (property.StartsWith("unit"))
            {
                // VLAN or subinterface configuration
            }
            else if (property.Contains("family inet address"))
            {
                // IP address configuration (simplified)
            }

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = string.Empty,
                UpdatedState = newState
            };
        }

        private static CommandResult SetProtocolsBgpHandler(CommandContext ctx)
        {
            var asNumber = ctx.ParsedArguments.GetValueOrDefault("as-number");
            if (string.IsNullOrEmpty(asNumber))
            {
                return CommandResult.CreateError("syntax error");
            }

            // Enter BGP configuration mode
            ctx.Session.CurrentMode = CliMode.RouterConfig;

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = string.Empty,
                UpdatedState = newState
            };
        }

        private static CommandResult SetProtocolsOspfAreaHandler(CommandContext ctx)
        {
            var areaId = ctx.ParsedArguments.GetValueOrDefault("area-id");
            var interfaceName = ctx.ParsedArguments.GetValueOrDefault("interface");

            if (string.IsNullOrEmpty(areaId) || string.IsNullOrEmpty(interfaceName))
            {
                return CommandResult.CreateError("syntax error");
            }

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = string.Empty,
                UpdatedState = newState
            };
        }

        private static CommandResult ShowBgpSummaryHandler(CommandContext ctx)
        {
            var output = @"Groups: 1 Peers: 1 Down peers: 0
  Table          Tot Paths  Act Paths Suppressed    History Damp State    Pending
  inet.0                 0          0          0          0          0          0
  Peer                     AS      InPkt     OutPkt    OutQ   Flaps Last Up/Dwn State|#Active/Received/Accepted/Damped...
";

            return CommandResult.CreateSuccess(output);
        }

        private static CommandResult ShowOspfNeighborHandler(CommandContext ctx)
        {
            var output = @"Address          Interface              State     ID               Pri  Dead
192.168.1.2      ge-0/0/0.0             Full      192.168.1.2      128    35
";

            return CommandResult.CreateSuccess(output);
        }

        private static CommandResult ShowRouteProtocolBgpHandler(CommandContext ctx)
        {
            var output = @"inet.0: 5 destinations, 5 routes (5 active, 0 holddown, 0 hidden)
+ = Active Route, - = Last Active, * = Both

10.0.0.0/8         *[BGP/170] 00:00:05, localpref 100
                      AS path: 65001 I, validation-state: unverified
                    > to 192.168.1.2 via ge-0/0/0.0
";

            return CommandResult.CreateSuccess(output);
        }

        private static CommandResult SetStaticRouteHandler(CommandContext ctx)
        {
            var network = ctx.ParsedArguments.GetValueOrDefault("network");
            var nextHop = ctx.ParsedArguments.GetValueOrDefault("next-hop");

            if (string.IsNullOrEmpty(network) || string.IsNullOrEmpty(nextHop))
            {
                return CommandResult.CreateError("syntax error");
            }

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = string.Empty,
                UpdatedState = newState
            };
        }

        private static CommandResult ShowRouteHandler(CommandContext ctx)
        {
            var output = @"inet.0: 5 destinations, 5 routes (5 active, 0 holddown, 0 hidden)
+ = Active Route, - = Last Active, * = Both

0.0.0.0/0          *[Static/5] 00:00:10
                    > to 192.168.1.1 via ge-0/0/0.0
192.168.1.0/24     *[Direct/0] 00:00:10
                    > via ge-0/0/0.0
192.168.1.1/32     *[Local/0] 00:00:10
                      Local via ge-0/0/0.0
";

            return CommandResult.CreateSuccess(output);
        }

        private static string GenerateDefaultConfig(NetworkDevice device)
        {
            return $"## Last commit: {DateTime.Now}\n" +
                   $"version 18.4R1.8;\n" +
                   $"system {{\n" +
                   $"    host-name {device.Hostname};\n" +
                   $"    root-authentication {{\n" +
                   $"        encrypted-password \"$1$...\"; ## SECRET-DATA\n" +
                   $"    }}\n" +
                   $"    services {{\n" +
                   $"        ssh;\n" +
                   $"        netconf {{\n" +
                   $"            ssh;\n" +
                   $"        }}\n" +
                   $"    }}\n" +
                   $"}}\n" +
                   $"interfaces {{\n" +
                   $"    ge-0/0/0 {{\n" +
                   $"        unit 0 {{\n" +
                   $"            family inet {{\n" +
                   $"                address 192.168.1.1/24;\n" +
                   $"            }}\n" +
                   $"        }}\n" +
                   $"    }}\n" +
                   $"}}\n";
        }
    }
}
