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
using com.espertech.esper.epl.agg.factory;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;

namespace com.espertech.esper.epl.expression.accessagg
{
    public class AggregationMethodFactoryFirstLastUnbound : AggregationMethodFactory
    {
        private readonly EventType _collectionEventType;
        private readonly bool _hasFilter;
        private readonly ExprAggMultiFunctionLinearAccessNode _parent;
        private readonly int _streamNum;
        private readonly Type _resultType;

        public AggregationMethodFactoryFirstLastUnbound(ExprAggMultiFunctionLinearAccessNode parent,
            EventType collectionEventType, Type resultType, int streamNum, bool hasFilter)
        {
            _parent = parent;
            _collectionEventType = collectionEventType;
            _resultType = resultType;
            _streamNum = streamNum;
            _hasFilter = hasFilter;
        }

        public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            throw new UnsupportedOperationException();
        }

        public AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize)
        {
            throw new UnsupportedOperationException();
        }

        public AggregationAccessor Accessor => throw new UnsupportedOperationException();

        public AggregationMethod Make()
        {
            if (_parent.StateType == AggregationStateType.FIRST)
                return AggregationMethodFactoryUtil.MakeFirstEver(_hasFilter);
            if (_parent.StateType == AggregationStateType.LAST)
                return AggregationMethodFactoryUtil.MakeLastEver(_hasFilter);
            throw new EPRuntimeException("Window aggregation function is not available");
        }

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationValidationUtil.ValidateAggregationType(this, intoTableAgg);
            var that = (AggregationMethodFactoryFirstLastUnbound) intoTableAgg;
            AggregationValidationUtil.ValidateStreamNumZero(that._streamNum);
            if (_collectionEventType != null)
                AggregationValidationUtil.ValidateEventType(_collectionEventType, that._collectionEventType);
            else
                AggregationValidationUtil.ValidateAggregationInputType(ResultType, that.ResultType);
        }

        public AggregationAgent AggregationStateAgent => throw new UnsupportedOperationException();

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_parent.PositionalParams, join, typesPerStream);
        }

        public Type ResultType
        {
            get { return _resultType; }
        }

        public bool IsAccessAggregation => false;

        public ExprAggregateNodeBase AggregationExpression => _parent;
    }
} // end of namespace