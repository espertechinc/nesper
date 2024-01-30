///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    /// <summary>
    /// Strategy for subselects with "=/!=/&gt;&lt; ALL".
    /// </summary>
    public class SubselectForgeNREqualsAllAnyWGroupBy : SubselectForgeNREqualsBase
    {
        private readonly ExprForge _havingEval;
        private readonly bool _isAll;

        public SubselectForgeNREqualsAllAnyWGroupBy(
            ExprSubselectNode subselect,
            ExprForge valueEval,
            ExprForge selectEval,
            bool resultWhenNoMatchingEvents,
            bool isNot,
            Coercer coercer,
            ExprForge havingEval,
            bool isAll) : base(subselect, valueEval, selectEval, resultWhenNoMatchingEvents, isNot, coercer)
        {
            _havingEval = havingEval;
            _isAll = isAll;
        }

        protected override CodegenExpression CodegenEvaluateInternal(
            CodegenMethodScope parent,
            SubselectForgeNRSymbol symbols,
            CodegenClassScope classScope)
        {
            if (subselect.EvaluationType == null) {
                return ConstantNull();
            }

            var aggService = classScope.NamespaceScope.AddOrGetDefaultFieldWellKnown(
                new CodegenFieldNameSubqueryAgg(subselect.SubselectNumber),
                typeof(AggregationResultFuture));

            var method = parent.MakeChild(subselect.EvaluationType, GetType(), classScope);
            var evalCtx = symbols.GetAddExprEvalCtx(method);
            var left = symbols.GetAddLeftResult(method);

            method.Block
                .DeclareVar<int>("cpid", ExprDotName(evalCtx, "AgentInstanceId"))
                .DeclareVar<AggregationService>(
                    "aggregationService",
                    ExprDotMethod(aggService, "GetContextPartitionAggregationService", Ref("cpid")))
                .DeclareVar<ICollection<object>>(
                    "groupKeys",
                    ExprDotMethod(Ref("aggregationService"), "GetGroupKeys", evalCtx))
                .DeclareVar<bool>("hasNullRow", ConstantFalse());

            var forEach = method.Block.ForEach<object>("groupKey", Ref("groupKeys"));
            {
                forEach.IfCondition(EqualsNull(left))
                    .BlockReturn(ConstantNull())
                    .ExprDotMethod(
                        Ref("aggregationService"),
                        "SetCurrentAccess",
                        Ref("groupKey"),
                        Ref("cpid"),
                        ConstantNull());

                if (_havingEval != null) {
                    CodegenLegoBooleanExpression.CodegenContinueIfNullOrNotPass(
                        forEach,
                        _havingEval.EvaluationType,
                        _havingEval.EvaluateCodegen(_havingEval.EvaluationType, method, symbols, classScope));
                }

                Type valueRightType;
                if (selectEval != null) {
                    valueRightType = selectEval.EvaluationType.GetBoxedType();
                    forEach.DeclareVar(
                        valueRightType,
                        "valueRight",
                        selectEval.EvaluateCodegen(valueRightType, method, symbols, classScope));
                }
                else {
                    valueRightType = typeof(object);
                    forEach.DeclareVar(
                        valueRightType,
                        "valueRight",
                        ExprDotUnderlying(ArrayAtIndex(symbols.GetAddEps(method), Constant(0))));
                }

                var ifRightNotNull = forEach.IfCondition(EqualsNull(Ref("valueRight")))
                    .AssignRef("hasNullRow", ConstantTrue())
                    .IfElse();
                {
                    if (coercer == null) {
                        ifRightNotNull.DeclareVar<bool>("eq", ExprDotMethod(left, "Equals", Ref("valueRight")));
                    }
                    else {
                        ifRightNotNull.DeclareVar<object>(
                                "left",
                                coercer.CoerceCodegen(left, symbols.LeftResultType, method, classScope))
                            .DeclareVar<object>(
                                "right",
                                coercer.CoerceCodegen(Ref("valueRight"), valueRightType, method, classScope))
                            .DeclareVar<bool>("eq", ExprDotMethod(Ref("left"), "Equals", Ref("right")));
                    }

                    if (isNot) {
                        if (_isAll) {
                            ifRightNotNull.IfCondition(Ref("eq")).BlockReturn(ConstantFalse());
                        }
                        else {
                            ifRightNotNull.IfCondition(Not(Ref("eq"))).BlockReturn(ConstantTrue());
                        }
                    }
                    else {
                        if (_isAll) {
                            ifRightNotNull.IfCondition(Not(Ref("eq"))).BlockReturn(ConstantFalse());
                        }
                        else {
                            ifRightNotNull.IfCondition(Ref("eq")).BlockReturn(ConstantTrue());
                        }
                    }
                }
            }

            method.Block
                .IfCondition(Ref("hasNullRow"))
                .BlockReturn(ConstantNull())
                .MethodReturn(_isAll ? ConstantTrue() : ConstantFalse());

            return LocalMethod(method);
        }
    }
} // end of namespace