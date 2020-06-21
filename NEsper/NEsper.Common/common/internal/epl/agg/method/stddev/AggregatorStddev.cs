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
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
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
        private readonly CodegenExpressionMember _cnt;
        private readonly CodegenExpressionMember _mean;
        private readonly CodegenExpressionMember _qn;

        public AggregatorStddev(
            AggregationForgeFactory factory,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter)
            : base(factory, col, rowCtor, membersColumnized, classScope, optionalDistinctValueType, optionalDistinctSerde, hasFilter, optionalFilter)
        {
            _mean = membersColumnized.AddMember(col, typeof(double), "mean");
            _qn = membersColumnized.AddMember(col, typeof(double), "qn");
            _cnt = membersColumnized.AddMember(col, typeof(long), "cnt");
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
            ApplyEvalEnterNonNull(method, ExprDotMethod(Cast(typeof(object), value), "AsDouble"));
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            ApplyEvalLeaveNonNull(method, ExprDotMethod(Cast(typeof(object), value), "AsDouble"));
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
            method.Block.IfCondition(Relational(_cnt, LT, Constant(2)))
                .BlockReturn(ConstantNull())
                .MethodReturn(StaticMethod(typeof(Math), "Sqrt", Op(_qn, "/", Op(_cnt, "-", Constant(1)))));
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
            method.Block.Apply(WriteDouble(output, row, _mean))
                .Apply(WriteDouble(output, row, _qn))
                .Apply(WriteLong(output, row, _cnt));
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenExpressionRef unitKey,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Apply(ReadDouble(row, _mean, input))
                .Apply(ReadDouble(row, _qn, input))
                .Apply(ReadLong(row, _cnt, input));
        }

        private void ApplyEvalEnterNonNull(
            CodegenMethod method,
            CodegenExpression doubleExpression)
        {
            method.Block.DeclareVar<double>("p", doubleExpression)
                .IfCondition(EqualsIdentity(_cnt, Constant(0)))
                .AssignRef(_mean, Ref("p"))
                .AssignRef(_qn, Constant(0))
                .AssignRef(_cnt, Constant(1))
                .IfElse()
                .Increment(_cnt)
                .DeclareVar<double>("oldmean", _mean)
                .AssignCompound(_mean, "+", Op(Op(Ref("p"), "-", _mean), "/", _cnt))
                .AssignCompound(_qn, "+", Op(Op(Ref("p"), "-", Ref("oldmean")), "*", Op(Ref("p"), "-", _mean)));
        }

        private void ApplyEvalLeaveNonNull(
            CodegenMethod method,
            CodegenExpression doubleExpression)
        {
            method.Block.DeclareVar<double>("p", doubleExpression)
                .IfCondition(Relational(_cnt, LE, Constant(1)))
                .Apply(GetClear())
                .IfElse()
                .Decrement(_cnt)
                .DeclareVar<double>("oldmean", _mean)
                .AssignCompound(_mean, "-", Op(Op(Ref("p"), "-", _mean), "/", _cnt))
                .AssignCompound(_qn, "-", Op(Op(Ref("p"), "-", Ref("oldmean")), "*", Op(Ref("p"), "-", _mean)));
        }

        private Consumer<CodegenBlock> GetClear()
        {
            return block => {
                block.AssignRef(_mean, Constant(0))
                    .AssignRef(_qn, Constant(0))
                    .AssignRef(_cnt, Constant(0));
            };
        }
    }
} // end of namespace