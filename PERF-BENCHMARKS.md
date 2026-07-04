# NEsper Performance Benchmarks

NEsper has three performance testing systems, each serving a different purpose.

> **Critical:** BenchmarkDotNet benchmarks **must** run with `-c Release`. Running in Debug causes an immediate startup error.

---

## Quick Reference

```powershell
# All microbenchmarks
dotnet run --project tst\NEsper.Benchmarks.Micro\NEsper.Benchmarks.Micro.csproj -c Release

# All end-to-end benchmarks
dotnet run --project tst\NEsper.Benchmarks.EndToEnd\NEsper.Benchmarks.EndToEnd.csproj -c Release

# One specific benchmark class (works for either project)
dotnet run --project tst\NEsper.Benchmarks.EndToEnd\NEsper.Benchmarks.EndToEnd.csproj -c Release -- --filter "*Equality*"

# List available benchmarks without running
dotnet run --project tst\NEsper.Benchmarks.Micro\NEsper.Benchmarks.Micro.csproj -c Release -- --list flat

# Profiling harness — attach a profiler to this process
dotnet run --project examples\NEsper.Benchmark\NEsper.Benchmark.Perforator\NEsper.Benchmark.Perforator.csproj

# Distributed simulation: 2 threads × 10,000 events/s
dotnet run --project examples\NEsper.Benchmark\NEsper.Benchmark.Server\NEsper.Benchmark.Server.csproj -c Release -- -rate 2x10000
```

---

## 1. Microbenchmarks (`NEsper.Benchmarks.Micro`)

**Location:** `tst/NEsper.Benchmarks.Micro/`

Isolated component-level benchmarks for low-level infrastructure — locks, filter index data structures, scheduling, collections, and threading primitives. Each benchmark compares the current implementation against a proposed improvement so the gain is immediately visible in BenchmarkDotNet's baseline/ratio column.

### Benchmarks included

| File | What it measures |
|---|---|
| `Locks/SlimReaderWriterLockBenchmark.cs` | `SupportsRecursion` vs `NoRecursion` lock acquisition overhead |
| `Locks/TrackedDisposableBenchmark.cs` | Disposable tracking overhead |
| `FilterIndex/FilterServiceBaseEvalBenchmark.cs` | Per-call `ArrayDeque` allocation vs reuse |
| `FilterIndex/BoxingBenchmark.cs` | Boxing cost in filter index hot paths |
| `FilterIndex/DoubleRangeBenchmark.cs` | Double range predicate evaluation |
| `FilterIndex/StringRangeBenchmark.cs` | String range predicate evaluation |
| `FilterIndex/OrderedListDictionaryViewBenchmark.cs` | Dictionary view traversal patterns |
| `Scheduling/SchedulingServiceBenchmark.cs` | `List<long>` alloc per call vs reuse; `Keys.Last()` LINQ vs O(1) `LastEntry` |
| `Collections/LinkedHashMapBenchmark.cs` | `LinkedHashMap` iteration patterns |
| `Compat/AtomicLongBenchmark.cs` | `AtomicLong` increment cost |
| `EventService/ThreadLocalBenchmark.cs` | Thread-local access patterns |
| `Instrumentation/InstrumentationHelperBenchmark.cs` | Instrumentation call overhead |

### Running

```powershell
# All microbenchmarks (interactive benchmark selector)
dotnet run --project tst\NEsper.Benchmarks.Micro\NEsper.Benchmarks.Micro.csproj -c Release

# Specific benchmark class
dotnet run --project tst\NEsper.Benchmarks.Micro\NEsper.Benchmarks.Micro.csproj -c Release -- --filter "*SlimReaderWriterLock*"

# Restrict to a single runtime
dotnet run --project tst\NEsper.Benchmarks.Micro\NEsper.Benchmarks.Micro.csproj -c Release -- --filter "*Scheduling*" --runtimes net9.0

# List all available benchmark methods
dotnet run --project tst\NEsper.Benchmarks.Micro\NEsper.Benchmarks.Micro.csproj -c Release -- --list flat
```

Results are written to `BenchmarkDotNet.Artifacts/results/` as Markdown, HTML, and CSV.

---

