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
    /// <summary>
    /// Factory for <seealso cref="com.espertech.esper.view.window.TimeAccumView" />.
    /// </summary>
    public class TimeAccumViewFactory
        : DataWindowViewFactory
        , DataWindowViewWithPrevious
    {
        /// <summary>Number of msec of quiet time before results are flushed.</summary>
        private ExprTimePeriodEvalDeltaConstFactory _timeDeltaComputationFactory;

        private EventType _eventType;

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

        public Object MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            ExprTimePeriodEvalDeltaConst timeDeltaComputation = _timeDeltaComputationFactory.Make(
                ViewName, "view", agentInstanceViewFactoryContext.AgentInstanceContext);
            ViewUpdatedCollection randomAccess =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprRandomAccess(agentInstanceViewFactoryContext);
            if (agentInstanceViewFactoryContext.IsRemoveStream)
            {
                return new TimeAccumViewRStream(this, agentInstanceViewFactoryContext, timeDeltaComputation);
            }
            else
            {
                return new TimeAccumView(this, agentInstanceViewFactoryContext, timeDeltaComputation, randomAccess);
            }
        }

        public EventType EventType => _eventType;

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is TimeAccumView))
            {
                return false;
            }

            var myView = (TimeAccumView) view;
            ExprTimePeriodEvalDeltaConst timeDeltaComputation = _timeDeltaComputationFactory.Make(
                ViewName, "view", agentInstanceContext);
            if (!timeDeltaComputation.EqualsTimePeriod(myView.TimeDeltaComputation))
            {
                return false;
            }

            return myView.IsEmpty;
        }

        public string ViewName => "TimeInMillis-Accumulative-Batch";

        private string ViewParamMessage => ViewName + " view requires a single numeric parameter or time period parameter";
    }
} // end of namespace
