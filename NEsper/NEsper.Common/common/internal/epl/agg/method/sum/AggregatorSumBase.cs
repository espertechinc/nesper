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
using com.espertech.esper.common.@internal.util;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.sum
{
    public abstract class AggregatorSumBase : AggregatorMethodWDistinctWFilterWValueBase
    {
        internal readonly CodegenExpressionRef cnt;
        internal readonly CodegenExpressionRef sum;
        internal readonly Type sumType;

        public AggregatorSumBase(
            AggregationForgeFactory factory, int col, CodegenCtor rowCtor, CodegenMemberCol membersColumnized,
            CodegenClassScope classScope, Type optionalDistinctValueType, bool hasFilter, ExprNode optionalFilter,
            Type sumType) : base(
            factory, col, rowCtor, membersColumnized, classScope, optionalDistinctValueType, hasFilter, optionalFilter)
        {
            cnt = membersColumnized.AddMember(col, typeof(long), "cnt");
            sum = membersColumnized.AddMember(col, sumType.GetPrimitiveType(), "sum");
            this.sumType = sumType;
            rowCtor.Block.AssignRef(sum, InitOfSum());
        }

        protected abstract CodegenExpression InitOfSum();

        protected abstract void ApplyAggEnterSum(CodegenExpressionRef value, Type valueType, CodegenMethod method);

        protected abstract void ApplyTableEnterSum(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope);

        protected abstract void ApplyAggLeaveSum(CodegenExpressionRef value, Type valueType, CodegenMethod method);

        protected abstract void ApplyTableLeaveSum(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope);

        protected abstract void WriteSum(
            CodegenExpressionRef row, CodegenExpressionRef output, CodegenMethod method, CodegenClassScope classScope);

        protected abstract void ReadSum(
            CodegenExpressionRef row, CodegenExpressionRef input, CodegenMethod method, CodegenClassScope classScope);

        protected override void ApplyEvalEnterNonNull(
            CodegenExpressionRef value, Type valueType, CodegenMethod method, ExprForgeCodegenSymbol symbols,
            ExprForge[] forges, CodegenClassScope classScope)
        {
            method.Block.Increment(cnt);
            ApplyAggEnterSum(value, valueType, method);
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.Increment(cnt);
            ApplyTableEnterSum(value, evaluationTypes, method, classScope);
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value, Type valueType, CodegenMethod method, ExprForgeCodegenSymbol symbols,
            ExprForge[] forges, CodegenClassScope classScope)
        {
            method.Block
                .IfCondition(Relational(cnt, LE, Constant(1)))
                .AssignRef(cnt, Constant(0))
                .AssignRef(sum, InitOfSum())
                .BlockReturnNoValue()
                .Decrement(cnt);
            ApplyAggLeaveSum(value, valueType, method);
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block
                .IfCondition(Relational(cnt, LE, Constant(1)))
                .AssignRef(cnt, Constant(0))
                .AssignRef(sum, InitOfSum())
                .BlockReturnNoValue()
                .Decrement(cnt);
            ApplyTableLeaveSum(value, evaluationTypes, method, classScope);
        }

        protected override void ClearWODistinct(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block
                .AssignRef(cnt, Constant(0))
                .AssignRef(sum, InitOfSum());
        }

        public override void GetValueCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.IfCondition(EqualsIdentity(cnt, Constant(0)))
                .BlockReturn(ConstantNull())
                .MethodReturn(sum);
        }

        protected override void WriteWODistinct(
            CodegenExpressionRef row, int col, CodegenExpressionRef output, CodegenExpressionRef unitKey,
            CodegenExpressionRef writer, CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.Apply(WriteLong(output, row, cnt));
            WriteSum(row, output, method, classScope);
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row, int col, CodegenExpressionRef input, CodegenExpressionRef unitKey,
            CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.Apply(ReadLong(row, cnt, input));
            ReadSum(row, input, method, classScope);
        }
    }
} // end of namespace