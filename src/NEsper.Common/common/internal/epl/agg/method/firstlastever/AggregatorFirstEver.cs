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
using com.espertech.esper.common.@internal.serde.compiletime.sharable;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.firstlastever
{
    /// <summary>
    /// Aggregator for the very first value.
    /// </summary>
    public class AggregatorFirstEver : AggregatorMethodWDistinctWFilterWValueBase
    {
        private readonly CodegenExpressionMember _isSet;
        private readonly CodegenExpressionMember _firstValue;
        private readonly CodegenExpressionInstanceField _serde;
        private readonly Type _childType;

        public AggregatorFirstEver(
            AggregationForgeFactory factory,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter,
            Type childType,
            DataInputOutputSerdeForge serde)
            : base(factory, col, rowCtor, membersColumnized, classScope, optionalDistinctValueType, optionalDistinctSerde, hasFilter, optionalFilter)
        {
            _childType = childType.GetBoxedType();
            _isSet = membersColumnized.AddMember(col, typeof(bool), "isSet");
            // NOTE: we had originally set the value of the member to childType which seems correct an
            //   appropriate.  However, the code is not doing proper type checking and cast conversion
            //   elsewhere which makes assignment problematic.  Revisit this problem when we have more
            //   time.
            _firstValue = membersColumnized.AddMember(col, typeof(object), "firstValue");
            _serde = classScope.AddOrGetDefaultFieldSharable(
                new CodegenSharableSerdeClassTyped(
                    CodegenSharableSerdeClassTyped.CodegenSharableSerdeName.VALUE_NULLABLE,
                    childType,
                    serde,
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
                .Expression(WriteNullable(RowDotMember(row, _firstValue), _serde, output, unitKey, writer, classScope));
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
                .AssignRef(RowDotMember(row, _firstValue),
                    Cast(_childType, ReadNullable(_serde, input, unitKey, classScope)));
        }

        private Consumer<CodegenBlock> EnterConsumer(CodegenExpression value)
        {
            return block => block.IfCondition(Not(_isSet))
                .AssignRef(_isSet, ConstantTrue())
                .AssignRef(_firstValue, value);
        }
    }
} // end of namespace