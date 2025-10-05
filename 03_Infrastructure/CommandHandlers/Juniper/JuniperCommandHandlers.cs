using Yanets.Core.Commands;
using Yanets.Core.Models;
using Yanets.SharedKernel;

namespace Yanets.Infrastructure.CommandHandlers.Juniper
{
    /// <summary>
    /// Juniper JunOS command handlers implementation
    /// </summary>
    public static class JuniperCommandHandlers
    {
        public static CommandResult ShowVersionHandler(CommandContext ctx)
        {
            var device = ctx.Device;

            var output = $@"Hostname: {device.Hostname}
Model: mx480
Junos: 18.4R1.8
JUNOS Software Release [18.4R1.8]

{device.Hostname} (ttyu0)

login:
";

            return CommandResult.CreateSuccess(output);
        }

        public static CommandResult ShowInterfacesTerseHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("Interface               Admin Link Proto    Local                 Remote");

            foreach (var iface in ctx.Device.Interfaces.Take(8)) // Show first 8 interfaces
            {
                var admin = iface.IsUp ? "up" : "down";
                var link = iface.IsUp ? "up" : "down";
                var proto = iface.IsUp ? "inet" : "down";

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

        public static CommandResult ShowConfigurationHandler(CommandContext ctx)
        {
            var config = ctx.State.RunningConfig;
            if (string.IsNullOrEmpty(config))
            {
                config = GenerateDefaultConfig(ctx.Device);
            }

            return CommandResult.CreateSuccess(config);
        }

        public static CommandResult ConfigureHandler(CommandContext ctx)
        {
            ctx.Session.CurrentMode = CliMode.GlobalConfig;
            ctx.Session.ModeStack.Push(CliMode.PrivilegedExec);

            return CommandResult.CreateSuccess(
                $"Entering configuration mode\n\n[edit]\n{ctx.Device.Hostname}# "
            );
        }

        public static CommandResult ShowChassisHardwareHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("Hardware inventory:");
            output.AppendLine("Item             Version  Part number  Serial number     Description");
            output.AppendLine("Chassis                                JN1234567890      MX480");
            output.AppendLine("Midplane         REV 07   710-013698   JN1234567890      MX480 Midplane");
            output.AppendLine("FPM Board        REV 03   710-014974   JN1234567890      Front Panel Display");
            output.AppendLine("PEM 0            Rev 02   740-013063   JN1234567890      Power Entry Module");
            output.AppendLine("PEM 1            Rev 02   740-013063   JN1234567890      Power Entry Module");
            output.AppendLine("SCB 0            REV 15   710-021523   JN1234567890      MX SCB");
            output.AppendLine("Routing Engine 0 REV 15   710-021523   JN1234567890      RE-MX480");
            output.AppendLine("FPC 0            REV 15   710-021523   JN1234567890      MX FPC Type 2");

            return CommandResult.CreateSuccess(output.ToString());
        }

        public static CommandResult ShowRouteSummaryHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("Autonomous system number: 65001");
            output.AppendLine("");
            output.AppendLine("Router ID: 192.168.1.1");
            output.AppendLine("");
            output.AppendLine("inet.0: 10 destinations, 10 routes (10 active, 0 holddown, 0 hidden)");
            output.AppendLine("  Direct:      5 routes,  5 active");
            output.AppendLine("  Local:       5 routes,  5 active");
            output.AppendLine("  Static:      0 routes,  0 active");
            output.AppendLine("  OSPF:        0 routes,  0 active");
            output.AppendLine("  BGP:         0 routes,  0 active");
            output.AppendLine("");

            // Show some routes
            foreach (var route in ctx.State.RoutingTable.Take(3))
            {
                output.AppendLine($"{route.Destination,-18} * [{route.Protocol,-8}] {route.Metric,3} {route.Interface}");
            }

            return CommandResult.CreateSuccess(output.ToString());
        }

        public static CommandResult ShowBgpSummaryHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("Groups: 1 Peers: 0 Down peers: 0");
            output.AppendLine("Table          Tot Paths  Act Paths Suppressed    History Damp State    Pending");
            output.AppendLine("inet.0                 0          0          0         0       0          0");
            output.AppendLine("");
            output.AppendLine("Neighbor                       AS  State");
            output.AppendLine("192.168.2.2                65002  Idle");

            return CommandResult.CreateSuccess(output.ToString());
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
