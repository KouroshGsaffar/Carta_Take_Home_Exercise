using NUnit.Framework;
using CartaVesting.Services;
using CartaVesting.Models;
using System.Collections.Generic;
using System;

namespace CartaVesting.Tests
{
    [TestFixture]
    public class VestingTests
    {
        [Test]
        public void PrecisionService_Truncates_WithoutRounding()
        {
            var service = new PrecisionService();
            Assert.That(service.Truncate(100.4567m, 2), Is.EqualTo(100.45m));
        }

        [Test]
        public void Processor_Subtracts_CancelledShares()
        {
            var processor = new VestingProcessor();
            var state = new Dictionary<AwardKey, AwardSummary>();
            var target = new DateTime(2022, 1, 1);

            var vest = new VestingEvent(EventType.VEST, "E1", "Alice", "ISO1", new DateTime(2021, 1, 1), 1000);
            processor.ProcessEvent(vest, state, target);

            var cancel = new VestingEvent(EventType.CANCEL, "E1", "Alice", "ISO1", new DateTime(2021, 6, 1), 700);
            processor.ProcessEvent(cancel, state, target);

            var key = new AwardKey("E1", "ISO1");
            
            Assert.That(state[key].TotalVested, Is.EqualTo(300));
        }

        [Test]
        public void Processor_Ignores_Events_After_TargetDate()
        {
            var processor = new VestingProcessor();
            var state = new Dictionary<AwardKey, AwardSummary>();
            var target = new DateTime(2020, 1, 1);

            var futureEvent = new VestingEvent(EventType.VEST, "E1", "Alice", "ISO1", new DateTime(2021, 1, 1), 1000);
            processor.ProcessEvent(futureEvent, state, target);

            var key = new AwardKey("E1", "ISO1");
            
            if (state.ContainsKey(key))
            {
                Assert.That(state[key].TotalVested, Is.EqualTo(0));
            }
            else
            {
                Assert.Pass(); 
            }
        }
    }
}