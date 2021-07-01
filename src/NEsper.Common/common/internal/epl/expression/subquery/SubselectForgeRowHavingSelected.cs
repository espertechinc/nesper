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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.subquery.SubselectForgeCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeRowHavingSelected : SubselectForgeRow
    {
        private readonly ExprSubselectRowNode subselect;

        public SubselectForgeRowHavingSelected(ExprSubselectRowNode subselect)
        {
            this.subselect = subselect;
        }

        public CodegenExpression EvaluateCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(subselect.EvaluationType, GetType(), classScope);
            var havingMethod = CodegenLegoMethodExpression.CodegenExpression(subselect.HavingExpr, method, classScope, true);
            CodegenExpression having = LocalMethod(
                havingMethod,
                REF_EVENTS_SHIFTED,
                symbols.GetAddIsNewData(method),
                symbols.GetAddExprEvalCtx(method));

            method.Block.ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols);
            CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
                method.Block,
                typeof(bool?),
                having,
                ConstantNull());

            if (subselect.SelectClause.Length == 1) {
                var eval = CodegenLegoMethodExpression.CodegenExpression(subselect.SelectClause[0].Forge, method, classScope, true);
                method.Block.MethodReturn(
                    LocalMethod(eval, REF_EVENTS_SHIFTED, ConstantTrue(), symbols.GetAddExprEvalCtx(method)));
            }
            else {
                method.Block.MethodReturn(LocalMethod(subselect.EvaluateRowCodegen(method, classScope)));
            }

            return LocalMethod(method);
        }

        public CodegenExpression EvaluateGetCollEventsCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbol,
            CodegenClassScope classScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateGetCollScalarCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbol,
            CodegenClassScope classScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateGetBeanCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateTypableSinglerowCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateTypableMultirowCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            return ConstantNull();
        }
    }
} // end of namespace