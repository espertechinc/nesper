///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeNRRelOpAnyWGroupBy : SubselectForgeNRRelOpBase
    {
        private readonly ExprForge havingEval;

        public SubselectForgeNRRelOpAnyWGroupBy(
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
            CodegenExpression aggService = classScope.NamespaceScope.AddOrGetFieldWellKnown(
                new CodegenFieldNameSubqueryAgg(subselect.SubselectNumber),
                typeof(AggregationResultFuture));

            var method = parent.MakeChild(subselect.EvaluationType, GetType(), classScope);
            var evalCtx = symbols.GetAddExprEvalCtx(method);
            var left = symbols.GetAddLeftResult(method);

            method.Block
                .DeclareVar<int>("cpid", ExprDotMethod(evalCtx, "getAgentInstanceId"))
                .DeclareVar<AggregationService>(
                    "aggregationService",
                    ExprDotMethod(aggService, "getContextPartitionAggregationService", Ref("cpid")))
                .DeclareVar<ICollection<object>>(
                    "groupKeys",
                    ExprDotMethod(Ref("aggregationService"), "getGroupKeys", evalCtx))
                .DeclareVar<bool>("hasRows", ConstantFalse())
                .DeclareVar<bool>("hasNonNullRow", ConstantFalse());

            var forEach = method.Block.ForEach(typeof(object), "groupKey", Ref("groupKeys"));
            {
                forEach.ExprDotMethod(
                    Ref("aggregationService"),
                    "SetCurrentAccess",
                    Ref("groupKey"),
                    Ref("cpid"),
                    ConstantNull());

                if (havingEval != null) {
                    CodegenLegoBooleanExpression.CodegenContinueIfNullOrNotPass(
                        forEach,
                        havingEval.EvaluationType,
                        havingEval.EvaluateCodegen(havingEval.EvaluationType, method, symbols, classScope));
                }

                forEach.AssignRef("hasRows", ConstantTrue());

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
                        ExprDotUnderlying(ArrayAtIndex(symbols.GetAddEPS(method), Constant(0))));
                }

                forEach.IfCondition(NotEqualsNull(Ref("valueRight")))
                    .AssignRef("hasNonNullRow", ConstantTrue())
                    .BlockEnd()
                    .IfCondition(And(NotEqualsNull(left), NotEqualsNull(Ref("valueRight"))))
                    .IfCondition(computer.Codegen(left, symbols.LeftResultType, Ref("valueRight"), valueRightType))
                    .BlockReturn(ConstantTrue());
            }

            method.Block
                .IfCondition(Not(Ref("hasRows")))
                .BlockReturn(ConstantFalse())
                .IfCondition(Not(Ref("hasNonNullRow")))
                .BlockReturn(ConstantNull())
                .IfCondition(EqualsNull(left))
                .BlockReturn(ConstantNull())
                .MethodReturn(ConstantFalse());

            return LocalMethod(method);
        }
    }
} // end of namespace