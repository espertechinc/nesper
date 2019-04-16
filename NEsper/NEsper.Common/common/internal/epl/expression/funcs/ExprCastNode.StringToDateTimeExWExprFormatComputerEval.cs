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
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCastNode
    {
        public class StringToDateTimeExWExprFormatComputerEval : StringToDateLongWExprFormatEval
        {
            private readonly TimeZoneInfo timeZone;

            public StringToDateTimeExWExprFormatComputerEval(
                ExprEvaluator dateFormatEval,
                TimeZoneInfo timeZone)
                : base(dateFormatEval)
            {
                this.timeZone = timeZone;
            }

            public override object Compute(
                object input,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return ComputeDtx(input, eventsPerStream, newData, exprEvaluatorContext);
            }

            public DateTimeEx ComputeDtx(
                object input,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var format = dateFormatEval.Evaluate(eventsPerStream, newData, exprEvaluatorContext);
                var dateFormat = StringToSimpleDateFormatSafe(format);
                return StringToDateTimExWStaticFormatComputer.StringToDtxWStaticFormatParse(
                    dateFormat, input, timeZone);
            }

            public static CodegenExpression Codegen(
                CodegenExpression input,
                ExprForge dateFormatForge,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope,
                TimeZoneInfo timeZone)
            {
                CodegenExpression timeZoneField =
                    codegenClassScope.AddOrGetFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
                var method = codegenMethodScope.MakeChild(
                        typeof(DateTimeEx), typeof(StringToDateTimeExWExprFormatComputerEval), codegenClassScope)
                    .AddParam(typeof(object), "input");
                CodegenExpression format;
                if (dateFormatForge.ForgeConstantType.IsConstant) {
                    format = FormatFieldExpr(typeof(DateFormat), dateFormatForge, codegenClassScope);
                }
                else {
                    method.Block
                        .DeclareVar(
                            typeof(object), "format",
                            dateFormatForge.EvaluateCodegen(typeof(object), method, exprSymbol, codegenClassScope))
                        .DeclareVar(
                            typeof(SimpleDateFormat), "dateFormat",
                            CodegenExpressionBuilder.StaticMethod(
                                typeof(ExprCastNode), "stringToSimpleDateFormatSafe", CodegenExpressionBuilder.Ref("format")));
                    format = CodegenExpressionBuilder.Ref("dateFormat");
                }

                method.Block.MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(StringToDateTimExWStaticFormatComputer), "stringToCalendarWStaticFormatParse", format,
                        CodegenExpressionBuilder.Ref("input"), timeZoneField));
                return CodegenExpressionBuilder.LocalMethod(method, input);
            }
        }
    }
}