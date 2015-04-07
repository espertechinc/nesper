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
    public class ExprFirstEverNodeFactory : AggregationMethodFactory
	{
	    private readonly ExprFirstEverNode _parent;
	    private readonly Type _childType;

	    public ExprFirstEverNodeFactory(ExprFirstEverNode parent, Type childType) {
	        this._parent = parent;
	        this._childType = childType;
	    }

	    public bool IsAccessAggregation
	    {
	        get { return false; }
	    }

	    public Type ResultType
	    {
	        get { return _childType; }
	    }

	    public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize) {
	        throw new IllegalStateException("Not an access aggregation function");
	    }

	    public AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize) {
	        throw new IllegalStateException("Not an access aggregation function");
	    }

	    public AggregationAccessor Accessor
	    {
	        get { throw new IllegalStateException("Not an access aggregation function"); }
	    }

	    public AggregationMethod Make(MethodResolutionService methodResolutionService, int agentInstanceId, int groupId, int aggregationId) {
	        return methodResolutionService.MakeFirstEverValueAggregator(agentInstanceId, groupId, aggregationId, _childType, _parent.HasFilter);
	    }

	    public ExprAggregateNodeBase AggregationExpression
	    {
	        get { return _parent; }
	    }

	    public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg) {
	        AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
	        ExprFirstEverNodeFactory that = (ExprFirstEverNodeFactory) intoTableAgg;
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
