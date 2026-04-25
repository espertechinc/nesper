# NEsper Performance Benchmark Suite — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create two BenchmarkDotNet console projects — NEsper.Benchmarks.Micro and NEsper.Benchmarks.EndToEnd — covering all 16 PERF_REVIEW findings and the full representative EPL scenario set.

**Architecture:** NEsper.Benchmarks.Micro isolates each PERF_REVIEW finding in a standalone benchmark class with both a "current" (allocating) and "improved" (non-allocating) variant, so before/after comparisons are built in from day one. NEsper.Benchmarks.EndToEnd compiles and deploys real EPL statements in `[GlobalSetup]` then measures event throughput across the full EPL feature surface. Both projects use `[MemoryDiagnoser]` so allocs/op is visible alongside ns/op.

**Tech Stack:** BenchmarkDotNet 0.14.0, .NET 8.0 + 9.0 (multi-target), NEsper.Compat + NEsper.Common + NEsper.Runtime + NEsper.Compiler

---

## File Map

### tst/NEsper.Benchmarks.Micro
| File | Responsibility |
|------|----------------|
| `NEsper.Benchmarks.Micro.csproj` | Console app, BDN reference, project refs |
| `Program.cs` | `BenchmarkSwitcher.Run` entry point |
| `Locks/TrackedDisposableBenchmark.cs` | H1: heap alloc per lock acquire |
| `Locks/SlimReaderWriterLockBenchmark.cs` | M1: SupportsRecursion vs NoRecursion |
| `Collections/LinkedHashMapBenchmark.cs` | H2: foreach LINQ enumerator |
| `FilterIndex/DoubleRangeBenchmark.cs` | H3: DoubleRange class alloc ×2 per event |
| `FilterIndex/FilterServiceBaseEvalBenchmark.cs` | H4: ArrayDeque alloc vs reuse |
| `FilterIndex/OrderedListDictionaryViewBenchmark.cs` | H5: view alloc per Head/Tail/Between |
| `FilterIndex/StringRangeBenchmark.cs` | H6: StringRange class alloc ×2 per event |
| `FilterIndex/BoxingBenchmark.cs` | M4: object vs typed dictionary lookup |
| `Scheduling/SchedulingServiceBenchmark.cs` | M2+M3: List alloc per tick, Last() scan |
| `EventService/ThreadLocalBenchmark.cs` | L5: 10× TLS vs 1× cached |
| `Compat/AtomicLongBenchmark.cs` | L2: CAS loop vs Interlocked.Increment |
| `Instrumentation/InstrumentationHelperBenchmark.cs` | L4: property getter vs const |

### tst/NEsper.Benchmarks.EndToEnd
| File | Responsibility |
|------|----------------|
| `NEsper.Benchmarks.EndToEnd.csproj` | Console app, BDN + Compiler reference |
| `Program.cs` | `BenchmarkSwitcher.Run` entry point |
| `TradeEvent.cs` | Shared POCO event model |
| `QuoteEvent.cs` | Second event type for join benchmark |
| `Passthrough/PassthroughBenchmark.cs` | Baseline: `select * from TradeEvent` |
| `Filter/EqualityFilterBenchmark.cs` | `where Price = 100.0` |
| `Filter/ComparisonFilterBenchmark.cs` | `where Price > 100.0` |
| `Filter/RangeFilterBenchmark.cs` | `where Price between 90.0 and 110.0` |
| `Filter/BooleanExprFilterBenchmark.cs` | `where Price > 90 and Volume > 500` |
| `Filter/StringFilterBenchmark.cs` | `where Symbol = 'MSFT'` |
| `Window/TimeWindowBenchmark.cs` | `TradeEvent#time(5 sec)` |
| `Window/LengthWindowBenchmark.cs` | `TradeEvent#length(1000)` |
| `Aggregation/AggregationBenchmark.cs` | `avg/sum/count` over time window |
| `Join/TwoStreamJoinBenchmark.cs` | Join TradeEvent + QuoteEvent on Symbol |
| `Pattern/SequencePatternBenchmark.cs` | `every (a -> b(Price > a.Price))` |
| `Deploy/DeployUndeployBenchmark.cs` | Concurrent deploy/undeploy under load (M5) |

---

## Task 1: Scaffold NEsper.Benchmarks.Micro

**Files:**
- Create: `tst/NEsper.Benchmarks.Micro/NEsper.Benchmarks.Micro.csproj`
- Create: `tst/NEsper.Benchmarks.Micro/Program.cs`
- Modify: `NEsper.sln`
- Modify: `NEsperAll.sln`

- [ ] **Step 1: Create the project file**

`tst/NEsper.Benchmarks.Micro/NEsper.Benchmarks.Micro.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
        <AssemblyName>NEsper.Benchmarks.Micro</AssemblyName>
        <RootNamespace>NEsper.Benchmarks.Micro</RootNamespace>
        <Nullable>enable</Nullable>
        <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="BenchmarkDotNet" Version="0.14.0" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\..\src\NEsper.Compat\NEsper.Compat.csproj" />
        <ProjectReference Include="..\..\src\NEsper.Common\NEsper.Common.csproj" />
        <ProjectReference Include="..\..\src\NEsper.Runtime\NEsper.Runtime.csproj" />
    </ItemGroup>
</Project>
```

- [ ] **Step 2: Create Program.cs**

`tst/NEsper.Benchmarks.Micro/Program.cs`:
```csharp
using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
```

- [ ] **Step 3: Add to both solution files**

```bash
dotnet sln NEsper.sln add --solution-folder tst tst/NEsper.Benchmarks.Micro/NEsper.Benchmarks.Micro.csproj
dotnet sln NEsperAll.sln add --solution-folder tst tst/NEsper.Benchmarks.Micro/NEsper.Benchmarks.Micro.csproj
```

- [ ] **Step 4: Verify build**

