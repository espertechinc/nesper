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

namespace com.espertech.esper.epl.expression.methodagg
{
    [Serializable]
    public class ExprAvedevNodeFactory : AggregationMethodFactory
	{
	    private readonly ExprAvedevNode _parent;
	    private readonly Type _aggregatedValueType;
        private readonly ExprNode[] _positionalParameters;

        public ExprAvedevNodeFactory(ExprAvedevNode parent, Type aggregatedValueType, ExprNode[] positionalParameters)
        {
            _parent = parent;
            _aggregatedValueType = aggregatedValueType;
            _positionalParameters = positionalParameters;
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

	    public AggregationMethod Make(MethodResolutionService methodResolutionService, int agentInstanceId, int groupId, int aggregationId)
        {
	        AggregationMethod method = methodResolutionService.MakeAvedevAggregator(agentInstanceId, groupId, aggregationId, _parent.HasFilter);
	        if (!_parent.IsDistinct)
            {
	            return method;
	        }
	        return methodResolutionService.MakeDistinctAggregator(agentInstanceId, groupId, aggregationId, method, _aggregatedValueType, _parent.HasFilter);
	    }

	    public ExprAggregateNodeBase AggregationExpression
	    {
	        get { return _parent; }
	    }

	    public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg) {
	        AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
	        ExprAvedevNodeFactory that = (ExprAvedevNodeFactory) intoTableAgg;
	        AggregationMethodFactoryUtil.ValidateAggregationInputType(_aggregatedValueType, that._aggregatedValueType);
	        AggregationMethodFactoryUtil.ValidateAggregationFilter(_parent.HasFilter, that._parent.HasFilter);
	    }

	    public AggregationAgent AggregationStateAgent
	    {
	        get { return null; }
	    }

        public ExprEvaluator GetMethodAggregationEvaluator(Boolean join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_positionalParameters, join, typesPerStream);
        }
	}
} // end of namespace
