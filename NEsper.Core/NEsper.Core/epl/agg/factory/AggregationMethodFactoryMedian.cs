///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;

namespace com.espertech.esper.epl.agg.factory
{
	public class AggregationMethodFactoryMedian : AggregationMethodFactory
	{
        protected internal readonly ExprMedianNode Parent;
        protected internal readonly Type AggregatedValueType;

	    public AggregationMethodFactoryMedian(ExprMedianNode parent, Type aggregatedValueType)
	    {
	        Parent = parent;
	        AggregatedValueType = aggregatedValueType;
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

	    public AggregationMethod Make()
        {
	        AggregationMethod method = MakeMedianAggregator(Parent.HasFilter);
	        if (!Parent.IsDistinct) {
	            return method;
	        }
	        return AggregationMethodFactoryUtil.MakeDistinctAggregator(method, Parent.HasFilter);
	    }

	    public ExprAggregateNodeBase AggregationExpression
	    {
	        get { return Parent; }
	    }

	    public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
	        service.AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
	        AggregationMethodFactoryMedian that = (AggregationMethodFactoryMedian) intoTableAgg;
	        service.AggregationMethodFactoryUtil.ValidateAggregationInputType(AggregatedValueType, that.AggregatedValueType);
	        service.AggregationMethodFactoryUtil.ValidateAggregationFilter(Parent.HasFilter, that.Parent.HasFilter);
	    }

	    public AggregationAgent AggregationStateAgent
	    {
	        get { return null; }
	    }

	    public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
	        return ExprMethodAggUtil.GetDefaultEvaluator(Parent.PositionalParams, join, typesPerStream);
	    }

	    private AggregationMethod MakeMedianAggregator(bool hasFilter)
	    {
	        if (!hasFilter) {
	            return new AggregatorMedian();
	        }
	        return new AggregatorMedianFilter();
	    }
	}
} // end of namespace
