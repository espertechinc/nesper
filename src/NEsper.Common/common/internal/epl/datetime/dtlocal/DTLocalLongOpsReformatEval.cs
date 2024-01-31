///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.datetime.reformatop;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalLongOpsReformatEval : DTLocalEvaluatorCalopReformatBase
    {
        private readonly TimeAbacus timeAbacus;

        private readonly TimeZoneInfo timeZone;

        public DTLocalLongOpsReformatEval(
            IList<CalendarOp> calendarOps,
            ReformatOp reformatOp,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
            : base(calendarOps, reformatOp)
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
            timeAbacus.DateTimeSet(target.AsInt64(), dtx);
            DTLocalUtil.EvaluateCalOpsDtx(calendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
            return reformatOp.Evaluate(dtx, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalLongOpsReformatForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression timeZoneField =
                codegenClassScope.AddOrGetDefaultFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var methodNode = codegenMethodScope.MakeChild(
                    forge.reformatForge.ReturnType,
                    typeof(DTLocalLongOpsReformatEval),
                    codegenClassScope)
                .AddParam<long>("target");

            var block = methodNode.Block
                .DeclareVar<DateTimeEx>("dtx", StaticMethod(typeof(DateTimeEx), "GetInstance", timeZoneField))
                .Expression(
                    forge.timeAbacus.DateTimeSetCodegen(Ref("target"), Ref("dtx"), methodNode, codegenClassScope));
            DTLocalUtil.EvaluateCalOpsDtxCodegen(
                block,
                forge.calendarForges,
                Ref("dtx"),
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.MethodReturn(
                forge.reformatForge.CodegenDateTimeEx(Ref("dtx"), methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace