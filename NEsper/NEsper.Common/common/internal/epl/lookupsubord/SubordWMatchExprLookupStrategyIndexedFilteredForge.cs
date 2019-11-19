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
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.lookupplansubord;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.lookupsubord
{
    public class SubordWMatchExprLookupStrategyIndexedFilteredForge : SubordWMatchExprLookupStrategyFactoryForge
    {
        private readonly ExprForge exprForge;

        public SubordWMatchExprLookupStrategyIndexedFilteredForge(
            ExprForge exprForge,
            SubordTableLookupStrategyFactoryForge lookupStrategyFactory)
        {
            this.exprForge = exprForge;
            OptionalInnerStrategy = lookupStrategyFactory;
        }

        public SubordTableLookupStrategyFactoryForge OptionalInnerStrategy { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(SubordWMatchExprLookupStrategyFactory), GetType(), classScope);
            method.Block
                .DeclareVar<ExprEvaluator>(
                    "eval",
                    ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(exprForge, method, GetType(), classScope))
                .DeclareVar<SubordTableLookupStrategyFactory>(
                    "lookup",
                    OptionalInnerStrategy.Make(method, symbols, classScope))
                .MethodReturn(
                    NewInstance<SubordWMatchExprLookupStrategyIndexedFilteredFactory>(
                        Ref("eval"),
                        Ref("lookup")));
            return LocalMethod(method);
        }

        public string ToQueryPlan()
        {
            return GetType().Name + " " + " strategy " + OptionalInnerStrategy.ToQueryPlan();
        }
    }
} // end of namespace