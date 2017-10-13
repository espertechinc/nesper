///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;

namespace com.espertech.esper.epl.agg.factory
{
	public class AggregationMethodFactoryPlugIn : AggregationMethodFactory
	{
        protected internal readonly ExprPlugInAggNode Parent;
        protected internal readonly AggregationFunctionFactory AggregationFunctionFactory;
        protected internal readonly Type AggregatedValueType;

	    public AggregationMethodFactoryPlugIn(ExprPlugInAggNode parent, AggregationFunctionFactory aggregationFunctionFactory, Type aggregatedValueType)
        {
	        Parent = parent;
	        AggregationFunctionFactory = aggregationFunctionFactory;
	        AggregatedValueType = aggregatedValueType;
	    }

	    public Type ResultType
	    {
	        get { return AggregationFunctionFactory.ValueType; }
	    }

	    public bool IsAccessAggregation
	    {
	        get { return false; }
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

	    public AggregationMethod Make()
        {
	        AggregationMethod method = AggregationFunctionFactory.NewAggregator();
	        if (!Parent.IsDistinct)
            {
	            return method;
	        }
	        return AggregationMethodFactoryUtil.MakeDistinctAggregator(method, false);
	    }

	    public ExprAggregateNodeBase AggregationExpression
	    {
	        get { return Parent; }
	    }

	    public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
	        service.AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
	    }

	    public AggregationAgent AggregationStateAgent
	    {
	        get { return null; }
	    }

	    public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
	        return ExprMethodAggUtil.GetDefaultEvaluator(Parent.PositionalParams, join, typesPerStream);
	    }
	}
} // end of namespace
