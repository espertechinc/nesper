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

namespace com.espertech.esper.common.@internal.epl.pattern.not
{
    /// <summary>
    ///     This class contains the state of an 'not' operator in the evaluation state tree.
    ///     The not operator inverts the truth of the subexpression under it. It defaults to being true rather than
    ///     being false at startup. True at startup means it will generate an event on newState such that parent expressions
    ///     may turn true. It turns permenantly false when it receives an event from a subexpression and the subexpression
    ///     quitted. It indicates the false state via an evaluateFalse call on its parent evaluator.
    /// </summary>
    public class EvalNotStateNode : EvalStateNode,
        Evaluator
    {
        internal readonly EvalNotNode evalNotNode;
        protected EvalStateNode childNode;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalNotNode">is the factory node associated to the state</param>
        public EvalNotStateNode(
            Evaluator parentNode,
            EvalNotNode evalNotNode)
            : base(parentNode)
        {
            this.evalNotNode = evalNotNode;
        }

        public override EvalNode FactoryNode => evalNotNode;

        public override bool IsNotOperator => true;

        public override bool IsFilterStateNode => false;

        public override bool IsObserverStateNodeNonRestarting => false;

        public void EvaluateFalse(
            EvalStateNode fromNode,
            bool restartable)
        {
            var agentInstanceContext = evalNotNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternNotEvalFalse(evalNotNode.factoryNode);
            agentInstanceContext.InstrumentationProvider.APatternNotEvalFalse();
        }

        public void EvaluateTrue(
            MatchedEventMap matchEvent,
            EvalStateNode fromNode,
            bool isQuitted,
            EventBean optionalTriggeringEvent)
        {
            var agentInstanceContext = evalNotNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternNotEvaluateTrue(evalNotNode.factoryNode, matchEvent);

            // Only is the subexpression stopped listening can we tell the parent evaluator that this
            // turned permanently false.
            if (isQuitted) {
                childNode = null;
                agentInstanceContext.AuditProvider.PatternFalse(evalNotNode.FactoryNode, this, agentInstanceContext);
                agentInstanceContext.AuditProvider.PatternInstance(
                    false, evalNotNode.factoryNode, agentInstanceContext);
                ParentEvaluator.EvaluateFalse(this, true);
            }

            agentInstanceContext.InstrumentationProvider.APatternNotEvaluateTrue(isQuitted);
        }

        public bool IsFilterChildNonQuitting => false;

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            // The not-operator does not pass along the matches
        }

        public override void Start(MatchedEventMap beginState)
        {
            var factoryNode = evalNotNode.FactoryNode;
            var agentInstanceContext = evalNotNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternNotStart(evalNotNode.factoryNode, beginState);
            agentInstanceContext.AuditProvider.PatternInstance(true, factoryNode, agentInstanceContext);

            childNode = evalNotNode.ChildNode.NewState(this);
            childNode.Start(beginState);

            // The not node acts by inverting the truth
            // By default the child nodes are false. This not node acts inverts the truth and pretends the child is true,
            // raising an event up.
            agentInstanceContext.AuditProvider.PatternTrue(factoryNode, this, beginState, false, agentInstanceContext);
            ParentEvaluator.EvaluateTrue(beginState, this, false, null);
            agentInstanceContext.InstrumentationProvider.APatternNotStart();
        }

        public override void Quit()
        {
            var agentInstanceContext = evalNotNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternNotQuit(evalNotNode.factoryNode);
            agentInstanceContext.AuditProvider.PatternInstance(false, evalNotNode.factoryNode, agentInstanceContext);

            if (childNode != null) {
                childNode.Quit();
            }

            agentInstanceContext.InstrumentationProvider.APatternNotQuit();
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitNot(evalNotNode.FactoryNode, this);
            if (childNode != null) {
                childNode.Accept(visitor);
            }
        }

        public override string ToString()
        {
            return "EvalNotStateNode child=" + childNode;
        }
    }
} // end of namespace