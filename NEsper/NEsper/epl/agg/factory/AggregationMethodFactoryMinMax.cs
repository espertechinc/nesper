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
using com.espertech.esper.type;

namespace com.espertech.esper.epl.agg.factory
{
    public class AggregationMethodFactoryMinMax : AggregationMethodFactory
    {
        private readonly bool _hasDataWindows;
        private readonly ExprMinMaxAggrNode _parent;

        public AggregationMethodFactoryMinMax(ExprMinMaxAggrNode parent, Type type, bool hasDataWindows)
        {
            _parent = parent;
            ResultType = type;
            _hasDataWindows = hasDataWindows;
        }

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

        public Type ResultType { get; }

        public AggregationMethod Make()
        {
            var method = MakeMinMaxAggregator(_parent.MinMaxTypeEnum, ResultType, _hasDataWindows, _parent.HasFilter);
            if (!_parent.IsDistinct) return method;
            return AggregationMethodFactoryUtil.MakeDistinctAggregator(method, _parent.HasFilter);
        }

        public ExprAggregateNodeBase AggregationExpression => _parent;

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationValidationUtil.ValidateAggregationType(this, intoTableAgg);
            var that = (AggregationMethodFactoryMinMax) intoTableAgg;
            AggregationValidationUtil.ValidateAggregationInputType(ResultType, that.ResultType);
            AggregationValidationUtil.ValidateAggregationFilter(_parent.HasFilter, that._parent.HasFilter);
            if (_parent.MinMaxTypeEnum != that._parent.MinMaxTypeEnum)
                throw new ExprValidationException(
                    "The aggregation declares " +
                    _parent.MinMaxTypeEnum.GetExpressionText() +
                    " and provided is " +
                    that._parent.MinMaxTypeEnum.GetExpressionText());
            AggregationValidationUtil.ValidateAggregationUnbound(_hasDataWindows, that._hasDataWindows);
        }

        public AggregationAgent AggregationStateAgent => null;

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_parent.PositionalParams, join, typesPerStream);
        }

        private AggregationMethod MakeMinMaxAggregator(
            MinMaxTypeEnum minMaxTypeEnum, Type targetType,
            bool isHasDataWindows, bool hasFilter)
        {
            if (!hasFilter) {
                if (!isHasDataWindows) {
                    return new AggregatorMinMaxEver(minMaxTypeEnum);
                }
                return new AggregatorMinMax(minMaxTypeEnum);
            }

            if (!isHasDataWindows) {
                return new AggregatorMinMaxEverFilter(minMaxTypeEnum);
            }
            return new AggregatorMinMaxFilter(minMaxTypeEnum);
        }
    }
} // end of namespace