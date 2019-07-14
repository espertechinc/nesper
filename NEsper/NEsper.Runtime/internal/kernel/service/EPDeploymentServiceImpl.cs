///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.statement;

//using static com.espertech.esper.common.client.util.UndeployRethrowPolicy.RETHROW_FIRST;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class EPDeploymentServiceImpl : EPDeploymentServiceSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPRuntimeSPI runtime;

        private readonly EPServicesContext services;

        public EPDeploymentServiceImpl(
            EPServicesContext services,
            EPRuntimeSPI runtime)
        {
            this.services = services;
            this.runtime = runtime;
        }

        public EPDeployment Deploy(EPCompiled compiled)
        {
            return Deploy(compiled, new DeploymentOptions());
        }

        public EPDeployment Deploy(
            EPCompiled compiled,
            DeploymentOptions options)
        {
            if (runtime.IsDestroyed) {
                throw new EPRuntimeDestroyedException(runtime.URI);
            }

            DeploymentInternal deployerResult;

            using (options.DeploymentLockStrategy.Acquire(services.EventProcessingRWLock)) {
                var statementIdRecovery = services.EpServicesHA.StatementIdRecoveryService;
                var currentStatementId = statementIdRecovery.CurrentStatementId ?? 1;

                string deploymentId;
                if (options.DeploymentId == null) {
                    deploymentId = Guid.NewGuid().ToString();
                }
                else {
                    deploymentId = options.DeploymentId;
                }

                if (services.DeploymentLifecycleService.GetDeploymentById(deploymentId) != null) {
                    throw new EPDeployException("Deployment by id '" + deploymentId + "' already exists");
                }

                deployerResult = Deployer.DeployFresh(
                    deploymentId,
                    currentStatementId,
                    compiled,
                    options.StatementNameRuntime,
                    options.StatementUserObjectRuntime,
                    options.StatementSubstitutionParameter,
                    runtime);
                statementIdRecovery.CurrentStatementId = currentStatementId + deployerResult.Statements.Length;

                // dispatch event
                DispatchOnDeploymentEvent(deployerResult);
            }

            var copy = new EPStatement[deployerResult.Statements.Length];
            Array.Copy(deployerResult.Statements, 0, copy, 0, deployerResult.Statements.Length);
            return new EPDeployment(
                deployerResult.DeploymentId, deployerResult.ModuleProvider.ModuleName, deployerResult.ModulePropertiesCached, copy,
                CollectionUtil.CopyArray(deployerResult.DeploymentIdDependencies), DateTimeHelper.GetCurrentTimeUniversal());
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

            return services.DeploymentLifecycleService.GetStatementByName(deploymentId, statementName);
        }

        public string[] Deployments => services.DeploymentLifecycleService.DeploymentIds;

        public IDictionary<string, DeploymentInternal> DeploymentMap => services.DeploymentLifecycleService.DeploymentMap;

        public EPDeployment GetDeployment(string deploymentId)
        {
            var deployed = services.DeploymentLifecycleService.GetDeploymentById(deploymentId);
            if (deployed == null) {
                return null;
            }

            var stmts = deployed.Statements;
            var copy = new EPStatement[stmts.Length];
            Array.Copy(stmts, 0, copy, 0, stmts.Length);
            return new EPDeployment(
                deploymentId, deployed.ModuleProvider.ModuleName, deployed.ModulePropertiesCached, copy,
                CollectionUtil.CopyArray(deployed.DeploymentIdDependencies), new DateTime(deployed.LastUpdateDate));
        }

        public void UndeployAll()
        {
            UndeployAllInternal(null);
        }

        public void UndeployAll(UndeploymentOptions options)
        {
            UndeployAllInternal(options);
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

        public void Destroy()
        {
        }

        public void AddDeploymentStateListener(DeploymentStateListener listener)
        {
            services.DeploymentLifecycleService.Listeners.Add(listener);
        }

        public void RemoveDeploymentStateListener(DeploymentStateListener listener)
        {
            services.DeploymentLifecycleService.Listeners.Remove(listener);
        }

        public IEnumerator<DeploymentStateListener> DeploymentStateListeners => services.DeploymentLifecycleService.Listeners.GetEnumerator();

        public void RemoveAllDeploymentStateListeners()
        {
            services.DeploymentLifecycleService.Listeners.Clear();
        }

        private void UndeployAllInternal(UndeploymentOptions options)
        {
            if (options == null) {
                options = new UndeploymentOptions();
            }

            var deploymentSvc = services.DeploymentLifecycleService;
            var deployments = services.DeploymentLifecycleService.DeploymentIds;
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
            IDictionary<string, int> deploymentIndexes = new Dictionary<string, int>();
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
                    var fromIndex = deploymentIndexes.Get(deploymentId);
                    var targetIndex = deploymentIndexes.Get(target);
                    graph.AddDependency(targetIndex, fromIndex);
                }
            }

            ISet<string> undeployed = new HashSet<string>();
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
            foreach (var dependency in dependencies) {
                RecursiveUndeploy(dependency, deployments, graph, undeployed, options);
            }

            var next = deployments[index];
            if (!undeployed.Add(next)) {
                return;
            }

            Undeploy(next, options);
        }

        private void UndeployRemoveInternal(
            string deploymentId,
            UndeploymentOptions options)
        {
            var deployment = services.DeploymentLifecycleService.GetDeploymentById(deploymentId);
            if (deployment == null) {
                throw new EPUndeployNotFoundException("Deployment id '" + deploymentId + "' cannot be found");
            }

            var statements = deployment.Statements;

            if (options == null) {
                options = new UndeploymentOptions();
            }

            using (options.UndeploymentLockStrategy.Acquire(services.EventProcessingRWLock)) {
                // build list of statements in reverse order
                var reverted = new StatementContext[statements.Length];
                var count = reverted.Length - 1;
                foreach (var stmt in statements) {
                    reverted[count--] = ((EPStatementSPI) stmt).StatementContext;
                }

                // check module preconditions
                var moduleName = deployment.ModuleProvider.ModuleName;
                Undeployer.CheckModulePreconditions(deploymentId, moduleName, deployment, services);

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
                    Undeployer.Undeploy(
                        deploymentId,
                        deployment.DeploymentTypes,
                        reverted,
                        deployment.ModuleProvider,
                        services);
                }
                catch (Exception ex) {
                    Log.Error("Exception encountered during undeploy: " + ex.Message, ex);
                    undeployException = ex;
                }

                // remove deployment
                services.EpServicesHA.DeploymentRecoveryService.Remove(deploymentId);
                services.DeploymentLifecycleService.Undeploy(deploymentId);

                DispatchOnUndeploymentEvent(deployment);

                // rethrow exception if configured
                if (undeployException != null &&
                    services.ConfigSnapshot.Runtime.ExceptionHandling.UndeployRethrowPolicy ==
                    UndeployRethrowPolicy.RETHROW_FIRST) {
                    throw new EPUndeployException(
                        "Undeploy completed with an exception: " + undeployException.Message,
                        undeployException);
                }

                ((EPEventServiceSPI) runtime.EventService).ClearCaches();
            }
        }

        private void DispatchOnDeploymentEvent(DeploymentInternal deployed)
        {
            var listeners = services.DeploymentLifecycleService.Listeners;
            if (listeners.IsEmpty()) {
                return;
            }

            var stmts = deployed.Statements;
            var @event = new DeploymentStateEventDeployed(
                services.RuntimeURI,
                deployed.DeploymentId, deployed.ModuleProvider.ModuleName, stmts);
            foreach (var listener in listeners) {
                try {
                    listener.OnDeployment(@event);
                }
                catch (Exception ex) {
                    HandleDeploymentEventListenerException("on-deployment", ex);
                }
            }
        }

        private void DispatchOnUndeploymentEvent(DeploymentInternal result)
        {
            var listeners = services.DeploymentLifecycleService.Listeners;
            if (listeners.IsEmpty()) {
                return;
            }

            var statements = result.Statements;
            var @event = new DeploymentStateEventUndeployed(
                services.RuntimeURI,
                result.DeploymentId, result.ModuleProvider.ModuleName, statements);
            foreach (var listener in listeners) {
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
            Log.Error(
                "Application-provided deployment state listener reported an exception upon receiving the " + typeOfOperation +
                " event, logging and ignoring the exception, detail: " + ex.Message, ex);
        }
    }
} // end of namespace