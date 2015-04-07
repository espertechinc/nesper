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
using com.espertech.esper.util;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Factory for <seealso cref="ExternallyTimedWindowView" />.
    /// </summary>
    public class ExternallyTimedWindowViewFactory
        : DataWindowViewFactory
        , DataWindowViewWithPrevious
    {
        private IList<ExprNode> _viewParameters;
    
        private EventType _eventType;
    
        /// <summary>
        /// The timestamp property name.
        /// </summary>
        internal ExprNode TimestampExpression;
        internal ExprEvaluator TimestampExpressionEval;
    
        /// <summary>
        /// The number of msec to expire.
        /// </summary>
        internal ExprTimePeriodEvalDeltaConst TimeDeltaComputation;
    
        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters) 
        {
            _viewParameters = expressionParameters;
        }
    
        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories) 
        {
            ExprNode[] validated = ViewFactorySupport.Validate(ViewName, parentEventType, statementContext, _viewParameters, true);
            if (_viewParameters.Count != 2) {
                throw new ViewParameterException(ViewParamMessage);
            }
    
            if (!validated[0].ExprEvaluator.ReturnType.IsNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }
            TimestampExpression = validated[0];
            TimestampExpressionEval = TimestampExpression.ExprEvaluator;
            ViewFactorySupport.AssertReturnsNonConstant(ViewName, validated[0], 0);
    
            TimeDeltaComputation = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDelta(ViewName, statementContext, _viewParameters[1], ViewParamMessage, 1);
            _eventType = parentEventType;
        }
    
        public Object MakePreviousGetter() {
            return new RandomAccessByIndexGetter();
        }
    
        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            IStreamRandomAccess randomAccess = ViewServiceHelper.GetOptPreviousExprRandomAccess(agentInstanceViewFactoryContext);
            return new ExternallyTimedWindowView(this, TimestampExpression, TimestampExpressionEval, TimeDeltaComputation, randomAccess, agentInstanceViewFactoryContext);
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view)
        {
            if (!(view is ExternallyTimedWindowView))
            {
                return false;
            }
    
            var myView = (ExternallyTimedWindowView) view;
            if ((!TimeDeltaComputation.EqualsTimePeriod(myView.TimeDeltaComputation)) ||
                (!ExprNodeUtility.DeepEquals(myView.TimestampExpression, TimestampExpression)))
            {
                return false;
            }
            return myView.IsEmpty();
        }

        public string ViewName
        {
            get { return "Externally-timed"; }
        }

        private string ViewParamMessage
        {
            get
            {
                return ViewName + " view requires a timestamp expression and a numeric or time period parameter for window size";
            }
        }
    }
}
