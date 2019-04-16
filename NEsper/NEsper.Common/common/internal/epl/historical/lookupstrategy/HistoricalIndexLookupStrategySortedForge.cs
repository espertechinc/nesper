///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.lookupstrategy
{
    public class HistoricalIndexLookupStrategySortedForge : HistoricalIndexLookupStrategyForge
    {
        private readonly Type coercionType;

        private readonly int lookupStream;
        private readonly QueryGraphValueEntryRangeForge range;

        public HistoricalIndexLookupStrategySortedForge(
            int lookupStream,
            QueryGraphValueEntryRangeForge range,
            Type coercionType)
        {
            this.lookupStream = lookupStream;
            this.range = range;
            this.coercionType = coercionType;
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(HistoricalIndexLookupStrategySorted), GetType(), classScope);

            method.Block
                .DeclareVar(
                    typeof(HistoricalIndexLookupStrategySorted), "strat",
                    NewInstance(typeof(HistoricalIndexLookupStrategySorted)))
                .ExprDotMethod(Ref("strat"), "setLookupStream", Constant(lookupStream))
                .ExprDotMethod(Ref("strat"), "setEvalRange", range.Make(coercionType, method, symbols, classScope))
                .ExprDotMethod(Ref("strat"), "init")
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace