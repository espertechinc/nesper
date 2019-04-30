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
using com.espertech.esper.common.@internal.epl.datetime.reformatop;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtxOpsReformatEval : DTLocalEvaluatorCalopReformatBase
    {
        public DTLocalDtxOpsReformatEval(
            IList<CalendarOp> calendarOps,
            ReformatOp reformatOp)
            : base(
                calendarOps, reformatOp)
        {
        }

        public override object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            DateTimeEx dtx = (DateTimeEx) ((DateTimeEx) target).Clone();
            DTLocalUtil.EvaluateCalOpsCalendar(calendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
            return reformatOp.Evaluate(dtx, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDtxOpsReformatForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                    forge.reformatForge.ReturnType, typeof(DTLocalDateOpsReformatEval), codegenClassScope)
                .AddParam(typeof(DateTimeEx), "target");

            CodegenBlock block = methodNode.Block
                .DeclareVar(
                    typeof(DateTimeEx), "dtx", Cast(typeof(DateTimeEx), ExprDotMethod(@Ref("target"), "clone")));
            DTLocalUtil.EvaluateCalOpsCalendarCodegen(
                block, forge.calendarForges, @Ref("dtx"), methodNode, exprSymbol, codegenClassScope);
            block.MethodReturn(forge.reformatForge.CodegenDateTimeEx(@Ref("dtx"), methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace