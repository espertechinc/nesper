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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    /// <summary>
    ///     Strategy for subselects with "=/!=/&gt;&lt; ALL".
    /// </summary>
    public class SubselectForgeNREqualsAllAnyAggregated : SubselectForgeNREqualsBase
    {
        private readonly ExprForge havingEval;

        public SubselectForgeNREqualsAllAnyAggregated(
            ExprSubselectNode subselect,
            ExprForge valueEval,
            ExprForge selectEval,
            bool resultWhenNoMatchingEvents,
            bool isNot,
            SimpleNumberCoercer coercer,
            ExprForge havingEval)
            : base(
                subselect,
                valueEval,
                selectEval,
                resultWhenNoMatchingEvents,
                isNot,
                coercer)
        {
            this.havingEval = havingEval;
        }

        protected override CodegenExpression CodegenEvaluateInternal(
            CodegenMethodScope parent,
            SubselectForgeNRSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(bool?), GetType(), classScope);
            var eps = symbols.GetAddEPS(method);
            var evalCtx = symbols.GetAddExprEvalCtx(method);
            var left = symbols.GetAddLeftResult(method);

            method.Block.IfRefNullReturnNull(symbols.GetAddLeftResult(method));
            if (havingEval != null) {
                CodegenExpression having = LocalMethod(
                    CodegenLegoMethodExpression.CodegenExpression(havingEval, method, classScope),
                    eps,
                    ConstantTrue(),
                    evalCtx);
                CodegenLegoBooleanExpression.CodegenReturnValueIfNullOrNotPass(
                    method.Block,
                    havingEval.EvaluationType,
                    having,
                    ConstantNull());
            }

            CodegenExpression select = LocalMethod(
                CodegenLegoMethodExpression.CodegenExpression(selectEval, method, classScope),
                eps,
                ConstantTrue(),
                evalCtx);
            var rightEvalType = selectEval.EvaluationType.GetBoxedType();
            method.Block
                .DeclareVar(rightEvalType, "rhs", select)
                .IfRefNullReturnNull("rhs");

            if (coercer == null) {
                method.Block.DeclareVar<bool>("eq", ExprDotMethod(left, "Equals", Ref("rhs")));
                if (isNot) {
                    method.Block.IfCondition(Ref("eq")).BlockReturn(ConstantFalse());
                }
                else {
                    method.Block.IfCondition(Not(Ref("eq"))).BlockReturn(ConstantFalse());
                }
            }
            else {
                method.Block.DeclareVar<object>("left", coercer.CoerceCodegen(left, symbols.LeftResultType))
                    .DeclareVar<object>("right", coercer.CoerceCodegen(Ref("rhs"), rightEvalType))
                    .DeclareVar<bool>("eq", ExprDotMethod(Ref("left"), "Equals", Ref("right")));
                if (isNot) {
                    method.Block.IfCondition(Ref("eq")).BlockReturn(ConstantFalse());
                }
                else {
                    method.Block.IfCondition(Not(Ref("eq"))).BlockReturn(ConstantFalse());
                }
            }

            method.Block.MethodReturn(ConstantTrue());
            return LocalMethod(method);
        }
    }
} // end of namespace