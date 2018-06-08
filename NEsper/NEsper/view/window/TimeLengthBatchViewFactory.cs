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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.view;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Factory for <seealso cref="com.espertech.esper.view.window.TimeLengthBatchView" />.
    /// </summary>
    public class TimeLengthBatchViewFactory
        : TimeBatchViewFactoryParams
        , DataWindowViewFactory
        , DataWindowViewWithPrevious
        , DataWindowBatchingViewFactory
    {
        /// <summary>Number of events to collect before batch fires.</summary>
        private ExprEvaluator _sizeEvaluator;

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            var validated = ViewFactorySupport.Validate(
                ViewName, viewFactoryContext.StatementContext, expressionParameters);
            var errorMessage = ViewName +
                                  " view requires a numeric or time period parameter as a time interval size, and an integer parameter as a maximal number-of-events, and an optional list of control keywords as a string parameter (please see the documentation)";
            if ((validated.Length != 2) && (validated.Length != 3))
            {
                throw new ViewParameterException(errorMessage);
            }

            timeDeltaComputationFactory = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName, viewFactoryContext.StatementContext, expressionParameters[0], errorMessage, 0);

            _sizeEvaluator = ViewFactorySupport.ValidateSizeParam(
                ViewName, viewFactoryContext.StatementContext, validated[1], 1);

            if (validated.Length > 2)
            {
                var keywords = ViewFactorySupport.Evaluate(
                    validated[2].ExprEvaluator, 2, ViewName, viewFactoryContext.StatementContext);
                ProcessKeywords(keywords, errorMessage);
            }
        }

        public void Attach(
            EventType parentEventType,
            StatementContext statementContext,
            ViewFactory optionalParentFactory,
            IList<ViewFactory> parentViewFactories)
        {
            eventType = parentEventType;
        }

        public Object MakePreviousGetter()
        {
            return new RelativeAccessByEventNIndexGetterImpl();
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var timeDeltaComputation = timeDeltaComputationFactory.Make(
                ViewName, "view", agentInstanceViewFactoryContext.AgentInstanceContext);
            var size = ViewFactorySupport.EvaluateSizeParam(
                ViewName, _sizeEvaluator, agentInstanceViewFactoryContext.AgentInstanceContext);
            var viewUpdatedCollection =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprRelativeAccess(agentInstanceViewFactoryContext);
            return new TimeLengthBatchView(
                this, agentInstanceViewFactoryContext, timeDeltaComputation, size, IsForceUpdate, IsStartEager,
                viewUpdatedCollection);
        }

        public EventType EventType => eventType;

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is TimeLengthBatchView))
            {
                return false;
            }

            var myView = (TimeLengthBatchView) view;
            var timeDeltaComputation = timeDeltaComputationFactory.Make(
                ViewName, "view", agentInstanceContext);
            if (!timeDeltaComputation.EqualsTimePeriod(myView.TimeDeltaComputation))
            {
                return false;
            }

            var size = ViewFactorySupport.EvaluateSizeParam(ViewName, _sizeEvaluator, agentInstanceContext);
            if (myView.NumberOfEvents != size)
            {
                return false;
            }

            if (myView.IsForceOutput != IsForceUpdate)
            {
                return false;
            }

            if (myView.IsStartEager)
            {
                // since it's already started
                return false;
            }

            return myView.IsEmpty();
        }

        public string ViewName => "TimeInMillis-Length-Batch";
    }
} // end of namespace
