///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public class ContextManagementServiceImpl : ContextManagementService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDictionary<string, ContextDeployment> deployments =
            new Dictionary<string, ContextDeployment>();

        public void AddContext(
            ContextDefinition contextDefinition,
            EPStatementInitServices services)
        {
            var deployment = deployments.Get(services.DeploymentId);
            if (deployment == null) {
                deployment = new ContextDeployment();
                deployments.Put(services.DeploymentId, deployment);
            }

            deployment.Add(contextDefinition, services);
        }

        public void AddStatement(
            string deploymentIdCreateContext,
            string contextName,
            ContextControllerStatementDesc statement,
            bool recovery)
        {
            var contextManager = GetAssertContextManager(deploymentIdCreateContext, contextName);
            contextManager.AddStatement(statement, recovery);
        }

        public void StoppedStatement(
            string deploymentIdCreateContext,
            string contextName,
            ContextControllerStatementDesc statement)
        {
            var contextManager = GetAssertContextManager(deploymentIdCreateContext, contextName);
            contextManager.StopStatement(statement);
        }

        public ContextManager GetContextManager(
            string deploymentIdCreateContext,
            string contextName)
        {
            var deployment = deployments.Get(deploymentIdCreateContext);
            if (deployment == null) {
                return null;
            }

            return deployment.GetContextManager(contextName);
        }

        public int ContextCount {
            get {
                var count = 0;
                foreach (var entry in deployments) {
                    count += entry.Value.ContextCount;
                }

                return count;
            }
        }

        public void DestroyedContext(
            string runtimeURI,
            string deploymentIdCreateContext,
            string contextName)
        {
            var deployment = deployments.Get(deploymentIdCreateContext);
            if (deployment == null) {
                Log.Warn(
                    "Destroy for context '" + contextName + "' deployment-id '" + deploymentIdCreateContext +
                    "' failed to locate");
                return;
            }

            deployment.DestroyContext(deploymentIdCreateContext, contextName);
            if (deployment.ContextCount == 0) {
                deployments.Remove(deploymentIdCreateContext);
            }

            ContextStateEventUtil.DispatchContext(
                Listeners,
                () => new ContextStateEventContextDestroyed(runtimeURI, deploymentIdCreateContext, contextName),
                (
                    listener,
                    eventContext) => listener.OnContextDestroyed(eventContext));
        }

        public CopyOnWriteList<ContextStateListener> Listeners { get; } = new CopyOnWriteList<ContextStateListener>();

        private ContextManager GetAssertContextManager(
            string deploymentIdCreateContext,
            string contextName)
        {
            var contextManager = GetContextManager(deploymentIdCreateContext, contextName);
            if (contextManager == null) {
                throw new ArgumentException(
                    "Cannot find context for name '" + contextName + "' deployment-id '" + deploymentIdCreateContext +
                    "'");
            }

            return contextManager;
        }
    }
} // end of namespace