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
	public class AggregationMethodFactoryCount : AggregationMethodFactory
	{
        protected internal readonly ExprCountNode Parent;
        protected internal readonly bool IgnoreNulls;
        protected internal readonly Type CountedValueType;

	    public AggregationMethodFactoryCount(ExprCountNode parent, bool ignoreNulls, Type countedValueType)
	    {
	        Parent = parent;
	        IgnoreNulls = ignoreNulls;
	        CountedValueType = countedValueType;
	    }

	    public bool IsAccessAggregation
	    {
	        get { return false; }
	    }

	    public Type ResultType
	    {
	        get { return typeof (long?); }
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
	        AggregationMethod method = MakeCountAggregator(IgnoreNulls, Parent.HasFilter);
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
	        AggregationMethodFactoryCount that = (AggregationMethodFactoryCount) intoTableAgg;
	        service.AggregationMethodFactoryUtil.ValidateAggregationFilter(Parent.HasFilter, that.Parent.HasFilter);
	        if (Parent.IsDistinct) {
	            service.AggregationMethodFactoryUtil.ValidateAggregationInputType(CountedValueType, that.CountedValueType);
	        }
	        if (IgnoreNulls != that.IgnoreNulls) {
	            throw new ExprValidationException("The aggregation declares" +
	                    (IgnoreNulls ? "" : " no") +
	                    " ignore nulls and provided is" +
	                    (that.IgnoreNulls ? "" : " no") +
	                    " ignore nulls");
	        }
	    }

	    public AggregationAgent AggregationStateAgent
	    {
	        get { return null; }
	    }

	    public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
	        return GetMethodAggregationEvaluatorCountBy(Parent.PositionalParams, join, typesPerStream);
	    }

	    public static ExprEvaluator GetMethodAggregationEvaluatorCountBy(ExprNode[] childNodes, bool join, EventType[] typesPerStream)
	    {
	        if (childNodes[0] is ExprWildcard && childNodes.Length == 2)
            {
	            return ExprMethodAggUtil.GetDefaultEvaluator(new ExprNode[] {childNodes[1]}, join, typesPerStream);
	        }
	        if (childNodes[0] is ExprWildcard && childNodes.Length == 1)
            {
	            return ExprMethodAggUtil.GetDefaultEvaluator(new ExprNode[0], join, typesPerStream);
	        }
	        return ExprMethodAggUtil.GetDefaultEvaluator(childNodes, join, typesPerStream);
	    }

	    private AggregationMethod MakeCountAggregator(bool isIgnoreNull, bool hasFilter)
	    {
	        if (!hasFilter)
            {
	            if (isIgnoreNull)
                {
	                return new AggregatorCountNonNull();
	            }
	            return new AggregatorCount();
	        }
	        else
            {
	            if (isIgnoreNull)
                {
	                return new AggregatorCountNonNullFilter();
	            }
	            return new AggregatorCountFilter();
	        }
	    }
	}
} // end of namespace
