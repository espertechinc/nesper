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
    public class AggregationMethodFactoryAvg : AggregationMethodFactory
    {
        private readonly Type _resultType;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationMethodFactoryAvg"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="childType">Type of the child.</param>
        /// <param name="optionalMathContext">The optional math context.</param>
        public AggregationMethodFactoryAvg(ExprAvgNode parent, Type childType, MathContext optionalMathContext)
        {
            Parent = parent;
            ChildType = childType;
            _resultType = GetAvgAggregatorType(childType);
            OptionalMathContext = optionalMathContext;
        }

        public Type ChildType { get; }

        public MathContext OptionalMathContext { get; }

        public ExprAvgNode Parent { get; }

        public bool IsAccessAggregation => false;

        public Type ResultType => _resultType;

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
            var method = MakeAvgAggregator(ChildType, Parent.HasFilter, OptionalMathContext);
            if (!Parent.IsDistinct) return method;
            return AggregationMethodFactoryUtil.MakeDistinctAggregator(method, Parent.HasFilter);
        }

        public ExprAggregateNodeBase AggregationExpression => Parent;

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationValidationUtil.ValidateAggregationType(this, intoTableAgg);
            var that = (AggregationMethodFactoryAvg) intoTableAgg;
            AggregationValidationUtil.ValidateAggregationInputType(ChildType, that.ChildType);
            AggregationValidationUtil.ValidateAggregationFilter(Parent.HasFilter, that.Parent.HasFilter);
        }

        public AggregationAgent AggregationStateAgent => null;

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(Parent.PositionalParams, join, typesPerStream);
        }

        private Type GetAvgAggregatorType(Type type)
        {
            if (type.IsDecimal() || type.IsBigInteger())
                return typeof(decimal);
            return typeof(double);
        }

        private AggregationMethod MakeAvgAggregator(Type type, bool hasFilter, MathContext optionalMathContext)
        {
            if (hasFilter)
            {
                if (type.IsDecimal() || type.IsBigInteger())
                    return new AggregatorAvgDecimalFilter(optionalMathContext);
                return new AggregatorAvgFilter();
            }

            if (type.IsDecimal() || type.IsBigInteger())
                return new AggregatorAvgDecimal(optionalMathContext);

            return new AggregatorAvg();
        }
    }
} // end of namespace