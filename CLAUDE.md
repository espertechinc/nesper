# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## About NEsper

NEsper is a Complex Event Processing (CEP) engine for .NET, ported from the Java-based Esper. It provides an Event Processing Language (EPL) for querying, analyzing, and processing event streams in real-time.

## Build & Test Commands

### Building the Solution

Build all projects:
```bash
dotnet build NEsperAll.sln
```

Build specific components:
```bash
dotnet build src/NEsper.Common/NEsper.Common.csproj
dotnet build src/NEsper.Compiler/NEsper.Compiler.csproj
dotnet build src/NEsper.Runtime/NEsper.Runtime.csproj
```

### Running Tests

Run all tests:
```bash
dotnet test NEsperAll.sln
```

Run tests for a specific project:
```bash
dotnet test tst/NEsper.Common.Tests/NEsper.Common.Tests.csproj
dotnet test tst/NEsper.Compiler.Tests/NEsper.Compiler.Tests.csproj
dotnet test tst/NEsper.Runtime.Tests/NEsper.Runtime.Tests.csproj
dotnet test tst/NEsper.Compat.Tests/NEsper.Compat.Tests.csproj
```

Run a single test by filter:
```bash
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
```

Run tests with specific settings (uses NEsper.runsettings):
```bash
dotnet test NEsperAll.sln --settings NEsper.runsettings
```

Run regression tests:
```bash
dotnet test tst/NEsper.Regression.Runner/NEsper.Regression.Runner.csproj
```

### Batch Test System (Recommended for Regression Tests)

NEsper includes a configurable batch test system for efficient regression testing with 3,801 tests:

```powershell
# View all available test batches and configuration
.\run-test-batches.ps1 -Summary

# Run all regression tests in organized batches
.\run-test-batches.ps1

# Run specific batch for quick feedback
.\run-test-batches.ps1 -BatchName "Client"          # ~2.5 min, 253 tests
.\run-test-batches.ps1 -BatchName "EPL-Database"    # ~20 sec, 55 tests

# Run with verbose output for debugging
.\run-test-batches.ps1 -BatchName "Event" -Verbose

# Run isolated tests that have state pollution issues
.\run-test-batches.ps1 -IsolatedOnly
```

**Configuration**: Edit `test-batches.json` to adjust batches, timeouts, and filters.

**Documentation**:
- **[QUICK-TEST-GUIDE.md](QUICK-TEST-GUIDE.md)** - Quick reference and common commands
- **[TEST-BATCHES-README.md](TEST-BATCHES-README.md)** - Complete documentation

**Test Statistics**:
- Total Tests: 3,801 regression + 1,890 unit = 5,691 tests
- Pass Rate: 99.87% (4 known XML XPath failures, 4 intermittent isolation issues)
- Frameworks: net8.0, net9.0

### Database Setup for Tests

Some tests require PostgreSQL. Start the database using:
```bash
cd tst/db
docker-compose up -d
```

Database credentials:
- Database: esper
- User: esper
- Password: 3sp3rP@ssw0rd
- Port: 5432

## Architecture Overview

NEsper follows a **compile-then-deploy architecture** with clear separation between compile-time and runtime:

### Core Components

**NEsper.Compat** - Java compatibility layer
- Provides Java-like collections (LinkedHashMap, LinkedHashSet)
- Threading primitives (CountDownLatch, AtomicLong, ReadWriteLock)
- Cross-platform utilities and time handling
- Located in: `src/NEsper.Compat/`

**NEsper.Common** - Shared compile-time and runtime infrastructure
- Event type system (EventBean, EventType, property accessors)
- Bytecode generation model for code generation
- Compilation stages (stage1, stage2, stage3)
- EPL language features (expressions, patterns, aggregations, joins, windows)
- Filter specifications and evaluation
- Located in: `src/NEsper.Common/`
- Root namespace: `com.espertech.esper.common`

**NEsper.Grammar** - ANTLR grammar for EPL parsing
- Contains ANTLR4 grammar files
- Generated parser and lexer for EPL syntax
- Located in: `src/NEsper.Grammar/`

**NEsper.Compiler** - EPL compilation to .NET assemblies
- Entry point: `EPCompilerProvider.Compiler`
- Parses EPL text into AST using ANTLR
- 3-stage compilation pipeline
- Generates C# code using Roslyn
- Outputs `EPCompiled` artifacts containing .NET assemblies
- Located in: `src/NEsper.Compiler/`

**NEsper.Runtime** - Event processing runtime engine
- Entry point: `EPRuntimeProvider.GetDefaultRuntime()`
- Deploys compiled EPL modules
- Processes incoming events
- Manages filter service, scheduler, variables, contexts
- Delivers results to listeners
- Located in: `src/NEsper.Runtime/`

**Optional Components:**
- **NEsper.Avro** - Apache Avro event format support
- **NEsper.IO** - Input/output adapters
- **NEsper.Data.*** - Database drivers (MySQL, PostgreSQL, SQLite, SQLServer, ODBC)
- **NEsper.Scripting.*** - Scripting engine integration (ClearScript, Jurassic)
- **NEsper.Log.NLog** - NLog integration

### EPL Compilation Pipeline (3 Stages)

**Stage 1 - Parse & Validate** (`common/internal/compile/stage1/`)
- Parse EPL text using ANTLR grammar
- Build statement specification (StatementSpec)
- Basic syntax validation

**Stage 2 - Semantic Analysis** (`common/internal/compile/stage2/`)
- Type checking and resolution
- Build filter specifications (FilterSpecCompiled)
- Create compiled specifications (StatementSpecCompiled)
- Validate semantics against event types

**Stage 3 - Code Generation** (`common/internal/compile/stage3/`)
- Generate "forge" descriptors for code generation
- Create StatementAIFactoryProvider classes
- Build CodegenClass model representing C# code
- Generate C# source using Roslyn SyntaxFactory
- Compile to .NET assemblies using Roslyn
- Package as EPCompiled with manifest

### Runtime Deployment & Execution Flow

**Deployment:**
1. Load compiled assemblies from EPCompiled artifact
2. Instantiate ModuleProvider classes
3. Initialize EPL objects (event types, variables, named windows, contexts)
4. Create StatementAIFactoryProvider instances
5. Register with deployment lifecycle service

**Statement Activation:**
1. StatementAIFactoryProvider creates StatementAgentInstanceFactory
2. Factory creates statement resources (filters, views, aggregations)
3. Register with filter service and schedule service
4. Attach update listeners/subscribers

**Event Processing:**
1. Events sent via `EPEventService.SendEvent()`
2. Wrapped as EventBean
3. Filter service evaluates using index-based matching
4. Matching statements receive events
5. Events flow through view chain (windows, patterns, aggregations)
6. Result set processing produces output
7. UpdateListener callbacks invoked with results

### Code Generation Architecture

NEsper uses **code generation instead of interpretation** for maximum performance:

**Why Code Generation:**
- Generated code runs at native .NET speed (no interpretation overhead)
- Full Roslyn optimization applies
- Type-safe compiled expressions
- Debuggable generated code

**Key Generated Classes:**
- `StatementAIFactoryProvider` - Factory for statement instances
- `StatementProvider` - Statement metadata
- Expression evaluators - Compiled EPL expressions
- Event property getters - Fast property access
- Filter evaluators - WHERE clause evaluation
- Aggregation functions - Compiled aggregation logic

**Codegen Model:** (`common/internal/bytecodemodel/`)
- `CodegenClass` - Represents a C# class
- `CodegenMethod` - Represents a method with parameters and body
- `CodegenExpression` - Type-safe expression DSL
- `CodeGenerationHelper` - Converts CodegenClass to Roslyn SyntaxTree

### Key Architectural Patterns

**Forge Pattern:**
- Compile-time "forge" objects create runtime instances
- Example: `FilterSpecParamForge` creates runtime evaluator
- Separation between compile-time description and runtime execution

**Factory Pattern:**
- `StatementAIFactoryProvider` creates statement instances
- Allows multiple statement instances per context partition
- Encapsulates statement initialization logic

**Service Locator (being removed):**
- `EPServicesContext` provides centralized access to runtime services
- **Active refactoring:** eliminating IContainer as a runtime service locator — see `.planning/service-locator-removal/PLAN.md`

**Event-Driven:**
- Asynchronous event processing
- Listener/observer pattern for result delivery
- Filter service uses index structures for efficient matching

**Path/Repository:**
- Compiled modules reference each other through path mechanism
- Supports modular EPL deployments
- Named windows, variables, tables can be shared across modules

## Namespace Convention

All NEsper code uses Java-style namespaces to maintain compatibility with Java Esper:
- Root namespace: `com.espertech.esper`
- Example: `com.espertech.esper.common.client.EventBean`
- This is intentional for cross-platform consistency

## Important Files Reference

