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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public class CalendarWithMinForge : CalendarForge, CalendarOp
    {
        private readonly CalendarFieldEnum fieldName;

        public CalendarWithMinForge(CalendarFieldEnum fieldName)
        {
            this.fieldName = fieldName;
        }

        public CalendarOp EvalOp
        {
            get => this;
        }

        public void Evaluate(DateTimeEx dateTimeEx, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            dateTimeEx.Set(fieldName.CalendarField, dateTimeEx.GetActualMinimum(fieldName.CalendarField));
        }

        public CodegenExpression CodegenDateTimeEx(CodegenExpression dateTimeEx, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope)
        {
            CodegenExpression field = Constant(fieldName.CalendarField);
            return ExprDotMethod(dateTimeEx, "set", field, ExprDotMethod(dateTimeEx, "getActualMinimum", field));
        }

        public DateTimeOffset Evaluate(DateTimeOffset dateTimeOffset, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            ValueRange range = dateTimeOffset.Range(fieldName.ChronoField);
            return dateTimeOffset.With(fieldName.ChronoField, range.Minimum);
        }

        public DateTime Evaluate(DateTime dateTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            ValueRange range = dateTime.Range(fieldName.ChronoField);
            return dateTime.With(fieldName.ChronoField, range.Minimum);
        }

        public CodegenExpression CodegenDateTimeOffset(CodegenExpression dateTimeOffset, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope)
        {
            return CodegenDateTimeOffsetZDTMinMax(dateTimeOffset, false, fieldName);
        }

        public CodegenExpression CodegenDateTime(CodegenExpression dateTime, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope)
        {
            return CodegenDateTimeOffsetZDTMinMax(dateTime, false, fieldName);
        }
    }
} // end of namespace