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
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.datetime.dtlocal.DTLocalUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtxOpsDateEval : DTLocalEvaluatorCalOpsCalBase,
        DTLocalEvaluator
    {
        private readonly TimeZoneInfo timeZone;

        public DTLocalDtxOpsDateEval(
            IList<CalendarOp> calendarOps,
            TimeZoneInfo timeZone)
            : base(calendarOps)
        {
            this.timeZone = timeZone;
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            Date dateValue = (Date) target;
            var dtx = DateTimeEx.GetInstance(timeZone);
            dtx.SetUtcMillis(dateValue.Time);

            EvaluateCalOpsCalendar(calendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);

            return dtx.Time;
        }

        public static CodegenExpression Codegen(
            DTLocalDtxOpsDateForge forge,
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(typeof(Date), typeof(DTLocalDtxOpsDateEval), codegenClassScope)
                .AddParam(innerType, "target");

            CodegenExpression timeZoneField = codegenClassScope.AddOrGetFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var block = methodNode.Block
                .DeclareVar<DateTimeEx>("dtx", StaticMethod(typeof(DateTimeEx), "getInstance", timeZoneField))
                .Expression(SetProperty(Ref("dtx"), "TimeInMillis", ExprDotName(Ref("target"), "Time")));
            EvaluateCalOpsCalendarCodegen(block, forge.calendarForges, Ref("dtx"), methodNode, exprSymbol, codegenClassScope);
            block.MethodReturn(ExprDotName(Ref("dtx"), "Time"));
            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace