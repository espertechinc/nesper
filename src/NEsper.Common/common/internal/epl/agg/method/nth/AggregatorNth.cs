///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.compiletime.sharable;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.serde.compiletime.sharable.CodegenSharableSerdeClassTyped.
    CodegenSharableSerdeName;

namespace com.espertech.esper.common.@internal.epl.agg.method.nth
{
    public class AggregatorNth : AggregatorMethodWDistinctWFilterWValueBase
    {
        private readonly AggregationForgeFactoryNth factory;
        private CodegenExpressionMember circularBuffer;
        private CodegenExpressionMember currentBufferElementPointer;
        private CodegenExpressionMember numDataPoints;
        private CodegenExpressionInstanceField serdeValue;

        public AggregatorNth(
            AggregationForgeFactoryNth factory,
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter) : base(optionalDistinctValueType, optionalDistinctSerde, hasFilter, optionalFilter)
        {
            this.factory = factory;
        }

        public override void InitForgeFiltered(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            circularBuffer = membersColumnized.AddMember(col, typeof(object[]), "buf");
            currentBufferElementPointer = membersColumnized.AddMember(col, typeof(int), "cbep");
            numDataPoints = membersColumnized.AddMember(col, typeof(long), "cnt");
            serdeValue = classScope.AddOrGetDefaultFieldSharable(
                new CodegenSharableSerdeClassTyped(VALUE_NULLABLE, factory.ChildType, factory.Serde, classScope));
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
                typeof(AggregatorNthSerde),
                "write",
                output,
                unitKey,
                writer,
                serdeValue,
                RowDotMember(row, circularBuffer),
                RowDotMember(row, numDataPoints),
                RowDotMember(row, currentBufferElementPointer),
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
            CodegenExpressionMember state = MemberCol("state", col);
            method.Block
                .DeclareVar<AggregationNthState>(
                    state.Ref,
                    StaticMethod(
                        typeof(AggregatorNthSerde),
                        "read",
                        input,
                        unitKey,
                        serdeValue,
                        Constant(factory.SizeOfBuf)))
                .AssignRef(RowDotMember(row, circularBuffer), ExprDotMethod(state, "getCircularBuffer"))
                .AssignRef(
                    RowDotMember(row, currentBufferElementPointer),
                    ExprDotMethod(state, "getCurrentBufferElementPointer"))
                .AssignRef(RowDotMember(row, numDataPoints), ExprDotMethod(state, "getNumDataPoints"));
        }

        protected override void AppendFormatWODistinct(FabricTypeCollector collector)
        {
            collector.AggregatorNth(AggregatorNthSerde.SERDE_VERSION, factory.SizeOfBuf, factory.Serde);
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            var sizeBuf = Constant(factory.SizeOfBuf);
            method.Block.IfNullReturnNull(circularBuffer)
                .DeclareVar<int>("index", Op(Op(currentBufferElementPointer, "+", sizeBuf), "%", sizeBuf))
                .MethodReturn(ArrayAtIndex(circularBuffer, Ref("index")));
        }

        private Consumer<CodegenBlock> ClearCode()
        {
            return block => {
                block.AssignRef(circularBuffer, NewArrayByLength(typeof(object), Constant(factory.SizeOfBuf)))
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
    }
} // end of namespace