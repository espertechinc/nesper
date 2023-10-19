///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.compiler;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.declared.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.common.@internal.epl.index.compile;
using com.espertech.esper.common.@internal.epl.namedwindow.compile;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.script.compiletime;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtypefactory;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.runtime.@event;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.statemgmtsettings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.compiler.client.option;
using com.espertech.esper.compiler.@internal.compiler.abstraction;
using com.espertech.esper.container;

using static com.espertech.esper.common.@internal.@event.json.compiletime.JsonEventTypeUtility;

namespace com.espertech.esper.compiler.@internal.util
{
	public class CompilerHelperServices
	{
		public static ModuleCompileTimeServices GetCompileTimeServices(
			CompilerArguments arguments,
			string moduleName,
			ICollection<string> moduleUses,
			bool isFireAndForget)
		{
			try {
				return GetServices(arguments, moduleName, moduleUses, isFireAndForget);
			}
			catch (EPCompileException) {
				throw;
			}
			catch (Exception ex) {
				throw new EPCompileException(
					"Failed compiler startup: " + ex.Message,
					ex,
					EmptyList<EPCompileExceptionItem>.Instance);
			}
		}

		private static ModuleCompileTimeServices GetServices(
			CompilerArguments arguments,
			string moduleName,
			ICollection<string> moduleUses,
			bool isFireAndForget)
		{
			var configuration = arguments.Configuration;
			var path = arguments.Path;
			var options = arguments.Options;

			// script
			var scriptCompiler = MakeScriptCompiler(configuration);

			// imports
			var importServiceCompileTime = MakeImportService(configuration);
			var typeResolverParent = new ParentTypeResolver(importServiceCompileTime.TypeResolver);
			var container = importServiceCompileTime.Container;

			// resolve pre-configured bean event types, make bean-stem service
			var resolvedBeanEventTypes = BeanEventTypeRepoUtil.ResolveBeanEventTypes(
				configuration.Common.EventTypeNames,
				importServiceCompileTime);
			var beanEventTypeStemService = BeanEventTypeRepoUtil.MakeBeanEventTypeStemService(
				configuration,
				resolvedBeanEventTypes,
				EventBeanTypedEventFactoryCompileTime.INSTANCE);

			// allocate repositories
			var eventTypeRepositoryPreconfigured = new EventTypeRepositoryImpl(true);
			var eventTypeCompileRegistry =
				new EventTypeCompileTimeRegistry(eventTypeRepositoryPreconfigured);
			var beanEventTypeFactoryPrivate = new BeanEventTypeFactoryPrivate(
				EventBeanTypedEventFactoryCompileTime.INSTANCE,
				EventTypeFactoryImpl.GetInstance(container),
				beanEventTypeStemService);
			var variableRepositoryPreconfigured = new VariableRepositoryPreconfigured();

			// allocate path registries
			var pathEventTypes = new PathRegistry<string, EventType>(PathRegistryObjectType.EVENTTYPE);
			var pathNamedWindows =
				new PathRegistry<string, NamedWindowMetaData>(PathRegistryObjectType.NAMEDWINDOW);
			var pathTables = new PathRegistry<string, TableMetaData>(PathRegistryObjectType.TABLE);
			var pathContexts = new PathRegistry<string, ContextMetaData>(PathRegistryObjectType.CONTEXT);
			var pathVariables = new PathRegistry<string, VariableMetaData>(PathRegistryObjectType.VARIABLE);
			var pathExprDeclared =
				new PathRegistry<string, ExpressionDeclItem>(PathRegistryObjectType.EXPRDECL);
			var pathScript =
				new PathRegistry<NameAndParamNum, ExpressionScriptProvided>(PathRegistryObjectType.SCRIPT);
			var pathClassProvided =
				new PathRegistry<string, ClassProvided>(PathRegistryObjectType.CLASSPROVIDED);

			// add runtime-path which is the information an existing runtime may have
			if (path.CompilerPathables != null) {
				foreach (var pathable in path.CompilerPathables) {
					var impl = (EPCompilerPathableImpl)pathable;
					pathVariables.MergeFrom(impl.VariablePathRegistry);
					pathEventTypes.MergeFrom(impl.EventTypePathRegistry);
					pathExprDeclared.MergeFrom(impl.ExprDeclaredPathRegistry);
					pathNamedWindows.MergeFrom(impl.NamedWindowPathRegistry);
					pathTables.MergeFrom(impl.TablePathRegistry);
					pathContexts.MergeFrom(impl.ContextPathRegistry);
					pathScript.MergeFrom(impl.ScriptPathRegistry);
					pathClassProvided.MergeFrom(impl.ClassProvidedPathRegistry);
					eventTypeRepositoryPreconfigured.MergeFrom(impl.EventTypePreconfigured);
					variableRepositoryPreconfigured.MergeFrom(impl.VariablePreconfigured);

					AddJsonUnderlyingClass(pathEventTypes, typeResolverParent);
				}
			}

			// build preconfigured type system
			EventTypeRepositoryBeanTypeUtil.BuildBeanTypes(
				beanEventTypeStemService,
				eventTypeRepositoryPreconfigured,
				resolvedBeanEventTypes,
				beanEventTypeFactoryPrivate,
				configuration.Common.EventTypesBean);
			EventTypeRepositoryMapTypeUtil.BuildMapTypes(
				eventTypeRepositoryPreconfigured,
				configuration.Common.MapTypeConfigurations,
				configuration.Common.EventTypesMapEvents,
				configuration.Common.EventTypesNestableMapEvents,
				beanEventTypeFactoryPrivate,
				importServiceCompileTime);
			EventTypeRepositoryOATypeUtil.BuildOATypes(
				eventTypeRepositoryPreconfigured,
				configuration.Common.ObjectArrayTypeConfigurations,
				configuration.Common.EventTypesNestableObjectArrayEvents,
				beanEventTypeFactoryPrivate,
				importServiceCompileTime);
			var eventTypeXMLXSDHandler = EventTypeXMLXSDHandlerFactory.Resolve(
				importServiceCompileTime,
				configuration.Common.EventMeta,
				EventTypeXMLXSDHandler.HANDLER_IMPL);
			var xmlFragmentEventTypeFactory = new XMLFragmentEventTypeFactory(
				beanEventTypeFactoryPrivate,
				eventTypeCompileRegistry,
				eventTypeRepositoryPreconfigured,
				eventTypeXMLXSDHandler);
			EventTypeRepositoryXMLTypeUtil.BuildXMLTypes(
				eventTypeRepositoryPreconfigured,
				configuration.Common.EventTypesXMLDOM,
				beanEventTypeFactoryPrivate,
				xmlFragmentEventTypeFactory,
				importServiceCompileTime,
				eventTypeXMLXSDHandler);
			var eventTypeAvroHandler = EventTypeAvroHandlerFactory.Resolve(
				importServiceCompileTime,
				configuration.Common.EventMeta.AvroSettings,
				EventTypeAvroHandlerConstants.COMPILE_TIME_HANDLER_IMPL);
			EventTypeRepositoryAvroTypeUtil.BuildAvroTypes(
				eventTypeRepositoryPreconfigured,
				configuration.Common.EventTypesAvro,
				eventTypeAvroHandler,
				beanEventTypeFactoryPrivate.EventBeanTypedEventFactory);
			EventTypeRepositoryVariantStreamUtil.BuildVariantStreams(
				eventTypeRepositoryPreconfigured,
				configuration.Common.VariantStreams,
                EventTypeFactoryImpl.GetInstance(container));

			// build preconfigured variables
			VariableUtil.ConfigureVariables(
				variableRepositoryPreconfigured,
				configuration.Common.Variables,
				importServiceCompileTime,
				EventBeanTypedEventFactoryCompileTime.INSTANCE,
				eventTypeRepositoryPreconfigured,
				beanEventTypeFactoryPrivate);

			var deploymentNumber = -1;
			foreach (var unit in path.Compileds) {
				deploymentNumber++;

				var cachePathable = ((CompilerPathCacheImpl)options.PathCache)?.Get(unit);
				if (cachePathable != null) {
					try {
						pathVariables.MergeFromCheckDuplicate(
							cachePathable.VariablePathRegistry,
							cachePathable.OptionalModuleName);
						pathEventTypes.MergeFromCheckDuplicate(
							cachePathable.EventTypePathRegistry,
							cachePathable.OptionalModuleName);
						pathExprDeclared.MergeFromCheckDuplicate(
							cachePathable.ExprDeclaredPathRegistry,
							cachePathable.OptionalModuleName);
						pathNamedWindows.MergeFromCheckDuplicate(
							cachePathable.NamedWindowPathRegistry,
							cachePathable.OptionalModuleName);
						pathTables.MergeFromCheckDuplicate(
							cachePathable.TablePathRegistry,
							cachePathable.OptionalModuleName);
						pathContexts.MergeFromCheckDuplicate(
							cachePathable.ContextPathRegistry,
							cachePathable.OptionalModuleName);
						pathScript.MergeFromCheckDuplicate(
							cachePathable.ScriptPathRegistry,
							cachePathable.OptionalModuleName);
						pathClassProvided.MergeFromCheckDuplicate(
							cachePathable.ClassProvidedPathRegistry,
							cachePathable.OptionalModuleName);
					}
					catch (PathException ex) {
						throw new EPCompileException(
							"Invalid path: " + ex.Message,
							ex,
							EmptyList<EPCompileExceptionItem>.Instance);
					}

					cachePathable.EventTypePathRegistry.Traverse((EventType type) =>
						AddJsonUnderlyingClass(type, typeResolverParent, null));
					continue;
				}

				var provider = ModuleProviderUtil.Analyze(unit, typeResolverParent, pathClassProvided);
				var unitModuleName = provider.ModuleProvider.ModuleName;

				// initialize event types
				var moduleTypes = new LinkedHashMap<string,EventType>();
				var eventTypeResolver = new EventTypeResolverImpl(
					moduleTypes,
					pathEventTypes,
					eventTypeRepositoryPreconfigured,
					beanEventTypeFactoryPrivate,
					EventSerdeFactoryDefault.INSTANCE);
				var eventTypeCollector = new EventTypeCollectorImpl(
					container,
					moduleTypes,
					beanEventTypeFactoryPrivate,
					provider.TypeResolver,
                    EventTypeFactoryImpl.GetInstance(container),
					beanEventTypeStemService,
					eventTypeResolver,
					xmlFragmentEventTypeFactory,
					eventTypeAvroHandler,
					EventBeanTypedEventFactoryCompileTime.INSTANCE,
					importServiceCompileTime,
					eventTypeXMLXSDHandler);
				try {
					provider.ModuleProvider.InitializeEventTypes(
						new EPModuleEventTypeInitServicesImpl(eventTypeCollector, eventTypeResolver));
				}
				catch (Exception e) {
					throw new EPException(e);
				}

				AddJsonUnderlyingClass(moduleTypes, typeResolverParent, null);

				// initialize named windows
				var moduleNamedWindows = new Dictionary<string, NamedWindowMetaData>();
				var namedWindowCollector = new NamedWindowCollectorImpl(moduleNamedWindows);
				try {
					provider.ModuleProvider.InitializeNamedWindows(
						new EPModuleNamedWindowInitServicesImpl(namedWindowCollector, eventTypeResolver));
				}
				catch (Exception e) {
					throw new EPException(e);
				}

				// initialize tables
				var moduleTables = new Dictionary<string, TableMetaData>();
				var tableCollector = new TableCollectorImpl(moduleTables);
				try {
					provider.ModuleProvider.InitializeTables(
						new EPModuleTableInitServicesImpl(tableCollector, eventTypeResolver));
				}
				catch (Exception e) {
					throw new EPException(e);
				}

				// initialize create-index indexes
				var indexCollector = new IndexCollectorCompileTime(
					moduleNamedWindows,
					moduleTables,
					pathNamedWindows,
					pathTables);
				try {
					provider.ModuleProvider.InitializeIndexes(new EPModuleIndexInitServicesImpl(indexCollector));
				}
				catch (Exception e) {
					throw new EPException(e);
				}

				// initialize create-contexts
				var moduleContexts = new Dictionary<string, ContextMetaData>();
				var contextCollector = new ContextCollectorImpl(moduleContexts);
				try {
					provider.ModuleProvider.InitializeContexts(
						new EPModuleContextInitServicesImpl(contextCollector, eventTypeResolver));
				}
				catch (Exception e) {
					throw new EPException(e);
				}

				// initialize variables
				var moduleVariables = new Dictionary<string, VariableMetaData>();
				var variableCollector = new VariableCollectorImpl(moduleVariables);
				try {
					provider.ModuleProvider.InitializeVariables(
						new EPModuleVariableInitServicesImpl(variableCollector, eventTypeResolver));
				}
				catch (Exception e) {
					throw new EPException(e);
				}

				// initialize module expressions
				var moduleExprDeclareds = new Dictionary<string, ExpressionDeclItem>();
				var exprDeclaredCollector = new ExprDeclaredCollectorCompileTime(moduleExprDeclareds);
				try {
					provider.ModuleProvider.InitializeExprDeclareds(
						new EPModuleExprDeclaredInitServicesImpl(exprDeclaredCollector));
				}
				catch (Exception e) {
					throw new EPException(e);
				}

				// initialize module scripts
				var moduleScripts = new Dictionary<NameAndParamNum, ExpressionScriptProvided>();
				var scriptCollector = new ScriptCollectorCompileTime(moduleScripts);
				try {
					provider.ModuleProvider.InitializeScripts(new EPModuleScriptInitServicesImpl(scriptCollector));
				}
				catch (Exception e) {
					throw new EPException(e);
				}

				// initialize inlined classes
				var moduleClassProvideds = new Dictionary<string, ClassProvided>();
				var classProvidedCollector = new ClassProvidedCollectorCompileTime(moduleClassProvideds, typeResolverParent);
				var artifactFactory = container.ArtifactRepositoryManager().DefaultRepository;
				
				try {
					provider.ModuleProvider.InitializeClassProvided(
						new EPModuleClassProvidedInitServicesImpl(classProvidedCollector, artifactFactory));
				}
				catch (Exception e) {
					throw new EPException(e);
				}

				// save path-visibility event types and named windows to the path
				var deploymentId = "D" + deploymentNumber;
				if (options.PathCache != null) {
					cachePathable = new EPCompilerPathableImpl(moduleName);
				}

				try {
					foreach (var type in moduleTypes) {
						if (type.Value.Metadata.AccessModifier.IsNonPrivateNonTransient()) {
							pathEventTypes.Add(type.Key, unitModuleName, type.Value, deploymentId);
							cachePathable?.EventTypePathRegistry.Add(
								type.Key,
								unitModuleName,
								type.Value,
								deploymentId);
						}
					}

					foreach (var entry in moduleNamedWindows) {
						if (entry.Value.EventType.Metadata.AccessModifier.IsNonPrivateNonTransient()) {
							pathNamedWindows.Add(entry.Key, unitModuleName, entry.Value, deploymentId);
							cachePathable?.NamedWindowPathRegistry.Add(
								entry.Key,
								unitModuleName,
								entry.Value,
								deploymentId);
						}
					}

					foreach (var entry in moduleTables) {
						if (entry.Value.TableVisibility.IsNonPrivateNonTransient()) {
							pathTables.Add(entry.Key, unitModuleName, entry.Value, deploymentId);
							cachePathable?.TablePathRegistry.Add(
								entry.Key,
								unitModuleName,
								entry.Value,
								deploymentId);
						}
					}

					foreach (var entry in moduleContexts) {
						if (entry.Value.ContextVisibility.IsNonPrivateNonTransient()) {
							pathContexts.Add(entry.Key, unitModuleName, entry.Value, deploymentId);
							cachePathable?.ContextPathRegistry.Add(
								entry.Key,
								unitModuleName,
								entry.Value,
								deploymentId);
						}
					}

					foreach (var entry in moduleVariables) {
						if (entry.Value.VariableVisibility.IsNonPrivateNonTransient()) {
							pathVariables.Add(entry.Key, unitModuleName, entry.Value, deploymentId);
							cachePathable?.VariablePathRegistry.Add(
								entry.Key,
								unitModuleName,
								entry.Value,
								deploymentId);
						}
					}

					foreach (var entry in moduleExprDeclareds) {
						if (entry.Value.Visibility.IsNonPrivateNonTransient()) {
							pathExprDeclared.Add(entry.Key, unitModuleName, entry.Value, deploymentId);
							cachePathable?.ExprDeclaredPathRegistry.Add(
								entry.Key,
								unitModuleName,
								entry.Value,
								deploymentId);
						}
					}

					foreach (var entry in moduleScripts) {
						if (entry.Value.Visibility.IsNonPrivateNonTransient()) {
							pathScript.Add(entry.Key, unitModuleName, entry.Value, deploymentId);
							cachePathable?.ScriptPathRegistry.Add(
								entry.Key,
								unitModuleName,
								entry.Value,
								deploymentId);
						}
					}

					foreach (var entry in moduleClassProvideds) {
						if (entry.Value.Visibility.IsNonPrivateNonTransient()) {
							pathClassProvided.Add(entry.Key, unitModuleName, entry.Value, deploymentId);
							cachePathable?.ClassProvidedPathRegistry.Add(
								entry.Key,
								unitModuleName,
								entry.Value,
								deploymentId);
						}
					}
				}
				catch (PathException ex) {
					throw new EPCompileException(
						"Invalid path: " + ex.Message,
						ex,
						EmptyList<EPCompileExceptionItem>.Instance);
				}

				if (cachePathable != null) {
					((CompilerPathCacheImpl)options.PathCache).Put(unit, cachePathable);
				}
			}

			var moduleDependencies = new ModuleDependenciesCompileTime();

			// build bean space of public and protected
			var eventTypeCompileTimeResolver = new EventTypeCompileTimeResolver(
				moduleName,
				moduleUses,
				eventTypeCompileRegistry,
				eventTypeRepositoryPreconfigured,
				pathEventTypes,
				moduleDependencies,
				isFireAndForget);

			// build named window registry
			var namedWindowCompileTimeRegistry = new NamedWindowCompileTimeRegistry();
			var namedWindowCompileTimeResolver = new NamedWindowCompileTimeResolverImpl(
				moduleName,
				moduleUses,
				namedWindowCompileTimeRegistry,
				pathNamedWindows,
				moduleDependencies,
				isFireAndForget);

			// build context registry
			var contextCompileTimeRegistry = new ContextCompileTimeRegistry();
			var contextCompileTimeResolver = new ContextCompileTimeResolverImpl(
				moduleName,
				moduleUses,
				contextCompileTimeRegistry,
				pathContexts,
				moduleDependencies,
				isFireAndForget);

			// build variable registry
			var variableCompileTimeRegistry = new VariableCompileTimeRegistry();
			var variableCompileTimeResolver = new VariableCompileTimeResolverImpl(
				moduleName,
				moduleUses,
				variableRepositoryPreconfigured,
				variableCompileTimeRegistry,
				pathVariables,
				moduleDependencies,
				isFireAndForget);

			// build declared-expression registry
			var exprDeclaredCompileTimeRegistry = new ExprDeclaredCompileTimeRegistry();
			var exprDeclaredCompileTimeResolver = new ExprDeclaredCompileTimeResolverImpl(
				moduleName,
				moduleUses,
				exprDeclaredCompileTimeRegistry,
				pathExprDeclared,
				moduleDependencies,
				isFireAndForget);

			// build table-registry
			var localTables = new Dictionary<string, TableMetaData>();
			var tableCompileTimeRegistry = new TableCompileTimeRegistry(localTables);
			var tableCompileTimeResolver = new TableCompileTimeResolverImpl(
				moduleName,
				moduleUses,
				tableCompileTimeRegistry,
				pathTables,
				moduleDependencies,
				isFireAndForget);

			// build script registry
			var scriptCompileTimeRegistry = new ScriptCompileTimeRegistry();
			var scriptCompileTimeResolver = new ScriptCompileTimeResolverImpl(
				moduleName,
				moduleUses,
				scriptCompileTimeRegistry,
				pathScript,
				moduleDependencies,
				isFireAndForget);

			// build classes registry
			var classProvidedCompileTimeRegistry = new ClassProvidedCompileTimeRegistry();
			var classProvidedCompileTimeResolver =
				new ClassProvidedCompileTimeResolverImpl(
					moduleName,
					moduleUses,
					classProvidedCompileTimeRegistry,
					pathClassProvided,
					moduleDependencies,
					isFireAndForget);

			// view resolution
			var plugInViews = new PluggableObjectCollection();
			plugInViews.AddViews(
				configuration.Compiler.PlugInViews,
				configuration.Compiler.PlugInVirtualDataWindows,
				importServiceCompileTime);
			var viewRegistry = new PluggableObjectRegistryImpl(
				new PluggableObjectCollection[] { ViewEnumHelper.BuiltinViews, plugInViews });
			ViewResolutionService viewResolutionService = new ViewResolutionServiceImpl(container, viewRegistry);

			var plugInPatternObj = new PluggableObjectCollection();
			plugInPatternObj.AddPatternObjects(
				configuration.Compiler.PlugInPatternObjects,
				importServiceCompileTime);
			plugInPatternObj.AddObjects(PatternObjectHelper.BuiltinPatternObjects);
			PatternObjectResolutionService patternResolutionService =
				new PatternObjectResolutionServiceImpl(plugInPatternObj);

			var indexCompileTimeRegistry =
				new IndexCompileTimeRegistry(new Dictionary<IndexCompileTimeKey, IndexDetailForge>());

			var moduleVisibilityRules =
				new ModuleAccessModifierServiceImpl(options, configuration.Compiler.ByteCode);

			var databaseConfigServiceCompileTime =
				new DatabaseConfigServiceImpl(
					container,
					configuration.Common.DatabaseReferences,
					importServiceCompileTime);

			CompilerServices compilerServices = new CompilerServicesImpl();

			var targetHA = configuration.GetType().Name.EndsWith("ConfigurationHA");
			var stateMgmtSettingsProvider =
				configuration.InternalUseGetStmtMgmtProvider(options.StateMgmtSetting);
			SerdeEventTypeCompileTimeRegistry serdeEventTypeRegistry =
				new SerdeEventTypeCompileTimeRegistryImpl(targetHA);
			var serdeResolver = targetHA
				? MakeSerdeResolver(container, configuration.Compiler.Serde, configuration.Common.TransientConfiguration)
				: SerdeCompileTimeResolverNonHA.INSTANCE;

			CompilerAbstraction compilerAbstraction = CompilerAbstractionRoslyn.INSTANCE;
			if (arguments.Options != null && arguments.Options.CompilerHook != null) {
				var abstraction =
					arguments.Options.CompilerHook.GetValue(new CompilerHookContext(moduleName));
				if (abstraction != null) {
					compilerAbstraction = abstraction;
				}
			}

			return new ModuleCompileTimeServices(
				container,
				compilerAbstraction,
				compilerServices,
				configuration,
				classProvidedCompileTimeRegistry,
				classProvidedCompileTimeResolver,
				contextCompileTimeRegistry,
				contextCompileTimeResolver,
				beanEventTypeStemService,
				beanEventTypeFactoryPrivate,
				databaseConfigServiceCompileTime,
				importServiceCompileTime,
				exprDeclaredCompileTimeRegistry,
				exprDeclaredCompileTimeResolver,
				eventTypeAvroHandler,
				eventTypeCompileRegistry,
				eventTypeCompileTimeResolver,
				eventTypeRepositoryPreconfigured,
				eventTypeXMLXSDHandler,
				isFireAndForget,
				indexCompileTimeRegistry,
				moduleDependencies,
				moduleVisibilityRules,
				namedWindowCompileTimeResolver,
				namedWindowCompileTimeRegistry,
				stateMgmtSettingsProvider,
				typeResolverParent,
				patternResolutionService,
				scriptCompileTimeRegistry,
				scriptCompileTimeResolver,
				scriptCompiler,
				serdeEventTypeRegistry,
				serdeResolver,
				tableCompileTimeRegistry,
				tableCompileTimeResolver,
				variableCompileTimeRegistry,
				variableCompileTimeResolver,
				viewResolutionService,
				xmlFragmentEventTypeFactory);
		}

		public static ScriptCompiler MakeScriptCompiler(Configuration configuration)
		{
			return new ScriptCompilerImpl(
				configuration.Container,
				configuration.Common);
		}

		public static ImportServiceCompileTime MakeImportService(Configuration configuration)
		{
			var timeAbacus = TimeAbacusFactory.Make(configuration.Common.TimeSource.TimeUnit);
			var expression = configuration.Compiler.Expression;
			var importService = new ImportServiceCompileTimeImpl(
                configuration.Container,
				configuration.Common.TransientConfiguration,
				timeAbacus,
                configuration.Common.EventTypeAutoNameNamespaces,
				expression.MathContext,
				expression.IsExtendedAggregation,
				configuration.Compiler.Language.IsSortUsingCollator
			);

			// Add auto-imports
			try {
				foreach (var importName in configuration.Common.Imports) {
					importService.AddImport(importName);
				}

				foreach (var importName in configuration.Common.AnnotationImports) {
					importService.AddAnnotationImport(importName);
				}

				foreach (var config in configuration.Compiler
					         .PlugInAggregationFunctions) {
					importService.AddAggregation(config.Name, config);
				}

				foreach (var config in configuration.Compiler
					         .PlugInAggregationMultiFunctions) {
					importService.AddAggregationMultiFunction(config);
				}

				foreach (var config in configuration.Compiler
					         .PlugInSingleRowFunctions) {
					importService.AddSingleRow(
						config.Name,
						config.FunctionClassName,
						config.FunctionMethodName,
						config.ValueCache,
						config.FilterOptimizable,
						config.RethrowExceptions,
						config.EventTypeName);
				}

				foreach (var config in configuration.Compiler
					         .PlugInDateTimeMethods) {
					importService.AddPlugInDateTimeMethod(config.Name, config);
				}

				foreach (var config in configuration.Compiler.PlugInEnumMethods) {
					importService.AddPlugInEnumMethod(config.Name, config);
				}
			}
			catch (ImportException ex) {
				throw new ConfigurationException("Error configuring compiler: " + ex.Message, ex);
			}

			return importService;
		}

