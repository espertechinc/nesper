///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.method.core
{
    public abstract class AggregationForgeFactoryBase : AggregationForgeFactory
    {
        public abstract AggregatorMethod Aggregator { get; }
        public abstract Type ResultType { get; }
        public abstract ExprAggregateNodeBase AggregationExpression { get; }
        public abstract AggregationPortableValidation AggregationPortableValidation { get; }

        public abstract ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream);

        public virtual MathContext OptionalMathContext { get; set; }

        public AggregationMultiFunctionStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            throw new IllegalStateException("Not an access aggregation function");
        }

        public AggregationStateFactoryForge GetAggregationStateFactory(
            bool isMatchRecognize,
            bool isJoin)
        {
            throw new IllegalStateException("Not an access aggregation function");
        }

        public AggregationAccessorForge AccessorForge =>
            throw new IllegalStateException("Not an access aggregation function");

        public AggregationAgentForge GetAggregationStateAgent(
            ImportService importService,
            string statementName)
        {
            throw new IllegalStateException("Not an access aggregation function");
        }

        public bool IsAccessAggregation => false;
    }
} // end of namespace