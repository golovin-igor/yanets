using Yanets.VirtualHostConsole.Core.Interfaces;

namespace Yanets.VirtualHostConsole.Core.Services
{
    public class CommandParser : ICommandParser
    {
        public async Task ParseAndExecuteAsync(string commandLine, IVirtualHost host, CliSession session)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                return;

            try
            {
                // Basic command parsing - in a full implementation this would be more sophisticated
                var result = await host.ExecuteCommandAsync(commandLine, session);

                if (!result.Success)
                {
                    Console.WriteLine(result.ErrorMessage);
                }
                else
                {
                    Console.Write(result.Output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Command execution error: {ex.Message}");
            }
        }
    }
}