///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.condition;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.pool;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.pattern.followedby
{
    /// <summary>
    ///     This class represents the state of a followed-by operator in the evaluation state tree, with a maximum number of
    ///     instances provided, and
    ///     with the additional capability to runtime-wide report on pattern instances.
    /// </summary>
    public class EvalFollowedByWithMaxStateNodeManaged : EvalStateNode,
        Evaluator
    {
        internal readonly int[] countActivePerChild;
        internal readonly EvalFollowedByNode evalFollowedByNode;
        internal readonly Dictionary<EvalStateNode, int> nodes;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalFollowedByNode">is the factory node associated to the state</param>
        public EvalFollowedByWithMaxStateNodeManaged(
            Evaluator parentNode,
            EvalFollowedByNode evalFollowedByNode)
            : base(parentNode)
        {
            this.evalFollowedByNode = evalFollowedByNode;
            nodes = new Dictionary<EvalStateNode, int>();
            if (evalFollowedByNode.IsTrackWithMax) {
                countActivePerChild = new int[evalFollowedByNode.ChildNodes.Length - 1];
            }
            else {
                countActivePerChild = null;
            }
        }

        public override EvalNode FactoryNode => evalFollowedByNode;

        public override bool IsNotOperator => false;

        public override bool IsFilterStateNode => false;

        public bool IsFilterChildNonQuitting => false;

        public override bool IsObserverStateNodeNonRestarting => false;

        public void EvaluateTrue(
            MatchedEventMap matchEvent,
            EvalStateNode fromNode,
            bool isQuitted,
            EventBean optionalTriggeringEvent)
        {
            var hasIndex = nodes.TryGetValue(fromNode, out var index);

            var agentInstanceContext = evalFollowedByNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternFollowedByEvaluateTrue(
                evalFollowedByNode.factoryNode,
                matchEvent,
                index);

            if (isQuitted) {
                nodes.Remove(fromNode);
                if (hasIndex && index > 0) {
                    if (evalFollowedByNode.IsTrackWithMax) {
                        countActivePerChild[index - 1]--;
                    }

                    if (evalFollowedByNode.IsTrackWithPool) {
                        var poolSvc =
                            evalFollowedByNode.Context.StatementContext.PatternSubexpressionPoolSvc;
                        poolSvc.RuntimeSvc.DecreaseCount(
                            evalFollowedByNode,
                            evalFollowedByNode.Context.AgentInstanceContext);
                        poolSvc.StmtHandler.DecreaseCount();
                    }
                }
            }

            // the node may already have quit as a result of an outer state quitting this state,
            // however the callback may still be received; It is fine to ignore this callback.
            if (!hasIndex) {
                agentInstanceContext.InstrumentationProvider.APatternFollowedByEvaluateTrue(false);
                return;
            }

            // If the match came from the very last filter, need to escalate
            var numChildNodes = evalFollowedByNode.ChildNodes.Length;
            var isFollowedByQuitted = false;
            if (index == numChildNodes - 1) {
                if (nodes.IsEmpty()) {
                    isFollowedByQuitted = true;
                    agentInstanceContext.AuditProvider.PatternInstance(
                        false,
                        evalFollowedByNode.factoryNode,
                        agentInstanceContext);
                }

                agentInstanceContext.AuditProvider.PatternTrue(
                    evalFollowedByNode.FactoryNode,
                    this,
                    matchEvent,
                    isFollowedByQuitted,
                    agentInstanceContext);
                ParentEvaluator.EvaluateTrue(matchEvent, this, isFollowedByQuitted, optionalTriggeringEvent);
            }
            else {
                // Else start a new sub-expression for the next-in-line filter
                if (evalFollowedByNode.IsTrackWithMax) {
                    var max = evalFollowedByNode.FactoryNode.GetMax(index);
                    if (max != -1 && max >= 0) {
                        if (countActivePerChild[index] >= max) {
                            evalFollowedByNode.Context.AgentInstanceContext.StatementContext.ExceptionHandlingService
                                .HandleCondition(
                                    new ConditionPatternSubexpressionMax(max),
                                    evalFollowedByNode.Context.AgentInstanceContext.StatementContext);
                            return;
                        }
                    }
                }

                if (evalFollowedByNode.IsTrackWithPool) {
                    var poolSvc =
                        evalFollowedByNode.Context.StatementContext.PatternSubexpressionPoolSvc;
                    var allow = poolSvc.RuntimeSvc.TryIncreaseCount(
                        evalFollowedByNode,
                        evalFollowedByNode.Context.AgentInstanceContext);
                    if (!allow) {
                        return;
                    }

                    poolSvc.StmtHandler.IncreaseCount();
                }

                if (evalFollowedByNode.IsTrackWithMax) {
                    countActivePerChild[index]++;
                }

                var child = evalFollowedByNode.ChildNodes[index + 1];
                var childState = child.NewState(this);
                nodes.Put(childState, index + 1);
                childState.Start(matchEvent);
            }

            agentInstanceContext.InstrumentationProvider.APatternFollowedByEvaluateTrue(isFollowedByQuitted);
        }

        public void EvaluateFalse(
            EvalStateNode fromNode,
            bool restartable)
        {
            var agentInstanceContext = evalFollowedByNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternFollowedByEvalFalse(evalFollowedByNode.factoryNode);

            fromNode.Quit();
            var hasIndex = nodes.TryGetValue(fromNode, out var index);
            if (hasIndex) {
                nodes.Remove(fromNode);
            }

            if (hasIndex && index > 0) {
                if (evalFollowedByNode.IsTrackWithMax) {
                    countActivePerChild[index - 1]--;
                }

                if (evalFollowedByNode.IsTrackWithPool) {
                    var poolSvc =
                        evalFollowedByNode.Context.StatementContext.PatternSubexpressionPoolSvc;
                    poolSvc.RuntimeSvc.DecreaseCount(
                        evalFollowedByNode,
                        evalFollowedByNode.Context.AgentInstanceContext);
                    poolSvc.StmtHandler.DecreaseCount();
                }
            }

            if (nodes.IsEmpty()) {
                agentInstanceContext.AuditProvider.PatternFalse(
                    evalFollowedByNode.FactoryNode,
                    this,
                    agentInstanceContext);
                ParentEvaluator.EvaluateFalse(this, true);
                Quit();
            }

            agentInstanceContext.InstrumentationProvider.APatternFollowedByEvalFalse();
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            PatternConsumptionUtil.ChildNodeRemoveMatches(matchEvent, nodes.Keys);
        }

        public override void Start(MatchedEventMap beginState)
        {
            var agentInstanceContext = evalFollowedByNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternFollowedByStart(
                evalFollowedByNode.factoryNode,
                beginState);
            agentInstanceContext.AuditProvider.PatternInstance(
                true,
                evalFollowedByNode.factoryNode,
                agentInstanceContext);

            var child = evalFollowedByNode.ChildNodes[0];
            var childState = child.NewState(this);
            nodes.Put(childState, 0);
            childState.Start(beginState);

            agentInstanceContext.InstrumentationProvider.APatternFollowedByStart();
        }

        public override void Quit()
        {
            var agentInstanceContext = evalFollowedByNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternFollowedByQuit(evalFollowedByNode.factoryNode);
            agentInstanceContext.AuditProvider.PatternInstance(
                false,
                evalFollowedByNode.factoryNode,
                agentInstanceContext);

            foreach (var entry in nodes) {
                entry.Key.Quit();
                if (evalFollowedByNode.IsTrackWithPool) {
                    if (entry.Value > 0) {
                        var poolSvc =
                            evalFollowedByNode.Context.StatementContext.PatternSubexpressionPoolSvc;
                        poolSvc.RuntimeSvc.DecreaseCount(
                            evalFollowedByNode,
                            evalFollowedByNode.Context.AgentInstanceContext);
                        poolSvc.StmtHandler.DecreaseCount();
                    }
                }
            }

            agentInstanceContext.InstrumentationProvider.APatternFollowedByQuit();
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitFollowedBy(evalFollowedByNode.FactoryNode, this, nodes, countActivePerChild);
            foreach (var node in nodes.Keys) {
                node.Accept(visitor);
            }
        }

        public override string ToString()
        {
            return "EvalFollowedByStateNode nodes=" + nodes.Count;
        }
    }
} // end of namespace