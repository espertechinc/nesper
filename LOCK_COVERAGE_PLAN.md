# Lock Implementation Test Coverage Plan

## Baseline (captured 2026-04-10)

| Metric | Value |
|---|---|
| Line coverage | 40.1% (452 / 1127 coverable) |
| Branch coverage | 45.9% (103 / 224) |
| Method coverage | 31.9% (68 / 213) |
| Tests passing | 86 / 86 |
| Test project | `tst/NEsper.Compat.Tests` |
| Source assembly | `src/NEsper.Compat` (filter: `threading.locks.*`) |

### Per-class baseline

| Class | Line% | Notes |
|---|---|---|
| `FairReaderWriterLock` | 91.2% | Nearly complete |
| `FifoReaderWriterLock` | 86.8% | Nearly complete |
| `DefaultReaderWriterLockManager` | 65.7% | Manager path gaps |
| `SlimReaderWriterLock` | 66.1% | Upgradeable path unused |
| `DefaultLockManager` | 56.6% | Category/telemetry paths untested |
| `CommonWriteLock` / `CommonWriteLock<T>` | 45–50% | `ReleaseAcquire`, `Release` uncovered |
| `CommonReadLock` / `CommonReadLock<T>` | 45–50% | `ReleaseAcquire`, `Release` uncovered |
| `SlimLock` | 24.3% | Contention-path spin code unreachable from current tests |
| `TelemetryEngine` | 4.1% | Only struct accessed; logic untested |
| 13 other classes | 0% | No tests at all |

### Zero-coverage classes

`DummyReaderWriterLock`, `LockableExtensions`, `LockException`,
`MonitorLock`, `MonitorSlimLock`, `MonitorSpinLock` (deprecated),
`StandardReaderWriterLock` (deprecated), `TelemetryLock`,
`TelemetryLockCategory`, `TelemetryProbe`, `TelemetryReaderWriterLock`,
`VoidLock`, `VoidReaderWriterLock`

---

## Coverage target

**~75% line / ~70% branch** after all phases are complete.

---

## Phase 1 — Trivial no-logic classes
**New file:** `compat/threading/locks/VoidAndDummyLockTests.cs`
**Estimated coverage gain:** +5–6% line

These classes have no real concurrency logic; tests just verify the public contract.

### 1a. `VoidLock`
- `Acquire()` returns a non-null `IDisposable`; `Dispose()` on it does not throw
- `Acquire(msec)` same contract
- `ReleaseAcquire()` returns a non-null `IDisposable`
- `Release()` does not throw

### 1b. `VoidReaderWriterLock`
- `ReadLock` and `WriteLock` properties are non-null and the same instance
- `AcquireReadLock()`, `AcquireWriteLock()`, `AcquireWriteLock(TimeSpan)` all return a non-null `IDisposable`
- `ReleaseWriteLock()` does not throw
- `IsWriterLockHeld` is always `false`

### 1c. `DummyReaderWriterLock`
- `ReadLock` and `WriteLock` are the same object (both are the backing `MonitorSlimLock`)
- `AcquireReadLock()`, `AcquireWriteLock()`, `AcquireWriteLock(TimeSpan)` return non-null
- `ReleaseWriteLock()` does not throw
- `IsWriterLockHeld` is always `false`

### 1d. `LockableExtensions`
**New file:** `compat/threading/locks/LockableExtensionsTests.cs`

- `Call(Action)` executes the action exactly once while the lock is held; lock is released after
- `Call<T>(Func<T>)` returns the value produced by the function; lock is released after
- Both overloads work with a `VoidLock` (simplest `ILockable`) and a `MonitorLock`

### 1e. `LockException`
Add to the extensions test file or a new `LockExceptionTests.cs`:

- Default constructor: `new LockException()` — message is null or empty
- Message constructor: `new LockException("msg")` — `Message == "msg"`
- Inner exception constructor: `new LockException("msg", new Exception("inner"))` — both preserved

---

## Phase 2 — Monitor-based `ILockable` implementations
**New file:** `compat/threading/locks/MonitorLockTests.cs`
**Estimated coverage gain:** +9–11% line

`MonitorLock` and `MonitorSlimLock` share the same logical contract. Cover both in the same file.

### 2a. `MonitorLock`

| Test | Scenario |
|---|---|
| Basic acquire/release | `using(lock.Acquire())` completes without exception |
| `Acquire(msec)` overload | explicit millisecond timeout succeeds when uncontested |
| `LockDepth` tracking | depth is 1 after first acquire, 2 after reentrant acquire, 0 after both releases |
| `IsHeldByCurrentThread` | true inside the using block, false after disposal |
| `ReleaseAcquire()` | after acquiring, `ReleaseAcquire()` temporarily releases; the returned disposable re-acquires; outer lock is restored on dispose |
| `Release()` | direct call releases without using |
| Timeout — writer contention | second thread `Acquire()` with short timeout throws `TimeoutException` while first thread holds the lock |
| Reentrant (same-thread) | two nested `Acquire()` calls on the same thread do not deadlock; depth becomes 2 then 1 then 0 |
| `LockTimeout` property | reflects the value passed to the constructor |

### 2b. `MonitorSlimLock`

Same test matrix as `MonitorLock` above.
`MonitorSlimLock` uses `SlimLock.Enter(timeout)` internally; the timeout test here exercises
`EnterMyLockSpin(thread, timeout)` which is the primary path missing from `SlimLock` at 24.3%.

---

## Phase 3 — `SlimLock` spin/contention paths
**New file:** `compat/threading/locks/SlimLockTests.cs`
**Estimated coverage gain:** +5–6% line

`SlimLock` is a low-level spin lock. Most of its untested code lives in the private
`EnterMyLockSpin*` / `EnterMyLockSleep*` helpers that only activate under thread contention.

| Test | Scenario |
|---|---|
| `Enter()` uncontested | Acquires immediately; `Release()` frees it |
| `Enter()` reentrant (same thread) | Second `Enter()` increments depth; first `Release()` keeps lock; second `Release()` frees |
| `Enter(timeout)` uncontested | Returns `true` immediately |
| `Enter(timeout)` — contention, acquires before timeout | Thread A holds lock; thread B calls `Enter(timeout)` with generous timeout; A releases; B returns `true` |
| `Enter(timeout)` — contention, times out | Thread A holds lock; thread B calls `Enter(small timeout)`; A never releases; B returns `false` |
| `SmartWait(iter)` all branches | Call with iter = 1, 15, 45, 75, 110 to exercise each `SpinWait`/`Sleep(0)` branch |
| `SmartWait(iter, timeEnd)` — not expired | Returns `true` for iter values in each band |
| `SmartWait(iter, timeEnd)` — expired at checkpoint | Force iter to a multiple of 10 > 60 with `timeEnd` already in the past; verify returns `false` |

---

## Phase 4 — `CommonReadLock` / `CommonWriteLock` gaps
**New file:** `compat/threading/locks/CommonLockTests.cs`
**Estimated coverage gain:** +4–5% line

Both the non-generic and generic variants have `ReleaseAcquire()` and `Release()` untested.
Use `SlimReaderWriterLock` or `FairReaderWriterLock` as the backing `IReaderWriterLockCommon`.

### 4a. `CommonReadLock` (non-generic, backed by `SlimReaderWriterLock`)

| Test | Scenario |
|---|---|
| `Acquire()` | Returns distinct disposable; dispose releases (writer can acquire after) |
| `Acquire(msec)` | Explicit timeout path succeeds |
| `Release()` | Calling `Release()` directly frees the lock |
| `ReleaseAcquire()` | While holding read lock: `ReleaseAcquire()` releases it (writer can acquire); disposing the returned handle re-acquires the read lock |

### 4b. `CommonWriteLock` (non-generic)

Same four tests as above but using the write side.

### 4c. `CommonReadLock<T>` and `CommonWriteLock<T>` (generic, backed by `FifoReaderWriterLock`)

Mirror the same four tests. The generic variants track `_lockValue` state, so also verify:
- `Release()` sets `_lockValue` to `default`
- `ReleaseAcquire()` sets `_lockValue` to `default` then resets it on re-acquire

---

## Phase 5 — `StandardReaderWriterLock` (deprecated)
**New file:** `compat/threading/locks/StandardReaderWriterLockTests.cs`
**Estimated coverage gain:** +4–5% line

`StandardReaderWriterLock` is marked `[Obsolete]` but is still callable code that converts
`ApplicationException` from the legacy `ReaderWriterLock` API into `TimeoutException`.

| Test | Scenario |
|---|---|
| Read lock — no contention | `AcquireReadLock()` then dispose, no exception |
| Write lock — no contention | `AcquireWriteLock()` then dispose |
| `AcquireWriteLock(TimeSpan)` | TimeSpan overload succeeds when uncontested |
| `IsWriterLockHeld` | False initially, true while write lock held, false after release |
| Reader timeout while writer held | Writer holds; reader calls `ReadLock.Acquire(ms)` — `TimeoutException` |
| Writer timeout while reader held | Reader holds; writer calls `WriteLock.Acquire(ms)` — `TimeoutException` |
| `ReleaseWriteLock()` | Releases write lock; subsequent reader can acquire |

---

## Phase 6 — Telemetry infrastructure
**New file:** `compat/threading/locks/TelemetryTests.cs`
**Estimated coverage gain:** +7–9% line

### 6a. `TelemetryLockCategory`

| Test | Scenario |
|---|---|
| `Name` property | Set via constructor |
| `OnLockReleased` + `Events` | Call `OnLockReleased` with an event; `Events` contains exactly that event |
| Thread-safety smoke test | Two threads concurrently call `OnLockReleased`; both events appear in `Events` |

### 6b. `TelemetryEngine`

| Test | Scenario |
|---|---|
| `GetCategory` — new | Returns a non-null `TelemetryLockCategory` with the correct `Name` |
| `GetCategory` — existing | Second call with same name returns the same instance |
| `CategoryDictionary` | Reflects categories after `GetCategory` calls |
| `Categories` | Enumerates all added categories |
| `DumpTo(TextWriter)` | After adding a category with one event via `TelemetryLockCategory.OnLockReleased`, output contains expected XML elements (`telemetry`, `category`, `event`) |

### 6c. `TelemetryLock`

| Test | Scenario |
|---|---|
| `Acquire()` — no listener | Acquires sub-lock; dispose releases it; no exception when no event handler |
| `Acquire()` — with `LockReleased` handler | Handler fires on dispose with non-zero `RequestTime`, `AcquireTime`, `ReleaseTime` |
| `Acquire(msec)` | Same as above via the timed overload |
| `ReleaseAcquire()` | Temporarily releases sub-lock; handler fires on dispose |
| `Release()` | Releases sub-lock directly |

### 6d. `TelemetryReaderWriterLock`

| Test | Scenario |
|---|---|
| `AcquireReadLock()` | Acquires and releases; `ReadLockReleased` event fires |
| `AcquireWriteLock()` | Acquires and releases; `WriteLockReleased` event fires |
| `AcquireWriteLock(TimeSpan)` | Same via TimeSpan overload |
| `IsWriterLockHeld` | Delegates to sub-lock |
| `ReleaseWriteLock()` | Releases write lock via `WriteLock.Release()` |

---

## Phase 7 — Manager coverage gaps
**New file:** `compat/threading/locks/DefaultLockManagerTests.cs`
Add cases to existing `DefaultReaderWriterLockManagerTests.cs`
**Estimated coverage gain:** +5–7% line

### 7a. `DefaultLockManager`

| Test | Scenario |
|---|---|
| `CreateLock(Type)` | Delegates to `CreateLock(string)` using `Type.FullName` |
| `CreateLock(Func<int,ILockable>)` | Uses the supplied factory ignoring `DefaultLockFactory` |
| `RegisterCategoryLock(Type, factory)` | Category resolves when `CreateLock(Type)` is called for the same type |
| Category prefix fallback | Register `"a.b"`; call `CreateLock("a.b.c")` — falls back to `"a.b"` factory |
| `CreateDefaultLock()` — no factory | `DefaultLockFactory = null` — throws `InvalidOperationException` |
| `IsTelemetryEnabled = true` | `CreateDefaultLock()` returns a `TelemetryLock` wrapper |
| `CreateLock(factory)` with telemetry | Returns a `TelemetryLock` when `IsTelemetryEnabled = true` |

### 7b. `DefaultReaderWriterLockManager` (add to existing test file)

