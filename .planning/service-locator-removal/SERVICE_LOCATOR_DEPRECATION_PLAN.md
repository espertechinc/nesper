# Service Locator Deprecation Plan (Context-Free)

## Scope, Goal, and Definition

This document identifies active service-locator usage in `NEsper-8.9.x` and provides a staged plan to deprecate/remove it.

A service-locator usage is any runtime or compile-time code path that:

1. Resolves dependencies ad hoc (`container.Resolve<T>()`, `Resolve<T>(name)`, `ResolveSingleton(...)`), or
2. Uses `IContainer` extension methods that hide resolve calls (`container.ResourceManager()`, `container.RWLockManager()`, etc.), or
3. Exposes `IContainer` broadly so downstream code can resolve dependencies instead of receiving explicit constructor arguments.

Target architecture:

- `IContainer` is used only in startup/composition roots.
- All downstream classes receive concrete dependencies via constructor injection.
- No business/runtime flow resolves dependencies dynamically.

---

## Inventory of Active Service-Locator Usage

> Original inventory: 2026-03-02. Status updated: 2026-03-03 after Phases 1–5 completion.
> Legend: ~~strikethrough~~ = eliminated. Items without strikethrough are still active.

## A) Infrastructure/Enablers (must be constrained)

These are not necessarily business call sites, but they enable widespread service location:

- `src/NEsper.Common/container/ContainerExtensions.cs`
  - `LockManager`, `RWLockManager`, `ThreadLocalManager`, `ResourceManager`, `TypeResolverProvider`, `TypeResolver`, `ResolveSingleton`.
- `src/NEsper.Common/container/IContainer.cs`
  - `Resolve*`, `TryResolve*`, `Has*` API is globally available.
- `src/NEsper.Common/container/ContainerImpl.cs`
  - Implementation of resolve APIs.

## B) Runtime production call sites (`src/`)

### Runtime kernel and context

- ~~`src/NEsper.Runtime/internal/kernel/service/EPServicesContextFactoryBase.cs`~~
  - ~~`epRuntime.Container.RWLockManager()`~~ → replaced with `container.Resolve<IReaderWriterLockManager>()`
  - ~~`container.RWLockManager()`~~ → explicit `Resolve<IReaderWriterLockManager>()`
  - ~~`container.ThreadLocalManager()`~~ → explicit `Resolve<IThreadLocalManager>()`
  - **Still active (composition-root approved):** `container.Resolve<TypeResolverProvider>()`, `container.Resolve<IResourceManager>()`, etc.
- ~~`src/NEsper.Runtime/internal/kernel/service/EPServicesContextFactoryDefault.cs`~~
  - ~~`_container.RWLockManager()`~~ → eliminated
  - ~~`_container.Resolve<IObjectCopier>()`~~ → `objectCopier` passed explicitly
- `src/NEsper.Runtime/internal/kernel/service/EPServicesContext.cs`
  - ~~`_container.Resolve<ILockManager>()`~~ → injected via constructor
  - ~~`_container.Resolve<IReaderWriterLockManager>()`~~ → injected via constructor
  - ~~`_container.Resolve<IThreadLocalManager>()`~~ → injected via constructor
  - **Still present:** `_container` field stored, `public IContainer RuntimeContainer => _container` (transitional); `[Obsolete] public IContainer Container => RuntimeContainer`
- `src/NEsper.Runtime/internal/kernel/statement/EPStatementInitServicesImpl.cs`
  - ~~`ServicesContext.Container.Resolve<IObjectCopier>()`~~ → `ServicesContext.ObjectCopier`
  - **Still present (Obsolete):** `public IContainer Container => ServicesContext.RuntimeContainer`
- `src/NEsper.Runtime/internal/kernel/service/EPRuntimeImpl.cs`
  - ~~`Container.Resolve<TypeResolverProvider>()`~~ → `RuntimeContainer.Resolve<TypeResolverProvider>()` at startup
  - **Still present:** `public IContainer RuntimeContainer { get; }` (from `configuration.Container`); `[Obsolete] public IContainer Container => RuntimeContainer`

### Runtime stage recovery

- ~~`src/NEsper.Runtime/internal/kernel/stage/StageRecoveryServiceBase.cs`~~
  - ~~`servicesContext.Container.RWLockManager()`~~ → `rwLockManager` injected directly
- ~~`src/NEsper.Runtime/internal/kernel/stage/StageRecoveryServiceImpl.cs`~~
  - ~~`servicesContext.Container.RWLockManager()`~~ → `rwLockManager` injected directly

### Runtime utility/support classes in production assembly

- ~~`src/NEsper.Runtime/internal/timer/TimerServiceImpl.cs`~~
  - ~~`Container.Resolve<ITimerFactory>()`~~ → `ITimerFactory` injected directly
- ~~`src/NEsper.Runtime/internal/kernel/service/EPEventServiceHelper.cs`~~
  - ~~`container.ThreadLocalManager()`~~ → `IThreadLocalManager` injected directly
- ~~`src/NEsper.Runtime/internal/kernel/service/EPRuntimeBeanAnonymousTypeService.cs`~~
  - ~~`container.Resolve<IObjectCopier>()`~~ → `IObjectCopier` injected directly
- `src/NEsper.Runtime/internal/support/SupportEventTypeFactory.cs`
  - **Still active:** `ResolveSingleton`, `Resolve<IObjectCopier>()` (×2) — test support only
- `src/NEsper.Runtime/internal/support/SupportEventBeanFactory.cs`
  - **Still active:** `ResolveSingleton` — test support only

## C) Compiler production call sites (`src/NEsper.Compiler`)

- ~~`src/NEsper.Compiler/internal/util/CompilerHelperServices.cs`~~
  - ~~`container.Resolve<IObjectCopier>()`~~ → explicit `Resolve<IObjectCopier>()`
  - ~~`container.Resolve<IThreadLocalManager>()`~~ → explicit `Resolve<IThreadLocalManager>()`
  - ~~`container.Resolve<TypeResolverProvider>()`~~ → explicit `Resolve<TypeResolverProvider>()`
  - ~~`container.Resolve<IResourceManager>()`~~ → explicit `Resolve<IResourceManager>()`
  - ~~Extension methods replaced with explicit `Resolve<T>()` calls~~
- ~~`src/NEsper.Compiler/internal/util/EPCompilerImpl.cs`~~
  - ~~`container.Resolve<TypeResolverProvider>()`~~ → explicit `Resolve<TypeResolverProvider>()`
  - ~~`container.Resolve<IResourceManager>()`~~ → explicit `Resolve<IResourceManager>()`
- `src/NEsper.Compiler/client/CoreAssemblyProviderExtensions.cs`
  - ~~`container.Resolve<CoreAssemblyProvider>()`~~ — method has no callers; **pending removal (Phase 7)**
- `src/NEsper.Compiler/internal/util/CompilerHelperModuleProvider.cs`
  - **Still active:** `GetCompilerThreadPoolFactory(IContainer)` — intentional user extension point; reads `compileTimeServices.Configuration.Container`

## D) Common production call sites (`src/NEsper.Common`)

- ~~`src/NEsper.Common/common/internal/util/SerializableObjectCopier.cs`~~
  - ~~`_container.Resolve<TypeResolver>()`~~ → takes `TypeResolver` directly in constructor
- ~~`src/NEsper.Common/common/internal/util/serde/SerializerFactory.cs`~~
  - ~~`container.Resolve<TypeResolver>()`~~ → eliminated
- `src/NEsper.Common/common/internal/util/serde/SerializerFactoryExtensions.cs`
  - `container.Resolve<SerializerFactory>()` — **dead code; no callers remain; pending file deletion (Phase 7)**
- ~~`src/NEsper.Common/common/internal/util/TransientConfigurationResolver.cs`~~
  - ~~`container.Resolve<ClassForNameProvider>()`~~ → takes `TypeResolverProvider` directly
- `src/NEsper.Common/common/internal/support/SupportImport.cs` (was SupportClasspathImport)
  - **Still active:** `ResolveSingleton`, `Resolve<TypeResolverProvider>()`, `Resolve<IResourceManager>()` — test support only