```bash
dotnet build tst/NEsper.Benchmarks.Micro/NEsper.Benchmarks.Micro.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add tst/NEsper.Benchmarks.Micro/ NEsper.sln NEsperAll.sln
git commit -m "Add NEsper.Benchmarks.Micro project scaffold"
```

---

## Task 2: Scaffold NEsper.Benchmarks.EndToEnd

**Files:**
- Create: `tst/NEsper.Benchmarks.EndToEnd/NEsper.Benchmarks.EndToEnd.csproj`
- Create: `tst/NEsper.Benchmarks.EndToEnd/Program.cs`
- Create: `tst/NEsper.Benchmarks.EndToEnd/TradeEvent.cs`
- Create: `tst/NEsper.Benchmarks.EndToEnd/QuoteEvent.cs`
- Modify: `NEsper.sln`
- Modify: `NEsperAll.sln`

- [ ] **Step 1: Create the project file**

`tst/NEsper.Benchmarks.EndToEnd/NEsper.Benchmarks.EndToEnd.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
        <AssemblyName>NEsper.Benchmarks.EndToEnd</AssemblyName>
        <RootNamespace>NEsper.Benchmarks.EndToEnd</RootNamespace>
        <Nullable>enable</Nullable>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="BenchmarkDotNet" Version="0.14.0" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\..\src\NEsper.Compat\NEsper.Compat.csproj" />
        <ProjectReference Include="..\..\src\NEsper.Common\NEsper.Common.csproj" />
        <ProjectReference Include="..\..\src\NEsper.Runtime\NEsper.Runtime.csproj" />
        <ProjectReference Include="..\..\src\NEsper.Compiler\NEsper.Compiler.csproj" />
    </ItemGroup>
</Project>
```

- [ ] **Step 2: Create TradeEvent.cs**

`tst/NEsper.Benchmarks.EndToEnd/TradeEvent.cs`:
```csharp
namespace NEsper.Benchmarks.EndToEnd;

public class TradeEvent
{
    public string Symbol    { get; set; } = string.Empty;
    public double Price     { get; set; }
    public long   Volume    { get; set; }
    public long   Timestamp { get; set; }
}
```

- [ ] **Step 3: Create QuoteEvent.cs**

`tst/NEsper.Benchmarks.EndToEnd/QuoteEvent.cs`:
```csharp
namespace NEsper.Benchmarks.EndToEnd;

public class QuoteEvent
{
    public string Symbol   { get; set; } = string.Empty;
    public double BidPrice { get; set; }
    public double AskPrice { get; set; }
}
```

- [ ] **Step 4: Create Program.cs**

`tst/NEsper.Benchmarks.EndToEnd/Program.cs`:
```csharp
using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
```

- [ ] **Step 5: Add to both solution files**

```bash
dotnet sln NEsper.sln add --solution-folder tst tst/NEsper.Benchmarks.EndToEnd/NEsper.Benchmarks.EndToEnd.csproj
dotnet sln NEsperAll.sln add --solution-folder tst tst/NEsper.Benchmarks.EndToEnd/NEsper.Benchmarks.EndToEnd.csproj
```

- [ ] **Step 6: Verify build**

```bash
dotnet build tst/NEsper.Benchmarks.EndToEnd/NEsper.Benchmarks.EndToEnd.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 7: Commit**

```bash
git add tst/NEsper.Benchmarks.EndToEnd/ NEsper.sln NEsperAll.sln
git commit -m "Add NEsper.Benchmarks.EndToEnd project scaffold with event model"
```

---

## Task 3: Micro — Lock benchmarks (H1, M1)

**Files:**
- Create: `tst/NEsper.Benchmarks.Micro/Locks/TrackedDisposableBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.Micro/Locks/SlimReaderWriterLockBenchmark.cs`

- [ ] **Step 1: Create TrackedDisposableBenchmark.cs**

`tst/NEsper.Benchmarks.Micro/Locks/TrackedDisposableBenchmark.cs`:
```csharp
// PERF_REVIEW: H1 — every Acquire() allocates a new TrackedDisposable on the heap
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.compat.threading.locks;

namespace NEsper.Benchmarks.Micro.Locks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class TrackedDisposableBenchmark
{
    private SlimReaderWriterLock _rwLock = null!;

    [GlobalSetup]
    public void Setup() => _rwLock = new SlimReaderWriterLock();

    [Benchmark(Baseline = true)]
    public void Current_AcquireDisposable()
    {
        using (_rwLock.ReadLock.Acquire()) { }
    }

