using Yanets.Core.Commands;
using Yanets.Core.Interfaces;
using Yanets.Core.Snmp;
using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.SharedKernel;
using Yanets.WebUI.Services;

namespace Yanets.WebUI.Vendors
{
    public class CiscoIosVendorProfile : VendorProfile
    {
        public override string VendorName => "Cisco";
        public override string Os => "IOS";
        public override string Version => "15.2";

        public override ICommandParser CommandParser => new Services.CiscoCommandParser();
        public override IPromptGenerator PromptGenerator => new Services.CiscoPromptGenerator();
        public override IMibProvider MibProvider => new Services.CiscoMibProvider();

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
                ["configure terminal"] = new CommandDefinition
                {
                    Syntax = "configure terminal",
                    PrivilegeLevel = 15,
                    Handler = ConfigureTerminalHandler
                }
            };

            SysObjectId = "1.3.6.1.4.1.9.1.1208"; // Cisco router
        }

        private static CommandResult ShowVersionHandler(CommandContext ctx)
        {
            var device = ctx.Device;
            return CommandResult.CreateSuccess($@"
Cisco IOS Software, {device.Type} Software (C2960-LANBASEK9-M), Version 15.2(4)E8, RELEASE SOFTWARE (fc2)
Technical Support: http://www.cisco.com/techsupport
Copyright (c) 1986-2019 by Cisco Systems, Inc.
Compiled Tue 25-Jun-19 12:53 by prod_rel_team

{device.Hostname} uptime is 1 week, 3 days, 2 hours, 15 minutes
System returned to ROM by power-on
System image file is ""flash:c2960-lanbasek9-mz.150-2.E8.bin""

cisco WS-C2960-24TT-L (PowerPC405) processor (revision B0) with 65536K bytes of memory.
Processor board ID FOC1234W5678
Last reset from power-on
1 Virtual Ethernet interface
24 FastEthernet interfaces
2 Gigabit Ethernet interfaces

64K bytes of flash-simulated non-volatile configuration memory.
Base ethernet MAC Address       : {GenerateMacAddress(device)}
Motherboard assembly number     : 73-10390-03
Power supply part number        : 341-0097-02
Motherboard serial number       : FOC12345678
Power supply serial number      : AZS12345678
Model revision number           : B0
Motherboard revision number     : B0
Model number                    : WS-C2960-24TT-L
System serial number            : FOC1234W5678

Configuration register is 0xF
");
        }

        private static CommandResult ShowIpInterfaceBriefHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("Interface              IP-Address      OK? Method Status                Protocol");

            foreach (var iface in ctx.Device.Interfaces.Where(i => !string.IsNullOrEmpty(i.IpAddress)))
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

        private static CommandResult ConfigureTerminalHandler(CommandContext ctx)
        {
            ctx.Session.CurrentMode = SharedKernel.CliMode.GlobalConfig;
            ctx.Session.ModeStack.Push(SharedKernel.CliMode.PrivilegedExec);

            return CommandResult.CreateSuccess(
                $"Enter configuration commands, one per line.  " +
                $"End with CNTL/Z.\n{ctx.Device.Hostname}(config)#"
            );
        }

        private static string GenerateMacAddress(NetworkDevice device)
        {
            // Generate a realistic Cisco MAC address
            var random = new Random();
            var bytes = new byte[6];
            random.NextBytes(bytes);
            bytes[0] = 0x00; // Cisco OUI starts with 00

            return string.Join(":", bytes.Select(b => b.ToString("X2")));
        }
    }
}
