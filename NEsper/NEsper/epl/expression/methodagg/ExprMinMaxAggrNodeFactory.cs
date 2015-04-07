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
using com.espertech.esper.type;

namespace com.espertech.esper.epl.expression.methodagg
{
    [Serializable]
    public class ExprMinMaxAggrNodeFactory : AggregationMethodFactory
	{
	    private readonly ExprMinMaxAggrNode _parent;
	    private readonly Type _type;
	    private readonly bool _hasDataWindows;

	    public ExprMinMaxAggrNodeFactory(ExprMinMaxAggrNode parent, Type type, bool hasDataWindows)
        {
	        _parent = parent;
	        _type = type;
	        _hasDataWindows = hasDataWindows;
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
            get { return _type; }
        }

        public AggregationMethod Make(MethodResolutionService methodResolutionService, int agentInstanceId, int groupId, int aggregationId)
        {
	        var method = methodResolutionService.MakeMinMaxAggregator(agentInstanceId, groupId, aggregationId, _parent.MinMaxTypeEnum, _type, _hasDataWindows, _parent.HasFilter);
	        if (!_parent.IsDistinct)
            {
	            return method;
	        }
	        return methodResolutionService.MakeDistinctAggregator(agentInstanceId, groupId, aggregationId, method, _type, _parent.HasFilter);
	    }

        public ExprAggregateNodeBase AggregationExpression
        {
            get { return _parent; }
        }

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
	        AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
	        var that = (ExprMinMaxAggrNodeFactory) intoTableAgg;
	        AggregationMethodFactoryUtil.ValidateAggregationInputType(_type, that._type);
	        AggregationMethodFactoryUtil.ValidateAggregationFilter(_parent.HasFilter, that._parent.HasFilter);
	        if (_parent.MinMaxTypeEnum != that._parent.MinMaxTypeEnum)
	        {
	            throw new ExprValidationException(
	                string.Format(
	                    "The aggregation declares {0} and provided is {1}",
                        this._parent.MinMaxTypeEnum.GetExpressionText(),
	                    that._parent.MinMaxTypeEnum.GetExpressionText()));
	        }
	        AggregationMethodFactoryUtil.ValidateAggregationUnbound(_hasDataWindows, that._hasDataWindows);
	    }

        public AggregationAgent AggregationStateAgent
        {
            get { return null; }
        }

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_parent.PositionalParams, join, typesPerStream);
        }
	}
} // end of namespace
