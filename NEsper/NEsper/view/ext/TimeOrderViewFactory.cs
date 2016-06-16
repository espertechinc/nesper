///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.util;
using com.espertech.esper.view.window;

namespace com.espertech.esper.view.ext
{
	/// <summary>
	/// Factory for views for time-ordering events.
	/// </summary>
	public class TimeOrderViewFactory
        : DataWindowViewFactory
        , DataWindowViewWithPrevious
	{
	    private IList<ExprNode> _viewParameters;

	    /// <summary>
	    /// The timestamp expression.
	    /// </summary>
	    protected ExprNode timestampExpression;

	    public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
	    {
	        _viewParameters = expressionParameters;
	    }

	    public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
	    {
	        var validated = ViewFactorySupport.Validate(ViewName, parentEventType, statementContext, _viewParameters, true);

	        if (_viewParameters.Count != 2) {
	            throw new ViewParameterException(ViewParamMessage);
	        }

	        if (!validated[0].ExprEvaluator.ReturnType.IsNumeric()) {
	            throw new ViewParameterException(ViewParamMessage);
	        }
	        timestampExpression = validated[0];
	        TimeDeltaComputation = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDelta(ViewName, statementContext, _viewParameters[1], ViewParamMessage, 1);
	        TimestampExpressionEvaluator = timestampExpression.ExprEvaluator;
	        EventType = parentEventType;
	    }

	    public object MakePreviousGetter()
        {
	        return new RandomAccessByIndexGetter();
	    }

	    public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
	    {
	        var sortedRandomAccess = agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory.GetOptPreviousExprSortedRankedAccess(agentInstanceViewFactoryContext);
	        return new TimeOrderView(agentInstanceViewFactoryContext, this, timestampExpression, timestampExpression.ExprEvaluator, TimeDeltaComputation, sortedRandomAccess);
	    }

	    public EventType EventType { get; private set; }

	    public bool CanReuse(View view)
	    {
	        if (!(view is TimeOrderView))
	        {
	            return false;
	        }

	        var other = (TimeOrderView) view;
	        if ((!TimeDeltaComputation.EqualsTimePeriod(other.TimeDeltaComputation)) ||
	            (!ExprNodeUtility.DeepEquals(other.TimestampExpression, timestampExpression)))
	        {
	            return false;
	        }

	        return other.IsEmpty();
	    }

	    public string ViewName
	    {
	        get { return "Time-Order"; }
	    }

	    public ExprEvaluator TimestampExpressionEvaluator { get; protected set; }

	    public ExprTimePeriodEvalDeltaConst TimeDeltaComputation { get; protected set; }

	    private string ViewParamMessage
	    {
	        get { return ViewName + " view requires the expression supplying timestamp values, and a numeric or time period parameter for interval size"; }
	    }
	}
} // end of namespace
