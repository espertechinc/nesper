///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalUtil
    {
        public static IList<CalendarOp> GetCalendarOps(IList<CalendarForge> forges)
        {
            IList<CalendarOp> ops = new List<CalendarOp>(forges.Count);
            foreach (CalendarForge forge in forges) {
                ops.Add(forge.EvalOp);
            }

            return ops;
        }

        protected internal static void EvaluateCalOpsCalendar(
            IList<CalendarOp> calendarOps,
            DateTimeEx cal,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (CalendarOp calendarOp in calendarOps) {
                calendarOp.Evaluate(cal, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        protected internal static void EvaluateCalOpsCalendarCodegen(
            CodegenBlock block,
            IList<CalendarForge> calendarForges,
            CodegenExpressionRef cal,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            foreach (CalendarForge calendarForge in calendarForges) {
                block.Expression(calendarForge.CodegenDateTimeEx(cal, codegenMethodScope, exprSymbol, codegenClassScope));
            }
        }

        protected internal static DateTimeOffset EvaluateCalOpsLDT(
            IList<CalendarOp> calendarOps,
            DateTimeOffset dto,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (CalendarOp calendarOp in calendarOps) {
                dto = calendarOp.Evaluate(dto, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            return dto;
        }

        protected internal static void EvaluateCalOpsLDTCodegen(
            CodegenBlock block,
            string resultVariable,
            IList<CalendarForge> calendarForges,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            foreach (CalendarForge calendarForge in calendarForges) {
                block.AssignRef(
                    resultVariable, calendarForge.CodegenDateTimeOffset(@Ref(resultVariable), codegenMethodScope, exprSymbol, codegenClassScope));
            }
        }

        protected internal static DateTime EvaluateCalOpsZDT(
            IList<CalendarOp> calendarOps,
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (CalendarOp calendarOp in calendarOps) {
                dateTime = calendarOp.Evaluate(dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            return dateTime;
        }

        protected internal static void EvaluateCalOpsZDTCodegen(
            CodegenBlock block,
            string resultVariable,
            IList<CalendarForge> calendarForges,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            foreach (CalendarForge calendarForge in calendarForges) {
                block.AssignRef(
                    resultVariable, calendarForge.CodegenDateTime(@Ref(resultVariable), codegenMethodScope, exprSymbol, codegenClassScope));
            }
        }
    }
} // end of namespace