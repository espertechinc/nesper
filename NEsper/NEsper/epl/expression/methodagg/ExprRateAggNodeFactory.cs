///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.methodagg
{
	public class ExprRateAggNodeFactory : AggregationMethodFactory
	{
	    private readonly ExprRateAggNode _parent;
	    private readonly bool _isEver;
	    private readonly long _intervalMSec;

	    public ExprRateAggNodeFactory(ExprRateAggNode parent, bool isEver, long intervalMSec)
	    {
	        _parent = parent;
	        _isEver = isEver;
	        _intervalMSec = intervalMSec;
	    }

	    public bool IsAccessAggregation
	    {
	        get { return false; }
	    }

	    public Type ResultType
	    {
	        get { return typeof (double?); }
	    }

	    public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
	        throw new IllegalStateException("Not an access aggregation function");
	    }

	    public AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize)
        {
	        throw new IllegalStateException("Not an access aggregation function");
	    }

	    public AggregationAccessor Accessor
	    {
	        get { throw new IllegalStateException("Not an access aggregation function"); }
	    }

	    public AggregationMethod Make(MethodResolutionService methodResolutionService, int agentInstanceId, int groupId, int aggregationId)
        {
	        if (_isEver) {
	            return methodResolutionService.MakeRateEverAggregator(agentInstanceId, groupId, aggregationId, _intervalMSec);
	        }
	        else {
	            return methodResolutionService.MakeRateAggregator(agentInstanceId, groupId, aggregationId);
	        }
	    }

	    public ExprAggregateNodeBase AggregationExpression
	    {
	        get { return _parent; }
	    }

	    public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
	        AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
	        ExprRateAggNodeFactory that = (ExprRateAggNodeFactory) intoTableAgg;
	        if (_intervalMSec != that._intervalMSec) {
	            throw new ExprValidationException("The size is " +
	                    _intervalMSec +
	                    " and provided is " +
	                    that._intervalMSec);
	        }
	        AggregationMethodFactoryUtil.ValidateAggregationUnbound(!_isEver, !that._isEver);
	    }

	    public AggregationAgent AggregationStateAgent
	    {
	        get { return null; }
	    }

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_parent.PositionalParams, join, typesPerStream);
        }
	}
} // end of namespace
