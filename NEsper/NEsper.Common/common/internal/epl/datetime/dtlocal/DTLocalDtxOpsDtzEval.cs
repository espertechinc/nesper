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
using static com.espertech.esper.common.@internal.epl.datetime.dtlocal.DTLocalUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtxOpsDtzEval : DTLocalEvaluatorCalOpsCalBase,
        DTLocalEvaluator
    {
        public DTLocalDtxOpsDtzEval(IList<CalendarOp> calendarOps)
            : base(calendarOps)
        {
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            DateTime dateTime = (DateTime) target;
            return EvaluateCalOpsDtx(calendarOps, dateTime, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDtxOpsDtzForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope
                .MakeChild(typeof(DateTime), typeof(DTLocalDtxOpsDtzEval), codegenClassScope)
                .AddParam(typeof(DateTime), "dateTime");

            CodegenBlock block = methodNode.Block;
            EvaluateCalOpsDtxCodegen(
                block,
                "dateTime",
                forge.calendarForges,
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.MethodReturn(@Ref("dateTime"));
            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace