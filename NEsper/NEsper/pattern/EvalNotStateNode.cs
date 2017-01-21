///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// This class contains the state of an 'not' operator in the evaluation state tree.
    /// The not operator inverts the truth of the subexpression under it. It defaults to 
    /// being true rather than being false at startup. True at startup means it will 
    /// generate an event on newState such that parent expressions may turn true. It 
    /// turns permenantly false when it receives an event from a subexpression and the 
    /// subexpression quitted. It indicates the false state via an evaluateFalse call 
    /// on its parent evaluator.
    /// </summary>
    public class EvalNotStateNode : EvalStateNode, Evaluator
    {
        private readonly EvalNotNode _evalNotNode;
        private EvalStateNode _childNode;
    
        /// <summary>Constructor. </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalNotNode">is the factory node associated to the state</param>
        public EvalNotStateNode(Evaluator parentNode, EvalNotNode evalNotNode) 
            : base(parentNode)
        {
            _evalNotNode = evalNotNode;
        }
    
        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            // The not-operator does not pass along the matches
        }

        public override EvalNode FactoryNode
        {
            get { return _evalNotNode; }
        }

        public override void Start(MatchedEventMap beginState)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternNotStart(_evalNotNode, beginState);}
            _childNode = _evalNotNode.ChildNode.NewState(this, null, 0L);
            _childNode.Start(beginState);
    
            // The not node acts by inverting the truth
            // By default the child nodes are false. This not node acts inverts the truth and pretends the child is true,
            // raising an event up.
            this.ParentEvaluator.EvaluateTrue(beginState, this, false);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternNotStart();}
        }
    
        public void EvaluateFalse(EvalStateNode fromNode, bool restartable)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternNotEvalFalse(_evalNotNode);}
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternNotEvalFalse();}
        }
    
        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternNotEvaluateTrue(_evalNotNode, matchEvent);}
            // Only is the subexpression stopped listening can we tell the parent evaluator that this
            // turned permanently false.
            if (isQuitted)
            {
                _childNode = null;
                this.ParentEvaluator.EvaluateFalse(this, true);
            }
            else
            {
                // If the subexpression did not quit, we stay in the "true" state
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternNotEvaluateTrue(isQuitted);}
        }
    
        public override void Quit()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternNotQuit(_evalNotNode);}
            if (_childNode != null)
            {
                _childNode.Quit();
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternNotQuit();}
        }
    
        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitNot(_evalNotNode.FactoryNode, this);
            if (_childNode != null) {
                _childNode.Accept(visitor);
            }
        }

        public override bool IsNotOperator
        {
            get { return true; }
        }

        public override bool IsFilterStateNode
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
            return "EvalNotStateNode child=" + _childNode;
        }
    }
}
