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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

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
            bool isAll)
            : base(subselect, valueEval, selectEval, resultWhenNoMatchingEvents, isNot, coercer)
        {
            this.filterEval = filterEval;
            this.isAll = isAll;
        }

        protected override CodegenExpression CodegenEvaluateInternal(
            CodegenMethodScope parent,
            SubselectForgeNRSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(bool?), this.GetType(), classScope);
            var left = symbols.GetAddLeftResult(method);
            method.Block.DeclareVar<bool>("hasNullRow", ConstantFalse());
            var @foreach = method.Block.ForEach(typeof(EventBean), "theEvent", symbols.GetAddMatchingEvents(method));
            {
                @foreach.AssignArrayElement(NAME_EPS, Constant(0), Ref("theEvent"));
                if (filterEval != null) {
                    CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
                        @foreach,
                        filterEval.EvaluationType,
                        filterEval.EvaluateCodegen(typeof(bool?), method, symbols, classScope));
                }

                @foreach.IfRefNullReturnNull(left);

                Type valueRightType;
                if (selectEval != null) {
                    valueRightType = Boxing.GetBoxedType(selectEval.EvaluationType);
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

                var ifRight = @foreach.IfCondition(NotEqualsNull(Ref("valueRight")));
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
                        ifRight.DeclareVar<object>("left", coercer.CoerceCodegen(left, symbols.LeftResultType))
                            .DeclareVar<object>("right", coercer.CoerceCodegen(Ref("valueRight"), valueRightType))
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