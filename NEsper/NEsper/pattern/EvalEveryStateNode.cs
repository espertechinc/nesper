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
    /// Contains the state collected by an "every" operator. The state includes
    ///  handles to any sub-listeners started by the operator.
    /// </summary>
    public class EvalEveryStateNode : EvalStateNode, Evaluator
    {
        private readonly EvalEveryNode _evalEveryNode;
        private readonly IList<EvalStateNode> _spawnedNodes;
        private MatchedEventMap _beginState;
    
        /// <summary>Constructor. </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalEveryNode">is the factory node associated to the state</param>
        public EvalEveryStateNode(Evaluator parentNode, EvalEveryNode evalEveryNode) 
            : base(parentNode)
        {
            _evalEveryNode = evalEveryNode;
            _spawnedNodes = new List<EvalStateNode>();
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
                PatternConsumptionUtil.ChildNodeRemoveMatches(matchEvent, _spawnedNodes);
            }
        }

        public override EvalNode FactoryNode
        {
            get { return _evalEveryNode; }
        }

        public override void Start(MatchedEventMap beginState)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternEveryStart(_evalEveryNode, beginState);}
            _beginState = beginState.ShallowCopy();
            var childState = _evalEveryNode.ChildNode.NewState(this, null, 0L);
            _spawnedNodes.Add(childState);
    
            // During the start of the child we need to use the temporary evaluator to catch any event created during a start.
            // Events created during the start would likely come from the "not" operator.
            // Quit the new child again if
            var spawnEvaluator = new EvalEveryStateSpawnEvaluator(_evalEveryNode.Context.PatternContext.StatementName);
            childState.ParentEvaluator = spawnEvaluator;
            childState.Start(beginState);
    
            // If the spawned expression turned true already, just quit it
            if (spawnEvaluator.IsEvaluatedTrue)
            {
                childState.Quit();
            }
            else
            {
                childState.ParentEvaluator = this;
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternEveryStart();}
        }
    
        public void EvaluateFalse(EvalStateNode fromNode, bool restartable)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternEveryEvalFalse(_evalEveryNode);}
            fromNode.Quit();
            _spawnedNodes.Remove(fromNode);

            if (!restartable)
            {
                ParentEvaluator.EvaluateFalse(this, false);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternEveryEvalFalse(); }
                return;
            }

            // Spawn all nodes below this EVERY node
            // During the start of a child we need to use the temporary evaluator to catch any event created during a start
            // Such events can be raised when the "not" operator is used.
            var spawnEvaluator = new EvalEveryStateSpawnEvaluator(_evalEveryNode.Context.PatternContext.StatementName);
            var spawned = _evalEveryNode.ChildNode.NewState(spawnEvaluator, null, 0L);
            spawned.Start(_beginState);
    
            // If the whole spawned expression already turned true, quit it again
            if (spawnEvaluator.IsEvaluatedTrue)
            {
                spawned.Quit();
            }
            else
            {
                _spawnedNodes.Add(spawned);
                spawned.ParentEvaluator = this;
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternEveryEvalFalse();}
        }
    
        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternEveryEvaluateTrue(_evalEveryNode, matchEvent);}
            if (isQuitted)
            {
                _spawnedNodes.Remove(fromNode);
            }
    
            // See explanation in EvalFilterStateNode for the type check
            if (fromNode.IsFilterStateNode || fromNode.IsObserverStateNodeNonRestarting)
            {
                // We do not need to newState new listeners here, since the filter state node below this node did not quit
            }
            else
            {
                // Spawn all nodes below this EVERY node
                // During the start of a child we need to use the temporary evaluator to catch any event created during a start
                // Such events can be raised when the "not" operator is used.
                var spawnEvaluator = new EvalEveryStateSpawnEvaluator(_evalEveryNode.Context.PatternContext.StatementName);
                var spawned = _evalEveryNode.ChildNode.NewState(spawnEvaluator, null, 0L);
                spawned.Start(_beginState);
    
                // If the whole spawned expression already turned true, quit it again
                if (spawnEvaluator.IsEvaluatedTrue)
                {
                    spawned.Quit();
                }
                else
                {
                    _spawnedNodes.Add(spawned);
                    spawned.ParentEvaluator = this;
                }
            }
    
            // All nodes indicate to their parents that their child node did not quit, therefore a false for isQuitted
            ParentEvaluator.EvaluateTrue(matchEvent, this, false);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternEveryEvaluateTrue();}
        }
    
        public override void Quit()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternEveryQuit(_evalEveryNode);}
            // Stop all child nodes
            foreach (var child in _spawnedNodes)
            {
                child.Quit();
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternEveryQuit();}
        }
    
        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitEvery(_evalEveryNode.FactoryNode, this,  _beginState);
            foreach (var spawnedNode in _spawnedNodes) {
                spawnedNode.Accept(visitor);
            }
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
            get { return true; }
        }

        public override bool IsObserverStateNodeNonRestarting
        {
            get { return false; }
        }

        public override String ToString()
        {
            return "EvalEveryStateNode spawnedChildren=" + _spawnedNodes.Count;
        }
    }
}
