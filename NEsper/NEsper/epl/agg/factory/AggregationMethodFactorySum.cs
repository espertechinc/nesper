///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.agg.factory
{
    public class AggregationMethodFactorySum : AggregationMethodFactory
    {
        private readonly Type _inputValueType;
        private readonly ExprSumNode _parent;

        public AggregationMethodFactorySum(ExprSumNode parent, Type inputValueType)
        {
            _parent = parent;
            _inputValueType = inputValueType;
            ResultType = GetSumAggregatorType(inputValueType);
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
            var method = MakeSumAggregator(_inputValueType, _parent.HasFilter);
            if (!_parent.IsDistinct) {
                return method;
            }

            return AggregationMethodFactoryUtil.MakeDistinctAggregator(method, _parent.HasFilter);
        }

        public ExprAggregateNodeBase AggregationExpression => _parent;

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationValidationUtil.ValidateAggregationType(this, intoTableAgg);
            var that = (AggregationMethodFactorySum) intoTableAgg;
            AggregationValidationUtil.ValidateAggregationInputType(_inputValueType, that._inputValueType);
            AggregationValidationUtil.ValidateAggregationFilter(_parent.HasFilter, that._parent.HasFilter);
        }

        public AggregationAgent AggregationStateAgent => null;

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_parent.PositionalParams, join, typesPerStream);
        }

        private Type GetSumAggregatorType(Type type)
        {
            if (type == typeof(BigInteger)) {
                return typeof(BigInteger);
            }

            if (type.IsDecimal()) {
                return typeof(decimal);
            }

            if (type == typeof(long?) || type == typeof(long)) {
                return typeof(long);
            }

            if (type == typeof(int?) || type == typeof(int)) {
                return typeof(int);
            }

            if (type == typeof(double?) || type == typeof(double)) {
                return typeof(double);
            }

            if (type == typeof(float?) || type == typeof(float)) {
                return typeof(float);
            }

            return typeof(int);
        }

        private AggregationMethod MakeSumAggregator(Type type, bool hasFilter)
        {
            if (!hasFilter) {
                if (type.IsBigInteger()) {
                    return new AggregatorSumBigInteger();
                }

                if (type.IsDecimal()) {
                    return new AggregatorSumDecimal();
                }

                if (type == typeof(long?) || type == typeof(long)) {
                    return new AggregatorSumLong();
                }

                if (type == typeof(int?) || type == typeof(int)) {
                    return new AggregatorSumInteger();
                }

                if (type == typeof(double?) || type == typeof(double)) {
                    return new AggregatorSumDouble();
                }

                if (type == typeof(float?) || type == typeof(float)) {
                    return new AggregatorSumFloat();
                }

                return new AggregatorSumNumInteger();
            }

            if (type.IsBigInteger()) {
                return new AggregatorSumBigIntegerFilter();
            }

            if (type.IsDecimal()) {
                return new AggregatorSumDecimalFilter();
            }

            if (type == typeof(long) || type == typeof(long)) {
                return new AggregatorSumLongFilter();
            }

            if (type == typeof(int?) || type == typeof(int)) {
                return new AggregatorSumIntegerFilter();
            }

            if (type == typeof(double?) || type == typeof(double)) {
                return new AggregatorSumDoubleFilter();
            }

            if (type == typeof(float?) || type == typeof(float)) {
                return new AggregatorSumFloatFilter();
            }

            return new AggregatorSumNumIntegerFilter();
        }
    }
} // end of namespace