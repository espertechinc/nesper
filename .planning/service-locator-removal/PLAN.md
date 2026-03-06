# Service Locator Removal Plan
## NEsper 8.9.x — IContainer Anti-Pattern Elimination

**Branch:** `service-locator-refactor` / `service-locator-refactor-testing`
**Date:** 2026-03-05
**Status:** Phases 1–8 nearly complete; 3 small tasks remain before merge

### Phase Completion Summary (as of 2026-03-05)

| Phase | Status | Notes |
|-------|--------|-------|
| 1 – Threading Primitives | ✅ COMPLETE | StatementContext, StatementContextRuntimeServices clean |
| 2 – Type System Services | ✅ COMPLETE | ArtifactTypeResolverProvider, ImportServiceBase, ScriptCompilerImpl clean |
| 3 – Compilation Services | ✅ COMPLETE | ModuleCompileTimeServices, StatementSpecMapEnv clean |
| 4 – Event Type System | ✅ COMPLETE | EventTypeFactoryImpl, BeanEventType, EventTypeCollectorImpl clean |
| 5 – Utility/Infrastructure | ✅ COMPLETE | DataflowInstantiator, EPDataFlowServiceImpl, SerializerUtil, DbProviderFactoryManagerDefault clean |
| 6 – Runtime Context | ✅ COMPLETE | EPServicesContext has no _container field; StageRuntimeServices clean |
| 7 – Entry Points | ✅ COMPLETE | EPRuntimeImpl.Container marked [Obsolete]; factory uses explicit Resolve<T>() |
| 8 – Cleanup | 🔄 IN PROGRESS | See remaining tasks below |

---

## Background and Scope

The codebase currently uses `IContainer` as a service locator in 33+ classes. The `ContainerExtensions`
class provides extension methods (`container.LockManager()`, `container.RWLockManager()`,
`container.ThreadLocalManager()`, etc.) that resolve singletons from the container at arbitrary call
sites deep in object graphs. This is the service locator anti-pattern.

**Target state:** The container is used only at application startup to register and wire singletons.
After startup, all services are passed explicitly through constructor injection or method parameters.
No runtime code calls `container.Resolve<T>()`.

**Key insight:** `StatementContext` currently stores `IContainer` and exposes `LockManager`,
`RWLockManager`, and `ThreadLocalManager` as computed properties that call container extension
methods. These three threading primitives appear in 16 files and are the most pervasive call source.
The cleanest path is to add these three fields to `StatementContextRuntimeServices` and stop passing
`IContainer` to `StatementContext` entirely.

---

## Phase Dependency Graph

```
Phase 1 (Threading Primitives)
  |
  +-> Phase 2 (Type System Services)
  |
  +-> Phase 3 (Compilation Services)
  |
  +-> Phase 4 (Event Type System)
  |
  +-> Phase 5 (Utility/Infrastructure)
       |
       +-> Phase 6 (Runtime Context)
                |
                +-> Phase 7 (Entry Points)
                         |
                         +-> Phase 8 (Cleanup)
```

Phases 1–5 can largely proceed in parallel. Phase 6 depends on all of 1–5. Phase 7 depends on 6. Phase 8 is a cleanup sweep.

---

## Remaining Work Inventory (as of 2026-03-05)

### Phase 8 — Completed tasks

| Task | Result |
|------|--------|
| Delete `FallbackContainer.cs` | ✅ Done |
| Pare `ContainerExtensions` to 2 methods (`CreateDefaultContainer`, `CheckContainer`) | ✅ Done |
| Delete `SerializerFactoryExtensions.cs` | ✅ Done — no callers remained |
| Delete `MetadataReferenceProviderExtensions.cs` | ✅ Done — no callers remained |
| Strip lazy-register body from `MetadataReferenceResolverExtensions.cs` | ✅ Done — kept `GetMetadataReference` (default delegate, referenced by `ContainerInitializer`) and `RegisterMetadataReferenceResolver` (user-facing API) |
| Register `SerializerFactory`, `MetadataReferenceResolver`, `MetadataReferenceProvider` eagerly in `ContainerInitializer` | ✅ Done |
| `CompilerHelperServices.cs` — replace extension calls with `Resolve<T>()` | ✅ Done |
| `DefaultSupportGraphEventUtil.cs` — takes `IResourceManager` directly, no container | ✅ Done |

