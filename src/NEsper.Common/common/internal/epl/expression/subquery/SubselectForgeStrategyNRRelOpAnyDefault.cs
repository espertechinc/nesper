///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class SubselectForgeStrategyNRRelOpAnyDefault : SubselectForgeNRRelOpBase
    {
        private readonly ExprForge filterEval;

        public SubselectForgeStrategyNRRelOpAnyDefault(
            ExprSubselectNode subselect,
            ExprForge valueEval,
            ExprForge selectEval,
            bool resultWhenNoMatchingEvents,
            RelationalOpEnumComputer computer,
            ExprForge filterEval) : base(subselect, valueEval, selectEval, resultWhenNoMatchingEvents, computer)
        {
            this.filterEval = filterEval;
        }

        protected override CodegenExpression CodegenEvaluateInternal(
            CodegenMethodScope parent,
            SubselectForgeNRSymbol symbols,
            CodegenClassScope classScope)
        {
            if (subselect.EvaluationType == null) {
                return ConstantNull();
            }

            var method = parent.MakeChild(typeof(bool?), GetType(), classScope);
            method.Block
                .DeclareVar(typeof(bool?), "hasRows", ConstantFalse())
                .DeclareVar(typeof(bool?), "hasNonNullRow", ConstantFalse());
            var foreachX = method.Block.ForEach(
                typeof(EventBean),
                "subselectEvent",
                symbols.GetAddMatchingEvents(method));
            {
                foreachX.AssignArrayElement(NAME_EPS, Constant(0), Ref("subselectEvent"));
                if (filterEval != null) {
                    CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
                        foreachX,
                        filterEval.EvaluationType,
                        filterEval.EvaluateCodegen(typeof(bool), method, symbols, classScope));
                }

                foreachX.AssignRef("hasRows", ConstantTrue());

                Type valueRightType;
                if (selectEval != null) {
                    valueRightType = selectEval.EvaluationType.GetBoxedType();
                    foreachX.DeclareVar(
                        valueRightType,
                        "valueRight",
                        selectEval.EvaluateCodegen(valueRightType, method, symbols, classScope));
                }
                else {
                    valueRightType = typeof(object);
                    foreachX.DeclareVar(
                        valueRightType,
                        "valueRight",
                        ExprDotUnderlying(ArrayAtIndex(symbols.GetAddEPS(method), Constant(0))));
                }

                foreachX.IfCondition(NotEqualsNull(Ref("valueRight")))
                    .AssignRef("hasNonNullRow", ConstantTrue())
                    .BlockEnd()
                    .IfCondition(And(NotEqualsNull(symbols.GetAddLeftResult(method)), NotEqualsNull(Ref("valueRight"))))
                    .IfCondition(
                        computer.Codegen(
                            symbols.GetAddLeftResult(method),
                            symbols.LeftResultType,
                            Ref("valueRight"),
                            valueRightType))
                    .BlockReturn(ConstantTrue());
            }

            method.Block
                .IfCondition(Not(Ref("hasRows")))
                .BlockReturn(ConstantFalse())
                .IfCondition(Or(Not(Ref("hasNonNullRow")), EqualsNull(symbols.GetAddLeftResult(method))))
                .BlockReturn(ConstantNull())
                .MethodReturn(ConstantFalse());
            return LocalMethod(method);
        }
    }
} // end of namespace