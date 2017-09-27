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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Contains the state collected by an "every" operator. The state includes handles
    /// to any sub-listeners started by the operator.
    /// </summary>
    public class EvalEveryDistinctStateNode : EvalStateNode, Evaluator
    {
        protected readonly EvalEveryDistinctNode EveryDistinctNode;
        protected readonly IDictionary<EvalStateNode, ISet<Object>> SpawnedNodes;
        protected MatchedEventMap BeginState;

        /// <summary>Constructor. </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="everyDistinctNode">is the factory node associated to the state</param>
        public EvalEveryDistinctStateNode(Evaluator parentNode, EvalEveryDistinctNode everyDistinctNode)
            : base(parentNode)
        {
            EveryDistinctNode = everyDistinctNode;
            SpawnedNodes = new LinkedHashMap<EvalStateNode, ISet<Object>>();
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            if (PatternConsumptionUtil.ContainsEvent(matchEvent, BeginState))
            {
                Quit();
                ParentEvaluator.EvaluateFalse(this, true);
            }
            else
            {
                PatternConsumptionUtil.ChildNodeRemoveMatches(matchEvent, SpawnedNodes.Keys);
            }
        }

        public override EvalNode FactoryNode
        {
            get { return EveryDistinctNode; }
        }

        public override void Start(MatchedEventMap beginState)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternEveryDistinctStart(EveryDistinctNode, beginState); }

            BeginState = beginState.ShallowCopy();
            var childState = EveryDistinctNode.ChildNode.NewState(this, null, 0L);
            SpawnedNodes.Put(childState, new HashSet<Object>());

            if (SpawnedNodes.Count != 1)
            {
                throw new IllegalStateException("EVERY state node is expected to have single child state node");
            }

            // During the start of the child we need to use the temporary evaluator to catch any event created during a start.
            // Events created during the start would likely come from the "not" operator.
            // Quit the new child again if
            var spawnEvaluator = new EvalEveryStateSpawnEvaluator(EveryDistinctNode.Context.PatternContext.StatementName);
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
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternEveryDistinctStart(); }
        }

        public void EvaluateFalse(EvalStateNode fromNode, bool restartable)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternEveryDistinctEvalFalse(EveryDistinctNode); }
            fromNode.Quit();
            SpawnedNodes.Remove(fromNode);

            // Spawn all nodes below this EVERY node
            // During the start of a child we need to use the temporary evaluator to catch any event created during a start
            // Such events can be raised when the "not" operator is used.
            var spawnEvaluator = new EvalEveryStateSpawnEvaluator(EveryDistinctNode.Context.PatternContext.StatementName);
            var spawned = EveryDistinctNode.ChildNode.NewState(spawnEvaluator, null, 0L);
            spawned.Start(BeginState);

            // If the whole spawned expression already turned true, quit it again
            if (spawnEvaluator.IsEvaluatedTrue)
            {
                spawned.Quit();
            }
            else
            {
                SpawnedNodes.Put(spawned, new HashSet<Object>());
                spawned.ParentEvaluator = this;
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternEveryDistinctEvalFalse(); }
        }

        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternEveryDistinctEvaluateTrue(EveryDistinctNode, matchEvent); }

            // determine if this evaluation has been seen before from the same node
            var matchEventKey = PatternExpressionUtil.GetKeys(matchEvent, EveryDistinctNode.FactoryNode.Convertor, EveryDistinctNode.FactoryNode.DistinctExpressionsArray, EveryDistinctNode.Context.AgentInstanceContext);
            var haveSeenThis = false;
            var keysFromNode = SpawnedNodes.Get(fromNode);
            if (keysFromNode != null)
            {
                if (keysFromNode.Contains(matchEventKey))
                {
                    haveSeenThis = true;
                }
                else
                {
                    keysFromNode.Add(matchEventKey);
                }
            }

            if (isQuitted)
            {
                SpawnedNodes.Remove(fromNode);
            }

            // See explanation in EvalFilterStateNode for the type check
            if (fromNode.IsFilterStateNode)
            {
                // We do not need to newState new listeners here, since the filter state node below this node did not quit
            }
            else
            {
                // Spawn all nodes below this EVERY node
                // During the start of a child we need to use the temporary evaluator to catch any event created during a start
                // Such events can be raised when the "not" operator is used.
                var spawnEvaluator = new EvalEveryStateSpawnEvaluator(EveryDistinctNode.Context.PatternContext.StatementName);
                var spawned = EveryDistinctNode.ChildNode.NewState(spawnEvaluator, null, 0L);
                spawned.Start(BeginState);

                // If the whole spawned expression already turned true, quit it again
                if (spawnEvaluator.IsEvaluatedTrue)
                {
                    spawned.Quit();
                }
                else
                {
                    var keyset = new HashSet<Object>();
                    if (keysFromNode != null)
                    {
                        keyset.AddAll(keysFromNode);
                    }
                    SpawnedNodes.Put(spawned, keyset);
                    spawned.ParentEvaluator = this;
                }
            }

            if (!haveSeenThis)
            {
                ParentEvaluator.EvaluateTrue(matchEvent, this, false);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternEveryDistinctEvaluateTrue(keysFromNode, null, matchEventKey, haveSeenThis); }
        }

        public override void Quit()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternEveryDistinctQuit(EveryDistinctNode); }
            // Stop all child nodes
            foreach (var child in SpawnedNodes.Keys)
            {
                child.Quit();
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternEveryDistinctQuit(); }
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitEveryDistinct(EveryDistinctNode.FactoryNode, this, BeginState, SpawnedNodes.Values);
            foreach (var spawnedNode in SpawnedNodes.Keys)
            {
                spawnedNode.Accept(visitor);
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
            get { return true; }
        }

        public override bool IsObserverStateNodeNonRestarting
        {
            get { return false; }
        }

        public override String ToString()
        {
            return "EvalEveryStateNode spawnedChildren=" + SpawnedNodes.Count;
        }
    }
}
