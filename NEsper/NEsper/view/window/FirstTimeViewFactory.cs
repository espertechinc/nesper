///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
    /// Factory for <seealso cref="FirstTimeView"/>.
    /// </summary>
    public class FirstTimeViewFactory : AsymetricDataWindowViewFactory, DataWindowBatchingViewFactory
    {
        private EventType _eventType;
    
        /// <summary>Number of msec before expiry. </summary>
        private ExprTimePeriodEvalDeltaConst _timeDeltaComputation;
    
        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            if (expressionParameters.Count != 1)
            {
                throw new ViewParameterException(ViewParamMessage);
            }
            _timeDeltaComputation = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDelta(ViewName, viewFactoryContext.StatementContext, expressionParameters[0], ViewParamMessage, 0);
        }
    
        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            _eventType = parentEventType;
        }
    
        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new FirstTimeView(this, agentInstanceViewFactoryContext, _timeDeltaComputation);
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view)
        {
            if (!(view is FirstTimeView))
            {
                return false;
            }
    
            var myView = (FirstTimeView) view;
            if (!_timeDeltaComputation.EqualsTimePeriod(myView.TimeDeltaComputation))
            {
                return false;
            }
    
            return myView.IsEmpty();
        }

        public string ViewName
        {
            get { return "First-Time"; }
        }

        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation
        {
            get { return _timeDeltaComputation; }
        }

        private string ViewParamMessage
        {
            get { return ViewName + " view requires a single numeric or time period parameter"; }
        }
    }
}
