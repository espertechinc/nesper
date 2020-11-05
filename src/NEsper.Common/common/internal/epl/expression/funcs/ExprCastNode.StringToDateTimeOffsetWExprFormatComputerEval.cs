///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCastNode
    {
        public class StringToDateTimeOffsetWExprFormatComputerEval : StringWExprFormatComputerEval
        {
            public StringToDateTimeOffsetWExprFormatComputerEval(ExprEvaluator dateFormatEval)
                : base(dateFormatEval)
            {
            }

            public override object Compute(
                object input,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var format = dateFormatEval.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
                var formatter = StringToDateTimeFormatterSafe(format);
                return StringToDateTimeOffsetWStaticFormatComputer.StringToDateTimeOffsetWStaticFormatParse(
                    input.ToString(),
                    formatter);
            }

            public static CodegenExpression Codegen(
                CodegenExpression input,
                ExprForge dateFormatForge,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var method = codegenMethodScope.MakeChild(
                        typeof(DateTimeOffset),
                        typeof(StringToDateTimeOffsetWExprFormatComputerEval),
                        codegenClassScope)
                    .AddParam(typeof(string), "input");
                CodegenExpression formatter;
                if (dateFormatForge.ForgeConstantType.IsConstant) {
                    formatter = FormatFieldExpr(typeof(DateFormat), dateFormatForge, codegenClassScope);
                }
                else {
                    method.Block.DeclareVar<DateTimeFormat>(
                        "formatter",
                        CodegenExpressionBuilder.StaticMethod(
                            typeof(ExprCastNode),
                            "StringToDateTimeFormatterSafe",
                            dateFormatForge.EvaluateCodegen(typeof(object), method, exprSymbol, codegenClassScope)));
                    formatter = CodegenExpressionBuilder.Ref("formatter");
                }

                method.Block.MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(StringToDateTimeOffsetWStaticFormatComputer),
                        "StringToDateTimeOffsetWStaticFormatParse",
                        CodegenExpressionBuilder.Ref("input"),
                        formatter));
                return CodegenExpressionBuilder.LocalMethod(method, input);
            }
        }
    }
}