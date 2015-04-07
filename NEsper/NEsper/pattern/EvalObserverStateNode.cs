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
using com.espertech.esper.pattern.observer;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents the state of an eventObserver sub-expression in the evaluation state tree.
    /// </summary>
    public class EvalObserverStateNode : EvalStateNode, ObserverEventEvaluator
    {
        private readonly EvalObserverNode _evalObserverNode;
        private EventObserver _eventObserver;
    
        /// <summary>Constructor. </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalObserverNode">is the factory node associated to the state</param>
        public EvalObserverStateNode(Evaluator parentNode, EvalObserverNode evalObserverNode)
            : base(parentNode)
        {
    
            _evalObserverNode = evalObserverNode;
        }
    
        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            if (PatternConsumptionUtil.ContainsEvent(matchEvent, _eventObserver.BeginState)) {
                Quit();
                ParentEvaluator.EvaluateFalse(this, true);
            }
        }

        public override EvalNode FactoryNode
        {
            get { return _evalObserverNode; }
        }

        public virtual PatternAgentInstanceContext Context
        {
            get { return _evalObserverNode.Context; }
        }

        public void ObserverEvaluateTrue(MatchedEventMap matchEvent, bool quitted)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternObserverEvaluateTrue(_evalObserverNode, matchEvent);}
            ParentEvaluator.EvaluateTrue(matchEvent, this, quitted);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternObserverEvaluateTrue();}
        }
    
        public void ObserverEvaluateFalse(bool restartable)
        {
            ParentEvaluator.EvaluateFalse(this, restartable);
        }
    
        public override void Start(MatchedEventMap beginState)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternObserverStart(_evalObserverNode, beginState);}
            _eventObserver = _evalObserverNode.FactoryNode.ObserverFactory.MakeObserver(_evalObserverNode.Context, beginState, this, null, null, ParentEvaluator.IsFilterChildNonQuitting);
            _eventObserver.StartObserve();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternObserverStart();}
        }
    
        public override void Quit()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternObserverQuit(_evalObserverNode);}
            _eventObserver.StopObserve();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternObserverQuit();}
        }
    
        public override void Accept(EvalStateNodeVisitor visitor) {
            visitor.VisitObserver(_evalObserverNode.FactoryNode, this, _eventObserver);
        }

        public override bool IsNotOperator
        {
            get { return false; }
        }

        public override bool IsFilterStateNode
        {
            get { return false; }
        }

        public override bool IsObserverStateNodeNonRestarting
        {
            get { return _evalObserverNode.FactoryNode.IsObserverStateNodeNonRestarting; }
        }

        public override String ToString()
        {
            return "EvalObserverStateNode eventObserver=" + _eventObserver;
        }
    }
}
