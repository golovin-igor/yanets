using Yanets.WebUI.VirtualNetwork.Models;

namespace Yanets.WebUI.VirtualNetwork
{
    public interface IProtocolHandler : IDisposable
    {
        ProtocolType Type { get; }
        IVirtualHost Host { get; }
        Task HandleConnectionAsync(NetworkStream stream);
        Task<byte[]> ProcessDataAsync(byte[] data);
        Task StartListeningAsync(int port);
        Task StopListeningAsync();
        event EventHandler<DataReceivedEventArgs> DataReceived;
    }
}