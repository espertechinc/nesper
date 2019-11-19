///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.method.core
{
    public class AggregatorCodegenUtil
    {
        public static CodegenExpression RowDotRef(
            CodegenExpressionRef row,
            CodegenExpressionRef @ref)
        {
            return Ref(row.Ref + "." + @ref.Ref);
        }

        public static CodegenExpression WriteNullable(
            CodegenExpression value,
            CodegenExpressionInstanceField serde,
            CodegenExpressionRef output,
            CodegenExpressionRef unitKey,
            CodegenExpressionRef writer,
            CodegenClassScope classScope)
        {
            return ExprDotMethod(serde, "Write", value, output, unitKey, writer);
        }

        public static CodegenExpression ReadNullable(
            CodegenExpressionInstanceField serde,
            CodegenExpressionRef input,
            CodegenExpressionRef unitKey,
            CodegenClassScope classScope)
        {
            return ExprDotMethod(serde, "Read", input, unitKey);
        }

        public static void PrefixWithFilterCheck(
            ExprForge filterForge,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var filterType = filterForge.EvaluationType;
            method.Block.DeclareVar(
                filterType.GetBoxedType(),
                "pass",
                filterForge.EvaluateCodegen(filterType, method, symbols, classScope));
            if (filterType.CanBeNull()) {
                method.Block.IfRefNull("pass").BlockReturnNoValue();
            }

            method.Block.IfCondition(Not(Unbox(Ref("pass")))).BlockReturnNoValue();
        }

        public static Consumer<CodegenBlock> WriteBoolean(
            CodegenExpressionRef output,
            CodegenExpressionRef row,
            CodegenExpressionRef @ref)
        {
            return block => block.ExprDotMethod(output, "WriteBoolean", RowDotRef(row, @ref));
        }

        public static Consumer<CodegenBlock> ReadBoolean(
            CodegenExpressionRef row,
            CodegenExpressionRef @ref,
            CodegenExpression input)
        {
            return block => block.AssignRef(RowDotRef(row, @ref), ExprDotMethod(input, "ReadBoolean"));
        }

        public static Consumer<CodegenBlock> WriteLong(
            CodegenExpressionRef output,
            CodegenExpressionRef row,
            CodegenExpressionRef @ref)
        {
            return block => block.ExprDotMethod(output, "WriteLong", RowDotRef(row, @ref));
        }

        public static Consumer<CodegenBlock> ReadLong(
            CodegenExpressionRef row,
            CodegenExpressionRef @ref,
            CodegenExpression input)
        {
            return block => block.AssignRef(RowDotRef(row, @ref), ExprDotMethod(input, "ReadLong"));
        }

        public static Consumer<CodegenBlock> WriteDecimal(
            CodegenExpressionRef output,
            CodegenExpressionRef row,
            CodegenExpressionRef @ref)
        {
            return block => block.ExprDotMethod(output, "WriteDecimal", RowDotRef(row, @ref));
        }

        public static Consumer<CodegenBlock> WriteDouble(
            CodegenExpressionRef output,
            CodegenExpressionRef row,
            CodegenExpressionRef @ref)
        {
            return block => block.ExprDotMethod(output, "WriteDouble", RowDotRef(row, @ref));
        }

        public static Consumer<CodegenBlock> ReadDecimal(
            CodegenExpressionRef row,
            CodegenExpressionRef @ref,
            CodegenExpression input)
        {
            return block => block.AssignRef(RowDotRef(row, @ref), ExprDotMethod(input, "ReadDecimal"));
        }

        public static Consumer<CodegenBlock> ReadDouble(
            CodegenExpressionRef row,
            CodegenExpressionRef @ref,
            CodegenExpression input)
        {
            return block => block.AssignRef(RowDotRef(row, @ref), ExprDotMethod(input, "ReadDouble"));
        }

        public static Consumer<CodegenBlock> WriteInt(
            CodegenExpressionRef output,
            CodegenExpressionRef row,
            CodegenExpressionRef @ref)
        {
            return block => block.ExprDotMethod(output, "WriteInt", RowDotRef(row, @ref));
        }

        public static Consumer<CodegenBlock> ReadInt(
            CodegenExpressionRef row,
            CodegenExpressionRef @ref,
            CodegenExpression input)
        {
            return block => block.AssignRef(RowDotRef(row, @ref), ExprDotMethod(input, "ReadInt"));
        }

        public static Consumer<CodegenBlock> WriteFloat(
            CodegenExpressionRef output,
            CodegenExpressionRef row,
            CodegenExpressionRef @ref)
        {
            return block => block.ExprDotMethod(output, "WriteFloat", RowDotRef(row, @ref));
        }

        public static Consumer<CodegenBlock> ReadFloat(
            CodegenExpressionRef row,
            CodegenExpressionRef @ref,
            CodegenExpression input)
        {
            return block => block.AssignRef(RowDotRef(row, @ref), ExprDotMethod(input, "ReadFloat"));
        }
    }
} // end of namespace