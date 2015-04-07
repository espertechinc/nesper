///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Factory for aggregation methods.
    /// </summary>
    public interface AggregationMethodFactory
    {
        bool IsAccessAggregation { get; }

        AggregationMethod Make(MethodResolutionService methodResolutionService, int agentInstanceId, int groupId, int aggregationId);

        Type ResultType { get; }

        AggregationStateKey GetAggregationStateKey(bool isMatchRecognize);
    
        AggregationStateFactory GetAggregationStateFactory(bool isMatchRecognize);

        AggregationAccessor Accessor { get; }

        ExprAggregateNodeBase AggregationExpression { get; }

        void ValidateIntoTableCompatible(AggregationMethodFactory intoTableAgg);

        AggregationAgent AggregationStateAgent { get; }

        ExprEvaluator GetMethodAggregationEvaluator(bool join, EventType[] typesPerStream);
    }
}
