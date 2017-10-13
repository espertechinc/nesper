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
    public abstract class ExprAggCountMinSketchNodeFactoryBase : AggregationMethodFactory
    {
        private readonly ExprAggCountMinSketchNode _parent;

        public abstract Type ResultType { get; }
        public abstract AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize);
        public abstract AggregationAccessor Accessor { get; }
        public abstract void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg);
        public abstract AggregationAgent AggregationStateAgent { get; }
        public abstract ExprEvaluator GetMethodAggregationEvaluator(bool @join, EventType[] typesPerStream);

        protected ExprAggCountMinSketchNodeFactoryBase(ExprAggCountMinSketchNode parent)
        {
            _parent = parent;
        }

        public virtual bool IsAccessAggregation
        {
            get { return true; }
        }

        public virtual AggregationStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            throw new UnsupportedOperationException();
        }

        public virtual AggregationMethod Make()
        {
            throw new UnsupportedOperationException();
        }

        public virtual ExprAggregateNodeBase AggregationExpression
        {
            get { return _parent; }
        }

        public virtual ExprAggCountMinSketchNode Parent
        {
            get { return _parent; }
        }
    }
}
