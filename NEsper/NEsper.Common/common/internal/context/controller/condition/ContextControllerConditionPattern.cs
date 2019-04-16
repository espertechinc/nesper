///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.condition
{
    public class ContextControllerConditionPattern : ContextControllerConditionNonHA,
        PatternMatchCallback
    {
        private readonly ContextControllerConditionCallback callback;

        private readonly IntSeqKey conditionPath;
        private readonly ContextController controller;
        private readonly object[] partitionKeys;
        private readonly ContextConditionDescriptorPattern pattern;

        protected EvalRootState patternStopCallback;

        public ContextControllerConditionPattern(
            IntSeqKey conditionPath,
            object[] partitionKeys,
            ContextConditionDescriptorPattern pattern,
            ContextControllerConditionCallback callback,
            ContextController controller)
        {
            this.conditionPath = conditionPath;
            this.partitionKeys = partitionKeys;
            this.pattern = pattern;
            this.callback = callback;
            this.controller = controller;
        }

        public ContextConditionDescriptor Descriptor => pattern;

        public bool Activate(
            EventBean optionalTriggeringEvent,
            ContextControllerEndConditionMatchEventProvider endConditionMatchEventProvider)
        {
            if (patternStopCallback != null) {
                patternStopCallback.Stop();
            }

            var agentInstanceContext = controller.Realization.AgentInstanceContextCreate;
            Func<FilterSpecActivatable, FilterValueSetParam[][]> contextAddendumFunction = filter =>
                ContextManagerUtil.ComputeAddendumNonStmt(partitionKeys, filter, controller.Realization);
            var patternAgentInstanceContext = new PatternAgentInstanceContext(
                pattern.PatternContext, agentInstanceContext, false, contextAddendumFunction);
            var rootNode = EvalNodeUtil.MakeRootNodeFromFactory(pattern.Pattern, patternAgentInstanceContext);

            var matchedEventMap = new MatchedEventMapImpl(pattern.PatternContext.MatchedEventMapMeta);
            if (optionalTriggeringEvent != null && endConditionMatchEventProvider != null) {
                endConditionMatchEventProvider.PopulateEndConditionFromTrigger(
                    matchedEventMap, optionalTriggeringEvent);
            }

            // capture any callbacks that may occur right after start
            var callback = new ConditionPatternMatchCallback(this);
            patternStopCallback = rootNode.Start(callback, pattern.PatternContext, matchedEventMap, false);
            callback.forwardCalls = true;

            if (callback.IsInvoked) {
                MatchFound(Collections.GetEmptyMap<string, object>(), optionalTriggeringEvent);
            }

            return false;
        }

        public void Deactivate()
        {
            if (patternStopCallback == null) {
                return;
            }

            patternStopCallback.Stop();
            patternStopCallback = null;
        }

        public bool IsImmediate => pattern.IsImmediate;

        public bool IsRunning => patternStopCallback != null;

        public long? ExpectedEndTime => null;

        public void MatchFound(
            IDictionary<string, object> matchEvent,
            EventBean optionalTriggeringEvent)
        {
            IDictionary<string, object> matchEventInclusive = null;
            if (pattern.IsInclusive) {
                if (matchEvent.Count < 2) {
                    matchEventInclusive = matchEvent;
                }
                else {
                    // need to reorder according to tag order
                    var ordered = new LinkedHashMap<string, object>();
                    foreach (string key in pattern.TaggedEvents) {
                        ordered.Put(key, matchEvent.Get(key));
                    }

                    foreach (string key in pattern.ArrayEvents) {
                        ordered.Put(key, matchEvent.Get(key));
                    }

                    matchEventInclusive = ordered;
                }
            }

            callback.RangeNotification(
                conditionPath, this, null, matchEvent, optionalTriggeringEvent, matchEventInclusive);
        }

        public class ConditionPatternMatchCallback : PatternMatchCallback
        {
            internal readonly ContextControllerConditionPattern condition;
            internal bool forwardCalls;

            public ConditionPatternMatchCallback(ContextControllerConditionPattern condition)
            {
                this.condition = condition;
            }

            public bool IsInvoked { get; private set; }

            public void MatchFound(
                IDictionary<string, object> matchEvent,
                EventBean optionalTriggeringEvent)
            {
                IsInvoked = true;
                if (forwardCalls) {
                    condition.MatchFound(matchEvent, optionalTriggeringEvent);
                }
            }
        }
    }
} // end of namespace