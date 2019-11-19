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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;

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

        void InitMethodForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope);

        AggregationMultiFunctionStateKey GetAggregationStateKey(bool isMatchRecognize);

        AggregationStateFactoryForge GetAggregationStateFactory(bool isMatchRecognize);

        AggregationAgentForge GetAggregationStateAgent(
            ImportService importService,
            string statementName);

        ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream);
    }
} // end of namespace