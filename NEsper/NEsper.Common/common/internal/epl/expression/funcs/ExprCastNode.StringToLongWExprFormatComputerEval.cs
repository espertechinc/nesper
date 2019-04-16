///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
        public class StringToLongWExprFormatComputerEval : StringToDateLongWExprFormatEval
        {
            public StringToLongWExprFormatComputerEval(ExprEvaluator dateFormatEval)
                : base(dateFormatEval)
            {
            }

            public override object Compute(
                object input,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return ComputeLong(input, eventsPerStream, newData, exprEvaluatorContext);
            }

            public long? ComputeLong(
                object input,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var format = dateFormatEval.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
                SimpleDateFormat dateFormat = StringToSimpleDateFormatSafe(format);
                return StringToLongWStaticFormatComputer.StringToLongWStaticFormatParseSafe(dateFormat, input);
            }

            public static CodegenExpression Codegen(
                CodegenExpression input,
                ExprForge formatForge,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var method = codegenMethodScope.MakeChild(
                        typeof(long), typeof(StringToLongWExprFormatComputerEval), codegenClassScope)
                    .AddParam(typeof(object), "input");
                CodegenExpression format;
                if (formatForge.ForgeConstantType.IsConstant) {
                    format = FormatFieldExpr(typeof(DateFormat), formatForge, codegenClassScope);
                }
                else {
                    method.Block
                        .DeclareVar(
                            typeof(object), "format",
                            formatForge.EvaluateCodegen(typeof(object), method, exprSymbol, codegenClassScope))
                        .DeclareVar(
                            typeof(SimpleDateFormat), "dateFormat",
                            CodegenExpressionBuilder.StaticMethod(
                                typeof(ExprCastNode), "stringToSimpleDateFormatSafe", CodegenExpressionBuilder.Ref("format")));
                    format = CodegenExpressionBuilder.Ref("dateFormat");
                }

                method.Block.MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(StringToLongWStaticFormatComputer), "stringToLongWStaticFormatParseSafe", format,
                        CodegenExpressionBuilder.Ref("input")));
                return CodegenExpressionBuilder.LocalMethod(method, input);
            }
        }
    }
}