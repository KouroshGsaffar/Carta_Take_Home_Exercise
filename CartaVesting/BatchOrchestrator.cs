using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CartaVesting.Models;
using CartaVesting.Services;

namespace CartaVesting
{
    public class BatchOrchestrator
    {
        private readonly ICsvParser _parser;
        private readonly IVestingProcessor _processor;
        private readonly IPrecisionService _precision;

        public BatchOrchestrator(ICsvParser parser, IVestingProcessor processor, IPrecisionService precision)
        {
            _parser = parser;
            _processor = processor;
            _precision = precision;
        }

        public async Task<List<string>> RunAsync(string filePath, DateTime targetDate, int precision)
        {
            var state = new Dictionary<AwardKey, AwardSummary>();
            var outputLines = new List<string>(); // List to hold results

            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var vestEvent = _parser.ParseLine(line, precision);
                if (vestEvent == null) continue;

                _processor.ProcessEvent(vestEvent, state, targetDate);
            }

            var sortedResults = state
                .Select(kvp => new 
                {
                    EmpId = kvp.Key.EmployeeId,
                    EmpName = kvp.Value.EmployeeName,
                    AwardId = kvp.Key.AwardId,
                    Total = kvp.Value.TotalVested
                })
                .OrderBy(x => x.EmpId)
                .ThenBy(x => x.AwardId);

            foreach (var item in sortedResults)
            {
                var finalAmount = _precision.Truncate(item.Total, precision);
                
                outputLines.Add($"{item.EmpId},{item.EmpName},{item.AwardId},{finalAmount.ToString($"F{precision}")}");
            }
            
            return outputLines;
        }
    }
}