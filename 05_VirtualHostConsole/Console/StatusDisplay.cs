using Yanets.VirtualHostConsole.Core.Interfaces;
using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Console
{
    public class StatusDisplay
    {
        private readonly IVirtualNetworkManager _networkManager;
        private Timer _displayTimer;
        private bool _isDisplaying;

        public StatusDisplay(IVirtualNetworkManager networkManager)
        {
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
        }

        public void StartRealTimeDisplay()
        {
            if (_isDisplaying)
                return;

            _isDisplaying = true;
            _displayTimer = new Timer(UpdateDisplay, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        }

        public void StopRealTimeDisplay()
        {
            if (!_isDisplaying)
                return;

            _isDisplaying = false;
            _displayTimer?.Dispose();
            _displayTimer = null;

            // Clear the status lines
            ClearStatusLines();
        }

        private void UpdateDisplay(object state)
        {
            if (!_isDisplaying)
                return;

            var stats = _networkManager.GetNetworkStatistics();

            // Save cursor position
            var top = Console.CursorTop;

            // Move to status area (top of console)
            Console.SetCursorPosition(0, 0);
            ClearStatusLines();

            // Display status
            Console.WriteLine($"YANETS Virtual Host Console - Network Status: {(stats.ActiveHosts > 0 ? "RUNNING" : "STOPPED")}");
            Console.WriteLine($"Hosts: {stats.ActiveHosts}/{stats.TotalHosts} | Connections: {stats.ActiveConnections} | Updated: {stats.LastUpdated:hh:mm:ss}");
            Console.WriteLine(new string('=', 80));

            // Show host details if any
            if (stats.PerHostStats.Any())
            {
                Console.WriteLine("Active Hosts:");
                foreach (var (hostId, hostStats) in stats.PerHostStats.Take(3)) // Show first 3 hosts
                {
                    var host = _networkManager.GetHostById(hostId);
                    Console.WriteLine($"  {host?.Hostname ?? hostId}: {hostStats.ActiveConnections} conn, {hostStats.CpuUtilization:F1}% CPU");
                }

                if (stats.PerHostStats.Count > 3)
                {
                    Console.WriteLine($"  ... and {stats.PerHostStats.Count - 3} more hosts");
                }
            }

            Console.WriteLine(new string('=', 80));

            // Restore cursor position
            Console.SetCursorPosition(0, Math.Max(top, 5));
        }

        private void ClearStatusLines()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth));
            }
            Console.SetCursorPosition(0, 0);
        }

        public void ShowFullStatus()
        {
            var stats = _networkManager.GetNetworkStatistics();

            Console.WriteLine("=== Network Status ===");
            Console.WriteLine($"Total Hosts: {stats.TotalHosts}");
            Console.WriteLine($"Active Hosts: {stats.ActiveHosts}");
            Console.WriteLine($"Total Connections: {stats.TotalConnections}");
            Console.WriteLine($"Active Connections: {stats.ActiveConnections}");
            Console.WriteLine($"Last Updated: {stats.LastUpdated}");
            Console.WriteLine();

            if (stats.ProtocolCounts.Any())
            {
                Console.WriteLine("Protocol Statistics:");
                foreach (var (protocol, count) in stats.ProtocolCounts)
                {
                    Console.WriteLine($"  {protocol}: {count} connections");
                }
                Console.WriteLine();
            }

            if (stats.PerHostStats.Any())
            {
                Console.WriteLine("Per-Host Statistics:");
                foreach (var (hostId, hostStats) in stats.PerHostStats)
                {
                    var host = _networkManager.GetHostById(hostId);
                    Console.WriteLine($"  {host?.Hostname ?? hostId}:");
                    Console.WriteLine($"    Status: {host?.Status}");
                    Console.WriteLine($"    Connections: {hostStats.ActiveConnections}");
                    Console.WriteLine($"    Commands: {hostStats.TotalCommandsExecuted}");
                    Console.WriteLine($"    SNMP Requests: {hostStats.TotalSnmpRequests}");
                    Console.WriteLine($"    Uptime: {hostStats.Uptime:hh\\:mm\\:ss}");
                    Console.WriteLine($"    CPU: {hostStats.CpuUtilization:F1}%");
                    Console.WriteLine($"    Memory: {hostStats.MemoryUsed / 1024:F1} KB");
                    Console.WriteLine();
                }
            }
        }
    }
}