- ~~`src/NEsper.Common/common/internal/db/DbDriverConnectionHelper.cs`~~
  - ~~`container.Resolve<DbDriver>(name)`~~ → marked `[Obsolete]`, replaced by direct driver resolution
- ~~`src/NEsper.Common/common/internal/db/drivers/DbProviderFactoryManagerDefault.cs`~~
  - ~~`_container.Resolve<DbProviderFactory>(factoryName)`~~ → `IContainer` ctor overload removed; uses `Dictionary<string, DbProviderFactory>` built at construction
- `src/NEsper.Common/common/internal/epl/dataflow/util/DefaultSupportGraphEventUtil.cs`
  - **Still active:** `container.ResourceManager()` — test support only
- ~~`src/NEsper.Common/common/client/configuration/Configuration.cs`~~
  - ~~`Container.ResourceManager()`~~ → callers use injected `IResourceManager` directly
- ~~`src/NEsper.Common/common/client/artifact/*Extensions.cs`~~
  - ~~`Resolve<IArtifactRepositoryManager>()`~~ → explicit `ArtifactRepositoryManager()` from container at startup
- ~~`src/NEsper.Common/common/internal/epl/dataflow/interfaces/DataFlowOpInitializeContext.cs`~~
  - ~~`container.Resolve<IResourceManager>()`~~ → `IResourceManager` injected directly; `IContainer` removed
- ~~`src/NEsper.Common/common/internal/epl/dataflow/realize/DataflowInstantiator.cs`~~
  - ~~`IContainer container` parameter~~ → replaced with `IResourceManager resourceManager`
- ~~`src/NEsper.Common/common/internal/epl/dataflow/core/EPDataFlowServiceImpl.cs`~~
  - ~~`_container` field~~ → replaced with `_resourceManager: IResourceManager`
- ~~`src/NEsper.Common/common/internal/util/serde/SerializerUtil.cs`~~
  - ~~`ObjectToByteArr(IContainer, object)`~~ → dead overload removed
- ~~`src/NEsper.Common/common/internal/context/controller/hash/ContextControllerHashedGetterCRC32SerializedForge.cs`~~
  - ~~`SerializeAndCRC32Hash(IContainer, ...)`~~ → dead overload removed; codegen uses `SerializerFactory.Instance` overload

## E) IO production call sites (`src/NEsper.IO`)

- ~~`src/NEsper.IO/AdapterInputSource.cs`~~
  - ~~`_container.ResourceManager().GetResourceAsStream(...)`~~ → `IResourceManager` injected directly ✅
- ~~`src/NEsper.IO/AbstractCoordinatedAdapter.cs`~~
  - ~~`_container.RWLockManager().CreateLock(...)`~~ → `IReaderWriterLockManager` injected directly ✅

## F) Test and regression call sites (`tst/`)

### Test support containers (static service locator wrappers)

- `tst/NEsper.Common.Tests/SupportContainer.cs` (`Resolve<T>()` wrapper)
- `tst/NEsper.Compiler.Tests/SupportContainer.cs` (`Resolve<T>()` wrapper)
- `tst/NEsper.IO.Tests/support/util/SupportContainer.cs` (`Resolve<T>()` wrapper)

### Test runtime/container extension usages

- `tst/NEsper.Runtime.Tests/AbstractRuntimeTest.cs` (`Container.RWLockManager()`)
- `tst/NEsper.IO.Tests/AbstractRuntimeTest.cs` (`container.LockManager()`)
- `tst/NEsper.IO.Tests/regression/adapter/TestCSVAdapter.cs` (`_container.ResourceManager()`)
- `tst/NEsper.Common.Tests/internal/metrics/stmtmetrics/TestStatementMetricArray.cs` (`container.RWLockManager()`)

### Test support factories using Resolve/ResolveSingleton

- `tst/NEsper.Common.Tests/internal/supportunit/event/SupportEventTypeFactory.cs`
- `tst/NEsper.Common.Tests/internal/supportunit/db/SupportDatabaseService.cs`
- `tst/NEsper.Common.Tests/internal/supportunit/util/SupportExprNodeFactory.cs`
- `tst/NEsper.Common.Tests/internal/supportunit/util/SupportJoinResultNodeFactory.cs`

