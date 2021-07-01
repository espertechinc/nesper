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

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCastNode
    {
        public abstract class StringToDateLongWStaticFormat : CasterParserComputerForge,
            CasterParserComputer
        {
            internal readonly string dateFormatString;

            public StringToDateLongWStaticFormat(string dateFormatString)
            {
                this.dateFormatString = dateFormatString;
            }

            public abstract object Compute(
                object input,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext exprEvaluatorContext);

            public CasterParserComputer EvaluatorComputer => this;

            public bool IsConstantForConstInput => true;

            public abstract CodegenExpression CodegenPremade(
                Type evaluationType,
                CodegenExpression input,
                Type inputType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope);

            internal event Action<string, string, Exception> HandleParseException;

            /// <summary>
            ///     Called when a format exception occurs.
            /// </summary>
            /// <param name="dateTimeFormat">The date time format.</param>
            /// <param name="input">The input.</param>
            /// <param name="formatException">The format exception.</param>
            protected void OnHandleParseException(
                string dateTimeFormat,
                string input,
                FormatException formatException)
            {
                HandleParseException?.Invoke(dateTimeFormat, input, formatException);
            }
        }
    }
}