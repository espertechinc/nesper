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
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeNRRelOpAllAnyAggregated : SubselectForgeNRRelOpBase
    {
        private readonly ExprForge havingEval;

        public SubselectForgeNRRelOpAllAnyAggregated(
            ExprSubselectNode subselect,
            ExprForge valueEval,
            ExprForge selectEval,
            bool resultWhenNoMatchingEvents,
            RelationalOpEnumComputer computer,
            ExprForge havingEval)
            : base(
                subselect,
                valueEval,
                selectEval,
                resultWhenNoMatchingEvents,
                computer)
        {
            this.havingEval = havingEval;
        }

        protected override CodegenExpression CodegenEvaluateInternal(
            CodegenMethodScope parent,
            SubselectForgeNRSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(bool?), GetType(), classScope);
            CodegenExpression eps = symbols.GetAddEPS(method);
            CodegenExpression evalCtx = symbols.GetAddExprEvalCtx(method);
            var left = symbols.GetAddLeftResult(method);

            if (havingEval != null) {
                CodegenExpression having = LocalMethod(
                    CodegenLegoMethodExpression.CodegenExpression(havingEval, method, classScope, true),
                    eps,
                    ConstantTrue(),
                    evalCtx);
                CodegenLegoBooleanExpression.CodegenReturnValueIfNullOrNotPass(
                    method.Block,
                    havingEval.EvaluationType,
                    having,
                    ConstantNull());
            }

            CodegenExpression rhsSide = LocalMethod(
                CodegenLegoMethodExpression.CodegenExpression(selectEval, method, classScope, true),
                eps,
                ConstantTrue(),
                evalCtx);
            var rhsType = selectEval.EvaluationType.GetBoxedType();
            method.Block
                .DeclareVar(rhsType, "rhs", rhsSide)
                .IfRefNullReturnNull("rhs")
                .MethodReturn(computer.Codegen(left, symbols.LeftResultType, Ref("rhs"), rhsType));

            return LocalMethod(method);
        }
    }
} // end of namespace