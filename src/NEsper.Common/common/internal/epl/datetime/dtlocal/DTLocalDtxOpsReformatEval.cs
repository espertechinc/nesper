///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.datetime.reformatop;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtxOpsReformatEval : DTLocalEvaluatorCalopReformatBase
    {
        public DTLocalDtxOpsReformatEval(
            IList<CalendarOp> calendarOps,
            ReformatOp reformatOp)
            : base(
                calendarOps,
                reformatOp)
        {
        }

        public override object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dtx = ((DateTimeEx) target).Clone();
            DTLocalUtil.EvaluateCalOpsDtx(calendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);
            return reformatOp.Evaluate(dtx, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDtxOpsReformatForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                    forge.reformatForge.ReturnType,
                    typeof(DTLocalDtxOpsReformatEval),
                    codegenClassScope)
                .AddParam(typeof(DateTimeEx), "target");

            var block = methodNode.Block
                .DeclareVar<DateTimeEx>(
                    "dtx",
                    Cast(typeof(DateTimeEx), ExprDotMethod(Ref("target"), "Clone")));
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