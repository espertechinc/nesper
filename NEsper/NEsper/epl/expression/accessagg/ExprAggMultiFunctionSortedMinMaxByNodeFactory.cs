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
    public class ExprAggMultiFunctionSortedMinMaxByNodeFactory : AggregationMethodFactory
    {
        public ExprAggMultiFunctionSortedMinMaxByNodeFactory(ExprAggMultiFunctionSortedMinMaxByNode parent,
            AggregationAccessor accessor, Type accessorResultType, EventType containedEventType,
            AggregationStateKey optionalStateKey, SortedAggregationStateFactoryFactory optionalStateFactory,
            AggregationAgent optionalAgent)
        {
            Parent = parent;
            Accessor = accessor;
            ResultType = accessorResultType;
            ContainedEventType = containedEventType;
            OptionalStateKey = optionalStateKey;
            OptionalStateFactory = optionalStateFactory;
            AggregationStateAgent = optionalAgent;
        }

        public EventType ContainedEventType { get; }

        public ExprAggMultiFunctionSortedMinMaxByNode Parent { get; }

        public SortedAggregationStateFactoryFactory OptionalStateFactory { get; }

        public AggregationStateKey OptionalStateKey { get; }

        public bool IsAccessAggregation => true;

        public AggregationMethod Make()
        {
            throw new UnsupportedOperationException();
        }

        public Type ResultType { get; }

        public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            return OptionalStateKey;
        }

        public AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize)
        {
            if (isMatchRecognize || OptionalStateFactory == null) return null;
            return OptionalStateFactory.MakeFactory();
        }

        public AggregationAccessor Accessor { get; }

        public ExprAggregateNodeBase AggregationExpression => Parent;

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationValidationUtil.ValidateAggregationType(this, intoTableAgg);
            var other = (ExprAggMultiFunctionSortedMinMaxByNodeFactory) intoTableAgg;
            AggregationValidationUtil.ValidateEventType(ContainedEventType, other.ContainedEventType);
            AggregationValidationUtil.ValidateAggFuncName(Parent.AggregationFunctionName,
                other.Parent.AggregationFunctionName);
        }

        public AggregationAgent AggregationStateAgent { get; }

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return null;
        }
    }
} // end of namespace