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
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.stddev
{
    /// <summary>
    ///     Standard deviation always generates double-typed numbers.
    /// </summary>
    public class AggregatorStddev : AggregatorMethodWDistinctWFilterWValueBase
    {
        private readonly CodegenExpressionRef cnt;
        private readonly CodegenExpressionRef mean;
        private readonly CodegenExpressionRef qn;

        public AggregatorStddev(
            AggregationForgeFactory factory,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            Type optionalDistinctValueType,
            bool hasFilter,
            ExprNode optionalFilter)
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
            mean = membersColumnized.AddMember(col, typeof(double), "mean");
            qn = membersColumnized.AddMember(col, typeof(double), "qn");
            cnt = membersColumnized.AddMember(col, typeof(long), "cnt");
        }

        protected override void ApplyEvalEnterNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            ApplyEvalEnterNonNull(
                method,
                SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(value, valueType));
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            ApplyEvalLeaveNonNull(
                method,
                SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(value, valueType));
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            ApplyEvalEnterNonNull(method, ExprDotMethod(Cast(typeof(object), value), "doubleValue"));
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            ApplyEvalLeaveNonNull(method, ExprDotMethod(Cast(typeof(object), value), "doubleValue"));
        }

        protected override void ClearWODistinct(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Apply(GetClear());
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.IfCondition(Relational(cnt, LT, Constant(2)))
                .BlockReturn(ConstantNull())
                .MethodReturn(StaticMethod(typeof(Math), "sqrt", Op(qn, "/", Op(cnt, "-", Constant(1)))));
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
            method.Block.Apply(WriteDouble(output, row, mean))
                .Apply(WriteDouble(output, row, qn))
                .Apply(WriteLong(output, row, cnt));
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenExpressionRef unitKey,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Apply(ReadDouble(row, mean, input))
                .Apply(ReadDouble(row, qn, input))
                .Apply(ReadLong(row, cnt, input));
        }

        private void ApplyEvalEnterNonNull(
            CodegenMethod method,
            CodegenExpression doubleExpression)
        {
            method.Block.DeclareVar<double>("p", doubleExpression)
                .IfCondition(EqualsIdentity(cnt, Constant(0)))
                .AssignRef(mean, Ref("p"))
                .AssignRef(qn, Constant(0))
                .AssignRef(cnt, Constant(1))
                .IfElse()
                .Increment(cnt)
                .DeclareVar<double>("oldmean", mean)
                .AssignCompound(mean, "+", Op(Op(Ref("p"), "-", mean), "/", cnt))
                .AssignCompound(qn, "+", Op(Op(Ref("p"), "-", Ref("oldmean")), "*", Op(Ref("p"), "-", mean)));
        }

        private void ApplyEvalLeaveNonNull(
            CodegenMethod method,
            CodegenExpression doubleExpression)
        {
            method.Block.DeclareVar<double>("p", doubleExpression)
                .IfCondition(Relational(cnt, LE, Constant(1)))
                .Apply(GetClear())
                .IfElse()
                .Decrement(cnt)
                .DeclareVar<double>("oldmean", mean)
                .AssignCompound(mean, "-", Op(Op(Ref("p"), "-", mean), "/", cnt))
                .AssignCompound(qn, "-", Op(Op(Ref("p"), "-", Ref("oldmean")), "*", Op(Ref("p"), "-", mean)));
        }

        private Consumer<CodegenBlock> GetClear()
        {
            return block => {
                block.AssignRef(mean, Constant(0))
                    .AssignRef(qn, Constant(0))
                    .AssignRef(cnt, Constant(0));
            };
        }
    }
} // end of namespace