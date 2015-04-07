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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.methodagg
{
    [Serializable]
    public class ExprAvgNodeFactory : AggregationMethodFactory
	{
	    private readonly ExprAvgNode _parent;
	    private readonly Type _childType;
	    private readonly Type _resultType;

	    public ExprAvgNodeFactory(ExprAvgNode parent, Type childType, MethodResolutionService methodResolutionService)
	    {
	        this._parent = parent;
	        this._childType = childType;
	        this._resultType = methodResolutionService.GetAvgAggregatorType(childType);
	    }

	    public bool IsAccessAggregation
	    {
	        get { return false; }
	    }

	    public Type ResultType
	    {
	        get { return _resultType; }
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
	        AggregationMethod method = methodResolutionService.MakeAvgAggregator(agentInstanceId, groupId, aggregationId, _childType, _parent.HasFilter);
	        if (!_parent.IsDistinct) {
	            return method;
	        }
	        return methodResolutionService.MakeDistinctAggregator(agentInstanceId, groupId, aggregationId, method, _childType, _parent.HasFilter);
	    }

	    public ExprAggregateNodeBase AggregationExpression
	    {
	        get { return _parent; }
	    }

	    public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
	        AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
	        ExprAvgNodeFactory that = (ExprAvgNodeFactory) intoTableAgg;
	        AggregationMethodFactoryUtil.ValidateAggregationInputType(_childType, that._childType);
	        AggregationMethodFactoryUtil.ValidateAggregationFilter(_parent.HasFilter, that._parent.HasFilter);
	    }

	    public AggregationAgent AggregationStateAgent
	    {
	        get { return null; }
	    }

        public ExprEvaluator GetMethodAggregationEvaluator(Boolean join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_parent.PositionalParams, join, typesPerStream);
        }
	}
} // end of namespace
