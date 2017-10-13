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
	public class AggregationMethodFactoryAvedev : AggregationMethodFactory
	{
        protected internal readonly ExprAvedevNode Parent;
        protected internal readonly Type AggregatedValueType;
        protected internal readonly ExprNode[] PositionalParameters;

	    public AggregationMethodFactoryAvedev(ExprAvedevNode parent, Type aggregatedValueType, ExprNode[] positionalParameters)
        {
	        Parent = parent;
	        AggregatedValueType = aggregatedValueType;
	        PositionalParameters = positionalParameters;
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
	        AggregationMethod method = MakeAvedevAggregator(Parent.HasFilter);
	        if (!Parent.IsDistinct)
            {
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
	        AggregationMethodFactoryAvedev that = (AggregationMethodFactoryAvedev) intoTableAgg;
	        service.AggregationMethodFactoryUtil.ValidateAggregationInputType(AggregatedValueType, that.AggregatedValueType);
	        service.AggregationMethodFactoryUtil.ValidateAggregationFilter(Parent.HasFilter, that.Parent.HasFilter);
	    }

	    public AggregationAgent AggregationStateAgent
	    {
	        get { return null; }
	    }

	    public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
	        return ExprMethodAggUtil.GetDefaultEvaluator(PositionalParameters, join, typesPerStream);
	    }

	    private AggregationMethod MakeAvedevAggregator(bool hasFilter)
	    {
	        if (!hasFilter)
            {
	            return new AggregatorAvedev();
	        }
	        else
            {
	            return new AggregatorAvedevFilter();
	        }
	    }
	}
} // end of namespace
