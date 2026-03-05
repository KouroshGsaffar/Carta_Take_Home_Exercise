using System;
using CartaVesting.Models;

namespace CartaVesting.Services
{
    public interface ICsvParser
    {
        VestingEvent? ParseLine(string line, int precision);
    }

    public class CsvParser : ICsvParser
    {
        private readonly IPrecisionService _precisionService;

        public CsvParser(IPrecisionService precisionService)
        {
            _precisionService = precisionService;
        }

        public VestingEvent? ParseLine(string line, int precision)
        {
            var parts = line.Split(',');
            if (parts.Length < 6) return null; // Skip malformed lines

            if (!Enum.TryParse<EventType>(parts[0].Trim(), true, out var type))
            {
                // If the type isn't VEST or CANCEL, we skip this line
                return null;
            }
            var empId = parts[1].Trim();
            var empName = parts[2].Trim();
            var awardId = parts[3].Trim();

            if (!DateTime.TryParse(parts[4].Trim(), out var date))
                return null; // Skip invalid dates

            if (!decimal.TryParse(parts[5].Trim(), out var rawQty))
                return null; // Skip invalid quantities

            // Stage 3: Truncate input immediately 
            var qty = _precisionService.Truncate(rawQty, precision);

            return new VestingEvent(type, empId, empName, awardId, date, qty);
        }
    }
}