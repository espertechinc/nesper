///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.view.window
{
    /// <summary>Factory for <seealso cref="TimeWindowView" />.</summary>
    public class TimeWindowViewFactory
        : DataWindowViewFactory
        , DataWindowViewWithPrevious
    {
        private EventType _eventType;
        private ExprTimePeriodEvalDeltaConstFactory _timeDeltaComputationFactory;

        public ExprTimePeriodEvalDeltaConstFactory TimeDeltaComputationFactory => _timeDeltaComputationFactory;

        private string ViewParamMessage => ViewName + " view requires a single numeric or time period parameter";

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            ExprTimePeriodEvalDeltaConst timeDeltaComputation = _timeDeltaComputationFactory.Make(
                ViewName, "view", agentInstanceViewFactoryContext.AgentInstanceContext);
            ViewUpdatedCollection randomAccess =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprRandomAccess(agentInstanceViewFactoryContext);
            return new TimeWindowView(agentInstanceViewFactoryContext, this, timeDeltaComputation, randomAccess);
        }

        public EventType EventType => _eventType;

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is TimeWindowView))
            {
                return false;
            }

            var myView = (TimeWindowView) view;
            ExprTimePeriodEvalDeltaConst delta = _timeDeltaComputationFactory.Make(
                ViewName, "view", agentInstanceContext);
            if (!delta.EqualsTimePeriod(myView.TimeDeltaComputation))
            {
                return false;
            }

            // For reuse of the time window it doesn't matter if it provides random access or not
            return myView.IsEmpty();
        }

        public string ViewName => "Time";

        public Object MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }

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
    }
} // end of namespace