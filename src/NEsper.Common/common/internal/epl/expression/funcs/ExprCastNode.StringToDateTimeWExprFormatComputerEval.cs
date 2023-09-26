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

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCastNode
    {
        public class StringToDateTimeWExprFormatComputerEval : StringWExprFormatComputerEval
        {
            public StringToDateTimeWExprFormatComputerEval(ExprEvaluator dateFormatEval)
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
                return StringToDateTimeWStaticFormatComputer.StringToDateTimeWStaticFormatParse(
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
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(DateTime),
                        typeof(StringToDateTimeWExprFormatComputerEval),
                        codegenClassScope)
                    .AddParam<string>("input");
                CodegenExpression format;
                if (dateFormatForge.ForgeConstantType.IsConstant) {
                    format = FormatFieldExpr(typeof(DateFormat), dateFormatForge, codegenClassScope);
                }
                else {
                    methodNode.Block
                        .DeclareVar<DateFormat>(
                            "formatter",
                            CodegenExpressionBuilder.StaticMethod(
                                typeof(ExprCastNode),
                                "StringToDateTimeFormatterSafe",
                                dateFormatForge.EvaluateCodegen(
                                    typeof(object),
                                    methodNode,
                                    exprSymbol,
                                    codegenClassScope)));
                    format = CodegenExpressionBuilder.Ref("formatter");
                }

                methodNode.Block.MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(StringToDateTimeWStaticFormatComputer),
                        "StringToDateTimeWStaticFormatParse",
                        CodegenExpressionBuilder.Ref("input"),
                        format));
                return CodegenExpressionBuilder.LocalMethod(methodNode, input);
            }
        }
    }
}