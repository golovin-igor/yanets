using System;
using Yanets.Application.Services;

namespace Yanets.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("YANETS Routing Commands Test");
            Console.WriteLine("============================");

            try
            {
                RoutingCommandsTest.TestRoutingCommands();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during testing: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
