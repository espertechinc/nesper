///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeNRRelOpAllDefault : SubselectForgeNRRelOpBase
    {
        private readonly ExprForge filterOrHavingEval;

        public SubselectForgeNRRelOpAllDefault(
            ExprSubselectNode subselect,
            ExprForge valueEval,
            ExprForge selectEval,
            bool resultWhenNoMatchingEvents,
            RelationalOpEnumComputer computer,
            ExprForge filterOrHavingEval)
            : base(
                subselect,
                valueEval,
                selectEval,
                resultWhenNoMatchingEvents,
                computer)
        {
            this.filterOrHavingEval = filterOrHavingEval;
        }

        protected override CodegenExpression CodegenEvaluateInternal(
            CodegenMethodScope parent,
            SubselectForgeNRSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(bool?), GetType(), classScope);
            method.Block
                .DeclareVar<bool>("hasRows", ConstantFalse())
                .DeclareVar<bool>("hasNullRow", ConstantFalse());
            var @foreach = method.Block.ForEach(
                typeof(EventBean),
                "subselectEvent",
                symbols.GetAddMatchingEvents(method));
            {
                @foreach.AssignArrayElement(NAME_EPS, Constant(0), Ref("subselectEvent"));
                if (filterOrHavingEval != null) {
                    CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
                        @foreach,
                        filterOrHavingEval.EvaluationType,
                        filterOrHavingEval.EvaluateCodegen(typeof(bool?), method, symbols, classScope));
                }

                @foreach.AssignRef("hasRows", ConstantTrue());

                Type valueRightType;
                if (selectEval != null) {
                    valueRightType = selectEval.EvaluationType.GetBoxedType();
                    @foreach.DeclareVar(
                        valueRightType,
                        "valueRight",
                        selectEval.EvaluateCodegen(valueRightType, method, symbols, classScope));
                }
                else {
                    valueRightType = typeof(object);
                    @foreach.DeclareVar(
                        valueRightType,
                        "valueRight",
                        ExprDotUnderlying(ArrayAtIndex(symbols.GetAddEPS(method), Constant(0))));
                }

                @foreach.IfRefNull("valueRight")
                    .AssignRef("hasNullRow", ConstantTrue())
                    .IfElse()
                    .IfCondition(NotEqualsNull(symbols.GetAddLeftResult(method)))
                    .IfCondition(
                        Not(
                            computer.Codegen(
                                symbols.GetAddLeftResult(method),
                                symbols.LeftResultType,
                                Ref("valueRight"),
                                valueRightType)))
                    .BlockReturn(ConstantFalse());
            }

            method.Block
                .IfCondition(Not(Ref("hasRows")))
                .BlockReturn(ConstantTrue())
                .IfCondition(EqualsNull(symbols.GetAddLeftResult(method)))
                .BlockReturn(ConstantNull())
                .IfCondition(Ref("hasNullRow"))
                .BlockReturn(ConstantNull())
                .MethodReturn(ConstantTrue());
            return LocalMethod(method);
        }
    }
} // end of namespace