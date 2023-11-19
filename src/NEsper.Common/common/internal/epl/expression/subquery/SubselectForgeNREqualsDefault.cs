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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    /// <summary>
    /// Strategy for subselects with "=/!=/&gt;&lt; ALL".
    /// </summary>
    public class SubselectForgeNREqualsDefault : SubselectForgeNREqualsBase
    {
        private readonly ExprForge filterEval;
        private readonly bool isAll;

        public SubselectForgeNREqualsDefault(
            ExprSubselectNode subselect,
            ExprForge valueEval,
            ExprForge selectEval,
            bool resultWhenNoMatchingEvents,
            bool isNot,
            Coercer coercer,
            ExprForge filterEval,
            bool isAll) : base(subselect, valueEval, selectEval, resultWhenNoMatchingEvents, isNot, coercer)
        {
            this.filterEval = filterEval;
            this.isAll = isAll;
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
            var left = symbols.GetAddLeftResult(method);
            method.Block.DeclareVar<bool>("hasNullRow", ConstantFalse());
            var foreachX = method.Block.ForEach(
                typeof(EventBean),
                "theEvent",
                symbols.GetAddMatchingEvents(method));
            {
                foreachX.AssignArrayElement(NAME_EPS, Constant(0), Ref("theEvent"));
                if (filterEval != null) {
                    CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
                        foreachX,
                        filterEval.EvaluationType,
                        filterEval.EvaluateCodegen(typeof(bool), method, symbols, classScope));
                }

                foreachX.IfNullReturnNull(left);

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
                        ExprDotUnderlying(ArrayAtIndex(symbols.GetAddEps(method), Constant(0))));
                }

                var ifRight = foreachX.IfCondition(NotEqualsNull(Ref("valueRight")));
                {
                    if (coercer == null) {
                        ifRight.DeclareVar<bool>("eq", ExprDotMethod(left, "Equals", Ref("valueRight")));
                        if (isAll) {
                            ifRight.IfCondition(NotOptional(!isNot, Ref("eq"))).BlockReturn(ConstantFalse());
                        }
                        else {
                            ifRight.IfCondition(NotOptional(isNot, Ref("eq"))).BlockReturn(ConstantTrue());
                        }
                    }
                    else {
                        ifRight.DeclareVar<object>(
                                "left",
                                coercer.CoerceCodegen(left, symbols.LeftResultType))
                            .DeclareVar<object>(
                                "right",
                                coercer.CoerceCodegen(Ref("valueRight"), valueRightType))
                            .DeclareVar<bool>("eq", ExprDotMethod(Ref("left"), "Equals", Ref("right")));
                        if (isAll) {
                            ifRight.IfCondition(NotOptional(!isNot, Ref("eq"))).BlockReturn(ConstantFalse());
                        }
                        else {
                            ifRight.IfCondition(NotOptional(isNot, Ref("eq"))).BlockReturn(ConstantTrue());
                        }
                    }
                }
                ifRight.IfElse().AssignRef("hasNullRow", ConstantTrue());
            }

            method.Block
                .IfCondition(Ref("hasNullRow"))
                .BlockReturn(ConstantNull())
                .MethodReturn(isAll ? ConstantTrue() : ConstantFalse());
            return LocalMethod(method);
        }
    }
} // end of namespace