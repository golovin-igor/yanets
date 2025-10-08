using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Core.Interfaces
{
    public interface ISessionManager
    {
        CliSession CreateSession(IVirtualHost host, EndPoint remoteEndPoint);
        CliSession GetSession(string sessionId);
        void RemoveSession(string sessionId);
        void UpdateSessionActivity(string sessionId);
        bool IsSessionExpired(string sessionId);
        IEnumerable<CliSession> GetActiveSessions();
        int GetActiveSessionCount();
        void CleanupExpiredSessions();
    }
}