### Phase 8 — Remaining tasks (3 items)

**1. Delete `CoreAssemblyProviderExtensions.cs`**

`CoreAssemblyProvider(this IContainer container)` extension method has no production callers.
The static `GetCoreAssemblies()` method is referenced directly by `RoslynCompiler.cs` as a
delegate — it can be moved to `RoslynCompiler` or `RoslynCompilerExtensions` and the file deleted.

**2. Mark `EPRuntimeImpl.RuntimeContainer` `[Obsolete]`**

`RuntimeContainer` is still live (used at two call sites within `EPRuntimeImpl` itself for startup
re-initialization). It cannot be removed yet without further restructuring. Marking `[Obsolete]`
is consistent with the `Container` alias treatment and signals intent.

**3. Inline `ArtifactRepositoryExtensions.GetDefaultArtifactRepositoryManager` into `ContainerInitializer`**

`ArtifactRepositoryExtensions.cs` now contains only the static helper
`GetDefaultArtifactRepositoryManager(IContainer)`, called from exactly one site:
`ContainerInitializer.InitializeDefaultServices` (line 108). Move the body inline and delete
the file to eliminate the last `src/` file whose sole purpose was lazy-registration scaffolding.

### Test/support code — separate PR

These files are in `src/` but serve as test fixtures. They still use `container.Resolve<T>()`
pattern. Lower priority; address in a follow-on cleanup PR.

| File | Issue |
|------|-------|
| `SupportClasspathImport.cs` | `GetInstance(IContainer)` calls `Resolve<ImportServiceCompileTime>`, `Resolve<TypeResolverProvider>`, `Resolve<IResourceManager>` |
| `SupportEventTypeFactory.cs` (Runtime) | `GetInstance(IContainer)` calls `Resolve<SupportEventTypeFactory>`, `Resolve<IObjectCopier>` |
| `SupportEventBeanFactory.cs` (Runtime) | `GetInstance(IContainer)` calls `Resolve<SupportEventBeanFactory>` |

### Startup-boundary `Resolve<T>()` calls — acceptable, no action needed

The following files call `container.Resolve<T>()` exclusively during application startup
(container wiring / factory construction). This is the intended final state — the container
boundary is explicit and confined to initialization code.

| File | Role |
|------|------|
| `ContainerImpl.cs` | Internal IContainer implementation |
| `ContainerInitializer.cs` | Registers all singletons eagerly at startup |
| `EPServicesContextFactoryBase.cs` | Resolves registered singletons once to build EPServicesContext |
| `CompilerHelperServices.cs` | Resolves registered singletons once to build ModuleCompileTimeServices |
| `CompilerHelperModuleProvider.cs` | Resolves `IArtifactRepositoryManager` at module-load time |
| `EPRuntimeImpl.cs` (`RuntimeContainer`) | Resolves `TypeResolverProvider` during runtime re-initialization |
| `ContainerInitializer.RegisterDatabaseDriver` | Windsor-specific named registration; intentional, documented |

---

## Phase 1: Threading Primitives

### Goal

Eliminate the three container lookups in `StatementContext` — `container.LockManager()`,
`container.RWLockManager()`, and `container.ThreadLocalManager()` — by injecting the resolved
singletons directly. Address the 7 other call sites outside `StatementContext` where these are
resolved from the container.

### Why First

These three are the most pervasive resolutions in the codebase, appearing at 16 call sites across 6
files. Once injected directly, `StatementContext.Container` is no longer needed for its only runtime
use (the three computed properties). This unblocks Phase 6.

### Files to Modify

**`src/NEsper.Common/common/internal/context/util/StatementContextRuntimeServices.cs`**
- Add three constructor parameters: `ILockManager lockManager`, `IReaderWriterLockManager rwLockManager`, `IThreadLocalManager threadLocalManager`
- Add three auto-properties: `public ILockManager LockManager { get; }`, etc.
- Update the "empty" constructor to set these three to null

**`src/NEsper.Common/common/internal/context/util/StatementContext.cs`**
- Replace the three container-delegating computed properties (lines 126–130) with delegations to `StatementContextRuntimeServices`
- Remove `IContainer container` from constructor and `Container = container` assignment
- Remove `public IContainer Container { get; set; }` property

