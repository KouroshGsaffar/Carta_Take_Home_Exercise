using NUnit.Framework;
using CartaVesting.Services;
using CartaVesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;

namespace CartaVesting.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private BatchOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            var precisionService = new PrecisionService();
            var parser = new CsvParser(precisionService);
            var processor = new VestingProcessor();
            _orchestrator = new BatchOrchestrator(parser, processor, precisionService);
        }

        [TestCase("example1.csv", "example1_out.txt", "2020-04-01", 0)]
        [TestCase("example2.csv", "example2_out.txt", "2021-02-01", 0)]
        [TestCase("example3.csv", "example3_out.txt", "2021-02-01", 1)]
        public async Task RunEndToEndTest(string inputFile, string expectedFile, string dateStr, int precision)
        {
            // Arrange
            var inputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", inputFile);
            var expectedPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Results", expectedFile);
            var targetDate = DateTime.Parse(dateStr);

            // Act
            var actualLines = await _orchestrator.RunAsync(inputPath, targetDate, precision);

            // Assert
            var expectedLines = await File.ReadAllLinesAsync(expectedPath);
            
            Assert.That(actualLines.Count, Is.EqualTo(expectedLines.Length), "Mismatch in number of output lines");

            for (int i = 0; i < expectedLines.Length; i++)
            {
                Assert.That(actualLines[i].Trim(), Is.EqualTo(expectedLines[i].Trim()), $"Mismatch at line {i+1}");
            }
        }
    }
}