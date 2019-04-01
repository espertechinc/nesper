///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.every
{
    /// <summary>
    ///     Contains the state collected by an "every" operator. The state includes handles to any sub-listeners
    ///     started by the operator.
    /// </summary>
    public class EvalEveryStateNode : EvalStateNode,
        Evaluator
    {
        internal readonly EvalEveryNode evalEveryNode;
        internal readonly IList<EvalStateNode> spawnedNodes;
        internal MatchedEventMap beginState;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalEveryNode">is the factory node associated to the state</param>
        public EvalEveryStateNode(
            Evaluator parentNode,
            EvalEveryNode evalEveryNode) : base(parentNode)
        {
            this.evalEveryNode = evalEveryNode;
            spawnedNodes = new List<EvalStateNode>();
        }

        public override EvalNode FactoryNode => evalEveryNode;

        public override bool IsNotOperator => false;

        public override bool IsFilterStateNode => false;

        public override bool IsObserverStateNodeNonRestarting => false;

        public void EvaluateFalse(EvalStateNode fromNode, bool restartable)
        {
            AgentInstanceContext agentInstanceContext = evalEveryNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternEveryEvalFalse(evalEveryNode.factoryNode);

            fromNode.Quit();
            spawnedNodes.Remove(fromNode);

            if (!restartable)
            {
                agentInstanceContext.AuditProvider.PatternFalse(evalEveryNode.FactoryNode, this, agentInstanceContext);
                agentInstanceContext.AuditProvider.PatternInstance(
                    false, evalEveryNode.factoryNode, agentInstanceContext);
                ParentEvaluator.EvaluateFalse(this, false);
                return;
            }

            // Spawn all nodes below this EVERY node
            // During the start of a child we need to use the temporary evaluator to catch any event created during a start
            // Such events can be raised when the "not" operator is used.
            var spawnEvaluator = new EvalEveryStateSpawnEvaluator(evalEveryNode.Context.StatementName);
            EvalStateNode spawned = evalEveryNode.ChildNode.NewState(spawnEvaluator);
            spawned.Start(beginState);

            // If the whole spawned expression already turned true, quit it again
            if (spawnEvaluator.IsEvaluatedTrue)
            {
                spawned.Quit();
            }
            else
            {
                spawnedNodes.Add(spawned);
                spawned.ParentEvaluator = this;
            }

            agentInstanceContext.InstrumentationProvider.APatternEveryEvalFalse();
        }

        public void EvaluateTrue(
            MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted, EventBean optionalTriggeringEvent)
        {
            AgentInstanceContext agentInstanceContext = evalEveryNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternEveryEvaluateTrue(
                evalEveryNode.factoryNode, matchEvent);

            if (isQuitted)
            {
                spawnedNodes.Remove(fromNode);
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
                var spawnEvaluator = new EvalEveryStateSpawnEvaluator(evalEveryNode.Context.StatementName);
                EvalStateNode spawned = evalEveryNode.ChildNode.NewState(spawnEvaluator);
                spawned.Start(beginState);

                // If the whole spawned expression already turned true, quit it again
                if (spawnEvaluator.IsEvaluatedTrue)
                {
                    spawned.Quit();
                }
                else
                {
                    spawnedNodes.Add(spawned);
                    spawned.ParentEvaluator = this;
                }
            }

            // All nodes indicate to their parents that their child node did not quit, therefore a false for isQuitted
            agentInstanceContext.AuditProvider.PatternTrue(
                evalEveryNode.FactoryNode, this, matchEvent, false, agentInstanceContext);
            ParentEvaluator.EvaluateTrue(matchEvent, this, false, optionalTriggeringEvent);

            agentInstanceContext.InstrumentationProvider.APatternEveryEvaluateTrue();
        }

        public bool IsFilterChildNonQuitting => true;

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            if (PatternConsumptionUtil.ContainsEvent(matchEvent, beginState))
            {
                Quit();
                AgentInstanceContext agentInstanceContext = evalEveryNode.Context.AgentInstanceContext;
                agentInstanceContext.AuditProvider.PatternFalse(evalEveryNode.FactoryNode, this, agentInstanceContext);
                ParentEvaluator.EvaluateFalse(this, true);
            }
            else
            {
                PatternConsumptionUtil.ChildNodeRemoveMatches(matchEvent, spawnedNodes);
            }
        }

        public override void Start(MatchedEventMap beginState)
        {
            AgentInstanceContext agentInstanceContext = evalEveryNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternEveryStart(evalEveryNode.factoryNode, beginState);
            agentInstanceContext.AuditProvider.PatternInstance(true, evalEveryNode.factoryNode, agentInstanceContext);

            this.beginState = beginState.ShallowCopy();
            EvalStateNode childState = evalEveryNode.ChildNode.NewState(this);
            spawnedNodes.Add(childState);

            // During the start of the child we need to use the temporary evaluator to catch any event created during a start.
            // Events created during the start would likely come from the "not" operator.
            // Quit the new child again if
            var spawnEvaluator = new EvalEveryStateSpawnEvaluator(evalEveryNode.Context.StatementName);
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

            agentInstanceContext.InstrumentationProvider.APatternEveryStart();
        }

        public override void Quit()
        {
            AgentInstanceContext agentInstanceContext = evalEveryNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternEveryQuit(evalEveryNode.factoryNode);
            agentInstanceContext.AuditProvider.PatternInstance(false, evalEveryNode.factoryNode, agentInstanceContext);

            // Stop all child nodes
            foreach (var child in spawnedNodes)
            {
                child.Quit();
            }

            agentInstanceContext.InstrumentationProvider.APatternEveryQuit();
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitEvery(evalEveryNode.FactoryNode, this, beginState);
            foreach (var spawnedNode in spawnedNodes)
            {
                spawnedNode.Accept(visitor);
            }
        }

        public override string ToString()
        {
            return "EvalEveryStateNode spawnedChildren=" + spawnedNodes.Count;
        }
    }
} // end of namespace