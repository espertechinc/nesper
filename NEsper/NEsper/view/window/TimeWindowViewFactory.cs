///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Factory for <seealso cref="TimeWindowView"/>.
    /// </summary>
    public class TimeWindowViewFactory : DataWindowViewFactory, DataWindowViewWithPrevious
    {
        /// <summary>Number of msec before expiry. </summary>
        private ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
    
        private EventType _eventType;
    
        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            if (expressionParameters.Count != 1)
            {
                throw new ViewParameterException(ViewParamMessage);
            }

            _timeDeltaComputation = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDelta(
                ViewName, viewFactoryContext.StatementContext, expressionParameters[0], ViewParamMessage, 0);
        }
    
        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            _eventType = parentEventType;
        }
    
        public Object MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }
    
        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var randomAccess = agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory.GetOptPreviousExprRandomAccess(agentInstanceViewFactoryContext); 
            return new TimeWindowView(agentInstanceViewFactoryContext, this, _timeDeltaComputation, randomAccess);
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view)
        {
            if (!(view is TimeWindowView))
            {
                return false;
            }
    
            var myView = (TimeWindowView) view;
            if (!_timeDeltaComputation.EqualsTimePeriod(myView.TimeDeltaComputation))
            {
                return false;
            }
    
            // For reuse of the time window it doesn't matter if it provides random access or not
            return myView.IsEmpty();
        }

        public string ViewName
        {
            get { return "Time"; }
        }

        private string ViewParamMessage
        {
            get { return ViewName + " view requires a single numeric or time period parameter"; }
        }
    }
}
