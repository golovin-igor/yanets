using Yanets.Core.Models;

namespace Yanets.Application.Services
{
    /// <summary>
    /// Interface for topology management services
    /// </summary>
    public interface ITopologyService
    {
        /// <summary>
        /// Gets a topology by ID
        /// </summary>
        NetworkTopology? GetTopology(Guid id);

        /// <summary>
        /// Gets all topologies
        /// </summary>
        IEnumerable<NetworkTopology> GetAllTopologies();

        /// <summary>
        /// Saves a topology
        /// </summary>
        void SaveTopology(NetworkTopology topology);

        /// <summary>
        /// Deletes a topology
        /// </summary>
        void DeleteTopology(Guid id);

        /// <summary>
        /// Starts simulation for all devices in a topology
        /// </summary>
        Task StartTopologySimulationAsync(Guid topologyId);

        /// <summary>
        /// Stops simulation for all devices in a topology
        /// </summary>
        Task StopTopologySimulationAsync(Guid topologyId);

        /// <summary>
        /// Gets simulation status for a topology
        /// </summary>
        bool IsTopologyRunning(Guid topologyId);
    }
}
