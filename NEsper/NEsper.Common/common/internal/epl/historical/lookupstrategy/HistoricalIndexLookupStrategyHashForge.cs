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
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.lookupstrategy
{
    public class HistoricalIndexLookupStrategyHashForge : HistoricalIndexLookupStrategyForge
    {
        private readonly Type[] _coercionTypes;
        private readonly ExprForge[] _evaluators;
        private readonly int _lookupStream;
        private readonly MultiKeyClassRef _multiKeyClassRef;

        public HistoricalIndexLookupStrategyHashForge(
            int lookupStream,
            ExprForge[] evaluators,
            Type[] coercionTypes,
            MultiKeyClassRef multiKeyClassRef)
        {
            _lookupStream = lookupStream;
            _evaluators = evaluators;
            _coercionTypes = coercionTypes;
            _multiKeyClassRef = multiKeyClassRef;
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
            var method = parent.MakeChild(typeof(HistoricalIndexLookupStrategyHash), GetType(), classScope);
            var evaluator = MultiKeyCodegen.CodegenExprEvaluatorMayMultikey(
                _evaluators, _coercionTypes, _multiKeyClassRef, method, classScope);

            method.Block
                .DeclareVar<HistoricalIndexLookupStrategyHash>(
                    "strat",
                    NewInstance(typeof(HistoricalIndexLookupStrategyHash)))
                .SetProperty(Ref("strat"), "LookupStream", Constant(_lookupStream))
                .SetProperty(Ref("strat"), "Evaluator", evaluator)
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace