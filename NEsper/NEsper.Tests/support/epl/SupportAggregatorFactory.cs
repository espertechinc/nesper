///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.support.epl
{
    public class SupportAggregatorFactory : AggregationMethodFactory
    {
        public bool IsAccessAggregation
        {
            get { return false; }
        }

        public AggregationMethod Make(MethodResolutionService methodResolutionService, int agentInstanceId, int groupId, int aggregationId)
        {
            return new SupportAggregator();
        }

        public Type ResultType
        {
            get { return typeof (int); }
        }

        public AggregationStateKey GetAggregationStateKey(bool isMatchRecognize) {
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

        public ExprAggregateNodeBase AggregationExpression
        {
            get { return null; }
        }

        public void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg) 
        {
            throw new UnsupportedOperationException();
        }

        public AggregationAgent AggregationStateAgent
        {
            get { return null; }
        }

        public ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream)
        {
            return null;
        }
    }
}
