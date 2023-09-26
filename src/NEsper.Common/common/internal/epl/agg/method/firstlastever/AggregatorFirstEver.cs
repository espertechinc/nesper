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
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.serde.compiletime.sharable.CodegenSharableSerdeClassTyped.
    CodegenSharableSerdeName;

namespace com.espertech.esper.common.@internal.epl.agg.method.firstlastever
{
    /// <summary>
    /// Aggregator for the very first value.
    /// </summary>
    public class AggregatorFirstEver : AggregatorMethodWDistinctWFilterWValueBase
    {
        private CodegenExpressionMember _isSet;
        private CodegenExpressionMember _firstValue;
        private CodegenExpressionInstanceField _serdeField;

        private readonly Type _childType;
        private readonly DataInputOutputSerdeForge _serde;

        public AggregatorFirstEver(
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter,
            Type childType,
            DataInputOutputSerdeForge serde) : base(
            optionalDistinctValueType,
            optionalDistinctSerde,
            hasFilter,
            optionalFilter)
        {
            _childType = childType.GetBoxedType();
            _serde = serde;
        }

        public override void InitForgeFiltered(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            _isSet = membersColumnized.AddMember(col, typeof(bool), "isSet");
            _firstValue = membersColumnized.AddMember(col, typeof(object), "firstValue");
            _serdeField = classScope.AddOrGetDefaultFieldSharable(
                new CodegenSharableSerdeClassTyped(
                    VALUE_NULLABLE,
                    _childType,
                    _serde,
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
            method.Block.Apply(EnterConsumer(forges[0].EvaluateCodegen(
                _childType, method, symbols, classScope)));
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Apply(EnterConsumer(value));
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            // no op
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            // no op
        }

        protected override void ClearWODistinct(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(_firstValue, ConstantNull())
                .AssignRef(_isSet, ConstantFalse());
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(_firstValue);
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
                .Apply(WriteBoolean(output, row, _isSet))
                .Expression(WriteNullable(RowDotMember(row, _firstValue), _serdeField, output, unitKey, writer, classScope));
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
                .Apply(ReadBoolean(row, _isSet, input))
                .AssignRef(RowDotMember(row, _firstValue), ReadNullable(_serdeField, input, unitKey, classScope));
        }

        protected override void AppendFormatWODistinct(FabricTypeCollector collector)
        {
            collector.Builtin(typeof(bool));
            collector.Serde(_serde);
        }

        private Consumer<CodegenBlock> EnterConsumer(CodegenExpression value)
        {
            return block => block.IfCondition(Not(_isSet))
                .AssignRef(_isSet, ConstantTrue())
                .AssignRef(_firstValue, value);
        }
    }
} // end of namespace