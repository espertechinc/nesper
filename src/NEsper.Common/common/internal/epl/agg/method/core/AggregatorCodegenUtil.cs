///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
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
        public static CodegenExpression RowDotMember(
            CodegenExpressionRef row,
            CodegenExpressionMember member)
        {
            return Ref(row.Ref + "." + member.Ref);
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
            CodegenExpressionMember member)
        {
            return block => block.ExprDotMethod(output, "WriteBoolean", RowDotMember(row, member));
        }

        public static Consumer<CodegenBlock> ReadBoolean(
            CodegenExpressionRef row,
            CodegenExpressionMember member,
            CodegenExpression input)
        {
            return block => block.AssignRef(RowDotMember(row, member), ExprDotMethod(input, "ReadBoolean"));
        }

        public static Consumer<CodegenBlock> WriteLong(
            CodegenExpressionRef output,
            CodegenExpressionRef row,
            CodegenExpressionMember member)
        {
            return block => block.ExprDotMethod(output, "WriteLong", RowDotMember(row, member));
        }

        public static Consumer<CodegenBlock> ReadLong(
            CodegenExpressionRef row,
            CodegenExpressionMember member,
            CodegenExpression input)
        {
            return block => block.AssignRef(RowDotMember(row, member), ExprDotMethod(input, "ReadLong"));
        }

        public static Consumer<CodegenBlock> WriteDecimal(
            CodegenExpressionRef output,
            CodegenExpressionRef row,
            CodegenExpressionMember member)
        {
            return block => block.ExprDotMethod(output, "WriteDecimal", RowDotMember(row, member));
        }

        public static Consumer<CodegenBlock> WriteDouble(
            CodegenExpressionRef output,
            CodegenExpressionRef row,
            CodegenExpressionMember member)
        {
            return block => block.ExprDotMethod(output, "WriteDouble", RowDotMember(row, member));
        }

        public static Consumer<CodegenBlock> ReadDecimal(
            CodegenExpressionRef row,
            CodegenExpressionMember member,
            CodegenExpression input)
        {
            return block => block.AssignRef(RowDotMember(row, member), ExprDotMethod(input, "ReadDecimal"));
        }

        public static Consumer<CodegenBlock> ReadDouble(
            CodegenExpressionRef row,
            CodegenExpressionMember member,
            CodegenExpression input)
        {
            return block => block.AssignRef(RowDotMember(row, member), ExprDotMethod(input, "ReadDouble"));
        }

        public static Consumer<CodegenBlock> WriteInt(
            CodegenExpressionRef output,
            CodegenExpressionRef row,
            CodegenExpressionMember member)
        {
            return block => block.ExprDotMethod(output, "WriteInt", RowDotMember(row, member));
        }

        public static Consumer<CodegenBlock> ReadInt(
            CodegenExpressionRef row,
            CodegenExpressionMember member,
            CodegenExpression input)
        {
            return block => block.AssignRef(RowDotMember(row, member), ExprDotMethod(input, "ReadInt"));
        }

        public static Consumer<CodegenBlock> WriteFloat(
            CodegenExpressionRef output,
            CodegenExpressionRef row,
            CodegenExpressionMember member)
        {
            return block => block.ExprDotMethod(output, "WriteFloat", RowDotMember(row, member));
        }

        public static Consumer<CodegenBlock> ReadFloat(
            CodegenExpressionRef row,
            CodegenExpressionMember member,
            CodegenExpression input)
        {
            return block => block.AssignRef(RowDotMember(row, member), ExprDotMethod(input, "ReadFloat"));
        }
    }
} // end of namespace