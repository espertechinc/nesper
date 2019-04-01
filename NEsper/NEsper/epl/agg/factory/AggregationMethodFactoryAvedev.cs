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
        private readonly Type _aggregatedValueType;
        private readonly ExprAvedevNode _parent;
        private readonly ExprNode[] _positionalParameters;

        public AggregationMethodFactoryAvedev(ExprAvedevNode parent, Type aggregatedValueType,
            ExprNode[] positionalParameters)
        {
            _parent = parent;
            _aggregatedValueType = aggregatedValueType;
            _positionalParameters = positionalParameters;
        }

        public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            throw new IllegalStateException("Not an access aggregation function");
        }

        public AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize)
        {
            throw new IllegalStateException("Not an access aggregation function");
        }

        public AggregationMethod Make()
        {
            var method = MakeAvedevAggregator(_parent.HasFilter);
            if (!_parent.IsDistinct) return method;
            return AggregationMethodFactoryUtil.MakeDistinctAggregator(method, _parent.HasFilter);
        }

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationValidationUtil.ValidateAggregationType(this, intoTableAgg);
            var that = (AggregationMethodFactoryAvedev) intoTableAgg;
            AggregationValidationUtil.ValidateAggregationInputType(_aggregatedValueType, that._aggregatedValueType);
            AggregationValidationUtil.ValidateAggregationFilter(_parent.HasFilter, that._parent.HasFilter);
        }

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_positionalParameters, join, typesPerStream);
        }

        public bool IsAccessAggregation => false;

        public Type ResultType => typeof(double);

        public AggregationAccessor Accessor => throw new IllegalStateException("Not an access aggregation function");

        public ExprAggregateNodeBase AggregationExpression => _parent;

        public AggregationAgent AggregationStateAgent => null;

        private AggregationMethod MakeAvedevAggregator(bool hasFilter)
        {
            if (!hasFilter)
                return new AggregatorAvedev();
            return new AggregatorAvedevFilter();
        }
    }
} // end of namespace