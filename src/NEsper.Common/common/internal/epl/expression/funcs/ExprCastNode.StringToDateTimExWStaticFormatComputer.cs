///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.module;
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
        public class StringToDateTimExWStaticFormatComputer : StringToDateLongWStaticFormat
        {
            private readonly TimeZoneInfo _timeZone;

            public StringToDateTimExWStaticFormatComputer(
                string dateFormatString,
                TimeZoneInfo timeZone)
                : base(
                    dateFormatString)
            {
                this._timeZone = timeZone;
            }

            public override object Compute(
                object input,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return StringToDtxWStaticFormatParse(new SimpleDateFormat(dateFormatString), input, _timeZone);
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            /// <param name="format">format</param>
            /// <param name="input">input</param>
            /// <param name="timeZone">time zone</param>
            /// <returns>dtx</returns>
            public static DateTimeEx StringToDtxWStaticFormatParse(
                DateFormat format,
                object input,
                TimeZoneInfo timeZone)
            {
                try {
                    return DateTimeEx.GetInstance(timeZone, format.Parse(input.ToString()));
                }
                catch (ParseException ex) {
                    throw ExprCastNode.HandleParseException(format, input.ToString(), ex);
                }
            }

            public override CodegenExpression CodegenPremade(
                Type evaluationType,
                CodegenExpression input,
                Type inputType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                CodegenExpression timeZoneField =
                    codegenClassScope.AddOrGetDefaultFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
                return CodegenExpressionBuilder.StaticMethod(
                    typeof(StringToDateTimExWStaticFormatComputer),
                    "StringToDtxWStaticFormatParse",
                    FormatField(dateFormatString, codegenClassScope),
                    input,
                    timeZoneField);
            }
        }
    }
}