**`src/NEsper.Runtime/internal/kernel/service/EPServicesContext.cs`**
- In the `StatementContextRuntimeServices` property getter (lines 336–391), pass the three threading primitives resolved once from `_container`:
  ```csharp
  _container.Resolve<ILockManager>(),
  _container.Resolve<IReaderWriterLockManager>(),
  _container.Resolve<IThreadLocalManager>(),
  ```
- Add `RWLockManager` and `ThreadLocalManager` properties resolved once at construction for use by `StageRecoveryServiceImpl` etc.

**`src/NEsper.Common/common/internal/context/activator/ViewableActivatorFilterMgmtCallback.cs`**
- Replace `container.LockManager().CreateLock(GetType())` with injected `ILockManager lockManager` constructor parameter

**`src/NEsper.IO/AbstractCoordinatedAdapter.cs`** (line 362)
- Replace `_container.RWLockManager().CreateLock("CSV")` with injected `IReaderWriterLockManager` field

**`src/NEsper.Common/common/internal/epl/fafquery/querymethod/FAFQueryMethodSelectNoFromExprEvaluatorContext.cs`**
- Uses `services.Container.ThreadLocalManager()` — replace with `services.ThreadLocalManager` after `EPServicesContext` exposes it

**`src/NEsper.Runtime/internal/kernel/service/EPServicesContextFactoryBase.cs`** and
**`src/NEsper.Runtime/internal/kernel/service/EPServicesContextFactoryDefault.cs`**
- Calls here occur during startup construction; replace extension methods with explicit `container.Resolve<T>()` calls — mark as Phase 7 targets for full cleanup

### Breaking Changes

- `StatementContext` constructor loses `IContainer container` as first parameter — all callers must be updated (primary site: `Deployer.cs`)
- `StatementContextRuntimeServices` constructor gains 3 parameters — primary caller is `EPServicesContext`

### Test Strategy

```powershell
.\run-test-batches.ps1 -BatchName "Client"   # ~2.5 min, 253 tests
```

### Success Criteria

- No `using com.espertech.esper.container;` in `StatementContext.cs`
- No `IContainer` field/property in `StatementContext`
- Build succeeds; Client test batch passes

---

## Phase 2: Type System Services

### Goal

Remove `IContainer` from `ArtifactTypeResolverProvider`, `ImportServiceBase`, and their callers.
Replace container-based resolution of `TypeResolverProvider` and `TypeResolver` with direct injection.

### Files to Modify

**`src/NEsper.Common/common/client/util/ArtifactTypeResolverProvider.cs`**
- Replace `IContainer _container` with `IArtifactRepositoryManager _artifactRepositoryManager`
- Constructor: `ArtifactTypeResolverProvider(IArtifactRepositoryManager artifactRepositoryManager)`
- Caller in `ContainerInitializer.InitializeDefaultServices` changes to: `new ArtifactTypeResolverProvider(ic.Resolve<IArtifactRepositoryManager>())`

**`src/NEsper.Common/common/internal/settings/ImportServiceBase.cs`**
- Investigate all `Container.xxx()` usages in subclasses (`ImportServiceRuntime.cs`, `ImportServiceCompileTimeImpl.cs`)
- Replace each with a directly injected field for the specific service needed
- Remove `public IContainer Container { get; }` property

**`src/NEsper.Common/common/internal/epl/script/core/ScriptCompilerImpl.cs`**
- Replace `IContainer _container` with `TypeResolver _typeResolver` (or the specific scripting service)
- Constructor receives `TypeResolver typeResolver` parameter

### Breaking Changes

- `ArtifactTypeResolverProvider` constructor signature changes (only caller: `ContainerInitializer`)
- `ImportServiceBase` constructor may lose `IContainer container` parameter

### Test Strategy

```bash
dotnet test tst/NEsper.Common.Tests/NEsper.Common.Tests.csproj
```
```powershell
.\run-test-batches.ps1 -BatchName "Event"
```

### Success Criteria

- `ArtifactTypeResolverProvider` has no `IContainer` field
- `ScriptCompilerImpl` has no `IContainer` field
- Build succeeds, tests pass

---

## Phase 3: Compilation Services

### Goal

Remove `IContainer` from the compile-time service chain: `ModuleCompileTimeServices`,
`StatementCompileTimeServices`, `StatementSpecMapEnv`, and `SelectExprProcessorHelper`.

