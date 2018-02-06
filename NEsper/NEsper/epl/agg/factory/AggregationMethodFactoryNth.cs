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
    public class AggregationMethodFactoryNth : AggregationMethodFactory
    {
        private readonly ExprNthAggNode _parent;
        private readonly int _size;

        public AggregationMethodFactoryNth(ExprNthAggNode parent, Type childType, int size)
        {
            _parent = parent;
            ResultType = childType;
            _size = size;
        }

        public bool IsAccessAggregation => false;

        public Type ResultType { get; }

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
            AggregationMethod method;
            if (_parent.OptionalFilter != null)
                method = new AggregatorNthFilter(_size + 1);
            else
                method = new AggregatorNth(_size + 1);
            if (!_parent.IsDistinct)
                return method;
            return AggregationMethodFactoryUtil.MakeDistinctAggregator(method, false);
        }

        public ExprAggregateNodeBase AggregationExpression => _parent;

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationValidationUtil.ValidateAggregationType(this, intoTableAgg);
            var that = (AggregationMethodFactoryNth) intoTableAgg;
            AggregationValidationUtil.ValidateAggregationInputType(ResultType, that.ResultType);
            if (_size != that._size)
                throw new ExprValidationException(
                    "The size is " + _size + " and provided is " + that._size);
        }

        public AggregationAgent AggregationStateAgent => null;

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_parent.PositionalParams, join, typesPerStream);
        }
    }
} // end of namespace