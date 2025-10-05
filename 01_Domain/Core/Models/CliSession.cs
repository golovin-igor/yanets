using Yanets.SharedKernel;

namespace Yanets.Core.Models
{
    /// <summary>
    /// Represents a CLI session with a network device
    /// </summary>
    public class CliSession
    {
        public NetworkDevice Device { get; set; } = null!;
        public bool IsAuthenticated { get; set; }
        public int PrivilegeLevel { get; set; }
        public CliMode CurrentMode { get; set; } = CliMode.UserExec;
        public Stack<CliMode> ModeStack { get; set; } = new();
        public Dictionary<string, object> SessionVariables { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastActivity { get; set; } = DateTime.Now;
        public string ClientAddress { get; set; } = string.Empty;

        /// <summary>
        /// Updates the last activity timestamp
        /// </summary>
        public void UpdateActivity()
        {
            LastActivity = DateTime.Now;
        }

        /// <summary>
        /// Gets the session duration
        /// </summary>
        public TimeSpan Duration => DateTime.Now - CreatedAt;

        /// <summary>
        /// Gets the idle time since last activity
        /// </summary>
        public TimeSpan IdleTime => DateTime.Now - LastActivity;

        /// <summary>
        /// Pushes a new mode onto the mode stack
        /// </summary>
        public void PushMode(CliMode mode)
        {
            ModeStack.Push(CurrentMode);
            CurrentMode = mode;
        }

        /// <summary>
        /// Pops a mode from the mode stack
        /// </summary>
        public bool PopMode()
        {
            if (ModeStack.Count == 0)
                return false;

            CurrentMode = ModeStack.Pop();
            return true;
        }

        /// <summary>
        /// Gets a session variable
        /// </summary>
        public object? GetVariable(string key)
        {
            return SessionVariables.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Sets a session variable
        /// </summary>
        public void SetVariable(string key, object value)
        {
            SessionVariables[key] = value;
        }

        /// <summary>
        /// Gets a strongly-typed session variable
        /// </summary>
        public T? GetVariable<T>(string key) where T : class
        {
            var value = GetVariable(key);
            return value as T;
        }

        /// <summary>
        /// Checks if the session has sufficient privilege level for a command
        /// </summary>
        public bool HasPrivilege(int requiredLevel)
        {
            return PrivilegeLevel >= requiredLevel;
        }

        /// <summary>
        /// Elevates privilege level if possible
        /// </summary>
        public bool ElevatePrivilege(int newLevel)
        {
            if (newLevel <= PrivilegeLevel)
                return false;

            PrivilegeLevel = newLevel;
            return true;
        }

        /// <summary>
        /// Resets the session to initial state
        /// </summary>
        public void Reset()
        {
            IsAuthenticated = false;
            PrivilegeLevel = 0;
            CurrentMode = CliMode.UserExec;
            ModeStack.Clear();
            SessionVariables.Clear();
            LastActivity = DateTime.Now;
        }

        /// <summary>
        /// Validates the session state
        /// </summary>
        public bool IsValid()
        {
            return Device != null &&
                   CreatedAt <= DateTime.Now &&
                   LastActivity <= DateTime.Now;
        }
    }
}
