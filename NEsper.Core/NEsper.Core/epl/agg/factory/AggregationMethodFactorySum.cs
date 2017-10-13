///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.agg.factory
{
	public class AggregationMethodFactorySum : AggregationMethodFactory
	{
	    protected internal readonly ExprSumNode Parent;
        protected internal readonly Type InputValueType;

	    public AggregationMethodFactorySum(ExprSumNode parent, Type inputValueType)
	    {
	        Parent = parent;
	        InputValueType = inputValueType;
	        ResultType = GetSumAggregatorType(inputValueType);
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

	    public Type ResultType { get; private set; }

	    public AggregationMethod Make()
        {
	        AggregationMethod method = MakeSumAggregator(InputValueType, Parent.HasFilter);
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
	        AggregationMethodFactorySum that = (AggregationMethodFactorySum) intoTableAgg;
            service.AggregationMethodFactoryUtil.ValidateAggregationInputType(InputValueType, that.InputValueType);
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

	    private Type GetSumAggregatorType(Type type)
	    {
	        if (type.IsBigInteger())
	        {
	            return typeof(BigInteger);
	        }
	        if (type.IsDecimal())
	        {
	            return typeof(decimal?);
	        }
	        if ((type == typeof(long?)) || (type == typeof(long)))
	        {
	            return typeof(long?);
	        }
	        if ((type == typeof(int?)) || (type == typeof(int)))
	        {
	            return typeof(int?);
	        }
	        if ((type == typeof(double?)) || (type == typeof(double)))
	        {
	            return typeof(double?);
	        }
	        if ((type == typeof(float?)) || (type == typeof(float)))
	        {
	            return typeof(float?);
	        }
	        return typeof(int?);
	    }

	    private AggregationMethod MakeSumAggregator(Type type, bool hasFilter)
	    {
	        if (!hasFilter)
            {
                if (type.IsBigInteger())
                {
	                return new AggregatorSumBigInteger();
	            }
    	        if (type.IsDecimal())
	            {
	                return new AggregatorSumDecimal();
	            }
    	        if ((type == typeof(long?)) || (type == typeof(long)))
	            {
	                return new AggregatorSumLong();
	            }
    	        if ((type == typeof(int?)) || (type == typeof(int)))
	            {
	                return new AggregatorSumInteger();
	            }
	            if ((type == typeof(double?)) || (type == typeof(double)))
	            {
	                return new AggregatorSumDouble();
	            }
	            if ((type == typeof(float?)) || (type == typeof(float)))
	            {
	                return new AggregatorSumFloat();
	            }
	            return new AggregatorSumNumInteger();
	        }
	        else
            {
                if (type.IsBigInteger())
	            {
	                return new AggregatorSumBigIntegerFilter();
	            }
	            if (type.IsDecimal())
	            {
	                return new AggregatorSumDecimalFilter();
	            }
    	        if ((type == typeof(long?)) || (type == typeof(long)))
	            {
	                return new AggregatorSumLongFilter();
	            }
                if ((type == typeof(int?)) || (type == typeof(int)))
                {
	                return new AggregatorSumIntegerFilter();
	            }
	            if ((type == typeof(double?)) || (type == typeof(double)))
	            {
	                return new AggregatorSumDoubleFilter();
	            }
	            if ((type == typeof(float?)) || (type == typeof(float)))
	            {
	                return new AggregatorSumFloatFilter();
	            }
	            return new AggregatorSumNumIntegerFilter();
	        }
	    }
	}
} // end of namespace
