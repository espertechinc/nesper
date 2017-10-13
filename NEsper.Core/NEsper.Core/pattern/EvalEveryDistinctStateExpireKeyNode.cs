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
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Contains the state collected by an "every" operator. The state includes handles 
    /// to any sub-listeners started by the operator.
    /// </summary>
    public class EvalEveryDistinctStateExpireKeyNode : EvalStateNode, Evaluator
    {
        protected readonly EvalEveryDistinctNode EveryNode;
        protected readonly IDictionary<EvalStateNode, IDictionary<Object, long>> SpawnedNodes;
        protected MatchedEventMap BeginState;

        /// <summary>Constructor. </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="everyNode">is the factory node associated to the state</param>
        public EvalEveryDistinctStateExpireKeyNode(Evaluator parentNode, EvalEveryDistinctNode everyNode)
            : base(parentNode)
        {
            EveryNode = everyNode;
            SpawnedNodes = new LinkedHashMap<EvalStateNode, IDictionary<Object, long>>();
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
            get { return EveryNode; }
        }

        public override void Start(MatchedEventMap beginState)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternEveryDistinctStart(EveryNode, beginState); }

            BeginState = beginState.ShallowCopy();
            var childState = EveryNode.ChildNode.NewState(this, null, 0L);
            SpawnedNodes.Put(childState, new LinkedHashMap<Object, long>());

            // During the start of the child we need to use the temporary evaluator to catch any event created during a start.
            // Events created during the start would likely come from the "not" operator.
            // Quit the new child again if
            var spawnEvaluator = new EvalEveryStateSpawnEvaluator(EveryNode.Context.PatternContext.StatementName);
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
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternEveryDistinctEvalFalse(EveryNode); }

            fromNode.Quit();
            SpawnedNodes.Remove(fromNode);

            // Spawn all nodes below this EVERY node
            // During the start of a child we need to use the temporary evaluator to catch any event created during a start
            // Such events can be raised when the "not" operator is used.
            var spawnEvaluator = new EvalEveryStateSpawnEvaluator(EveryNode.Context.PatternContext.StatementName);
            var spawned = EveryNode.ChildNode.NewState(spawnEvaluator, null, 0L);
            spawned.Start(BeginState);

            // If the whole spawned expression already turned true, quit it again
            if (spawnEvaluator.IsEvaluatedTrue)
            {
                spawned.Quit();
            }
            else
            {
                SpawnedNodes.Put(spawned, new LinkedHashMap<Object, long>());
                spawned.ParentEvaluator = this;
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternEveryDistinctEvalFalse(); }
        }

        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternEveryDistinctEvaluateTrue(EveryNode, matchEvent); }

            // determine if this evaluation has been seen before from the same node
            var matchEventKey = PatternExpressionUtil.GetKeys(matchEvent, EveryNode.FactoryNode.Convertor, EveryNode.FactoryNode.DistinctExpressionsArray, EveryNode.Context.AgentInstanceContext);
            var haveSeenThis = false;
            var keysFromNode = SpawnedNodes.Get(fromNode);
            if (keysFromNode != null)
            {
                // Clean out old keys
                var currentTime = EveryNode.Context.PatternContext.TimeProvider.Time;
                var entries = new List<object>();

                foreach (var entry in keysFromNode)
                {
                    if (currentTime >= entry.Value)
                    {
                        entries.Add(entry.Key);
                    }
                    else
                    {
                        break;
                    }
                }

                entries.ForEach(k => keysFromNode.Remove(k));

                if (keysFromNode.ContainsKey(matchEventKey))
                {
                    haveSeenThis = true;
                }
                else
                {
                    long expiryTime = EveryNode.FactoryNode.AbsExpiry(EveryNode.Context);
                    keysFromNode.Put(matchEventKey, expiryTime);
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
                var spawnEvaluator = new EvalEveryStateSpawnEvaluator(EveryNode.Context.PatternContext.StatementName);
                var spawned = EveryNode.ChildNode.NewState(spawnEvaluator, null, 0L);
                spawned.Start(BeginState);

                // If the whole spawned expression already turned true, quit it again
                if (spawnEvaluator.IsEvaluatedTrue)
                {
                    spawned.Quit();
                }
                else
                {
                    var keyset = new LinkedHashMap<Object, long>();
                    if (keysFromNode != null)
                    {
                        keyset.PutAll(keysFromNode);
                    }
                    SpawnedNodes.Put(spawned, keyset);
                    spawned.ParentEvaluator = this;
                }
            }

            if (!haveSeenThis)
            {
                ParentEvaluator.EvaluateTrue(matchEvent, this, false);
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternEveryDistinctEvaluateTrue(null, keysFromNode, matchEventKey, haveSeenThis); }
        }

        public override void Quit()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternEveryDistinctQuit(EveryNode); }
            // Stop all child nodes
            foreach (EvalStateNode child in SpawnedNodes.Keys)
            {
                child.Quit();
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternEveryDistinctQuit(); }
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitEveryDistinct(EveryNode.FactoryNode, this, BeginState, SpawnedNodes.Values);
            foreach (EvalStateNode spawnedNode in SpawnedNodes.Keys)
            {
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
            return "EvalEveryStateNode spawnedChildren=" + SpawnedNodes.Count;
        }
    }
}
