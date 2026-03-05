using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using CartaVesting.Services;

namespace CartaVesting
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Setup DI Container (Singleton Principle)
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPrecisionService, PrecisionService>()
                .AddSingleton<ICsvParser, CsvParser>()
                .AddSingleton<IVestingProcessor, VestingProcessor>()
                .AddSingleton<BatchOrchestrator>()
                .BuildServiceProvider();

            // 2. Validate Arguments
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ./vesting_program <filename> <target_date> [precision]");
                return;
            }

            string filename = args[0];
            if (!DateTime.TryParse(args[1], out DateTime targetDate))
            {
                Console.WriteLine("Error: Invalid Target Date format (YYYY-MM-DD).");
                return;
            }

            int precision = 0; // Default is 0
            if (args.Length >= 3)
            {
                if (!int.TryParse(args[2], out precision))
                {
                    Console.WriteLine("Error: Precision must be an integer.");
                    return;
                }

                // VALIDATION REQUIREMENT: Precision must be between 0 and 6 
                if (precision < 0 || precision > 6)
                {
                    Console.WriteLine("Error: Precision must be between 0 and 6 (inclusive).");
                    return;
                }
            }

            // 3. Execute
            if (!System.IO.File.Exists(filename))
            {
                Console.WriteLine($"Error: File '{filename}' not found.");
                return;
            }

            var orchestrator = serviceProvider.GetService<BatchOrchestrator>();
            var results = await orchestrator.RunAsync(filename, targetDate, precision);

            foreach (var line in results)
            {
                Console.WriteLine(line);
            }
        }
    }
}