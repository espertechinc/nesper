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
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.view.window
{
    /// <summary>Factory for <seealso cref="FirstTimeView" />.</summary>
    public class FirstTimeViewFactory
        : AsymetricDataWindowViewFactory,
          DataWindowBatchingViewFactory
    {
        private EventType _eventType;

        /// <summary>Number of msec before expiry.</summary>
        private ExprTimePeriodEvalDeltaConstFactory _timeDeltaComputationFactory;

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            if (expressionParameters.Count != 1)
            {
                throw new ViewParameterException(ViewParamMessage);
            }
            _timeDeltaComputationFactory = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName, viewFactoryContext.StatementContext, expressionParameters[0], ViewParamMessage, 0);
        }

        public void Attach(
            EventType parentEventType,
            StatementContext statementContext,
            ViewFactory optionalParentFactory,
            IList<ViewFactory> parentViewFactories)
        {
            _eventType = parentEventType;
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            ExprTimePeriodEvalDeltaConst timeDeltaComputation = _timeDeltaComputationFactory.Make(
                ViewName, "view", agentInstanceViewFactoryContext.AgentInstanceContext);
            return new FirstTimeView(this, agentInstanceViewFactoryContext, timeDeltaComputation);
        }

        public EventType EventType => _eventType;

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is FirstTimeView))
            {
                return false;
            }

            var myView = (FirstTimeView) view;
            ExprTimePeriodEvalDeltaConst delta = _timeDeltaComputationFactory.Make(
                ViewName, "view", agentInstanceContext);
            if (!delta.EqualsTimePeriod(myView.TimeDeltaComputation))
            {
                return false;
            }
            return myView.IsEmpty();
        }

        public string ViewName => "First-TimeInMillis";

        private string ViewParamMessage => ViewName + " view requires a single numeric or time period parameter";
    }
} // end of namespace