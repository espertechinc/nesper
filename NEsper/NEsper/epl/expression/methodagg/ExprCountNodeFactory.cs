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
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.methodagg
{
    [Serializable]
    public class ExprCountNodeFactory : AggregationMethodFactory
	{
	    private readonly ExprCountNode _parent;
	    private readonly bool _ignoreNulls;
	    private readonly Type _countedValueType;

	    public ExprCountNodeFactory(ExprCountNode parent, bool ignoreNulls, Type countedValueType)
	    {
	        _parent = parent;
	        _ignoreNulls = ignoreNulls;
	        _countedValueType = countedValueType;
	    }

	    public bool IsAccessAggregation
	    {
	        get { return false; }
	    }

	    public Type ResultType
	    {
	        get { return typeof (long?); }
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
	        AggregationMethod method = methodResolutionService.MakeCountAggregator(agentInstanceId, groupId, aggregationId, _ignoreNulls, _parent.HasFilter);
	        if (!_parent.IsDistinct) {
	            return method;
	        }
	        return methodResolutionService.MakeDistinctAggregator(agentInstanceId, groupId, aggregationId, method, _countedValueType, _parent.HasFilter);
	    }

	    public ExprAggregateNodeBase AggregationExpression
	    {
	        get { return _parent; }
	    }

	    public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg) {
	        AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
	        ExprCountNodeFactory that = (ExprCountNodeFactory) intoTableAgg;
	        AggregationMethodFactoryUtil.ValidateAggregationFilter(_parent.HasFilter, that._parent.HasFilter);
	        if (_parent.IsDistinct) {
	            AggregationMethodFactoryUtil.ValidateAggregationInputType(_countedValueType, that._countedValueType);
	        }
	        if (_ignoreNulls != that._ignoreNulls) {
	            throw new ExprValidationException("The aggregation declares" +
	                    (_ignoreNulls ? "" : " no") +
	                    " ignore nulls and provided is" +
	                    (that._ignoreNulls ? "" : " no") +
	                    " ignore nulls");
	        }
	    }

	    public AggregationAgent AggregationStateAgent
	    {
	        get { return null; }
	    }

        public ExprEvaluator GetMethodAggregationEvaluator(Boolean join, EventType[] typesPerStream)
        {
            return GetMethodAggregationEvaluatorCountBy(_parent.PositionalParams, join, typesPerStream);
        }

        public static ExprEvaluator GetMethodAggregationEvaluatorCountBy(ExprNode[] childNodes, Boolean join, EventType[] typesPerStream)
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
	}
} // end of namespace
