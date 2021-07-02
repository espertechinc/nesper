///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
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
    /// Represents a in-subselect evaluation strategy.
    /// </summary>
    public class SubselectForgeNREqualsIn : SubselectForgeNREqualsInBase
    {
        private readonly ExprForge _filterEval;

        public SubselectForgeNREqualsIn(
            ExprSubselectNode subselect,
            ExprForge valueEval,
            ExprForge selectEval,
            bool resultWhenNoMatchingEvents,
            bool isNotIn,
            Coercer coercer,
            ExprForge filterEval)
            : base(subselect, valueEval, selectEval, resultWhenNoMatchingEvents, isNotIn, coercer)
        {
            this._filterEval = filterEval;
        }

        protected override CodegenExpression CodegenEvaluateInternal(
            CodegenMethodScope parent,
            SubselectForgeNRSymbol symbols,
            CodegenClassScope classScope)
        {
            if (subselect.EvaluationType.IsNullType()) {
                return ConstantNull();
            }
            
            var method = parent.MakeChild(typeof(bool?), GetType(), classScope);
            var left = symbols.GetAddLeftResult(method);
            method.Block.DeclareVar<bool>("hasNullRow", ConstantFalse());
            var @foreach = method.Block.ForEach(typeof(EventBean), "theEvent", symbols.GetAddMatchingEvents(method));
            {
                @foreach.AssignArrayElement(NAME_EPS, Constant(0), Ref("theEvent"));
                if (_filterEval != null) {
                    CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
                        @foreach,
                        _filterEval.EvaluationType,
                        _filterEval.EvaluateCodegen(typeof(bool?), method, symbols, classScope));
                }

                @foreach.IfNullReturnNull(left);

                Type valueRightType;
                if (selectEval != null) {
                    valueRightType = Boxing.GetBoxedType(selectEval.EvaluationType);
                    @foreach.DeclareVar(
                        valueRightType,
                        "valueRight",
                        selectEval.EvaluateCodegen(valueRightType, method, symbols, classScope));
                }
                else {
                    valueRightType = subselect.RawEventType.UnderlyingType;
                    @foreach.DeclareVar(
                        valueRightType,
                        "valueRight",
                        Cast(valueRightType, ExprDotUnderlying(ArrayAtIndex(symbols.GetAddEPS(method), Constant(0)))));
                }

                var ifRight = @foreach.IfCondition(NotEqualsNull(Ref("valueRight")));
                {
                    if (coercer == null) {
                        ifRight.IfCondition(ExprDotMethod(left, "Equals", Ref("valueRight")))
                            .BlockReturn(Constant(!isNotIn));
                    }
                    else {
                        ifRight.DeclareVar<object>("left", coercer.CoerceCodegen(left, symbols.LeftResultType))
                            .DeclareVar<object>("right", coercer.CoerceCodegen(Ref("valueRight"), valueRightType))
                            .DeclareVar<bool>("eq", StaticMethod<object>("Equals", Ref("left"), Ref("right")))
                            .IfCondition(Ref("eq"))
                            .BlockReturn(Constant(!isNotIn));
                    }
                }
                ifRight.IfElse().AssignRef("hasNullRow", ConstantTrue());
            }

            method.Block
                .IfCondition(Ref("hasNullRow"))
                .BlockReturn(ConstantNull())
                .MethodReturn(Constant(isNotIn));
            return LocalMethod(method);
        }
    }
} // end of namespace