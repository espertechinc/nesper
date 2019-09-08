///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.epl.agg.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public class SubselectForgeNRExistsWGroupBy : SubselectForgeNR
    {
        private readonly ExprSubselectNode subselect;

        public SubselectForgeNRExistsWGroupBy(ExprSubselectNode subselect)
        {
            this.subselect = subselect;
        }

        public CodegenExpression EvaluateMatchesCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(bool), GetType(), classScope);
            CodegenExpression aggService = classScope.NamespaceScope.AddOrGetFieldWellKnown(
                new CodegenFieldNameSubqueryAgg(subselect.SubselectNumber),
                typeof(AggregationResultFuture));

            method.Block.ApplyTri(
                new SubselectForgeCodegenUtil.ReturnIfNoMatch(ConstantFalse(), ConstantFalse()),
                method,
                symbols);
            method.Block.MethodReturn(
                Not(
                    ExprDotMethodChain(aggService)
                        .Add("GetGroupKeys", symbols.GetAddExprEvalCtx(method))
                        .Add("IsEmpty")));
            return LocalMethod(method);
        }
    }
} // end of namespace