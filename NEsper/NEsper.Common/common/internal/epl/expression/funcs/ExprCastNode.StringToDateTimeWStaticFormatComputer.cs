///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCastNode
    {
        public class StringToDateTimeWStaticFormatComputer : StringWStaticFormatComputer
        {
            public StringToDateTimeWStaticFormatComputer(string format)
                : base(format)
            {
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            /// <param name="input">string</param>
            /// <param name="format">formatter</param>
            /// <returns>lt</returns>
            public static DateTime StringToDateTimeWStaticFormatParse(
                string input,
                DateTimeFormat format)
            {
                //try
                //{
                return format.Parse(input).DateTime.DateTime;
                //catch (DateTimeParseException e)
                //{
                //    throw HandleParseException(format.ToString(), input, e);
                //}
            }

            public override object Parse(string input)
            {
                return StringToDateTimeWStaticFormatParse(input, DateTimeFormat.For(format));
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
                    typeof(StringToDateTimeWStaticFormatComputer),
                    "StringDateTimeWStaticFormatParse",
                    input,
                    CodegenFormatter(codegenClassScope));
            }
        }
    }
}