///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.declared.runtime;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.pattern.pool;
using com.espertech.esper.common.@internal.epl.rowrecog.state;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtypefactory;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.statement.insertintolatch;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.option;
using com.espertech.esper.runtime.@internal.kernel.statement;
using com.espertech.esper.runtime.@internal.kernel.updatedispatch;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class Deployer
    {
        public static DeploymentInternal DeployFresh(
            string deploymentId,
            int statementIdFirstStatement,
            EPCompiled compiled,
            StatementNameRuntimeOption statementNameResolverRuntime,
            StatementUserObjectRuntimeOption userObjectResolverRuntime,
            StatementSubstitutionParameterOption substitutionParameterResolver,
            EPRuntimeSPI epRuntime)
        {
            return Deploy(
                false, deploymentId, statementIdFirstStatement, compiled, statementNameResolverRuntime, userObjectResolverRuntime,
                substitutionParameterResolver, epRuntime);
        }

        public static DeploymentInternal DeployRecover(
            string deploymentId,
            int statementIdFirstStatement,
            EPCompiled compiled,
            StatementNameRuntimeOption statementNameResolverRuntime,
            StatementUserObjectRuntimeOption userObjectResolverRuntime,
            StatementSubstitutionParameterOption substitutionParameterResolver,
            EPRuntimeSPI epRuntime)
        {
            return Deploy(
                true, deploymentId, statementIdFirstStatement, compiled, statementNameResolverRuntime, userObjectResolverRuntime,
                substitutionParameterResolver, epRuntime);
        }

        private static DeploymentInternal Deploy(
            bool recovery,
            string deploymentId,
            int statementIdFirstStatement,
            EPCompiled compiled,
            StatementNameRuntimeOption statementNameResolverRuntime,
            StatementUserObjectRuntimeOption userObjectResolverRuntime,
            StatementSubstitutionParameterOption substitutionParameterResolver,
            EPRuntimeSPI epRuntime)
        {
            // set variable local version
            epRuntime.ServicesContext.VariableManagementService.SetLocalVersion();

            return DeploySafe(
                epRuntime.Container,
                recovery, 
                deploymentId, 
                statementIdFirstStatement, 
                compiled, 
                statementNameResolverRuntime, 
                userObjectResolverRuntime,
                substitutionParameterResolver, epRuntime);
        }

        private static DeploymentInternal DeploySafe(
            IContainer container,
            bool recovery,
            string deploymentId,
            int statementIdFirstStatement,
            EPCompiled compiled,
            StatementNameRuntimeOption statementNameResolverRuntime,
            StatementUserObjectRuntimeOption userObjectResolverRuntime,
            StatementSubstitutionParameterOption substitutionParameterResolver,
            EPRuntimeSPI epRuntime)
        {
            ModuleProviderResult provider = ModuleProviderUtil.Analyze(compiled, epRuntime.ServicesContext.ImportServiceRuntime);
            string moduleName = provider.ModuleProvider.ModuleName;
            EPServicesContext services = epRuntime.ServicesContext;

            // resolve external dependencies
            ModuleDependenciesRuntime moduleDependencies = provider.ModuleProvider.ModuleDependencies;
            var deploymentIdDependencies = ResolveDependencies(moduleDependencies, services);

            // keep protected types
            var beanEventTypeFactory = new BeanEventTypeFactoryPrivate(
                new EventBeanTypedEventFactoryRuntime(services.EventTypeAvroHandler),
                EventTypeFactoryImpl.GetInstance(container),
                services.BeanEventTypeStemService);

            // initialize module event types
            IDictionary<string, EventType> moduleEventTypes = new Dictionary<string, EventType>();
            var eventTypeResolver = new EventTypeResolverImpl(
                moduleEventTypes,
                services.EventTypePathRegistry,
                services.EventTypeRepositoryBus,
                services.BeanEventTypeFactoryPrivate);
            var eventTypeCollector = new EventTypeCollectorImpl(
                moduleEventTypes, beanEventTypeFactory,
                services.EventTypeFactory,
                services.BeanEventTypeStemService, eventTypeResolver,
                services.XmlFragmentEventTypeFactory,
                services.EventTypeAvroHandler,
                services.EventBeanTypedEventFactory);
            try {
                provider.ModuleProvider.InitializeEventTypes(new EPModuleEventTypeInitServicesImpl(eventTypeCollector, eventTypeResolver));
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                throw new EPException(e);
            }

            // initialize module named windows
            IDictionary<string, NamedWindowMetaData> moduleNamedWindows = new Dictionary<string, NamedWindowMetaData>();
            NamedWindowCollector namedWindowCollector = new NamedWindowCollectorImpl(moduleNamedWindows);
            try {
                provider.ModuleProvider.InitializeNamedWindows(new EPModuleNamedWindowInitServicesImpl(namedWindowCollector, eventTypeResolver));
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                throw new EPException(e);
            }

            // initialize module tables
            IDictionary<string, TableMetaData> moduleTables = new Dictionary<string, TableMetaData>();
            var tableCollector = new TableCollectorImpl(moduleTables);
            try {
                provider.ModuleProvider.InitializeTables(new EPModuleTableInitServicesImpl(tableCollector, eventTypeResolver));
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                throw new EPException(e);
            }

            // initialize create-index indexes
            ISet<ModuleIndexMeta> moduleIndexes = new HashSet<ModuleIndexMeta>();
            var indexCollector = new IndexCollectorRuntime(moduleIndexes);
            try {
                provider.ModuleProvider.InitializeIndexes(new EPModuleIndexInitServicesImpl(indexCollector));
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                throw new EPException(e);
            }

            // initialize module contexts
            IDictionary<string, ContextMetaData> moduleContexts = new Dictionary<string, ContextMetaData>();
            ContextCollector contextCollector = new ContextCollectorImpl(moduleContexts);
            try {
                provider.ModuleProvider.InitializeContexts(new EPModuleContextInitServicesImpl(contextCollector, eventTypeResolver));
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                throw new EPException(e);
            }

            // initialize module variables
            IDictionary<string, VariableMetaData> moduleVariables = new Dictionary<string, VariableMetaData>();
            VariableCollector variableCollector = new VariableCollectorImpl(moduleVariables);
            try {
                provider.ModuleProvider.InitializeVariables(new EPModuleVariableInitServicesImpl(variableCollector, eventTypeResolver));
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                throw new EPException(e);
            }

            // initialize module expressions
            IDictionary<string, ExpressionDeclItem> moduleExpressions = new Dictionary<string, ExpressionDeclItem>();
            var exprDeclaredCollector = new ExprDeclaredCollectorRuntime(moduleExpressions);
            try {
                provider.ModuleProvider.InitializeExprDeclareds(new EPModuleExprDeclaredInitServicesImpl(exprDeclaredCollector));
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                throw new EPException(e);
            }

            // initialize module scripts
            IDictionary<NameAndParamNum, ExpressionScriptProvided> moduleScripts = new Dictionary<NameAndParamNum, ExpressionScriptProvided>();
            var scriptCollectorRuntime = new ScriptCollectorRuntime(moduleScripts);
            try {
                provider.ModuleProvider.InitializeScripts(new EPModuleScriptInitServicesImpl(scriptCollectorRuntime));
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                throw new EPException(ex);
            }

            // save path-visibility event types and named windows to the path
            var deploymentIdCrc32 = CRC32Util.ComputeCRC32(deploymentId);
            IDictionary<long, EventType> deploymentTypes = new EmptyDictionary<long, EventType>();
            IList<string> pathEventTypes = new List<string>(2);
            IList<string> pathNamedWindows = new List<string>(2);
            IList<string> pathTables = new List<string>(2);
            IList<string> pathContexts = new List<string>(2);
            IList<string> pathVariables = new List<string>(2);
            IList<string> pathExprDecl = new List<string>(2);
            IList<NameAndParamNum> pathScripts = new List<NameAndParamNum>();
            foreach (var entry in moduleNamedWindows) {
                if (entry.Value.EventType.Metadata.AccessModifier.IsNonPrivateNonTransient()) {
                    try {
                        services.NamedWindowPathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
                    }
                    catch (PathExceptionAlreadyRegistered ex) {
                        throw new EPDeployPreconditionException(ex.Message, ex);
                    }

                    pathNamedWindows.Add(entry.Key);
                }
            }

            foreach (var entry in moduleTables) {
                if (entry.Value.TableVisibility.IsNonPrivateNonTransient()) {
                    try {
                        services.TablePathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
                    }
                    catch (PathExceptionAlreadyRegistered ex) {
                        throw new EPDeployPreconditionException(ex.Message, ex);
                    }

                    pathTables.Add(entry.Key);
                }
            }

            foreach (var entry in moduleEventTypes) {
                var eventTypeSPI = (EventTypeSPI) entry.Value;
                var nameTypeId = CRC32Util.ComputeCRC32(eventTypeSPI.Name);
                var eventTypeMetadata = entry.Value.Metadata;
                if (eventTypeMetadata.AccessModifier == NameAccessModifier.PRECONFIGURED) {
                    // For XML all fragment event types are public
                    if (eventTypeMetadata.ApplicationType != EventTypeApplicationType.XML) {
                        throw new IllegalStateException("Unrecognized public visibility type in deployment");
                    }
                }
                else if (eventTypeMetadata.AccessModifier.IsNonPrivateNonTransient()) {
                    if (eventTypeMetadata.BusModifier == EventTypeBusModifier.BUS) {
                        eventTypeSPI.SetMetadataId(nameTypeId, -1);
                        services.EventTypeRepositoryBus.AddType(eventTypeSPI);
                    }
                    else {
                        eventTypeSPI.SetMetadataId(deploymentIdCrc32, nameTypeId);
                    }

                    try {
                        services.EventTypePathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
                    }
                    catch (PathExceptionAlreadyRegistered ex) {
                        throw new EPDeployPreconditionException(ex.Message, ex);
                    }
                }
                else {
                    eventTypeSPI.SetMetadataId(deploymentIdCrc32, nameTypeId);
                }

                if (eventTypeMetadata.AccessModifier.IsNonPrivateNonTransient()) {
                    pathEventTypes.Add(entry.Key);
                }

                // we retain all types to enable variant-streams
                if (deploymentTypes.IsEmpty()) {
                    deploymentTypes = new Dictionary<long, EventType>(4);
                }

                deploymentTypes.Put(nameTypeId, eventTypeSPI);
            }

            foreach (var entry in moduleContexts) {
                if (entry.Value.ContextVisibility.IsNonPrivateNonTransient()) {
                    try {
                        services.ContextPathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
                    }
                    catch (PathExceptionAlreadyRegistered ex) {
                        throw new EPDeployPreconditionException(ex.Message, ex);
                    }

                    pathContexts.Add(entry.Key);
                }
            }

            foreach (var entry in moduleVariables) {
                if (entry.Value.VariableVisibility.IsNonPrivateNonTransient()) {
                    try {
                        services.VariablePathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
                    }
                    catch (PathExceptionAlreadyRegistered ex) {
                        throw new EPDeployPreconditionException(ex.Message, ex);
                    }

                    pathVariables.Add(entry.Key);
                }
            }

            foreach (var entry in moduleExpressions) {
                if (entry.Value.Visibility.IsNonPrivateNonTransient()) {
                    try {
                        services.ExprDeclaredPathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
                    }
                    catch (PathExceptionAlreadyRegistered ex) {
                        throw new EPDeployPreconditionException(ex.Message, ex);
                    }

                    pathExprDecl.Add(entry.Key);
                }
            }

            foreach (var entry in moduleScripts) {
                if (entry.Value.Visibility.IsNonPrivateNonTransient()) {
                    try {
                        services.ScriptPathRegistry.Add(entry.Key, moduleName, entry.Value, deploymentId);
                    }
                    catch (PathExceptionAlreadyRegistered ex) {
                        throw new EPDeployPreconditionException(ex.Message, ex);
                    }

                    pathScripts.Add(entry.Key);
                }
            }

            foreach (var index in moduleIndexes) {
                if (index.IsNamedWindow) {
                    var namedWindow = services.NamedWindowPathRegistry.GetWithModule(index.InfraName, index.InfraModuleName);
                    if (namedWindow == null) {
                        throw new IllegalStateException("Failed to find named window '" + index.InfraName + "'");
                    }

                    ValidateIndexPrecondition(namedWindow.IndexMetadata, index);
                }
                else {
                    var table = services.TablePathRegistry.GetWithModule(index.InfraName, index.InfraModuleName);
                    if (table == null) {
                        throw new IllegalStateException("Failed to find table '" + index.InfraName + "'");
                    }

                    ValidateIndexPrecondition(table.IndexMetadata, index);
                }
            }

            var moduleIncidentals = new ModuleIncidentals(moduleNamedWindows, moduleContexts, moduleVariables, moduleExpressions, moduleTables);

            // get module statements
            IList<StatementProvider> statementResources;
            try {
                statementResources = provider.ModuleProvider.Statements;
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                AppDomain appDomain = AppDomain.CurrentDomain;
                Assembly[] assemblies = appDomain.GetAssemblies();
                throw new EPException(e);
            }

            // initialize all statements
            IList<StatementLightweight> lightweights = new List<StatementLightweight>();
            IDictionary<int, IDictionary<int, object>> substitutionParameters;
            try {
                var statementId = statementIdFirstStatement;
                foreach (var statement in statementResources) {
                    var lightweight = InitStatement(
                        recovery, moduleName, statement, deploymentId, statementId, eventTypeResolver, moduleIncidentals,
                        statementNameResolverRuntime, userObjectResolverRuntime, services);
                    lightweights.Add(lightweight);
                    statementId++;
                }

                // set parameters
                substitutionParameters = SetSubstitutionParameterValues(deploymentId, lightweights, substitutionParameterResolver);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception) {
                ReverseDeployment(deploymentId, deploymentTypes, lightweights, new EPStatement[0], provider, services);
                throw;
            }

            // start statements depending on context association
            var statements = new EPStatement[lightweights.Count];
            var count = 0;
            foreach (var lightweight in lightweights) {
                EPStatementSPI stmt;
                try {
                    stmt = DeployerStatement.DeployStatement(recovery, lightweight, services, epRuntime);
                }
                catch (Exception ex) {
                    ReverseDeployment(deploymentId, deploymentTypes, lightweights, statements, provider, services);
                    throw new EPDeployException("Failed to deploy: " + ex.Message, ex);
                }

                statements[count++] = stmt;

                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().QaEngineManagementStmtStarted(
                        epRuntime.URI, deploymentId, lightweight.StatementContext.StatementId, stmt.Name,
                        (string) stmt.GetProperty(StatementProperty.EPL), epRuntime.EventService.CurrentTime);
                }
            }

            // add dependencies
            AddDependencies(deploymentId, moduleDependencies, services);

            // keep statement and deployment
            var deploymentIdDependenciesArray = deploymentIdDependencies.ToArray();
            var deployed = new DeploymentInternal(
                deploymentId, statements, deploymentIdDependenciesArray,
                CollectionUtil.ToArray(pathNamedWindows), 
                CollectionUtil.ToArray(pathTables),
                CollectionUtil.ToArray(pathVariables),
                CollectionUtil.ToArray(pathContexts),
                CollectionUtil.ToArray(pathEventTypes),
                CollectionUtil.ToArray(pathExprDecl),
                NameAndParamNum.ToArray(pathScripts),
                ModuleIndexMeta.ToArray(moduleIndexes), 
                provider.ModuleProvider,
                provider.ModuleProvider.ModuleProperties, 
                deploymentTypes, 
                DateTimeHelper.CurrentTimeMillis);
            services.DeploymentLifecycleService.AddDeployment(deploymentId, deployed);

            // register for recovery
            if (!recovery) {
                var recoveryInformation = GetRecoveryInformation(deployed);
                services.DeploymentRecoveryService.Add(
                    deploymentId, statementIdFirstStatement, compiled,
                    recoveryInformation.StatementUserObjectsRuntime,
                    recoveryInformation.StatementNamesWhenProvidedByAPI,
                    substitutionParameters);
            }

            return deployed;
        }

        private static void ReverseDeployment(
            string deploymentId,
            IDictionary<long, EventType> deploymentTypes,
            IList<StatementLightweight> lightweights,
            EPStatement[] statements,
            ModuleProviderResult provider,
            EPServicesContext services)
        {
            List<StatementContext> revert = new List<StatementContext>();
            foreach (var stmtToRemove in lightweights) {
                revert.Add(stmtToRemove.StatementContext);
            }

            revert.Reverse();
            var reverted = revert.ToArray();
            Undeployer.Disassociate(statements);
            Undeployer.Undeploy(deploymentId, deploymentTypes, reverted, provider.ModuleProvider, services);
        }

        private static IDictionary<int, IDictionary<int, object>> SetSubstitutionParameterValues(
            string deploymentId,
            IList<StatementLightweight> lightweights,
            StatementSubstitutionParameterOption substitutionParameterResolver)
        {
            if (substitutionParameterResolver == null) {
                foreach (var lightweight in lightweights) {
                    var required = lightweight.StatementInformationals.SubstitutionParamTypes;
                    if (required != null && required.Length > 0) {
                        throw new EPDeploySubstitutionParameterException(
                            "Statement '" + lightweight.StatementContext.StatementName + "' has " + required.Length + " substitution parameters");
                    }
                }

                return new EmptyDictionary<int, IDictionary<int, object>>();
            }

            var providedAllStmt = new Dictionary<int, IDictionary<int, object>>();
            foreach (var lightweight in lightweights) {
                var substitutionTypes = lightweight.StatementInformationals.SubstitutionParamTypes;
                var paramNames = lightweight.StatementInformationals.SubstitutionParamNames;
                var handler = new DeployerSubstitutionParameterHandler(deploymentId, lightweight, providedAllStmt, substitutionTypes, paramNames);

                try {
                    substitutionParameterResolver.Invoke(handler);
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception ex) {
                    throw new EPDeploySubstitutionParameterException(
                        "Failed to set substitution parameter value for statement '" + lightweight.StatementContext.StatementName + "': " +
                        ex.Message,
                        ex);
                }

                if (substitutionTypes == null || substitutionTypes.Length == 0) {
                    continue;
                }

                // check that all values are provided
                var provided = providedAllStmt.Get(lightweight.StatementContext.StatementId);
                var providedSize = provided?.Count ?? 0;
                if (providedSize != substitutionTypes.Length) {
                    for (var i = 0; i < substitutionTypes.Length; i++) {
                        if (provided == null || !provided.ContainsKey(i + 1)) {
                            var name = Convert.ToString(i + 1);
                            if (paramNames != null && !paramNames.IsEmpty()) {
                                foreach (var entry in paramNames) {
                                    if (entry.Value == i + 1) {
                                        name = "'" + entry.Key + "'";
                                    }
                                }
                            }

                            throw new EPDeploySubstitutionParameterException(
                                "Missing value for substitution parameter " + name + " for statement '" + lightweight.StatementContext.StatementName +
                                "'");
                        }
                    }
                }
            }

            return providedAllStmt;
        }

        private static RecoveryInformation GetRecoveryInformation(DeploymentInternal deployerResult)
        {
            IDictionary<int, object> userObjects = new EmptyDictionary<int, object>();
            IDictionary<int, string> statementNamesWhenOverridden = new EmptyDictionary<int, string>();
            foreach (EPStatement stmt in deployerResult.Statements) {
                var spi = (EPStatementSPI) stmt;
                if (stmt.UserObjectRuntime != null) {
                    if (userObjects.IsEmpty()) {
                        userObjects = new Dictionary<int, object>();
                    }

                    userObjects.Put(spi.StatementId, spi.StatementContext.UserObjectRuntime);
                }

                if (!spi.StatementContext.StatementInformationals.StatementNameCompileTime.Equals(spi.Name)) {
                    if (statementNamesWhenOverridden.IsEmpty()) {
                        statementNamesWhenOverridden = new Dictionary<int, string>();
                    }

                    statementNamesWhenOverridden.Put(spi.StatementId, spi.Name);
                }
            }

            return new RecoveryInformation(userObjects, statementNamesWhenOverridden);
        }

        private static void AddDependencies(
            string deploymentId,
            ModuleDependenciesRuntime moduleDependencies,
            EPServicesContext services)
        {
            foreach (var eventType in moduleDependencies.PathEventTypes) {
                services.EventTypePathRegistry.AddDependency(eventType.Name, eventType.ModuleName, deploymentId);
            }

            foreach (var namedWindow in moduleDependencies.PathNamedWindows) {
                services.NamedWindowPathRegistry.AddDependency(namedWindow.Name, namedWindow.ModuleName, deploymentId);
            }

            foreach (var table in moduleDependencies.PathTables) {
                services.TablePathRegistry.AddDependency(table.Name, table.ModuleName, deploymentId);
            }

            foreach (var variable in moduleDependencies.PathVariables) {
                services.VariablePathRegistry.AddDependency(variable.Name, variable.ModuleName, deploymentId);
            }

            foreach (var context in moduleDependencies.PathContexts) {
                services.ContextPathRegistry.AddDependency(context.Name, context.ModuleName, deploymentId);
            }

            foreach (var exprDecl in moduleDependencies.PathExpressions) {
                services.ExprDeclaredPathRegistry.AddDependency(exprDecl.Name, exprDecl.ModuleName, deploymentId);
            }

            foreach (var script in moduleDependencies.PathScripts) {
                services.ScriptPathRegistry.AddDependency(new NameAndParamNum(script.Name, script.ParamNum), script.ModuleName, deploymentId);
            }

            foreach (var index in moduleDependencies.PathIndexes) {
                EventTableIndexMetadata indexMetadata;
                if (index.IsNamedWindow) {
                    var namedWindowName = NameAndModule.FindName(index.InfraName, moduleDependencies.PathNamedWindows);
                    var namedWindow = services.NamedWindowPathRegistry.GetWithModule(
                        namedWindowName.Name, namedWindowName.ModuleName);
                    indexMetadata = namedWindow.IndexMetadata;
                }
                else {
                    var tableName = NameAndModule.FindName(index.InfraName, moduleDependencies.PathTables);
                    var table = services.TablePathRegistry.GetWithModule(tableName.Name, tableName.ModuleName);
                    indexMetadata = table.IndexMetadata;
                }

                indexMetadata.AddIndexReference(index.IndexName, deploymentId);
            }
        }

        private static ISet<string> ResolveDependencies(
            ModuleDependenciesRuntime moduleDependencies,
            EPServicesContext services)
        {
            ISet<string> dependencies = new HashSet<string>();

            foreach (var publicEventType in moduleDependencies.PublicEventTypes) {
                if (services.EventTypeRepositoryBus.GetTypeByName(publicEventType) == null) {
                    throw MakePreconditionExceptionPreconfigured(PathRegistryObjectType.EVENTTYPE, publicEventType);
                }
            }

            foreach (var publicVariable in moduleDependencies.PublicVariables) {
                if (services.ConfigSnapshot.Common.Variables.Get(publicVariable) == null) {
                    throw MakePreconditionExceptionPreconfigured(PathRegistryObjectType.VARIABLE, publicVariable);
                }
            }

            foreach (var pathNamedWindow in moduleDependencies.PathNamedWindows) {
                var depIdNamedWindow = services.NamedWindowPathRegistry.GetDeploymentId(pathNamedWindow.Name, pathNamedWindow.ModuleName);
                if (depIdNamedWindow == null) {
                    throw MakePreconditionExceptionPath(PathRegistryObjectType.NAMEDWINDOW, pathNamedWindow);
                }

                dependencies.Add(depIdNamedWindow);
            }

            foreach (var pathTable in moduleDependencies.PathTables) {
                var depIdTable = services.TablePathRegistry.GetDeploymentId(pathTable.Name, pathTable.ModuleName);
                if (depIdTable == null) {
                    throw MakePreconditionExceptionPath(PathRegistryObjectType.TABLE, pathTable);
                }

                dependencies.Add(depIdTable);
            }

            foreach (var pathEventType in moduleDependencies.PathEventTypes) {
                var depIdEventType = services.EventTypePathRegistry.GetDeploymentId(pathEventType.Name, pathEventType.ModuleName);
                if (depIdEventType == null) {
                    throw MakePreconditionExceptionPath(PathRegistryObjectType.EVENTTYPE, pathEventType);
                }

                dependencies.Add(depIdEventType);
            }

            foreach (var pathVariable in moduleDependencies.PathVariables) {
                var depIdVariable = services.VariablePathRegistry.GetDeploymentId(pathVariable.Name, pathVariable.ModuleName);
                if (depIdVariable == null) {
                    throw MakePreconditionExceptionPath(PathRegistryObjectType.VARIABLE, pathVariable);
                }

                dependencies.Add(depIdVariable);
            }

            foreach (var pathContext in moduleDependencies.PathContexts) {
                var depIdContext = services.ContextPathRegistry.GetDeploymentId(pathContext.Name, pathContext.ModuleName);
                if (depIdContext == null) {
                    throw MakePreconditionExceptionPath(PathRegistryObjectType.CONTEXT, pathContext);
                }

                dependencies.Add(depIdContext);
            }

            foreach (var pathExpression in moduleDependencies.PathExpressions) {
                var depIdExpression = services.ExprDeclaredPathRegistry.GetDeploymentId(pathExpression.Name, pathExpression.ModuleName);
                if (depIdExpression == null) {
                    throw MakePreconditionExceptionPath(PathRegistryObjectType.EXPRDECL, pathExpression);
                }

                dependencies.Add(depIdExpression);
            }

            foreach (var pathScript in moduleDependencies.PathScripts) {
                var depIdExpression = services.ScriptPathRegistry.GetDeploymentId(
                    new NameAndParamNum(pathScript.Name, pathScript.ParamNum), pathScript.ModuleName);
                if (depIdExpression == null) {
                    throw MakePreconditionExceptionPath(PathRegistryObjectType.SCRIPT, new NameAndModule(pathScript.Name, pathScript.ModuleName));
                }

                dependencies.Add(depIdExpression);
            }

            foreach (var index in moduleDependencies.PathIndexes) {
                string depIdIndex;
                if (index.IsNamedWindow) {
                    var namedWindowName = NameAndModule.FindName(index.InfraName, moduleDependencies.PathNamedWindows);
                    var namedWindow = services.NamedWindowPathRegistry.GetWithModule(
                        namedWindowName.Name, namedWindowName.ModuleName);
                    depIdIndex = namedWindow.IndexMetadata.GetIndexDeploymentId(index.IndexName);
                }
                else {
                    var tableName = NameAndModule.FindName(index.InfraName, moduleDependencies.PathTables);
                    var table = services.TablePathRegistry.GetWithModule(tableName.Name, tableName.ModuleName);
                    depIdIndex = table.IndexMetadata.GetIndexDeploymentId(index.IndexName);
                }

                if (depIdIndex == null) {
                    throw MakePreconditionExceptionPath(PathRegistryObjectType.INDEX, new NameAndModule(index.IndexName, index.IndexModuleName));
                }

                dependencies.Add(depIdIndex);
            }

            return dependencies;
        }

        private static StatementLightweight InitStatement(
            bool recovery,
            string moduleName,
            StatementProvider statementProvider,
            string deploymentId,
            int statementId,
            EventTypeResolver eventTypeResolver,
            ModuleIncidentals moduleIncidentals,
            StatementNameRuntimeOption statementNameResolverRuntime,
            StatementUserObjectRuntimeOption userObjectResolverRuntime,
            EPServicesContext services)
        {
            var container = services.Container;
            var informationals = statementProvider.Informationals;

            // set instrumentation unless already provided
            if (informationals.InstrumentationProvider == null) {
                informationals.InstrumentationProvider = InstrumentationDefault.INSTANCE;
            }

            var statementResultService = new StatementResultServiceImpl(informationals, services);
            FilterSharedLookupableRegistery filterSharedLookupableRegistery = new ProxyFilterSharedLookupableRegistery {
                ProcRegisterLookupable = (eventTypeX, lookupable) => 
                    services.FilterSharedLookupableRepository.RegisterLookupable(statementId, eventTypeX, lookupable)
            };

            FilterSharedBoolExprRegistery filterSharedBoolExprRegistery = new ProxyFilterSharedBoolExprRegistery {
                ProcRegisterBoolExpr = node => services.FilterSharedBoolExprRepository.RegisterBoolExpr(statementId, node)
            };

            IDictionary<int, FilterSpecActivatable> filterSpecActivatables = new Dictionary<int, FilterSpecActivatable>();
            FilterSpecActivatableRegistry filterSpecActivatableRegistry = new ProxyFilterSpecActivatableRegistry {
                ProcRegister = filterSpecActivatable => filterSpecActivatables.Put(filterSpecActivatable.FilterCallbackId, filterSpecActivatable)
            };

            var contextPartitioned = informationals.OptionalContextName != null;
            var statementResourceService = new StatementResourceService(contextPartitioned);

            var epInitServices = new EPStatementInitServicesImpl(
                informationals.Annotations, deploymentId,
                eventTypeResolver, filterSpecActivatableRegistry, filterSharedBoolExprRegistery, filterSharedLookupableRegistery, moduleIncidentals,
                recovery, statementResourceService, statementResultService, services);

            statementProvider.Initialize(epInitServices);

            var statementName = informationals.StatementNameCompileTime;
            if (statementNameResolverRuntime != null) {
                var statementNameAssigned = statementNameResolverRuntime.Invoke(
                    new StatementNameRuntimeContext(
                        deploymentId, statementName, statementId, (string) informationals.Properties.Get(StatementProperty.EPL),
                        informationals.Annotations));
                if (statementNameAssigned != null) {
                    statementName = statementNameAssigned;
                }
            }

            statementName = statementName.Trim();

            var multiMatchHandler = services.MultiMatchHandlerFactory.Make(informationals.HasSubquery, informationals.IsNeedDedup);

            var stmtMetric = services.MetricReportingService.GetStatementHandle(statementId, deploymentId, statementName);

            var optionalEPL = (string) informationals.Properties.Get(StatementProperty.EPL);
            InsertIntoLatchFactory insertIntoFrontLatchFactory = null;
            InsertIntoLatchFactory insertIntoBackLatchFactory = null;
            if (informationals.InsertIntoLatchName != null) {
                var latchFactoryNameBack = "insert_stream_B_" + informationals.InsertIntoLatchName + "_" + statementName;
                var latchFactoryNameFront = "insert_stream_F_" + informationals.InsertIntoLatchName + "_" + statementName;
                var msecTimeout = services.RuntimeSettingsService.ConfigurationRuntime.Threading.InsertIntoDispatchTimeout;
                var locking = services.RuntimeSettingsService.ConfigurationRuntime.Threading.InsertIntoDispatchLocking;
                var latchFactoryFront = new InsertIntoLatchFactory(
                    latchFactoryNameFront, informationals.IsStateless, msecTimeout, locking, services.TimeSourceService);
                var latchFactoryBack = new InsertIntoLatchFactory(
                    latchFactoryNameBack, informationals.IsStateless, msecTimeout, locking, services.TimeSourceService);
                insertIntoFrontLatchFactory = latchFactoryFront;
                insertIntoBackLatchFactory = latchFactoryBack;
            }

            var statementHandle = new EPStatementHandle(
                statementName,
                deploymentId,
                statementId,
                optionalEPL,
                informationals.Priority,
                informationals.IsPreemptive,
                informationals.IsCanSelfJoin,
                multiMatchHandler,
                informationals.HasVariables,
                informationals.HasTableAccess,
                stmtMetric,
                insertIntoFrontLatchFactory,
                insertIntoBackLatchFactory);

            // determine context
            StatementAIResourceRegistry statementAgentInstanceRegistry = null;
            ContextRuntimeDescriptor contextRuntimeDescriptor = null;
            var optionalContextName = informationals.OptionalContextName;
            if (optionalContextName != null) {
                var contextDeploymentId = ContextDeployTimeResolver.ResolveContextDeploymentId(
                    informationals.OptionalContextModuleName, informationals.OptionalContextVisibility, optionalContextName, deploymentId,
                    services.ContextPathRegistry);
                var contextManager = services.ContextManagementService.GetContextManager(contextDeploymentId, optionalContextName);
                contextRuntimeDescriptor = contextManager.ContextRuntimeDescriptor;
                var registryRequirements = statementProvider.StatementAIFactoryProvider.Factory.RegistryRequirements;
                statementAgentInstanceRegistry = contextManager.AllocateAgentInstanceResourceRegistry(registryRequirements);
            }

            var statementCPCacheService = new StatementCPCacheService(contextPartitioned, statementResourceService, statementAgentInstanceRegistry);

            var eventType = statementProvider.StatementAIFactoryProvider.Factory.StatementEventType;

            var configurationThreading = services.RuntimeSettingsService.ConfigurationRuntime.Threading;
            var preserveDispatchOrder = configurationThreading.IsListenerDispatchPreserveOrder && !informationals.IsStateless;
            var isSpinLocks = configurationThreading.ListenerDispatchLocking == Locking.SPIN;
            var msecBlockingTimeout = configurationThreading.ListenerDispatchTimeout;
            UpdateDispatchViewBase dispatchChildView;
            if (preserveDispatchOrder) {
                if (isSpinLocks) {
                    dispatchChildView = new UpdateDispatchViewBlockingSpin(
                        eventType, statementResultService, services.DispatchService, msecBlockingTimeout, services.TimeSourceService);
                }
                else {
                    dispatchChildView = new UpdateDispatchViewBlockingWait(
                        eventType, statementResultService, services.DispatchService, msecBlockingTimeout);
                }
            }
            else {
                dispatchChildView = new UpdateDispatchViewNonBlocking(eventType, statementResultService, services.DispatchService);
            }

            var countSubexpressions = services.ConfigSnapshot.Runtime.Patterns.MaxSubexpressions != null;
            PatternSubexpressionPoolStmtSvc patternSubexpressionPoolStmtSvc = null;
            if (countSubexpressions) {
                var stmtCounter = new PatternSubexpressionPoolStmtHandler();
                patternSubexpressionPoolStmtSvc = new PatternSubexpressionPoolStmtSvc(services.PatternSubexpressionPoolRuntimeSvc, stmtCounter);
                services.PatternSubexpressionPoolRuntimeSvc.AddPatternContext(statementId, statementName, stmtCounter);
            }

            var countMatchRecogStates = services.ConfigSnapshot.Runtime.MatchRecognize.MaxStates != null;
            RowRecogStatePoolStmtSvc rowRecogStatePoolStmtSvc = null;
            if (countMatchRecogStates && informationals.HasMatchRecognize) {
                var stmtCounter = new RowRecogStatePoolStmtHandler();
                rowRecogStatePoolStmtSvc = new RowRecogStatePoolStmtSvc(services.RowRecogStatePoolEngineSvc, stmtCounter);
                services.RowRecogStatePoolEngineSvc.AddPatternContext(new DeploymentIdNamePair(deploymentId, statementName), stmtCounter);
            }

            // get user object for runtime
            object userObjectRuntime = null;
            if (userObjectResolverRuntime != null) {
                userObjectRuntime = userObjectResolverRuntime.GetUserObject(
                    new StatementUserObjectRuntimeContext(
                        deploymentId, statementName, statementId, (string) informationals.Properties.Get(StatementProperty.EPL),
                        informationals.Annotations));
            }

            var statementContext = new StatementContext(
                container,
                contextRuntimeDescriptor,
                deploymentId,
                statementId,
                statementName,
                moduleName,
                informationals,
                userObjectRuntime,
                services.StatementContextRuntimeServices,
                statementHandle,
                filterSpecActivatables,
                patternSubexpressionPoolStmtSvc,
                rowRecogStatePoolStmtSvc,
                new ScheduleBucket(statementId),
                statementAgentInstanceRegistry,
                statementCPCacheService,
                statementProvider.StatementAIFactoryProvider,
                statementResultService,
                dispatchChildView
            );

            foreach (var readyCallback in epInitServices.ReadyCallbacks) {
                readyCallback.Ready(statementContext, moduleIncidentals, recovery);
            }

            return new StatementLightweight(statementProvider, informationals, statementResultService, statementContext);
        }

        private static EPDeployPreconditionException MakePreconditionExceptionPath(
            PathRegistryObjectType objectType,
            NameAndModule nameAndModule)
        {
            var message = "Required dependency ";
            message += objectType.Name + " '" + nameAndModule.Name + "'";
            if (!string.IsNullOrEmpty(nameAndModule.ModuleName)) {
                message += " module '" + nameAndModule.ModuleName + "'";
            }

            message += " cannot be found";
            return new EPDeployPreconditionException(message);
        }

        private static EPDeployPreconditionException MakePreconditionExceptionPreconfigured(
            PathRegistryObjectType objectType,
            string name)
        {
            var message = "Required pre-configured ";
            message += objectType.Name + " '" + name + "'";
            message += " cannot be found";
            return new EPDeployPreconditionException(message);
        }

        private static void ValidateIndexPrecondition(
            EventTableIndexMetadata indexMetadata,
            ModuleIndexMeta index)
        {
            if (indexMetadata.GetIndexByName(index.IndexName) != null) {
                var ex = new PathExceptionAlreadyRegistered(index.IndexName, PathRegistryObjectType.INDEX, index.IndexModuleName);
                throw new EPDeployPreconditionException(ex.Message, ex);
            }
        }

        private class RecoveryInformation
        {
            public RecoveryInformation(
                IDictionary<int, object> statementUserObjectsRuntime,
                IDictionary<int, string> statementNamesWhenProvidedByAPI)
            {
                StatementUserObjectsRuntime = statementUserObjectsRuntime;
                StatementNamesWhenProvidedByAPI = statementNamesWhenProvidedByAPI;
            }

            public IDictionary<int, object> StatementUserObjectsRuntime { get; }

            public IDictionary<int, string> StatementNamesWhenProvidedByAPI { get; }
        }
    }
} // end of namespace