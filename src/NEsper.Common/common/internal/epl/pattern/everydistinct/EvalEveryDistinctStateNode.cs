///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.every;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.pattern.everydistinct
{
    /// <summary>
    ///     Contains the state collected by an "every" operator. The state includes handles to any sub-listeners
    ///     started by the operator.
    /// </summary>
    public class EvalEveryDistinctStateNode : EvalStateNode,
        Evaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal readonly EvalEveryDistinctNode everyDistinctNode;
        internal readonly IDictionary<EvalStateNode, ISet<object>> spawnedNodes;
        internal MatchedEventMap beginState;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="everyDistinctNode">is the factory node associated to the state</param>
        public EvalEveryDistinctStateNode(
            Evaluator parentNode,
            EvalEveryDistinctNode everyDistinctNode)
            : base(parentNode)
        {
            this.everyDistinctNode = everyDistinctNode;
            spawnedNodes = new LinkedHashMap<EvalStateNode, ISet<object>>();
        }

        public override EvalNode FactoryNode => everyDistinctNode;

        public override bool IsFilterStateNode => false;

        public override bool IsNotOperator => false;

        public override bool IsObserverStateNodeNonRestarting => false;

        public void EvaluateFalse(
            EvalStateNode fromNode,
            bool restartable)
        {
            var agentInstanceContext = everyDistinctNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternEveryDistinctEvalFalse(everyDistinctNode.factoryNode);

            fromNode.Quit();
            spawnedNodes.Remove(fromNode);

            // Spawn all nodes below this EVERY node
            // During the start of a child we need to use the temporary evaluator to catch any event created during a start
            // Such events can be raised when the "not" operator is used.
            var spawnEvaluator = new EvalEveryStateSpawnEvaluator(everyDistinctNode.Context.StatementName);
            var spawned = everyDistinctNode.ChildNode.NewState(spawnEvaluator);
            spawned.Start(beginState);

            // If the whole spawned expression already turned true, quit it again
            if (spawnEvaluator.IsEvaluatedTrue) {
                spawned.Quit();
            }
            else {
                spawnedNodes.Put(spawned, new HashSet<object>());
                spawned.ParentEvaluator = this;
            }

            agentInstanceContext.InstrumentationProvider.APatternEveryDistinctEvalFalse();
        }

        public void EvaluateTrue(
            MatchedEventMap matchEvent,
            EvalStateNode fromNode,
            bool isQuitted,
            EventBean optionalTriggeringEvent)
        {
            var agentInstanceContext = everyDistinctNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternEveryDistinctEvaluateTrue(
                everyDistinctNode.factoryNode,
                matchEvent);

            // determine if this evaluation has been seen before from the same node
            var matchEventKey = PatternExpressionUtil.GetKeys(
                matchEvent,
                everyDistinctNode.FactoryNode.Convertor,
                everyDistinctNode.FactoryNode.DistinctExpression,
                everyDistinctNode.Context.AgentInstanceContext);
            var haveSeenThis = false;
            var keysFromNode = spawnedNodes.Get(fromNode);
            if (keysFromNode != null) {
                if (keysFromNode.Contains(matchEventKey)) {
                    haveSeenThis = true;
                }
                else {
                    keysFromNode.Add(matchEventKey);
                }
            }

            if (isQuitted) {
                spawnedNodes.Remove(fromNode);
            }

            // See explanation in EvalFilterStateNode for the type check
            if (fromNode.IsFilterStateNode) {
                // We do not need to newState new listeners here, since the filter state node below this node did not quit
            }
            else {
                // Spawn all nodes below this EVERY node
                // During the start of a child we need to use the temporary evaluator to catch any event created during a start
                // Such events can be raised when the "not" operator is used.
                var spawnEvaluator = new EvalEveryStateSpawnEvaluator(everyDistinctNode.Context.StatementName);
                var spawned = everyDistinctNode.ChildNode.NewState(spawnEvaluator);
                spawned.Start(beginState);

                // If the whole spawned expression already turned true, quit it again
                if (spawnEvaluator.IsEvaluatedTrue) {
                    spawned.Quit();
                }
                else {
                    ISet<object> keyset = new HashSet<object>();
                    if (keysFromNode != null) {
                        keyset.AddAll(keysFromNode);
                    }

                    spawnedNodes.Put(spawned, keyset);
                    spawned.ParentEvaluator = this;
                }
            }

            if (!haveSeenThis) {
                agentInstanceContext.AuditProvider.PatternTrue(
                    everyDistinctNode.FactoryNode,
                    this,
                    matchEvent,
                    false,
                    agentInstanceContext);
                ParentEvaluator.EvaluateTrue(matchEvent, this, false, optionalTriggeringEvent);
            }

            agentInstanceContext.InstrumentationProvider.APatternEveryDistinctEvaluateTrue(
                keysFromNode,
                null,
                matchEventKey,
                haveSeenThis);
        }

        public bool IsFilterChildNonQuitting => true;

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            if (PatternConsumptionUtil.ContainsEvent(matchEvent, beginState)) {
                Quit();
                var agentInstanceContext = everyDistinctNode.Context.AgentInstanceContext;
                agentInstanceContext.AuditProvider.PatternFalse(
                    everyDistinctNode.FactoryNode,
                    this,
                    agentInstanceContext);
                ParentEvaluator.EvaluateFalse(this, true);
            }
            else {
                PatternConsumptionUtil.ChildNodeRemoveMatches(matchEvent, spawnedNodes.Keys);
            }
        }

        public override void Start(MatchedEventMap beginState)
        {
            var agentInstanceContext = everyDistinctNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternEveryDistinctStart(
                everyDistinctNode.factoryNode,
                beginState);
            agentInstanceContext.AuditProvider.PatternInstance(
                true,
                everyDistinctNode.factoryNode,
                agentInstanceContext);

            this.beginState = beginState.ShallowCopy();
            var childState = everyDistinctNode.ChildNode.NewState(this);
            spawnedNodes.Put(childState, new HashSet<object>());

            if (spawnedNodes.Count != 1) {
                throw new IllegalStateException("EVERY state node is expected to have single child state node");
            }

            // During the start of the child we need to use the temporary evaluator to catch any event created during a start.
            // Events created during the start would likely come from the "not" operator.
            // Quit the new child again if
            var spawnEvaluator = new EvalEveryStateSpawnEvaluator(everyDistinctNode.Context.StatementName);
            childState.ParentEvaluator = spawnEvaluator;
            childState.Start(beginState);

            // If the spawned expression turned true already, just quit it
            if (spawnEvaluator.IsEvaluatedTrue) {
                childState.Quit();
            }
            else {
                childState.ParentEvaluator = this;
            }

            agentInstanceContext.InstrumentationProvider.APatternEveryDistinctStart();
        }

        public override void Quit()
        {
            var agentInstanceContext = everyDistinctNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternEveryDistinctQuit(everyDistinctNode.factoryNode);
            agentInstanceContext.AuditProvider.PatternInstance(
                false,
                everyDistinctNode.factoryNode,
                agentInstanceContext);

            // Stop all child nodes
            foreach (var child in spawnedNodes.Keys) {
                child.Quit();
            }

            agentInstanceContext.InstrumentationProvider.APatternEveryDistinctQuit();
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitEveryDistinct(
                everyDistinctNode.FactoryNode,
                this,
                beginState,
                spawnedNodes.Values.Unwrap<object>());
            foreach (var spawnedNode in spawnedNodes.Keys) {
                spawnedNode.Accept(visitor);
            }
        }

        public override string ToString()
        {
            return "EvalEveryStateNode spawnedChildren=" + spawnedNodes.Count;
        }
    }
} // end of namespace