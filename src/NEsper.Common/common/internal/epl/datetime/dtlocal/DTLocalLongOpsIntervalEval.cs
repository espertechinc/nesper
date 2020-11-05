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
using com.espertech.esper.common.@internal.epl.datetime.interval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.datetime.dtlocal.DTLocalUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalLongOpsIntervalEval : DTLocalEvaluatorCalOpsIntervalBase
    {
        private readonly TimeAbacus timeAbacus;

        private readonly TimeZoneInfo timeZone;

        public DTLocalLongOpsIntervalEval(
            IList<CalendarOp> calendarOps,
            IntervalOp intervalOp,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
            : base(calendarOps, intervalOp)
        {
            this.timeZone = timeZone;
            this.timeAbacus = timeAbacus;
        }

        public override object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dtx = DateTimeEx.GetInstance(timeZone);
            var startRemainder = timeAbacus.DateTimeSet(target.AsInt64(), dtx);
            EvaluateCalOpsDtx(calendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
            var time = timeAbacus.DateTimeGet(dtx, startRemainder);
            return intervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression CodegenPointInTime(
            DTLocalLongOpsIntervalForge forge,
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression timeZoneField =
                codegenClassScope.AddOrGetDefaultFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalLongOpsIntervalEval), codegenClassScope)
                .AddParam(typeof(long), "target");

            var block = methodNode.Block
                .DeclareVar<DateTimeEx>("dtx", StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField))
                .DeclareVar<long>(
                    "startRemainder",
                    forge.timeAbacus.DateTimeSetCodegen(Ref("target"), Ref("dtx"), methodNode, codegenClassScope));
            EvaluateCalOpsDtxCodegen(
                block,
                forge.calendarForges,
                Ref("dtx"),
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.DeclareVar<long>(
                    "time",
                    forge.timeAbacus.DateTimeGetCodegen(Ref("dtx"), Ref("startRemainder"), codegenClassScope))
                .MethodReturn(
                    forge.intervalForge.Codegen(Ref("time"), Ref("time"), methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, inner);
        }

        public override object Evaluate(
            object startTimestamp,
            object endTimestamp,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var startLong = startTimestamp.AsInt64();
            var endLong = endTimestamp.AsInt64();
            var dtx = DateTimeEx.GetInstance(timeZone);
            var startRemainder = timeAbacus.DateTimeSet(startLong, dtx);
            EvaluateCalOpsDtx(calendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
            var startTime = timeAbacus.DateTimeGet(dtx, startRemainder);
            var endTime = startTime + (endLong - startLong);
            return intervalOp.Evaluate(startTime, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression CodegenStartEnd(
            DTLocalLongOpsIntervalForge forge,
            CodegenExpression start,
            CodegenExpression end,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression timeZoneField =
                codegenClassScope.AddOrGetDefaultFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalLongOpsIntervalEval), codegenClassScope)
                .AddParam(typeof(long), "startLong")
                .AddParam(typeof(long), "endLong");

            var block = methodNode.Block
                .DeclareVar<DateTimeEx>("dtx", StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField))
                .DeclareVar<long>(
                    "startRemainder",
                    forge.timeAbacus.DateTimeSetCodegen(Ref("startLong"), Ref("dtx"), methodNode, codegenClassScope));
            EvaluateCalOpsDtxCodegen(
                block,
                forge.calendarForges,
                Ref("dtx"),
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.DeclareVar<long>(
                    "startTime",
                    forge.timeAbacus.DateTimeGetCodegen(Ref("dtx"), Ref("startRemainder"), codegenClassScope))
                .DeclareVar<long>(
                    "endTime",
                    Op(Ref("startTime"), "+", Op(Ref("endLong"), "-", Ref("startLong"))))
                .MethodReturn(
                    forge.intervalForge.Codegen(
                        Ref("startTime"),
                        Ref("endTime"),
                        methodNode,
                        exprSymbol,
                        codegenClassScope));
            return LocalMethod(methodNode, start, end);
        }
    }
} // end of namespace