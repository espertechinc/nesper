///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class is always the root node in the evaluation tree representing an event 
    /// expression. It hold the handle to the EPStatement implementation for notifying 
    /// when matches are found.
    /// </summary>
    public class EvalRootNode 
        : EvalNodeBase
        , PatternStarter
    {
        private readonly EvalRootFactoryNode _factoryNode;
        private readonly EvalNode _childNode;
    
        public EvalRootNode(PatternAgentInstanceContext context, EvalRootFactoryNode factoryNode, EvalNode childNode)
                    : base(context)
        {
            _factoryNode = factoryNode;
            _childNode = childNode;
        }

        public EvalNode ChildNode
        {
            get { return _childNode; }
        }

        public EvalRootFactoryNode FactoryNode
        {
            get { return _factoryNode; }
        }

        public PatternStopCallback Start(PatternMatchCallback callback, PatternContext context, bool isRecoveringResilient)
        {
            MatchedEventMap beginState = new MatchedEventMapImpl(context.MatchedEventMapMeta);
            return StartInternal(callback, context, beginState, isRecoveringResilient);
        }

        public EvalRootState Start(PatternMatchCallback callback,
                                   PatternContext context,
                                   MatchedEventMap beginState,
                                   bool isRecoveringResilient)
        {
            return StartInternal(callback, context, beginState, isRecoveringResilient);
        }
    
        protected EvalRootState StartInternal(PatternMatchCallback callback,
                                               PatternContext context,
                                               MatchedEventMap beginState,
                                               bool isRecoveringResilient)
        {
            if (beginState == null) {
                throw new ArgumentException("No pattern begin-state has been provided");
            }
            var rootStateNode = NewState(null, null, 0L);
            var rootState = (EvalRootState) rootStateNode;
            rootState.Callback = callback;
            rootState.StartRecoverable(isRecoveringResilient, beginState);
            return rootState;
        }
    
        public override EvalStateNode NewState(Evaluator parentNode, EvalStateNodeNumber stateNodeNumber, long stateNodeId)
        {
            return new EvalRootStateNode(_childNode);
        }
    }
}