### Files to Modify

**`src/NEsper.Common/common/internal/compile/stage3/ModuleCompileTimeServices.cs`**
- Read all usages of `Container` within compile-time code
- Add fields for whichever services are actually resolved
- Remove `IContainer container` constructor parameter

**`src/NEsper.Common/common/internal/compile/stage1/specmapper/StatementSpecMapEnv.cs`**
- Find all callers of `mapEnv.Container` in the specmapper files
- Replace each usage with the specific service actually needed
- Remove `public IContainer Container { get; }`

**`src/NEsper.Common/common/internal/epl/resultset/select/core/SelectExprProcessorHelper.cs`**
- Read the full file to find what is resolved from the container
- Replace with injected fields for the specific services needed

**`src/NEsper.Compiler/internal/util/CompilerHelperServices.cs`**
- Verify any `IContainer container` parameter is only compilation-wiring; make explicit what is resolved

### Breaking Changes

- `ModuleCompileTimeServices` constructor signature changes — callers in `CompilerHelperServices.cs` and `EPCompilerImpl.cs`
- `StatementSpecMapEnv` constructor loses `IContainer container` — callers in stage1 specmapper

### Test Strategy

```bash
dotnet test tst/NEsper.Compiler.Tests/NEsper.Compiler.Tests.csproj
```
```powershell
.\run-test-batches.ps1 -BatchName "EPL-Database"
```

### Success Criteria

- No `IContainer` field in `ModuleCompileTimeServices`, `StatementCompileTimeServices`, or `StatementSpecMapEnv`
- Compiler tests pass

---

## Phase 4: Event Type System

### Goal

Remove `IContainer` from `EventTypeFactoryImpl`, `BeanEventType`, `BeanEventBeanSerializableCopyMethodForge`,
and `EventTypeCollectorImpl`.

### Key Patterns Found

- `BeanEventType` calls `_container.Resolve<IObjectCopier>()` and `_container.Resolve<SerializableObjectCopier>()` at lines 478, 507
- `BeanEventBeanSerializableCopyMethodForge` calls `_container.Resolve<IObjectCopier>()` in `GetCopyMethod`
- `EventTypeFactoryImpl.GetInstance(IContainer container)` creates or returns the factory as a singleton
- `EventTypeCollectorImpl` already receives `TypeResolver typeResolver` directly — verify if `container` is still used

### Files to Modify

**`src/NEsper.Common/common/internal/event/bean/core/BeanEventType.cs`**
- Replace `IContainer _container` with `IObjectCopier _objectCopier`
- Constructor gains `IObjectCopier objectCopier` parameter
- Lines 478, 507 use `_objectCopier` directly

**`src/NEsper.Common/common/internal/event/eventtypefactory/EventTypeFactoryImpl.cs`**
- Replace `IContainer Container` with `IObjectCopier _objectCopier`
- `GetInstance(IContainer container)` static method → `GetInstance(IObjectCopier objectCopier)`
- `CreateBeanType` passes `_objectCopier` instead of `Container`
- Caller in `EPServicesContextFactoryDefault.MakeEventTypeFactory`: resolve `IObjectCopier` from container once at startup

**`src/NEsper.Common/common/internal/event/bean/core/BeanEventBeanSerializableCopyMethodForge.cs`**
- Replace `IContainer _container` with `IObjectCopier _objectCopier`
- Straightforward single-use replacement

**`src/NEsper.Common/common/internal/event/path/EventTypeCollectorImpl.cs`**
- Verify that `container` field has no remaining usages after earlier phases
- Remove the field and constructor parameter if dead

### Breaking Changes

- `BeanEventType` constructor gains `IObjectCopier objectCopier` — primary site: `EventTypeFactoryImpl.CreateBeanType`
- `EventTypeFactoryImpl.GetInstance` signature change — caller: `EPServicesContextFactoryDefault`

### Test Strategy

```powershell
.\run-test-batches.ps1 -BatchName "Event"
.\run-test-batches.ps1 -BatchName "EPL-NamedWindow"
```

### Success Criteria

- No `IContainer` field in any event type class
- Event and named window tests pass

---

## Phase 5: Utility and Infrastructure Services

### Goal

Remove `IContainer` from `SerializableObjectCopier`, `SerializerFactory`, `DatabaseConfigServiceImpl`,
`DbProviderFactoryManagerDefault`, `EPDataFlowServiceImpl`, and `ScriptCompilerImpl` (if not done in Phase 2).

