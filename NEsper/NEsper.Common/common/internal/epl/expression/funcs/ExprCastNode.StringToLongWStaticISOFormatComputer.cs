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
using com.espertech.esper.common.@internal.epl.pattern.observer;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCastNode
    {
        public class StringToLongWStaticISOFormatComputer : CasterParserComputerForge,
            CasterParserComputer
        {
            public object Compute(
                object input,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return StringToLongWStaticISOParse(input.ToString());
            }

            public bool IsConstantForConstInput => true;

            public CodegenExpression CodegenPremade(
                Type evaluationType,
                CodegenExpression input,
                Type inputType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return CodegenExpressionBuilder.StaticMethod(
                    typeof(StringToLongWStaticISOFormatComputer),
                    "StringToLongWStaticISOParse",
                    input);
            }

            public CasterParserComputer EvaluatorComputer => this;

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            /// <param name="input">input</param>
            /// <returns>msec</returns>
            public static long StringToLongWStaticISOParse(string input)
            {
                try {
                    return TimerScheduleISO8601Parser.ParseDate(input).TimeInMillis;
                }
                catch (ScheduleParameterException ex) {
                    throw HandleParseISOException(input, ex);
                }
            }
        }
    }
}