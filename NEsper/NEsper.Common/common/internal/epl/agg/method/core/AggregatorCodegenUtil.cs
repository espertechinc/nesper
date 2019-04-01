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
using com.espertech.esper.compat.function;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.method.core
{
    public class AggregatorCodegenUtil
    {
        public static CodegenExpression RowDotRef(CodegenExpressionRef row, CodegenExpressionRef @ref)
        {
            return Ref(row.Ref + "." + @ref.Ref);
        }

        public static CodegenExpression WriteNullable(
            CodegenExpression value, CodegenExpressionField serde, CodegenExpressionRef output,
            CodegenExpressionRef unitKey, CodegenExpressionRef writer, CodegenClassScope classScope)
        {
            return ExprDotMethod(serde, "write", value, output, unitKey, writer);
        }

        public static CodegenExpression ReadNullable(
            CodegenExpressionField serde, CodegenExpressionRef input, CodegenExpressionRef unitKey,
            CodegenClassScope classScope)
        {
            return ExprDotMethod(serde, "read", input, unitKey);
        }

        public static void PrefixWithFilterCheck(
            ExprForge filterForge, CodegenMethod method, ExprForgeCodegenSymbol symbols, CodegenClassScope classScope)
        {
            var filterType = filterForge.EvaluationType;
            method.Block.DeclareVar(
                filterType, "pass", filterForge.EvaluateCodegen(filterType, method, symbols, classScope));
            if (!filterType.IsPrimitive) {
                method.Block.IfRefNull("pass").BlockReturnNoValue();
            }

            method.Block.IfCondition(Not(Ref("pass"))).BlockReturnNoValue();
        }

        public static Consumer<CodegenBlock> WriteBoolean(
            CodegenExpressionRef output, CodegenExpressionRef row, CodegenExpressionRef @ref)
        {
            return block => block.ExprDotMethod(output, "writeBoolean", RowDotRef(row, @ref));
        }

        public static Consumer<CodegenBlock> ReadBoolean(
            CodegenExpressionRef row, CodegenExpressionRef @ref, CodegenExpression input)
        {
            return block => block.AssignRef(RowDotRef(row, @ref), ExprDotMethod(input, "readBoolean"));
        }

        public static Consumer<CodegenBlock> WriteLong(
            CodegenExpressionRef output, CodegenExpressionRef row, CodegenExpressionRef @ref)
        {
            return block => block.ExprDotMethod(output, "writeLong", RowDotRef(row, @ref));
        }

        public static Consumer<CodegenBlock> ReadLong(
            CodegenExpressionRef row, CodegenExpressionRef @ref, CodegenExpression input)
        {
            return block => block.AssignRef(RowDotRef(row, @ref), ExprDotMethod(input, "readLong"));
        }

        public static Consumer<CodegenBlock> WriteDouble(
            CodegenExpressionRef output, CodegenExpressionRef row, CodegenExpressionRef @ref)
        {
            return block => block.ExprDotMethod(output, "writeDouble", RowDotRef(row, @ref));
        }

        public static Consumer<CodegenBlock> ReadDouble(
            CodegenExpressionRef row, CodegenExpressionRef @ref, CodegenExpression input)
        {
            return block => block.AssignRef(RowDotRef(row, @ref), ExprDotMethod(input, "readDouble"));
        }

        public static Consumer<CodegenBlock> WriteInt(
            CodegenExpressionRef output, CodegenExpressionRef row, CodegenExpressionRef @ref)
        {
            return block => block.ExprDotMethod(output, "writeInt", RowDotRef(row, @ref));
        }

        public static Consumer<CodegenBlock> ReadInt(
            CodegenExpressionRef row, CodegenExpressionRef @ref, CodegenExpression input)
        {
            return block => block.AssignRef(RowDotRef(row, @ref), ExprDotMethod(input, "readInt"));
        }

        public static Consumer<CodegenBlock> WriteFloat(
            CodegenExpressionRef output, CodegenExpressionRef row, CodegenExpressionRef @ref)
        {
            return block => block.ExprDotMethod(output, "writeFloat", RowDotRef(row, @ref));
        }

        public static Consumer<CodegenBlock> ReadFloat(
            CodegenExpressionRef row, CodegenExpressionRef @ref, CodegenExpression input)
        {
            return block => block.AssignRef(RowDotRef(row, @ref), ExprDotMethod(input, "readFloat"));
        }
    }
} // end of namespace