///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCastNode
    {
        public class StringToDateTimeOffsetWStaticFormatComputer : StringWStaticFormatComputer
        {
            public StringToDateTimeOffsetWStaticFormatComputer(string format)
                : base(format)
            {
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            /// <param name="input">string</param>
            /// <param name="format">formatter</param>
            /// <returns>ldt</returns>
            public static DateTimeOffset StringToDateTimeOffsetWStaticFormatParse(
                string input,
                DateFormat format)
            {
                var dateTimeEx = format.Parse(input);
                return dateTimeEx.DateTime;
                // throw HandleParseException(formatter.ToString(), input, e);
            }

            public override object Parse(string input)
            {
                return StringToDateTimeOffsetWStaticFormatParse(input, dateFormat);
                // return StringToDateTimeOffsetWStaticFormatParse(input, DateTimeFormat.ISO_DATE_TIME);
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
                    typeof(StringToDateTimeOffsetWStaticFormatComputer),
                    "StringToDateTimeOffsetWStaticFormatParse",
                    input,
                    CodegenFormatter(codegenClassScope));
            }
        }
    }
}