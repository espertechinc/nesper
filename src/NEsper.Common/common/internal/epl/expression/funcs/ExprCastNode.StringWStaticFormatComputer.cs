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
        public abstract class StringWStaticFormatComputer : CasterParserComputerForge,
            CasterParserComputer
        {
            internal readonly string format;
            internal readonly DateFormat dateFormat;

            public StringWStaticFormatComputer(string format)
            {
                this.format = format;
                dateFormat = DateTimeFormat.For(format);
            }

            public object Compute(
                object input,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return Parse(input.ToString());
            }

            public bool IsConstantForConstInput => true;

            public CasterParserComputer EvaluatorComputer => this;

            public abstract CodegenExpression CodegenPremade(
                Type evaluationType,
                CodegenExpression input,
                Type inputType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope);

            public abstract object Parse(string input);

            internal CodegenExpression CodegenFormatter(CodegenClassScope codegenClassScope)
            {
                return codegenClassScope.AddDefaultFieldUnshared(
                    true,
                    typeof(DateTimeFormat),
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(DateTimeFormat),
                        "For",
                        CodegenExpressionBuilder.Constant(format)));
            }
        }
    }
}