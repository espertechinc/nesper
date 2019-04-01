///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public class CalendarWithTimeForge : CalendarForge
    {
        internal ExprForge hour;
        internal ExprForge min;
        internal ExprForge msec;
        internal ExprForge sec;

        public CalendarWithTimeForge(ExprForge hour, ExprForge min, ExprForge sec, ExprForge msec)
        {
            this.hour = hour;
            this.min = min;
            this.sec = sec;
            this.msec = msec;
        }

        public CalendarOp EvalOp => new CalendarWithTimeForgeOp(
            hour.ExprEvaluator, min.ExprEvaluator, sec.ExprEvaluator, msec.ExprEvaluator);

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression dateTimeEx, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarWithTimeForgeOp.CodegenCalendar(
                this, dateTimeEx, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression dateTimeOffset, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarWithTimeForgeOp.CodegenDateTimeOffset(this, dateTimeOffset, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression dateTime, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarWithTimeForgeOp.CodegenDateTime(this, dateTime, codegenMethodScope, exprSymbol, codegenClassScope);
        }
    }
} // end of namespace