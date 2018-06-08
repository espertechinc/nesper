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
    public class TimeToLiveViewFactory
        : DataWindowViewFactory
        , DataWindowViewWithPrevious
    {
        private IList<ExprNode> _viewParameters;

        /// <summary>The timestamp expression.</summary>
        private ExprNode _timestampExpression;

        private ExprEvaluator _timestampExpressionEvaluator;

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
            ExprNode[] validated = ViewFactorySupport.Validate(ViewName, parentEventType, statementContext, _viewParameters, true);

            if (_viewParameters.Count != 1)
            {
                throw new ViewParameterException(GetViewParamMessage());
            }
            if (validated[0].ExprEvaluator.ReturnType.GetBoxedType() != typeof(long?))
            {
                throw new ViewParameterException(GetViewParamMessage());
            }
            _timestampExpression = validated[0];
            _eventType = parentEventType;
            _timestampExpressionEvaluator = _timestampExpression.ExprEvaluator;
        }

        public Object MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            IStreamSortRankRandomAccess sortedRandomAccess = agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory.GetOptPreviousExprSortedRankedAccess(agentInstanceViewFactoryContext);
            return new TimeOrderView(agentInstanceViewFactoryContext, this, _timestampExpression, _timestampExpressionEvaluator, ExprTimePeriodEvalDeltaConstZero.INSTANCE, sortedRandomAccess);
        }

        public EventType EventType => _eventType;

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is TimeToLiveViewFactory))
            {
                return false;
            }
            var other = (TimeToLiveViewFactory)view;
            return ExprNodeUtility.DeepEquals(other.TimestampExpression, _timestampExpression, false);
        }

        public string ViewName => "Time-To-Live";

        public ExprNode TimestampExpression => _timestampExpression;

        public ExprEvaluator TimestampExpressionEvaluator => _timestampExpressionEvaluator;

        private string GetViewParamMessage()
        {
            return ViewName + " view requires a single expression supplying long-type timestamp values as a parameter";
        }
    }
} // end of namespace