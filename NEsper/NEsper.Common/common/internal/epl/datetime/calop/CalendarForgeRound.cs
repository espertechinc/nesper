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
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public class CalendarForgeRound : CalendarForge,
        CalendarOp
    {
        private readonly int code;
        private readonly CalendarFieldEnum fieldName;

        public CalendarForgeRound(
            CalendarFieldEnum fieldName,
            DatetimeMethodEnum method)
        {
            this.fieldName = fieldName;
            if (method == DatetimeMethodEnum.ROUNDCEILING) {
                code = ApacheCommonsDateUtils.MODIFY_CEILING;
            }
            else if (method == DatetimeMethodEnum.ROUNDFLOOR) {
                code = ApacheCommonsDateUtils.MODIFY_TRUNCATE;
            }
            else if (method == DatetimeMethodEnum.ROUNDHALF) {
                code = ApacheCommonsDateUtils.MODIFY_ROUND;
            }
            else {
                throw new ArgumentException("Unrecognized method '" + method + "'");
            }
        }

        public CalendarOp EvalOp => this;

        public void Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var dateTime = ApacheCommonsDateUtils.Modify(
                dateTimeEx.DateTime.DateTime, fieldName.GetCalendarField(), code);

        }

        public DateTimeOffset Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (code == ApacheCommonsDateUtils.MODIFY_TRUNCATE) {
                return dateTimeOffset.TruncatedTo(fieldName.ChronoUnit);
            }

            if (code == ApacheCommonsDateUtils.MODIFY_CEILING) {
                return dateTimeOffset.Plus(1, fieldName.ChronoUnit).TruncatedTo(fieldName.ChronoUnit);
            }

            throw new EPException("Round-half operation not supported for LocalDateTime");
        }

        public DateTime Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (code == ApacheCommonsDateUtils.MODIFY_TRUNCATE) {
                return dateTime.TruncatedTo(fieldName.ChronoUnit);
            }

            if (code == ApacheCommonsDateUtils.MODIFY_CEILING) {
                return dateTime.Plus(1, fieldName.ChronoUnit).TruncatedTo(fieldName.ChronoUnit);
            }

            throw new EPException("Round-half operation not supported for ZonedDateTime");
        }

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression dateTimeEx,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                typeof(ApacheCommonsDateUtils), "modify", dateTimeEx,
                Constant(fieldName.GetCalendarField()), Constant(code));
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression dateTimeOffset,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return Codegen<DateTimeOffset>(dateTimeOffset);
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression dateTime,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return Codegen<DateTime>(dateTime);
        }

        private CodegenExpression Codegen<T>(
            CodegenExpression val)
        {
            var type = typeof(T);
            var chronoUnit = EnumValue(typeof(ChronoUnit), fieldName.ChronoUnit.Name());
            if (code == ApacheCommonsDateUtils.MODIFY_TRUNCATE) {
                return ExprDotMethod(val, "truncatedTo", chronoUnit);
            }

            if (code == ApacheCommonsDateUtils.MODIFY_CEILING) {
                return ExprDotMethodChain(val).Add("plus", Constant(1), chronoUnit).Add("truncatedTo", chronoUnit);
            }

            throw new EPException("Round-half operation not supported for " + type.Name);
        }
    }
} // end of namespace