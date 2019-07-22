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
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.io;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.serde.CodegenSharableSerdeClassTyped.CodegenSharableSerdeName;

namespace com.espertech.esper.common.@internal.epl.agg.method.nth
{
    public class AggregatorNth : AggregatorMethodWDistinctWFilterWValueBase
    {
        private readonly CodegenExpressionRef circularBuffer;
        private readonly CodegenExpressionRef currentBufferElementPointer;

        private readonly AggregationFactoryMethodNth factory;
        private readonly CodegenExpressionRef numDataPoints;
        private readonly CodegenExpressionField serdeValue;

        public AggregatorNth(
            AggregationFactoryMethodNth factory,
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
            circularBuffer = membersColumnized.AddMember(col, typeof(object[]), "buf");
            currentBufferElementPointer = membersColumnized.AddMember(col, typeof(int), "cbep");
            numDataPoints = membersColumnized.AddMember(col, typeof(long), "cnt");
            serdeValue =
                classScope.AddOrGetFieldSharable(new CodegenSharableSerdeClassTyped(VALUE_NULLABLE, factory.childType));
        }

        protected override void ApplyEvalEnterNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            ApplyEvalEnterNonNull(value, method);
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            ApplyEvalLeaveNonNull(method);
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            ApplyEvalEnterNonNull(value, method);
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            ApplyEvalLeaveNonNull(method);
        }

        protected override void ClearWODistinct(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Apply(ClearCode());
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
            method.Block.StaticMethod(
                GetType(),
                "Write",
                output,
                unitKey,
                writer,
                serdeValue,
                RowDotRef(row, circularBuffer),
                RowDotRef(row, numDataPoints),
                RowDotRef(row, currentBufferElementPointer),
                Constant(factory.SizeOfBuf));
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenExpressionRef unitKey,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            CodegenExpressionRef state = RefCol("state", col);
            method.Block
                .DeclareVar<AggregationNthState>(
                    state.Ref,
                    StaticMethod(GetType(), "Read", input, unitKey, serdeValue, Constant(factory.SizeOfBuf)))
                .AssignRef(RowDotRef(row, circularBuffer), ExprDotMethod(state, "getCircularBuffer"))
                .AssignRef(
                    RowDotRef(row, currentBufferElementPointer),
                    ExprDotMethod(state, "getCurrentBufferElementPointer"))
                .AssignRef(RowDotRef(row, numDataPoints), ExprDotMethod(state, "getNumDataPoints"));
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            var sizeBuf = Constant(factory.SizeOfBuf);
            method.Block.IfRefNullReturnNull(circularBuffer)
                .DeclareVar<int>("index", Op(Op(currentBufferElementPointer, "+", sizeBuf), "%", sizeBuf))
                .MethodReturn(ArrayAtIndex(circularBuffer, Ref("index")));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="input">input</param>
        /// <param name="unitKey">unit key</param>
        /// <param name="serdeNullable">binding</param>
        /// <param name="sizeBuf">size</param>
        /// <returns>state</returns>
        /// <throws>IOException ioerror</throws>
        public static AggregationNthState Read(
            DataInput input,
            byte[] unitKey,
            DataInputOutputSerdeWCollation<object> serdeNullable,
            int sizeBuf)
        {
            var filled = input.ReadBoolean();
            var state = new AggregationNthState();
            if (!filled) {
                return state;
            }

            var circularBuffer = new object[sizeBuf];
            state.CircularBuffer = circularBuffer;
            state.NumDataPoints = input.ReadLong();
            state.CurrentBufferElementPointer = input.ReadInt();
            for (var i = 0; i < sizeBuf; i++) {
                circularBuffer[i] = serdeNullable.Read(input, unitKey);
            }

            return state;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="output">output</param>
        /// <param name="unitKey">unit key</param>
        /// <param name="writer">writer</param>
        /// <param name="serdeNullable">binding</param>
        /// <param name="circularBuffer">buffer</param>
        /// <param name="numDataPoints">points</param>
        /// <param name="currentBufferElementPointer">pointer</param>
        /// <param name="sizeBuf">size</param>
        /// <throws>IOException io error</throws>
        public static void Write(
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer,
            DataInputOutputSerdeWCollation<object> serdeNullable,
            object[] circularBuffer,
            long numDataPoints,
            int currentBufferElementPointer,
            int sizeBuf)
        {
            output.WriteBoolean(circularBuffer != null);
            if (circularBuffer != null) {
                output.WriteLong(numDataPoints);
                output.WriteInt(currentBufferElementPointer);
                for (var i = 0; i < sizeBuf; i++) {
                    serdeNullable.Write(circularBuffer[i], output, unitKey, writer);
                }
            }
        }

        private Consumer<CodegenBlock> ClearCode()
        {
            return block => {
                block
                    .AssignRef(circularBuffer, NewArrayByLength(typeof(object), Constant(factory.SizeOfBuf)))
                    .AssignRef(numDataPoints, Constant(0))
                    .AssignRef(currentBufferElementPointer, Constant(0));
            };
        }

        protected void ApplyEvalEnterNonNull(
            CodegenExpressionRef valueExpr,
            CodegenMethod method)
        {
            method.Block.Increment(numDataPoints)
                .IfCondition(EqualsNull(circularBuffer))
                .Apply(ClearCode())
                .BlockEnd()
                .AssignArrayElement(circularBuffer, currentBufferElementPointer, valueExpr)
                .AssignRef(
                    currentBufferElementPointer,
                    Op(Op(currentBufferElementPointer, "+", Constant(1)), "%", Constant(factory.SizeOfBuf)));
        }

        protected void ApplyEvalLeaveNonNull(CodegenMethod method)
        {
            method.Block.IfCondition(Relational(Constant(factory.SizeOfBuf), GT, numDataPoints))
                .DeclareVar<int>("diff", Op(Constant(factory.SizeOfBuf), "-", Cast(typeof(int), numDataPoints)))
                .DeclareVar<int>(
                    "index",
                    Op(
                        Op(Op(currentBufferElementPointer, "+", Ref("diff")), "-", Constant(1)),
                        "%",
                        Constant(factory.SizeOfBuf)))
                .AssignArrayElement(circularBuffer, Ref("index"), ConstantNull())
                .BlockEnd()
                .Decrement(numDataPoints);
        }

        public class AggregationNthState
        {
            public object[] CircularBuffer;
            public int CurrentBufferElementPointer;
            public long NumDataPoints;
        }
    }
} // end of namespace