### Key Patterns Found

**`SerializableObjectCopier`** — most complex pattern: lazy singleton registration inside the Copy
method. Uses `lock (_container)` to lazily register `ObjectSerializer` into the container at first use.

**`DbProviderFactoryManagerDefault`** — calls `container.Has(driverTypeName)` and
`container.Register<DbProviderFactory>(...)` during construction — mixing startup registration with
object construction.

### Files to Modify

**`src/NEsper.Common/common/internal/util/SerializableObjectCopier.cs`**

Replace the container-based lazy singleton pattern entirely:
```csharp
private readonly ObjectSerializer _serializer;

public SerializableObjectCopier(ObjectSerializer serializer) {
    _serializer = serializer;
}
```

Move `ObjectSerializer` creation to `ContainerInitializer.InitializeDefaultServices`:
```csharp
container.Register<ObjectSerializer>(
    ic => SerializerFactory.CreateDefaultSerializer(ic),
    Lifespan.Singleton);
container.Register<IObjectCopier>(
    ic => new SerializableObjectCopier(ic.Resolve<ObjectSerializer>()),
    Lifespan.Singleton);
```

Remove `GetInstance(IContainer container)` static method — callers receive `IObjectCopier` by injection.

**`src/NEsper.Common/common/internal/util/serde/SerializerFactory.cs`**

Add an overload that takes `TypeResolver` directly; keep container-based version as a thin wrapper for startup code:
```csharp
public static Serializer CreateDefaultSerializer(TypeResolver typeResolver) { ... }
public static Serializer CreateDefaultSerializer(IContainer container) {
    var typeResolver = container.Resolve<TypeResolver>(); // or TypeResolverProvider
    return CreateDefaultSerializer(typeResolver);
}
```

**`src/NEsper.Common/common/internal/epl/historical/database/connection/DatabaseConfigServiceImpl.cs`**

Replace `IContainer _container` with `DbProviderFactoryManager _dbProviderFactoryManager`. The factory manager already has the right API for looking up providers by name.

**`src/NEsper.Common/common/internal/db/drivers/DbProviderFactoryManagerDefault.cs`**

Refactor so it does not need `IContainer`:
- Move registration logic out to `ContainerInitializer.InitializeDatabaseDrivers`
- Hold a `Dictionary<string, DbProviderFactory>` populated by `AppDomain` scanning in constructor
- Constructor takes no `IContainer`

**`src/NEsper.Common/common/internal/epl/dataflow/core/EPDataFlowServiceImpl.cs`**

Read the full file to understand what `IContainer` is used for. If used to resolve services at dataflow initialization, replace with direct injection of those specific services.

### Breaking Changes

- `SerializableObjectCopier` constructor changes — primary caller is `ContainerInitializer`
- `DbProviderFactoryManagerDefault` constructor loses `IContainer` — primary caller is `ContainerInitializer.InitializeDatabaseDrivers`

### Test Strategy

```powershell
.\run-test-batches.ps1 -BatchName "EPL-Database"
```
```bash
dotnet test tst/NEsper.Compat.Tests/NEsper.Compat.Tests.csproj
```

### Success Criteria

- `SerializableObjectCopier` has no `IContainer` field
- `DbProviderFactoryManagerDefault` has no `IContainer` field
- Database and compat tests pass

---

## Phase 6: Runtime Context Chain

### Goal

Remove `IContainer` from `EPServicesContext`, `EPStatementInitServicesImpl`, `StageRuntimeServices`,
`StatementContext` (should already be done from Phase 1), and `StatementContextRuntimeServices`
(should already be done from Phase 1).

### Precondition

Phases 1–5 must be complete. Verify by searching for remaining `_container.Resolve` calls in candidate files.

### Files to Modify

**`src/NEsper.Runtime/internal/kernel/service/EPServicesContext.cs`**
- After Phases 1–5, `_container` should only be referenced in the startup-time `StatementContextRuntimeServices` getter and `StageRuntimeServices` constructor
- Verify by grepping for `_container` in the file
- Once the only remaining uses are the initial resolve calls, move those resolves to the constructor, store them as fields, and remove `_container` entirely
- Remove `public IContainer Container { get; }` property (line 328)

