using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using Yanets.Core.Models;
using Yanets.Core.Interfaces;
using Yanets.Core.Vendors;
using Yanets.Application.Services;
using Yanets.Application.Services.Vendors;
using Yanets.WebUI.Services;
using Yanets.WebUI.Vendors;
using Yanets.SharedKernel;

namespace Yanets.WebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopologyController : ControllerBase
    {
        private readonly Yanets.Application.Services.ITopologyService _topologyService;

        public TopologyController(Yanets.Application.Services.ITopologyService topologyService)
        {
            _topologyService = topologyService;
        }

        /// <summary>
        /// Get all network topologies
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<NetworkTopology>> GetTopologies()
        {
            var topologies = _topologyService.GetAllTopologies();
            return Ok(topologies);
        }

        /// <summary>
        /// Get a specific topology by ID
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<NetworkTopology> GetTopology(Guid id)
        {
            var topology = _topologyService.GetTopology(id);
            if (topology == null)
                return NotFound();

            return Ok(topology);
        }

        /// <summary>
        /// Create a new topology
        /// </summary>
        [HttpPost]
        public ActionResult<NetworkTopology> CreateTopology([FromBody] CreateTopologyRequest request)
        {
            // Validate the request
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    errors = validationResults.Select(v => new { field = v.MemberNames.FirstOrDefault(), message = v.ErrorMessage })
                });
            }

            var topology = new NetworkTopology
            {
                Name = request.Name,
                Description = request.Description
            };

            _topologyService.SaveTopology(topology);
            return CreatedAtAction(nameof(GetTopology), new { id = topology.Id }, topology);
        }

        /// <summary>
        /// Update an existing topology
        /// </summary>
        [HttpPut("{id}")]
        public ActionResult UpdateTopology(Guid id, [FromBody] UpdateTopologyRequest request)
        {
            var topology = _topologyService.GetTopology(id);
            if (topology == null)
                return NotFound();

            topology.Name = request.Name;
            topology.Description = request.Description;

            _topologyService.SaveTopology(topology);
            return NoContent();
        }

        /// <summary>
        /// Delete a topology
        /// </summary>
        [HttpDelete("{id}")]
        public ActionResult DeleteTopology(Guid id)
        {
            var topology = _topologyService.GetTopology(id);
            if (topology == null)
                return NotFound();

            _topologyService.DeleteTopology(id);
            return NoContent();
        }

        /// <summary>
        /// Add a device to a topology
        /// </summary>
        [HttpPost("{topologyId}/devices")]
        public ActionResult<NetworkDevice> AddDevice(Guid topologyId, [FromBody] CreateDeviceRequest request)
        {
            var topology = _topologyService.GetTopology(topologyId);
            if (topology == null)
                return NotFound();

            var device = new RouterDevice
            {
                Name = request.Name,
                Hostname = request.Hostname,
                Position = new Point(request.PositionX, request.PositionY)
            };

            // Set vendor after creation to avoid conflicts
            device.Vendor = CreateVendorProfile(request.VendorName);

            topology.AddDevice(device);
            _topologyService.SaveTopology(topology);

            return CreatedAtAction(nameof(GetTopology), new { id = topologyId }, device);
        }

        /// <summary>
        /// Get all devices in a topology
        /// </summary>
        [HttpGet("{topologyId}/devices")]
        public ActionResult<IEnumerable<NetworkDevice>> GetDevices(Guid topologyId)
        {
            var topology = _topologyService.GetTopology(topologyId);
            if (topology == null)
                return NotFound();

            return Ok(topology.Devices);
        }

        private VendorProfile CreateVendorProfile(string vendorName)
        {
            return vendorName.ToLower() switch
            {
                "cisco" => new Application.Services.Vendors.CiscoIosVendorProfile(),
                "juniper" => new Application.Services.Vendors.JuniperJunosVendorProfile(),
                _ => new Application.Services.Vendors.CiscoIosVendorProfile() // Default to Cisco
            };
        }
    }

    // Router implementation for testing
    public class RouterDevice : NetworkDevice
    {
        public RouterDevice() : base(DeviceType.Router, "Cisco")
        {
            // Vendor will be set later to avoid conflicts
        }

        public override bool IsValid() => base.IsValid();
    }

    // Request/Response models
    public class CreateTopologyRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateTopologyRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CreateDeviceRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public string VendorName { get; set; } = "Cisco";
        public int PositionX { get; set; }
        public int PositionY { get; set; }
    }
}