| Test | Scenario |
|---|---|
| `RegisterCategoryLock<T>(factory)` | Generic overload registers with `typeof(T).FullName` |
| `RegisterCategoryLock(Type, factory)` | Delegates to string overload |
| `CreateLock(Type)` — generic type name stripping | Pass a `List<int>` type; the backtick suffix is stripped before lookup |
| `CreateLock(Func<int,IReaderWriterLock>)` | Uses supplied factory |
| Category prefix fallback | Register `"a.b"`; `CreateLock("a.b.c.d")` falls back through segments |
| `CreateDefaultLock()` — no factory | `DefaultLockFactory = null` — throws `InvalidOperationException` |
| `IsTelemetryEnabled = true` + `CreateDefaultLock()` | Returns a `TelemetryReaderWriterLock`; events fire on lock/release |

---

## Phase 8 — `MonitorSpinLock` smoke tests (deprecated)
Add to `MonitorLockTests.cs`
**Estimated coverage gain:** +2–3% line

`MonitorSpinLock` is `[Obsolete]`. Minimal coverage to catch future regressions.
Mark the test class with `#pragma warning disable CS0618` to suppress the deprecation warning.

| Test | Scenario |
|---|---|
| Basic acquire/release | `using(lock.Acquire())` succeeds |
| `Acquire(msec)` | Succeeds when uncontested |
| `IsHeldByCurrentThread` | `true` inside, `false` after |
| `LockDepth` | 1 inside acquire block |
| `ReleaseAcquire()` | Temporarily releases; re-acquires on dispose |
| `Release()` | Direct release |
| Timeout under contention | Short timeout — `TimeoutException` when another thread holds it |

---

## Execution order and file map

| Phase | Priority | New test file | Existing file modified |
|---|---|---|---|
| 1 | High | `VoidAndDummyLockTests.cs` | — |
| 1d–e | High | `LockableExtensionsTests.cs` | — |
| 2 | High | `MonitorLockTests.cs` | — |
| 3 | Medium | `SlimLockTests.cs` | — |
| 4 | Medium | `CommonLockTests.cs` | — |
| 5 | Low | `StandardReaderWriterLockTests.cs` | — |
| 6 | Medium | `TelemetryTests.cs` | — |
| 7a | Medium | `DefaultLockManagerTests.cs` | — |
| 7b | Medium | — | `DefaultReaderWriterLockManagerTests.cs` |
| 8 | Low | — | `MonitorLockTests.cs` |

All new files go under `tst/NEsper.Compat.Tests/compat/threading/locks/`.

---

## Coverage uplift estimates

| After phase | Projected line% |
|---|---|
| Baseline | 40.1% |
| + Phase 1 (trivial no-logic) | ~46% |
| + Phase 2 (Monitor locks) | ~56% |
| + Phase 3 (SlimLock spin) | ~61% |
| + Phase 4 (Common locks) | ~65% |
| + Phase 5 (Standard RW lock) | ~69% |
| + Phase 6 (Telemetry) | ~76% |
| + Phase 7 (Managers) | ~80% |
| + Phase 8 (MonitorSpinLock) | ~82% |

---

## How to re-run coverage

```powershell
# From repo root — run only lock tests with coverage
dotnet test tst\NEsper.Compat.Tests\NEsper.Compat.Tests.csproj `
  --filter "FullyQualifiedName~compat.threading.locks" `
  --collect:"XPlat Code Coverage" `
  --results-directory TestResults\LockCoverage `
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

# Generate HTML report (requires reportgenerator global tool)
reportgenerator `
  -reports:"TestResults\LockCoverage\**\coverage.cobertura.xml" `
  -targetdir:"TestResults\LockCoverage\html" `
  -reporttypes:"Html;TextSummary" `
  -classfilters:"+com.espertech.esper.compat.threading.locks.*" `
  -title:"Lock Implementations Coverage"

# Open report
Start-Process "TestResults\LockCoverage\html\index.html"
```

---

## Out of scope

- `TelemetryProbe` — a two-line data struct with no logic; coverage is noise
- Concurrency stress / chaos testing (race detectors, Helgrind) — separate concern
- Integration tests that drive lock implementations indirectly via EPL statements
