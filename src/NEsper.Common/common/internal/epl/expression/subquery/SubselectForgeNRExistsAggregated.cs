///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.subquery.SubselectForgeCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeNRExistsAggregated : SubselectForgeNR
    {
        private readonly ExprForge havingEval;

        public SubselectForgeNRExistsAggregated(ExprForge havingEval)
        {
            this.havingEval = havingEval;
        }

        public CodegenExpression EvaluateMatchesCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(bool), GetType(), classScope);
            var havingMethod = CodegenLegoMethodExpression.CodegenExpression(havingEval, method, classScope);
            CodegenExpression having = LocalMethod(
                havingMethod,
                REF_EVENTS_SHIFTED,
                symbols.GetAddIsNewData(method),
                symbols.GetAddExprEvalCtx(method));

            method.Block.ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols);
            CodegenLegoBooleanExpression.CodegenReturnValueIfNullOrNotPass(
                method.Block,
                typeof(bool?),
                having,
                ConstantFalse());
            method.Block.MethodReturn(ConstantTrue());
            return LocalMethod(method);
        }
    }
} // end of namespace