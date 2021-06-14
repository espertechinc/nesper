///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    /// <summary>
    ///     This class represents the state of a "within" operator in the evaluation state tree.
    ///     The within operator applies to a subexpression and is thus expected to only
    ///     have one child node.
    /// </summary>
    public class EvalGuardStateNode : EvalStateNode,
        Evaluator,
        Quitable
    {
        internal EvalStateNode activeChildNode;
        internal MatchedEventMap beginState;
        internal EvalGuardNode evalGuardNode;
        internal Guard guard;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalGuardNode">is the factory node associated to the state</param>
        public EvalGuardStateNode(
            Evaluator parentNode,
            EvalGuardNode evalGuardNode)
            : base(parentNode)
        {
            this.evalGuardNode = evalGuardNode;
        }

        public override EvalNode FactoryNode => evalGuardNode;

        public override bool IsNotOperator => false;

        public override bool IsFilterStateNode => false;

        public override bool IsObserverStateNodeNonRestarting => false;

        public void EvaluateTrue(
            MatchedEventMap matchEvent,
            EvalStateNode fromNode,
            bool isQuitted,
            EventBean optionalTriggeringEvent)
        {
            var agentInstanceContext = evalGuardNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternGuardEvaluateTrue(
                evalGuardNode.factoryNode,
                matchEvent);

            var haveQuitted = activeChildNode == null;

            // If one of the children quits, remove the child
            if (isQuitted) {
                agentInstanceContext.AuditProvider.PatternInstance(
                    false,
                    evalGuardNode.factoryNode,
                    agentInstanceContext);
                activeChildNode = null;

                // Stop guard, since associated subexpression is gone
                guard.StopGuard();
            }

            if (!haveQuitted) {
                var guardPass = guard.Inspect(matchEvent);
                if (guardPass) {
                    agentInstanceContext.AuditProvider.PatternTrue(
                        evalGuardNode.FactoryNode,
                        this,
                        matchEvent,
                        isQuitted,
                        agentInstanceContext);
                    ParentEvaluator.EvaluateTrue(matchEvent, this, isQuitted, optionalTriggeringEvent);
                }
            }

            agentInstanceContext.InstrumentationProvider.APatternGuardEvaluateTrue(isQuitted);
        }

        public void EvaluateFalse(
            EvalStateNode fromNode,
            bool restartable)
        {
            activeChildNode = null;
            var agentInstanceContext = evalGuardNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternGuardEvalFalse(evalGuardNode.factoryNode);
            agentInstanceContext.AuditProvider.PatternFalse(evalGuardNode.FactoryNode, this, agentInstanceContext);
            agentInstanceContext.AuditProvider.PatternInstance(false, evalGuardNode.factoryNode, agentInstanceContext);
            ParentEvaluator.EvaluateFalse(this, true);
            agentInstanceContext.InstrumentationProvider.APatternGuardEvalFalse();
        }

        public bool IsFilterChildNonQuitting => false;

        public PatternAgentInstanceContext Context => evalGuardNode.Context;

        public void GuardQuit()
        {
            var agentInstanceContext = evalGuardNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternGuardGuardQuit(evalGuardNode.factoryNode);

            // It is possible that the child node has already been quit such as when the parent wait time was shorter.
            // 1. parent node's guard indicates quit to all children
            // 2. this node's guards also indicates quit, however that already occured
            activeChildNode?.Quit();

            activeChildNode = null;

            // Indicate to parent state that this is permanently false.
            agentInstanceContext.AuditProvider.PatternFalse(evalGuardNode.FactoryNode, this, agentInstanceContext);
            agentInstanceContext.AuditProvider.PatternInstance(false, evalGuardNode.factoryNode, agentInstanceContext);
            ParentEvaluator.EvaluateFalse(this, true);

            agentInstanceContext.InstrumentationProvider.APatternGuardGuardQuit();
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            if (PatternConsumptionUtil.ContainsEvent(matchEvent, beginState)) {
                Quit();
                var agentInstanceContext = evalGuardNode.Context.AgentInstanceContext;
                agentInstanceContext.AuditProvider.PatternFalse(evalGuardNode.FactoryNode, this, agentInstanceContext);
                ParentEvaluator.EvaluateFalse(this, true);
            }
            else {
                activeChildNode?.RemoveMatch(matchEvent);
            }
        }

        public override void Start(MatchedEventMap beginState)
        {
            var agentInstanceContext = evalGuardNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternGuardStart(evalGuardNode.factoryNode, beginState);
            agentInstanceContext.AuditProvider.PatternInstance(true, evalGuardNode.factoryNode, agentInstanceContext);

            this.beginState = beginState;
            guard = evalGuardNode.FactoryNode.GuardFactory.MakeGuard(evalGuardNode.Context, beginState, this, null);
            activeChildNode = evalGuardNode.ChildNode.NewState(this);

            // Start the single child state
            activeChildNode.Start(beginState);

            // Start the guard
            guard.StartGuard();

            agentInstanceContext.InstrumentationProvider.APatternGuardStart();
        }

        public override void Quit()
        {
            if (activeChildNode == null) {
                return;
            }

            var agentInstanceContext = evalGuardNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternGuardQuit(evalGuardNode.factoryNode);
            agentInstanceContext.AuditProvider.PatternInstance(false, evalGuardNode.factoryNode, agentInstanceContext);

            if (activeChildNode != null) {
                activeChildNode.Quit();
                guard.StopGuard();
            }

            activeChildNode = null;

            agentInstanceContext.InstrumentationProvider.APatternGuardQuit();
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitGuard(evalGuardNode.FactoryNode, this, guard);
            activeChildNode?.Accept(visitor);
        }

        public override string ToString()
        {
            return "EvaluationWitinStateNode activeChildNode=" +
                   activeChildNode +
                   " guard=" +
                   guard;
        }
    }
} // end of namespace