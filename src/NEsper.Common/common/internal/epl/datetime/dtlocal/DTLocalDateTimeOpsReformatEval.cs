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
using static com.espertech.esper.common.@internal.epl.datetime.dtlocal.DTLocalUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDateTimeOpsReformatEval : DTLocalEvaluatorCalopReformatBase
    {
        public DTLocalDateTimeOpsReformatEval(
            IList<CalendarOp> calendarOps,
            ReformatOp reformatOp)
            : base(calendarOps, reformatOp)
        {
        }

        public override object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dateTime = (DateTime)target;
            dateTime = EvaluateCalOpsDateTime(calendarOps, dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
            return reformatOp.Evaluate(dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDateTimeOpsReformatForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                    forge.reformatForge.ReturnType,
                    typeof(DTLocalDateTimeOpsReformatEval),
                    codegenClassScope)
                .AddParam<DateTime>("dateTime");

            var block = methodNode.Block;
            EvaluateCalOpsDateTimeCodegen(
                block,
                "dateTime",
                forge.calendarForges,
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.MethodReturn(
                forge.reformatForge.CodegenDateTime(Ref("dateTime"), methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace