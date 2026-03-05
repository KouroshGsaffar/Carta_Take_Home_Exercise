# Vesting Calculator (KS-CARTA)

A robust, scalable command-line interface (CLI) application that calculates cumulative vesting schedules from CSV event streams.

## Project Overview

This solution implements a **Batch Processing Pipeline** to handle vesting events. It is designed to be memory-efficient, testable, and strictly adheres to the provided constraints.

### Features
* **Stage 1:** Calculates basic vesting totals based on a target date.
* **Stage 2:** Handles cancellation events, ensuring validation against running totals.
* **Stage 3:** Specific precision handling (0-6 decimal places) with truncation logic.
* **Architecture:** Uses Dependency Injection (DI) and a streaming pipeline to handle potentially large datasets.

---

## Project Structure

```
KS-CARTA/
├── CartaVesting/                  # Main Application
│   ├── Models/                    # Data Structures (VestingEvent, AwardSummary)
│   ├── Services/                  # Business Logic (Parser, Processor, Precision)
│   ├── BatchOrchestrator.cs       # Pipeline Logic
│   └── Program.cs                 # Entry Point & DI Setup
├── CartaVesting.Tests/            # Test Project
│   ├── Data/                      # Integration Test Input Files
│   ├── Results/                   # Expected Output Files
│   ├── IntegrationTests.cs        # End-to-End Tests
│   └── VestingTests.cs            # Unit Tests
└── KS-CARTA.sln                   # Solution File

```

---

## Build and Run Instructions

### Prerequisites
* .NET 10.0 SDK (or newer)
* Command line terminal (Bash, PowerShell, or CMD)

### 1. Building the Application
Navigate to the root directory and run:

```bash
dotnet build

```

### 2. Running the Tests

This project includes Unit Tests (logic validation) and Integration Tests (end-to-end file processing).

```bash
dotnet test

```

### 3. Running the Application

You can run the application directly using `dotnet run`. The arguments are:


`./vesting_program <filename> <target_date> [precision]` 

**Usage Syntax:**

```bash
dotnet run --project CartaVesting <file_location> <target_date> [precision]

```

**Examples:**

```bash
# Basic usage (Default precision 0)
dotnet run --project CartaVesting ./CartaVesting.Tests/Data/example1.csv 2020-04-01

# With Precision (2 decimal places)
dotnet run --project CartaVesting ./CartaVesting.Tests/Data/example3.csv 2021-02-01 2

```

*(Note: The `--project` flag is only needed if running from the root solution folder. If inside the `CartaVesting` folder, just use `dotnet run ...`)*

---

## Design Decisions 

### 1. Pipeline Architecture (ETL Pattern)

To ensure the application can "handle large data sets robustly", I implemented a **Streaming Pipeline** architecture rather than loading the entire file into memory.

* **Reader:** Streams the CSV file line-by-line using `File.ReadLines` (`IEnumerable<string>`).
* **Parser:** Transforms lines into strong-typed `VestingEvent` objects on the fly.
* **Processor:** Accumulates state in a memory-efficient Dictionary.
* **Writer:** Aggregates and sorts the final output only after processing is complete.

This approach prevents `OutOfMemoryException` errors on extremely large input files.

### 2. Dependency Injection & Singleton Pattern

I used `Microsoft.Extensions.DependencyInjection` to manage services.

* **Why:** It decouples the components (`ICsvParser`, `IVestingProcessor`, `IPrecisionService`), allowing for easier unit testing and adherence to the **Single Responsibility Principle**.


* **Services:** All stateless services are registered as **Singletons**.

### 3. Precision Strategy (Double Truncation)

Floating-point math can introduce artifacts (e.g., `0.3000000004`). To satisfy Stage 3 requirements:

* I truncate input values *immediately* upon parsing.
* I truncate output values *immediately* before printing.
* I avoided standard rounding and implemented a custom `Truncate` method: `Math.Floor(value * power) / power`.

### 4. Cancellation Logic

* **Validation:** Cancellation events are validated against the cumulative vested total *at the time of processing*.
* 
**Invalid Events:** Per requirements, if a cancellation request exceeds the currently vested amount, the event is deemed invalid and ignored.



---

## Assumptions Made 

1. **Strict Date Parsing:** The application assumes dates are in `YYYY-MM-DD` format. Invalid dates cause the line to be skipped.
2. **Chronological Data:** While the code handles unsorted inputs by tracking state, it assumes the file is roughly chronological for the purpose of validating "Cancellation" events against "Running Totals."
3. **Future Events:** Employees with *only* future vesting events are still included in the output with `0` vested shares, as per the requirement to "Include all employees... even if 0 shares vested".
4. **Precision Limits:** Precision is strictly enforced between 0 and 6. Values outside this range throw a validation error.

---

## LLM Usage Declaration
Per the submission requirements, I utilized a Large Language Model (Gemini) to assist with the following:
* **System Design:** Validating the batch processing architecture and schematic.
* **Boilerplate Code:** Generating the initial project structure, Dependency Injection setup, and standard `using` directives.
* **Testing Strategy:** Generating scenarios for NUnit tests, specifically for edge cases in precision (Stage 3) and cancellation logic (Stage 2).
* **Documentation:** Drafting and refining this README file to ensure clarity and professional formatting.

---

## Time Constraints & Future Improvements

Per the requirement to note "what you would change with more time" , the current implementation uses a **Streaming (Line-by-Line)** approach. While this is memory-efficient and robust for the current requirements, I would expand this in a production environment to align closer with the "Batch/Chunking" architecture diagram I initially designed.

With more time, I would implement the following:

### 1. True Batch/Chunk Processing
* **Current:** Reads and processes one line at a time (`IEnumerable<string>`).
* **Future:** Utilize `.Chunk(batchSize)` to process records in micro-batches (e.g., 1,000 rows).
* **Benefit:** This prepares the system for high-throughput scenarios, such as bulk database inserts (reducing network round-trips) or batch API calls, which are common in enterprise ETL pipelines.

### 2. Parallel Processing
* **Current:** Single-threaded sequential processing.
* **Future:** With chunking implemented, I would use `Parallel.ForEach` to process chunks concurrently.
* **Benefit:** This would significantly reduce runtime on multi-core machines for massive datasets (e.g., millions of rows). It would require converting the `Dictionary` state to a `ConcurrentDictionary` or implementing a "Map-Reduce" pattern to safely aggregate results.

### 3. Observability & Metrics
* **Current:** Console output only.
* **Future:** Integrate OpenTelemetry or a simple metrics counter (Events Per Second) in the "Job Listener" component.
* **Benefit:** Provides real-time visibility into the health and performance of the batch job.