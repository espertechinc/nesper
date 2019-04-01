///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.method.core
{
    public abstract class AggregatorMethodWDistinctWFilterWValueBase : AggregatorMethodWDistinctWFilterBase
    {
        public AggregatorMethodWDistinctWFilterWValueBase(
            AggregationForgeFactory factory, int col, CodegenCtor rowCtor, CodegenMemberCol membersColumnized,
            CodegenClassScope classScope, Type optionalDistinctValueType, bool hasFilter, ExprNode optionalFilter)
            : base(factory, col, rowCtor, membersColumnized, classScope, optionalDistinctValueType, hasFilter, optionalFilter)
        {
        }

        protected abstract void ApplyEvalEnterNonNull(
            CodegenExpressionRef value, Type valueType, CodegenMethod method, ExprForgeCodegenSymbol symbols,
            ExprForge[] forges, CodegenClassScope classScope);

        protected abstract void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value, Type valueType, CodegenMethod method, ExprForgeCodegenSymbol symbols,
            ExprForge[] forges, CodegenClassScope classScope);

        protected abstract void ApplyTableEnterNonNull(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope);

        protected abstract void ApplyTableLeaveNonNull(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope);

        protected override void ApplyEvalEnterFiltered(
            CodegenMethod method, ExprForgeCodegenSymbol symbols, ExprForge[] forges, CodegenClassScope classScope)
        {
            ApplyEvalValuePrefix(true, method, symbols, forges, classScope);
            ApplyEvalEnterNonNull(Ref("val"), forges[0].EvaluationType, method, symbols, forges, classScope);
        }

        protected override void ApplyTableEnterFiltered(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            ApplyTableValuePrefix(true, value, method, classScope);
            ApplyTableEnterNonNull(value, evaluationTypes, method, classScope);
        }

        protected override void ApplyEvalLeaveFiltered(
            CodegenMethod method, ExprForgeCodegenSymbol symbols, ExprForge[] forges, CodegenClassScope classScope)
        {
            ApplyEvalValuePrefix(false, method, symbols, forges, classScope);
            ApplyEvalLeaveNonNull(Ref("val"), forges[0].EvaluationType, method, symbols, forges, classScope);
        }

        protected override void ApplyTableLeaveFiltered(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            ApplyTableValuePrefix(false, value, method, classScope);
            ApplyTableLeaveNonNull(value, evaluationTypes, method, classScope);
        }

        private void ApplyEvalValuePrefix(
            bool enter, CodegenMethod method, ExprForgeCodegenSymbol symbols, ExprForge[] forges,
            CodegenClassScope classScope)
        {
            var type = forges[0].EvaluationType;
            var expr = forges[0].EvaluateCodegen(type, method, symbols, classScope);
            method.Block.DeclareVar(type, "val", expr);
            if (!type.IsPrimitive) {
                method.Block.IfRefNull("val").BlockReturnNoValue();
            }

            if (distinct != null) {
                method.Block.IfCondition(Not(ExprDotMethod(distinct, enter ? "add" : "remove", Ref("val"))))
                    .BlockReturnNoValue();
            }
        }

        private void ApplyTableValuePrefix(
            bool enter, CodegenExpressionRef value, CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.IfCondition(EqualsNull(value)).BlockReturnNoValue();
            if (distinct != null) {
                method.Block.IfCondition(Not(ExprDotMethod(distinct, enter ? "add" : "remove", value)))
                    .BlockReturnNoValue();
            }
        }
    }
} // end of namespace