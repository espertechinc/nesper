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
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
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
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.container;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilerHelperServices
    {
        protected internal static ModuleCompileTimeServices GetCompileTimeServices(
            CompilerArguments arguments,
            string moduleName,
            ICollection<string> moduleUses)
        {
            try {
                return GetServices(arguments, moduleName, moduleUses);
            }
            catch (EPCompileException) {
                throw;
            }
            catch (Exception t) {
                throw new EPCompileException("Failed compiler startup: " + t.Message, t, new EmptyList<EPCompileExceptionItem>());
            }
        }

        private static ModuleCompileTimeServices GetServices(
            CompilerArguments arguments,
            string moduleName,
            ICollection<string> moduleUses)
        {
            var configuration = arguments.Configuration;
            var path = arguments.Path;
            var options = arguments.Options;

            // imports
            var importServiceCompileTime = MakeImportService(configuration);
            var container = importServiceCompileTime.Container;

            // resolve pre-configured bean event types, make bean-stem service
            var resolvedBeanEventTypes = BeanEventTypeRepoUtil.ResolveBeanEventTypes(configuration.Common.EventTypeNames, importServiceCompileTime);
            var beanEventTypeStemService = BeanEventTypeRepoUtil.MakeBeanEventTypeStemService(
                configuration, resolvedBeanEventTypes, EventBeanTypedEventFactoryCompileTime.INSTANCE);

            // build preconfigured type system
            var eventTypeRepositoryPreconfigured = new EventTypeRepositoryImpl(true);
            var eventTypeCompileRegistry = new EventTypeCompileTimeRegistry(eventTypeRepositoryPreconfigured);
            var beanEventTypeFactoryPrivate = new BeanEventTypeFactoryPrivate(
                EventBeanTypedEventFactoryCompileTime.INSTANCE, EventTypeFactoryImpl.GetInstance(container), beanEventTypeStemService);
            EventTypeRepositoryBeanTypeUtil.BuildBeanTypes(
                beanEventTypeStemService, eventTypeRepositoryPreconfigured, resolvedBeanEventTypes, beanEventTypeFactoryPrivate,
                configuration.Common.EventTypesBean);
            EventTypeRepositoryMapTypeUtil.BuildMapTypes(
                eventTypeRepositoryPreconfigured, configuration.Common.MapTypeConfigurations, configuration.Common.EventTypesMapEvents,
                configuration.Common.EventTypesNestableMapEvents, beanEventTypeFactoryPrivate, importServiceCompileTime);
            EventTypeRepositoryOATypeUtil.BuildOATypes(
                eventTypeRepositoryPreconfigured, configuration.Common.ObjectArrayTypeConfigurations,
                configuration.Common.EventTypesNestableObjectArrayEvents, beanEventTypeFactoryPrivate, importServiceCompileTime);
            var xmlFragmentEventTypeFactory = new XMLFragmentEventTypeFactory(
                beanEventTypeFactoryPrivate, eventTypeCompileRegistry, eventTypeRepositoryPreconfigured);
            EventTypeRepositoryXMLTypeUtil.BuildXMLTypes(
                eventTypeRepositoryPreconfigured, 
                configuration.Common.EventTypesXMLDOM, 
                beanEventTypeFactoryPrivate, 
                xmlFragmentEventTypeFactory,
                container.ResourceManager());
                //importServiceCompileTime);
            var eventTypeAvroHandler = EventTypeAvroHandlerFactory.Resolve(
                importServiceCompileTime, configuration.Common.EventMeta.AvroSettings, EventTypeAvroHandlerConstants.COMPILE_TIME_HANDLER_IMPL);
            EventTypeRepositoryAvroTypeUtil.BuildAvroTypes(
                eventTypeRepositoryPreconfigured, configuration.Common.EventTypesAvro, eventTypeAvroHandler,
                beanEventTypeFactoryPrivate.EventBeanTypedEventFactory);
            EventTypeRepositoryVariantStreamUtil.BuildVariantStreams(
                eventTypeRepositoryPreconfigured, configuration.Common.VariantStreams, EventTypeFactoryImpl.GetInstance(container));

            // build preconfigured variables
            var variableRepositoryPreconfigured = new VariableRepositoryPreconfigured();
            VariableUtil.ConfigureVariables(
                variableRepositoryPreconfigured, configuration.Common.Variables, importServiceCompileTime,
                EventBeanTypedEventFactoryCompileTime.INSTANCE, eventTypeRepositoryPreconfigured, beanEventTypeFactoryPrivate);

            // determine all event types that are in path
            var pathEventTypes = new PathRegistry<string, EventType>(PathRegistryObjectType.EVENTTYPE);
            var pathNamedWindows = new PathRegistry<string, NamedWindowMetaData>(PathRegistryObjectType.NAMEDWINDOW);
            var pathTables = new PathRegistry<string, TableMetaData>(PathRegistryObjectType.TABLE);
            var pathContexts = new PathRegistry<string, ContextMetaData>(PathRegistryObjectType.CONTEXT);
            var pathVariables = new PathRegistry<string, VariableMetaData>(PathRegistryObjectType.VARIABLE);
            var pathExprDeclared = new PathRegistry<string, ExpressionDeclItem>(PathRegistryObjectType.EXPRDECL);
            var pathScript = new PathRegistry<NameAndParamNum, ExpressionScriptProvided>(PathRegistryObjectType.SCRIPT);

            var deploymentNumber = -1;

            foreach (var unit in path.Compileds) {
                deploymentNumber++;
                var provider = ModuleProviderUtil.Analyze(unit, importServiceCompileTime);
                var unitModuleName = provider.ModuleProvider.ModuleName;

                // initialize event types
                IDictionary<string, EventType> moduleTypes = new Dictionary<string, EventType>();
                var eventTypeResolver = new EventTypeResolverImpl(
                    moduleTypes, pathEventTypes, eventTypeRepositoryPreconfigured, beanEventTypeFactoryPrivate);
                var eventTypeCollector = new EventTypeCollectorImpl(
                    moduleTypes,
                    beanEventTypeFactoryPrivate, EventTypeFactoryImpl.GetInstance(container),
                    beanEventTypeStemService, eventTypeResolver, xmlFragmentEventTypeFactory,
                    eventTypeAvroHandler, EventBeanTypedEventFactoryCompileTime.INSTANCE);
                try {
                    provider.ModuleProvider.InitializeEventTypes(new EPModuleEventTypeInitServicesImpl(eventTypeCollector, eventTypeResolver));
                }
                catch (Exception e) {
                    throw new EPException(e);
                }

                // initialize named windows
                IDictionary<string, NamedWindowMetaData> moduleNamedWindows = new Dictionary<string, NamedWindowMetaData>();
                NamedWindowCollector namedWindowCollector = new NamedWindowCollectorImpl(moduleNamedWindows);
                try {
                    provider.ModuleProvider.InitializeNamedWindows(new EPModuleNamedWindowInitServicesImpl(namedWindowCollector, eventTypeResolver));
                }
                catch (Exception e) {
                    throw new EPException(e);
                }

                // initialize tables
                IDictionary<string, TableMetaData> moduleTables = new Dictionary<string, TableMetaData>();
                TableCollector tableCollector = new TableCollectorImpl(moduleTables);
                try {
                    provider.ModuleProvider.InitializeTables(new EPModuleTableInitServicesImpl(tableCollector, eventTypeResolver));
                }
                catch (Exception e) {
                    throw new EPException(e);
                }

                // initialize create-index indexes
                var indexCollector = new IndexCollectorCompileTime(moduleNamedWindows, moduleTables, pathNamedWindows, pathTables);
                try {
                    provider.ModuleProvider.InitializeIndexes(new EPModuleIndexInitServicesImpl(indexCollector));
                }
                catch (Exception e) {
                    throw new EPException(e);
                }

                // initialize create-contexts
                IDictionary<string, ContextMetaData> moduleContexts = new Dictionary<string, ContextMetaData>();
                var contextCollector = new ContextCollectorImpl(moduleContexts);
                try {
                    provider.ModuleProvider.InitializeContexts(new EPModuleContextInitServicesImpl(contextCollector, eventTypeResolver));
                }
                catch (Exception e) {
                    throw new EPException(e);
                }

                // initialize variables
                IDictionary<string, VariableMetaData> moduleVariables = new Dictionary<string, VariableMetaData>();
                var variableCollector = new VariableCollectorImpl(moduleVariables);
                try {
                    provider.ModuleProvider.InitializeVariables(new EPModuleVariableInitServicesImpl(variableCollector, eventTypeResolver));
                }
                catch (Exception e) {
                    throw new EPException(e);
                }

                // initialize module expressions
                IDictionary<string, ExpressionDeclItem> moduleExprDeclareds = new Dictionary<string, ExpressionDeclItem>();
                ExprDeclaredCollector exprDeclaredCollector = new ExprDeclaredCollectorCompileTime(moduleExprDeclareds);
                try {
                    provider.ModuleProvider.InitializeExprDeclareds(new EPModuleExprDeclaredInitServicesImpl(exprDeclaredCollector));
                }
                catch (Exception e) {
                    throw new EPException(e);
                }

                // initialize module scripts
                IDictionary<NameAndParamNum, ExpressionScriptProvided> moduleScripts = new Dictionary<NameAndParamNum, ExpressionScriptProvided>();
                var scriptCollector = new ScriptCollectorCompileTime(moduleScripts);
                try {
                    provider.ModuleProvider.InitializeScripts(new EPModuleScriptInitServicesImpl(scriptCollector));
                }
                catch (Exception e) {
                    throw new EPException(e);
                }

                // save path-visibility event types and named windows to the path
                var deploymentId = "D" + deploymentNumber;
                try {
                    foreach (var type in moduleTypes) {
                        if (type.Value.Metadata.AccessModifier.IsNonPrivateNonTransient) {
                            pathEventTypes.Add(type.Key, unitModuleName, type.Value, deploymentId);
                        }
                    }

                    foreach (var entry in moduleNamedWindows) {
                        if (entry.Value.EventType.Metadata.AccessModifier.IsNonPrivateNonTransient) {
                            pathNamedWindows.Add(entry.Key, unitModuleName, entry.Value, deploymentId);
                        }
                    }

                    foreach (var entry in moduleTables) {
                        if (entry.Value.TableVisibility.IsNonPrivateNonTransient) {
                            pathTables.Add(entry.Key, unitModuleName, entry.Value, deploymentId);
                        }
                    }

                    foreach (var entry in moduleContexts) {
                        if (entry.Value.ContextVisibility.IsNonPrivateNonTransient) {
                            pathContexts.Add(entry.Key, unitModuleName, entry.Value, deploymentId);
                        }
                    }

                    foreach (var entry in moduleVariables) {
                        if (entry.Value.VariableVisibility.IsNonPrivateNonTransient) {
                            pathVariables.Add(entry.Key, unitModuleName, entry.Value, deploymentId);
                        }
                    }

                    foreach (var entry in moduleExprDeclareds) {
                        if (entry.Value.Visibility.IsNonPrivateNonTransient) {
                            pathExprDeclared.Add(entry.Key, unitModuleName, entry.Value, deploymentId);
                        }
                    }

                    foreach (var entry in moduleScripts) {
                        if (entry.Value.Visibility.IsNonPrivateNonTransient) {
                            pathScript.Add(entry.Key, unitModuleName, entry.Value, deploymentId);
                        }
                    }
                }
                catch (PathException ex) {
                    throw new EPCompileException(
                        "Invalid path: " + ex.Message, ex,
                        new EmptyList<EPCompileExceptionItem>());
                }
            }

            // add runtime-path which is the information an existing runtime may have
            if (path.CompilerPathables != null) {
                foreach (var pathable in path.CompilerPathables) {
                    var impl = (EPCompilerPathableImpl) pathable;
                    pathVariables.MergeFrom(impl.VariablePathRegistry);
                    pathEventTypes.MergeFrom(impl.EventTypePathRegistry);
                    pathExprDeclared.MergeFrom(impl.ExprDeclaredPathRegistry);
                    pathNamedWindows.MergeFrom(impl.NamedWindowPathRegistry);
                    pathTables.MergeFrom(impl.TablePathRegistry);
                    pathContexts.MergeFrom(impl.ContextPathRegistry);
                    pathScript.MergeFrom(impl.ScriptPathRegistry);
                    eventTypeRepositoryPreconfigured.MergeFrom(impl.EventTypePreconfigured);
                    variableRepositoryPreconfigured.MergeFrom(impl.VariablePreconfigured);
                }
            }

            var moduleDependencies = new ModuleDependenciesCompileTime();

            // build bean space of public and protected
            var eventTypeCompileTimeResolver = new EventTypeCompileTimeResolver(
                moduleName, moduleUses, eventTypeCompileRegistry, eventTypeRepositoryPreconfigured, pathEventTypes, moduleDependencies);

            // build named window registry
            var namedWindowCompileTimeRegistry = new NamedWindowCompileTimeRegistry();
            NamedWindowCompileTimeResolver namedWindowCompileTimeResolver = new NamedWindowCompileTimeResolverImpl(
                moduleName, moduleUses, namedWindowCompileTimeRegistry, pathNamedWindows, moduleDependencies);

            // build context registry
            var contextCompileTimeRegistry = new ContextCompileTimeRegistry();
            ContextCompileTimeResolver contextCompileTimeResolver = new ContextCompileTimeResolverImpl(
                moduleName, moduleUses, contextCompileTimeRegistry, pathContexts, moduleDependencies);

            // build variable registry
            var variableCompileTimeRegistry = new VariableCompileTimeRegistry();
            VariableCompileTimeResolver variableCompileTimeResolver = new VariableCompileTimeResolverImpl(
                moduleName, moduleUses, variableRepositoryPreconfigured, variableCompileTimeRegistry, pathVariables, moduleDependencies);

            // build declared-expression registry
            var exprDeclaredCompileTimeRegistry = new ExprDeclaredCompileTimeRegistry();
            ExprDeclaredCompileTimeResolver exprDeclaredCompileTimeResolver = new ExprDeclaredCompileTimeResolverImpl(
                moduleName, moduleUses, exprDeclaredCompileTimeRegistry, pathExprDeclared, moduleDependencies);

            // build table-registry
            IDictionary<string, TableMetaData> localTables = new Dictionary<string, TableMetaData>();
            var tableCompileTimeRegistry = new TableCompileTimeRegistry(localTables);
            TableCompileTimeResolver tableCompileTimeResolver = new TableCompileTimeResolverImpl(
                moduleName, moduleUses, tableCompileTimeRegistry, pathTables, moduleDependencies);

            // build script registry
            var scriptCompileTimeRegistry = new ScriptCompileTimeRegistry();
            ScriptCompileTimeResolver scriptCompileTimeResolver = new ScriptCompileTimeResolverImpl(
                moduleName, moduleUses, scriptCompileTimeRegistry, pathScript, moduleDependencies);

            // view resolution
            var plugInViews = new PluggableObjectCollection();
            plugInViews.AddViews(configuration.Compiler.PlugInViews, configuration.Compiler.PlugInVirtualDataWindows, importServiceCompileTime);
            var viewRegistry = new PluggableObjectRegistryImpl(new[] {ViewEnumHelper.BuiltinViews, plugInViews});
            ViewResolutionService viewResolutionService = new ViewResolutionServiceImpl(viewRegistry);

            var plugInPatternObj = new PluggableObjectCollection();
            plugInPatternObj.AddPatternObjects(configuration.Compiler.PlugInPatternObjects, importServiceCompileTime);
            plugInPatternObj.AddObjects(PatternObjectHelper.BuiltinPatternObjects);
            PatternObjectResolutionService patternResolutionService = new PatternObjectResolutionServiceImpl(plugInPatternObj);

            var indexCompileTimeRegistry = new IndexCompileTimeRegistry(new Dictionary<IndexCompileTimeKey, IndexDetailForge>());

            ModuleAccessModifierService moduleVisibilityRules = new ModuleAccessModifierServiceImpl(options, configuration.Compiler.ByteCode);

            DatabaseConfigServiceCompileTime databaseConfigServiceCompileTime =
                new DatabaseConfigServiceImpl(configuration.Common.DatabaseReferences, importServiceCompileTime);

            CompilerServices compilerServices = new CompilerServicesImpl();

            return new ModuleCompileTimeServices(
                container,
                compilerServices,
                configuration,
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
                indexCompileTimeRegistry,
                moduleDependencies, 
                moduleVisibilityRules, 
                namedWindowCompileTimeResolver, 
                namedWindowCompileTimeRegistry,
                patternResolutionService, 
                scriptCompileTimeRegistry, 
                scriptCompileTimeResolver,
                tableCompileTimeRegistry, 
                tableCompileTimeResolver, 
                variableCompileTimeRegistry, 
                variableCompileTimeResolver, 
                viewResolutionService,
                xmlFragmentEventTypeFactory);
        }

        protected internal static ImportServiceCompileTime MakeImportService(Configuration configuration)
        {
            var timeAbacus = TimeAbacusFactory.Make(configuration.Common.TimeSource.TimeUnit);
            var expression = configuration.Compiler.Expression;
            var importService = new ImportServiceCompileTime(
                configuration.Container,
                configuration.Common.TransientConfiguration, timeAbacus,
                configuration.Common.EventTypeAutoNamePackages,
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

                foreach (var config in configuration.Compiler.PlugInAggregationFunctions) {
                    importService.AddAggregation(config.Name, config);
                }

                foreach (var config in configuration.Compiler.PlugInAggregationMultiFunctions) {
                    importService.AddAggregationMultiFunction(config);
                }

                foreach (var config in configuration.Compiler.PlugInSingleRowFunctions) {
                    importService.AddSingleRow(
                        config.Name, config.FunctionClassName, config.FunctionMethodName, config.ValueCache, config.FilterOptimizable,
                        config.RethrowExceptions, config.EventTypeName);
                }
            }
            catch (ImportException ex) {
                throw new ConfigurationException("Error configuring compiler: " + ex.Message, ex);
            }

            return importService;
        }
    }
} // end of namespace