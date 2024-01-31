///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCastNode
    {
        public class StringToDateTimeIsoFormatComputer : CasterParserComputerForge,
            CasterParserComputer
        {
            public object Compute(
                object input,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return StringToDateTimeWStaticFormatComputer.StringToDateTimeWStaticFormatParse(
                    input.ToString(),
                    DateTimeFormat.ISO_DATE_TIME);
            }

            public bool IsConstantForConstInput => true;

            public CasterParserComputer EvaluatorComputer => this;

            public CodegenExpression CodegenPremade(
                Type evaluationType,
                CodegenExpression input,
                Type inputType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return CodegenExpressionBuilder.StaticMethod(
                    typeof(StringToDateTimeWStaticFormatComputer),
                    "StringToDateTimeWStaticFormatParse",
                    input,
                    CodegenExpressionBuilder.PublicConstValue(typeof(DateTimeFormat), "ISO_DATE_TIME"));
            }
        }
    }
}