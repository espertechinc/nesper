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
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public class CalendarSetForge : CalendarForge
    {
        internal readonly DateTimeFieldEnum field;
        internal readonly ExprForge valueExpr;

        public CalendarSetForge(
            DateTimeFieldEnum field,
            ExprForge valueExpr)
        {
            this.field = field;
            this.valueExpr = valueExpr;
        }

        public CalendarOp EvalOp => new CalendarSetForgeOp(field, valueExpr.ExprEvaluator);

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression dateTimeEx,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarSetForgeOp.CodegenCalendar(
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
            return CalendarSetForgeOp.CodegenDateTimeOffset(
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
            return CalendarSetForgeOp.CodegenDateTime(
                this,
                dateTime,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }
    }
} // end of namespace