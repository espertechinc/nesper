///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;

namespace com.espertech.esper.epl.agg.factory
{
    public class AggregationMethodFactoryPlugIn : AggregationMethodFactory
    {
        private readonly Type _aggregatedValueType;
        private readonly AggregationFunctionFactory _aggregationFunctionFactory;
        private readonly ExprPlugInAggNode _parent;

        public AggregationMethodFactoryPlugIn(ExprPlugInAggNode parent,
            AggregationFunctionFactory aggregationFunctionFactory, Type aggregatedValueType)
        {
            _parent = parent;
            _aggregationFunctionFactory = aggregationFunctionFactory;
            _aggregatedValueType = aggregatedValueType;
        }

        public Type ResultType => _aggregationFunctionFactory.ValueType;

        public bool IsAccessAggregation => false;

        public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            throw new IllegalStateException("Not an access aggregation function");
        }

        public AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize)
        {
            throw new IllegalStateException("Not an access aggregation function");
        }

        public AggregationAccessor Accessor => throw new IllegalStateException("Not an access aggregation function");

        public AggregationMethod Make()
        {
            var method = _aggregationFunctionFactory.NewAggregator();
            if (!_parent.IsDistinct) return method;
            return AggregationMethodFactoryUtil.MakeDistinctAggregator(method, false);
        }

        public ExprAggregateNodeBase AggregationExpression => _parent;

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationValidationUtil.ValidateAggregationType(this, intoTableAgg);
        }

        public AggregationAgent AggregationStateAgent => null;

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_parent.PositionalParams, join, typesPerStream);
        }
    }
} // end of namespace