## 2. End-to-End Benchmarks (`NEsper.Benchmarks.EndToEnd`)

**Location:** `tst/NEsper.Benchmarks.EndToEnd/`

Full-stack CEP engine throughput benchmarks — compile, deploy, send events, measure events/second and GC allocations across realistic EPL patterns. Each benchmark runs on both **net8.0** and **net9.0** with 3 warmup + 10 measurement iterations.

**Job matrix**: defined once in `EndToEndJobsAttribute.cs` and controlled by two environment variables (both set by `run-benchmarks.ps1` flags):

| Variable | Values | Default |
|---|---|---|
| `NESPER_BENCH_GC` | `workstation` \| `server` \| `both` | `workstation` (matches the committed baseline) |
| `NESPER_BENCH_RUNTIMES` | `net8.0` \| `net9.0` \| `both` | `both` |

Via the runner script: `.\run-benchmarks.ps1 -Suite e2e -GC server`. Direct:

```powershell
$env:NESPER_BENCH_GC = "server"
dotnet run --project tst\NEsper.Benchmarks.EndToEnd\NEsper.Benchmarks.EndToEnd.csproj -c Release -- --filter "*Contended*"
```

> Do **not** pass BDN's `--runtimes` to the e2e suite — it does not filter the job-defined runtimes, it adds an extra default-configured job that pollutes the results table. Use `NESPER_BENCH_RUNTIMES` instead.

Publish externally only from Server GC runs — production CEP deployments run Server GC and allocation-heavy paths behave differently under it.

### Benchmarks included

| File | EPL under test | Parameters |
|---|---|---|
| `Passthrough/PassthroughBenchmark.cs` | `select * from TradeEvent` | EventCount: 1k, 10k |
| `Filter/EqualityFilterBenchmark.cs` | `where Price = N` | EventCount × StatementCount (1/10/100) |
| `Filter/ComparisonFilterBenchmark.cs` | `where Price > N` | EventCount × StatementCount |
| `Filter/RangeFilterBenchmark.cs` | `where Price between N and M` | EventCount × StatementCount |
| `Filter/BooleanExprFilterBenchmark.cs` | `where Price > N and Volume > M` | EventCount × StatementCount |
| `Filter/StringFilterBenchmark.cs` | `where Symbol = 'X'` | EventCount × StatementCount |
| `Aggregation/AggregationBenchmark.cs` | `select avg(Price)` | EventCount |
| `Window/LengthWindowBenchmark.cs` | `TradeEvent#length(1000)` | EventCount: 1k, 10k |
| `Window/TimeWindowBenchmark.cs` | `TradeEvent#time(...)` | EventCount |
| `Join/TwoStreamJoinBenchmark.cs` | Two-stream equijoin | EventCount |
| `Pattern/SequencePatternBenchmark.cs` | Sequence pattern matching | EventCount |
| `Deploy/DeployUndeployBenchmark.cs` | Deploy + 100 events + undeploy cycle | — |
| `Threading/ContendedSendBenchmark.cs` | N threads sending concurrently into shared statements — statement-lock and filter-lock contention | ThreadCount (1/2/4/8) × StatementCount (1/10) |
| `Time/ExternalTimeBenchmark.cs` | External-time mode: `AdvanceTime` interleaved per event over a time window (scheduler `Evaluate` per advance) | EventCount: 1k, 10k |
| `Events/EventRepresentationBenchmark.cs` | Bean vs Map vs JSON event send through an identical statement (per-representation getter/sender paths) | EventCount: 10k |

### Running

```powershell
# All end-to-end benchmarks (interactive benchmark selector)
dotnet run --project tst\NEsper.Benchmarks.EndToEnd\NEsper.Benchmarks.EndToEnd.csproj -c Release

# Specific benchmark class
dotnet run --project tst\NEsper.Benchmarks.EndToEnd\NEsper.Benchmarks.EndToEnd.csproj -c Release -- --filter "*Equality*"
dotnet run --project tst\NEsper.Benchmarks.EndToEnd\NEsper.Benchmarks.EndToEnd.csproj -c Release -- --filter "*Window*"
dotnet run --project tst\NEsper.Benchmarks.EndToEnd\NEsper.Benchmarks.EndToEnd.csproj -c Release -- --filter "*Deploy*"

# Combine results from multiple benchmarks into one table
dotnet run --project tst\NEsper.Benchmarks.EndToEnd\NEsper.Benchmarks.EndToEnd.csproj -c Release -- --filter "*Filter*" --join

# List all available benchmark methods
dotnet run --project tst\NEsper.Benchmarks.EndToEnd\NEsper.Benchmarks.EndToEnd.csproj -c Release -- --list flat
```

