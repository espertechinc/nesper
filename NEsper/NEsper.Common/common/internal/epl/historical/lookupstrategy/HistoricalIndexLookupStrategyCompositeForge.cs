///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.lookupstrategy
{
    public class HistoricalIndexLookupStrategyCompositeForge : HistoricalIndexLookupStrategyForge
    {
        private readonly ExprForge[] evaluators;

        private readonly int lookupStream;
        private readonly QueryGraphValueEntryRangeForge[] ranges;

        public HistoricalIndexLookupStrategyCompositeForge(
            int lookupStream, ExprForge[] evaluators, QueryGraphValueEntryRangeForge[] ranges)
        {
            this.lookupStream = lookupStream;
            this.evaluators = evaluators;
            this.ranges = ranges;
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(HistoricalIndexLookupStrategyComposite), GetType(), classScope);

            method.Block.DeclareVar(
                typeof(QueryGraphValueEntryRange[]), "rangeGetters",
                NewArrayByLength(typeof(QueryGraphValueEntryRange), Constant(ranges.Length)));
            for (var i = 0; i < ranges.Length; i++) {
                method.Block.AssignArrayElement(
                    Ref("rangeGetters"), Constant(i), ranges[i].Make(null, method, symbols, classScope));
            }

            method.Block
                .DeclareVar(
                    typeof(HistoricalIndexLookupStrategyComposite), "strat",
                    NewInstance(typeof(HistoricalIndexLookupStrategyComposite)))
                .ExprDotMethod(Ref("strat"), "setLookupStream", Constant(lookupStream))
                .ExprDotMethod(
                    Ref("strat"), "setHashGetter",
                    ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                        evaluators, null, method, GetType(), classScope))
                .ExprDotMethod(Ref("strat"), "setRangeProps", Ref("rangeGetters"))
                .ExprDotMethod(Ref("strat"), "init")
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace