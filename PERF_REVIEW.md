# NEsper 8.9.x — Performance Review & Remediation Plan

**Review date**: 2026-04-19  
**Branch**: performance-respec  
**Reviewer**: Claude Code (claude-sonnet-4-6)

---

## Executive Summary

The dominant performance issue is **per-event heap allocation from the lock infrastructure**. Every `ILockable.Acquire()` call — the standard locking idiom throughout the runtime — allocates a `TrackedDisposable` on the heap. At minimum 4 such allocations occur per event in the hot filter path, generating sustained GC pressure proportional to throughput. Secondary concerns include: a LINQ-based enumerator in `LinkedHashMap`, `OrderedListDictionaryView` allocations across all comparison and range filter indexes, `DoubleRange`/`StringRange` class allocations per range-filter event, and **boxing of numeric property values** on every equality and comparison filter lookup.

Beyond allocation: the coarse filter-service lock serializes all concurrent event evaluation behind a single `ReaderWriterLockSlim`, `InstrumentationHelper.ENABLED` is a static property rather than a field/const so the JIT cannot eliminate the guarded dead code, and `_threadLocals.GetOrCreate()` is called redundantly multiple times per event dispatch path.

All findings with remediation are listed below, sorted from highest to lowest impact.

---

## Findings & Remediation Plan

### [H1] — HIGH PRIORITY — Heap allocation per lock acquisition (`TrackedDisposable`)

**Files**:
- `src/NEsper.Compat/compat/threading/locks/CommonReadLock.cs:17`
- `src/NEsper.Compat/compat/threading/locks/MonitorSlimLock.cs:87`

**Problem**:  
Every `using (lock.Acquire())` call creates a new `TrackedDisposable` heap object. In the event evaluation critical path, this occurs at minimum 4 times per event:
1. `EPEventServiceImpl.cs:389` — `EventProcessingRWLock`
2. `FilterServiceLockCoarse.cs:53` — filter service outer lock
3. `EventTypeIndex.cs:178` — event-type subtree read lock
4. `FilterParamIndexEquals.cs:54` — constants-map read lock

`TrackedDisposable.Dispose()` also issues `Interlocked.Exchange` on every call, adding an unnecessary CAS barrier alongside the allocation.

**Remediation**:  
Replace the `IDisposable`-based lock scope with a zero-allocation mechanism:

Option A (preferred for .NET 9): Use the new `Lock.EnterScope()` / `ReaderWriterLockSlim` equivalent returning a `ref struct` scope.

Option B (compatible): Expose `try/finally` wrappers and migrate hot-path callers:
```csharp
// Before:
using (eventTypesRWLock.ReadLock.Acquire()) { ... }

// After:
eventTypesRWLock.AcquireReaderLock(timeout);
try { ... }
finally { eventTypesRWLock.ReleaseReaderLock(); }
```

Option C (struct disposable): Create an `ILockScope` struct that wraps the release action — zero heap allocation because structs are stack-allocated:
```csharp
public readonly struct LockScope : IDisposable {
    private readonly ILockable _lock;
    internal LockScope(ILockable lk) { _lock = lk; }
    public void Dispose() => _lock.Release();
}
```

**Effort**: Medium — affects lock infrastructure + all hot-path call sites  
**Impact**: Eliminates ≥4 gen-0 GC objects per event; critical for high-throughput workloads

---

### [H2] — HIGH PRIORITY — `LinkedHashMap.GetEnumerator()` allocates LINQ enumerator per iteration

**File**: `src/NEsper.Compat/compat/collections/LinkedHashMap.cs:571`

**Problem**:  
```csharp
return _hashList.Select(subPair => new KeyValuePair<TK, TV>(...)).GetEnumerator();
```
Every `foreach` on a `LinkedHashMap` allocates a `SelectEnumerableIterator` heap object. This directly impacts `FilterParamIndexBooleanExpr.MatchEvent()` (`FilterParamIndexBooleanExpr.cs:95`), which iterates the evaluators map on every event reaching a boolean-expression filter.

**Remediation**:  
Implement a custom struct enumerator:
```csharp
public struct Enumerator : IEnumerator<KeyValuePair<TK, TV>>
{
    private LinkedListNode<Pair<TK, TV>> _current;
    internal Enumerator(LinkedList<Pair<TK, TV>> list) { _current = list.First; ... }
    public bool MoveNext() { _current = _current?.Next; return _current != null; }
    public KeyValuePair<TK, TV> Current => new KeyValuePair<TK, TV>(_current.Value.First, _current.Value.Second);
    // ...
}
public Enumerator GetEnumerator() => new Enumerator(_hashList);
IEnumerator<KeyValuePair<TK, TV>> IEnumerable<KeyValuePair<TK, TV>>.GetEnumerator() => GetEnumerator();
```
Also fix `LinkedHashMap.Keys` (line 314) and `Values` (line 405) properties which call `.ToList()` on every access — replace with lazy-enumerable views.

**Effort**: Low-Medium  
**Impact**: Eliminates 1 heap object per boolean-expression filter evaluation per event

---

### [H3] — HIGH PRIORITY — `FilterParamIndexDoubleRange` allocates two `DoubleRange` heap objects per event

**File**: `src/NEsper.Runtime/internal/filtersvcimpl/FilterParamIndexDoubleRange.cs:59–60`

**Problem**:  
```csharp
var rangeStart = new DoubleRange(attributeValue - LargestRangeValueDouble, attributeValue);
var rangeEnd   = new DoubleRange(attributeValue, double.MaxValue);
var subMap = Ranges.Between(rangeStart, true, rangeEnd, true);
```
`DoubleRange` is a class (`common/internal/filterspec/DoubleRange.cs:18`). Two heap objects are created on every range filter evaluation solely to bound a submap query; they do not escape the method.

**Remediation**:  
Convert `DoubleRange` to a `readonly struct`. This is the simplest fix and makes the two range-bound variables stack-allocated. Verify that `DoubleRange` instances stored as dictionary keys are still correctly boxed (they will be, as `IDictionary<DoubleRange, ...>` will box struct keys). Alternatively, if struct conversion is too broad a change, introduce thread-local pre-allocated `DoubleRange` query-boundary instances and reuse them by mutating fields before the submap query.

**Effort**: Low-Medium (struct conversion) or Low (thread-local reuse)  
**Impact**: Eliminates 2 heap objects per event per range-filter statement

---

### [H4] — HIGH PRIORITY — `FilterServiceBase.EvaluateInternal(statementId)` allocates `ArrayDeque` per call

**File**: `src/NEsper.Runtime/internal/filtersvcimpl/FilterServiceBase.cs:159`

**Problem**:  
```csharp
var allMatches = new ArrayDeque<FilterHandle>();
RetryableMatchEvent(theEvent, allMatches, ctx);
```
The single-statement filter evaluation overload allocates a new 256-element-capacity `ArrayDeque` on every call, bypassing the thread-local reuse used by the primary overload (`MatchesArrayThreadLocal`).

**Remediation**:  
Add a second thread-local `ArrayDeque<FilterHandle>` to `EPEventServiceThreadLocalEntry` for use in single-statement evaluation, or reuse the existing `MatchesArrayThreadLocal` and filter the results post-hoc:
```csharp
// Reuse existing thread-local, filter after:
var allMatches = threadLocalEntry.MatchesArrayThreadLocal;
RetryableMatchEvent(theEvent, allMatches, ctx);
foreach (var match in allMatches) {
    if (match.StatementId == statementId) matches.Add(match);
}
allMatches.Clear();
```

**Effort**: Low  
**Impact**: Eliminates 1 large heap allocation per single-statement filter evaluation call

---

### [M1] — MEDIUM — `SlimReaderWriterLock` uses `LockRecursionPolicy.SupportsRecursion` everywhere

**File**: `src/NEsper.Compat/compat/threading/locks/SlimReaderWriterLock.cs:31`

**Problem**:  
```csharp
_rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
```
`SupportsRecursion` makes `ReaderWriterLockSlim` track per-thread state via a `[ThreadStatic]` lookup on every acquire and release. `NoRecursion` eliminates this. Since this is the default factory (`ContainerInitializer.cs:136`), every filter index lock and event-type index lock carries this overhead. No observed call sites require reentrancy.

**Remediation**:  
Change constructor default to `LockRecursionPolicy.NoRecursion`. If any specific site proves it needs recursion, introduce a `SlimReentrantReaderWriterLock` subtype.

**Effort**: Very low  
**Impact**: Reduces overhead on every lock acquire/release across all default RW locks

---

### [M2] — MEDIUM — `SchedulingServiceImpl.Evaluate()` allocates `new List<long>()` per timer tick

**File**: `src/NEsper.Runtime/internal/schedulesvcimpl/SchedulingServiceImpl.cs:152`

**Problem**:  
```csharp
IList<long> removeKeys = new List<long>();
```
Allocated inside the `lock(this)` block on every timer evaluation, even when there are schedules to fire.

**Remediation**:  
Promote to a reusable instance field:
```csharp
private readonly List<long> _removeKeys = new List<long>();
// In Evaluate():
_removeKeys.Clear();
foreach (var entry in headMap) { _removeKeys.Add(entry.Key); ... }
```
Safe because `Evaluate()` is `lock(this)`-guarded.

**Effort**: Trivial  
**Impact**: Eliminates 1 heap object per timer tick that fires scheduled callbacks

---

### [M3] — MEDIUM — `SchedulingServiceImpl.FurthestTimeHandle` uses LINQ `Last()` on ordered keys

**File**: `src/NEsper.Runtime/internal/schedulesvcimpl/SchedulingServiceImpl.cs:232`

**Problem**:  
```csharp
return _timeHandleMap.Keys.Last();
```
`OrderedListDictionary` is backed by `List<KeyValuePair<TK,TV>>`. `.Keys` returns a view object; `.Last()` may trigger an O(n) LINQ scan unless `IList<T>` is detected by the optimizer. This is called by `EPEventServiceImpl.AdvanceTimeSpanInternal()` on every step of a time-span advance.

**Remediation**:  
Add a `LastKey` helper to `OrderedListDictionary`:
```csharp
public TK LastKey => _itemList[_itemList.Count - 1].Key; // O(1)
```
Update `SchedulingServiceImpl.FurthestTimeHandle` and `NearestTimeHandle` to use it.

**Effort**: Low  
**Impact**: O(1) vs O(n) for time-span advancement; important for simulations with many time steps

---

### [L1] — LOW — Redundant double-assignment in `EPEventServiceImpl` constructor

**File**: `src/NEsper.Runtime/internal/kernel/service/EPEventServiceImpl.cs:85–88`

**Problem**:  
`_isUsingExternalClocking` and `_isPrioritized` are each assigned twice in sequence (lines 85–86 then 87–88) from the same source. No functional defect today, but a maintenance trap.

**Remediation**: Delete lines 85–86.

**Effort**: Trivial

---

### [L2] — LOW — `AtomicLong.GetAndIncrement()` uses CAS spin loop instead of `Interlocked.Increment - 1`

**File**: `src/NEsper.Compat/compat/AtomicLong.cs:62–68`

**Problem**:  
Manual CAS loop is correct but retries under contention. `Interlocked.Increment` is unconditional.

**Remediation**:  
```csharp
public long GetAndIncrement() => Interlocked.Increment(ref _value) - 1;
```

**Effort**: Trivial  
**Impact**: Minor; eliminates retry path under contention

---

### [H5] — HIGH PRIORITY — `OrderedListDictionaryView` allocated per event in comparison/range filter indexes

**Files**:
- `src/NEsper.Runtime/internal/filtersvcimpl/FilterParamIndexCompare.cs:168–172`
- `src/NEsper.Runtime/internal/filtersvcimpl/FilterParamIndexCompareString.cs:103–107`
- `src/NEsper.Runtime/internal/filtersvcimpl/FilterParamIndexDoubleRange.cs:62` (additive to H3)
- `src/NEsper.Runtime/internal/filtersvcimpl/FilterParamIndexStringRange.cs:61`

**Problem**:  
Every call to `constantsMap.Head(value)`, `constantsMap.Tail(value)`, or `Ranges.Between(start, ..., end, ...)` in `MatchEvent()` allocates a new `OrderedListDictionaryView<TK, TV>` heap object (`OrderedListDictionary.cs:435, 452, 469`). This view is a trivial wrapper around a `BoundRange` and the parent list — it exists solely to support the subsequent `foreach`. Iterating `.Values` on the view further allocates an `OrderedListDictionaryValues` enumerator. Combined: **2 heap objects per event** for each active comparison or range filter.

For `FilterParamIndexDoubleRange` this is a *third* allocation per event on top of the two `DoubleRange` objects already noted in H3.

**Remediation**:  
Add non-allocating enumeration helpers to `OrderedListDictionary` that accept index bounds directly:

```csharp
// In OrderedListDictionary<TK, TV>:
public void ForEachValueInHead(TK value, bool inclusive, Action<TV> visitor)
{
    int limit = GetHeadIndex(value, inclusive);
    for (int i = 0; i <= limit; i++)
        visitor(_itemList[i].Value);
}

public void ForEachValueInTail(TK value, bool inclusive, Action<TV> visitor)
{
    int start = GetTailIndex(value, inclusive);
    for (int i = start; i < _itemList.Count; i++)
        visitor(_itemList[i].Value);
}

public void ForEachInBetween(TK lo, bool loInclusive, TK hi, bool hiInclusive,
    Action<KeyValuePair<TK, TV>> visitor)
{
    int start = GetTailIndex(lo, loInclusive);
    int limit = GetHeadIndex(hi, hiInclusive);
    for (int i = start; i <= limit; i++)
        visitor(_itemList[i]);
}
```

Replace `subMap = Head/Tail/Between(...)` + `foreach (subMap.Values)` with a single call to the appropriate helper. This eliminates the view and enumerator allocations entirely.

**Effort**: Low-Medium  
**Impact**: Eliminates 2 GC objects per event per active comparison or range filter; additive benefit on top of H3

---

### [H6] — HIGH PRIORITY — `FilterParamIndexStringRange.MatchEvent()` allocates two `StringRange` class objects per event

**File**: `src/NEsper.Runtime/internal/filtersvcimpl/FilterParamIndexStringRange.cs:59–60`

**Problem**:  
```csharp
var rangeStart = new StringRange(null, attributeValue);
var rangeEnd   = new StringRange(attributeValue, null);
var subMap = Ranges.Between(rangeStart, true, rangeEnd, true);
```
`StringRange` is a class (`common/internal/filterspec/StringRange.cs`). Two heap objects are allocated on every event solely to bound the `Between()` submap query — identical pattern to H3's `DoubleRange`. They do not escape `MatchEvent()`.

**Remediation**:  
Same two options as H3: (a) convert `StringRange` to a `readonly struct`, or (b) add a `ForEachInBetween` overload to `OrderedListDictionary<StringRange, EventEvaluator>` that accepts raw string boundary values and compares inline, bypassing any range object:

```csharp
// Preferred: leverage H5's ForEachInBetween with dummy-struct bounds,
// or add a dedicated overload:
Ranges.ForEachMatchingStringRange(attributeValue, FilterOperator, (entry) =>
    entry.Value.MatchEvent(theEvent, matches, ctx));
```

**Effort**: Low-Medium  
**Impact**: Eliminates 2 additional GC objects per event for string-range filter statements

---

### [L3] — LOW — `FilterParamIndexCompare.UpdateBounds()` uses LINQ `First()`/`Last()` when O(1) properties exist

**File**: `src/NEsper.Runtime/internal/filtersvcimpl/FilterParamIndexCompare.cs:222–223`

**Problem**:  
```csharp
lowerBounds = constantsMap.Keys.First().AsDouble();
upperBounds = constantsMap.Keys.Last().AsDouble();
```
`constantsMap.Keys` returns an `OrderedListDictionaryKeys` view object; `.First()` / `.Last()` are LINQ extension methods that allocate an enumerator and scan the collection. `OrderedListDictionary` already exposes `FirstEntry` and `LastEntry` properties that are O(1) direct `_itemList[0]` / `_itemList[Count-1]` accesses.

**Remediation**:  
```csharp
lowerBounds = constantsMap.FirstEntry.Key.AsDouble();
upperBounds = constantsMap.LastEntry.Key.AsDouble();
```

**Effort**: Trivial  
**Impact**: Eliminates enumerator allocation on every filter `Remove()` call; also makes `UpdateBounds` O(1) instead of potentially O(n)

---

### [M4] — MEDIUM — Boxing of numeric property values on every equality and comparison filter lookup

**Files**:
- `src/NEsper.Runtime/internal/filtersvcimpl/FilterParamIndexEqualsBase.cs:23`
- `src/NEsper.Runtime/internal/filtersvcimpl/FilterParamIndexCompare.cs:46`

**Problem**:  
Both indexes store filter constants and look up event property values using `object` keys:
```csharp
// FilterParamIndexEqualsBase
IDictionary<object, EventEvaluator> ConstantsMap = new Dictionary<object, EventEvaluator>()...

// FilterParamIndexCompare
IOrderedDictionary<object, EventEvaluator> constantsMap = new OrderedListDictionary<object, EventEvaluator>();
```
When the filtered property is a numeric type (int, long, double — the most common case), the value returned by `Lookupable.Eval.Eval(theEvent, ctx)` is already boxed as `object`. The dictionary/map then compares it against stored `object` keys, which involves `object.Equals()` and `GetHashCode()` virtual dispatch rather than direct numeric comparison. For `FilterParamIndexCompare`, the value is additionally unboxed to `double` for bounds checking (`propertyValue.AsDouble()`) then the original boxed reference is used for the submap lookup — the boxing round-trip adds no value.

Equality is by far the most common filter operator; this path runs on every event for every `=` filter.

**Remediation**:  
At filter-compile time, the `returnType` of `ExprFilterSpecLookupable` is known. Use it to select a type-specialized index implementation:
```csharp
// Factory selects by lookupable return type:
return lookupable.ReturnType switch {
    var t when t == typeof(int)    => new FilterParamIndexEqualsInt(lookupable, lock),
    var t when t == typeof(long)   => new FilterParamIndexEqualsLong(lookupable, lock),
    var t when t == typeof(double) => new FilterParamIndexEqualsDouble(lookupable, lock),
    _                              => new FilterParamIndexEquals(lookupable, lock),  // object fallback
};
```
Each specialized implementation uses `Dictionary<double, EventEvaluator>` etc., so both stored constants and the runtime lookup value are unboxed primitives. The JIT can then use direct numeric equality rather than virtual `object.Equals()`.

**Effort**: Medium (new classes + factory switch; same pattern repeated for Compare)  
**Impact**: Eliminates boxing round-trip on every equality/comparison filter lookup — the most frequently executed filter path

---

### [M5] — MEDIUM — `FilterServiceLockCoarse` write-lock blocks all event evaluation during deploy/undeploy

**File**: `src/NEsper.Runtime/internal/filtersvcimpl/FilterServiceLockCoarse.cs:31–99`

**Problem**:  
`FilterServiceLockCoarse` wraps the entire filter service in a single `IReaderWriterLock`. `Evaluate()` holds the read-lock for the duration of the full filter-tree traversal. `Add()`, `Remove()`, and `RemoveType()` hold the **write** lock, which blocks all concurrent readers. During deployment or undeployment of EPL statements — which can add or remove hundreds of filter handles — every in-flight event evaluation is blocked until the operation completes.

Under steady-state load (no deploys), concurrent reads proceed freely since `ReaderWriterLockSlim` allows multiple simultaneous readers. The contention issue is specifically at deploy/undeploy time.

Note: inner filter indexes use `FilterServiceGranularLockFactoryNone` (no per-index locks), which is correct given the outer lock.

**Remediation**:  
Two options:  
- **Option A (existing infrastructure)**: Switch the default to `FilterServiceLockGranular`, which already exists in the codebase. This uses per-event-type locks instead of a single service-level lock, so deploys targeting one event type do not block evaluation of other types.  
- **Option B**: Implement copy-on-write for the filter tree. Reads use a snapshot; writes replace the snapshot atomically. Eliminates all read-path lock acquisition.

**Effort**: Low (Option A — wiring change) / High (Option B)  
**Impact**: Eliminates event-evaluation stalls during concurrent deploy/undeploy; critical for systems that hot-deploy statements

---

### [L4] — LOW — `InstrumentationHelper.ENABLED` is a property; JIT cannot constant-fold guarded blocks

**File**: `src/NEsper.Runtime/internal/metrics/instrumentation/InstrumentationHelper.cs:13`

**Problem**:  
```csharp
public static bool ENABLED { get; set; } = false;
```
`ENABLED` is a static auto-property. Every `if (InstrumentationHelper.ENABLED)` check in a `MatchEvent()` call (4–8 per index implementation) compiles to a property getter invocation. Because the getter is settable, the JIT cannot prove the value is constant and will not eliminate the dead branch or inline the check to a register-resident value. This happens 4–8 times per `MatchEvent()` across all filter index types.

**Remediation**:  
Change to a static readonly field:
```csharp
public static readonly bool ENABLED = false;
```
Or, if toggling is never needed at runtime, use a `const`:
```csharp
public const bool ENABLED = false;
```
With `const`, the C# compiler itself eliminates all `if (ENABLED) { ... }` blocks at compile time — zero runtime cost. With `readonly`, the JIT hoists the single memory load and eliminates branch prediction pressure.

**Effort**: Trivial (field/const change; if `const`, all callers recompile with dead code stripped)  
**Impact**: Eliminates 4–8 dead-branch checks per MatchEvent; enables better inlining in production builds

---

### [L5] — LOW — `_threadLocals.GetOrCreate()` called redundantly multiple times per event

**File**: `src/NEsper.Runtime/internal/kernel/service/EPEventServiceImpl.cs` (lines 342, 362, 376, 425, 479, 511, 548, 565, 804, 936, 1001)

**Problem**:  
`_threadLocals.GetOrCreate()` is called 10+ times across the event dispatch path (ProcessWrappedEvent → ProcessMatches → AdvanceTime etc.). Each call performs a `[ThreadStatic]` lookup to retrieve the per-thread entry. While `[ThreadStatic]` access is fast, it is not free — it involves an IL helper call or a TLS slot read — and the repeated access within a single event-processing call on the same thread returns the same object every time.

**Remediation**:  
Cache the result at the outermost entry point and pass it down or keep it in a local:
```csharp
// At top of ProcessWrappedEvent:
var tlEntry = _threadLocals.GetOrCreate();
// Use tlEntry.WorkQueue, tlEntry.MatchesArrayThreadLocal, etc. throughout
```
Eliminate all subsequent `GetOrCreate()` calls within the same logical dispatch.

**Effort**: Low (local variable threading through call chain)  
**Impact**: Reduces TLS lookups from ~10 to 1 per event; minor but free speedup

---

## Prioritized Remediation Roadmap

| Priority | ID | File(s) | Est. Effort | Expected Gain |
|----------|----|---------|-------------|---------------|
| 1 | H1 | `CommonReadLock.cs`, `MonitorSlimLock.cs` + callers | Medium | Eliminates ≥4 GC objects/event |
| 2 | H5 | `OrderedListDictionary.cs` + 4 filter index callers | Low-Medium | Eliminates 2 GC objects/event per comparison/range filter |
| 3 | M4 | `FilterParamIndexEqualsBase.cs`, `FilterParamIndexCompare.cs` + factory | Medium | Eliminates boxing on every equality/comparison lookup |
| 4 | H2 | `LinkedHashMap.cs` | Low-Medium | Eliminates 1 GC object/boolean-filter event |
| 5 | H3 | `FilterParamIndexDoubleRange.cs`, `DoubleRange.cs` | Low-Medium | Eliminates 2 GC objects/range-filter event |
| 6 | H6 | `FilterParamIndexStringRange.cs`, `StringRange.cs` | Low-Medium | Eliminates 2 GC objects/string-range event |
| 7 | H4 | `FilterServiceBase.cs:159` | Low | Eliminates 1 large GC object/stmt-eval |
| 8 | M1 | `SlimReaderWriterLock.cs:31` | Very low | Reduces all RW lock overhead |
| 9 | M5 | `FilterServiceLockCoarse.cs` | Low (Option A) | Eliminates deploy/undeploy stalls |
| 10 | M2 | `SchedulingServiceImpl.cs:152` | Trivial | Eliminates 1 GC object/timer tick |
| 11 | M3 | `OrderedListDictionary.cs`, `SchedulingServiceImpl.cs` | Low | O(1) time-step lookup |
| 12 | L1 | `EPEventServiceImpl.cs:85–88` | Trivial | Code hygiene |
| 13 | L2 | `AtomicLong.cs:62–68` | Trivial | Minor contention reduction |
| 14 | L3 | `FilterParamIndexCompare.cs:222–223` | Trivial | O(1) bounds update, eliminates enumerator alloc |
| 15 | L4 | `InstrumentationHelper.cs:13` | Trivial | Eliminates 4–8 dead checks/MatchEvent in prod |
| 16 | L5 | `EPEventServiceImpl.cs` (various) | Low | Reduces TLS lookups from ~10 to 1/event |

**Recommended first step**: H1 (TrackedDisposable elimination) — broadest single impact. M4 (boxing) is elevated to priority 3 because equality filters are the most common filter type and boxing occurs on every single event through that path. H5 (OrderedListDictionaryView) and M4 can be pursued in parallel as they touch different files. L4 (InstrumentationHelper const) is trivial and should be done opportunistically.