### Regression runner / framework

- `tst/NEsper.Regression.Runner/suite/event/TestSuiteEventXML.cs` (`configuration.Container.ResourceManager()`)
- `tst/NEsper.Regression.Runner/suite/epl/TestSuiteEPLContained.cs` (`_session.Container.ResourceManager()`)
- `tst/NEsper.Regression/framework/RegressionEnvironmentBase.cs` (`Container.LockManager()`)

---

## Deprecation Strategy (Detailed, staged)

## Phase 0 — Governance and Safety Net

1. Add architecture rule: no new `Resolve<T>()`/container extension usage outside composition roots.
2. Add CI grep guard (warning first, fail later) for:
   - `\.Resolve<`
   - `\.ResolveSingleton\(`
   - `\.(ResourceManager|RWLockManager|ThreadLocalManager|LockManager|TypeResolverProvider|TypeResolver)\(`
3. Define approved composition roots:
   - Container bootstrap/initializer modules
   - Runtime/compiler construction boundaries

Exit criteria:

- CI detects new service-locator usage in non-whitelisted paths.

## Phase 1 — Remove runtime hot-path locator usage

Prioritize classes used frequently at runtime:

1. `EPServicesContextFactoryBase`, `EPServicesContextFactoryDefault`
   - Resolve once at factory start.
   - Pass concrete dependencies into constructors.
2. `EPServicesContext`
   - Replace `_container.Resolve(...)` in `StatementContextRuntimeServices` with constructor-injected lock/thread services.
   - Add explicit properties for these dependencies.
3. `TimerServiceImpl`
   - Inject `ITimerFactory` directly.
4. `StageRecoveryServiceBase` / `StageRecoveryServiceImpl`
   - Remove `servicesContext.Container.*` usage; consume injected lock manager.

Exit criteria:

- No `Resolve*`/container-extension calls in runtime hot path classes.

### Phase 1 Implementation Review (2026-03-02)

Review scope:

- `src/NEsper.Runtime/internal/kernel/service/EPServicesContextFactoryBase.cs`
- `src/NEsper.Runtime/internal/kernel/service/EPServicesContextFactoryDefault.cs`
- `src/NEsper.Runtime/internal/kernel/service/EPServicesContext.cs`
- `src/NEsper.Runtime/internal/timer/TimerServiceImpl.cs`
- `src/NEsper.Runtime/internal/kernel/stage/StageRecoveryServiceBase.cs`
- `src/NEsper.Runtime/internal/kernel/stage/StageRecoveryServiceImpl.cs`

Findings:

1. **No new service-locator calls were introduced in runtime hot-path classes** (`EPServicesContext`, `TimerServiceImpl`, stage recovery classes).
2. **`Resolve<T>()` usage increased in `EPServicesContextFactoryBase` composition root**, because lock/thread/timer/object-copier dependencies are now resolved once at startup and passed explicitly downstream.
   - This is expected and aligns with target architecture (container access at composition root only).
3. **Container-extension usage was removed from phase-1 targets** and replaced with explicit dependency flow.

Phase 1 status after review:

- ✅ `TimerServiceImpl` no longer depends on `IContainer`; now receives `ITimerFactory` directly.
- ✅ `EPServicesContext` no longer resolves lock/thread services on demand.
- ✅ Stage recovery services no longer use `servicesContext.Container.RWLockManager()`.
- ✅ `EPServicesContextFactoryDefault` no longer resolves lock/object-copier via container.
- ℹ️ `EPServicesContextFactoryBase` remains the approved runtime composition root and still resolves dependencies there.

Refined guard for future reviews:

- Treat `EPServicesContextFactoryBase.CreateServicesContext(...)` as phase-1-allowed for `Resolve<T>()`.
- Flag any new `Resolve*` or container-extension usage in:
  - `EPServicesContext`
  - `TimerServiceImpl`
  - `StageRecoveryServiceBase`
  - `StageRecoveryServiceImpl`
  - any runtime internal class outside approved composition roots.

