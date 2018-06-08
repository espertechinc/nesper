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

namespace com.espertech.esper.epl.expression.accessagg
{
    public class ExprAggMultiFunctionLinearAccessNodeFactoryAccess : AggregationMethodFactory
    {
        private readonly AggregationStateFactory _optionalStateFactory;

        private readonly AggregationStateKey _optionalStateKey;
        private readonly ExprAggMultiFunctionLinearAccessNode _parent;

        public ExprAggMultiFunctionLinearAccessNodeFactoryAccess(ExprAggMultiFunctionLinearAccessNode parent,
            AggregationAccessor accessor, Type accessorResultType, EventType containedEventType,
            AggregationStateKey optionalStateKey, AggregationStateFactory optionalStateFactory,
            AggregationAgent optionalAgent)
        {
            _parent = parent;
            Accessor = accessor;
            ResultType = accessorResultType;
            ContainedEventType = containedEventType;
            _optionalStateKey = optionalStateKey;
            _optionalStateFactory = optionalStateFactory;
            AggregationStateAgent = optionalAgent;
        }

        public EventType ContainedEventType { get; }

        public AggregationMethod Make()
        {
            throw new UnsupportedOperationException();
        }

        public Type ResultType { get; }

        public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            return _optionalStateKey;
        }

        public AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize)
        {
            return _optionalStateFactory;
        }

        public AggregationAccessor Accessor { get; }

        public ExprAggregateNodeBase AggregationExpression => _parent;

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationValidationUtil.ValidateAggregationType(this, intoTableAgg);
            var other = (ExprAggMultiFunctionLinearAccessNodeFactoryAccess) intoTableAgg;
            AggregationValidationUtil.ValidateEventType(ContainedEventType, other.ContainedEventType);
        }

        public AggregationAgent AggregationStateAgent { get; }

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return null;
        }

        public bool IsAccessAggregation => true;
    }
} // end of namespace