**`src/NEsper.Runtime/internal/kernel/stage/StageRuntimeServices.cs`**
- After Phase 1 removes threading usage, find any remaining `Container` usages
- Remove the constructor parameter and `Container` property

**`src/NEsper.Runtime/internal/kernel/service/EPStatementInitServicesImpl.cs`**
- `public IContainer Container => ServicesContext.Container` — remove once `EPServicesContext.Container` is gone

### Breaking Changes

- `EPServicesContext` constructor loses `IContainer container` — the most widely-called constructor; `EPServicesContextFactoryBase.CreateServices` must be updated
- `StageRuntimeServices` constructor loses `IContainer container`

### Test Strategy

```bash
dotnet test tst/NEsper.Common.Tests/
dotnet test tst/NEsper.Runtime.Tests/
```
```powershell
.\run-test-batches.ps1 -BatchName "Client"
.\run-test-batches.ps1 -BatchName "Context"
.\run-test-batches.ps1 -BatchName "EPL-NamedWindow"
```

### Success Criteria

- No `IContainer` field in `EPServicesContext`, `StageRuntimeServices`, `StatementContextRuntimeServices`
- All test batches pass

---

## Phase 7: Entry Points

### Goal

Remove `IContainer` from runtime entry points. At this point the container is only needed at
startup to register and resolve the initial set of singletons.

### Strategy

`Configuration.Container` is a user-facing public API — users call `config.Container = myContainer`
to inject a custom container. This property must remain. However, the container should flow only as
far as `EPServicesContextFactoryBase.CreateServices`; after construction of `EPServicesContext`,
the container reference is dropped.

### Files to Modify

**`src/NEsper.Common/client/configuration/Configuration.cs`**
- Keep `Container` property as public API
- Remove `IResourceManager ResourceManager => Container.ResourceManager()` delegation (line 60)
- Callers of `ConfigSnapshot.ResourceManager` — if any — must use the resolved `IResourceManager` directly

**`src/NEsper.Runtime/internal/kernel/service/EPRuntimeImpl.cs`**
- After Phase 6, `Container` property on `EPRuntimeImpl` should no longer be used by runtime code
- Deprecate or remove the property; check `SupportStatementContextFactory.cs` for test usage

**`src/NEsper.Runtime/internal/kernel/service/EPServicesContextFactoryBase.cs`** and
**`EPServicesContextFactoryDefault.cs`**
- Replace `container.RWLockManager()` and `container.ThreadLocalManager()` extension method calls with explicit `container.Resolve<IReaderWriterLockManager>()` etc.
- This makes the container-usage boundary explicit and documents startup wiring

### Breaking Changes

- `EPRuntimeImpl.Container` deprecation/removal is a minor public API change; deprecation first is safer

### Test Strategy

```powershell
.\run-test-batches.ps1   # full run
```

### Success Criteria

- Container only accessed in factory/initializer startup code
- Full regression suite passes at ~99.87% rate

---

## Phase 8: Cleanup

### Goal

Remove vestigial lazy-register extension code, clean up a handful of remaining `src/` service
locator call sites, and document the final responsibility of `IContainer`.

### Completed tasks

| Task | Status |
|------|--------|
| Delete `FallbackContainer.cs` | ✅ |
| Pare `ContainerExtensions` to 2 methods | ✅ |
| Delete `SerializerFactoryExtensions.cs` | ✅ |
| Delete `MetadataReferenceProviderExtensions.cs` | ✅ |
| Strip lazy-register body from `MetadataReferenceResolverExtensions.cs` | ✅ |
| Register `SerializerFactory`, `MetadataReferenceResolver`, `MetadataReferenceProvider` eagerly | ✅ |
| `DefaultSupportGraphEventUtil` takes `IResourceManager` directly | ✅ |

### Remaining tasks

1. Delete `CoreAssemblyProviderExtensions.cs` — no callers of the extension method; move `GetCoreAssemblies` if needed
2. Mark `EPRuntimeImpl.RuntimeContainer` `[Obsolete]` — property still live but signals deprecation
3. Inline `ArtifactRepositoryExtensions.GetDefaultArtifactRepositoryManager` into `ContainerInitializer` and delete `ArtifactRepositoryExtensions.cs`

### Final `IContainer` responsibility (target state achieved)

