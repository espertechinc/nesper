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
    public class AggregationMethodFactoryCount : AggregationMethodFactory
    {
        private readonly Type _countedValueType;
        private readonly bool _ignoreNulls;
        private readonly ExprCountNode _parent;

        public AggregationMethodFactoryCount(ExprCountNode parent, bool ignoreNulls, Type countedValueType)
        {
            _parent = parent;
            _ignoreNulls = ignoreNulls;
            _countedValueType = countedValueType;
        }

        public bool IsAccessAggregation => false;

        public Type ResultType => typeof(long);

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
            var method = MakeCountAggregator(_ignoreNulls, _parent.HasFilter);
            if (!_parent.IsDistinct) return method;
            return AggregationMethodFactoryUtil.MakeDistinctAggregator(method, _parent.HasFilter);
        }

        public ExprAggregateNodeBase AggregationExpression => _parent;

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationValidationUtil.ValidateAggregationType(this, intoTableAgg);
            var that = (AggregationMethodFactoryCount) intoTableAgg;
            AggregationValidationUtil.ValidateAggregationFilter(_parent.HasFilter, that._parent.HasFilter);
            if (_parent.IsDistinct)
                AggregationValidationUtil.ValidateAggregationInputType(_countedValueType, that._countedValueType);
            if (_ignoreNulls != that._ignoreNulls)
                throw new ExprValidationException("The aggregation declares" +
                                                  (_ignoreNulls ? "" : " no") +
                                                  " ignore nulls and provided is" +
                                                  (that._ignoreNulls ? "" : " no") +
                                                  " ignore nulls");
        }

        public AggregationAgent AggregationStateAgent => null;

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return GetMethodAggregationEvaluatorCountBy(_parent.PositionalParams, join, typesPerStream);
        }

        public static ExprEvaluator GetMethodAggregationEvaluatorCountBy(ExprNode[] childNodes, bool join,
            EventType[] typesPerStream)
        {
            if (childNodes[0] is ExprWildcard && childNodes.Length == 2)
                return ExprMethodAggUtil.GetDefaultEvaluator(new[] {childNodes[1]}, join, typesPerStream);
            if (childNodes[0] is ExprWildcard && childNodes.Length == 1)
                return ExprMethodAggUtil.GetDefaultEvaluator(new ExprNode[0], join, typesPerStream);
            return ExprMethodAggUtil.GetDefaultEvaluator(childNodes, join, typesPerStream);
        }

        private AggregationMethod MakeCountAggregator(bool isIgnoreNull, bool hasFilter)
        {
            if (!hasFilter)
            {
                if (isIgnoreNull) return new AggregatorCountNonNull();
                return new AggregatorCount();
            }

            if (isIgnoreNull) return new AggregatorCountNonNullFilter();
            return new AggregatorCountFilter();
        }
    }
} // end of namespace