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
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.accessagg
{
    public class ExprAggMultiFunctionLinearAccessNodeFactoryAccess : AggregationMethodFactory
    {
        private readonly ExprAggMultiFunctionLinearAccessNode _parent;
        private readonly AggregationAccessor _accessor;
        private readonly Type _accessorResultType;
        private readonly EventType _containedEventType;
    
        private readonly AggregationStateKey _optionalStateKey;
        private readonly AggregationStateFactory _optionalStateFactory;
        private readonly AggregationAgent _optionalAgent;
    
        public ExprAggMultiFunctionLinearAccessNodeFactoryAccess(ExprAggMultiFunctionLinearAccessNode parent, AggregationAccessor accessor, Type accessorResultType, EventType containedEventType, AggregationStateKey optionalStateKey, AggregationStateFactory optionalStateFactory, AggregationAgent optionalAgent)
        {
            _parent = parent;
            _accessor = accessor;
            _accessorResultType = accessorResultType;
            _containedEventType = containedEventType;
            _optionalStateKey = optionalStateKey;
            _optionalStateFactory = optionalStateFactory;
            _optionalAgent = optionalAgent;
        }

        public bool IsAccessAggregation
        {
            get { return true; }
        }

        public AggregationMethod Make()
        {
            throw new UnsupportedOperationException();
        }

        public Type ResultType
        {
            get { return _accessorResultType; }
        }

        public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            return _optionalStateKey;
        }
    
        public AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize)
        {
            return _optionalStateFactory;
        }

        public AggregationAccessor Accessor
        {
            get { return _accessor; }
        }

        public ExprAggregateNodeBase AggregationExpression
        {
            get { return _parent; }
        }

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg)
        {
            AggregationMethodFactoryUtil.ValidateAggregationType(this, intoTableAgg);
            var other = (ExprAggMultiFunctionLinearAccessNodeFactoryAccess) intoTableAgg;
            AggregationMethodFactoryUtil.ValidateEventType(_containedEventType, other.ContainedEventType);
        }

        public AggregationAgent AggregationStateAgent
        {
            get { return _optionalAgent; }
        }

        public EventType ContainedEventType
        {
            get { return _containedEventType; }
        }

        public ExprEvaluator GetMethodAggregationEvaluator(Boolean join, EventType[] typesPerStream)
        {
            return null;
        }
    }
}
