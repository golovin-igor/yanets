using Microsoft.AspNetCore.Mvc;
using System.Text;
using Yanets.WebUI.VirtualNetwork;

namespace Yanets.WebUI.Controllers
{
    [ApiController]
    [Route("virtual-network")]
    public class VirtualNetworkDashboardController : ControllerBase
    {
        private readonly IVirtualNetworkManager _networkManager;
        private readonly ILogger<VirtualNetworkDashboardController> _logger;

        public VirtualNetworkDashboardController(
            IVirtualNetworkManager networkManager,
            ILogger<VirtualNetworkDashboardController> logger)
        {
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("")]
        public ContentResult Index()
        {
            var stats = _networkManager.GetNetworkStatistics();
            var hosts = _networkManager.GetAllHosts().OrderBy(h => h.IpAddress).ToList();

            var html = new StringBuilder();
            html.AppendLine("<!doctype html>");
            html.AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\"/>");
            html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"/>");
            html.AppendLine("<title>Virtual Network Dashboard</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1, h2 { margin-bottom: 0.3rem; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin-top: 10px; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; }");
            html.AppendLine("th { background: #f5f5f5; text-align: left; }");
            html.AppendLine(".badge { display: inline-block; padding: 2px 8px; border-radius: 10px; font-size: 12px; }");
            html.AppendLine(".up { background: #d4edda; color: #155724; }");
            html.AppendLine(".down { background: #f8d7da; color: #721c24; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");

            html.AppendLine("<h1>YANETS - Virtual Network Dashboard</h1>");
            html.AppendLine("<p>Simulated subnet: 192.168.1.0/24 &nbsp; | &nbsp; Gateway: 192.168.1.1</p>");

            html.AppendLine("<h2>Summary</h2>");
            html.AppendLine("<ul>");
            html.AppendLine($"<li>Total Hosts: <strong>{stats.TotalHosts}</strong></li>");
            html.AppendLine($"<li>Active Hosts: <strong>{stats.ActiveHosts}</strong></li>");
            html.AppendLine($"<li>Active Connections: <strong>{stats.ActiveConnections}</strong></li>");
            html.AppendLine($"<li>Last Updated: <strong>{stats.LastUpdated:yyyy-MM-dd HH:mm:ss}</strong> (UTC)</li>");
            html.AppendLine("</ul>");

            html.AppendLine("<h2>Used IP Addresses</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<thead><tr><th>IP Address</th><th>Hostname</th><th>Vendor</th><th>Status</th><th>Uptime</th></tr></thead>");
            html.AppendLine("<tbody>");
            foreach (var h in hosts)
            {
                var statusClass = h.Status == VirtualNetwork.Models.HostStatus.Running ? "up" : "down";
                var uptime = DateTime.UtcNow - h.CreatedAt;
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{h.IpAddress}</td>");
                html.AppendLine($"<td>{h.Hostname}</td>");
                html.AppendLine($"<td>{h.VendorProfile?.VendorName ?? "Unknown"}</td>");
                html.AppendLine($"<td><span class=\"badge {statusClass}\">{h.Status}</span></td>");
                html.AppendLine($"<td>{uptime:dd\\:hh\\:mm\\:ss}</td>");
                html.AppendLine("</tr>");
            }
            if (hosts.Count == 0)
            {
                html.AppendLine("<tr><td colspan=\"5\">No hosts provisioned yet.</td></tr>");
            }
            html.AppendLine("</tbody>");
            html.AppendLine("</table>");

            html.AppendLine("<p style=\"margin-top:12px;color:#666;font-size:12px\">");
            html.AppendLine("API: <code>/api/virtualnetwork/subnets/usage</code> &nbsp;|&nbsp; ");
            html.AppendLine("Used IPs: <code>/api/virtualnetwork/subnets/used-ips</code>");
            html.AppendLine("</p>");

            html.AppendLine("</body></html>");

            return new ContentResult
            {
                Content = html.ToString(),
                ContentType = "text/html; charset=utf-8",
                StatusCode = 200
            };
        }
    }
}