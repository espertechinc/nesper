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
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.count
{
    public class AggregatorCount : AggregatorMethodWDistinctWFilterBase
    {
        private readonly CodegenExpressionRef cnt;
        private readonly bool isEver;

        public AggregatorCount(
            AggregationForgeFactory factory,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            Type optionalDistinctValueType,
            bool hasFilter,
            ExprNode optionalFilter,
            bool isEver)
            : base(
                factory,
                col,
                rowCtor,
                membersColumnized,
                classScope,
                optionalDistinctValueType,
                hasFilter,
                optionalFilter)
        {
            this.isEver = isEver;
            cnt = membersColumnized.AddMember(col, typeof(long), "cnt");
        }

        protected override void ApplyEvalEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            Consumer<CodegenBlock> increment = block => block.Increment(cnt);

            // handle wildcard
            if (forges.Length == 0 || optionalFilter != null && forges.Length == 1) {
                method.Block.Apply(increment);
                return;
            }

            var evalType = forges[0].EvaluationType;
            method.Block.DeclareVar(
                evalType,
                "value",
                forges[0].EvaluateCodegen(evalType, method, symbols, classScope));
            if (!evalType.IsPrimitive) {
                method.Block.IfRefNull("value").BlockReturnNoValue();
            }

            if (distinct != null) {
                method.Block.IfCondition(Not(ExprDotMethod(distinct, "add", Ref("value")))).BlockReturnNoValue();
            }

            method.Block.Apply(increment);
        }

        protected override void ApplyTableEnterFiltered(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (distinct != null) {
                method.Block.IfCondition(Not(ExprDotMethod(distinct, "add", Ref("value")))).BlockReturnNoValue();
            }

            method.Block.Increment(cnt);
        }

        public override void ApplyEvalLeaveCodegen(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            if (!isEver) {
                base.ApplyEvalLeaveCodegen(method, symbols, forges, classScope);
            }
        }

        protected override void ApplyEvalLeaveFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            Consumer<CodegenBlock> decrement = block =>
                block.IfCondition(Relational(cnt, GT, Constant(0))).Decrement(cnt);

            // handle wildcard
            if (forges.Length == 0 || optionalFilter != null && forges.Length == 1) {
                method.Block.Apply(decrement);
                return;
            }

            var evalType = forges[0].EvaluationType;
            method.Block.DeclareVar(
                evalType,
                "value",
                forges[0].EvaluateCodegen(evalType, method, symbols, classScope));
            if (!evalType.IsPrimitive) {
                method.Block.IfRefNull("value").BlockReturnNoValue();
            }

            if (distinct != null) {
                method.Block.IfCondition(Not(ExprDotMethod(distinct, "remove", Ref("value")))).BlockReturnNoValue();
            }

            method.Block.Apply(decrement);
        }

        protected override void ApplyTableLeaveFiltered(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (distinct != null) {
                method.Block.IfCondition(Not(ExprDotMethod(distinct, "remove", Ref("value")))).BlockReturnNoValue();
            }

            method.Block.Decrement(cnt);
        }

        protected override void ClearWODistinct(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(cnt, Constant(0));
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(cnt);
        }

        protected override void WriteWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef output,
            CodegenExpressionRef unitKey,
            CodegenExpressionRef writer,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Apply(WriteLong(output, row, cnt));
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenExpressionRef unitKey,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Apply(ReadLong(row, cnt, input));
        }
    }
} // end of namespace