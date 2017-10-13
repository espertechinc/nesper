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
using com.espertech.esper.pattern.guard;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents the state of a "within" operator in the evaluation state tree.
    /// The within operator applies to a subexpression and is thus expected to only have one 
    /// child node.
    /// </summary>
    public class EvalGuardStateNode : EvalStateNode, Evaluator, Quitable
    {
        private readonly EvalGuardNode _evalGuardNode;
        private EvalStateNode _activeChildNode;
        private Guard _guard;
        private MatchedEventMap _beginState;

        /// <summary>Constructor. </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalGuardNode">is the factory node associated to the state</param>
        public EvalGuardStateNode(Evaluator parentNode, EvalGuardNode evalGuardNode)
            : base(parentNode)
        {
            _evalGuardNode = evalGuardNode;
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            if (PatternConsumptionUtil.ContainsEvent(matchEvent, _beginState))
            {
                Quit();
                ParentEvaluator.EvaluateFalse(this, true);
            }
            else
            {
                if (_activeChildNode != null)
                {
                    _activeChildNode.RemoveMatch(matchEvent);
                }
            }
        }

        public override EvalNode FactoryNode
        {
            get { return _evalGuardNode; }
        }

        public PatternAgentInstanceContext Context
        {
            get { return _evalGuardNode.Context; }
        }

        public override void Start(MatchedEventMap beginState)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternGuardStart(_evalGuardNode, beginState); }
            _beginState = beginState;
            _guard = _evalGuardNode.FactoryNode.GuardFactory.MakeGuard(_evalGuardNode.Context, beginState, this, null, null);
            _activeChildNode = _evalGuardNode.ChildNode.NewState(this, null, 0L);

            // Start the single child state
            _activeChildNode.Start(beginState);

            // Start the guard
            _guard.StartGuard();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternGuardStart(); }
        }

        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternGuardEvaluateTrue(_evalGuardNode, matchEvent); }
            bool haveQuitted = _activeChildNode == null;

            // If one of the children quits, remove the child
            if (isQuitted)
            {
                _activeChildNode = null;

                // Stop guard, since associated subexpression is gone
                _guard.StopGuard();
            }

            if (!(haveQuitted))
            {
                bool guardPass = _guard.Inspect(matchEvent);
                if (guardPass)
                {
                    ParentEvaluator.EvaluateTrue(matchEvent, this, isQuitted);
                }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternGuardEvaluateTrue(isQuitted); }
        }

        public void EvaluateFalse(EvalStateNode fromNode, bool restartable)
        {
            _activeChildNode = null;
            ParentEvaluator.EvaluateFalse(this, true);
        }

        public override void Quit()
        {
            if (_activeChildNode == null)
            {
                return;
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternGuardQuit(_evalGuardNode); }
            if (_activeChildNode != null)
            {
                _activeChildNode.Quit();
                _guard.StopGuard();
            }

            _activeChildNode = null;
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternGuardQuit(); }
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitGuard(_evalGuardNode.FactoryNode, this, _guard);
            if (_activeChildNode != null)
            {
                _activeChildNode.Accept(visitor);
            }
        }

        public override String ToString()
        {
            return "EvaluationWitinStateNode activeChildNode=" + _activeChildNode +
                     " guard=" + _guard;
        }

        public override bool IsNotOperator
        {
            get { return false; }
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

        public void GuardQuit()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternGuardGuardQuit(_evalGuardNode); }
            // It is possible that the child node has already been quit such as when the parent wait time was shorter.
            // 1. parent node's guard indicates quit to all children
            // 2. this node's guards also indicates quit, however that already occured
            if (_activeChildNode != null)
            {
                _activeChildNode.Quit();
            }
            _activeChildNode = null;

            // Indicate to parent state that this is permanently false.
            ParentEvaluator.EvaluateFalse(this, true);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternGuardGuardQuit(); }
        }
    }
}
