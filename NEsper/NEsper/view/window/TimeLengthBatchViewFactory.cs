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
using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

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
        /// <summary>Number of events to collect before batch fires. </summary>
        private long _numberOfEvents;

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            var viewParameters = new Object[expressionParameters.Count];
            for (var i = 1; i < expressionParameters.Count; i++)
            {
                viewParameters[i] = ViewFactorySupport.ValidateAndEvaluate(
                    ViewName, viewFactoryContext.StatementContext, expressionParameters[i]);
            }
            var errorMessage = ViewName +
                                  " view requires a numeric or time period parameter as a time interval size, and an integer parameter as a maximal number-of-events, and an optional list of control keywords as a string parameter (please see the documentation)";
            if ((viewParameters.Length != 2) && (viewParameters.Length != 3))
            {
                throw new ViewParameterException(errorMessage);
            }

            TimeDeltaComputation = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDelta(
                ViewName, viewFactoryContext.StatementContext, expressionParameters[0], errorMessage, 0);

            // parameter 2
            var parameter = viewParameters[1];
            if (!(parameter.IsNumber()) || (TypeHelper.IsFloatingPointNumber(parameter)))
            {
                throw new ViewParameterException(errorMessage);
            }
            _numberOfEvents = parameter.AsLong();

            if (viewParameters.Length > 2)
            {
                ProcessKeywords(viewParameters[2], errorMessage);
            }
        }

        public void Attach(
            EventType parentEventType,
            StatementContext statementContext,
            ViewFactory optionalParentFactory,
            IList<ViewFactory> parentViewFactories)
        {
            base.EventType = parentEventType;
        }

        public Object MakePreviousGetter()
        {
            return new RelativeAccessByEventNIndexGetterImpl();
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var viewUpdatedCollection = agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory.GetOptPreviousExprRelativeAccess(agentInstanceViewFactoryContext);
            return new TimeLengthBatchView(this, agentInstanceViewFactoryContext, TimeDeltaComputation, _numberOfEvents, IsForceUpdate, IsStartEager, viewUpdatedCollection);
        }

        public bool CanReuse(View view)
        {
            if (!(view is TimeLengthBatchView))
            {
                return false;
            }

            var myView = (TimeLengthBatchView) view;

            if (!TimeDeltaComputation.EqualsTimePeriod(myView.TimeDeltaComputation))
            {
                return false;
            }

            if (myView.NumberOfEvents != _numberOfEvents)
            {
                return false;
            }

            if (myView.IsForceOutput != IsForceUpdate)
            {
                return false;
            }

            if (myView.IsStartEager) // since it's already started
            {
                return false;
            }

            return myView.IsEmpty();
        }

        public string ViewName
        {
            get { return "Time-Length-Batch"; }
        }

        public long NumberOfEvents
        {
            get { return _numberOfEvents; }
        }
    }
}
