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
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.avg
{
    /// <summary>
    ///     Average that generates double-typed numbers.
    /// </summary>
    public class AggregatorAvgBig : AggregatorMethodWDistinctWFilterWValueBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly CodegenExpressionRef cnt;
        private readonly AggregationFactoryMethodAvg factory;
        private readonly CodegenExpressionRef sum;

        public AggregatorAvgBig(
            AggregationFactoryMethodAvg factory,
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
            this.factory = factory;
            sum = membersColumnized.AddMember(col, typeof(decimal), "sum");
            cnt = membersColumnized.AddMember(col, typeof(long), "cnt");
            rowCtor.Block.AssignRef(sum, Constant(0d));
        }

        protected override void ApplyEvalEnterNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            if (valueType == typeof(BigInteger)) {
                method.Block.AssignRef(
                    sum,
                    ExprDotMethod(
                        sum,
                        "add",
                        NewInstance<decimal>(value)));
            }
            else {
                method.Block.AssignRef(
                    sum,
                    ExprDotMethod(
                        sum,
                        "add",
                        valueType == typeof(decimal) ? value : Cast(typeof(decimal), value)));
            }

            method.Block.Increment(cnt);
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            method.Block.IfCondition(Relational(cnt, LE, Constant(1)))
                .Apply(ClearCode())
                .IfElse()
                .Decrement(cnt)
                .Apply(
                    block => {
                        if (valueType == typeof(BigInteger)) {
                            block.AssignRef(
                                sum,
                                ExprDotMethod(
                                    sum,
                                    "subtract",
                                    NewInstance<decimal>(value)));
                        }
                        else {
                            block.AssignRef(
                                sum,
                                ExprDotMethod(
                                    sum,
                                    "subtract",
                                    valueType == typeof(decimal) ? value : Cast(typeof(decimal), value)));
                        }
                    });
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (evaluationTypes[0] == typeof(BigInteger)) {
                method.Block.AssignRef(
                    sum,
                    ExprDotMethod(
                        sum,
                        "add",
                        NewInstance<decimal>(
                            Cast(typeof(BigInteger), value))));
            }
            else {
                method.Block.AssignRef(
                    sum,
                    ExprDotMethod(
                        sum,
                        "add",
                        Cast(typeof(decimal), value)));
            }

            method.Block.Increment(cnt);
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.IfCondition(Relational(cnt, LE, Constant(1)))
                .Apply(ClearCode())
                .IfElse()
                .Decrement(cnt)
                .Apply(
                    block => {
                        if (evaluationTypes[0] == typeof(BigInteger)) {
                            block.AssignRef(
                                sum,
                                ExprDotMethod(
                                    sum,
                                    "subtract",
                                    NewInstance<decimal>(
                                        Cast(typeof(BigInteger), value))));
                        }
                        else {
                            block.AssignRef(
                                sum,
                                ExprDotMethod(
                                    sum,
                                    "subtract",
                                    Cast(typeof(decimal), value)));
                        }
                    });
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
            var math = factory.optionalMathContext == null
                ? ConstantNull()
                : classScope.AddOrGetFieldSharable(new MathContextCodegenField(factory.optionalMathContext));
            method.Block.MethodReturn(StaticMethod(GetType(), "GetValueDecimalDivide", cnt, math, sum));
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
                .Apply(WriteLong(output, row, cnt))
                .StaticMethod(typeof(DIOSerdeBigInteger), "writeBigDec", RowDotRef(row, sum), output);
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
                .Apply(ReadLong(row, cnt, input))
                .AssignRef(
                    RowDotRef(row, sum),
                    StaticMethod(typeof(DIOSerdeBigInteger), "readBigDec", input));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="cnt">count</param>
        /// <param name="optionalMathContext">math ctx</param>
        /// <param name="sum">sum</param>
        /// <returns>result</returns>
        public static decimal? GetValueDecimalDivide(
            long cnt,
            MathContext optionalMathContext,
            decimal sum)
        {
            if (cnt == 0) {
                return null;
            }

            try {
                if (optionalMathContext == null) {
                    return sum / cnt;
                }

                return decimal.Round(
                    decimal.Divide(sum, cnt),
                    optionalMathContext.Precision,
                    optionalMathContext.RoundingMode);
            }
            catch (ArithmeticException ex) {
                Log.Error("Error computing avg aggregation result: " + ex.Message, ex);
                return 0.0m;
            }
        }

        private Consumer<CodegenBlock> ClearCode()
        {
            return block =>
                block.AssignRef(sum, NewInstance<decimal>(Constant(0.0m))).AssignRef(cnt, Constant(0));
        }
    }
} // end of namespace