### Useful BenchmarkDotNet flags

| Flag | Effect |
|---|---|
| `--filter "*Foo*"` | Run only benchmarks whose class name matches |
| `--runtimes net9.0` | Restrict to one target runtime |
| `--join` | Combine all results into a single comparison table |
| `--exporters html,csv` | Request additional output formats |
| `--list flat` | Print all benchmark methods without running |

---

## 3. Perforator — Profiling Harness (`NEsper.Benchmark.Perforator`)

**Location:** `examples/NEsper.Benchmark/NEsper.Benchmark.Perforator/`

A tight-loop profiling harness designed for attaching a profiler (dotTrace, Visual Studio Profiler, PerfView). Not intended for reproducible numbers — use the BenchmarkDotNet suites above for that. Runs each EPL statement for 20 seconds, then prints events sent, elapsed microseconds, and events/second.

The process sets `PriorityClass = RealTime` automatically at startup.

### EPL statements exercised (in sequence)

| Statement | EPL |
|---|---|
| `TextWinLengthWithGroup` | `select Symbol, TradeTime, BidPrice, avg(BidPrice), sum(BidPrice) from BidData.std:groupwin(Symbol).win:length(10000)` |
| `TextWinLength` | `select Symbol, TradeTime, avg(BidPrice), sum(BidPrice), count(*) from BidData.win:length(1000)` |
| `TextAggregationWithTimeWindow` | `select Symbol, TradeTime, avg(BidPrice), sum(BidPrice), count(*) from BidData.win:time(2 seconds)` |
| `TextAggregationWithNoWindow` | `select Symbol, TradeTime, BidPrice, avg(BidPrice), sum(BidPrice) from BidData` |
| `TextWithPropertyNames` | `select Symbol, TradeTime, BidPrice from BidData` |
| `TextWithWildcard` | `select * from BidData` |

### Running

```powershell
# Debug build — best for attaching a profiler (symbols present, no inlining)
dotnet run --project examples\NEsper.Benchmark\NEsper.Benchmark.Perforator\NEsper.Benchmark.Perforator.csproj

# Release build — for raw throughput numbers
dotnet run --project examples\NEsper.Benchmark\NEsper.Benchmark.Perforator\NEsper.Benchmark.Perforator.csproj -c Release
```

---

## 4. Distributed Benchmark (`NEsper.Benchmark.Server` + `NEsper.Benchmark.Client`)

**Location:** `examples/NEsper.Benchmark/`

A network-based throughput benchmark that simulates production market-data ingestion. The server hosts an EPL engine and receives events from one or more client processes over TCP or UDP. Stats are dumped to the server console at a configurable interval.

### Standalone simulation (no client process needed)

```powershell
# 4 threads × 5,000 events/s = 20,000 events/s, run indefinitely
dotnet run --project examples\NEsper.Benchmark\NEsper.Benchmark.Server\NEsper.Benchmark.Server.csproj -c Release -- -rate 4x5000

# Fixed iteration count — stops after 10 iterations
dotnet run --project examples\NEsper.Benchmark\NEsper.Benchmark.Server\NEsper.Benchmark.Server.csproj -c Release -- -rate 4x5000x10
```

### Server + separate client (two terminals)

**Terminal 1 — start server:**
```powershell
dotnet run --project examples\NEsper.Benchmark\NEsper.Benchmark.Server\NEsper.Benchmark.Server.csproj -c Release -- -port 6789 -exec inline -stat 5
```

**Terminal 2 — start client:**
```powershell
dotnet run --project examples\NEsper.Benchmark\NEsper.Benchmark.Client\NEsper.Benchmark.Client.csproj -c Release -- -host localhost -port 6789 -rate 10000
```

### Server flags

