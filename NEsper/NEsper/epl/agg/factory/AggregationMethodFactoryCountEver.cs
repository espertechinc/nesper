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
    public class AggregationMethodFactoryCountEver : AggregationMethodFactory
    {
        private readonly bool _ignoreNulls;
        private readonly ExprCountEverNode _parent;

        public AggregationMethodFactoryCountEver(ExprCountEverNode parent, bool ignoreNulls)
        {
            _parent = parent;
            _ignoreNulls = ignoreNulls;
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
            return MakeCountEverValueAggregator(_parent.HasFilter, _ignoreNulls);
        }

        public ExprAggregateNodeBase AggregationExpression => _parent;

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationValidationUtil.ValidateAggregationType(this, intoTableAgg);
            var that = (AggregationMethodFactoryCountEver) intoTableAgg;
            if (that._ignoreNulls != _ignoreNulls)
                throw new ExprValidationException("The aggregation declares " +
                                                  (_ignoreNulls ? "ignore-nulls" : "no-ignore-nulls") +
                                                  " and provided is " +
                                                  (that._ignoreNulls ? "ignore-nulls" : "no-ignore-nulls"));
            AggregationValidationUtil.ValidateAggregationFilter(_parent.HasFilter, that._parent.HasFilter);
        }

        public AggregationAgent AggregationStateAgent => null;

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_parent.PositionalParams, join, typesPerStream);
        }

        private AggregationMethod MakeCountEverValueAggregator(bool hasFilter, bool ignoreNulls)
        {
            if (!hasFilter)
            {
                if (ignoreNulls) return new AggregatorCountEverNonNull();
                return new AggregatorCountEver();
            }

            if (ignoreNulls) return new AggregatorCountEverNonNullFilter();
            return new AggregatorCountEverFilter();
        }
    }
} // end of namespace