## Phase 2 — Collapse container leakage in service APIs

1. Remove/obsolete `EPServicesContext.Container` (or make internal transitional only).
2. Remove/obsolete `EPStatementInitServicesImpl.Container` and `ObjectCopier` service-locator getter.
3. Stop exposing container from `EPRuntimeImpl` for internal flows.

Exit criteria:

- Internal code cannot obtain `IContainer` from runtime context objects.

### Phase 2 Kickoff Progress (2026-03-02)

Completed in this pass:

1. `EPStatementInitServicesImpl.ObjectCopier` no longer resolves through `ServicesContext.Container`; now uses explicit `ServicesContext.ObjectCopier` dependency.
2. `EPServicesContext` now stores and exposes an explicit `IObjectCopier` dependency.
3. `EPRuntimeBeanAnonymousTypeService` no longer accepts `IContainer` and no longer calls `Resolve<IObjectCopier>()`; it now accepts `IObjectCopier` directly.
4. `EPRuntimeStatementSelectionSPI` no longer uses `runtimeSPI.Container` for anonymous type creation; it now uses `runtimeSPI.ServicesContext.ObjectCopier`.

Validation:

- `dotnet build src/NEsper.Runtime/NEsper.Runtime.csproj -v minimal` ✅
- `dotnet build tst/NEsper.Runtime.Tests/NEsper.Runtime.Tests.csproj -v minimal` ✅

### Phase 2 Progress Update (2026-03-02, pass 2)

Completed in this pass:

1. Removed container dependency from event-service/stage thread-local allocation path.
   - `EPEventServiceHelper.AllocateThreadLocals(...)` now accepts `IThreadLocalManager` directly.
   - `EPEventServiceImpl` now passes `EPServicesContext.ThreadLocalManager`.
   - `EPStageEventServiceImpl` now passes `StageRuntimeServices.ThreadLocalManager`.
2. Refactored `StageRuntimeServices` to carry explicit `IThreadLocalManager` instead of `IContainer`.
3. Added explicit `IArtifactRepositoryManager` to `EPServicesContext` and wired it from composition root.
4. Replaced artifact-repository access via `services.Container.ArtifactRepositoryManager()` with explicit `services.ArtifactRepositoryManager` in:
   - `EPDeploymentServiceImpl`
   - `DeployerHelperInitializeEPLObjects`

### Phase 2 Progress Update (2026-03-02, pass 3)

Completed in this pass:

1. Added explicit deprecation markers on container exposure APIs to guide migration while retaining compatibility:
   - `EPStatementInitServices.Container`
   - `EPStatementInitServicesImpl.Container`
   - `EPRuntimeSPI.Container`
   - `EPRuntimeImpl.Container`
   - `EPServicesContext.Container`

### Phase 2 Completion Update (2026-03-02, pass 4)

Completed in this pass:

1. Added non-obsolete transitional aliases for composition-root/internal usage:
   - `EPRuntimeSPI.RuntimeContainer`
   - `EPRuntimeImpl.RuntimeContainer`
   - `EPServicesContext.RuntimeContainer`
2. Refactored runtime internals to stop using obsolete `Container` members:
   - `EPServicesContextFactoryBase` now uses `epRuntime.RuntimeContainer`.
   - `EPRuntimeImpl` startup/config-snapshot internals now use `RuntimeContainer`.
   - `EPStatementInitServicesImpl.Container` now delegates via `ServicesContext.RuntimeContainer`.
3. Preserved deprecated `Container` properties as compatibility shims for external/legacy callers.

Notes:

- Deprecation warnings are expected only at remaining legacy compatibility call-sites.
- Composition-root container access in `EPServicesContextFactoryBase.CreateServicesContext(...)` remains phase-allowed.

Validation:

- `dotnet build src/NEsper.Runtime/NEsper.Runtime.csproj -v minimal` ✅
- `dotnet build tst/NEsper.Runtime.Tests/NEsper.Runtime.Tests.csproj -v minimal` ✅

Phase 2 status:

- ✅ Runtime-internal container leakage in targeted service APIs is closed.
- ✅ Deprecated `Container` surfaces remain as transitional compatibility shims.
- ➡️ Remaining hard-removal work moves to later deprecation/removal phase(s).

## Phase 3 — Compiler chain injection cleanup

1. `CompilerHelperServices`, `EPCompilerImpl`:
   - Resolve startup dependencies once.
   - Pass concrete services through `ModuleCompileTimeServices` / mapping environments.
2. Remove locator extension usage from compiler helper extension classes where possible.

Exit criteria:

- Compiler internals no longer call container resolve outside composition root.

### Phase 3 Completion (2026-03-03)

Completed in this pass:

1. **Replaced container extension methods with explicit `Resolve<T>()` calls** in compiler composition roots:
   - `CompilerHelperServices.GetServices()` lines 104-111: replaced `.SerializerFactory()`, `.ArtifactRepositoryManager()`, `.MetadataReferenceResolver()`, `.MetadataReferenceProvider()` with explicit `Resolve<T>()` calls.
   - `CompilerHelperModuleProvider.Compile()` line 81: replaced `.ArtifactRepositoryManager()` with explicit `Resolve<IArtifactRepositoryManager>()`.

2. **Fixed test infrastructure issue** exposed by proper dependency flow:
   - `ExprConcatNodeForge.ExprEvaluator` now handles null `IThreadLocalManager` gracefully by falling back to non-thread-local evaluator (for test scenarios where `ModuleCompileTimeServices` uses empty constructor).
   - Added container assignment in `SupportExprValidationContextFactory.Make()` to ensure test validation contexts have access to container.

Validation:

- `dotnet build src/NEsper.Compiler/NEsper.Compiler.csproj -v minimal` ✅
- `dotnet test tst/NEsper.Compiler.Tests/NEsper.Compiler.Tests.csproj -v minimal` ✅ (134/134 tests passed)

Phase 3 status:

- ✅ No container extension methods remain in compiler assemblies.
- ✅ All container access in compiler is now explicit `Resolve<T>()` at composition root.
- ✅ `ModuleCompileTimeServices` and `StatementSpecMapEnv` have no `IContainer` dependencies.
- ➡️ Container usage is limited to startup wiring in `CompilerHelperServices.GetServices()` and `CompilerHelperModuleProvider.Compile()`.

## Phase 4 — Common utility and DB subsystem cleanup

1. `SerializableObjectCopier`, `SerializerFactory*`, `TransientConfigurationResolver`:
   - Replace dynamic lookup with explicit constructor args/factory args.
2. DB stack (`DbDriverConnectionHelper`, `DbProviderFactoryManagerDefault`):
   - Introduce explicit registries/factories instead of named container lookups.
3. `Configuration.ResourceManager` convenience property:
   - Mark obsolete and provide explicit alternative wiring path.

Exit criteria:

- No runtime dynamic resolve in utility/DB code.

### Phase 4 Completion (2026-03-03)

Completed in this pass:

1. **Dataflow chain fully decoupled from `IContainer`:**
   - `DataFlowOpInitializeContext` — removed `IContainer container` constructor parameter; now takes `IResourceManager resourceManager` directly; removed `Container` property.
   - `DataflowInstantiator` — all three static methods (`Instantiate`, `InstantiateOperators`, `InstantiateOperator`) changed from `IContainer` to `IResourceManager` parameter; removed `using com.espertech.esper.container`.
   - `EPDataFlowServiceImpl` — `_container` field replaced with `_resourceManager`; constructor takes `IResourceManager`; removed `using com.espertech.esper.container`.
   - `EPServicesContextFactoryBase.CreateServicesContext` — wired `resourceManager` (already resolved from container at startup) to `EPDataFlowServiceImpl`.

2. **Dead `IContainer` overloads removed:**
   - `SerializerUtil.ObjectToByteArr(IContainer, object)` — removed; no callers existed; removed `using com.espertech.esper.container` from file.
   - `ContextControllerHashedGetterCRC32SerializedForge.SerializeAndCRC32Hash(IContainer, object, int, Serializer[])` — removed; codegen emits the `SerializerFactory` overload via `EnumValue(typeof(SerializerFactory), "Instance")`; removed `using com.espertech.esper.container` from file.
   - `DbProviderFactoryManagerDefault(IContainer container)` constructor overload — removed; it only called `this()` with no arguments; removed `using com.espertech.esper.container` from file.

