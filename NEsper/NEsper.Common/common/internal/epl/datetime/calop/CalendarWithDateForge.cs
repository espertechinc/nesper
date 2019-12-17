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
        private readonly ExprForge _day;
        private readonly ExprForge _month;
        private readonly ExprForge _year;

        public CalendarWithDateForge(
            ExprForge year,
            ExprForge month,
            ExprForge day)
        {
            _year = year;
            _month = month;
            _day = day;
        }

        public ExprForge Day => _day;

        public ExprForge Month => _month;

        public ExprForge Year => _year;

        public CalendarOp EvalOp => new CalendarWithDateForgeOp(
            _year.ExprEvaluator,
            _month.ExprEvaluator,
            _day.ExprEvaluator);

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression dateTimeEx,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarWithDateForgeOp.CodegenDateTimeEx(
                this,
                dateTimeEx,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression dateTimeOffset,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarWithDateForgeOp.CodegenDateTimeOffset(
                this,
                dateTimeOffset,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression dateTime,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarWithDateForgeOp.CodegenDateTime(
                this,
                dateTime,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }
    }
} // end of namespace