| Flag | Default | Description |
|---|---|---|
| `-port N` | 6789 | Listen port |
| `-exec inline\|unbound\|bound\|mqueue` | `inline` | Threading / dispatch model |
| `-thread N` | — | Thread multiplier for `bound`/`mqueue` executors |
| `-queue N` | — | Maximum queue depth for `bound`/`mqueue` |
| `-sleep N` | 0 | Simulate listener work (milliseconds) |
| `-stat N` | 5 | Stats dump interval (seconds) |
| `-rate NxM` or `NxMxI` | disabled | Standalone simulation: N threads × M events/s × I iterations |
| `-tcp` / `-udp` | TCP | Transport protocol |
| `-addr host` | any | Bind to a specific local address |
| `-prio class` | — | Set process priority (e.g. `RealTime`, `High`) |

### Client flags

| Flag | Default | Description |
|---|---|---|
| `-host H` | localhost | Server hostname |
| `-port N` | 6789 | Server port |
| `-rate N` | 4000 | Events per second (minimum 1000) |
| `-tcp` / `-udp` | TCP | Transport protocol |
| `-maxevents N` | unlimited | Stop after N total events |
| `-mtu N` | 1024 | Socket MTU (bytes) |
| `-delay N` | 0 | Startup delay before connecting (milliseconds) |

### Executor types

| Type | Behaviour |
|---|---|
| `inline` | Events dispatched on the receiving thread (lowest latency, single-threaded) |
| `unbound` | .NET `ThreadPool` — unbounded queue |
| `bound` | Fixed-size thread pool with bounded queue; drops to caller-side execution when full |
| `mqueue` | Multiple independent queues, one per thread core |

---

## Coverage & Roadmap

Assessment of what the benchmark suites can and cannot tell us today (reviewed 2026-07-04).

### What the current suites cover well

- **Single-threaded event path**: passthrough + the five filter-shape benchmarks with statement-count sweeps exercise the send → filter-match → statement-dispatch path directly. `[MemoryDiagnoser]` exposes per-event allocation regressions as bytes/op.
- **Fix validation**: the micro benchmarks follow a baseline-vs-improved pattern (e.g. `Current_SupportsRecursion` vs `Improved_NoRecursion`) tied to specific review findings, so a hot-path fix can be proven before and after.
- **Profiler entry point**: Perforator provides 20-second steady-state loops at RealTime priority for dotTrace/PerfView/VS Profiler attachment.
- **Runtime comparison**: every BenchmarkDotNet suite runs net8.0 and net9.0.

### Gaps closed 2026-07-04

- **Contended send** (`Threading/ContendedSendBenchmark`): N threads sending concurrently into shared stateful statements — measures statement agent-instance lock and filter-service lock contention that single-threaded benchmarks never see.
- **External-time mode** (`Time/ExternalTimeBenchmark`): `AdvanceTime` interleaved per event, exposing the scheduler `Evaluate` per-advance allocation cost hit by market-replay workloads.
- **Event representations** (`Events/EventRepresentationBenchmark`): bean vs map vs JSON senders through an identical statement, covering the per-representation property-getter codegen paths.

### Gaps closed 2026-07-04 (continued)

- **Server GC jobs** (`EndToEndJobsAttribute.cs`): the e2e job matrix now supports Workstation GC (default), Server GC, or both, selected via `NESPER_BENCH_GC` / `run-benchmarks.ps1 -GC`. Production-representative numbers no longer require a code change.

### Remaining gaps (priority order)

1. **Latency percentiles** — all benchmarks measure throughput (time per N events). No p50/p99/p999 per-event latency exists anywhere; GC pauses from hot-path allocation are invisible. Best home: HdrHistogram-style capture in the Benchmark.Server harness.
2. **Committed baselines / regression gating** — results land in `BenchmarkDotNet.Artifacts/` but nothing stores a baseline or fails CI on regression. Commit a baseline JSON per release and diff against it (the `run-benchmarks.ps1` manifest + results directory is the natural unit).
3. **Insert-into latch paths** — `InsertIntoLatchSpin`/`InsertIntoLatchWait` (multi-statement insert-into chains under threading config) have no benchmark.
4. **Wider EPL feature coverage in end-to-end** — group-by aggregation, named windows/tables, contexts/partitioned statements, output-rate limiting, subqueries, match-recognize. The regression suite covers these functionally but only with coarse wall-clock threshold assertions (typically 10× headroom), which catch order-of-magnitude regressions only.
5. **XML/Avro event representations** — lower priority than JSON/Map; add if those paths become product-relevant.

