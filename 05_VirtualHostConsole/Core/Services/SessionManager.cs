using Microsoft.Extensions.Logging;
using Yanets.VirtualHostConsole.Core.Interfaces;
using Yanets.VirtualHostConsole.Core.Models;

namespace Yanets.VirtualHostConsole.Core.Services
{
    public class SessionManager : ISessionManager
    {
        private readonly Dictionary<string, CliSession> _activeSessions;
        private readonly ILogger<SessionManager> _logger;
        private readonly Timer _cleanupTimer;

        public SessionManager(ILogger<SessionManager> logger)
        {
            _activeSessions = new Dictionary<string, CliSession>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Start cleanup timer (runs every 5 minutes)
            _cleanupTimer = new Timer(CleanupExpiredSessions, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public CliSession CreateSession(IVirtualHost host, EndPoint remoteEndPoint)
        {
            try
            {
                var session = new CliSession
                {
                    HostId = host.Id,
                    Host = host,
                    RemoteEndPoint = remoteEndPoint,
                    IsAuthenticated = false,
                    PrivilegeLevel = 0,
                    CurrentMode = CliMode.UserExec
                };

                _activeSessions[session.SessionId] = session;

                _logger.LogInformation("Created session {SessionId} for host {HostId}", session.SessionId, host.Id);

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create session for host {HostId}", host.Id);
                throw;
            }
        }

        public CliSession GetSession(string sessionId)
        {
            return _activeSessions.TryGetValue(sessionId, out var session) ? session : null;
        }

        public void RemoveSession(string sessionId)
        {
            if (_activeSessions.Remove(sessionId))
            {
                _logger.LogInformation("Removed session {SessionId}", sessionId);
            }
        }

        public void UpdateSessionActivity(string sessionId)
        {
            if (_activeSessions.TryGetValue(sessionId, out var session))
            {
                session.UpdateActivity();
            }
        }

        public bool IsSessionExpired(string sessionId)
        {
            return !_activeSessions.TryGetValue(sessionId, out var session) || session.IsExpired();
        }

        public IEnumerable<CliSession> GetActiveSessions()
        {
            return _activeSessions.Values.Where(s => !s.IsExpired());
        }

        public int GetActiveSessionCount()
        {
            return GetActiveSessions().Count();
        }

        public void CleanupExpiredSessions()
        {
            try
            {
                var expiredSessions = _activeSessions.Where(kvp => kvp.Value.IsExpired()).ToList();

                foreach (var (sessionId, _) in expiredSessions)
                {
                    _activeSessions.Remove(sessionId);
                }

                if (expiredSessions.Any())
                {
                    _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session cleanup");
            }
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _activeSessions.Clear();
        }
    }
}