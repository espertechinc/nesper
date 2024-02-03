///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.expression.declared.runtime;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.util;
using com.espertech.esper.runtime.@internal.kernel.stage;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class DeployerHelperInitializeEPLObjects
	{
		public static DeployerModuleEPLObjects InitializeEPLObjects(
			ModuleProviderCLPair provider,
			string deploymentId,
			EPServicesContext services)
		{
			// keep protected types
			var beanEventTypeFactory = new BeanEventTypeFactoryPrivate(
				new EventBeanTypedEventFactoryRuntime(services.EventTypeAvroHandler),
				services.EventTypeFactory,
				services.BeanEventTypeStemService);

			// initialize module event types
			IDictionary<string, EventType> moduleEventTypes = new LinkedHashMap<string, EventType>();
			var eventTypeResolver = new EventTypeResolverImpl(
				moduleEventTypes,
				services.EventTypePathRegistry,
				services.EventTypeRepositoryBus,
				services.BeanEventTypeFactoryPrivate,
				services.EventSerdeFactory);
			var eventTypeCollector = new EventTypeCollectorImpl(
				services.Container,
				moduleEventTypes,
				beanEventTypeFactory,
				provider.TypeResolver,
				services.EventTypeFactory,
				services.BeanEventTypeStemService,
				eventTypeResolver,
				services.XmlFragmentEventTypeFactory,
				services.EventTypeAvroHandler,
				services.EventBeanTypedEventFactory,
				services.ImportServiceRuntime,
				services.EventTypeXMLXSDHandler);
			
			try {
				provider.ModuleProvider.InitializeEventTypes(new EPModuleEventTypeInitServicesImpl(eventTypeCollector, eventTypeResolver));
			}
			catch (Exception e) {
				throw new EPException(e);
			}

			JsonEventTypeUtility.AddJsonUnderlyingClass(moduleEventTypes, services.TypeResolverParent, deploymentId);

			// initialize module named windows
			IDictionary<string, NamedWindowMetaData> moduleNamedWindows = new Dictionary<string, NamedWindowMetaData>();
			NamedWindowCollector namedWindowCollector = new NamedWindowCollectorImpl(moduleNamedWindows);
			try {
				provider.ModuleProvider.InitializeNamedWindows(new EPModuleNamedWindowInitServicesImpl(namedWindowCollector, eventTypeResolver));
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
			catch (Exception e) {
				throw new EPException(e);
			}

			// initialize create-index indexes
			ISet<ModuleIndexMeta> moduleIndexes = new HashSet<ModuleIndexMeta>();
			var indexCollector = new IndexCollectorRuntime(moduleIndexes);
			try {
				provider.ModuleProvider.InitializeIndexes(new EPModuleIndexInitServicesImpl(indexCollector));
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
			catch (Exception e) {
				throw new EPException(e);
			}

			// initialize module variables
			IDictionary<string, VariableMetaData> moduleVariables = new Dictionary<string, VariableMetaData>();
			VariableCollector variableCollector = new VariableCollectorImpl(moduleVariables);
			try {
				provider.ModuleProvider.InitializeVariables(new EPModuleVariableInitServicesImpl(variableCollector, eventTypeResolver));
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
			catch (Exception e) {
				throw new EPException(e);
			}

			// initialize module scripts
			IDictionary<NameAndParamNum, ExpressionScriptProvided> moduleScripts = new Dictionary<NameAndParamNum, ExpressionScriptProvided>();
			var scriptCollectorRuntime = new ScriptCollectorRuntime(moduleScripts);
			try {
				provider.ModuleProvider.InitializeScripts(new EPModuleScriptInitServicesImpl(scriptCollectorRuntime));
			}
			catch (Exception e) {
				throw new EPException(e);
			}

			// initialize module class-provided create-class
			IDictionary<string, ClassProvided> moduleClasses = new Dictionary<string, ClassProvided>();
			var classProvidedCollectorRuntime = new ClassProvidedCollectorRuntime(moduleClasses);
			var artifactRepositoryManager = services.Container.ArtifactRepositoryManager();
			var artifactRepository = artifactRepositoryManager.DefaultRepository;
			try {
				provider.ModuleProvider.InitializeClassProvided(new EPModuleClassProvidedInitServicesImpl(classProvidedCollectorRuntime, artifactRepository));
			}
			catch (Exception e) {
				throw new EPException(e);
			}

			foreach (var moduleClass in moduleClasses) {
				moduleClass.Value.LoadClasses(provider.TypeResolver);
			}

			return new DeployerModuleEPLObjects(
				beanEventTypeFactory,
				moduleEventTypes,
				moduleNamedWindows,
				moduleTables,
				moduleIndexes,
				moduleContexts,
				moduleVariables,
				moduleExpressions,
				moduleScripts,
				moduleClasses,
				eventTypeCollector.Serdes,
				eventTypeResolver);
		}

		public static void ValidateStagedEPLObjects(
			DeployerModuleEPLObjects moduleEPLObjects,
			string moduleName,
			int rolloutItemNumber,
			EPStageService stageService)
		{
			var spi = (EPStageServiceSPI) stageService;
			if (spi.IsEmpty()) {
				return;
			}

			foreach (var entry in moduleEPLObjects.ModuleContexts) {
				CheckAlreadyDefinedByStage(spi, EPObjectType.CONTEXT, svc => svc.ContextPathRegistry, entry.Key, moduleName, rolloutItemNumber);
			}

			foreach (var entry in moduleEPLObjects.ModuleNamedWindows) {
				CheckAlreadyDefinedByStage(spi, EPObjectType.NAMEDWINDOW, svc => svc.NamedWindowPathRegistry, entry.Key, moduleName, rolloutItemNumber);
			}

			foreach (var entry in moduleEPLObjects.ModuleVariables) {
				CheckAlreadyDefinedByStage(spi, EPObjectType.VARIABLE, svc => svc.VariablePathRegistry, entry.Key, moduleName, rolloutItemNumber);
			}

			foreach (var entry in moduleEPLObjects.ModuleEventTypes) {
				CheckAlreadyDefinedByStage(spi, EPObjectType.EVENTTYPE, svc => svc.EventTypePathRegistry, entry.Key, moduleName, rolloutItemNumber);
			}

			foreach (var entry in moduleEPLObjects.ModuleTables) {
				CheckAlreadyDefinedByStage(spi, EPObjectType.TABLE, svc => svc.TablePathRegistry, entry.Key, moduleName, rolloutItemNumber);
			}

			foreach (var entry in moduleEPLObjects.ModuleExpressions) {
				CheckAlreadyDefinedByStage(spi, EPObjectType.EXPRESSION, svc => svc.ExprDeclaredPathRegistry, entry.Key, moduleName, rolloutItemNumber);
			}

			foreach (var entry in moduleEPLObjects.ModuleScripts) {
				CheckAlreadyDefinedByStage(spi, EPObjectType.SCRIPT, svc => svc.ScriptPathRegistry, entry.Key, moduleName, rolloutItemNumber);
			}

			foreach (var entry in moduleEPLObjects.ModuleClasses) {
				CheckAlreadyDefinedByStage(spi, EPObjectType.CLASSPROVIDED, svc => svc.ClassProvidedPathRegistry, entry.Key, moduleName, rolloutItemNumber);
			}
		}

		private static void CheckAlreadyDefinedByStage<TK, TE>(
			EPStageServiceSPI spi,
			EPObjectType objectType,
			Func<StageSpecificServices, PathRegistry<TK, TE>> registryFunc,
			TK objectKey,
			string moduleName,
			int rolloutItemNumber)
			where TK : class
		{
			foreach (var entry in spi.Stages) {
				var registry = registryFunc.Invoke(entry.Value.StageSpecificServices);
				if (registry.GetWithModule(objectKey, moduleName) != null) {
					throw new EPDeployPreconditionException(
						objectType.GetPrettyName() + " by name '" + objectKey + "' is already defined by stage '" + entry.Key + "'",
						rolloutItemNumber);
				}
			}
		}
	}
} // end of namespace
