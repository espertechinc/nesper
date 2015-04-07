///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class contains the state of a single filter expression in the evaluation state tree.
    /// </summary>
    public class EvalFilterStateNode : EvalStateNode, FilterHandleCallback
    {
        private readonly EvalFilterNode _evalFilterNode;
    
        protected bool IsStarted;
        protected EPStatementHandleCallback Handle;
        protected MatchedEventMap BeginState;
    
        /// <summary>Constructor. </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalFilterNode">is the factory node associated to the state</param>
        public EvalFilterStateNode(Evaluator parentNode, EvalFilterNode evalFilterNode)
            : base(parentNode)
        {
            _evalFilterNode = evalFilterNode;
        }

        public override EvalNode FactoryNode
        {
            get { return _evalFilterNode; }
        }

        public string StatementId
        {
            get { return _evalFilterNode.Context.PatternContext.StatementId; }
        }

        public override void Start(MatchedEventMap beginState)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternFilterStart(_evalFilterNode, beginState);}
            BeginState = beginState;
            if (IsStarted)
            {
                throw new IllegalStateException("Filter state node already active");
            }
    
            // Start the filter
            IsStarted = true;
            StartFiltering();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternFilterStart();}
        }
    
        public override void Quit()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternFilterQuit(_evalFilterNode, BeginState);}
            IsStarted = false;
            StopFiltering();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternFilterQuit();}
        }
    
        private void EvaluateTrue(MatchedEventMap theEvent, bool isQuitted)
        {
            ParentEvaluator.EvaluateTrue(theEvent, this, isQuitted);
        }

        public EvalFilterNode EvalFilterNode
        {
            get { return _evalFilterNode; }
        }

        public virtual void MatchFound(EventBean theEvent, ICollection<FilterHandleCallback> allStmtMatches)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternFilterMatch(_evalFilterNode, theEvent);}
    
            if (!IsStarted)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternFilterMatch(true);}
                return;
            }
    
            MatchedEventMap passUp = BeginState.ShallowCopy();
    
            if (_evalFilterNode.FactoryNode.FilterSpec.OptionalPropertyEvaluator != null)
            {
                EventBean[] propertyEvents = _evalFilterNode.FactoryNode.FilterSpec.OptionalPropertyEvaluator.GetProperty(theEvent, _evalFilterNode.Context.AgentInstanceContext);
                if (propertyEvents == null)
                {
                    return; // no results, ignore match
                }
                // Add event itself to the match event structure if a tag was provided
                if (_evalFilterNode.FactoryNode.EventAsName != null)
                {
                    passUp.Add(_evalFilterNode.FactoryNode.EventAsTagNumber, propertyEvents);
                }
            }
            else
            {
                // Add event itself to the match event structure if a tag was provided
                if (_evalFilterNode.FactoryNode.EventAsName != null)
                {
                    passUp.Add(_evalFilterNode.FactoryNode.EventAsTagNumber, theEvent);
                }
            }
    
            // Explanation for the type cast...
            // Each state node stops listening if it resolves to true, and all nodes newState
            // new listeners again. However this would be a performance drain since
            // and expression such as "on all b()" would remove the listener for b() for every match
            // and the all node would newState a new listener. The remove operation and the add operation
            // therefore don't take place if the EvalEveryStateNode node sits on top of a EvalFilterStateNode node.
            bool isQuitted = false;
            if (!(ParentEvaluator.IsFilterChildNonQuitting))
            {
                StopFiltering();
                isQuitted = true;
            }
    
            EvaluateTrue(passUp, isQuitted);
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternFilterMatch(isQuitted);}
        }
    
        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitFilter(_evalFilterNode.FactoryNode, this, Handle, BeginState);
        }

        public bool IsSubSelect
        {
            get { return false; }
        }

        public override String ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("EvalFilterStateNode");
            buffer.Append(" tag=");
            buffer.Append(_evalFilterNode.FactoryNode.FilterSpec);
            buffer.Append(" spec=");
            buffer.Append(_evalFilterNode.FactoryNode.FilterSpec);
            return buffer.ToString();
        }

        public override bool IsFilterStateNode
        {
            get { return true; }
        }

        public override bool IsNotOperator
        {
            get { return false; }
        }

        public override bool IsObserverStateNodeNonRestarting
        {
            get { return false; }
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            if (!IsStarted) {
                return;
            }
            if (PatternConsumptionUtil.ContainsEvent(matchEvent, BeginState)) {
                Quit();
                ParentEvaluator.EvaluateFalse(this, true);
            }
        }
    
        protected void StartFiltering()
        {
            FilterService filterService = _evalFilterNode.Context.PatternContext.FilterService;
            Handle = new EPStatementHandleCallback(_evalFilterNode.Context.AgentInstanceContext.EpStatementAgentInstanceHandle, this);
            FilterValueSet filterValues = _evalFilterNode.FactoryNode.FilterSpec.GetValueSet(BeginState, _evalFilterNode.Context.AgentInstanceContext, _evalFilterNode.AddendumFilters);
            filterService.Add(filterValues, Handle);
            long filtersVersion = filterService.FiltersVersion;
            _evalFilterNode.Context.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = filtersVersion;
        }
    
        private void StopFiltering()
        {
            PatternContext context = _evalFilterNode.Context.PatternContext;
            if (Handle != null) {
                context.FilterService.Remove(Handle);
            }
            Handle = null;
            IsStarted = false;
            long filtersVersion = context.FilterService.FiltersVersion;
            _evalFilterNode.Context.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = filtersVersion;
        }
    }
}
