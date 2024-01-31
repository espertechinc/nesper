///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public class CalendarForgeRound : CalendarForge,
        CalendarOp
    {
        private readonly DateTimeFieldEnum field;
        private readonly DateTimeMethodEnum method;

        public CalendarForgeRound(
            DateTimeFieldEnum field,
            DateTimeMethodEnum method)
        {
            this.field = field;
            this.method = method;
        }

        public CalendarOp EvalOp => this;

        public DateTimeEx Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            switch (method) {
                case DateTimeMethodEnum.ROUNDFLOOR:
                    return dateTimeEx.Truncate(field);

                case DateTimeMethodEnum.ROUNDCEILING:
                    return dateTimeEx.Ceiling(field);

                case DateTimeMethodEnum.ROUNDHALF:
                    return dateTimeEx.Round(field);

                default:
                    throw new UnsupportedOperationException();
            }
        }

        public DateTimeOffset Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            switch (method) {
                case DateTimeMethodEnum.ROUNDFLOOR:
                    return dateTimeOffset.Truncate(field);

                case DateTimeMethodEnum.ROUNDCEILING:
                    return dateTimeOffset.Ceiling(field);

                case DateTimeMethodEnum.ROUNDHALF:
                    return dateTimeOffset.Round(field);

                default:
                    throw new UnsupportedOperationException();
            }
        }

        public DateTime Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            switch (method) {
                case DateTimeMethodEnum.ROUNDFLOOR:
                    return dateTime.Truncate(field);

                case DateTimeMethodEnum.ROUNDCEILING:
                    return dateTime.Ceiling(field);

                case DateTimeMethodEnum.ROUNDHALF:
                    return dateTime.Round(field);

                default:
                    throw new UnsupportedOperationException();
            }
        }

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression dateTimeEx,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var dateTimeField = EnumValue(field);
            switch (method) {
                case DateTimeMethodEnum.ROUNDFLOOR:
                    return ExprDotMethod(dateTimeEx, "Truncate", dateTimeField);

                case DateTimeMethodEnum.ROUNDCEILING:
                    return ExprDotMethod(dateTimeEx, "Ceiling", dateTimeField);

                case DateTimeMethodEnum.ROUNDHALF:
                    return ExprDotMethod(dateTimeEx, "Round", dateTimeField);

                default:
                    throw new UnsupportedOperationException();
            }
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression dateTimeOffset,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var dateTimeField = EnumValue(field);
            switch (method) {
                case DateTimeMethodEnum.ROUNDFLOOR:
                    return StaticMethod(typeof(DateTimeOffsetHelper), "Truncate", dateTimeOffset, dateTimeField);

                case DateTimeMethodEnum.ROUNDCEILING:
                    return StaticMethod(typeof(DateTimeOffsetHelper), "Ceiling", dateTimeOffset, dateTimeField);

                case DateTimeMethodEnum.ROUNDHALF:
                    return StaticMethod(typeof(DateTimeOffsetHelper), "Round", dateTimeOffset, dateTimeField);

                default:
                    throw new UnsupportedOperationException();
            }
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression dateTime,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var dateTimeField = EnumValue(field);
            switch (method) {
                case DateTimeMethodEnum.ROUNDFLOOR:
                    return StaticMethod(typeof(DateTimeHelper), "Truncate", dateTime, dateTimeField);

                case DateTimeMethodEnum.ROUNDCEILING:
                    return StaticMethod(typeof(DateTimeHelper), "Ceiling", dateTime, dateTimeField);

                case DateTimeMethodEnum.ROUNDHALF:
                    return StaticMethod(typeof(DateTimeHelper), "Round", dateTime, dateTimeField);

                default:
                    throw new UnsupportedOperationException();
            }
        }
    }
} // end of namespace