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
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.datetime.dtlocal.DTLocalUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtxOpsIntervalEval : DTLocalEvaluatorCalOpsIntervalBase
    {
        private readonly TimeZoneInfo timeZone;

        public DTLocalDtxOpsIntervalEval(
            IList<CalendarOp> calendarOps,
            IntervalOp intervalOp,
            TimeZoneInfo timeZone)
            : base(calendarOps, intervalOp)
        {
            this.timeZone = timeZone;
        }

        public override object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dtx = ((DateTimeEx)target).Clone();
            EvaluateCalOpsDtx(calendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
            var time = dtx.UtcMillis;
            return intervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression CodegenPointInTime(
            DTLocalDtxOpsIntervalForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalDtxOpsIntervalEval), codegenClassScope)
                .AddParam<DateTimeEx>("target");

            var block = methodNode.Block
                .DeclareVar<DateTimeEx>("dtx", Cast(typeof(DateTimeEx), ExprDotMethod(Ref("target"), "Clone")));
            EvaluateCalOpsDtxCodegen(
                block,
                forge.calendarForges,
                Ref("dtx"),
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.DeclareVar<long>("time", ExprDotName(Ref("dtx"), "UtcMillis"))
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
            var startLong = ((DateTimeEx)startTimestamp).UtcMillis;
            var endLong = ((DateTimeEx)endTimestamp).UtcMillis;
            var dtx = DateTimeEx.GetInstance(timeZone);
            dtx.SetUtcMillis(startLong);
            EvaluateCalOpsDtx(calendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
            var startTime = dtx.UtcMillis;
            var endTime = startTime + (endLong - startLong);
            return intervalOp.Evaluate(startTime, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression CodegenStartEnd(
            DTLocalDtxOpsIntervalForge forge,
            CodegenExpression start,
            CodegenExpression end,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression timeZoneField =
                codegenClassScope.AddOrGetDefaultFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalDtxOpsIntervalEval), codegenClassScope)
                .AddParam<DateTimeEx>("startTimestamp")
                .AddParam<DateTimeEx>("endTimestamp");

            var block = methodNode.Block
                .DeclareVar<long>("startLong", ExprDotName(Ref("startTimestamp"), "UtcMillis"))
                .DeclareVar<long>("endLong", ExprDotName(Ref("endTimestamp"), "UtcMillis"))
                .DeclareVar<DateTimeEx>("dtx", StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField))
                .Expression(ExprDotMethod(Ref("dtx"), "SetUtcMillis", Ref("startLong")));
            EvaluateCalOpsDtxCodegen(
                block,
                forge.calendarForges,
                Ref("dtx"),
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.DeclareVar<long>("startTime", ExprDotName(Ref("dtx"), "UtcMillis"))
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