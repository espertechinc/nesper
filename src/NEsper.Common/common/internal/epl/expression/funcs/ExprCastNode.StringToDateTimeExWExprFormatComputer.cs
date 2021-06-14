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
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCastNode
    {
        public class StringToDateTimeExWExprFormatComputer : StringToDateLongWExprFormatForge
        {
            private readonly TimeZoneInfo timeZone;

            public StringToDateTimeExWExprFormatComputer(
                ExprForge dateFormatForge,
                TimeZoneInfo timeZone)
                : base(dateFormatForge)
            {
                this.timeZone = timeZone;
            }

            public override CasterParserComputer EvaluatorComputer =>
                new StringToDateTimeExWExprFormatComputerEval(dateFormatForge.ExprEvaluator, timeZone);

            public override CodegenExpression CodegenPremade(
                Type evaluationType,
                CodegenExpression input,
                Type inputType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return StringToDateTimeExWExprFormatComputerEval.Codegen(
                    input,
                    dateFormatForge,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope,
                    timeZone);
            }
        }
    }
}