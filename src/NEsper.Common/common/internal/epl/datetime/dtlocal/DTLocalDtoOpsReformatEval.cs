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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtoOpsReformatEval : DTLocalEvaluatorCalopReformatBase
    {
        public DTLocalDtoOpsReformatEval(
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
            var dto = (DateTimeOffset)target;
            dto = DTLocalUtil.EvaluateCalOpsDto(calendarOps, dto, eventsPerStream, isNewData, exprEvaluatorContext);
            return reformatOp.Evaluate(dto, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDtoOpsReformatForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                    forge.reformatForge.ReturnType,
                    typeof(DTLocalDtoOpsReformatEval),
                    codegenClassScope)
                .AddParam<DateTimeOffset>("dto");

            var block = methodNode.Block;
            DTLocalUtil.EvaluateCalOpsDtoCodegen(
                block,
                "dto",
                forge.calendarForges,
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.MethodReturn(
                forge.reformatForge.CodegenDateTimeOffset(Ref("dto"), methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace