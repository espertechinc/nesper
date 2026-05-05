# Logging Respec: Replace Common.Logging with Microsoft.Extensions.Logging.Abstractions

## Motivation

`Common.Logging` / `Common.Logging.Core` (last released 2017, effectively abandoned) is being
flagged by Dependabot and is a transitive source of ambiguity with the `log4net` ecosystem.
Replace with `Microsoft.Extensions.Logging.Abstractions` (MEL), the modern .NET standard.

## Architecture (Current State)

```
[All src/ code]
    → com.espertech.esper.compat.logging.ILog       (NEsper's own interface, NEsper.Compat)
    → com.espertech.esper.compat.logging.LogManager (NEsper's own factory, NEsper.Compat)
         ↓ default factory wires to:
    → LogCommon (wraps Common.Logging.ILog)          (NEsper.Compat)  ← REMOVE
         ↓ which wraps:
    → Common.Logging.ILog / LogManager               (external)       ← REMOVE

[NEsper.Log.NLog]
    → LoggerNLog.Register() rewires LogManager.Factory → LoggerNLog (wraps NLog.ILogger)
    → When registered, Common.Logging is bypassed entirely at runtime
```

## Architecture (Target State)

```
[All src/ code]
    → com.espertech.esper.compat.logging.ILog       (unchanged)
    → com.espertech.esper.compat.logging.LogManager (updated default factory)
         ↓ default factory wires to:
    → LogMEL (wraps Microsoft.Extensions.Logging.ILogger)  ← NEW
         ↓ backed by:
    → ILoggerFactory (settable; defaults to NullLoggerFactory.Instance)

[NEsper.Log.NLog]
    → LoggerNLog.Register() unchanged — still rewires LogManager factories
```

## Direct Common.Logging Violations (files that bypass the shim)

| File | Type |
|---|---|
| `src/NEsper.Common/.../VariableUtil.cs` | `using Common.Logging;` + `LogManager.GetLogger` |
| `src/NEsper.Runtime/.../TimerUnitMultiple.cs` | `using Common.Logging;` + `LogManager.GetLogger` |
| `tst/NEsper.Regression/.../MultithreadFireAndForgetIndex.cs` | `using Common.Logging;` |

Note: All other `tst/` hits for `Common.Logging` are `configuration.Common.Logging` (NEsper's
own config object — unrelated to the package).

## Phases

### Phase 1 — Audit (COMPLETE)
- [x] Map shim architecture (`ILog`, `LogCommon`, `LogManager`, `LoggerNLog`)
- [x] Identify all direct `Common.Logging` package imports across `src/` and `tst/`
- [x] Confirm `tst/` false positives (`configuration.Common.Logging` is NEsper config, not the package)
- [x] Record findings in this document

### Phase 2 — Rewrite NEsper.Compat logging shim
- [ ] Add `Microsoft.Extensions.Logging.Abstractions` to `NEsper.Compat.csproj`
- [ ] Remove `Common.Logging` and `Common.Logging.Core` from `NEsper.Compat.csproj`
- [ ] Delete `LogCommon.cs` (wraps `Common.Logging.ILog` — no longer needed)
- [ ] Create `LogMEL.cs` — implements `ILog` by wrapping `Microsoft.Extensions.Logging.ILogger`
      - Map `Fatal` / `IsFatalEnabled` → `LogLevel.Critical` (MEL has no Fatal)
- [ ] Rewrite `LogManager.cs` — replace static constructor default factory:
      - Hold a settable `ILoggerFactory` (default: `NullLoggerFactory.Instance`)
      - `FactoryLoggerFromType` / `FactoryLoggerFromName` delegates remain unchanged in signature

### Phase 3 — Fix direct Common.Logging import violations
- [ ] `VariableUtil.cs` — remove `using Common.Logging;`, qualify to `compat.logging.LogManager`
- [ ] `TimerUnitMultiple.cs` — same fix
- [ ] `MultithreadFireAndForgetIndex.cs` (tst) — remove `using Common.Logging;`, fix reference

### Phase 4 — Remove Common.Logging from NEsper.Common.csproj
- [ ] Remove `Common.Logging` 3.4.1 and `Common.Logging.Core` 3.4.1 from `NEsper.Common.csproj`
      (were already redundant; after Phase 2 they no longer exist anywhere)

### Phase 5 — Update NEsper.Log.NLog (optional enhancement)
- [ ] `LoggerNLog.Register()` already works unchanged — no functional change required
- [ ] Optionally add `NLog.Extensions.Logging` to enable MEL `ILoggerFactory` integration pattern

### Phase 6 — Build and verify
- [ ] `dotnet build NEsperAll.sln` — zero errors
- [ ] `dotnet list NEsperAll.sln package --include-transitive | Select-String "Common.Logging"` — empty
- [ ] Run `NEsper.Compat.Tests`, `NEsper.Common.Tests`, `NEsper.Runtime.Tests`

### Phase 7 — Commit
- [ ] Stage all changes and commit with descriptive message

## Risk Notes

| Item | Risk | Mitigation |
|---|---|---|
| `IsFatalEnabled` / `Fatal()` | Medium — MEL has no Fatal level | Map to `LogLevel.Critical` in `LogMEL` |
| `LogManager` static init | Low — factory delegates replaceable by design | Default to `NullLoggerFactory` (safe no-op) |
| 2 direct `Common.Logging` imports in src/ | Low — trivial `using` swap, API identical | Covered in Phase 3 |
| 1 direct `Common.Logging` import in tst/ | Low | Covered in Phase 3 |
| `LoggerNLog.Register()` callers | None — no signature change | No callers need updating |
