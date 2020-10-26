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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.pattern.pool;
using com.espertech.esper.common.@internal.epl.rowrecog.state;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.statement.insertintolatch;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.option;
using com.espertech.esper.runtime.@internal.kernel.statement;
using com.espertech.esper.runtime.@internal.kernel.updatedispatch;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class DeployerHelperInitStatement
	{
		public static DeployerModuleStatementLightweights InitializeStatements(
			int rolloutItemNumber,
			bool recovery,
			DeployerModuleEPLObjects moduleEPLObjects,
			DeployerModulePaths modulePaths,
			string moduleName,
			ModuleProviderCLPair moduleProvider,
			string deploymentId,
			int statementIdFirstStatement,
			StatementUserObjectRuntimeOption userObjectResolverRuntime,
			StatementNameRuntimeOption statementNameResolverRuntime,
			StatementSubstitutionParameterOption substitutionParameterResolver,
			EPServicesContext services)
		{
			// get module statements
			IList<StatementProvider> statementResources;
			try {
				statementResources = moduleProvider.ModuleProvider.Statements;
			}
			catch (Exception e) {
				throw new EPException(e);
			}

			// initialize all statements
			IList<StatementLightweight> lightweights = new List<StatementLightweight>();
			IDictionary<int, IDictionary<int, object>> substitutionParameters;
			ISet<string> statementNames = new HashSet<string>();
			var moduleIncidentals = moduleEPLObjects.Incidentals;
			try {
				var statementId = statementIdFirstStatement;
				foreach (var statement in statementResources) {
					var lightweight = InitStatement(
						recovery,
						moduleName,
						statement,
						deploymentId,
						statementId,
						moduleEPLObjects.EventTypeResolver,
						moduleIncidentals,
						statementNameResolverRuntime,
						userObjectResolverRuntime,
						moduleProvider.ClassLoader,
						services);
					lightweights.Add(lightweight);
					statementId++;

					var statementName = lightweight.StatementContext.StatementName;
					if (statementNames.Contains(statementName)) {
						throw new EPDeployException(
							"Duplicate statement name provide by statement name resolver for statement name '" + statementName + "'",
							rolloutItemNumber);
					}

					statementNames.Add(statementName);
				}

				// set parameters
				substitutionParameters = SetSubstitutionParameterValues(rolloutItemNumber, deploymentId, lightweights, substitutionParameterResolver);
			}
			catch (Exception) {
				DeployerHelperResolver.ReverseDeployment(deploymentId, modulePaths.DeploymentTypes, lightweights, new EPStatement[0], moduleProvider, services);
				throw;
			}

			return new DeployerModuleStatementLightweights(statementIdFirstStatement, lightweights, substitutionParameters);
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
			ClassLoader moduleClassLoader,
			EPServicesContext services)
		{
			var informationals = statementProvider.Informationals;

			// set instrumentation unless already provided
			if (informationals.InstrumentationProvider == null) {
				informationals.InstrumentationProvider = InstrumentationDefault.INSTANCE;
			}

			var statementResultService = new StatementResultServiceImpl(informationals, services);
			FilterSharedLookupableRegistery filterSharedLookupableRegistery = new ProxyFilterSharedLookupableRegistery() {
				ProcRegisterLookupable = (
					eventTypeX,
					lookupable) => {
					services.FilterSharedLookupableRepository.RegisterLookupable(statementId, eventTypeX, lookupable);
				},
			};

			FilterSharedBoolExprRegistery filterSharedBoolExprRegistery = new ProxyFilterSharedBoolExprRegistery() {
				ProcRegisterBoolExpr = (node) => { services.FilterSharedBoolExprRepository.RegisterBoolExpr(statementId, node); },
			};

			IDictionary<int, FilterSpecActivatable> filterSpecActivatables = new Dictionary<int, FilterSpecActivatable>();
			FilterSpecActivatableRegistry filterSpecActivatableRegistry = new ProxyFilterSpecActivatableRegistry() {
				ProcRegister = (filterSpecActivatable) => { filterSpecActivatables.Put(filterSpecActivatable.FilterCallbackId, filterSpecActivatable); },
			};

			var contextPartitioned = informationals.OptionalContextName != null;
			var statementResourceService = new StatementResourceService(contextPartitioned);

			// determine statement name
			var statementName = informationals.StatementNameCompileTime;
			if (statementNameResolverRuntime != null) {
				string statementNameAssigned = statementNameResolverRuntime.Invoke(
					new StatementNameRuntimeContext(
						deploymentId,
						statementName,
						statementId,
						(string) informationals.Properties.Get(StatementProperty.EPL),
						informationals.Annotations));
				if (statementNameAssigned != null) {
					statementName = statementNameAssigned;
				}
			}

			statementName = statementName.Trim();

			var epInitServices = new EPStatementInitServicesImpl(
				statementName,
				informationals.Properties,
				informationals.Annotations,
				deploymentId,
				eventTypeResolver,
				filterSpecActivatableRegistry,
				filterSharedBoolExprRegistery,
				filterSharedLookupableRegistery,
				moduleIncidentals,
				recovery,
				statementResourceService,
				statementResultService,
				services);

			if (!services.EpServicesHA.RuntimeExtensionServices.IsHAEnabled) {
				statementProvider.Initialize(epInitServices);
			}
			else {
				// for HA we set the context classloader as state may be loaded considering the module provider's classloader
				// - NEsper doesn't support HA like this, and we wouldn't want to deliver this information in
				// - this manner.  An alternative delivery mechanism must be established to carry the information
				// - without relying on the "current" thread to carry that detail.

				// ClassLoader originalClassLoader = Thread.CurrentThread().ContextClassLoader;
				// try {
				// 	Thread.CurrentThread().ContextClassLoader = moduleClassLoader;
				// 	statementProvider.Initialize(epInitServices);
				// }
				// finally {
				// 	Thread.CurrentThread().ContextClassLoader = originalClassLoader;
				// }
			}

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
					latchFactoryNameFront,
					informationals.IsStateless,
					msecTimeout,
					locking,
					services.TimeSourceService);
				var latchFactoryBack = new InsertIntoLatchFactory(
					latchFactoryNameBack,
					informationals.IsStateless,
					msecTimeout,
					locking,
					services.TimeSourceService);
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
					informationals.OptionalContextModuleName,
					informationals.OptionalContextVisibility,
					optionalContextName,
					deploymentId,
					services.ContextPathRegistry);
				var contextManager = services.ContextManagementService.GetContextManager(contextDeploymentId, optionalContextName);
				contextRuntimeDescriptor = contextManager.ContextRuntimeDescriptor;
				var registryRequirements = statementProvider.StatementAIFactoryProvider.Factory.RegistryRequirements;
				statementAgentInstanceRegistry = contextManager.AllocateAgentInstanceResourceRegistry(registryRequirements);
			}

			var statementCPCacheService = new StatementCPCacheService(
				contextPartitioned,
				statementResourceService,
				statementAgentInstanceRegistry);

			var eventType = statementProvider.StatementAIFactoryProvider.Factory.StatementEventType;

			var configurationThreading = services.RuntimeSettingsService.ConfigurationRuntime.Threading;
			var preserveDispatchOrder = configurationThreading.IsListenerDispatchPreserveOrder && !informationals.IsStateless;
			var isSpinLocks = configurationThreading.ListenerDispatchLocking == Locking.SPIN;
			var msecBlockingTimeout = configurationThreading.ListenerDispatchTimeout;
			UpdateDispatchViewBase dispatchChildView;
			if (preserveDispatchOrder) {
				if (isSpinLocks) {
					dispatchChildView = new UpdateDispatchViewBlockingSpin(
						eventType,
						statementResultService,
						services.DispatchService,
						msecBlockingTimeout,
						services.TimeSourceService);
				}
				else {
					dispatchChildView = new UpdateDispatchViewBlockingWait(eventType, statementResultService, services.DispatchService, msecBlockingTimeout);
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
						deploymentId,
						statementName,
						statementId,
						(string) informationals.Properties.Get(StatementProperty.EPL),
						informationals.Annotations));
			}

			var statementContext = new StatementContext(
				services.Container,
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
				dispatchChildView,
				services.FilterService,
				services.SchedulingService,
				services.InternalEventRouteDest
			);

			foreach (var readyCallback in epInitServices.ReadyCallbacks) {
				readyCallback.Ready(statementContext, moduleIncidentals, recovery);
			}

			return new StatementLightweight(statementProvider, informationals, statementResultService, statementContext);
		}

		private static IDictionary<int, IDictionary<int, object>> SetSubstitutionParameterValues(
			int rolloutItemNumber,
			string deploymentId,
			IList<StatementLightweight> lightweights,
			StatementSubstitutionParameterOption substitutionParameterResolver)
		{
			if (substitutionParameterResolver == null) {
				foreach (var lightweight in lightweights) {
					var required = lightweight.StatementInformationals.SubstitutionParamTypes;
					if (required != null && required.Length > 0) {
						throw new EPDeploySubstitutionParameterException(
							"Statement '" + lightweight.StatementContext.StatementName + "' has " + required.Length + " substitution parameters",
							rolloutItemNumber);
					}
				}

				return EmptyDictionary<int, IDictionary<int, object>>.Instance;
			}

			IDictionary<int, IDictionary<int, object>> providedAllStmt = new Dictionary<int, IDictionary<int, object>>();
			foreach (var lightweight in lightweights) {
				var substitutionTypes = lightweight.StatementInformationals.SubstitutionParamTypes;
				IDictionary<string, int> paramNames = lightweight.StatementInformationals.SubstitutionParamNames;
				var handler = new DeployerSubstitutionParameterHandler(
					deploymentId,
					lightweight,
					providedAllStmt,
					substitutionTypes,
					paramNames);

				try {
					substitutionParameterResolver.Invoke(handler);
				}
				catch (Exception ex) {
					throw new EPDeploySubstitutionParameterException(
						"Failed to set substitution parameter value for statement '" + lightweight.StatementContext.StatementName + "': " + ex.Message,
						ex,
						rolloutItemNumber);
				}

				if (substitutionTypes == null || substitutionTypes.Length == 0) {
					continue;
				}

				// check that all values are provided
				var provided = providedAllStmt.Get(lightweight.StatementContext.StatementId);
				var providedSize = provided == null ? 0 : provided.Count;
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
								"Missing value for substitution parameter " + name + " for statement '" + lightweight.StatementContext.StatementName + "'",
								rolloutItemNumber);
						}
					}
				}
			}

			return providedAllStmt;
		}
	}
} // end of namespace
