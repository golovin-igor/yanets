using Yanets.Core.Commands;
using Yanets.Core.Interfaces;
using Yanets.Core.Models;
using Yanets.Core.Vendors;
using Yanets.SharedKernel;

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

            var newState = ctx.State.Clone();
            return new CommandResult
            {
                Success = true,
                Output = $"\n%LINK-3-UPDOWN: Interface {iface.Name}, changed state to up\n" +
                         $"%LINEPROTO-5-UPDOWN: Line protocol on Interface {iface.Name}, changed state to up\n",
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
