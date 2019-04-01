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
    public class CalendarWithDateForge : CalendarForge
    {
        internal ExprForge day;
        internal ExprForge month;

        internal ExprForge year;

        public CalendarWithDateForge(ExprForge year, ExprForge month, ExprForge day)
        {
            this.year = year;
            this.month = month;
            this.day = day;
        }

        public CalendarOp EvalOp => new CalendarWithDateForgeOp(
            year.ExprEvaluator, month.ExprEvaluator, day.ExprEvaluator);

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression dateTimeEx, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarWithDateForgeOp.CodegenCalendar(
                this, dateTimeEx, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression dateTimeOffset, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarWithDateForgeOp.CodegenDateTimeOffset(this, dateTimeOffset, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression dateTime, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarWithDateForgeOp.CodegenDateTime(this, dateTime, codegenMethodScope, exprSymbol, codegenClassScope);
        }
    }
} // end of namespace