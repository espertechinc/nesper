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

> Status: Identified from current source via grep/code search on 2026-03-02.

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

- `src/NEsper.Runtime/internal/kernel/service/EPServicesContextFactoryBase.cs`
  - `epRuntime.Container.RWLockManager()`
  - `container.Resolve<TypeResolverProvider>()`
  - `container.Resolve<IResourceManager>()`
  - `container.RWLockManager()`
  - `container.ThreadLocalManager()`
- `src/NEsper.Runtime/internal/kernel/service/EPServicesContextFactoryDefault.cs`
  - `_container.RWLockManager()`
  - `_container.Resolve<IObjectCopier>()`
- `src/NEsper.Runtime/internal/kernel/service/EPServicesContext.cs`
  - `_container.Resolve<ILockManager>()`
  - `_container.Resolve<IReaderWriterLockManager>()`
  - `_container.Resolve<IThreadLocalManager>()`
  - Exposes `public IContainer Container => _container;`
- `src/NEsper.Runtime/internal/kernel/statement/EPStatementInitServicesImpl.cs`
  - `ServicesContext.Container.Resolve<IObjectCopier>()`
  - Exposes `public IContainer Container => ServicesContext.Container;`
- `src/NEsper.Runtime/internal/kernel/service/EPRuntimeImpl.cs`
  - `Container.Resolve<TypeResolverProvider>()`
  - Exposes `public IContainer Container { get; }`

### Runtime stage recovery

- `src/NEsper.Runtime/internal/kernel/stage/StageRecoveryServiceBase.cs`
  - `servicesContext.Container.RWLockManager()`
- `src/NEsper.Runtime/internal/kernel/stage/StageRecoveryServiceImpl.cs`
  - `servicesContext.Container.RWLockManager()`

### Runtime utility/support classes in production assembly

- `src/NEsper.Runtime/internal/timer/TimerServiceImpl.cs`
  - `Container.Resolve<ITimerFactory>()`
- `src/NEsper.Runtime/internal/kernel/service/EPEventServiceHelper.cs`
  - `container.ThreadLocalManager()`
- `src/NEsper.Runtime/internal/kernel/service/EPRuntimeBeanAnonymousTypeService.cs`
  - `container.Resolve<IObjectCopier>()`, `_container.Resolve<IObjectCopier>()`
- `src/NEsper.Runtime/internal/support/SupportEventTypeFactory.cs`
  - `ResolveSingleton`, `Resolve<IObjectCopier>()`
- `src/NEsper.Runtime/internal/support/SupportEventBeanFactory.cs`
  - `ResolveSingleton`

## C) Compiler production call sites (`src/NEsper.Compiler`)

- `src/NEsper.Compiler/internal/util/CompilerHelperServices.cs`
  - `container.Resolve<IObjectCopier>()`
  - `container.Resolve<IThreadLocalManager>()`
  - `container.Resolve<TypeResolverProvider>()`
  - `container.Resolve<IResourceManager>()`
- `src/NEsper.Compiler/internal/util/EPCompilerImpl.cs`
  - `container.Resolve<TypeResolverProvider>()`
  - `container.Resolve<IResourceManager>()`
- `src/NEsper.Compiler/client/CoreAssemblyProviderExtensions.cs`
  - `container.Resolve<CoreAssemblyProvider>()`

## D) Common production call sites (`src/NEsper.Common`)

- `src/NEsper.Common/common/internal/util/SerializableObjectCopier.cs`
  - `_container.Resolve<TypeResolver>()`
  - `_container.Resolve<TypeResolverProvider>()`
  - `_container.Resolve<ObjectSerializer>()`
- `src/NEsper.Common/common/internal/util/serde/SerializerFactory.cs`
  - `container.Resolve<TypeResolver>()`
  - `container.Resolve<TypeResolverProvider>()`
- `src/NEsper.Common/common/internal/util/serde/SerializerFactoryExtensions.cs`
  - `container.Resolve<SerializerFactory>()`
- `src/NEsper.Common/common/internal/util/TransientConfigurationResolver.cs`
  - `container.Resolve<ClassForNameProvider>()`
- `src/NEsper.Common/common/internal/support/SupportClasspathImport.cs`
  - `ResolveSingleton`, `Resolve<TypeResolverProvider>()`, `Resolve<IResourceManager>()`
- `src/NEsper.Common/common/internal/db/DbDriverConnectionHelper.cs`
  - `container.Resolve<DbDriver>(name)`
- `src/NEsper.Common/common/internal/db/drivers/DbProviderFactoryManagerDefault.cs`
  - `_container.Resolve<DbProviderFactory>(factoryName)`
- `src/NEsper.Common/common/internal/epl/dataflow/util/DefaultSupportGraphEventUtil.cs`
  - `container.ResourceManager()`
- `src/NEsper.Common/common/client/configuration/Configuration.cs`
  - `Container.ResourceManager()`
- `src/NEsper.Common/common/client/artifact/*Extensions.cs`
  - `Resolve<IArtifactRepositoryManager>()`, `Resolve<MetadataReferenceResolver>()`, `Resolve<MetadataReferenceProvider>()`

## E) IO production call sites (`src/NEsper.IO`)

- `src/NEsper.IO/AdapterInputSource.cs`
  - `_container.ResourceManager().GetResourceAsStream(...)`
- `src/NEsper.IO/AbstractCoordinatedAdapter.cs`
  - `_container.RWLockManager().CreateLock(...)`

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

## Phase 4 — Common utility and DB subsystem cleanup

1. `SerializableObjectCopier`, `SerializerFactory*`, `TransientConfigurationResolver`:
   - Replace dynamic lookup with explicit constructor args/factory args.
2. DB stack (`DbDriverConnectionHelper`, `DbProviderFactoryManagerDefault`):
   - Introduce explicit registries/factories instead of named container lookups.
3. `Configuration.ResourceManager` convenience property:
   - Mark obsolete and provide explicit alternative wiring path.

Exit criteria:

- No runtime dynamic resolve in utility/DB code.

## Phase 5 — IO package cleanup

1. `AdapterInputSource`: inject `IResourceManager` (not container).
2. `AbstractCoordinatedAdapter`: inject `IReaderWriterLockManager`.

Exit criteria:

- IO assembly has no service-locator calls.

## Phase 6 — Test harness migration

1. Replace static `SupportContainer.Resolve<T>()` wrappers with explicit fixture-level dependencies.
2. Keep minimal test-only bootstrap helpers, but ban ad-hoc resolve calls in tests.
3. Migrate support factories away from `ResolveSingleton` and `Resolve<T>()`.

Exit criteria:

- Test code uses fixture injection/helpers, not global service location.

## Phase 7 — API deprecation and removal

1. Mark `ContainerExtensions` convenience resolver methods `[Obsolete]` with migration guidance.
2. Later remove obsolete methods after one deprecation cycle.
3. Restrict remaining container usage to composition root and legacy compatibility shims.

Exit criteria:

- Public deprecation warnings in place and cleanup branch ready for hard removal.

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

1. Runtime hot-path cleanup (`EPServicesContextFactoryBase`, `EPServicesContext`, `TimerServiceImpl`).
2. Stage recovery + statement init service cleanup.
3. Compiler service chain cleanup.
4. Common utility/DB cleanup.
5. IO and tests.
6. Mark extension methods obsolete and enforce CI rule from warning -> failure.