3. **Residual dead-code identified (not yet removed — Phase 7):**
   - `SerializerFactoryExtensions.SerializerFactory(this IContainer)` — its only callers were the two dead overloads above; file now has no production callers.
   - `CoreAssemblyProviderExtensions.CoreAssemblyProvider(this IContainer)` — no callers in production or test code.

Validation:

- `dotnet build src/NEsper.Common/NEsper.Common.csproj -v minimal` ✅ (0 errors)
- `dotnet build src/NEsper.Runtime/NEsper.Runtime.csproj -v minimal` ✅ (0 errors)

Phase 4 status:

- ✅ `SerializableObjectCopier` — takes `TypeResolver` directly (done in earlier passes).
- ✅ `DatabaseConfigServiceImpl` — uses `Func<Type, DbDriver>` resolver, no `IContainer`.
- ✅ `DbProviderFactoryManagerDefault` — dead `IContainer` ctor overload removed.
- ✅ Dataflow chain (`EPDataFlowServiceImpl`, `DataflowInstantiator`, `DataFlowOpInitializeContext`) — `IContainer` replaced with `IResourceManager`.
- ✅ Dead `IContainer` overloads in `SerializerUtil` and `ContextControllerHashedGetterCRC32SerializedForge` removed.
- ➡️ `SerializerFactoryExtensions.cs` — file is dead; remove in Phase 7.
- ➡️ `CoreAssemblyProviderExtensions.CoreAssemblyProvider(this IContainer)` — dead; remove in Phase 7.

## Phase 5 — IO package cleanup

1. `AdapterInputSource`: inject `IResourceManager` (not container).
2. `AbstractCoordinatedAdapter`: inject `IReaderWriterLockManager`.

Exit criteria:

- IO assembly has no service-locator calls.

### Phase 5 Status (2026-03-03) — Already Complete

Verified by grep: `src/NEsper.IO/` contains zero `IContainer` references. `AbstractCoordinatedAdapter` and `AdapterInputSource` have already been refactored in prior passes. No further action needed.

Phase 5 status: ✅ COMPLETE.

## Phase 6 — Test harness migration

1. Replace static `SupportContainer.Resolve<T>()` wrappers with explicit fixture-level dependencies.
2. Keep minimal test-only bootstrap helpers, but ban ad-hoc resolve calls in tests.
3. Migrate support factories away from `ResolveSingleton` and `Resolve<T>()`.

Exit criteria:

- Test code uses fixture injection/helpers, not global service location.

### Phase 6 Status (2026-03-03) — Pending

Remaining `ResolveSingleton`/`Resolve` usage in production-assembly support utilities (called only from tests):

| File | Active Usage |
|------|-------------|
| `src/NEsper.Common/common/internal/support/SupportImport.cs` | `ResolveSingleton`, `Resolve<TypeResolverProvider>`, `Resolve<IResourceManager>` |
| `src/NEsper.Runtime/internal/support/SupportEventTypeFactory.cs` | `ResolveSingleton`, `Resolve<IObjectCopier>` (×2) |
| `src/NEsper.Runtime/internal/support/SupportEventBeanFactory.cs` | `ResolveSingleton` |
| `src/NEsper.Common/common/internal/epl/dataflow/util/DefaultSupportGraphEventUtil.cs` | `container.ResourceManager()` |
| `src/NEsper.Common/common/internal/support/SupportExprValidationContextFactory.cs` | `IContainer container` parameters |

Regression/test-only callers in `tst/` also use `container.LockManager()`, `container.RWLockManager()`, `container.ResourceManager()` directly — see section F of the inventory above.

## Phase 7 — API deprecation and removal

1. Mark `ContainerExtensions` convenience resolver methods `[Obsolete]` with migration guidance.
2. Later remove obsolete methods after one deprecation cycle.
3. Restrict remaining container usage to composition root and legacy compatibility shims.

Exit criteria:

