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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalUtil
    {
        public static IList<CalendarOp> GetCalendarOps(IList<CalendarForge> forges)
        {
            IList<CalendarOp> ops = new List<CalendarOp>(forges.Count);
            foreach (var forge in forges) {
                ops.Add(forge.EvalOp);
            }

            return ops;
        }

        internal static void EvaluateCalOpsDtx(
            IList<CalendarOp> calendarOps,
            DateTimeEx dtx,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (var calendarOp in calendarOps) {
                calendarOp.Evaluate(dtx, eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        internal static void EvaluateCalOpsDtxCodegen(
            CodegenBlock block,
            IList<CalendarForge> calendarForges,
            CodegenExpressionRef dtx,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            foreach (var calendarForge in calendarForges) {
                // block.Expression
                block.AssignRef(dtx, calendarForge.CodegenDateTimeEx(dtx, codegenMethodScope, exprSymbol, codegenClassScope));
            }
        }

        internal static DateTimeOffset EvaluateCalOpsDto(
            IList<CalendarOp> calendarOps,
            DateTimeOffset dto,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (var calendarOp in calendarOps) {
                dto = calendarOp.Evaluate(dto, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            return dto;
        }

        internal static void EvaluateCalOpsDtoCodegen(
            CodegenBlock block,
            string resultVariable,
            IList<CalendarForge> calendarForges,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            foreach (var calendarForge in calendarForges) {
                block.AssignRef(
                    resultVariable,
                    calendarForge.CodegenDateTimeOffset(
                        Ref(resultVariable),
                        codegenMethodScope,
                        exprSymbol,
                        codegenClassScope));
            }
        }

        internal static DateTime EvaluateCalOpsDateTime(
            IList<CalendarOp> calendarOps,
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (var calendarOp in calendarOps) {
                dateTime = calendarOp.Evaluate(dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
            }

            return dateTime;
        }

        internal static void EvaluateCalOpsDateTimeCodegen(
            CodegenBlock block,
            string resultVariable,
            IList<CalendarForge> calendarForges,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            foreach (var calendarForge in calendarForges) {
                block.AssignRef(
                    resultVariable,
                    calendarForge.CodegenDateTime(
                        Ref(resultVariable),
                        codegenMethodScope,
                        exprSymbol,
                        codegenClassScope));
            }
        }
    }
} // end of namespace