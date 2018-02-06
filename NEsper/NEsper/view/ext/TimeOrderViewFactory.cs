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
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;
using com.espertech.esper.view.window;

namespace com.espertech.esper.view.ext
{
    /// <summary>Factory for views for time-ordering events.</summary>
    public class TimeOrderViewFactory
        : DataWindowViewFactory
        , DataWindowViewWithPrevious
    {
        /// <summary>The timestamp expression.</summary>
        private ExprNode _timestampExpression;

        private ExprEvaluator _timestampExpressionEvaluator;

        /// <summary>The interval to wait for newer events to arrive.</summary>
        private ExprTimePeriodEvalDeltaConstFactory _timeDeltaComputationFactory;

        private IList<ExprNode> _viewParameters;
        private EventType _eventType;

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            _viewParameters = expressionParameters;
        }

        public void Attach(
            EventType parentEventType,
            StatementContext statementContext,
            ViewFactory optionalParentFactory,
            IList<ViewFactory> parentViewFactories)
        {
            var validated = ViewFactorySupport.Validate(
                ViewName, parentEventType, statementContext, _viewParameters, true);

            if (_viewParameters.Count != 2)
            {
                throw new ViewParameterException(ViewParamMessage);
            }

            if (!validated[0].ExprEvaluator.ReturnType.IsNumeric())
            {
                throw new ViewParameterException(ViewParamMessage);
            }
            _timestampExpression = validated[0];
            _timeDeltaComputationFactory = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName, statementContext, _viewParameters[1], ViewParamMessage, 1);
            _timestampExpressionEvaluator = _timestampExpression.ExprEvaluator;
            _eventType = parentEventType;
        }

        public Object MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var timeDeltaComputation = _timeDeltaComputationFactory.Make(
                ViewName, "view", agentInstanceViewFactoryContext.AgentInstanceContext);
            var sortedRandomAccess =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprSortedRankedAccess(agentInstanceViewFactoryContext);
            return new TimeOrderView(
                agentInstanceViewFactoryContext, this, _timestampExpression, _timestampExpression.ExprEvaluator,
                timeDeltaComputation, sortedRandomAccess);
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is TimeOrderView))
            {
                return false;
            }

            var other = (TimeOrderView) view;
            var timeDeltaComputation = _timeDeltaComputationFactory.Make(
                ViewName, "view", agentInstanceContext);
            if ((!timeDeltaComputation.EqualsTimePeriod(other.TimeDeltaComputation)) ||
                (!ExprNodeUtility.DeepEquals(other.TimestampExpression, _timestampExpression, false)))
            {
                return false;
            }

            return other.IsEmpty();
        }

        public string ViewName
        {
            get { return "Time-Order"; }
        }

        public ExprEvaluator TimestampExpressionEvaluator
        {
            get { return _timestampExpressionEvaluator; }
        }

        private string ViewParamMessage
        {
            get
            {
                return ViewName +
                       " view requires the expression supplying timestamp values, and a numeric or time period parameter for interval size";
            }
        }
    }
} // end of namespace
