///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.pattern;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service.resource
{
    public class StatementResourceService
    {
        public StatementResourceHolder ResourcesZero { get; private set; }

        public IDictionary<int, StatementResourceHolder> ResourcesNonZero { get; private set; }

        public IDictionary<ContextStatePathKey, EvalRootState> ContextEndEndpoints { get; private set; }

        public IDictionary<ContextStatePathKey, EvalRootState> ContextStartEndpoints { get; private set; }

        public void StartContextPattern(EvalRootState patternStopCallback, bool startEndpoint, ContextStatePathKey path)
        {
            AddContextPattern(patternStopCallback, startEndpoint, path);
        }

        public void StopContextPattern(bool startEndpoint, ContextStatePathKey path)
        {
            RemoveContextPattern(startEndpoint, path);
        }

        public void StartContextPartition(StatementAgentInstanceFactoryResult startResult, int agentInstanceId)
        {
            IReaderWriterLock iLock =
                startResult.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementAgentInstanceLock;
            StatementResourceHolder recoveryResources = null;

            if (startResult is StatementAgentInstanceFactorySelectResult)
            {
                StatementAgentInstanceFactorySelectResult selectResult =
                    (StatementAgentInstanceFactorySelectResult) startResult;
                recoveryResources = new StatementResourceHolder(
                    iLock,
                    selectResult.TopViews,
                    selectResult.EventStreamViewables,
                    selectResult.PatternRoots,
                    selectResult.OptionalAggegationService,
                    selectResult.SubselectStrategies,
                    selectResult.OptionalPostLoadJoin);
            }

            if (startResult is StatementAgentInstanceFactoryCreateWindowResult)
            {
                StatementAgentInstanceFactoryCreateWindowResult createResult =
                    (StatementAgentInstanceFactoryCreateWindowResult) startResult;
                recoveryResources = new StatementResourceHolder(
                    iLock,
                    new Viewable[] { createResult.TopView },
                    null,
                    null,
                    null,
                    null,
                    createResult.PostLoad);
            }

            if (startResult is StatementAgentInstanceFactoryCreateTableResult)
            {
                StatementAgentInstanceFactoryCreateTableResult createResult = (StatementAgentInstanceFactoryCreateTableResult) startResult;
                recoveryResources = new StatementResourceHolder(
                    iLock,
                    new Viewable[] { createResult.FinalView },
                    null,
                    null, 
                    createResult.OptionalAggegationService,
                    null, 
                    null);
            }

            if (startResult is StatementAgentInstanceFactoryOnTriggerResult)
            {
                var onTriggerResult = (StatementAgentInstanceFactoryOnTriggerResult) startResult;
                recoveryResources = new StatementResourceHolder(
                    iLock,
                    null, 
                    null, 
                    new EvalRootState[] { onTriggerResult.OptPatternRoot }, 
                    onTriggerResult.OptionalAggegationService, 
                    onTriggerResult.SubselectStrategies, 
                    null);
            }

            if (recoveryResources != null)
            {
                AddRecoveryResources(agentInstanceId, recoveryResources);
            }
        }

        public void EndContextPartition(int agentInstanceId)
        {
            RemoveRecoveryResources(agentInstanceId);
        }

        private void AddRecoveryResources(int agentInstanceId, StatementResourceHolder recoveryResources)
        {
            if (agentInstanceId == 0)
            {
                ResourcesZero = recoveryResources;
            }
            else
            {
                if (ResourcesNonZero == null)
                {
                    ResourcesNonZero = new SortedDictionary<int, StatementResourceHolder>();
                }
                ResourcesNonZero.Put(agentInstanceId, recoveryResources);
            }
        }

        private void RemoveRecoveryResources(int agentInstanceId)
        {
            if (agentInstanceId == 0)
            {
                ResourcesZero = null;
            }
            else if (ResourcesNonZero != null)
            {
                ResourcesNonZero.Remove(agentInstanceId);
            }
        }

        private void RemoveContextPattern(bool startEndpoint, ContextStatePathKey path)
        {
            if (startEndpoint)
            {
                if (ContextStartEndpoints != null)
                {
                    ContextStartEndpoints.Remove(path);
                }
            }
            else
            {
                if (ContextEndEndpoints != null)
                {
                    ContextEndEndpoints.Remove(path);
                }
            }
        }

        private void AddContextPattern(EvalRootState rootState, bool startEndpoint, ContextStatePathKey path)
        {
            if (startEndpoint)
            {
                if (ContextStartEndpoints == null)
                {
                    ContextStartEndpoints = new Dictionary<ContextStatePathKey, EvalRootState>();
                }
                ContextStartEndpoints.Put(path, rootState);
            }
            else
            {
                if (ContextEndEndpoints == null)
                {
                    ContextEndEndpoints = new Dictionary<ContextStatePathKey, EvalRootState>();
                }
                ContextEndEndpoints.Put(path, rootState);
            }
        }
    }
}