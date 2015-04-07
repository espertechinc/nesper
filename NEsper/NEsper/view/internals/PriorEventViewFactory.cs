///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prior;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// Factory for making <seealso cref="PriorEventView"/> instances.
    /// </summary>
    public class PriorEventViewFactory : ViewFactory
    {
        private EventType _eventType;
    
        /// <summary>unbound to indicate the we are not receiving remove stream events (unbound stream, stream without child views) therefore must use a different buffer. </summary>
        private bool _isUnbound;
    
        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            IList<Object> viewParameters = ViewFactorySupport.ValidateAndEvaluate(ViewName, viewFactoryContext.StatementContext, expressionParameters);
            if (viewParameters.Count != 1)
            {
                throw new ViewParameterException("View requires a single parameter indicating unbound or not");
            }
            _isUnbound = (bool) viewParameters[0];
        }
    
        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            _eventType = parentEventType;
        }
    
        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext) {
            return new PriorEventView(agentInstanceViewFactoryContext.PriorViewUpdatedCollection);
        }
    
        public ViewUpdatedCollection MakeViewUpdatedCollection(IDictionary<int, IList<ExprPriorNode>> callbacksPerIndex, int agentInstanceId) {
    
            if (callbacksPerIndex.IsEmpty())
            {
                throw new IllegalStateException("No resources requested");
            }
    
            // Construct an array of requested prior-event indexes (such as 10th prior event, 8th prior = {10, 8})
            int[] requested = new int[callbacksPerIndex.Count];
            int count = 0;
            foreach (int reqIndex in callbacksPerIndex.Keys)
            {
                requested[count++] = reqIndex;
            }
    
            // For unbound streams the buffer is strictly rolling new events
            if (_isUnbound)
            {
                return new PriorEventBufferUnbound(callbacksPerIndex.Keys.Last());
            }
            // For bound streams (with views posting old and new data), and if only one prior index requested
            else if (requested.Length == 1)
            {
                return new PriorEventBufferSingle(requested[0]);
            }
            else
            {
                // For bound streams (with views posting old and new data)
                // Multiple prior event indexes requested, such as "Prior(2, price), Prior(8, price)"
                // Sharing a single viewUpdatedCollection for multiple prior-event indexes
                return new PriorEventBufferMulti(requested);
            }
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view)
        {
            return false;
        }

        public string ViewName
        {
            get { return "Prior-Event"; }
        }
    }
}
