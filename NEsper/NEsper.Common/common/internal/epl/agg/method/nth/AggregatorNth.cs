///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.compiletime.sharable;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.io;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.nth
{
    public class AggregatorNth : AggregatorMethodWDistinctWFilterWValueBase
    {
        private readonly AggregationForgeFactoryNth _factory;
        private readonly CodegenExpressionMember _circularBuffer;
        private readonly CodegenExpressionMember _currentBufferElementPointer;
        private readonly CodegenExpressionMember _numDataPoints;
        private readonly CodegenExpressionInstanceField _serdeValue;

        public AggregatorNth(
            AggregationForgeFactoryNth factory,
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
            this._factory = factory;
            _circularBuffer = membersColumnized.AddMember(col, typeof(object[]), "buf");
            _currentBufferElementPointer = membersColumnized.AddMember(col, typeof(int), "cbep");
            _numDataPoints = membersColumnized.AddMember(col, typeof(long), "cnt");
            _serdeValue = classScope.AddOrGetDefaultFieldSharable(
                new CodegenSharableSerdeClassTyped(
                    CodegenSharableSerdeClassTyped.CodegenSharableSerdeName.VALUE_NULLABLE,
                    factory.ChildType,
                    factory.Serde,
                    classScope));
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
                _serdeValue,
                RowDotMember(row, _circularBuffer),
                RowDotMember(row, _numDataPoints),
                RowDotMember(row, _currentBufferElementPointer),
                Constant(_factory.SizeOfBuf));
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenExpressionRef unitKey,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            var state = MemberCol("state", col);
            method.Block
                .DeclareVar<AggregationNthState>(
                    state.Ref,
                    StaticMethod(GetType(), "Read", input, unitKey, _serdeValue, Constant(_factory.SizeOfBuf)))
                .AssignRef(RowDotMember(row, _circularBuffer), ExprDotName(state, "CircularBuffer"))
                .AssignRef(
                    RowDotMember(row, _currentBufferElementPointer),
                    ExprDotName(state, "CurrentBufferElementPointer"))
                .AssignRef(RowDotMember(row, _numDataPoints), ExprDotName(state, "NumDataPoints"));
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            var sizeBuf = Constant(_factory.SizeOfBuf);
            method.Block.IfNullReturnNull(_circularBuffer)
                .DeclareVar<int>("index", Op(Op(_currentBufferElementPointer, "+", sizeBuf), "%", sizeBuf))
                .MethodReturn(ArrayAtIndex(_circularBuffer, Ref("index")));
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
            DataInputOutputSerde serdeNullable,
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
                circularBuffer[i] = serdeNullable.ReadAny(input, unitKey);
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
            DataInputOutputSerde serdeNullable,
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
                    .AssignRef(_circularBuffer, NewArrayByLength(typeof(object), Constant(_factory.SizeOfBuf)))
                    .AssignRef(_numDataPoints, Constant(0))
                    .AssignRef(_currentBufferElementPointer, Constant(0));
            };
        }

        protected void ApplyEvalEnterNonNull(
            CodegenExpressionRef valueExpr,
            CodegenMethod method)
        {
            method.Block.Increment(_numDataPoints)
                .IfCondition(EqualsNull(_circularBuffer))
                .Apply(ClearCode())
                .BlockEnd()
                .AssignArrayElement(_circularBuffer, _currentBufferElementPointer, valueExpr)
                .AssignRef(
                    _currentBufferElementPointer,
                    Op(Op(_currentBufferElementPointer, "+", Constant(1)), "%", Constant(_factory.SizeOfBuf)));
        }

        protected void ApplyEvalLeaveNonNull(CodegenMethod method)
        {
            method.Block.IfCondition(Relational(Constant(_factory.SizeOfBuf), GT, _numDataPoints))
                .DeclareVar<int>("diff", Op(Constant(_factory.SizeOfBuf), "-", Cast(typeof(int), _numDataPoints)))
                .DeclareVar<int>(
                    "index",
                    Op(
                        Op(Op(_currentBufferElementPointer, "+", Ref("diff")), "-", Constant(1)),
                        "%",
                        Constant(_factory.SizeOfBuf)))
                .AssignArrayElement(_circularBuffer, Ref("index"), ConstantNull())
                .BlockEnd()
                .Decrement(_numDataPoints);
        }

        public class AggregationNthState
        {
            public object[] CircularBuffer;
            public int CurrentBufferElementPointer;
            public long NumDataPoints;
        }
    }
} // end of namespace