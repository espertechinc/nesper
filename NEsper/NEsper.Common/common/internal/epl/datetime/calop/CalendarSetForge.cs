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
    public class CalendarSetForge : CalendarForge
    {
        internal readonly CalendarFieldEnum fieldName;
        internal readonly ExprForge valueExpr;

        public CalendarSetForge(CalendarFieldEnum fieldName, ExprForge valueExpr)
        {
            this.fieldName = fieldName;
            this.valueExpr = valueExpr;
        }

        public CalendarOp EvalOp => new CalendarSetForgeOp(fieldName, valueExpr.ExprEvaluator);

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression dateTimeEx, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarSetForgeOp.CodegenCalendar(this, dateTimeEx, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression dateTimeOffset, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarSetForgeOp.CodegenDateTimeOffset(this, dateTimeOffset, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression dateTime, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CalendarSetForgeOp.CodegenDateTime(this, dateTime, codegenMethodScope, exprSymbol, codegenClassScope);
        }
    }
} // end of namespace