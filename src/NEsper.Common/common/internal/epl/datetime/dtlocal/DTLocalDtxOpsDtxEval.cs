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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.datetime.dtlocal.DTLocalUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtxOpsDtxEval : DTLocalEvaluatorCalOpsCalBase,
        DTLocalEvaluator
    {
        public DTLocalDtxOpsDtxEval(IList<CalendarOp> calendarOps)
            : base(calendarOps)
        {
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dtxValue = (DateTimeEx)target;
            var dtx = dtxValue.Clone();

            EvaluateCalOpsDtx(calendarOps, dtx, eventsPerStream, isNewData, exprEvaluatorContext);

            return dtx;
        }

        public static CodegenExpression Codegen(
            DTLocalDtxOpsDtxForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(DateTimeEx), typeof(DTLocalDtxOpsDtxEval), codegenClassScope)
                .AddParam<DateTimeEx>("target");

            var block = methodNode.Block.DeclareVar<DateTimeEx>(
                "dtx",
                ExprDotMethod(Ref("target"), "Clone"));
            EvaluateCalOpsDtxCodegen(
                block,
                forge.calendarForges,
                Ref("dtx"),
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.MethodReturn(Ref("dtx"));
            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace