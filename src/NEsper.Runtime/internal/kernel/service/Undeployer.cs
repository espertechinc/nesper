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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.statement;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class Undeployer
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static void CheckModulePreconditions(
			string deploymentId,
			string moduleName,
			DeploymentInternal deployment,
			EPServicesContext services)
		{
			foreach (var namedWindow in deployment.PathNamedWindows) {
				CheckDependency(services.NamedWindowPathRegistry, namedWindow, moduleName);
			}

			foreach (var table in deployment.PathTables) {
				CheckDependency(services.TablePathRegistry, table, moduleName);
			}

			foreach (var variable in deployment.PathVariables) {
				CheckDependency(services.VariablePathRegistry, variable, moduleName);
			}

			foreach (var context in deployment.PathContexts) {
				CheckDependency(services.ContextPathRegistry, context, moduleName);
			}

			foreach (var eventType in deployment.PathEventTypes) {
				CheckDependency(services.EventTypePathRegistry, eventType, moduleName);
			}

			foreach (var exprDecl in deployment.PathExprDecls) {
				CheckDependency(services.ExprDeclaredPathRegistry, exprDecl, moduleName);
			}

			foreach (var script in deployment.PathScripts) {
				CheckDependency(services.ScriptPathRegistry, script, moduleName);
			}

			foreach (var index in deployment.PathIndexes) {
				if (index.IsNamedWindow) {
					var namedWindow = services.NamedWindowPathRegistry.GetWithModule(index.InfraName, index.InfraModuleName);
					ValidateIndexPrecondition(namedWindow.IndexMetadata, index, deploymentId);
				}
				else {
					var table = services.TablePathRegistry.GetWithModule(index.InfraName, index.InfraModuleName);
					ValidateIndexPrecondition(table.IndexMetadata, index, deploymentId);
				}
			}

			foreach (var classProvided in deployment.PathClassProvideds) {
				CheckDependency(services.ClassProvidedPathRegistry, classProvided, moduleName);
			}
		}

		public static void Disassociate(EPStatement[] statements)
		{
			foreach (var stmt in statements) {
				var statement = (EPStatementSPI) stmt;
				if (statement != null) {
					statement.ParentView = null;
					statement.SetDestroyed();
				}
			}
		}

		public static void Undeploy(
			string deploymentId,
			IDictionary<long, EventType> deploymentTypes,
			StatementContext[] reverted,
			ModuleProvider moduleProvider,
			EPServicesContext services)
		{
			foreach (var statement in reverted) {
				// remove any match-recognize counts
				services.RowRecogStatePoolEngineSvc?.RemoveStatement(new DeploymentIdNamePair(statement.DeploymentId, statement.StatementName));

				var enumerator = statement.FinalizeCallbacks;
				while (enumerator.MoveNext()) {
					enumerator.Current.StatementDestroyed(statement);
				}

				try {
					if (statement.DestroyCallback != null) {
						statement.DestroyCallback.Destroy(new StatementDestroyServices(services.FilterService), statement);
					}
					else {
						statement.StatementAIFactoryProvider.Factory.StatementDestroy(statement);
					}
				}
				catch (Exception ex) {
					Log.Error("Exception encountered during stop: " + ex.Message, ex);
				}

				if (statement.ContextRuntimeDescriptor != null) {
					try {
						services.ContextManagementService.StoppedStatement(
							statement.ContextRuntimeDescriptor.ContextDeploymentId,
							statement.ContextName,
							statement.StatementId,
							statement.StatementName,
							statement.DeploymentId);
					}
					catch (Exception ex) {
						Log.Error("Exception encountered during stop: " + ex.Message, ex);
					}
				}

				services.EpServicesHA.ListenerRecoveryService.Remove(statement.StatementId);
				services.StatementLifecycleService.RemoveStatement(statement.StatementId);
				services.PatternSubexpressionPoolRuntimeSvc.RemoveStatement(statement.StatementId);
				services.FilterSharedBoolExprRepository.RemoveStatement(statement.StatementId);
				services.FilterSharedLookupableRepository.RemoveReferencesStatement(statement.StatementId);
			}

			var moduleDependencies = moduleProvider.ModuleDependencies;
			foreach (var namedWindow in moduleDependencies.PathNamedWindows) {
				services.NamedWindowPathRegistry.RemoveDependency(namedWindow.Name, namedWindow.ModuleName, deploymentId);
			}

			foreach (var table in moduleDependencies.PathTables) {
				services.TablePathRegistry.RemoveDependency(table.Name, table.ModuleName, deploymentId);
			}

			foreach (var variable in moduleDependencies.PathVariables) {
				services.VariablePathRegistry.RemoveDependency(variable.Name, variable.ModuleName, deploymentId);
			}

			foreach (var context in moduleDependencies.PathContexts) {
				services.ContextPathRegistry.RemoveDependency(context.Name, context.ModuleName, deploymentId);
			}

			foreach (var eventType in moduleDependencies.PathEventTypes) {
				services.EventTypePathRegistry.RemoveDependency(eventType.Name, eventType.ModuleName, deploymentId);
			}

			foreach (var exprDecl in moduleDependencies.PathExpressions) {
				services.ExprDeclaredPathRegistry.RemoveDependency(exprDecl.Name, exprDecl.ModuleName, deploymentId);
			}

			foreach (var script in moduleDependencies.PathScripts) {
				services.ScriptPathRegistry.RemoveDependency(new NameAndParamNum(script.Name, script.ParamNum), script.ModuleName, deploymentId);
			}

			foreach (var classDecl in moduleDependencies.PathClasses) {
				services.ClassProvidedPathRegistry.RemoveDependency(classDecl.Name, classDecl.ModuleName, deploymentId);
			}

			foreach (var index in moduleDependencies.PathIndexes) {
				EventTableIndexMetadata indexMetadata;
				if (index.IsNamedWindow) {
					var namedWindowName = NameAndModule.FindName(index.InfraName, moduleDependencies.PathNamedWindows);
					var namedWindow = services.NamedWindowPathRegistry.GetWithModule(namedWindowName.Name, namedWindowName.ModuleName);
					indexMetadata = namedWindow.IndexMetadata;
				}
				else {
					var tableName = NameAndModule.FindName(index.InfraName, moduleDependencies.PathTables);
					var table = services.TablePathRegistry.GetWithModule(tableName.Name, tableName.ModuleName);
					indexMetadata = table.IndexMetadata;
				}

				indexMetadata.RemoveIndexReference(index.IndexName, deploymentId);
			}

			DeleteFromEventTypeBus(services, deploymentTypes);
			DeleteFromPathRegistries(services, deploymentId);

			services.TypeResolverParent.Remove(deploymentId);

			if (InstrumentationHelper.ENABLED) {
				var instrumentation = InstrumentationHelper.Get();
				foreach (var ctx in reverted) {
					instrumentation.QaEngineManagementStmtStop(
						services.RuntimeURI,
						deploymentId,
						ctx.StatementId,
						ctx.StatementName,
						(string) ctx.StatementInformationals.Properties.Get(StatementProperty.EPL),
						services.SchedulingService.Time);
				}
			}
		}

		public static void DeleteFromEventTypeBus(
			EPServicesContext services,
			IDictionary<long, EventType> eventTypes)
		{
			foreach (var entry in eventTypes) {
				if (entry.Value.Metadata.BusModifier == EventTypeBusModifier.BUS) {
					services.EventTypeRepositoryBus.RemoveType(entry.Value);
				}
			}
		}

		public static void DeleteFromPathRegistries(
			EPServicesContext services,
			string deploymentId)
		{
			services.EventTypePathRegistry.DeleteDeployment(deploymentId);
			services.NamedWindowPathRegistry.DeleteDeployment(deploymentId);
			services.TablePathRegistry.DeleteDeployment(deploymentId);
			services.ContextPathRegistry.DeleteDeployment(deploymentId);
			services.VariablePathRegistry.DeleteDeployment(deploymentId);
			services.ExprDeclaredPathRegistry.DeleteDeployment(deploymentId);
			services.ScriptPathRegistry.DeleteDeployment(deploymentId);
			services.ClassProvidedPathRegistry.DeleteDeployment(deploymentId);
			services.EventTypeSerdeRepository.RemoveSerdes(deploymentId);
		}

		private static void CheckDependency<TK, TR>(
			PathRegistry<TK, TR> registry,
			TK entityKey,
			string moduleName) where TK : class
		{
			var dependencies = registry.GetDependencies(entityKey, moduleName);
			if (dependencies != null && !dependencies.IsEmpty()) {
				throw MakeException(registry.ObjectType, entityKey.ToString(), dependencies.First());
			}
		}

		private static EPUndeployPreconditionException MakeException(
			PathRegistryObjectType objectType,
			string name,
			string otherDeploymentId)
		{
			var objectName = objectType.Name;
			var firstUppercase = objectName.Substring(0, 1).ToUpperInvariant() + objectName.Substring(1);
			return new EPUndeployPreconditionException(
				firstUppercase + " '" + name + "' cannot be un-deployed as it is referenced by deployment '" + otherDeploymentId + "'");
		}

		private static void ValidateIndexPrecondition(
			EventTableIndexMetadata indexMetadata,
			ModuleIndexMeta index,
			string deploymentId)
		{
			var imk = indexMetadata.GetIndexByName(index.IndexName);
			var entry = indexMetadata.Indexes.Get(imk);
			if (entry == null) {
				return;
			}

			var referring = indexMetadata.Indexes.Get(imk).ReferringDeployments;
			if (referring != null && referring.Length > 0) {
				string first = null;
				foreach (var referringeploymentId in referring) {
					if (!referringeploymentId.Equals(deploymentId)) {
						first = referringeploymentId;
					}
				}

				if (first != null) {
					throw MakeException(PathRegistryObjectType.INDEX, index.IndexName, first);
				}
			}
		}
	}
} // end of namespace
