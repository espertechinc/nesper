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
        private readonly EvalFilterNode _evalFilterNode;
        private MatchedEventMap _beginState;
        private EPStatementHandleCallbackFilter _handle;
        private bool _isStarted;

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
            _evalFilterNode = evalFilterNode;
        }

        public override EvalNode FactoryNode => _evalFilterNode;

        public EvalFilterNode EvalFilterNode => _evalFilterNode;

        public override bool IsFilterStateNode => true;

        public override bool IsNotOperator => false;

        public override bool IsObserverStateNodeNonRestarting => false;

        public virtual void MatchFound(
            EventBean theEvent,
            ICollection<FilterHandleCallback> allStmtMatches)
        {
            var agentInstanceContext = _evalFilterNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternFilterMatch(_evalFilterNode.factoryNode, theEvent);

            if (!_isStarted) {
                agentInstanceContext.InstrumentationProvider.APatternFilterMatch(true);
                return;
            }

            var passUp = _beginState.ShallowCopy();

            if (_evalFilterNode.FactoryNode.FilterSpec.OptionalPropertyEvaluator != null) {
                var propertyEvents =
                    _evalFilterNode.FactoryNode.FilterSpec.OptionalPropertyEvaluator.GetProperty(
                        theEvent,
                        _evalFilterNode.Context.AgentInstanceContext);
                if (propertyEvents == null) {
                    return; // no results, ignore match
                }

                // Add event itself to the match event structure if a tag was provided
                if (_evalFilterNode.FactoryNode.EventAsName != null) {
                    passUp.Add(_evalFilterNode.FactoryNode.EventAsTagNumber, propertyEvents);
                }
            }
            else {
                // Add event itself to the match event structure if a tag was provided
                if (_evalFilterNode.FactoryNode.EventAsName != null) {
                    passUp.Add(_evalFilterNode.FactoryNode.EventAsTagNumber, theEvent);
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
                    false,
                    _evalFilterNode.factoryNode,
                    agentInstanceContext);
            }

            EvaluateTrue(passUp, isQuitted, theEvent);

            agentInstanceContext.InstrumentationProvider.APatternFilterMatch(isQuitted);
        }

        public bool IsSubSelect => false;

        public override void Start(MatchedEventMap beginState)
        {
            var agentInstanceContext = _evalFilterNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternFilterStart(_evalFilterNode.factoryNode, beginState);

            _beginState = beginState;
            if (_isStarted) {
                throw new IllegalStateException("Filter state node already active");
            }

            agentInstanceContext.AuditProvider.PatternInstance(true, _evalFilterNode.factoryNode, agentInstanceContext);

            // Start the filter
            _isStarted = true;

            var filterService = _evalFilterNode.Context.FilterService;
            _handle = new EPStatementHandleCallbackFilter(
                _evalFilterNode.Context.AgentInstanceContext.EpStatementAgentInstanceHandle,
                this);
            var filterSpec = _evalFilterNode.FactoryNode.FilterSpec;
            var filterValues = filterSpec.GetValueSet(
                beginState,
                _evalFilterNode.AddendumFilters,
                agentInstanceContext,
                agentInstanceContext.StatementContextFilterEvalEnv);
            if (filterValues != null) {
                filterService.Add(filterSpec.FilterForEventType, filterValues, _handle);
                var filtersVersion = filterService.FiltersVersion;
                _evalFilterNode.Context.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion
                    .StmtFilterVersion = filtersVersion;
            }

            agentInstanceContext.InstrumentationProvider.APatternFilterStart();
        }

        public override void Quit()
        {
            var agentInstanceContext = _evalFilterNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternFilterQuit(_evalFilterNode.factoryNode, _beginState);
            agentInstanceContext.AuditProvider.PatternInstance(false, _evalFilterNode.factoryNode, agentInstanceContext);

            _isStarted = false;
            StopFiltering();

            agentInstanceContext.InstrumentationProvider.APatternFilterQuit();
        }

        private void EvaluateTrue(
            MatchedEventMap theEvent,
            bool isQuitted,
            EventBean optionalTriggeringEvent)
        {
            var agentInstanceContext = _evalFilterNode.Context.AgentInstanceContext;
            agentInstanceContext.AuditProvider.PatternTrue(
                _evalFilterNode.FactoryNode,
                this,
                theEvent,
                isQuitted,
                agentInstanceContext);
            ParentEvaluator.EvaluateTrue(theEvent, this, isQuitted, optionalTriggeringEvent);
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitFilter(_evalFilterNode.FactoryNode, this, _handle, _beginState);
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("EvalFilterStateNode");
            buffer.Append(" tag=");
            buffer.Append(_evalFilterNode.FactoryNode.FilterSpec);
            buffer.Append(" spec=");
            buffer.Append(_evalFilterNode.FactoryNode.FilterSpec);
            return buffer.ToString();
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            if (!_isStarted) {
                return;
            }

            if (PatternConsumptionUtil.ContainsEvent(matchEvent, _beginState)) {
                Quit();
                var agentInstanceContext = _evalFilterNode.Context.AgentInstanceContext;
                agentInstanceContext.AuditProvider.PatternFalse(_evalFilterNode.FactoryNode, this, agentInstanceContext);
                ParentEvaluator.EvaluateFalse(this, true);
            }
        }

        private void StopFiltering()
        {
            var agentInstanceContext = _evalFilterNode.Context.AgentInstanceContext;
            var filterSpec = _evalFilterNode.FactoryNode.FilterSpec;
            var filterValues = filterSpec.GetValueSet(
                _beginState,
                _evalFilterNode.AddendumFilters,
                agentInstanceContext,
                agentInstanceContext.StatementContextFilterEvalEnv);
            var filterService = _evalFilterNode.Context.FilterService;
            if (_handle != null && filterValues != null) {
                filterService.Remove(_handle, filterSpec.FilterForEventType, filterValues);
                var filtersVersionX = filterService.FiltersVersion;
                _evalFilterNode.Context.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = filtersVersionX;
            }

            _handle = null;
            _isStarted = false;
            var filtersVersion = filterService.FiltersVersion;
            _evalFilterNode.Context.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion
                .StmtFilterVersion = filtersVersion;
        }
        
        public override void Transfer(AgentInstanceTransferServices services) {
            if (_handle == null) {
                return;
            }
            var filterSpec = _evalFilterNode.FactoryNode.FilterSpec;
            var filterValues = filterSpec.GetValueSet(
                _beginState,
                _evalFilterNode.AddendumFilters,
                services.AgentInstanceContext,
                services.AgentInstanceContext.StatementContextFilterEvalEnv);
            if (filterValues != null) {
                services.AgentInstanceContext.FilterService.Remove(_handle, filterSpec.FilterForEventType, filterValues);
                services.TargetFilterService.Add(filterSpec.FilterForEventType, filterValues, _handle);
            }
        }
    }
} // end of namespace