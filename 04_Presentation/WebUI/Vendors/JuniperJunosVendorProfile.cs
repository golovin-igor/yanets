using Yanets.Core.Commands;
using Yanets.Core.Interfaces;
using Yanets.Core.Snmp;
using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.SharedKernel;
using Yanets.WebUI.Services;

namespace Yanets.WebUI.Vendors
{
    public class JuniperJunosVendorProfile : VendorProfile
    {
        public override string VendorName => "Juniper";
        public override string Os => "JunOS";
        public override string Version => "18.4R1";

        public override ICommandParser CommandParser => new Services.JuniperCommandParser();
        public override IPromptGenerator PromptGenerator => new Services.JuniperPromptGenerator();
        public override IMibProvider MibProvider => new Services.JuniperMibProvider();

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
                ["configure"] = new CommandDefinition
                {
                    Syntax = "configure",
                    PrivilegeLevel = 15,
                    Handler = ConfigureHandler
                }
            };

            SysObjectId = "1.3.6.1.4.1.2636.1.1.1.4"; // Juniper router
        }

        private static CommandResult ShowVersionHandler(CommandContext ctx)
        {
            var device = ctx.Device;
            return CommandResult.CreateSuccess($@"
Hostname: {device.Hostname}
Model: mx480
Junos: 18.4R1.8
JUNOS Software Release [18.4R1.8]

{device.Hostname} (ttyu0)

login:
");
        }

        private static CommandResult ShowInterfacesTerseHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("Interface               Admin Link Proto    Local                 Remote");

            foreach (var iface in ctx.Device.Interfaces.Take(5)) // Show first 5 interfaces
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

        private static CommandResult ConfigureHandler(CommandContext ctx)
        {
            ctx.Session.CurrentMode = SharedKernel.CliMode.GlobalConfig;
            ctx.Session.ModeStack.Push(SharedKernel.CliMode.PrivilegedExec);

            return CommandResult.CreateSuccess(
                $"Entering configuration mode\n\n{ctx.Device.Hostname}#"
            );
        }
    }
}
