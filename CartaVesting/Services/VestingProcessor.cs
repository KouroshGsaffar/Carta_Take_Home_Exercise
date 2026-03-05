using System;
using System.Collections.Generic;
using CartaVesting.Models;

namespace CartaVesting.Services
{
    public interface IVestingProcessor
    {
        void ProcessEvent(VestingEvent evt, Dictionary<AwardKey, AwardSummary> state, DateTime targetDate);
    }

    public class VestingProcessor : IVestingProcessor
    {
        public void ProcessEvent(VestingEvent evt, Dictionary<AwardKey, AwardSummary> state, DateTime targetDate)
        {
            var key = new AwardKey(evt.EmployeeId, evt.AwardId);

            if (!state.ContainsKey(key))
            {
                state[key] = new AwardSummary 
                { 
                    EmployeeName = evt.EmployeeName, 
                    TotalVested = 0 
                };
            }

            // Ignore future events for calculation, but they are already registered above
            if (evt.Date > targetDate) return;

            var summary = state[key];

            switch (evt.Type)
            {
                case EventType.VEST:
                    summary.TotalVested += evt.Quantity;
                    break;

                case EventType.CANCEL:
                    // Stage 2: Cancelled shares cannot exceed vested shares
                    if (evt.Quantity <= summary.TotalVested)
                    {
                        summary.TotalVested -= evt.Quantity;
                    }
                    break;
            }
        }
    }
}