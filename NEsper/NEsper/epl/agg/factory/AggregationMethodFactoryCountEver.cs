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
	public class AggregationMethodFactoryCountEver : AggregationMethodFactory
	{
        protected internal readonly ExprCountEverNode Parent;
        protected internal readonly bool IgnoreNulls;

	    public AggregationMethodFactoryCountEver(ExprCountEverNode parent, bool ignoreNulls)
        {
	        Parent = parent;
	        IgnoreNulls = ignoreNulls;
	    }

	    public bool IsAccessAggregation
	    {
	        get { return false; }
	    }

	    public Type ResultType
	    {
	        get { return typeof (long); }
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
	        return MakeCountEverValueAggregator(Parent.HasFilter, IgnoreNulls);
	    }

	    public ExprAggregateNodeBase AggregationExpression
	    {
	        get { return Parent; }
	    }

	    public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            service.AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
	        AggregationMethodFactoryCountEver that = (AggregationMethodFactoryCountEver) intoTableAgg;
	        if (that.IgnoreNulls != IgnoreNulls)
            {
	            throw new ExprValidationException("The aggregation declares " +
	                    (IgnoreNulls ? "ignore-nulls" : "no-ignore-nulls") +
	                    " and provided is " +
	                    (that.IgnoreNulls ? "ignore-nulls" : "no-ignore-nulls"));
	        }
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

	    private AggregationMethod MakeCountEverValueAggregator(bool hasFilter, bool ignoreNulls)
	    {
	        if (!hasFilter)
	        {
	            if (ignoreNulls)
	            {
	                return new AggregatorCountEverNonNull();
	            }
	            return new AggregatorCountEver();
	        }
	        else
	        {
	            if (ignoreNulls)
	            {
	                return new AggregatorCountEverNonNullFilter();
	            }
	            return new AggregatorCountEverFilter();
	        }
	    }
	}

} // end of namespace
