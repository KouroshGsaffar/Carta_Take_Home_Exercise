using System;

namespace CartaVesting.Models
{
    // Define the Enum
    public enum EventType
    {
        VEST,
        CANCEL
    }

    // Represents a single row from the CSV
    public record VestingEvent(
        EventType Type,          // VEST or CANCEL
        string EmployeeId, 
        string EmployeeName, 
        string AwardId, 
        DateTime Date, 
        decimal Quantity
    );

    // Composite key for aggregation
    public record AwardKey(string EmployeeId, string AwardId);

    // The mutable state for an employee's award
    public class AwardSummary
    {
        public string EmployeeName { get; set; } = string.Empty;
        public decimal TotalVested { get; set; }
    }
}