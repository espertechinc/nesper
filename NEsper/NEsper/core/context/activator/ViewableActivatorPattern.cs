///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.pattern;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.activator
{
    public class ViewableActivatorPattern : ViewableActivator
    {
        private readonly PatternContext _patternContext;
        private readonly EvalRootFactoryNode _rootFactoryNode;
        private readonly EventType _eventType;
        private readonly bool _hasConsumingFilter;
        private readonly bool _suppressSameEventMatches;
        private readonly bool _discardPartialsOnMatch;
        private readonly bool _isCanIterate;

        internal ViewableActivatorPattern(
            PatternContext patternContext,
            EvalRootFactoryNode rootFactoryNode,
            EventType eventType,
            bool hasConsumingFilter,
            bool suppressSameEventMatches,
            bool discardPartialsOnMatch,
            bool isCanIterate)
        {
            _patternContext = patternContext;
            _rootFactoryNode = rootFactoryNode;
            _eventType = eventType;
            _hasConsumingFilter = hasConsumingFilter;
            _suppressSameEventMatches = suppressSameEventMatches;
            _discardPartialsOnMatch = discardPartialsOnMatch;
            _isCanIterate = isCanIterate;
        }

        public ViewableActivationResult Activate(
            AgentInstanceContext agentInstanceContext,
            bool isSubselect,
            bool isRecoveringResilient)
        {
            PatternAgentInstanceContext patternAgentInstanceContext =
                agentInstanceContext.StatementContext.PatternContextFactory.CreatePatternAgentContext(
                    _patternContext, agentInstanceContext, _hasConsumingFilter);
            EvalRootNode rootNode = EvalNodeUtil.MakeRootNodeFromFactory(_rootFactoryNode, patternAgentInstanceContext);

            EventStream sourceEventStream = _isCanIterate ?
                (EventStream) new ZeroDepthStreamIterable(_eventType) :
                (EventStream) new ZeroDepthStreamNoIterate(_eventType);
            StatementContext statementContext = _patternContext.StatementContext;
            PatternMatchCallback callback = matchEvent =>
            {
                EventBean compositeEvent = statementContext.EventAdapterService.AdapterForTypedMap(
                    matchEvent, _eventType);
                sourceEventStream.Insert(compositeEvent);
            };

            var rootState = (EvalRootState) rootNode.Start(callback, _patternContext, isRecoveringResilient);
            return new ViewableActivationResult(
                sourceEventStream, rootState.Stop, null, rootState, rootState, _suppressSameEventMatches, _discardPartialsOnMatch, null);
        }

        public EvalRootFactoryNode RootFactoryNode
        {
            get { return _rootFactoryNode; }
        }

        public PatternContext PatternContext
        {
            get { return _patternContext; }
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool HasConsumingFilter
        {
            get { return _hasConsumingFilter; }
        }

        public bool IsSuppressSameEventMatches
        {
            get { return _suppressSameEventMatches; }
        }

        public bool IsDiscardPartialsOnMatch
        {
            get { return _discardPartialsOnMatch; }
        }

        public bool IsCanIterate
        {
            get { return _isCanIterate; }
        }
    }
}
