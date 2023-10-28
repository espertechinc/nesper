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
        private readonly ExprSubselectNode _subselect;
        private readonly ExprForge _havingEval;

        public SubselectForgeNRExistsWGroupByWHaving(
            ExprSubselectNode subselect,
            ExprForge havingEval)
        {
            _subselect = subselect;
            _havingEval = havingEval;
        }

        public CodegenExpression EvaluateMatchesCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            var aggService = classScope.NamespaceScope.AddOrGetDefaultFieldWellKnown(
                new CodegenFieldNameSubqueryAgg(_subselect.SubselectNumber),
                typeof(AggregationResultFuture));

            var method = parent.MakeChild(typeof(bool), GetType(), classScope);
            var evalCtx = symbols.GetAddExprEvalCtx(method);

            method.Block
                .ApplyTri(new ReturnIfNoMatch(ConstantFalse(), ConstantFalse()), method, symbols)
                .DeclareVar(typeof(int), "cpid", ExprDotName(evalCtx, "AgentInstanceId"))
                .DeclareVar(typeof(AggregationService), "aggregationService", ExprDotMethod(aggService, "GetContextPartitionAggregationService", Ref("cpid")))
                .DeclareVar(typeof(ICollection<object>), "groupKeys", ExprDotMethod(Ref("aggregationService"), "GetGroupKeys", evalCtx));
            method.Block.ApplyTri(DECLARE_EVENTS_SHIFTED, method, symbols);

            var forEach = method.Block.ForEach<object>("groupKey", Ref("groupKeys"));
            {
                forEach.ExprDotMethod(
                    Ref("aggregationService"),
                    "SetCurrentAccess",
                    Ref("groupKey"),
                    Ref("cpid"),
                    ConstantNull());
                CodegenLegoBooleanExpression.CodegenContinueIfNullOrNotPass(
                    forEach,
                    _havingEval.EvaluationType,
                    _havingEval.EvaluateCodegen(_havingEval.EvaluationType, method, symbols, classScope));
                forEach.BlockReturn(ConstantTrue());
            }
            method.Block.MethodReturn(ConstantFalse());

            return LocalMethod(method);
        }
    }
} // end of namespace