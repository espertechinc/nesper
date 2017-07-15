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

using AggregationMethodFactoryUtil = com.espertech.esper.epl.agg.factory.AggregationMethodFactoryUtil;

namespace com.espertech.esper.epl.expression.accessagg
{
    public class AggregationMethodFactoryFirstLastUnbound : AggregationMethodFactory
    {
        private readonly ExprAggMultiFunctionLinearAccessNode _parent;
        private readonly EventType _collectionEventType;
        private readonly Type _resultType;
        private readonly int _streamNum;

        public AggregationMethodFactoryFirstLastUnbound(
            ExprAggMultiFunctionLinearAccessNode parent,
            EventType collectionEventType,
            Type resultType,
            int streamNum)
        {
            _parent = parent;
            _collectionEventType = collectionEventType;
            _resultType = resultType;
            _streamNum = streamNum;
        }

        public Type ResultType
        {
            get { return _resultType; }
        }

        public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            throw new UnsupportedOperationException();
        }

        public bool IsAccessAggregation
        {
            get { return false; }
        }

        public AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize)
        {
            throw new UnsupportedOperationException();
        }

        public AggregationAccessor Accessor
        {
            get { throw new UnsupportedOperationException(); }
        }

        public AggregationMethod Make()
        {
            if (_parent.StateType == AggregationStateType.FIRST)
            {
                return AggregationMethodFactoryUtil.MakeFirstEver(false);
            }
            else if (_parent.StateType == AggregationStateType.LAST)
            {
                return AggregationMethodFactoryUtil.MakeLastEver(false);
            }
            throw new EPException("Window aggregation function is not available");
        }

        public ExprAggregateNodeBase AggregationExpression
        {
            get { return _parent; }
        }

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            agg.service.AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
            var that = (AggregationMethodFactoryFirstLastUnbound) intoTableAgg;
            agg.service.AggregationMethodFactoryUtil.ValidateStreamNumZero(that._streamNum);
            if (_collectionEventType != null)
            {
                agg.service.AggregationMethodFactoryUtil.ValidateEventType(
                    _collectionEventType, that._collectionEventType);
            }
            else
            {
                agg.service.AggregationMethodFactoryUtil.ValidateAggregationInputType(_resultType, that._resultType);
            }
        }

        public AggregationAgent AggregationStateAgent
        {
            get { throw new UnsupportedOperationException(); }
        }

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultEvaluator(_parent.PositionalParams, join, typesPerStream);
        }
    }
} // end of namespace
