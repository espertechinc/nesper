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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
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
        private readonly ExprRateAggNode _parent;
        private readonly bool _isEver;
        private readonly long _intervalTime;
        private readonly TimeProvider _timeProvider;
        private readonly TimeAbacus _timeAbacus;

        public AggregationMethodFactoryRate(
            ExprRateAggNode parent,
            bool isEver,
            long intervalTime,
            TimeProvider timeProvider,
            TimeAbacus timeAbacus)
        {
            _parent = parent;
            _isEver = isEver;
            _intervalTime = intervalTime;
            _timeProvider = timeProvider;
            _timeAbacus = timeAbacus;
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

        public AggregationMethod Make()
        {
            if (_isEver)
            {
                return new AggregatorRateEver(_intervalTime, _timeAbacus.GetOneSecond(), _timeProvider);
            }
            else
            {
                return new AggregatorRate(_timeAbacus.GetOneSecond());
            }
        }

        public ExprAggregateNodeBase AggregationExpression
        {
            get { return _parent; }
        }

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
            AggregationMethodFactoryRate that = (AggregationMethodFactoryRate) intoTableAgg;
            if (_intervalTime != that._intervalTime)
            {
                throw new ExprValidationException(
                    "The size is " +
                    _intervalTime +
                    " and provided is " +
                    that._intervalTime);
            }
            AggregationMethodFactoryUtil.ValidateAggregationUnbound(!_isEver, !that._isEver);
        }

        public AggregationAgent AggregationStateAgent
        {
            get { return null; }
        }

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_parent.PositionalParams, join, typesPerStream);
        }
    }
} // end of namespace
