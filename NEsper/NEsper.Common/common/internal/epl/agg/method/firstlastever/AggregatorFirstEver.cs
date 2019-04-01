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
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.serde.CodegenSharableSerdeClassTyped.CodegenSharableSerdeName;

namespace com.espertech.esper.common.@internal.epl.agg.method.firstlastever
{
    /// <summary>
    /// Aggregator for the very first value.
    /// </summary>
    public class AggregatorFirstEver : AggregatorMethodWDistinctWFilterWValueBase
    {
        private readonly CodegenExpressionRef isSet;
        private readonly CodegenExpressionRef firstValue;
        private readonly CodegenExpressionField serde;

        public AggregatorFirstEver(
            AggregationForgeFactory factory, int col, CodegenCtor rowCtor, CodegenMemberCol membersColumnized,
            CodegenClassScope classScope, Type optionalDistinctValueType, bool hasFilter, ExprNode optionalFilter,
            Type childType)
            : base(
                factory, col, rowCtor, membersColumnized, classScope, optionalDistinctValueType, hasFilter,
                optionalFilter)
        {
            isSet = membersColumnized.AddMember(col, typeof(bool), "isSet");
            firstValue = membersColumnized.AddMember(col, typeof(object), "firstValue");
            serde = classScope.AddOrGetFieldSharable(new CodegenSharableSerdeClassTyped(VALUE_NULLABLE, childType));
        }

        protected override void ApplyEvalEnterNonNull(
            CodegenExpressionRef value, Type valueType, CodegenMethod method, ExprForgeCodegenSymbol symbols,
            ExprForge[] forges, CodegenClassScope classScope)
        {
            method.Block.Apply(EnterConsumer(forges[0].EvaluateCodegen(typeof(object), method, symbols, classScope)));
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.Apply(EnterConsumer(value));
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value, Type valueType, CodegenMethod method, ExprForgeCodegenSymbol symbols,
            ExprForge[] forges, CodegenClassScope classScope)
        {
            // no op
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            // no op
        }

        protected override void ClearWODistinct(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.AssignRef(firstValue, ConstantNull())
                .AssignRef(isSet, ConstantFalse());
        }

        public override void GetValueCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.MethodReturn(firstValue);
        }

        protected override void WriteWODistinct(
            CodegenExpressionRef row, int col, CodegenExpressionRef output, CodegenExpressionRef unitKey,
            CodegenExpressionRef writer, CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.Apply(WriteBoolean(output, row, isSet))
                .Expression(WriteNullable(RowDotRef(row, firstValue), serde, output, unitKey, writer, classScope));
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row, int col, CodegenExpressionRef input, CodegenExpressionRef unitKey,
            CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.Apply(ReadBoolean(row, isSet, input))
                .AssignRef(RowDotRef(row, firstValue), ReadNullable(serde, input, unitKey, classScope));
        }

        private Consumer<CodegenBlock> EnterConsumer(CodegenExpression value)
        {
            return block => block.IfCondition(Not(isSet))
                .AssignRef(isSet, ConstantTrue())
                .AssignRef(firstValue, value);
        }
    }
} // end of namespace