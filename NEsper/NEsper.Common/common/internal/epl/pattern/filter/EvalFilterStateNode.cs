///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.pattern.filter
{
    /// <summary>
    ///     This class contains the state of a single filter expression in the evaluation state tree.
    /// </summary>
    public class EvalFilterStateNode : EvalStateNode,
        FilterHandleCallback
    {
        internal readonly EvalFilterNode evalFilterNode;
        internal MatchedEventMap beginState;
        internal EPStatementHandleCallbackFilter handle;

        internal bool isStarted;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalFilterNode">is the factory node associated to the state</param>
        public EvalFilterStateNode(
            Evaluator parentNode,
            EvalFilterNode evalFilterNode)
            : base(parentNode)
        {
            this.evalFilterNode = evalFilterNode;
        }

        public override EvalNode FactoryNode => evalFilterNode;

        public EvalFilterNode EvalFilterNode => evalFilterNode;

        public override bool IsFilterStateNode => true;

        public override bool IsNotOperator => false;

        public override bool IsObserverStateNodeNonRestarting => false;

        public virtual void MatchFound(
            EventBean theEvent,
            ICollection<FilterHandleCallback> allStmtMatches)
        {
            var agentInstanceContext = evalFilterNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternFilterMatch(evalFilterNode.factoryNode, theEvent);

            if (!isStarted) {
                agentInstanceContext.InstrumentationProvider.APatternFilterMatch(true);
                return;
            }

            var passUp = beginState.ShallowCopy();

            if (evalFilterNode.FactoryNode.FilterSpec.OptionalPropertyEvaluator != null) {
                var propertyEvents =
                    evalFilterNode.FactoryNode.FilterSpec.OptionalPropertyEvaluator.GetProperty(
                        theEvent, evalFilterNode.Context.AgentInstanceContext);
                if (propertyEvents == null) {
                    return; // no results, ignore match
                }

                // Add event itself to the match event structure if a tag was provided
                if (evalFilterNode.FactoryNode.EventAsName != null) {
                    passUp.Add(evalFilterNode.FactoryNode.EventAsTagNumber, propertyEvents);
                }
            }
            else {
                // Add event itself to the match event structure if a tag was provided
                if (evalFilterNode.FactoryNode.EventAsName != null) {
                    passUp.Add(evalFilterNode.FactoryNode.EventAsTagNumber, theEvent);
                }
            }

            // Explanation for the type cast...
            // Each state node stops listening if it resolves to true, and all nodes newState
            // new listeners again. However this would be a performance drain since
            // and expression such as "on all b()" would remove the listener for b() for every match
            // and the all node would newState a new listener. The remove operation and the add operation
            // therefore don't take place if the EvalEveryStateNode node sits on top of a EvalFilterStateNode node.
            var isQuitted = false;
            if (!ParentEvaluator.IsFilterChildNonQuitting) {
                StopFiltering();
                isQuitted = true;
                agentInstanceContext.AuditProvider.PatternInstance(
                    false, evalFilterNode.factoryNode, agentInstanceContext);
            }

            EvaluateTrue(passUp, isQuitted, theEvent);

            agentInstanceContext.InstrumentationProvider.APatternFilterMatch(isQuitted);
        }

        public bool IsSubSelect => false;

        public override void Start(MatchedEventMap beginState)
        {
            var agentInstanceContext = evalFilterNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternFilterStart(evalFilterNode.factoryNode, beginState);

            this.beginState = beginState;
            if (isStarted) {
                throw new IllegalStateException("Filter state node already active");
            }

            agentInstanceContext.AuditProvider.PatternInstance(true, evalFilterNode.factoryNode, agentInstanceContext);

            // Start the filter
            isStarted = true;

            var filterService = evalFilterNode.Context.FilterService;
            handle = new EPStatementHandleCallbackFilter(
                evalFilterNode.Context.AgentInstanceContext.EpStatementAgentInstanceHandle, this);
            var filterSpec = evalFilterNode.FactoryNode.FilterSpec;
            FilterValueSetParam[][] filterValues = filterSpec.GetValueSet(
                beginState, evalFilterNode.AddendumFilters, agentInstanceContext,
                agentInstanceContext.StatementContextFilterEvalEnv);
            filterService.Add(filterSpec.FilterForEventType, filterValues, handle);
            var filtersVersion = filterService.FiltersVersion;
            evalFilterNode.Context.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion
                .StmtFilterVersion = filtersVersion;

            agentInstanceContext.InstrumentationProvider.APatternFilterStart();
        }

        public override void Quit()
        {
            var agentInstanceContext = evalFilterNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternFilterQuit(evalFilterNode.factoryNode, beginState);
            agentInstanceContext.AuditProvider.PatternInstance(false, evalFilterNode.factoryNode, agentInstanceContext);

            isStarted = false;
            StopFiltering();

            agentInstanceContext.InstrumentationProvider.APatternFilterQuit();
        }

        private void EvaluateTrue(
            MatchedEventMap theEvent,
            bool isQuitted,
            EventBean optionalTriggeringEvent)
        {
            var agentInstanceContext = evalFilterNode.Context.AgentInstanceContext;
            agentInstanceContext.AuditProvider.PatternTrue(
                evalFilterNode.FactoryNode, this, theEvent, isQuitted, agentInstanceContext);
            ParentEvaluator.EvaluateTrue(theEvent, this, isQuitted, optionalTriggeringEvent);
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitFilter(evalFilterNode.FactoryNode, this, handle, beginState);
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("EvalFilterStateNode");
            buffer.Append(" tag=");
            buffer.Append(evalFilterNode.FactoryNode.FilterSpec);
            buffer.Append(" spec=");
            buffer.Append(evalFilterNode.FactoryNode.FilterSpec);
            return buffer.ToString();
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            if (!isStarted) {
                return;
            }

            if (PatternConsumptionUtil.ContainsEvent(matchEvent, beginState)) {
                Quit();
                var agentInstanceContext = evalFilterNode.Context.AgentInstanceContext;
                agentInstanceContext.AuditProvider.PatternFalse(evalFilterNode.FactoryNode, this, agentInstanceContext);
                ParentEvaluator.EvaluateFalse(this, true);
            }
        }

        private void StopFiltering()
        {
            var agentInstanceContext = evalFilterNode.Context.AgentInstanceContext;
            var filterSpec = evalFilterNode.FactoryNode.FilterSpec;
            FilterValueSetParam[][] filterValues = filterSpec.GetValueSet(
                beginState, evalFilterNode.AddendumFilters, agentInstanceContext,
                agentInstanceContext.StatementContextFilterEvalEnv);
            var filterService = evalFilterNode.Context.FilterService;
            if (handle != null) {
                filterService.Remove(handle, filterSpec.FilterForEventType, filterValues);
            }

            handle = null;
            isStarted = false;
            var filtersVersion = filterService.FiltersVersion;
            evalFilterNode.Context.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion
                .StmtFilterVersion = filtersVersion;
        }
    }
} // end of namespace