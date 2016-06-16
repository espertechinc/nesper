///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.context;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.core.service
{
    public class EPContextPartitionAdminImpl : EPContextPartitionAdminSPI
    {
        private readonly EPServicesContext _services;
    
        public EPContextPartitionAdminImpl(EPServicesContext services)
        {
            _services = services;
        }

        public bool IsSupportsExtract
        {
            get { return _services.ContextManagerFactoryService.IsSupportsExtract; }
        }

        public String[] GetContextStatementNames(String contextName)
        {
            var contextManager = _services.ContextManagementService.GetContextManager(contextName);
            if (contextManager == null)
            {
                return null;
            }
    
            var statements = new String[contextManager.Statements.Count];
            var count = 0;
            foreach (var entry in contextManager.Statements)
            {
                statements[count++] = entry.Value.Statement.StatementContext.StatementName;
            }
            return statements;
        }
    
        public int GetContextNestingLevel(String contextName)
        {
            var contextManager = CheckedGetContextManager(contextName);
            return contextManager.NumNestingLevels;
        }
    
        public ContextPartitionCollection DestroyContextPartitions(String contextName, ContextPartitionSelector selector)
        {
            var contextManager = CheckedGetContextManager(contextName);
            var descriptor = contextManager.ExtractDestroyPaths(selector);
            return new ContextPartitionCollection(descriptor.ContextPartitionInformation);
        }
    
        public ContextPartitionDescriptor DestroyContextPartition(String contextName, int agentInstanceId)
        {
            var contextManager = CheckedGetContextManager(contextName);
            var descriptor = contextManager.ExtractDestroyPaths(new CPSelectorById(agentInstanceId));
            return descriptor.ContextPartitionInformation.Get(agentInstanceId);
        }
    
        public EPContextPartitionExtract ExtractDestroyPaths(String contextName, ContextPartitionSelector selector)
        {
            var contextManager = CheckedGetContextManager(contextName);
            var descriptor = contextManager.ExtractDestroyPaths(selector);
            return DescriptorToExtract(contextManager.NumNestingLevels, descriptor);
        }
    
        public ContextPartitionCollection StopContextPartitions(String contextName, ContextPartitionSelector selector)
        {
            var contextManager = CheckedGetContextManager(contextName);
            var descriptor = contextManager.ExtractStopPaths(selector);
            return new ContextPartitionCollection(descriptor.ContextPartitionInformation);
        }
    
        public ContextPartitionCollection StartContextPartitions(String contextName, ContextPartitionSelector selector)
        {
            var contextManager = CheckedGetContextManager(contextName);
            return new ContextPartitionCollection(contextManager.StartPaths(selector));
        }
    
        public ContextPartitionCollection GetContextPartitions(String contextName, ContextPartitionSelector selector)
        {
            var contextManager = CheckedGetContextManager(contextName);
            return new ContextPartitionCollection(contextManager.ExtractPaths(selector).ContextPartitionInformation);
        }
    
        public ContextPartitionDescriptor StopContextPartition(String contextName, int agentInstanceId)
        {
            var contextManager = CheckedGetContextManager(contextName);
            var descriptor = contextManager.ExtractStopPaths(new CPSelectorById(agentInstanceId));
            return descriptor.ContextPartitionInformation.Get(agentInstanceId);
        }
    
        public ContextPartitionDescriptor StartContextPartition(String contextName, int agentInstanceId)
        {
            var contextManager = CheckedGetContextManager(contextName);
            var descriptorMap = contextManager.StartPaths(new CPSelectorById(agentInstanceId));
            return descriptorMap.Get(agentInstanceId);
        }
    
        public ContextPartitionDescriptor GetDescriptor(String contextName, int agentInstanceId)
        {
            var contextManager = CheckedGetContextManager(contextName);
            var descriptor = contextManager.ExtractPaths(new CPSelectorById(agentInstanceId));
            return descriptor.ContextPartitionInformation.Get(agentInstanceId);
        }
    
        public EPContextPartitionExtract ExtractStopPaths(String contextName, ContextPartitionSelector selector)
        {
            var contextManager = CheckedGetContextManager(contextName);
            var descriptor = contextManager.ExtractStopPaths(selector);
            return DescriptorToExtract(contextManager.NumNestingLevels, descriptor);
        }
    
        public EPContextPartitionExtract ExtractPaths(String contextName, ContextPartitionSelector selector)
        {
            var contextManager = CheckedGetContextManager(contextName);
            var contextPaths = contextManager.ExtractPaths(selector);
            return DescriptorToExtract(contextManager.NumNestingLevels, contextPaths);
        }

        public ISet<int> GetContextPartitionIds(String contextName, ContextPartitionSelector selector)
        {
            var contextManager = CheckedGetContextManager(contextName);
            return new HashSet<int>(contextManager.GetAgentInstanceIds(selector));
        }
    
        public EPContextPartitionImportResult ImportStartPaths(String contextName, EPContextPartitionImportable importable, AgentInstanceSelector agentInstanceSelector)
        {
            var contextManager = CheckedGetContextManager(contextName);
            var importCallback = new CPImportCallback();
            var state = new ContextControllerState(importable.Paths, true, importCallback);
            contextManager.ImportStartPaths(state, agentInstanceSelector);

            ContextStateCache contextStateCache = contextManager.ContextStateCache;
            foreach (var entry in importable.Paths) {
                entry.Value.State = ContextPartitionState.STARTED;
                contextStateCache.UpdateContextPath(contextName, entry.Key, entry.Value);
            }

            return new EPContextPartitionImportResult(importCallback.ExistingToImported, importCallback.AllocatedToImported);
        }
    
        private ContextManager CheckedGetContextManager(String contextName)
        {
            var contextManager = _services.ContextManagementService.GetContextManager(contextName);
            if (contextManager == null)
            {
                throw new ArgumentException("Context by name '" + contextName + "' could not be found");
            }
            return contextManager;
        }
    
        private EPContextPartitionExtract DescriptorToExtract(int numNestingLevels, ContextStatePathDescriptor contextPaths)
        {
            var importable = new EPContextPartitionImportable(contextPaths.Paths);
            return new EPContextPartitionExtract(new ContextPartitionCollection(contextPaths.ContextPartitionInformation), importable, numNestingLevels);
        }
    
        public class CPImportCallback : ContextPartitionImportCallback
        {
            internal readonly IDictionary<int, int> ExistingToImported = new Dictionary<int, int>();
            internal readonly IDictionary<int, int> AllocatedToImported = new Dictionary<int, int>();
    
            public void Existing(int agentInstanceId, int exportedAgentInstanceId)
            {
                ExistingToImported.Put(agentInstanceId, exportedAgentInstanceId);
            }
    
            public void Allocated(int agentInstanceId, int exportedAgentInstanceId)
            {
                AllocatedToImported.Put(agentInstanceId, exportedAgentInstanceId);
            }
        }
    
        public class CPSelectorById : ContextPartitionSelectorById
        {
            private readonly int _agentInstanceId;
    
            public CPSelectorById(int agentInstanceId)
            {
                _agentInstanceId = agentInstanceId;
            }

            public ICollection<int> ContextPartitionIds
            {
                get { return Collections.SingletonList(_agentInstanceId); }
            }
        }
    }
}
