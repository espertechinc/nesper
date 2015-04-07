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
    /// Factory for <seealso cref="com.espertech.esper.view.window.TimeAccumView"/>.
    /// </summary>
    public class TimeAccumViewFactory : DataWindowViewFactory, DataWindowViewWithPrevious
    {
        private EventType _eventType;
    
        /// <summary>Number of msec of quiet time before results are flushed. </summary>
        private ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
    
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
            IStreamRandomAccess randomAccess = ViewServiceHelper.GetOptPreviousExprRandomAccess(agentInstanceViewFactoryContext);
            if (agentInstanceViewFactoryContext.IsRemoveStream)
            {
                return new TimeAccumViewRStream(this, agentInstanceViewFactoryContext, _timeDeltaComputation);
            }
            else
            {
                return new TimeAccumView(this, agentInstanceViewFactoryContext, _timeDeltaComputation, randomAccess);
            }
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view)
        {
            if (!(view is TimeAccumView))
            {
                return false;
            }
    
            var myView = (TimeAccumView) view;
            if (!_timeDeltaComputation.EqualsTimePeriod(myView.TimeDeltaComputation))
            {
                return false;
            }
    
            return myView.IsEmpty();
        }

        public string ViewName
        {
            get { return "Time-Accumulative-Batch"; }
        }

        private string ViewParamMessage
        {
            get { return ViewName + " view requires a single numeric parameter or time period parameter"; }
        }
    }
}
