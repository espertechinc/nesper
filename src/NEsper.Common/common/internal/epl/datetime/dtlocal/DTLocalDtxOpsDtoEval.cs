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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtxOpsDtoEval : DTLocalEvaluatorCalOpsCalBase,
        DTLocalEvaluator
    {
        public DTLocalDtxOpsDtoEval(IList<CalendarOp> calendarOps)
            : base(calendarOps)
        {
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dto = (DateTimeOffset)target;
            return DTLocalUtil.EvaluateCalOpsDto(calendarOps, dto, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDtxOpsDtoForge forge,
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(DateTimeOffset), typeof(DTLocalDtxOpsDtoEval), codegenClassScope)
                .AddParam<DateTimeOffset>("target");
            var block = methodNode.Block;
            DTLocalUtil.EvaluateCalOpsDtoCodegen(
                block,
                "target",
                forge.calendarForges,
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.MethodReturn(Ref("target"));
            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace