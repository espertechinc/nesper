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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public class CalendarWithMaxForge : CalendarForge,
        CalendarOp
    {
        private readonly DateTimeFieldEnum _field;

        public CalendarWithMaxForge(DateTimeFieldEnum field)
        {
            _field = field;
        }

        public CalendarOp EvalOp => this;

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression dateTimeEx,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotMethod(
                dateTimeEx,
                "SetFieldValue",
                Constant(_field),
                ExprDotMethod(dateTimeEx, "GetActualMaximum", Constant(_field)));
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression dateTimeOffset,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CodegenDateTimeOffsetDtxMinMax(dateTimeOffset, true, _field);
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression dateTime,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return CodegenDateTimeOffsetDtxMinMax(dateTime, true, _field);
        }

        public DateTimeEx Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return dateTimeEx.SetFieldValue(_field, dateTimeEx.GetActualMaximum(_field));
        }

        public DateTimeOffset Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            ValueRange<int> range = dateTimeOffset.Range(_field);
            return dateTimeOffset.With(_field, range.Maximum);
        }

        public DateTime Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            ValueRange<int> range = dateTime.Range(_field);
            return dateTime.With(_field, range.Maximum);
        }

        protected internal static CodegenExpression CodegenDateTimeOffsetDtxMinMax(
            CodegenExpression val,
            bool max,
            DateTimeFieldEnum field)
        {
            var fieldExpr = EnumValue(field);
            var valueRange = ExprDotMethod(val, "Range", fieldExpr);
            return ExprDotMethod(val, "With", fieldExpr, GetProperty(valueRange, max ? "Maximum" : "Minimum"));
        }
    }
} // end of namespace