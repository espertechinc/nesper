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
using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Factory for <seealso cref="com.espertech.esper.view.window.ExternallyTimedBatchView" />.
    /// </summary>
    public class ExternallyTimedBatchViewFactory 
        : DataWindowBatchingViewFactory
        , DataWindowViewFactory
        , DataWindowViewWithPrevious
    {
        private IList<ExprNode> _viewParameters;
    
        private EventType _eventType;
    
        /// <summary>
        /// The timestamp property name.
        /// </summary>
        internal ExprNode TimestampExpression;
        internal ExprEvaluator TimestampExpressionEval;
        internal long? OptionalReferencePoint;
    
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
            String windowName = ViewName;
            ExprNode[] validated = ViewFactorySupport.Validate(windowName, parentEventType, statementContext, _viewParameters, true);
            if (_viewParameters.Count < 2 || _viewParameters.Count > 3) {
                throw new ViewParameterException(ViewParamMessage);
            }
    
            // validate first parameter: timestamp expression
            if (!validated[0].ExprEvaluator.ReturnType.IsNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }
            TimestampExpression = validated[0];
            TimestampExpressionEval = TimestampExpression.ExprEvaluator;
            ViewFactorySupport.AssertReturnsNonConstant(windowName, validated[0], 0);
    
            TimeDeltaComputation = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDelta(ViewName, statementContext, _viewParameters[1], ViewParamMessage, 1);
    
            // validate optional parameters
            if (validated.Length == 3) {
                Object constant = ViewFactorySupport.ValidateAndEvaluate(windowName, statementContext, validated[2]);
                if ((!constant.IsNumber()) || (TypeHelper.IsFloatingPointNumber(constant)))
                {
                    throw new ViewParameterException("Externally-timed batch view requires a Long-typed reference point in msec as a third parameter");
                }
                OptionalReferencePoint = constant.AsLong();
            }
    
            this._eventType = parentEventType;
        }
    
        public Object MakePreviousGetter() {
            return new RelativeAccessByEventNIndexMap();
        }
    
        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            IStreamRelativeAccess relativeAccessByEvent = ViewServiceHelper.GetOptPreviousExprRelativeAccess(agentInstanceViewFactoryContext);
            return new ExternallyTimedBatchView(this, TimestampExpression, TimestampExpressionEval, TimeDeltaComputation, OptionalReferencePoint, relativeAccessByEvent, agentInstanceViewFactoryContext);
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view)
        {
            if (!(view is ExternallyTimedBatchView))
            {
                return false;
            }
    
            ExternallyTimedBatchView myView = (ExternallyTimedBatchView) view;
            if ((!TimeDeltaComputation.EqualsTimePeriod(myView.GetTimeDeltaComputation())) ||
                (!ExprNodeUtility.DeepEquals(myView.TimestampExpression, TimestampExpression)))
            {
                return false;
            }
            return myView.IsEmpty();
        }

        public string ViewName
        {
            get { return "Externally-timed-batch"; }
        }

        private string ViewParamMessage
        {
            get
            {
                return ViewName +
                       " view requires a timestamp expression and a numeric or time period parameter for window size and an optional long-typed reference point in msec, and an optional list of control keywords as a string parameter (please see the documentation)";
            }
        }
    }
}
