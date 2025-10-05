using Yanets.Core.Interfaces;
using Yanets.Core.Models;

namespace Yanets.WebUI.Services
{
    public interface ITopologyService
    {
        NetworkTopology? GetTopology(Guid id);
        IEnumerable<NetworkTopology> GetAllTopologies();
        void SaveTopology(NetworkTopology topology);
        void DeleteTopology(Guid id);
    }

    public class TopologyService : ITopologyService
    {
        private readonly List<NetworkTopology> _topologies = new();

        public NetworkTopology? GetTopology(Guid id)
        {
            return _topologies.FirstOrDefault(t => t.Id == id);
        }

        public IEnumerable<NetworkTopology> GetAllTopologies()
        {
            return _topologies;
        }

        public void SaveTopology(NetworkTopology topology)
        {
            var existing = _topologies.FirstOrDefault(t => t.Id == topology.Id);
            if (existing != null)
            {
                _topologies.Remove(existing);
            }
            _topologies.Add(topology);
        }

        public void DeleteTopology(Guid id)
        {
            var topology = _topologies.FirstOrDefault(t => t.Id == id);
            if (topology != null)
            {
                _topologies.Remove(topology);
            }
        }
    }
}
