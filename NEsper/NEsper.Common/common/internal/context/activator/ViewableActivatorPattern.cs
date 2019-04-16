///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorPattern : ViewableActivator
    {
        internal bool discardPartialsOnMatch;
        internal EventBeanTypedEventFactory eventBeanTypedEventFactory;
        internal EventType eventType;
        internal bool hasConsumingFilter;
        internal bool isCanIterate;

        internal PatternContext patternContext;
        internal EvalRootFactoryNode rootFactoryNode;
        internal bool suppressSameEventMatches;

        public EvalRootFactoryNode RootFactoryNode {
            set => rootFactoryNode = value;
        }

        public bool HasConsumingFilter {
            set => hasConsumingFilter = value;
        }

        public bool SuppressSameEventMatches {
            set => suppressSameEventMatches = value;
        }

        public bool DiscardPartialsOnMatch {
            set => discardPartialsOnMatch = value;
        }

        public bool CanIterate {
            get => isCanIterate;
            set => isCanIterate = value;
        }

        public EventBeanTypedEventFactory EventBeanTypedEventFactory {
            get => eventBeanTypedEventFactory;
            set => eventBeanTypedEventFactory = value;
        }

        public PatternContext PatternContext {
            get => patternContext;
            set => patternContext = value;
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public bool IsConsumingFilter => hasConsumingFilter;

        public bool IsSuppressSameEventMatches => suppressSameEventMatches;

        public bool IsDiscardPartialsOnMatch => discardPartialsOnMatch;

        public bool IsCanIterate => isCanIterate;

        public ViewableActivationResult Activate(
            AgentInstanceContext agentInstanceContext,
            bool isSubselect,
            bool isRecoveringResilient)
        {
            var patternAgentInstanceContext = new PatternAgentInstanceContext(
                patternContext, agentInstanceContext, hasConsumingFilter, null);
            var rootNode = EvalNodeUtil.MakeRootNodeFromFactory(rootFactoryNode, patternAgentInstanceContext);

            EventStream sourceEventStream = isCanIterate
                ? (EventStream) new ZeroDepthStreamIterable(eventType)
                : (EventStream) new ZeroDepthStreamNoIterate(eventType);

            // we set a child now in case the start itself indicates results
            sourceEventStream.Child = ViewNoop.INSTANCE;

            PatternMatchCallback callback = new ProxyPatternMatchCallback() {
                ProcMatchFound = (
                    matchEvent,
                    optionalTriggeringEvent) => {
                    EventBean compositeEvent = eventBeanTypedEventFactory.AdapterForTypedMap(matchEvent, eventType);
                    sourceEventStream.Insert(compositeEvent);
                }
            };

            EvalRootState rootState = rootNode.Start(callback, patternContext, isRecoveringResilient);


            return new ViewableActivationResult(
                sourceEventStream,
                new ProxyAgentInstanceStopCallback(services => rootState.Stop()),
                rootState, suppressSameEventMatches,
                discardPartialsOnMatch, rootState, null);
        }
    }
} // end of namespace