		private static SerdeCompileTimeResolver MakeSerdeResolver(
            IContainer container,
			ConfigurationCompilerSerde config,
			IDictionary<string, object> transientConfiguration)
		{
			var context = new SerdeProviderFactoryContext();

			IList<SerdeProvider> providers = null;
			if (config.SerdeProviderFactories != null) {
				foreach (var factory in config.SerdeProviderFactories) {
					try {
						var instance = TypeHelper.Instantiate<SerdeProviderFactory>(
							factory,
							TransientConfigurationResolver
								.ResolveTypeResolverProvider(container, transientConfiguration)
								.TypeResolver);
						var provider = instance.GetProvider(context);
						if (provider == null) {
							throw new ConfigurationException(
								"Binding provider factory '" + factory + "' returned a null value");
						}

						if (providers == null) {
							providers = new List<SerdeProvider>();
						}

						providers.Add(provider);
					}
					catch (Exception ex) {
						throw new ConfigurationException(
							"Binding provider factory '" + factory + "' failed to initialize: " + ex.Message,
							ex);
					}
				}
			}

			if (providers == null) {
				providers = EmptyList<SerdeProvider>.Instance;
			}

			return new SerdeCompileTimeResolverImpl(
				providers,
				config.IsEnableExtendedBuiltin,
				config.IsEnableSerializable,
				config.IsEnableExternalizable,
				config.IsEnableSerializationFallback);
		}
	}
} // end of namespace
