///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    ///     This class is always the root node in the evaluation tree representing an event expression.
    ///     It hold the handle to the EPStatement implementation for notifying when matches are found.
    /// </summary>
    public class EvalRootNode : EvalNode
    {
        internal readonly AgentInstanceContext agentInstanceContext;
        internal readonly EvalNode childNode;
        internal readonly EvalRootFactoryNode factoryNode;

        public EvalRootNode(
            PatternAgentInstanceContext context,
            EvalRootFactoryNode factoryNode,
            EvalNode childNode)
        {
            this.factoryNode = factoryNode;
            this.childNode = childNode;
            agentInstanceContext = context.AgentInstanceContext;
        }

        public EvalNode ChildNode => childNode;

        public EvalRootFactoryNode FactoryNode => factoryNode;

        public AgentInstanceContext AgentInstanceContext => agentInstanceContext;

        public EvalStateNode NewState(Evaluator parentNode)
        {
            return new EvalRootStateNode(this, childNode);
        }

        public EvalRootState Start(
            PatternMatchCallback callback,
            PatternContext context,
            bool isRecoveringResilient)
        {
            MatchedEventMap beginState = new MatchedEventMapImpl(context.MatchedEventMapMeta);
            return StartInternal(callback, context, beginState, isRecoveringResilient);
        }

        public EvalRootState Start(
            PatternMatchCallback callback,
            PatternContext context,
            MatchedEventMap beginState,
            bool isRecoveringResilient)
        {
            return StartInternal(callback, context, beginState, isRecoveringResilient);
        }

        private EvalRootState StartInternal(
            PatternMatchCallback callback,
            PatternContext context,
            MatchedEventMap beginState,
            bool isRecoveringResilient)
        {
            if (beginState == null) {
                throw new ArgumentException("No pattern begin-state has been provided");
            }

            var rootStateNode = NewState(null);
            var rootState = (EvalRootState)rootStateNode;
            rootState.Callback = callback;
            rootState.StartRecoverable(isRecoveringResilient, beginState);
            return rootState;
        }
    }
} // end of namespace