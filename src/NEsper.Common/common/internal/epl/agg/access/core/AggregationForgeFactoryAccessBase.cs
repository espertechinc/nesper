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
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.access.core
{
    public abstract class AggregationForgeFactoryAccessBase : AggregationForgeFactory
    {
        public bool IsAccessAggregation => true;

        public AggregatorMethod Aggregator => throw new IllegalStateException("Not applicable for access-aggregations");

        public virtual MathContext OptionalMathContext => null;

        public ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            throw new IllegalStateException("Not applicable for access-aggregations");
        }

        public abstract Type ResultType { get; }
        public abstract AggregationAccessorForge AccessorForge { get; }
        public abstract ExprAggregateNodeBase AggregationExpression { get; }
        public abstract AggregationPortableValidation AggregationPortableValidation { get; }

        public abstract AggregationMultiFunctionStateKey GetAggregationStateKey(bool isMatchRecognize);

        public abstract AggregationStateFactoryForge GetAggregationStateFactory(
            bool isMatchRecognize,
            bool isJoin);

        public abstract AggregationAgentForge GetAggregationStateAgent(
            ImportService importService,
            string statementName);
    }
} // end of namespace