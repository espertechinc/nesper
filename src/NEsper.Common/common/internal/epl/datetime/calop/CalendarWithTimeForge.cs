///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private readonly ExprForge _hour;
        private readonly ExprForge _min;
        private readonly ExprForge _msec;
        private readonly ExprForge _sec;

        public CalendarWithTimeForge(
            ExprForge hour,
            ExprForge min,
            ExprForge sec,
            ExprForge msec)
        {
            _hour = hour;
            _min = min;
            _sec = sec;
            _msec = msec;
        }

        public ExprForge Hour => _hour;

        public ExprForge Min => _min;

        public ExprForge Msec => _msec;

        public ExprForge Sec => _sec;

        public CalendarOp EvalOp => new CalendarWithTimeForgeOp(
            _hour.ExprEvaluator,
            _min.ExprEvaluator,
            _sec.ExprEvaluator,
            _msec.ExprEvaluator);

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression dateTimeEx,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarWithTimeForgeOp.CodegenDateTimeEx(
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
            return CalendarWithTimeForgeOp.CodegenDateTimeOffset(
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
            return CalendarWithTimeForgeOp.CodegenDateTime(
                this,
                dateTime,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }
    }
} // end of namespace