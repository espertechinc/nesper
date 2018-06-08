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
        /// <summary>The timestamp property name.</summary>
        private ExprNode _timestampExpression;
        private ExprEvaluator _timestampExpressionEval;
        private long? _optionalReferencePoint;
        /// <summary>The number of msec to expire.</summary>
        private ExprTimePeriodEvalDeltaConstFactory _timeDeltaComputationFactory;
        private IList<ExprNode> _viewParameters;
        private EventType _eventType;
    
        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            _viewParameters = expressionParameters;
        }

        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            var windowName = ViewName;
            var validated = ViewFactorySupport.Validate(windowName, parentEventType, statementContext, _viewParameters, true);
            if (_viewParameters.Count < 2 || _viewParameters.Count > 3) {
                throw new ViewParameterException(ViewParamMessage);
            }
    
            // validate first parameter: timestamp expression
            if (!validated[0].ExprEvaluator.ReturnType.IsNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }
            _timestampExpression = validated[0];
            _timestampExpressionEval = _timestampExpression.ExprEvaluator;
            ViewFactorySupport.AssertReturnsNonConstant(windowName, validated[0], 0);
    
            _timeDeltaComputationFactory = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(ViewName, statementContext, _viewParameters[1], ViewParamMessage, 1);
    
            // validate optional parameters
            if (validated.Length == 3) {
                var constant = ViewFactorySupport.ValidateAndEvaluate(windowName, statementContext, validated[2]);
                if ((!constant.IsNumber()) || constant.IsFloatingPointNumber()) {
                    throw new ViewParameterException("Externally-timed batch view requires a long-typed reference point in msec as a third parameter");
                }
                _optionalReferencePoint = constant.AsLong();
            }
    
            this._eventType = parentEventType;
        }
    
        public Object MakePreviousGetter()
        {
            return new RelativeAccessByEventNIndexGetterImpl();
        }
    
        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext) {
            var timeDeltaComputation = _timeDeltaComputationFactory.Make(ViewName, "view", agentInstanceViewFactoryContext.AgentInstanceContext);
            var viewUpdatedCollection = agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory.GetOptPreviousExprRelativeAccess(agentInstanceViewFactoryContext);
            return new ExternallyTimedBatchView(this, _timestampExpression, _timestampExpressionEval, timeDeltaComputation, _optionalReferencePoint, viewUpdatedCollection, agentInstanceViewFactoryContext);
        }

        public EventType EventType => _eventType;

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is ExternallyTimedBatchView))
            {
                return false;
            }

            var myView = (ExternallyTimedBatchView) view;
            var delta = _timeDeltaComputationFactory.Make(ViewName, "view", agentInstanceContext);
            if ((!delta.EqualsTimePeriod(myView.GetTimeDeltaComputation())) ||
                (!ExprNodeUtility.DeepEquals(myView.TimestampExpression, _timestampExpression, false)))
            {
                return false;
            }
            return myView.IsEmpty();
        }

        public string ViewName => "Externally-timed-batch";

        public ExprEvaluator TimestampExpressionEval => _timestampExpressionEval;

        public long? OptionalReferencePoint => _optionalReferencePoint;

        private string ViewParamMessage => ViewName +
                                           " view requires a timestamp expression and a numeric or time period parameter for window size and an optional long-typed reference point in msec, and an optional list of control keywords as a string parameter (please see the documentation)";
    }
} // end of namespace