    [Benchmark]
    public void Improved_DirectAcquireRelease()
    {
        _rwLock.AcquireReaderLock(LockConstants.DefaultTimeout);
        try { }
        finally { _rwLock.ReleaseReaderLock(); }
    }
}
```

- [ ] **Step 2: Create SlimReaderWriterLockBenchmark.cs**

`tst/NEsper.Benchmarks.Micro/Locks/SlimReaderWriterLockBenchmark.cs`:
```csharp
// PERF_REVIEW: M1 — SupportsRecursion forces per-thread TLS state tracking on every acquire/release
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace NEsper.Benchmarks.Micro.Locks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class SlimReaderWriterLockBenchmark
{
    private ReaderWriterLockSlim _withRecursion    = null!;
    private ReaderWriterLockSlim _withoutRecursion = null!;

    [GlobalSetup]
    public void Setup()
    {
        _withRecursion    = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        _withoutRecursion = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    }

    [Benchmark(Baseline = true)]
    public void Current_SupportsRecursion()
    {
        _withRecursion.EnterReadLock();
        _withRecursion.ExitReadLock();
    }

    [Benchmark]
    public void Improved_NoRecursion()
    {
        _withoutRecursion.EnterReadLock();
        _withoutRecursion.ExitReadLock();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _withRecursion.Dispose();
        _withoutRecursion.Dispose();
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build tst/NEsper.Benchmarks.Micro/NEsper.Benchmarks.Micro.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add tst/NEsper.Benchmarks.Micro/Locks/
git commit -m "Add lock microbenchmarks H1 (TrackedDisposable) and M1 (SupportsRecursion)"
```

---

## Task 4: Micro — Collections benchmark (H2)

**Files:**
- Create: `tst/NEsper.Benchmarks.Micro/Collections/LinkedHashMapBenchmark.cs`

- [ ] **Step 1: Create LinkedHashMapBenchmark.cs**

`tst/NEsper.Benchmarks.Micro/Collections/LinkedHashMapBenchmark.cs`:
```csharp
// PERF_REVIEW: H2 — GetEnumerator() returns a LINQ SelectEnumerableIterator heap object per foreach
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.compat.collections;

namespace NEsper.Benchmarks.Micro.Collections;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class LinkedHashMapBenchmark
{
    private LinkedHashMap<string, int> _map = null!;

    [Params(10, 100)]
    public int ItemCount;

    [GlobalSetup]
    public void Setup()
    {
        _map = new LinkedHashMap<string, int>();
        for (int i = 0; i < ItemCount; i++)
            _map[$"key{i}"] = i;
    }

    // Baseline: establishes current allocation cost per foreach.
    // Once H2 is fixed (struct enumerator added), add Improved_StructEnumerator() here.
    [Benchmark(Baseline = true)]
    public int Current_LinqEnumerator()
    {
        int sum = 0;
        foreach (var pair in _map)
            sum += pair.Value;
        return sum;
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build tst/NEsper.Benchmarks.Micro/NEsper.Benchmarks.Micro.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add tst/NEsper.Benchmarks.Micro/Collections/
git commit -m "Add LinkedHashMap microbenchmark H2"
```

---

## Task 5: Micro — FilterIndex benchmarks (H3, H5, H6)

**Files:**
- Create: `tst/NEsper.Benchmarks.Micro/FilterIndex/DoubleRangeBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.Micro/FilterIndex/OrderedListDictionaryViewBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.Micro/FilterIndex/StringRangeBenchmark.cs`

- [ ] **Step 1: Create DoubleRangeBenchmark.cs**

`tst/NEsper.Benchmarks.Micro/FilterIndex/DoubleRangeBenchmark.cs`:
```csharp
// PERF_REVIEW: H3 — FilterParamIndexDoubleRange allocates two DoubleRange class objects per event
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.@internal.filterspec;

namespace NEsper.Benchmarks.Micro.FilterIndex;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class DoubleRangeBenchmark
{
    private const double AttributeValue = 105.5;
    private const double LargestRangeValue = 1000.0;

    [Benchmark(Baseline = true)]
    public (DoubleRange, DoubleRange) Current_AllocateTwoRanges()
    {
        var rangeStart = new DoubleRange(AttributeValue - LargestRangeValue, AttributeValue);
        var rangeEnd   = new DoubleRange(AttributeValue, double.MaxValue);
        return (rangeStart, rangeEnd);
    }

    [Benchmark]
    public (double, double, double, double) Improved_StructBounds()
    {
        // Stack-only bounds: no heap pressure. After H3 fix converts DoubleRange to readonly struct,
        // the Current_ variant should match this allocation profile.
        double lo = AttributeValue - LargestRangeValue;
        double hi = AttributeValue;
        return (lo, hi, hi, double.MaxValue);
    }
}
```

- [ ] **Step 2: Create OrderedListDictionaryViewBenchmark.cs**

`tst/NEsper.Benchmarks.Micro/FilterIndex/OrderedListDictionaryViewBenchmark.cs`:
```csharp
// PERF_REVIEW: H5 — Head/Tail/Between each allocate an OrderedListDictionaryView + enumerator per call
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.compat.collections;

namespace NEsper.Benchmarks.Micro.FilterIndex;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class OrderedListDictionaryViewBenchmark
{
    private OrderedListDictionary<double, int> _dict = null!;
    private const double QueryValue = 105.5;

    [Params(10, 100)]
    public int EntryCount;

    [GlobalSetup]
    public void Setup()
    {
        _dict = new OrderedListDictionary<double, int>();
        for (int i = 0; i < EntryCount; i++)
            _dict[i * 10.0] = i;
    }

    [Benchmark(Baseline = true)]
    public int Current_HeadViewAlloc()
    {
        int sum = 0;
        var view = _dict.Head(QueryValue, false);
        foreach (var v in view.Values)
            sum += v;
        return sum;
    }

    [Benchmark]
    public int Improved_DirectIteration()
    {
        // Simulates ForEachValueInHead — walks the backing list without creating a view object.
        // After H5 is implemented in production, replace with _dict.ForEachValueInHead(QueryValue, false, v => sum += v).
        int sum = 0;
        foreach (var kvp in _dict)
        {
            if (kvp.Key >= QueryValue) break;
            sum += kvp.Value;
        }
        return sum;
    }
}
```

- [ ] **Step 3: Create StringRangeBenchmark.cs**

`tst/NEsper.Benchmarks.Micro/FilterIndex/StringRangeBenchmark.cs`:
```csharp
// PERF_REVIEW: H6 — FilterParamIndexStringRange allocates two StringRange class objects per event
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.@internal.filterspec;

namespace NEsper.Benchmarks.Micro.FilterIndex;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class StringRangeBenchmark
{
    private const string AttributeValue = "MSFT";

    [Benchmark(Baseline = true)]
    public (StringRange, StringRange) Current_AllocateTwoRanges()
    {
        var rangeStart = new StringRange(null, AttributeValue);
        var rangeEnd   = new StringRange(AttributeValue, null);
        return (rangeStart, rangeEnd);
    }

    [Benchmark]
    public (string?, string?, string?, string?) Improved_InlineBounds()
    {
        // No allocation: boundary strings are already constants.
        // After H6 fix converts StringRange to readonly struct, Current_ should match.
        return (null, AttributeValue, AttributeValue, null);
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build tst/NEsper.Benchmarks.Micro/NEsper.Benchmarks.Micro.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add tst/NEsper.Benchmarks.Micro/FilterIndex/DoubleRangeBenchmark.cs \
        tst/NEsper.Benchmarks.Micro/FilterIndex/OrderedListDictionaryViewBenchmark.cs \
        tst/NEsper.Benchmarks.Micro/FilterIndex/StringRangeBenchmark.cs
git commit -m "Add FilterIndex microbenchmarks H3 (DoubleRange), H5 (OLDView), H6 (StringRange)"
```

---

## Task 6: Micro — FilterIndex benchmarks (H4, M4)

**Files:**
- Create: `tst/NEsper.Benchmarks.Micro/FilterIndex/FilterServiceBaseEvalBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.Micro/FilterIndex/BoxingBenchmark.cs`

- [ ] **Step 1: Create FilterServiceBaseEvalBenchmark.cs**

`tst/NEsper.Benchmarks.Micro/FilterIndex/FilterServiceBaseEvalBenchmark.cs`:
```csharp
// PERF_REVIEW: H4 — EvaluateInternal(statementId) allocates a new 256-capacity ArrayDeque per call
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.compat;

namespace NEsper.Benchmarks.Micro.FilterIndex;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class FilterServiceBaseEvalBenchmark
{
    private ArrayDeque<object> _reusable = null!;

    [GlobalSetup]
    public void Setup() => _reusable = new ArrayDeque<object>();

    [Benchmark(Baseline = true)]
    public int Current_AllocatePerCall()
    {
        var matches = new ArrayDeque<object>();
        matches.Add(new object());
        int count = matches.Count;
        return count;
    }

    [Benchmark]
    public int Improved_ReuseInstance()
    {
        _reusable.Clear();
        _reusable.Add(new object());
        int count = _reusable.Count;
        _reusable.Clear();
        return count;
    }
}
```

- [ ] **Step 2: Create BoxingBenchmark.cs**

`tst/NEsper.Benchmarks.Micro/FilterIndex/BoxingBenchmark.cs`:
```csharp
// PERF_REVIEW: M4 — FilterParamIndexEqualsBase stores keys as object, boxing every numeric value
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace NEsper.Benchmarks.Micro.FilterIndex;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class BoxingBenchmark
{
    private Dictionary<object, int> _objectDict = null!;
    private Dictionary<double, int> _typedDict  = null!;
    private const double LookupKey  = 100.0;
    private const int    EntryCount = 100;

    [GlobalSetup]
    public void Setup()
    {
        _objectDict = new Dictionary<object, int>(EntryCount);
        _typedDict  = new Dictionary<double, int>(EntryCount);
        for (int i = 0; i < EntryCount; i++)
        {
            _objectDict[(double)i] = i;
            _typedDict[(double)i]  = i;
        }
    }

    [Benchmark(Baseline = true)]
    public bool Current_ObjectDictLookup()
    {
        return _objectDict.TryGetValue(LookupKey, out _);
    }

    [Benchmark]
    public bool Improved_TypedDictLookup()
    {
        return _typedDict.TryGetValue(LookupKey, out _);
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build tst/NEsper.Benchmarks.Micro/NEsper.Benchmarks.Micro.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add tst/NEsper.Benchmarks.Micro/FilterIndex/FilterServiceBaseEvalBenchmark.cs \
        tst/NEsper.Benchmarks.Micro/FilterIndex/BoxingBenchmark.cs
git commit -m "Add FilterIndex microbenchmarks H4 (ArrayDeque reuse) and M4 (boxing)"
```

---

## Task 7: Micro — Scheduling benchmarks (M2, M3)

**Files:**
- Create: `tst/NEsper.Benchmarks.Micro/Scheduling/SchedulingServiceBenchmark.cs`

- [ ] **Step 1: Create SchedulingServiceBenchmark.cs**

`tst/NEsper.Benchmarks.Micro/Scheduling/SchedulingServiceBenchmark.cs`:
```csharp
// PERF_REVIEW: M2 — new List<long>() allocated inside lock(this) on every Evaluate() call
// PERF_REVIEW: M3 — Keys.Last() is a LINQ scan; OrderedListDictionary.LastEntry.Key is O(1)
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.compat.collections;

namespace NEsper.Benchmarks.Micro.Scheduling;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class SchedulingServiceBenchmark
{
    private List<long>                        _reusableList = null!;
    private OrderedListDictionary<long, long> _timeMap      = null!;

    [Params(10, 100)]
    public int EntryCount;

    [GlobalSetup]
    public void Setup()
    {
        _reusableList = new List<long>(EntryCount);
        _timeMap      = new OrderedListDictionary<long, long>();
        for (int i = 0; i < EntryCount; i++)
            _timeMap[(long)i * 1000L] = i;
    }

    [Benchmark(Baseline = true)]
    public int M2_Current_AllocateList()
    {
        IList<long> removeKeys = new List<long>();
        foreach (var entry in _timeMap)
            removeKeys.Add(entry.Key);
        return removeKeys.Count;
    }

    [Benchmark]
    public int M2_Improved_ReuseList()
    {
        _reusableList.Clear();
        foreach (var entry in _timeMap)
            _reusableList.Add(entry.Key);
        return _reusableList.Count;
    }

    [Benchmark]
    public long M3_Current_LinqLast()
    {
        return _timeMap.Keys.Last();
    }

    [Benchmark]
    public long M3_Improved_LastEntry()
    {
        return _timeMap.LastEntry.Key;
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build tst/NEsper.Benchmarks.Micro/NEsper.Benchmarks.Micro.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add tst/NEsper.Benchmarks.Micro/Scheduling/
git commit -m "Add Scheduling microbenchmarks M2 (List alloc) and M3 (Last() scan)"
```

---

## Task 8: Micro — EventService, Compat, Instrumentation (L2, L4, L5)

**Files:**
- Create: `tst/NEsper.Benchmarks.Micro/EventService/ThreadLocalBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.Micro/Compat/AtomicLongBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.Micro/Instrumentation/InstrumentationHelperBenchmark.cs`

- [ ] **Step 1: Create ThreadLocalBenchmark.cs**

`tst/NEsper.Benchmarks.Micro/EventService/ThreadLocalBenchmark.cs`:
```csharp
// PERF_REVIEW: L5 — _threadLocals.GetOrCreate() called ~10 times per event dispatch on same thread
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace NEsper.Benchmarks.Micro.EventService;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class ThreadLocalBenchmark
{
    [ThreadStatic]
    private static object? _tlsEntry;

    [GlobalSetup]
    public void Setup() => _tlsEntry = new object();

    [Benchmark(Baseline = true)]
    public int Current_TenTlsLookups()
    {
        var e1  = _tlsEntry; var e2  = _tlsEntry; var e3  = _tlsEntry;
        var e4  = _tlsEntry; var e5  = _tlsEntry; var e6  = _tlsEntry;
        var e7  = _tlsEntry; var e8  = _tlsEntry; var e9  = _tlsEntry;
        var e10 = _tlsEntry;
        return e1 == e10 ? 1 : 0;
    }

    [Benchmark]
    public int Improved_OneTlsLookupCached()
    {
        var entry = _tlsEntry;
        var e1 = entry; var e2 = entry; var e3 = entry; var e4 = entry;
        var e5 = entry; var e6 = entry; var e7 = entry; var e8 = entry;
        var e9 = entry; var e10 = entry;
        return e1 == e10 ? 1 : 0;
    }
}
```

- [ ] **Step 2: Create AtomicLongBenchmark.cs**

`tst/NEsper.Benchmarks.Micro/Compat/AtomicLongBenchmark.cs`:
```csharp
// PERF_REVIEW: L2 — GetAndIncrement() uses a CAS spin loop; Interlocked.Increment - 1 is unconditional
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.compat;

namespace NEsper.Benchmarks.Micro.Compat;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class AtomicLongBenchmark
{
    private AtomicLong _atomicLong = null!;
    private long       _rawValue;

    [GlobalSetup]
    public void Setup()
    {
        _atomicLong = new AtomicLong(0);
        _rawValue   = 0;
    }

    [Benchmark(Baseline = true)]
    public long Current_CasSpinLoop()
    {
        return _atomicLong.GetAndIncrement();
    }

    [Benchmark]
    public long Improved_InterlockedIncrement()
    {
        return Interlocked.Increment(ref _rawValue) - 1;
    }
}
```

- [ ] **Step 3: Create InstrumentationHelperBenchmark.cs**

`tst/NEsper.Benchmarks.Micro/Instrumentation/InstrumentationHelperBenchmark.cs`:
```csharp
// PERF_REVIEW: L4 — ENABLED is a settable property; JIT cannot eliminate the dead-branch guard
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace NEsper.Benchmarks.Micro.Instrumentation;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class InstrumentationHelperBenchmark
{
    private const bool         ConstFalse    = false;
    private static readonly bool ReadonlyFalse = false;

    // Mirrors the 4 per-MatchEvent ENABLED checks in filter index hot paths
    [Benchmark(Baseline = true)]
    public int Current_PropertyCheck()
    {
        int count = 0;
        if (InstrumentationHelper.ENABLED) count++;
        if (InstrumentationHelper.ENABLED) count++;
        if (InstrumentationHelper.ENABLED) count++;
        if (InstrumentationHelper.ENABLED) count++;
        return count;
    }

    [Benchmark]
    public int Improved_ConstCheck()
    {
        int count = 0;
        if (ConstFalse) count++;
        if (ConstFalse) count++;
        if (ConstFalse) count++;
        if (ConstFalse) count++;
        return count;
    }

    [Benchmark]
    public int Improved_ReadonlyFieldCheck()
    {
        int count = 0;
        if (ReadonlyFalse) count++;
        if (ReadonlyFalse) count++;
        if (ReadonlyFalse) count++;
        if (ReadonlyFalse) count++;
        return count;
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build tst/NEsper.Benchmarks.Micro/NEsper.Benchmarks.Micro.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add tst/NEsper.Benchmarks.Micro/EventService/ \
        tst/NEsper.Benchmarks.Micro/Compat/ \
        tst/NEsper.Benchmarks.Micro/Instrumentation/
git commit -m "Add microbenchmarks L2 (AtomicLong), L4 (InstrumentationHelper), L5 (TLS lookup)"
```

---

## Task 9: EndToEnd — Passthrough, Equality, Comparison filters

**Files:**
- Create: `tst/NEsper.Benchmarks.EndToEnd/Passthrough/PassthroughBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.EndToEnd/Filter/EqualityFilterBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.EndToEnd/Filter/ComparisonFilterBenchmark.cs`

The end-to-end pattern used throughout tasks 9–12:
- `[GlobalSetup]`: create `Configuration`, call `config.Common.AddEventType(typeof(TradeEvent))`, create runtime via `EPRuntimeProvider.GetRuntime(uniqueName, config)`, compile EPL using `EPCompilerProvider.Compiler.Compile(epl, new CompilerArguments(config))`, deploy, pre-allocate event array.
- `[Benchmark]`: loop `EventCount` times calling `_runtime.EventService.SendEventBean(_events[i], "TradeEvent")`.
- `[GlobalCleanup]`: `UndeployAll()` then `Destroy()`.

Namespaces needed in every end-to-end file:
```csharp
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;
```

- [ ] **Step 1: Create PassthroughBenchmark.cs**

`tst/NEsper.Benchmarks.EndToEnd/Passthrough/PassthroughBenchmark.cs`:
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Passthrough;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class PassthroughBenchmark
{
    private EPRuntime    _runtime = null!;
    private TradeEvent[] _events  = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"passthrough-{Guid.NewGuid()}", config);

        var compiled = EPCompilerProvider.Compiler.Compile(
            "select * from TradeEvent", new CompilerArguments(config));
        _runtime.DeploymentService.Deploy(compiled);

        _events = new TradeEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
            _events[i] = new TradeEvent { Symbol = "MSFT", Price = 100.0 + i, Volume = 1000L, Timestamp = i };
    }

    [Benchmark]
    public void SendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventBean(_events[i], "TradeEvent");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
```

- [ ] **Step 2: Create EqualityFilterBenchmark.cs**

`tst/NEsper.Benchmarks.EndToEnd/Filter/EqualityFilterBenchmark.cs`:
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Filter;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class EqualityFilterBenchmark
{
    private EPRuntime    _runtime = null!;
    private TradeEvent[] _events  = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [Params(1, 10, 100)]
    public int StatementCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"eq-{Guid.NewGuid()}", config);

        var compiler = EPCompilerProvider.Compiler;
        var args     = new CompilerArguments(config);
        for (int s = 0; s < StatementCount; s++)
        {
            var compiled = compiler.Compile(
                $"select * from TradeEvent where Price = {100.0 + s}", args);
            _runtime.DeploymentService.Deploy(compiled);
        }

        _events = new TradeEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
            _events[i] = new TradeEvent { Symbol = "MSFT", Price = 100.0 + (i % StatementCount), Volume = 1000L, Timestamp = i };
    }

    [Benchmark]
    public void SendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventBean(_events[i], "TradeEvent");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
```

- [ ] **Step 3: Create ComparisonFilterBenchmark.cs**

`tst/NEsper.Benchmarks.EndToEnd/Filter/ComparisonFilterBenchmark.cs`:
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Filter;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class ComparisonFilterBenchmark
{
    private EPRuntime    _runtime = null!;
    private TradeEvent[] _events  = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [Params(1, 10, 100)]
    public int StatementCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"cmp-{Guid.NewGuid()}", config);

        var compiler = EPCompilerProvider.Compiler;
        var args     = new CompilerArguments(config);
        for (int s = 0; s < StatementCount; s++)
        {
            var compiled = compiler.Compile(
                $"select * from TradeEvent where Price > {90.0 + s}", args);
            _runtime.DeploymentService.Deploy(compiled);
        }

        _events = new TradeEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
            _events[i] = new TradeEvent { Symbol = "MSFT", Price = 100.0 + (i % 200), Volume = 1000L, Timestamp = i };
    }

    [Benchmark]
    public void SendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventBean(_events[i], "TradeEvent");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build tst/NEsper.Benchmarks.EndToEnd/NEsper.Benchmarks.EndToEnd.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add tst/NEsper.Benchmarks.EndToEnd/Passthrough/ \
        tst/NEsper.Benchmarks.EndToEnd/Filter/EqualityFilterBenchmark.cs \
        tst/NEsper.Benchmarks.EndToEnd/Filter/ComparisonFilterBenchmark.cs
git commit -m "Add end-to-end benchmarks: passthrough, equality filter, comparison filter"
```

---

## Task 10: EndToEnd — Range, Boolean, String filters

**Files:**
- Create: `tst/NEsper.Benchmarks.EndToEnd/Filter/RangeFilterBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.EndToEnd/Filter/BooleanExprFilterBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.EndToEnd/Filter/StringFilterBenchmark.cs`

- [ ] **Step 1: Create RangeFilterBenchmark.cs**

`tst/NEsper.Benchmarks.EndToEnd/Filter/RangeFilterBenchmark.cs`:
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Filter;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class RangeFilterBenchmark
{
    private EPRuntime    _runtime = null!;
    private TradeEvent[] _events  = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [Params(1, 10, 100)]
    public int StatementCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"range-{Guid.NewGuid()}", config);

        var compiler = EPCompilerProvider.Compiler;
        var args     = new CompilerArguments(config);
        for (int s = 0; s < StatementCount; s++)
        {
            double lo = 80.0 + s;
            double hi = 120.0 + s;
            var compiled = compiler.Compile(
                $"select * from TradeEvent where Price between {lo} and {hi}", args);
            _runtime.DeploymentService.Deploy(compiled);
        }

        _events = new TradeEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
            _events[i] = new TradeEvent { Symbol = "MSFT", Price = 90.0 + (i % 50), Volume = 1000L, Timestamp = i };
    }

    [Benchmark]
    public void SendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventBean(_events[i], "TradeEvent");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
```

- [ ] **Step 2: Create BooleanExprFilterBenchmark.cs**

`tst/NEsper.Benchmarks.EndToEnd/Filter/BooleanExprFilterBenchmark.cs`:
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Filter;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class BooleanExprFilterBenchmark
{
    private EPRuntime    _runtime = null!;
    private TradeEvent[] _events  = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [Params(1, 10, 100)]
    public int StatementCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"boolexpr-{Guid.NewGuid()}", config);

        var compiler = EPCompilerProvider.Compiler;
        var args     = new CompilerArguments(config);
        for (int s = 0; s < StatementCount; s++)
        {
            var compiled = compiler.Compile(
                $"select * from TradeEvent where Price > {90.0 + s} and Volume > 500", args);
            _runtime.DeploymentService.Deploy(compiled);
        }

        _events = new TradeEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
            _events[i] = new TradeEvent { Symbol = "MSFT", Price = 100.0 + (i % 50), Volume = 600L + i, Timestamp = i };
    }

    [Benchmark]
    public void SendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventBean(_events[i], "TradeEvent");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
```

- [ ] **Step 3: Create StringFilterBenchmark.cs**

`tst/NEsper.Benchmarks.EndToEnd/Filter/StringFilterBenchmark.cs`:
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Filter;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class StringFilterBenchmark
{
    private static readonly string[] Symbols = { "AAPL", "GOOG", "MSFT", "AMZN", "NFLX" };

    private EPRuntime    _runtime = null!;
    private TradeEvent[] _events  = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [Params(1, 10)]
    public int StatementCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"strflt-{Guid.NewGuid()}", config);

        var compiler = EPCompilerProvider.Compiler;
        var args     = new CompilerArguments(config);
        for (int s = 0; s < StatementCount; s++)
        {
            var sym      = Symbols[s % Symbols.Length];
            var compiled = compiler.Compile(
                $"select * from TradeEvent where Symbol = '{sym}'", args);
            _runtime.DeploymentService.Deploy(compiled);
        }

        _events = new TradeEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
            _events[i] = new TradeEvent { Symbol = Symbols[i % Symbols.Length], Price = 100.0, Volume = 1000L, Timestamp = i };
    }

    [Benchmark]
    public void SendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventBean(_events[i], "TradeEvent");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
```

- [ ] **Step 4: Build**

```bash
dotnet build tst/NEsper.Benchmarks.EndToEnd/NEsper.Benchmarks.EndToEnd.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add tst/NEsper.Benchmarks.EndToEnd/Filter/RangeFilterBenchmark.cs \
        tst/NEsper.Benchmarks.EndToEnd/Filter/BooleanExprFilterBenchmark.cs \
        tst/NEsper.Benchmarks.EndToEnd/Filter/StringFilterBenchmark.cs
git commit -m "Add end-to-end benchmarks: range filter, boolean expression filter, string filter"
```

---

## Task 11: EndToEnd — Window benchmarks

**Files:**
- Create: `tst/NEsper.Benchmarks.EndToEnd/Window/TimeWindowBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.EndToEnd/Window/LengthWindowBenchmark.cs`

- [ ] **Step 1: Create TimeWindowBenchmark.cs**

`tst/NEsper.Benchmarks.EndToEnd/Window/TimeWindowBenchmark.cs`:
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Window;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class TimeWindowBenchmark
{
    private EPRuntime    _runtime = null!;
    private TradeEvent[] _events  = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"timewin-{Guid.NewGuid()}", config);

        var compiled = EPCompilerProvider.Compiler.Compile(
            "select * from TradeEvent#time(5 sec)", new CompilerArguments(config));
        _runtime.DeploymentService.Deploy(compiled);

        _events = new TradeEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
            _events[i] = new TradeEvent { Symbol = "MSFT", Price = 100.0 + i, Volume = 1000L, Timestamp = i };
    }

    [Benchmark]
    public void SendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventBean(_events[i], "TradeEvent");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
```

- [ ] **Step 2: Create LengthWindowBenchmark.cs**

`tst/NEsper.Benchmarks.EndToEnd/Window/LengthWindowBenchmark.cs`:
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Window;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class LengthWindowBenchmark
{
    private EPRuntime    _runtime = null!;
    private TradeEvent[] _events  = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"lenwin-{Guid.NewGuid()}", config);

        var compiled = EPCompilerProvider.Compiler.Compile(
            "select * from TradeEvent#length(1000)", new CompilerArguments(config));
        _runtime.DeploymentService.Deploy(compiled);

        _events = new TradeEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
            _events[i] = new TradeEvent { Symbol = "MSFT", Price = 100.0 + i, Volume = 1000L, Timestamp = i };
    }

    [Benchmark]
    public void SendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventBean(_events[i], "TradeEvent");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build tst/NEsper.Benchmarks.EndToEnd/NEsper.Benchmarks.EndToEnd.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add tst/NEsper.Benchmarks.EndToEnd/Window/
git commit -m "Add end-to-end benchmarks: time window, length window"
```

---

## Task 12: EndToEnd — Aggregation, Join, Pattern, Deploy

**Files:**
- Create: `tst/NEsper.Benchmarks.EndToEnd/Aggregation/AggregationBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.EndToEnd/Join/TwoStreamJoinBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.EndToEnd/Pattern/SequencePatternBenchmark.cs`
- Create: `tst/NEsper.Benchmarks.EndToEnd/Deploy/DeployUndeployBenchmark.cs`

- [ ] **Step 1: Create AggregationBenchmark.cs**

`tst/NEsper.Benchmarks.EndToEnd/Aggregation/AggregationBenchmark.cs`:
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Aggregation;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class AggregationBenchmark
{
    private EPRuntime    _runtime = null!;
    private TradeEvent[] _events  = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"agg-{Guid.NewGuid()}", config);

        var compiled = EPCompilerProvider.Compiler.Compile(
            "select avg(Price), sum(Volume), count(*) from TradeEvent#time(5 sec)",
            new CompilerArguments(config));
        _runtime.DeploymentService.Deploy(compiled);

        _events = new TradeEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
            _events[i] = new TradeEvent { Symbol = "MSFT", Price = 100.0 + (i % 100), Volume = 1000L + i, Timestamp = i };
    }

    [Benchmark]
    public void SendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventBean(_events[i], "TradeEvent");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
```

- [ ] **Step 2: Create TwoStreamJoinBenchmark.cs**

`tst/NEsper.Benchmarks.EndToEnd/Join/TwoStreamJoinBenchmark.cs`:
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Join;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class TwoStreamJoinBenchmark
{
    private static readonly string[] Symbols = { "AAPL", "GOOG", "MSFT", "AMZN", "NFLX" };

    private EPRuntime    _runtime     = null!;
    private TradeEvent[] _tradeEvents = null!;
    private QuoteEvent[] _quoteEvents = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        config.Common.AddEventType(typeof(QuoteEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"join-{Guid.NewGuid()}", config);

        var compiled = EPCompilerProvider.Compiler.Compile(
            "select t.Symbol, t.Price, q.BidPrice, q.AskPrice " +
            "from TradeEvent#length(100) as t, QuoteEvent#length(100) as q " +
            "where t.Symbol = q.Symbol",
            new CompilerArguments(config));
        _runtime.DeploymentService.Deploy(compiled);

        _tradeEvents = new TradeEvent[EventCount];
        _quoteEvents = new QuoteEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
        {
            var sym = Symbols[i % Symbols.Length];
            _tradeEvents[i] = new TradeEvent { Symbol = sym, Price = 100.0 + i, Volume = 1000L, Timestamp = i };
            _quoteEvents[i] = new QuoteEvent { Symbol = sym, BidPrice = 99.5 + i, AskPrice = 100.5 + i };
        }
    }

    [Benchmark]
    public void SendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
        {
            svc.SendEventBean(_tradeEvents[i], "TradeEvent");
            svc.SendEventBean(_quoteEvents[i], "QuoteEvent");
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
```

- [ ] **Step 3: Create SequencePatternBenchmark.cs**

`tst/NEsper.Benchmarks.EndToEnd/Pattern/SequencePatternBenchmark.cs`:
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Pattern;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class SequencePatternBenchmark
{
    private EPRuntime    _runtime = null!;
    private TradeEvent[] _events  = null!;

    [Params(1_000, 10_000)]
    public int EventCount;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime = EPRuntimeProvider.GetRuntime($"pattern-{Guid.NewGuid()}", config);

        var compiled = EPCompilerProvider.Compiler.Compile(
            "select a.Price as aPrice, b.Price as bPrice " +
            "from pattern [every (a=TradeEvent -> b=TradeEvent(Price > a.Price))]",
            new CompilerArguments(config));
        _runtime.DeploymentService.Deploy(compiled);

        _events = new TradeEvent[EventCount];
        for (int i = 0; i < EventCount; i++)
            _events[i] = new TradeEvent { Symbol = "MSFT", Price = 100.0 + (i % 50), Volume = 1000L, Timestamp = i };
    }

    [Benchmark]
    public void SendN()
    {
        var svc = _runtime.EventService;
        for (int i = 0; i < EventCount; i++)
            svc.SendEventBean(_events[i], "TradeEvent");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _runtime.DeploymentService.UndeployAll();
        _runtime.Destroy();
    }
}
```

- [ ] **Step 4: Create DeployUndeployBenchmark.cs**

`tst/NEsper.Benchmarks.EndToEnd/Deploy/DeployUndeployBenchmark.cs`:
```csharp
// PERF_REVIEW: M5 — FilterServiceLockCoarse write-lock blocks all event evaluation during deploy/undeploy
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Benchmarks.EndToEnd.Deploy;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount: 3, iterationCount: 10)]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 10)]
public class DeployUndeployBenchmark
{
    private EPRuntime  _runtime  = null!;
    private EPCompiled _compiled = null!;
    private TradeEvent _event    = null!;
    private string?    _deployId;

    [GlobalSetup]
    public void Setup()
    {
        var config = new Configuration();
        config.Common.AddEventType(typeof(TradeEvent));
        _runtime  = EPRuntimeProvider.GetRuntime($"deploy-{Guid.NewGuid()}", config);
        _compiled = EPCompilerProvider.Compiler.Compile(
            "select * from TradeEvent where Price > 100.0",
            new CompilerArguments(config));
        _event = new TradeEvent { Symbol = "MSFT", Price = 105.0, Volume = 1000L, Timestamp = 0L };
    }

    [Benchmark]
    public void DeployProcessUndeploy()
    {
        var deployment = _runtime.DeploymentService.Deploy(_compiled);
        _deployId = deployment.DeploymentId;

        var svc = _runtime.EventService;
        for (int i = 0; i < 100; i++)
            svc.SendEventBean(_event, "TradeEvent");

        _runtime.DeploymentService.Undeploy(_deployId);
        _deployId = null;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (_deployId != null)
            _runtime.DeploymentService.Undeploy(_deployId);
        _runtime.Destroy();
    }
}
```

- [ ] **Step 5: Build**

```bash
dotnet build tst/NEsper.Benchmarks.EndToEnd/NEsper.Benchmarks.EndToEnd.csproj
```
Expected: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add tst/NEsper.Benchmarks.EndToEnd/Aggregation/ \
        tst/NEsper.Benchmarks.EndToEnd/Join/ \
        tst/NEsper.Benchmarks.EndToEnd/Pattern/ \
        tst/NEsper.Benchmarks.EndToEnd/Deploy/
git commit -m "Add end-to-end benchmarks: aggregation, join, pattern, deploy/undeploy"
```

---

## Task 13: Smoke test and gitignore

- [ ] **Step 1: List micro benchmarks**

```bash
dotnet run -c Release --project tst/NEsper.Benchmarks.Micro -- --list flat
```
Expected: 13 benchmark classes listed (TrackedDisposable, SlimRWLock, LinkedHashMap, DoubleRange, FilterServiceBaseEval, OLDView, StringRange, Boxing, Scheduling, ThreadLocal, AtomicLong, InstrumentationHelper).

- [ ] **Step 2: List end-to-end benchmarks**

```bash
dotnet run -c Release --project tst/NEsper.Benchmarks.EndToEnd -- --list flat
```
Expected: 12 benchmark classes listed (Passthrough, EqualityFilter, ComparisonFilter, RangeFilter, BooleanExprFilter, StringFilter, TimeWindow, LengthWindow, Aggregation, TwoStreamJoin, SequencePattern, DeployUndeploy).

- [ ] **Step 3: Run one micro benchmark (quick check)**

```bash
dotnet run -c Release --project tst/NEsper.Benchmarks.Micro -- --filter *AtomicLong* --job short
```
Expected: BDN table showing `Current_CasSpinLoop` and `Improved_InterlockedIncrement` with ns/op and Allocated columns.

- [ ] **Step 4: Run one end-to-end benchmark (quick check)**

```bash
dotnet run -c Release --project tst/NEsper.Benchmarks.EndToEnd -- --filter *Passthrough* --job short
```
Expected: BDN table showing `SendN` with ns/op timing for EventCount=1000 and EventCount=10000.

- [ ] **Step 5: Add gitignore entries**

Append to `.gitignore` (do not remove existing entries):
```
BenchmarkDotNet.Artifacts/
BenchmarkResults/
```

- [ ] **Step 6: Commit**

```bash
git add .gitignore
git commit -m "Add BenchmarkDotNet artifact directories to .gitignore"
```
