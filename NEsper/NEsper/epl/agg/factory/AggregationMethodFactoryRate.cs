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
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.agg.factory
{
    public class AggregationMethodFactoryRate : AggregationMethodFactory
    {
        private readonly long _intervalTime;
        private readonly bool _isEver;
        private readonly ExprRateAggNode _parent;
        private readonly TimeAbacus _timeAbacus;
        private readonly TimeProvider _timeProvider;

        public AggregationMethodFactoryRate(ExprRateAggNode parent, bool isEver, long intervalTime,
            TimeProvider timeProvider, TimeAbacus timeAbacus)
        {
            _parent = parent;
            _isEver = isEver;
            _intervalTime = intervalTime;
            _timeProvider = timeProvider;
            _timeAbacus = timeAbacus;
        }

        public bool IsAccessAggregation => false;

        public Type ResultType => typeof(double);

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
            if (_isEver)
            {
                if (_parent.PositionalParams.Length == 0) {
                    return new AggregatorRateEver(_intervalTime, _timeAbacus.OneSecond, _timeProvider);
                }
                else {
                    return new AggregatorRateEverFilter(_intervalTime, _timeAbacus.OneSecond, _timeProvider);
                }
            }

            if (_parent.OptionalFilter != null) {
                return new AggregatorRateFilter(_timeAbacus.OneSecond);
            }

            return new AggregatorRate(_timeAbacus.OneSecond);
        }

        public ExprAggregateNodeBase AggregationExpression => _parent;

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationValidationUtil.ValidateAggregationType(this, intoTableAgg);
            var that = (AggregationMethodFactoryRate) intoTableAgg;
            if (_intervalTime != that._intervalTime)
                throw new ExprValidationException("The size is " + _intervalTime + " and provided is " + that._intervalTime);
            AggregationValidationUtil.ValidateAggregationUnbound(!_isEver, !that._isEver);
        }

        public AggregationAgent AggregationStateAgent => null;

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_parent.PositionalParams, join, typesPerStream);
        }
    }
} // end of namespace