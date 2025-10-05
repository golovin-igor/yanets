using Yanets.Core.Models;

namespace Yanets.Core.Commands
{
    /// <summary>
    /// Represents the result of command execution
    /// </summary>
    public class CommandResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public Models.DeviceState? UpdatedState { get; set; }

        /// <summary>
        /// Creates a successful command result
        /// </summary>
        public static CommandResult CreateSuccess(string output = "")
        {
            return new CommandResult
            {
                Success = true,
                Output = output
            };
        }

        /// <summary>
        /// Creates an error command result
        /// </summary>
        public static CommandResult CreateError(string errorMessage)
        {
            return new CommandResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Creates a command result with state update
        /// </summary>
        public static CommandResult CreateWithStateUpdate(Models.DeviceState updatedState, string output = "")
        {
            return new CommandResult
            {
                Success = true,
                Output = output,
                UpdatedState = updatedState
            };
        }

        /// <summary>
        /// Validates the command result
        /// </summary>
        public bool IsValid()
        {
            // Either success with optional output, or error with error message
            if (Success)
            {
                return string.IsNullOrEmpty(ErrorMessage);
            }
            else
            {
                return !string.IsNullOrEmpty(ErrorMessage);
            }
        }

        /// <summary>
        /// Combines multiple command results
        /// </summary>
        public static CommandResult Combine(IEnumerable<CommandResult> results)
        {
            var combined = new CommandResult();
            var allSuccessful = true;
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            foreach (var result in results)
            {
                if (!result.Success)
                    allSuccessful = false;

                if (!string.IsNullOrEmpty(result.Output))
                    outputBuilder.AppendLine(result.Output);

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    errorBuilder.AppendLine(result.ErrorMessage);

                // Use the last updated state if any
                if (result.UpdatedState != null)
                    combined.UpdatedState = result.UpdatedState;
            }

            combined.Success = allSuccessful;
            combined.Output = outputBuilder.ToString().TrimEnd();
            combined.ErrorMessage = errorBuilder.ToString().TrimEnd();

            return combined;
        }

        /// <summary>
        /// Returns a string representation of the result
        /// </summary>
        public override string ToString()
        {
            if (Success)
            {
                return $"Success: {Output}";
            }
            else
            {
                return $"Error: {ErrorMessage}";
            }
        }
    }
}
