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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCastNode
    {
        public class StringToLongWStaticFormatComputer : StringToDateLongWStaticFormat
        {
            public StringToLongWStaticFormatComputer(string dateFormatString)
                : base(dateFormatString)
            {
            }

            public override object Compute(
                object input,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return StringToLongWStaticFormatParseSafe(new SimpleDateFormat(dateFormatString), input);
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            /// <param name="format">format</param>
            /// <param name="input">input</param>
            /// <returns>msec</returns>
            public static long StringToLongWStaticFormatParseSafe(
                DateFormat format,
                object input)
            {
                try {
                    return format.Parse(input.ToString()).UtcMillis;
                }
                catch (ParseException e) {
                    throw ExprCastNode.HandleParseException(format, input.ToString(), e);
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
                return CodegenExpressionBuilder.StaticMethod(
                    typeof(StringToLongWStaticFormatComputer),
                    "StringToLongWStaticFormatParseSafe",
                    FormatField(dateFormatString, codegenClassScope),
                    input);
            }
        }
    }
}