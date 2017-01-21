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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.agg.factory
{
	public class AggregationMethodFactoryMinMax : AggregationMethodFactory
	{
        protected internal readonly ExprMinMaxAggrNode Parent;
        protected internal readonly Type Type;
        protected internal readonly bool HasDataWindows;

	    public AggregationMethodFactoryMinMax(ExprMinMaxAggrNode parent, Type type, bool hasDataWindows)
        {
	        Parent = parent;
	        Type = type;
	        HasDataWindows = hasDataWindows;
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

	    public Type ResultType
	    {
	        get { return Type; }
	    }

	    public AggregationMethod Make()
        {
	        AggregationMethod method = MakeMinMaxAggregator(Parent.MinMaxTypeEnum, Type, HasDataWindows, Parent.HasFilter);
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
	        AggregationMethodFactoryMinMax that = (AggregationMethodFactoryMinMax) intoTableAgg;
	        service.AggregationMethodFactoryUtil.ValidateAggregationInputType(Type, that.Type);
	        service.AggregationMethodFactoryUtil.ValidateAggregationFilter(Parent.HasFilter, that.Parent.HasFilter);
	        if (Parent.MinMaxTypeEnum != that.Parent.MinMaxTypeEnum)
            {
	            throw new ExprValidationException("The aggregation declares " +
	                    Parent.MinMaxTypeEnum.GetExpressionText() +
	                    " and provided is " +
	                    that.Parent.MinMaxTypeEnum.GetExpressionText());
	        }
	        service.AggregationMethodFactoryUtil.ValidateAggregationUnbound(HasDataWindows, that.HasDataWindows);
	    }

	    public AggregationAgent AggregationStateAgent
	    {
	        get { return null; }
	    }

	    public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
	        return ExprMethodAggUtil.GetDefaultEvaluator(Parent.PositionalParams, join, typesPerStream);
	    }

	    private AggregationMethod MakeMinMaxAggregator(
	        MinMaxTypeEnum minMaxTypeEnum,
	        Type targetType,
	        bool isHasDataWindows,
	        bool hasFilter)
	    {
	        if (!hasFilter)
	        {
	            if (!isHasDataWindows)
	            {
	                return new AggregatorMinMaxEver(minMaxTypeEnum);
	            }
	            return new AggregatorMinMax(minMaxTypeEnum);
	        }
	        else
	        {
	            if (!isHasDataWindows)
	            {
	                return new AggregatorMinMaxEverFilter(minMaxTypeEnum);
	            }
	            return new AggregatorMinMaxFilter(minMaxTypeEnum);
	        }
	    }
	}
} // end of namespace
