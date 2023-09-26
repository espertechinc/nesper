///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class EPContextPartitionServiceImpl : EPContextPartitionService
    {
        private readonly EPServicesContext services;

        public EPContextPartitionServiceImpl(EPServicesContext services)
        {
            this.services = services;
        }

        public string[] GetContextStatementNames(
            string deploymentId,
            string contextName)
        {
            var contextManager = CheckedGetContextManager(deploymentId, contextName);
            var statements = new string[contextManager.Statements.Count];
            var count = 0;
            foreach (var entry in contextManager.Statements)
            {
                statements[count++] = entry.Value.Lightweight.StatementContext.StatementName;
            }

            return statements;
        }

        public int GetContextNestingLevel(
            string deploymentId,
            string contextName)
        {
            var contextManager = CheckedGetContextManager(deploymentId, contextName);
            return contextManager.NumNestingLevels;
        }

        public ContextPartitionCollection GetContextPartitions(
            string deploymentId,
            string contextName,
            ContextPartitionSelector selector)
        {
            var contextManager = CheckedGetContextManager(deploymentId, contextName);
            return contextManager.GetContextPartitions(selector);
        }

        public ISet<int> GetContextPartitionIds(
            string deploymentId,
            string contextName,
            ContextPartitionSelector selector)
        {
            var contextManager = CheckedGetContextManager(deploymentId, contextName);
            return contextManager.GetContextPartitionIds(selector);
        }

        public long GetContextPartitionCount(
            string deploymentId,
            string contextName)
        {
            var contextManager = CheckedGetContextManager(deploymentId, contextName);
            return contextManager.ContextPartitionCount;
        }

        public ContextPartitionIdentifier GetIdentifier(
            string deploymentId,
            string contextName,
            int agentInstanceId)
        {
            var contextManager = CheckedGetContextManager(deploymentId, contextName);
            return contextManager.GetContextIdentifier(agentInstanceId);
        }

        public void AddContextStateListener(ContextStateListener listener)
        {
            services.ContextManagementService.Listeners.Add(listener);
        }

        public void RemoveContextStateListener(ContextStateListener listener)
        {
            services.ContextManagementService.Listeners.Remove(listener);
        }

        public IEnumerator<ContextStateListener> ContextStateListeners {
            get { return services.ContextManagementService.Listeners.GetEnumerator(); }
        }

        public void RemoveContextStateListeners()
        {
            services.ContextManagementService.Listeners.Clear();
        }

        public void AddContextPartitionStateListener(
            string deploymentId,
            string contextName,
            ContextPartitionStateListener listener)
        {
            var contextManager = CheckedGetContextManager(deploymentId, contextName);
            contextManager.AddListener(listener);
        }

        public void RemoveContextPartitionStateListener(
            string deploymentId,
            string contextName,
            ContextPartitionStateListener listener)
        {
            var contextManager = CheckedGetContextManager(deploymentId, contextName);
            contextManager.RemoveListener(listener);
        }

        public IEnumerator<ContextPartitionStateListener> GetContextPartitionStateListeners(
            string deploymentId,
            string contextName)
        {
            var contextManager = CheckedGetContextManager(deploymentId, contextName);
            return contextManager.Listeners;
        }

        public void RemoveContextPartitionStateListeners(
            string deploymentId,
            string contextName)
        {
            var contextManager = CheckedGetContextManager(deploymentId, contextName);
            contextManager.RemoveListeners();
        }

        public IDictionary<string, object> GetContextProperties(
            string deploymentId,
            string contextName,
            int contextPartitionId)
        {
            var contextManager = CheckedGetContextManager(deploymentId, contextName);
            return contextManager.GetContextPartitions(contextPartitionId);
        }

        private ContextManager CheckedGetContextManager(
            string deploymentId,
            string contextName)
        {
            ContextManager contextManager = services.ContextManagementService.GetContextManager(deploymentId, contextName);
            if (contextManager == null)
            {
                throw new ArgumentException("Context by name '" + contextName + "' could not be found for deployment-id '" + deploymentId + "'");
            }

            return contextManager;
        }
    }
} // end of namespace