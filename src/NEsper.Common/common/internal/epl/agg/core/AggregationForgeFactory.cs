///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    /// <summary>
    ///     Factory for aggregation methods.
    /// </summary>
    public interface AggregationForgeFactory
    {
        bool IsAccessAggregation { get; }

        AggregatorMethod Aggregator { get; }

        Type ResultType { get; }

        AggregationAccessorForge AccessorForge { get; }

        ExprAggregateNodeBase AggregationExpression { get; }

        AggregationPortableValidation AggregationPortableValidation { get; }

        MathContext OptionalMathContext { get; }

        AggregationMultiFunctionStateKey GetAggregationStateKey(bool isMatchRecognize);

        AggregationStateFactoryForge GetAggregationStateFactory(
            bool isMatchRecognize,
            bool isJoin);

        AggregationAgentForge GetAggregationStateAgent(
            ImportService importService,
            string statementName);

        ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream);
    }
} // end of namespace