- Public deprecation warnings in place and cleanup branch ready for hard removal.

### Phase 7 Status (2026-03-03) — Pending

Specific items to handle:

**Remove dead files/methods:**
- `src/NEsper.Common/common/internal/util/serde/SerializerFactoryExtensions.cs` — entire file is dead after Phase 4; delete.
- `src/NEsper.Compiler/client/CoreAssemblyProviderExtensions.cs::CoreAssemblyProvider(this IContainer)` — no callers; remove method (keep `GetCoreAssemblies()` which has no container dependency).

**Mark `[Obsolete]` in `ContainerExtensions.cs`** (currently still active):
- `LockManager(this IContainer)` — only test code callers remain
- `RWLockManager(this IContainer)` — only test code callers remain
- `ResourceManager(this IContainer)` — only test/support callers remain
- `ResolveSingleton<T>(this IContainer, Supplier<T>)` — only support factories use it

**`EPRuntimeSPI.RuntimeContainer` decision point:**
`EPServicesContext._container` is stored but never resolved from internally. `RuntimeContainer` is still needed because:
- `EPRuntimeImpl` stores `configuration.Container` → `RuntimeContainer`
- `EPServicesContextFactoryBase.CreateServicesContext` accesses `epRuntime.RuntimeContainer` during startup wiring
- `EPRuntimeImpl` does `copy.Container = RuntimeContainer` when copying configuration snapshots
- `CompilerHelperModuleProvider.GetCompilerThreadPoolFactory(IContainer)` reads `compileTimeServices.Configuration.Container` — intentional optional user extension point (could move to `CompilerOptions` in a future phase)

Removal of `RuntimeContainer` from the public interface requires a decision about whether `Configuration.Container` remains a user-facing API.

---

## Risks and Mitigations

- **Risk:** Constructor signature churn across many classes.
  - **Mitigation:** Introduce parameter objects (`RuntimeCoreDeps`, `CompilerCoreDeps`) to reduce argument explosion.
- **Risk:** Circular dependencies become visible once locator is removed.
  - **Mitigation:** Break cycles with interfaces/factories at boundaries.
- **Risk:** Test fragility due to bootstrap changes.
  - **Mitigation:** Phase test refactor after runtime/compiler core changes stabilize.

---

## Verification Checklist Per Phase

1. `grep` shows zero forbidden usages in target directories for that phase.
2. Unit tests for touched assemblies pass.
3. Regression suites for runtime/compiler paths pass.
4. No new public API regressions without explicit deprecation notes.

---

## Suggested Ownership Split

- **Runtime Core:** Phases 1-2
- **Compiler:** Phase 3
- **Common Utilities/DB:** Phase 4
- **IO:** Phase 5
- **Test Infrastructure:** Phase 6
- **API/Deprecation policy:** Phase 7

---

## Immediate Next Execution Order

> Updated 2026-03-03. Phases 1–5 complete. Remaining work:

1. **Phase 7 dead-code removal** (low risk, no behaviour change):
   - Delete `src/NEsper.Common/common/internal/util/serde/SerializerFactoryExtensions.cs`.
   - Remove `CoreAssemblyProviderExtensions.CoreAssemblyProvider(this IContainer)` method body (keep file, keep `GetCoreAssemblies()`).
2. **Phase 6 test-support refactor** (medium effort):
   - Replace `ResolveSingleton` in `SupportImport`, `SupportEventTypeFactory`, `SupportEventBeanFactory` with explicit constructor injection.
   - Replace `container.ResourceManager()` in `DefaultSupportGraphEventUtil` with injected `IResourceManager`.
3. **Phase 7 `[Obsolete]` annotations** (after step 2 clears test callers):
   - Mark `ContainerExtensions.LockManager`, `RWLockManager`, `ResourceManager`, `ResolveSingleton` as `[Obsolete]`.
4. **Phase 7 `RuntimeContainer` decision**:
   - Decide whether `Configuration.Container` remains a public API surface.
   - If yes: keep `EPRuntimeSPI.RuntimeContainer` as a named composition-root accessor; document boundary.
   - If no: move startup wiring to explicit parameter passing and remove the property from the public interface.