**Compilation Entry Points:**
- `src/NEsper.Compiler/client/EPCompiler.cs` - Public API
- `src/NEsper.Compiler/internal/util/EPCompilerImpl.cs` - Main implementation

**Runtime Entry Points:**
- `src/NEsper.Runtime/client/EPRuntime.cs` - Public API
- `src/NEsper.Runtime/internal/kernel/service/EPRuntimeImpl.cs` - Main implementation
- `src/NEsper.Runtime/internal/kernel/service/Deployer.cs` - Deployment orchestration

**Core Abstractions:**
- `src/NEsper.Common/common/client/EventBean.cs` - Event wrapper
- `src/NEsper.Common/common/client/EventType.cs` - Event type metadata
- `src/NEsper.Common/common/client/EPCompiled.cs` - Compiled EPL artifact
- `src/NEsper.Common/common/internal/context/module/StatementAIFactoryProvider.cs` - Statement factory

**Codegen Infrastructure:**
- `src/NEsper.Common/common/internal/bytecodemodel/core/CodegenClass.cs` - Class model
- `src/NEsper.Common/common/internal/bytecodemodel/core/CodeGenerationHelper.cs` - Roslyn integration
- `src/NEsper.Common/common/internal/bytecodemodel/model/expression/` - Expression DSL

**Event System:**
- `src/NEsper.Common/common/internal/event/bean/` - POCO event representation
- `src/NEsper.Common/common/internal/event/map/` - Map-based events
- `src/NEsper.Common/common/internal/event/json/` - JSON events
- `src/NEsper.Common/common/internal/event/xml/` - XML events
- `src/NEsper.Avro/` - Avro event representation

## Target Framework

Projects target both **.NET 8.0** and **.NET 9.0** (multi-targeting):
```xml
<TargetFrameworks>net8.0;net9.0</TargetFrameworks>
```

## Testing Infrastructure

**Test Framework:** NUnit 4.x

**Test Projects:**
- `tst/NEsper.Common.Tests/` - Unit tests for NEsper.Common
- `tst/NEsper.Compiler.Tests/` - Unit tests for NEsper.Compiler
- `tst/NEsper.Runtime.Tests/` - Unit tests for NEsper.Runtime
- `tst/NEsper.Compat.Tests/` - Unit tests for compatibility layer
- `tst/NEsper.Regression/` - Comprehensive regression test suite
- `tst/NEsper.Regression.Runner/` - Runner for regression tests

**Test Settings:**
- `NEsper.runsettings` configures test execution
- Default timeout: 300000ms (5 minutes)
- Uses parallel test workers

## Common Development Patterns

### Exception Handling
- Most exceptions inherit from `EPException` (common/client/EPException.cs)
- Compile-time exceptions: `EPCompileException`
- Runtime exceptions: `EPRuntimeException`, `EPDeployException`
- Configuration exceptions: `ConfigurationException`
- All exceptions now use modern C# patterns (no Java-style exception chaining)

### Working with Events
Events are wrapped as `EventBean` which provides:
- `EventType` - Metadata about event structure
- `Get(string propertyName)` - Generic property access
- Property getters are code-generated for performance

### Working with EPL Statements
```csharp
// Compile EPL
var compiler = EPCompilerProvider.Compiler;
var compiled = compiler.Compile("select * from MyEvent", args);

// Deploy to runtime
var runtime = EPRuntimeProvider.GetDefaultRuntime(config);
var deployment = runtime.DeploymentService.Deploy(compiled);

// Attach listener
var statement = deployment.Statements[0];
statement.AddListener(new MyUpdateListener());

// Send events
runtime.EventService.SendEventBean(eventBean, "MyEvent");
```

## Solution Structure

- **NEsperAll.sln** - Complete solution with all projects
- **NEsper.sln** - Core projects only
- **NEsper.Documentation.sln** - Documentation projects

## Active Refactoring Work

Plans are stored in `.planning/` — see `.planning/README.md` for the index.

| Branch | Plan | Status |
|--------|------|--------|
| `service-locator-refactor` | `.planning/service-locator-removal/PLAN.md` | Planned |

## Additional Notes

- The codebase is a port from Java Esper, so you'll see Java idioms translated to C#
- Code generation is central to performance - understand the forge→codegen→runtime flow
- The three-stage compilation model separates parsing, semantic analysis, and code generation
- Tests are comprehensive - regression suite has thousands of test cases
- Database tests require PostgreSQL running via Docker
