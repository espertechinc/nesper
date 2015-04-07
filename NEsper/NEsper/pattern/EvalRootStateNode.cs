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
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class is always the root node in the evaluation state tree representing any
    /// activated event expression. It hold the handle to a further state node with subnodes 
    /// making up a whole evaluation state tree.
    /// </summary>
    [Serializable]
    public class EvalRootStateNode : EvalStateNode, Evaluator, PatternStopCallback, EvalRootState
    {
        protected EvalNode RootSingleChildNode;
        private EvalStateNode _topStateNode;
        private PatternMatchCallback _callback;
    
        /// <summary>Constructor. </summary>
        /// <param name="rootSingleChildNode">is the root nodes single child node</param>
        public EvalRootStateNode(EvalNode rootSingleChildNode) 
            : base(null)
        {
            RootSingleChildNode = rootSingleChildNode;
        }

        public override EvalNode FactoryNode
        {
            get { return RootSingleChildNode; }
        }

        /// <summary>Hands the callback to use to indicate matching events. </summary>
        /// <value>is invoked when the event expressions turns true.</value>
        public PatternMatchCallback Callback
        {
            set { _callback = value; }
        }

        public void StartRecoverable(bool startRecoverable, MatchedEventMap beginState) {
            Start(beginState);
        }
    
        public override void Start(MatchedEventMap beginState)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternRootStart(beginState);}
            _topStateNode = RootSingleChildNode.NewState(this, null, 0L);
            _topStateNode.Start(beginState);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternRootStart();}
        }
    
        public void Stop()
        {
            Quit();
        }
    
        public override void Quit()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternRootQuit();}
            if (_topStateNode != null)
            {
                _topStateNode.Quit();
                HandleQuitEvent();
            }
            _topStateNode = null;
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternRootQuit();}
        }
    
        public void HandleQuitEvent() {
            // no action
        }
    
        public void HandleChildQuitEvent() {
            // no action
        }
    
        public void HandleEvaluateFalseEvent() {
            // no action
        }
    
        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternRootEvaluateTrue(matchEvent);}
    
            if (isQuitted)
            {
                _topStateNode = null;
                HandleChildQuitEvent();
            }
    
            _callback.Invoke(matchEvent.MatchingEventsAsMap);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternRootEvaluateTrue(_topStateNode == null);}
        }
    
        public void EvaluateFalse(EvalStateNode fromNode, bool restartable)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternRootEvalFalse();}
            if (_topStateNode != null) {
                _topStateNode.Quit();
                _topStateNode = null;
                HandleEvaluateFalseEvent();
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternRootEvalFalse();}
        }
    
        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitRoot(this);
            if (_topStateNode != null) {
                _topStateNode.Accept(visitor);
            }
        }

        public override bool IsFilterStateNode
        {
            get { return false; }
        }

        public override bool IsNotOperator
        {
            get { return false; }
        }

        public bool IsFilterChildNonQuitting
        {
            get { return false; }
        }

        public override bool IsObserverStateNodeNonRestarting
        {
            get { return false; }
        }

        public override String ToString()
        {
            return "EvalRootStateNode topStateNode=" + _topStateNode;
        }

        public EvalStateNode TopStateNode
        {
            get { return _topStateNode; }
            protected set { _topStateNode = value; }
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            if (_topStateNode != null) {
                _topStateNode.RemoveMatch(matchEvent);
            }
        }
    }
}
