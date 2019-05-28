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
    public class SubordWMatchExprLookupStrategyAllFilteredForge : SubordWMatchExprLookupStrategyFactoryForge
    {
        private readonly ExprNode exprEvaluator;

        public SubordWMatchExprLookupStrategyAllFilteredForge(ExprNode exprEvaluator)
        {
            this.exprEvaluator = exprEvaluator;
        }

        public SubordTableLookupStrategyFactoryForge OptionalInnerStrategy => null;

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(SubordWMatchExprLookupStrategyAllFilteredFactory), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(ExprEvaluator), "eval",
                    ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(exprEvaluator.Forge, method, GetType(), classScope))
                .MethodReturn(NewInstance<SubordWMatchExprLookupStrategyAllFilteredFactory>(Ref("eval")));
            return LocalMethod(method);
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace