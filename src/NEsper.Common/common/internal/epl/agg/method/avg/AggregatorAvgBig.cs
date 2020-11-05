///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;
using System.Reflection;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.avg
{
    /// <summary>
    ///     Average that generates double-typed numbers.
    /// </summary>
    public class AggregatorAvgBig : AggregatorMethodWDistinctWFilterWValueBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AggregationForgeFactoryAvg _factory;
        private readonly CodegenExpressionMember _sum;
        private readonly CodegenExpressionMember _cnt;

        public AggregatorAvgBig(
            AggregationForgeFactoryAvg factory,
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
            _factory = factory;
            _sum = membersColumnized.AddMember(col, typeof(BigInteger), "sum");
            _cnt = membersColumnized.AddMember(col, typeof(long), "cnt");
            rowCtor.Block.AssignRef(_sum, EnumValue(typeof(BigInteger), "Zero"));
        }

        protected override void ApplyEvalEnterNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(
                _sum,
                Op(_sum, "+", ExprDotMethod(value, "AsBigInteger")));

            method.Block.Increment(_cnt);
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            method.Block
                .IfCondition(Relational(_cnt, LE, Constant(1)))
                .Apply(ClearCode())
                .IfElse()
                .Decrement(_cnt)
                .Apply(
                    block => block.AssignRef(
                        _sum,
                        Op(_sum, "-", ExprDotMethod(value, "AsBigInteger"))));
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(
                _sum, Op(_sum, "+", ExprDotMethod(value, "AsBigInteger")));
            method.Block.Increment(_cnt);
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block
                .IfCondition(Relational(_cnt, LE, Constant(1)))
                .Apply(ClearCode())
                .IfElse()
                .Decrement(_cnt)
                .Apply(
                    block => block.AssignRef(
                        _sum, Op(_sum, "-", ExprDotMethod(value, "AsBigInteger"))));
        }

        protected override void ClearWODistinct(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Apply(ClearCode());
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            var math = _factory.OptionalMathContext == null
                ? ConstantNull()
                : classScope.AddOrGetDefaultFieldSharable(new MathContextCodegenField(_factory.OptionalMathContext));
            method.Block.MethodReturn(Op(_sum, "/", _cnt));
            //StaticMethod(GetType(), "GetValueDivide", cnt, math, sum));
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
            method.Block
                .Apply(WriteLong(output, row, _cnt))
                .StaticMethod(typeof(DIOBigIntegerUtil), "WriteBigInt", RowDotMember(row, _sum), output);
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenExpressionRef unitKey,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block
                .Apply(ReadLong(row, _cnt, input))
                .AssignRef(
                    RowDotMember(row, _sum),
                    StaticMethod(typeof(DIOBigIntegerUtil), "ReadBigInt", input));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="cnt">count</param>
        /// <param name="optionalMathContext">math ctx</param>
        /// <param name="sum">sum</param>
        /// <returns>result</returns>
        public static BigInteger? GetValueDivide(
            long cnt,
            MathContext optionalMathContext,
            BigInteger sum)
        {
            if (cnt == 0) {
                return null;
            }

            try {
                if (optionalMathContext == null) {
                    return sum / cnt;
                }

                return sum / cnt;
            }
            catch (ArithmeticException ex) {
                Log.Error("Error computing avg aggregation result: " + ex.Message, ex);
                return BigInteger.Zero;
            }
        }

        private Consumer<CodegenBlock> ClearCode()
        {
            return block =>
                block
                    .AssignRef(_sum, EnumValue(typeof(BigInteger), "Zero"))
                    .AssignRef(_cnt, Constant(0));
        }
    }
} // end of namespace