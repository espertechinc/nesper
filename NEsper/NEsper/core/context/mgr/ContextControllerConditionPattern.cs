///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerConditionPattern
        : ContextControllerCondition
    {
        private readonly EPServicesContext _servicesContext;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly ContextDetailConditionPattern _endpointPatternSpec;
        private readonly ContextControllerConditionCallback _callback;
        private readonly ContextInternalFilterAddendum _filterAddendum;
        private readonly bool _isStartEndpoint;
        private readonly ContextStatePathKey _contextStatePathKey;
    
        protected EvalRootState PatternStopCallback;
    
        public ContextControllerConditionPattern(EPServicesContext servicesContext, AgentInstanceContext agentInstanceContext, ContextDetailConditionPattern endpointPatternSpec, ContextControllerConditionCallback callback, ContextInternalFilterAddendum filterAddendum, bool startEndpoint, ContextStatePathKey contextStatePathKey)
        {
            _servicesContext = servicesContext;
            _agentInstanceContext = agentInstanceContext;
            _endpointPatternSpec = endpointPatternSpec;
            _callback = callback;
            _filterAddendum = filterAddendum;
            _isStartEndpoint = startEndpoint;
            _contextStatePathKey = contextStatePathKey;
        }
    
        public void Activate(EventBean optionalTriggeringEvent, MatchedEventMap priorMatches, long timeOffset, bool isRecoveringReslient) {
            if (PatternStopCallback != null) {
                PatternStopCallback.Stop();
            }
    
            PatternStreamSpecCompiled patternStreamSpec = _endpointPatternSpec.PatternCompiled;
            StatementContext stmtContext = _agentInstanceContext.StatementContext;
    
            EvalRootFactoryNode rootFactoryNode = _servicesContext.PatternNodeFactory.MakeRootNode();
            int streamNum = _isStartEndpoint ? _contextStatePathKey.SubPath : -1 * _contextStatePathKey.SubPath;
            bool allowResilient = _contextStatePathKey.Level == 1;
            rootFactoryNode.AddChildNode(patternStreamSpec.EvalFactoryNode);
            PatternContext patternContext = stmtContext.PatternContextFactory.CreateContext(stmtContext, streamNum, rootFactoryNode, new MatchedEventMapMeta(patternStreamSpec.AllTags, !patternStreamSpec.ArrayEventTypes.IsEmpty()), allowResilient);
    
            PatternAgentInstanceContext patternAgentInstanceContext = stmtContext.PatternContextFactory.CreatePatternAgentContext(patternContext, _agentInstanceContext, false);
            EvalRootNode rootNode = EvalNodeUtil.MakeRootNodeFromFactory(rootFactoryNode, patternAgentInstanceContext);
    
            if (priorMatches == null) {
                priorMatches = new MatchedEventMapImpl(patternContext.MatchedEventMapMeta);
            }
    
            // capture any callbacks that may occur right after start
            ConditionPatternMatchCallback callback = new ConditionPatternMatchCallback(this);
            PatternStopCallback = rootNode.Start(callback.MatchFound, patternContext, priorMatches, isRecoveringReslient);
            callback.ForwardCalls = true;
    
            if (_agentInstanceContext.StatementContext.ExtensionServicesContext != null && _agentInstanceContext.StatementContext.ExtensionServicesContext.StmtResources != null) {
                _agentInstanceContext.StatementContext.ExtensionServicesContext.StmtResources.StartContextPattern(PatternStopCallback, _isStartEndpoint, _contextStatePathKey);
            }
    
            if (callback.IsInvoked) {
                MatchFound(Collections.GetEmptyMap<String, Object>());
            }
        }
    
        public void MatchFound(IDictionary<String, Object> matchEvent) {
            IDictionary<String, Object> matchEventInclusive = null;
            if (_endpointPatternSpec.IsInclusive) {
                if (matchEvent.Count < 2) {
                    matchEventInclusive = matchEvent;
                }
                else {
                    // need to reorder according to tag order
                    var ordered = new LinkedHashMap<String, Object>();
                    foreach (String key in _endpointPatternSpec.PatternCompiled.TaggedEventTypes.Keys) {
                        ordered.Put(key, matchEvent.Get(key));
                    }
                    foreach (String key in _endpointPatternSpec.PatternCompiled.ArrayEventTypes.Keys) {
                        ordered.Put(key, matchEvent.Get(key));
                    }
                    matchEventInclusive = ordered;
                }
            }
            _callback.RangeNotification(matchEvent, this, null, matchEventInclusive, _filterAddendum);
        }
    
        public void Deactivate() {
            if (PatternStopCallback != null) {
                PatternStopCallback.Stop();
                PatternStopCallback = null;
                if (_agentInstanceContext.StatementContext.ExtensionServicesContext != null && _agentInstanceContext.StatementContext.ExtensionServicesContext.StmtResources != null) {
                    _agentInstanceContext.StatementContext.ExtensionServicesContext.StmtResources.StopContextPattern(_isStartEndpoint, _contextStatePathKey);
                }
            }
        }

        public bool IsRunning
        {
            get { return PatternStopCallback != null; }
        }

        public long? ExpectedEndTime
        {
            get { return null; }
        }

        public bool IsImmediate
        {
            get { return _endpointPatternSpec.IsImmediate; }
        }

        public class ConditionPatternMatchCallback
        {
            private readonly ContextControllerConditionPattern _condition;

            private bool _isInvoked;
            internal bool ForwardCalls;

            public ConditionPatternMatchCallback(ContextControllerConditionPattern condition)
            {
                _condition = condition;
            }

            public void MatchFound(IDictionary<String, Object> matchEvent)
            {
                _isInvoked = true;
                if (ForwardCalls)
                {
                    _condition.MatchFound(matchEvent);
                }
            }

            public bool IsInvoked
            {
                get { return _isInvoked; }
            }
        }
    }
}
