using Yanets.Core.Commands;
using Yanets.Core.Models;
using Yanets.SharedKernel;

namespace Yanets.Infrastructure.CommandHandlers.Cisco
{
    /// <summary>
    /// Cisco IOS command handlers implementation
    /// </summary>
    public static class CiscoCommandHandlers
    {
        public static CommandResult ShowVersionHandler(CommandContext ctx)
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

        public static CommandResult ShowIpInterfaceBriefHandler(CommandContext ctx)
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

        public static CommandResult ShowRunningConfigHandler(CommandContext ctx)
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

        public static CommandResult ConfigureTerminalHandler(CommandContext ctx)
        {
            ctx.Session.CurrentMode = CliMode.GlobalConfig;
            ctx.Session.ModeStack.Push(CliMode.PrivilegedExec);

            return CommandResult.CreateSuccess(
                $"Enter configuration commands, one per line.  " +
                $"End with CNTL/Z.\n{ctx.Device.Hostname}(config)#"
            );
        }

        public static CommandResult InterfaceHandler(CommandContext ctx)
        {
            var interfaceName = ctx.ParsedArguments.GetValueOrDefault("interface");
            if (string.IsNullOrEmpty(interfaceName))
            {
                return CommandResult.CreateError("% Incomplete command.");
            }

            var iface = ctx.Device.Interfaces
                .FirstOrDefault(i => i.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase));

            if (iface == null)
            {
                return CommandResult.CreateError($"% Invalid interface {interfaceName}");
            }

            ctx.Session.CurrentMode = CliMode.InterfaceConfig;
            ctx.Session.SessionVariables["current_interface"] = iface;

            return CommandResult.CreateSuccess(string.Empty);
        }

        public static CommandResult IpAddressHandler(CommandContext ctx)
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

        public static CommandResult NoShutdownHandler(CommandContext ctx)
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

        public static CommandResult ShowIpRouteHandler(CommandContext ctx)
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

            return CommandResult.CreateSuccess(output.ToString());
        }

        public static CommandResult ShowProcessesCpuHandler(CommandContext ctx)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("CPU utilization for five seconds: 5%/0%; one minute: 3%; five minutes: 2%");
            output.AppendLine("");
            output.AppendLine("  PID Runtime(ms)   Invoked  uSecs   5Sec   1Min   5Min TTY Process");
            output.AppendLine("    1           4        46     86  0.00%  0.00%  0.00%   0 Chunk Manager");
            output.AppendLine("    2          28       149    187  0.00%  0.00%  0.00%   0 Load Meter");
            output.AppendLine($"    3       {ctx.State.Resources.CpuUtilization}        12    416  0.00%  0.00%  0.00%   0 Exec");
            output.AppendLine("    5           0         1      0  0.00%  0.00%  0.00%   0 Check heaps");
            output.AppendLine("    6           0         1      0  0.00%  0.00%  0.00%   0 Pool Manager");

            return CommandResult.CreateSuccess(output.ToString());
        }

        public static CommandResult ShowMemoryStatisticsHandler(CommandContext ctx)
        {
            var resources = ctx.State.Resources;
            var output = new System.Text.StringBuilder();
            output.AppendLine("                Head    Total(b)     Used(b)     Free(b)   Lowest(b)  Largest(b)");
            output.AppendLine($"Processor    12345678   {resources.MemoryTotal * 1024}   {resources.MemoryUsed * 1024}   {(resources.MemoryTotal - resources.MemoryUsed) * 1024}   {resources.MemoryUsed * 1024}   {(resources.MemoryTotal - resources.MemoryUsed) * 1024}");

            return CommandResult.CreateSuccess(output.ToString());
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