`IContainer` is now used only at startup:
1. `ContainerInitializer.InitializeDefaultServices` — registers all singletons eagerly
2. `EPServicesContextFactoryBase.CreateServices` — resolves registered singletons once to build context
3. `CompilerHelperServices.GetServices` — resolves registered singletons once to build compile-time context
4. `ContainerExtensions.CreateDefaultContainer` — public bootstrapping API
5. `Configuration.Container` — public user API for custom container injection

`ContainerInitializer.RegisterDatabaseDriver` intentionally accesses `WindsorContainer` directly
for named component registration. This is the one acceptable Windsor-specific call site.

### Test Strategy

```bash
dotnet build NEsperAll.sln
```
```powershell
.\run-test-batches.ps1   # full run
```

### Success Criteria

- ✅ `ContainerExtensions` has exactly 2 methods: `CreateDefaultContainer` and `CheckContainer`
- ✅ `DefaultSupportGraphEventUtil` takes `IResourceManager` directly
- ⬜ `CoreAssemblyProviderExtensions.cs` deleted
- ⬜ `EPRuntimeImpl.RuntimeContainer` marked `[Obsolete]`
- ⬜ `ArtifactRepositoryExtensions.cs` deleted
- ⬜ All 5,691 tests pass (3,801 regression + 1,890 unit) — full regression run to confirm

---

## Implementation Notes

### Pattern for Replacing ResolveSingleton

**Before (anti-pattern):**
```csharp
// Deep in object graph at runtime:
var copier = SerializableObjectCopier.GetInstance(_container);
```

**After (constructor injection):**
```csharp
// At startup in ContainerInitializer:
container.Register<IObjectCopier>(
    ic => new SerializableObjectCopier(ic.Resolve<ObjectSerializer>()),
    Lifespan.Singleton);

// At call site — receive IObjectCopier via constructor:
private readonly IObjectCopier _copier;
public MyClass(IObjectCopier copier) { _copier = copier; }
```

### Handling Named `DbProviderFactory` Registrations

`DatabaseConfigServiceImpl` uses the container for named lookups by type name. The replacement:
1. `DbProviderFactoryManagerDefault` holds a `Dictionary<string, DbProviderFactory>` built at construction by scanning `AppDomain`
2. `DatabaseConfigServiceImpl` receives `DbProviderFactoryManager` directly and queries it by name
3. For `DbDriver` named registration, introduce a `DbDriverRegistry` singleton with a dictionary

### Testing Cadence Recommendation

| After Phase | Test Command |
|---|---|
| 1 | `.\run-test-batches.ps1 -BatchName "Client"` |
| 2 | `dotnet test tst/NEsper.Common.Tests/` |
| 3 | `dotnet test tst/NEsper.Compiler.Tests/` |
| 4 | `.\run-test-batches.ps1 -BatchName "Event"` |
| 5 | `.\run-test-batches.ps1 -BatchName "EPL-Database"` |
| 6 | `.\run-test-batches.ps1 -BatchName "Client"` + `"Context"` |
| 7 | `.\run-test-batches.ps1` (full run) |
| 8 | `dotnet build NEsperAll.sln` + full run |

---

## Summary Table

| Phase | Container Users Removed | Test Batch | Effort |
|---|---|---|---|
| 1 | StatementContext, StatementContextRuntimeServices, ViewableActivatorFilterMgmtCallback | Client | Medium |
| 2 | ArtifactTypeResolverProvider, ImportServiceBase, ScriptCompilerImpl | Event | Low |
| 3 | ModuleCompileTimeServices, StatementSpecMapEnv, SelectExprProcessorHelper | EPL-Database | Medium |
| 4 | EventTypeFactoryImpl, BeanEventType, BeanEventBeanSerializableCopyMethodForge, EventTypeCollectorImpl | Event, NamedWindow | Medium |
| 5 | SerializableObjectCopier, SerializerFactory, DatabaseConfigServiceImpl, DbProviderFactoryManagerDefault, EPDataFlowServiceImpl | EPL-Database | High |
| 6 | EPServicesContext, StageRuntimeServices, StatementContextRuntimeServices | Client, Context, NamedWindow | High |
| 7 | Configuration (partial), EPRuntimeImpl, factory base classes | Full regression | Medium |
| 8 | ContainerExtensions cleanup, Windsor removal | Full build + regression | Medium |
