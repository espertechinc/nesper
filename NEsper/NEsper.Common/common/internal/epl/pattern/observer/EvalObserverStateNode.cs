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
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>
    ///     This class represents the state of an eventObserver sub-expression in the evaluation state tree.
    /// </summary>
    public class EvalObserverStateNode : EvalStateNode,
        ObserverEventEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal readonly EvalObserverNode evalObserverNode;
        internal EventObserver eventObserver;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalObserverNode">is the factory node associated to the state</param>
        public EvalObserverStateNode(
            Evaluator parentNode,
            EvalObserverNode evalObserverNode) : base(parentNode)
        {
            this.evalObserverNode = evalObserverNode;
        }

        public override EvalNode FactoryNode => evalObserverNode;

        public override bool IsNotOperator => false;

        public override bool IsFilterStateNode => false;

        public override bool IsObserverStateNodeNonRestarting =>
            evalObserverNode.FactoryNode.IsObserverStateNodeNonRestarting;

        public PatternAgentInstanceContext Context => evalObserverNode.Context;

        public void ObserverEvaluateTrue(MatchedEventMap matchEvent, bool quitted)
        {
            var agentInstanceContext = evalObserverNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternObserverEvaluateTrue(
                evalObserverNode.factoryNode, matchEvent);
            agentInstanceContext.AuditProvider.PatternTrue(
                evalObserverNode.FactoryNode, this, matchEvent, quitted, agentInstanceContext);
            if (quitted) {
                agentInstanceContext.AuditProvider.PatternInstance(
                    false, evalObserverNode.factoryNode, agentInstanceContext);
            }

            ParentEvaluator.EvaluateTrue(matchEvent, this, quitted, null);
            agentInstanceContext.InstrumentationProvider.APatternObserverEvaluateTrue();
        }

        public void ObserverEvaluateFalse(bool restartable)
        {
            var agentInstanceContext = evalObserverNode.Context.AgentInstanceContext;
            agentInstanceContext.AuditProvider.PatternFalse(evalObserverNode.FactoryNode, this, agentInstanceContext);
            agentInstanceContext.AuditProvider.PatternInstance(
                false, evalObserverNode.factoryNode, agentInstanceContext);
            ParentEvaluator.EvaluateFalse(this, restartable);
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            if (PatternConsumptionUtil.ContainsEvent(matchEvent, eventObserver.BeginState)) {
                Quit();
                var agentInstanceContext = evalObserverNode.Context.AgentInstanceContext;
                agentInstanceContext.AuditProvider.PatternFalse(
                    evalObserverNode.FactoryNode, this, agentInstanceContext);
                ParentEvaluator.EvaluateFalse(this, true);
            }
        }

        public override void Start(MatchedEventMap beginState)
        {
            var agentInstanceContext = evalObserverNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternObserverStart(
                evalObserverNode.factoryNode, beginState);
            agentInstanceContext.AuditProvider.PatternInstance(
                true, evalObserverNode.factoryNode, agentInstanceContext);

            eventObserver = evalObserverNode.FactoryNode.ObserverFactory.MakeObserver(
                Context, beginState, this, null, ParentEvaluator.IsFilterChildNonQuitting);
            eventObserver.StartObserve();

            agentInstanceContext.InstrumentationProvider.APatternObserverStart();
        }

        public override void Quit()
        {
            var agentInstanceContext = evalObserverNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternObserverQuit(evalObserverNode.factoryNode);
            agentInstanceContext.AuditProvider.PatternInstance(
                false, evalObserverNode.factoryNode, agentInstanceContext);

            eventObserver.StopObserve();

            agentInstanceContext.InstrumentationProvider.APatternObserverQuit();
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitObserver(evalObserverNode.FactoryNode, this, eventObserver);
        }

        public override string ToString()
        {
            return "EvalObserverStateNode eventObserver=" + eventObserver;
        }
    }
} // end of namespace