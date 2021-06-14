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
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.util;
using com.espertech.esper.runtime.@internal.kernel.statement;

using static com.espertech.esper.common.client.util.UndeployRethrowPolicy; // RETHROW_FIRST
using static com.espertech.esper.runtime.@internal.kernel.service.DeployerHelperDependencies; // getDependenciesConsumed, getDependenciesProvided

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class EPDeploymentServiceImpl : EPDeploymentServiceSPI
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly EPServicesContext _services;
		private readonly EPRuntimeSPI _runtime;

		public EPDeploymentServiceImpl(
			EPServicesContext services,
			EPRuntimeSPI runtime)
		{
			_services = services;
			_runtime = runtime;
		}

		public EPDeploymentRollout Rollout(ICollection<EPDeploymentRolloutCompiled> items)
		{
			return Rollout(items, new RolloutOptions());
		}

		private IDisposable GetRolloutLock(RolloutOptions options)
		{
			try {
				return options.RolloutLockStrategy.Acquire(_services.EventProcessingRWLock);
			}
			catch (Exception e) {
				throw new EPDeployLockException(e.Message, e);
			}
		}

		private IDisposable GetDeploymentLock(DeploymentOptions options)
		{
			try {
				return options.DeploymentLockStrategy.Acquire(_services.EventProcessingRWLock);
			}
			catch (Exception e) {
				throw new EPDeployLockException(e.Message, e);
			}
		}

		public EPDeploymentRollout Rollout(
			ICollection<EPDeploymentRolloutCompiled> items,
			RolloutOptions options)
		{
			if (options == null) {
				options = new RolloutOptions();
			}

			ValidateRuntimeAlive();
			var rollItemNum = 0;
			foreach (var item in items) {
				CheckManifest(rollItemNum++, item.Compiled.Manifest);
			}

			DeployerRolloutDeploymentResult rolloutResult;

			using (GetRolloutLock(options)) {
				var statementIdRecovery = _services.EpServicesHA.StatementIdRecoveryService;
				var currentStatementId = statementIdRecovery.CurrentStatementId;
				if (currentStatementId == null) {
					currentStatementId = 1;
				}

				rolloutResult = DeployerRollout.Rollout(currentStatementId.Value, items, _runtime);
				statementIdRecovery.CurrentStatementId = currentStatementId + rolloutResult.NumStatements;

				// dispatch event
				for (var i = 0; i < rolloutResult.Deployments.Length; i++) {
					DispatchOnDeploymentEvent(rolloutResult.Deployments[i], i);
				}
			}

			var deployments = new EPDeploymentRolloutItem[items.Count];
			for (var i = 0; i < rolloutResult.Deployments.Length; i++) {
				var deployment = MakeDeployment(rolloutResult.Deployments[i]);
				deployments[i] = new EPDeploymentRolloutItem(deployment);
			}

			return new EPDeploymentRollout(deployments);
		}

		public EPDeployment Deploy(EPCompiled compiled)
		{
			return Deploy(compiled, new DeploymentOptions());
		}

		public EPDeployment Deploy(
			EPCompiled compiled,
			DeploymentOptions options)
		{
			using (_services.Container.EnterContextualReflection()) {
				if (options == null) {
					options = new DeploymentOptions();
				}

				ValidateRuntimeAlive();
				CheckManifest(-1, compiled.Manifest);

				DeploymentInternal deployerResult;

				using (GetDeploymentLock(options)) {
					var statementIdRecovery = _services.EpServicesHA.StatementIdRecoveryService;
					var currentStatementId = statementIdRecovery.CurrentStatementId;
					if (currentStatementId == null) {
						currentStatementId = 1;
					}

					var deploymentId = DeployerHelperResolver.DetermineDeploymentIdCheckExists(
						-1,
						options,
						_runtime.ServicesContext.DeploymentLifecycleService);
					deployerResult = Deployer.DeployFresh(
						deploymentId,
						currentStatementId.Value,
						compiled,
						options.StatementNameRuntime,
						options.StatementUserObjectRuntime,
						options.StatementSubstitutionParameter,
						options.DeploymentClassLoaderOption,
						_runtime);
					statementIdRecovery.CurrentStatementId = currentStatementId + deployerResult.Statements.Length;

					// dispatch event
					DispatchOnDeploymentEvent(deployerResult, -1);
				}

				return MakeDeployment(deployerResult);
			}
		}

		public EPStatement GetStatement(
			string deploymentId,
			string statementName)
		{
			if (deploymentId == null) {
				throw new ArgumentException("Missing deployment-id parameter");
			}

			if (statementName == null) {
				throw new ArgumentException("Missing statement-name parameter");
			}

			return _services.DeploymentLifecycleService.GetStatementByName(deploymentId, statementName);
		}

		public string[] Deployments {
			get { return _services.DeploymentLifecycleService.DeploymentIds; }
		}

		public IDictionary<string, DeploymentInternal> DeploymentMap {
			get { return _services.DeploymentLifecycleService.DeploymentMap; }
		}

		public EPDeployment GetDeployment(string deploymentId)
		{
			return EPDeploymentServiceUtil.ToDeployment(_services.DeploymentLifecycleService, deploymentId);
		}

		public bool IsDeployed(string deploymentId)
		{
			return _services.DeploymentLifecycleService.GetDeploymentById(deploymentId) != null;
		}

		public void UndeployAll()
		{
			UndeployAllInternal(null);
		}

		public void UndeployAll(UndeploymentOptions options)
		{
			UndeployAllInternal(options);
		}

		private void UndeployAllInternal(UndeploymentOptions options)
		{
			if (options == null) {
				options = new UndeploymentOptions();
			}

			var deploymentSvc = _services.DeploymentLifecycleService;
			var deployments = _services.DeploymentLifecycleService.DeploymentIds;
			if (deployments.Length == 0) {
				return;
			}

			if (deployments.Length == 1) {
				Undeploy(deployments[0]);
				return;
			}

			if (deployments.Length == 2) {
				var zero = deploymentSvc.GetDeploymentById(deployments[0]);
				var zeroDependsOn = zero.DeploymentIdDependencies;
				if (zeroDependsOn != null && zeroDependsOn.Length > 0) {
					Undeploy(deployments[0]);
					Undeploy(deployments[1]);
				}
				else {
					Undeploy(deployments[1]);
					Undeploy(deployments[0]);
				}

				return;
			}

			// build map of deployment-to-index
			var deploymentIndexes = new Dictionary<string, int>();
			var count = 0;
			foreach (var deployment in deployments) {
				deploymentIndexes.Put(deployment, count++);
			}

			var graph = new DependencyGraph(deployments.Length, false);
			foreach (var deploymentId in deployments) {
				var deployment = deploymentSvc.GetDeploymentById(deploymentId);
				var dependentOn = deployment.DeploymentIdDependencies;
				if (dependentOn == null || dependentOn.Length == 0) {
					continue;
				}

				foreach (var target in dependentOn) {
					int fromIndex = deploymentIndexes.Get(deploymentId);
					int targetIndex = deploymentIndexes.Get(target);
					graph.AddDependency(targetIndex, fromIndex);
				}
			}

			var undeployed = new HashSet<string>();
			foreach (var rootIndex in graph.RootNodes) {
				RecursiveUndeploy(rootIndex, deployments, graph, undeployed, options);
			}
		}

		private void RecursiveUndeploy(
			int index,
			string[] deployments,
			DependencyGraph graph,
			ISet<string> undeployed,
			UndeploymentOptions options)
		{
			var dependencies = graph.GetDependenciesForStream(index);
			foreach (int dependency in dependencies) {
				RecursiveUndeploy(dependency, deployments, graph, undeployed, options);
			}

			var next = deployments[index];
			if (!undeployed.Add(next)) {
				return;
			}

			Undeploy(next, options);
		}

		public void Undeploy(string deploymentId)
		{
			UndeployRemoveInternal(deploymentId, null);
		}

		public void Undeploy(
			string deploymentId,
			UndeploymentOptions options)
		{
			UndeployRemoveInternal(deploymentId, options);
		}

		private void UndeployRemoveInternal(
			string deploymentId,
			UndeploymentOptions options)
		{
			var deployment = _services.DeploymentLifecycleService.GetDeploymentById(deploymentId);
			if (deployment == null) {
				var stageUri = _services.StageRecoveryService.DeploymentGetStage(deploymentId);
				if (stageUri != null) {
					throw new EPUndeployPreconditionException("Deployment id '" + deploymentId + "' is staged and cannot be undeployed");
				}

				throw new EPUndeployNotFoundException("Deployment id '" + deploymentId + "' cannot be found");
			}

			var statements = deployment.Statements;

			if (options == null) {
				options = new UndeploymentOptions();
			}


			using (options.UndeploymentLockStrategy.Acquire(_services.EventProcessingRWLock)) {
				// build list of statements in reverse order
				var reverted = new StatementContext[statements.Length];
				var count = reverted.Length - 1;
				foreach (var stmt in statements) {
					reverted[count--] = ((EPStatementSPI) stmt).StatementContext;
				}

				// check module preconditions
				var moduleName = deployment.ModuleProvider.ModuleName;
				Undeployer.CheckModulePreconditions(deploymentId, moduleName, deployment, _services);

				// check preconditions
				try {
					foreach (var statement in reverted) {
						statement.StatementAIFactoryProvider.Factory.StatementDestroyPreconditions(statement);
					}
				}
				catch (UndeployPreconditionException t) {
					throw new EPUndeployException("Precondition not satisfied for undeploy: " + t.Message, t);
				}

				// disassociate statements
				Undeployer.Disassociate(statements);

				// undeploy statements
				Exception undeployException = null;
				try {
					Undeployer.Undeploy(deploymentId, deployment.DeploymentTypes, reverted, deployment.ModuleProvider, _services);
				}
				catch (Exception ex) {
					log.Error("Exception encountered during undeploy: " + ex.Message, ex);
					undeployException = ex;
				}

				// remove deployment
				_services.EpServicesHA.DeploymentRecoveryService.Remove(deploymentId);
				_services.DeploymentLifecycleService.RemoveDeployment(deploymentId);

				DispatchOnUndeploymentEvent(deployment, -1);

				// rethrow exception if configured
				if (undeployException != null &&
				    _services.ConfigSnapshot.Runtime.ExceptionHandling.UndeployRethrowPolicy == RETHROW_FIRST) {
					throw new EPUndeployException("Undeploy completed with an exception: " + undeployException.Message, undeployException);
				}

				((EPEventServiceSPI) _runtime.EventService).ClearCaches();
			}
		}

		public void Destroy()
		{
		}

		public void AddDeploymentStateListener(DeploymentStateListener listener)
		{
			_services.DeploymentLifecycleService.Listeners.Add(listener);
		}

		public void RemoveDeploymentStateListener(DeploymentStateListener listener)
		{
			_services.DeploymentLifecycleService.Listeners.Remove(listener);
		}

		public IEnumerator<DeploymentStateListener> DeploymentStateListeners => 
			_services.DeploymentLifecycleService.Listeners.GetEnumerator();

		public void RemoveAllDeploymentStateListeners()
		{
			_services.DeploymentLifecycleService.Listeners.Clear();
		}

		public EPDeploymentDependencyProvided GetDeploymentDependenciesProvided(string selfDeploymentId)
		{
			if (selfDeploymentId == null) {
				throw new ArgumentException("deployment-id is null");
			}

			using (_services.EventProcessingRWLock.AcquireReadLock()) {
				return GetDependenciesProvided(selfDeploymentId, _services, _services.DeploymentLifecycleService);
			}
		}

		public EPDeploymentDependencyConsumed GetDeploymentDependenciesConsumed(string selfDeploymentId)
		{
			if (selfDeploymentId == null) {
				throw new ArgumentException("deployment-id is null");
			}

			using (_services.EventProcessingRWLock.AcquireReadLock()) {
				return GetDependenciesConsumed(selfDeploymentId, _services, _services.DeploymentLifecycleService);
			}
		}

		private void DispatchOnDeploymentEvent(
			DeploymentInternal deployed,
			int rolloutItemNumber)
		{
			var listeners = _services.DeploymentLifecycleService.Listeners;
			if (listeners.IsEmpty()) {
				return;
			}

			var stmts = deployed.Statements;
			var @event = new DeploymentStateEventDeployed(
				_services.RuntimeURI,
				deployed.DeploymentId,
				deployed.ModuleProvider.ModuleName,
				stmts,
				rolloutItemNumber);
			foreach (DeploymentStateListener listener in listeners) {
				try {
					listener.OnDeployment(@event);
				}
				catch (Exception ex) {
					HandleDeploymentEventListenerException("on-deployment", ex);
				}
			}
		}

		private void DispatchOnUndeploymentEvent(
			DeploymentInternal result,
			int rolloutItemNumber)
		{
			var listeners = _services.DeploymentLifecycleService.Listeners;
			if (listeners.IsEmpty()) {
				return;
			}

			var statements = result.Statements;
			var @event = new DeploymentStateEventUndeployed(
				_services.RuntimeURI,
				result.DeploymentId,
				result.ModuleProvider.ModuleName,
				statements,
				rolloutItemNumber);
			foreach (DeploymentStateListener listener in listeners) {
				try {
					listener.OnUndeployment(@event);
				}
				catch (Exception ex) {
					HandleDeploymentEventListenerException("on-undeployment", ex);
				}
			}
		}

		private void HandleDeploymentEventListenerException(
			string typeOfOperation,
			Exception ex)
		{
			log.Error(
				"Application-provided deployment state listener reported an exception upon receiving the " +
				typeOfOperation +
				" event, logging and ignoring the exception, detail: " +
				ex.Message,
				ex);
		}

		private void ValidateRuntimeAlive()
		{
			if (_runtime.IsDestroyed) {
				throw new EPRuntimeDestroyedException(_runtime.URI);
			}
		}

		private void CheckManifest(
			int rolloutItemNumber,
			EPCompiledManifest manifest)
		{
			try {
				RuntimeVersion.CheckVersion(manifest.CompilerVersion);
			}
			catch (RuntimeVersion.VersionException ex) {
				throw new EPDeployDeploymentVersionException(ex.Message, ex, rolloutItemNumber);
			}

			if (manifest.ModuleProviderClassName == null) {
				if (manifest.QueryProviderClassName != null) {
					throw new EPDeployException(
						"Cannot deploy EPL that was compiled as a fire-and-forget query, make sure to use the 'compile' method of the compiler",
						rolloutItemNumber);
				}

				throw new EPDeployException("Failed to find module provider class name in manifest (is this a compiled module?)", rolloutItemNumber);
			}

			try {
				_services.EventSerdeFactory.VerifyHADeployment(manifest.IsTargetHA);
			}
			catch (ExprValidationException ex) {
				throw new EPDeployException(ex.Message, ex, rolloutItemNumber);
			}
		}

		private EPDeployment MakeDeployment(DeploymentInternal deployerResult)
		{
			var copy = new EPStatement[deployerResult.Statements.Length];
			Array.Copy(deployerResult.Statements, 0, copy, 0, deployerResult.Statements.Length);
			return new EPDeployment(
				deployerResult.DeploymentId,
				deployerResult.ModuleProvider.ModuleName,
				deployerResult.ModulePropertiesCached,
				copy,
				CollectionUtil.CopyArray(deployerResult.DeploymentIdDependencies),
				DateTimeHelper.GetCurrentTime());
		}
	}
} // end of namespace
