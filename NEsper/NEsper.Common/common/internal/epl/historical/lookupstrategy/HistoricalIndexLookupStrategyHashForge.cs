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
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.lookupstrategy
{
    public class HistoricalIndexLookupStrategyHashForge : HistoricalIndexLookupStrategyForge
    {
        private readonly Type[] coercionTypes;
        private readonly ExprForge[] evaluators;

        private readonly int lookupStream;

        public HistoricalIndexLookupStrategyHashForge(int lookupStream, ExprForge[] evaluators, Type[] coercionTypes)
        {
            this.lookupStream = lookupStream;
            this.evaluators = evaluators;
            this.coercionTypes = coercionTypes;
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(HistoricalIndexLookupStrategyHash), GetType(), classScope);

            method.Block
                .DeclareVar(
                    typeof(HistoricalIndexLookupStrategyHash), "strat",
                    NewInstance(typeof(HistoricalIndexLookupStrategyHash)))
                .ExprDotMethod(Ref("strat"), "setLookupStream", Constant(lookupStream))
                .ExprDotMethod(
                    Ref("strat"), "setEvaluator",
                    ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                        evaluators, coercionTypes, method, GetType(), classScope))
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace