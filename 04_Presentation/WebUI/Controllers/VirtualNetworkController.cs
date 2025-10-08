using Microsoft.AspNetCore.Mvc;
using Yanets.WebUI.VirtualNetwork;
using Yanets.WebUI.VirtualNetwork.Models;

namespace Yanets.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VirtualNetworkController : ControllerBase
    {
        private readonly IVirtualNetworkManager _networkManager;
        private readonly ILogger<VirtualNetworkController> _logger;

        public VirtualNetworkController(
            IVirtualNetworkManager networkManager,
            ILogger<VirtualNetworkController> logger)
        {
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get virtual network statistics
        /// </summary>
        [HttpGet("statistics")]
        public ActionResult<NetworkStatistics> GetStatistics()
        {
            try
            {
                var stats = _networkManager.GetNetworkStatistics();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get network statistics");
                return StatusCode(500, new { error = "Failed to get network statistics" });
            }
        }

        /// <summary>
        /// Get all virtual hosts
        /// </summary>
        [HttpGet("hosts")]
        public ActionResult<IEnumerable<VirtualHostDto>> GetHosts()
        {
            try
            {
                var hosts = _networkManager.GetAllHosts();
                var hostDtos = hosts.Select(h => new VirtualHostDto
                {
                    Id = h.Id,
                    Hostname = h.Hostname,
                    IpAddress = h.IpAddress,
                    SubnetName = h.SubnetName,
                    Status = h.Status.ToString(),
                    CreatedAt = h.CreatedAt,
                    Vendor = h.VendorProfile?.VendorName ?? "Unknown"
                });

                return Ok(hostDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get virtual hosts");
                return StatusCode(500, new { error = "Failed to get virtual hosts" });
            }
        }

        /// <summary>
        /// Get a specific virtual host by ID
        /// </summary>
        [HttpGet("hosts/{hostId}")]
        public ActionResult<VirtualHostDto> GetHost(string hostId)
        {
            try
            {
                var host = _networkManager.GetHostById(hostId);
                if (host == null)
                    return NotFound(new { error = $"Host {hostId} not found" });

                var hostDto = new VirtualHostDto
                {
                    Id = host.Id,
                    Hostname = host.Hostname,
                    IpAddress = host.IpAddress,
                    SubnetName = host.SubnetName,
                    Status = host.Status.ToString(),
                    CreatedAt = host.CreatedAt,
                    Vendor = host.VendorProfile?.VendorName ?? "Unknown"
                };

                return Ok(hostDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get host {HostId}", hostId);
                return StatusCode(500, new { error = "Failed to get host" });
            }
        }

        /// <summary>
        /// Create a new virtual host
        /// </summary>
        [HttpPost("hosts")]
        public async Task<ActionResult<VirtualHostDto>> CreateHost([FromBody] CreateHostRequest request)
        {
            try
            {
                var host = await _networkManager.CreateHostAsync(
                    request.Hostname,
                    request.Vendor,
                    request.SubnetName);

                var hostDto = new VirtualHostDto
                {
                    Id = host.Id,
                    Hostname = host.Hostname,
                    IpAddress = host.IpAddress,
                    SubnetName = host.SubnetName,
                    Status = host.Status.ToString(),
                    CreatedAt = host.CreatedAt,
                    Vendor = host.VendorProfile?.VendorName ?? "Unknown"
                };

                return CreatedAtAction(nameof(GetHost), new { hostId = host.Id }, hostDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create host {Hostname}", request.Hostname);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Start the virtual network
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartNetwork()
        {
            try
            {
                var success = await _networkManager.StartNetworkAsync();
                if (success)
                {
                    return Ok(new { message = "Virtual network started successfully" });
                }
                else
                {
                    return StatusCode(500, new { error = "Failed to start virtual network" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start virtual network");
                return StatusCode(500, new { error = "Failed to start virtual network" });
            }
        }

        /// <summary>
        /// Stop the virtual network
        /// </summary>
        [HttpPost("stop")]
        public async Task<IActionResult> StopNetwork()
        {
            try
            {
                var success = await _networkManager.StopNetworkAsync();
                if (success)
                {
                    return Ok(new { message = "Virtual network stopped successfully" });
                }
                else
                {
                    return StatusCode(500, new { error = "Failed to stop virtual network" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop virtual network");
                return StatusCode(500, new { error = "Failed to stop virtual network" });
            }
        }

        /// <summary>
        /// Remove a virtual host
        /// </summary>
        [HttpDelete("hosts/{hostId}")]
        public async Task<IActionResult> RemoveHost(string hostId)
        {
            try
            {
                var success = await _networkManager.RemoveHostAsync(hostId);
                if (success)
                {
                    return Ok(new { message = $"Host {hostId} removed successfully" });
                }
                else
                {
                    return NotFound(new { error = $"Host {hostId} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove host {HostId}", hostId);
                return StatusCode(500, new { error = "Failed to remove host" });
            }
        }

        /// <summary>
        /// Get subnet information
        /// </summary>
        [HttpGet("subnets")]
        public ActionResult<IEnumerable<SubnetDto>> GetSubnets()
        {
            try
            {
                // For now, return the default subnet information
                var subnets = new List<SubnetDto>
                {
                    new SubnetDto
                    {
                        Name = "default",
                        Cidr = "192.168.1.0/24",
                        Gateway = "192.168.1.1",
                        TotalAddresses = 254,
                        UsedAddresses = _networkManager.GetAllHosts().Count(),
                        AvailableAddresses = 254 - _networkManager.GetAllHosts().Count()
                    }
                };

                return Ok(subnets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get subnet information");
                return StatusCode(500, new { error = "Failed to get subnet information" });
            }
        }

        /// <summary>
        /// Save network configuration
        /// </summary>
        [HttpPost("save")]
        public async Task<IActionResult> SaveConfiguration([FromBody] SaveConfigurationRequest request)
        {
            try
            {
                var success = await _networkManager.SaveConfigurationAsync(request.FilePath);
                if (success)
                {
                    return Ok(new { message = "Configuration saved successfully" });
                }
                else
                {
                    return StatusCode(500, new { error = "Failed to save configuration" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save configuration");
                return StatusCode(500, new { error = "Failed to save configuration" });
            }
        }

        /// <summary>
        /// Load network configuration
        /// </summary>
        [HttpPost("load")]
        public async Task<IActionResult> LoadConfiguration([FromBody] LoadConfigurationRequest request)
        {
            try
            {
                var success = await _networkManager.LoadConfigurationAsync(request.FilePath);
                if (success)
                {
                    return Ok(new { message = "Configuration loaded successfully" });
                }
                else
                {
                    return StatusCode(500, new { error = "Failed to load configuration" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load configuration");
                return StatusCode(500, new { error = "Failed to load configuration" });
            }
        }
    }

    // DTOs for API communication
    public class VirtualHostDto
    {
        public string Id { get; set; }
        public string Hostname { get; set; }
        public string IpAddress { get; set; }
        public string SubnetName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Vendor { get; set; }
    }

    public class SubnetDto
    {
        public string Name { get; set; }
        public string Cidr { get; set; }
        public string Gateway { get; set; }
        public int TotalAddresses { get; set; }
        public int UsedAddresses { get; set; }
        public int AvailableAddresses { get; set; }
    }

    public class CreateHostRequest
    {
        public string Hostname { get; set; }
        public string Vendor { get; set; } = "cisco";
        public string SubnetName { get; set; } = "default";
    }

    public class SaveConfigurationRequest
    {
        public string FilePath { get; set; }
    }

    public class LoadConfigurationRequest
    {
        public string FilePath { get; set; }
    }
}