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
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.agg.factory
{
	public class AggregationMethodFactoryRate : AggregationMethodFactory
	{
        protected internal readonly ExprRateAggNode Parent;
        protected internal readonly bool IsEver;
        protected internal readonly long IntervalMSec;
        protected internal readonly TimeProvider TimeProvider;

	    public AggregationMethodFactoryRate(ExprRateAggNode parent, bool isEver, long intervalMSec, TimeProvider timeProvider)
	    {
	        Parent = parent;
	        IsEver = isEver;
	        IntervalMSec = intervalMSec;
	        TimeProvider = timeProvider;
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
	        if (IsEver)
	        {
	            return new AggregatorRateEver(IntervalMSec, TimeProvider);
	        }
	        else
	        {
	            return new AggregatorRate();
	        }
	    }

	    public ExprAggregateNodeBase AggregationExpression
	    {
	        get { return Parent; }
	    }

	    public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
	        service.AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
	        AggregationMethodFactoryRate that = (AggregationMethodFactoryRate) intoTableAgg;
	        if (IntervalMSec != that.IntervalMSec)
            {
	            throw new ExprValidationException(string.Format("The size is {0} and provided is {1}", IntervalMSec, that.IntervalMSec));
	        }
            service.AggregationMethodFactoryUtil.ValidateAggregationUnbound(!IsEver, !that.IsEver);
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