### A note on the regression-suite "performance" tests

`tst/NEsper.Regression` contains ~70 files of ported perf tests (`EPLJoin*Performance`, `InfraNamedWindowPerformance`, `ExprFilterOptimizablePerf`, …). They assert wall-clock ceilings with large headroom (e.g. "stay below 50 ms" asserting `< 500`). Treat them as functional smoke guards, not measurements — do not invest in tightening them; the BenchmarkDotNet suites are the measurement system.

---

## Baseline Snapshot — 2026-07-04

First full run of the three coverage-gap benchmarks. Use these numbers as the before/after reference when landing the hot-path fixes from `SENIOR-REVIEW.md`.

**Environment**: AMD Ryzen 9 7950X (16C/32T), Windows 11 (10.0.26200), .NET 8.0.26 / .NET 9.0.15, **Workstation GC** (see caveats), 3 warmup + 10 measurement iterations.

### Event representations (10k events, identical filter + projection statement)

| Representation | .NET 8 | .NET 9 | Allocated | vs Bean |
|---|---:|---:|---:|---|
| Bean (POCO) | 2.68 ms | 1.70 ms | 2.50 MB | baseline |
| Map | 2.67 ms | 2.00 ms | 2.27 MB | ~1.0× time, 0.91× alloc |
| JSON | 9.59 ms | 6.17 ms | 7.38 MB | ~3.6× time, 2.95× alloc |

- Map events are effectively free relative to beans.
- JSON carries a heavy parse+build tax (~3.6× time) — its own optimization target if JSON ingestion matters.
- The bean baseline allocates **~262 bytes/event** on a stateless filter — the floor that the SENIOR-REVIEW C1 fix (per-match lock allocations) should lower.

### Contention (10k total events split across N threads, stateful `avg` over `#length(1000)`)

| Threads | 1 stmt (net8) | 10 stmts (net8) | 10-stmt speedup vs 1T |
|---|---:|---:|---:|
| 1 | 4.33 ms | 43.4 ms | 1.0× |
| 2 | 5.82 ms | 32.6 ms | 1.3× |
| 4 | 5.82 ms | 22.4 ms | 1.9× |
| 8 | 7.41 ms | 16.2 ms | 2.7× |

- **Single statement scales negatively** (4.3 → 7.4 ms for the same total work, 1→8 threads): one statement means one write lock, so threads serialize; the 71% overhead growth is pure contention cost.
- **Ten independent statements reach only 2.7× on 8 threads** — the gap points at shared-path costs (filter-service read lock, GC pressure). Primary profiling target.
- **Allocations scale linearly with matches: ~700 bytes per statement-match** (7.64 MB @ 1 stmt → 70.1 MB @ 10 stmts per 10k events) — SENIOR-REVIEW finding C1 in production-shaped numbers. This Alloc column is the regression gate for the C1 fix.

### External time (`AdvanceTime` per event, `avg` over `#time(5 sec)`)

| EventCount | .NET 8 | .NET 9 | Allocated |
|---|---:|---:|---:|
| 1,000 | 1.25 ms | 1.04 ms | 3.87 MB |
| 10,000 | 12.75 ms | 10.24 ms | 38.7 MB |

- **~3.9 KB allocated per event** — roughly 15× the bean send path — with high Gen1 promotion. SENIOR-REVIEW finding M2 (per-advance scheduler allocations) compounded by time-window expiry churn. The before/after metric for the M2 fix.

### Cross-cutting

- **.NET 9 is consistently faster than .NET 8**: bean send 37%, JSON 36%, external time 20%.
- **Caveats**: Workstation GC (production runs Server GC — see Server GC jobs below); ±10–20% error bars on some contended cases; thread create/join overhead is inside the contended measurements (constant across variants, so ratios hold).
- Full BenchmarkDotNet output (Markdown/HTML/CSV): `BenchmarkDotNet.Artifacts/results/`.
