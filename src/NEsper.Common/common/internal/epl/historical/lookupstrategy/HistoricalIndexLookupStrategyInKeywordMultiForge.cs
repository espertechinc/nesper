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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.lookupstrategy
{
    public class HistoricalIndexLookupStrategyInKeywordMultiForge : HistoricalIndexLookupStrategyForge
    {
        private readonly int lookupStream;
        private readonly ExprNode evaluator;

        public HistoricalIndexLookupStrategyInKeywordMultiForge(
            int lookupStream,
            ExprNode evaluator)
        {
            this.lookupStream = lookupStream;
            this.evaluator = evaluator;
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
            CodegenMethod method = parent.MakeChild(
                typeof(HistoricalIndexLookupStrategyInKeywordMulti),
                GetType(),
                classScope);

            method.Block
                .DeclareVar<HistoricalIndexLookupStrategyInKeywordMulti>(
                    "strat",
                    NewInstance(typeof(HistoricalIndexLookupStrategyInKeywordMulti)))
                .SetProperty(Ref("strat"), "LookupStream", Constant(lookupStream))
                .SetProperty(
                    Ref("strat"),
                    "Evaluator",
                    ExprNodeUtilityCodegen.CodegenEvaluator(evaluator.Forge, method, GetType(), classScope))
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace