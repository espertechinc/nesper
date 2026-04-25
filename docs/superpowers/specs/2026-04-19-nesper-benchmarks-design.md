# NEsper Performance Benchmark Suite — Design Spec

**Date**: 2026-04-19  
**Branch**: performance-respec  
**Status**: Approved

---

## Purpose

Provide two complementary benchmark projects that serve:

1. **Throughput characterization** — absolute events/sec, ns/op, and allocs/op numbers for documentation and release notes
2. **Before/after validation** — per-finding measurement to confirm each PERF_REVIEW remediation delivers its expected gain

---

## Project Structure

Two console app projects under `tst/`, added to both `NEsper.sln` and `NEsperAll.sln`:

```
tst/
  NEsper.Benchmarks.Micro/
    NEsper.Benchmarks.Micro.csproj
    Program.cs
    Locks/
      TrackedDisposableBenchmark.cs        ← H1
      SlimReaderWriterLockBenchmark.cs     ← M1
    Collections/
      LinkedHashMapBenchmark.cs            ← H2
    FilterIndex/
      DoubleRangeBenchmark.cs              ← H3
      FilterServiceBaseEvalBenchmark.cs    ← H4
      OrderedListDictionaryViewBenchmark.cs ← H5
      StringRangeBenchmark.cs              ← H6
      BoxingBenchmark.cs                   ← M4
    Scheduling/
      SchedulingServiceBenchmark.cs        ← M2, M3
    EventService/
      ThreadLocalBenchmark.cs              ← L5
    Compat/
      AtomicLongBenchmark.cs               ← L2
    Instrumentation/
      InstrumentationHelperBenchmark.cs    ← L4

  NEsper.Benchmarks.EndToEnd/
    NEsper.Benchmarks.EndToEnd.csproj
    Program.cs
    Passthrough/
      PassthroughBenchmark.cs              ← baseline
    Filter/
      EqualityFilterBenchmark.cs           ← where Price = 100.0
      ComparisonFilterBenchmark.cs         ← where Price > 100.0
      RangeFilterBenchmark.cs              ← where Price between 90.0 and 110.0
      BooleanExprFilterBenchmark.cs        ← where Price > 90 and Volume > 500
      StringFilterBenchmark.cs             ← string equality and range
    Window/
      TimeWindowBenchmark.cs               ← win:time(5 sec)
      LengthWindowBenchmark.cs             ← win:length(1000)
    Aggregation/
      AggregationBenchmark.cs              ← avg/sum/count over time window
    Join/
      TwoStreamJoinBenchmark.cs            ← TradeEvent join QuoteEvent on Symbol
    Pattern/
      SequencePatternBenchmark.cs          ← every (a -> b(Price > a.Price))
    Deploy/
      DeployUndeployBenchmark.cs           ← concurrent deploy/undeploy under load (M5)
```

---

## Framework

**BenchmarkDotNet** — industry standard for .NET microbenchmarks. Produces ns/op, MB/s, and allocs/op. Runs via `dotnet run -c Release`.

Both projects are `OutputType=Exe` console apps (not NUnit test projects).

---

## BDN Configuration

Every benchmark class carries:

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
```

- **Micro project**: default BDN iteration counts for fast per-fix feedback
- **End-to-end project**: `[SimpleJob(warmupCount: 3, iterationCount: 10)]` for stable throughput numbers

---

## Event Model

A `TradeEvent` POCO defined in each project (no shared helper project needed):

```csharp
public class TradeEvent {
    public string Symbol    { get; set; }
    public double Price     { get; set; }
    public long   Volume    { get; set; }
    public long   Timestamp { get; set; }
}
```

The join benchmark additionally uses a `QuoteEvent`:

```csharp
public class QuoteEvent {
    public string Symbol   { get; set; }
    public double BidPrice { get; set; }
    public double AskPrice { get; set; }
}
```

---

## Benchmark Parameters

| Context | Parameter | Values |
|---------|-----------|--------|
| End-to-end | Events per iteration | `[Params(1_000, 10_000)]` |
| Filter end-to-end | Concurrent deployed statements | `[Params(1, 10, 100)]` |
| Microbenchmarks | Per-benchmark where finding is scale-sensitive | Defined per file |

---

## Microbenchmark Inventory

Each benchmark isolates the allocation or overhead identified in PERF_REVIEW. The primary metric is noted.

| File | PERF_REVIEW ID | Measures | Primary Metric |
|------|---------------|----------|----------------|
| `TrackedDisposableBenchmark` | H1 | `using (lock.Acquire())` heap alloc per acquire | Allocated/op |
| `SlimReaderWriterLockBenchmark` | M1 | `SupportsRecursion` vs `NoRecursion` acquire latency | ns/op |
| `LinkedHashMapBenchmark` | H2 | `foreach` LINQ enumerator vs struct enumerator | Allocated/op |
| `DoubleRangeBenchmark` | H3 | Two `new DoubleRange(...)` per range query vs struct | Allocated/op |
| `FilterServiceBaseEvalBenchmark` | H4 | `new ArrayDeque<FilterHandle>()` per single-stmt eval | Allocated/op |
| `OrderedListDictionaryViewBenchmark` | H5 | `Head()`/`Tail()`/`Between()` view alloc vs inline | Allocated/op |
| `StringRangeBenchmark` | H6 | Two `new StringRange(...)` per range query vs struct | Allocated/op |
| `BoxingBenchmark` | M4 | `object`-keyed dict lookup for double/int vs typed | ns/op + Allocated/op |
| `SchedulingServiceBenchmark` | M2, M3 | `new List<long>()` per tick + `Last()` O(n) scan | Allocated/op + ns/op |
| `ThreadLocalBenchmark` | L5 | 10x `GetOrCreate()` vs 1x cached per dispatch | ns/op |
| `AtomicLongBenchmark` | L2 | CAS spin loop vs `Interlocked.Increment - 1` | ns/op |
| `InstrumentationHelperBenchmark` | L4 | Property getter vs `readonly`/`const` dead-branch elim | ns/op |

---

## End-to-End Benchmark Inventory

Each benchmark compiles + deploys an EPL statement in `[GlobalSetup]`, then sends N events in the iteration body. Reports events/sec.

| File | EPL Pattern | Characterizes |
|------|-------------|---------------|
| `PassthroughBenchmark` | `select * from TradeEvent` | Baseline dispatch overhead |
| `EqualityFilterBenchmark` | `where Price = 100.0` | Equality filter index (M4) |
| `ComparisonFilterBenchmark` | `where Price > 100.0` | Comparison filter index (H5) |
| `RangeFilterBenchmark` | `where Price between 90.0 and 110.0` | Range filter index (H3, H5, H6) |
| `BooleanExprFilterBenchmark` | `where Price > 90 and Volume > 500` | Boolean expression evaluator (H2) |
| `StringFilterBenchmark` | `where Symbol = 'MSFT'` / `Symbol between 'A' and 'M'` | String equality + range (H6) |
| `TimeWindowBenchmark` | `select * from TradeEvent#time(5 sec)` | Time-window eviction (M2, M3) |
| `LengthWindowBenchmark` | `select * from TradeEvent#length(1000)` | Length-window eviction |
| `AggregationBenchmark` | `select avg(Price), sum(Volume) from TradeEvent#time(5 sec)` | Aggregation pipeline |
| `TwoStreamJoinBenchmark` | Join `TradeEvent` + `QuoteEvent` on Symbol | Join evaluation |
| `SequencePatternBenchmark` | `every (a=TradeEvent -> b=TradeEvent(Price > a.Price))` | Pattern detection |
| `DeployUndeployBenchmark` | Concurrent deploy/undeploy while events flow | Lock contention under churn (M5) |

---

## Running the Benchmarks

```bash
# All microbenchmarks
dotnet run -c Release --project tst/NEsper.Benchmarks.Micro -- --filter *

# Single finding
dotnet run -c Release --project tst/NEsper.Benchmarks.Micro -- --filter *TrackedDisposable*

# All end-to-end
dotnet run -c Release --project tst/NEsper.Benchmarks.EndToEnd -- --filter *

# Export for before/after comparison
dotnet run -c Release --project tst/NEsper.Benchmarks.EndToEnd -- --filter * --exporters json --artifacts ./BenchmarkResults
```

---

## Before/After Validation Workflow

1. On `master` (or before applying a fix): run the relevant benchmark, save JSON artifacts
2. Apply the fix branch
3. Run the same benchmark, save JSON artifacts to a second folder
4. Diff the JSON exports — `allocs/op` and `ns/op` deltas are the validation signal
5. BDN's `--join` flag merges multiple result sets into a single comparison table

---

## Output Artifacts

- BDN default output: `BenchmarkDotNet.Artifacts/` — gitignored
- Before/after exports: `BenchmarkResults/` — gitignored, saved manually per comparison run
