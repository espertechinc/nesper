///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.subquery.SubselectForgeCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeNRExistsWGroupByWHaving : SubselectForgeNR
    {
        private readonly ExprSubselectNode subselect;
        private readonly ExprForge havingEval;

        public SubselectForgeNRExistsWGroupByWHaving(
            ExprSubselectNode subselect,
            ExprForge havingEval)
        {
            this.subselect = subselect;
            this.havingEval = havingEval;
        }

        public CodegenExpression EvaluateMatchesCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpression aggService = classScope.NamespaceScope.AddOrGetFieldWellKnown(
                new CodegenFieldNameSubqueryAgg(subselect.SubselectNumber),
                typeof(AggregationResultFuture));

            var method = parent.MakeChild(typeof(bool), GetType(), classScope);
            var evalCtx = symbols.GetAddExprEvalCtx(method);

            method.Block
                .ApplyTri(new ReturnIfNoMatch(ConstantFalse(), ConstantFalse()), method, symbols)
                .DeclareVar(typeof(int), "cpid", ExprDotName(evalCtx, "AgentInstanceId"))
                .DeclareVar(
                    typeof(AggregationService),
                    "aggregationService",
                    ExprDotMethod(aggService, "getContextPartitionAggregationService", Ref("cpid")))
                .DeclareVar(
                    typeof(ICollection<object>),
                    "groupKeys",
                    ExprDotMethod(Ref("aggregationService"), "getGroupKeys", evalCtx));
            method.Block.ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols);

            var forEach = method.Block.ForEach(typeof(object), "groupKey", Ref("groupKeys"));
            {
                forEach.ExprDotMethod(
                    Ref("aggregationService"),
                    "SetCurrentAccess",
                    Ref("groupKey"),
                    Ref("cpid"),
                    ConstantNull());
                CodegenLegoBooleanExpression.CodegenContinueIfNullOrNotPass(
                    forEach,
                    havingEval.EvaluationType,
                    havingEval.EvaluateCodegen(havingEval.EvaluationType, method, symbols, classScope));
                forEach.BlockReturn(ConstantTrue());
            }
            method.Block.MethodReturn(ConstantFalse());

            return LocalMethod(method);
        }
    }
} // end of namespace