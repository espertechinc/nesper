///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public class IntervalForgeOp : IntervalOp
    {
        private readonly ExprEvaluator evaluatorTimestamp;
        private readonly IntervalForgeImpl.IIntervalOpEval intervalOpEval;

        public IntervalForgeOp(
            ExprEvaluator evaluatorTimestamp,
            IntervalForgeImpl.IIntervalOpEval intervalOpEval)
        {
            this.evaluatorTimestamp = evaluatorTimestamp;
            this.intervalOpEval = intervalOpEval;
        }

        public object Evaluate(
            long startTs,
            long endTs,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var parameter = evaluatorTimestamp.Evaluate(eventsPerStream, isNewData, context);
            if (parameter == null) {
                return parameter;
            }

            return intervalOpEval.Evaluate(startTs, endTs, parameter, eventsPerStream, isNewData, context);
        }

        public static CodegenExpression Codegen(
            IntervalForgeImpl forge,
            CodegenExpression start,
            CodegenExpression end,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(typeof(bool?), typeof(IntervalForgeOp), codegenClassScope)
                .AddParam(typeof(long), "startTs").AddParam(typeof(long), "endTs");

            var evaluationType = forge.ForgeTimestamp.EvaluationType;
            var block = methodNode.Block
                .DeclareVar(
                    evaluationType, "parameter",
                    forge.ForgeTimestamp.EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope));
            if (!forge.ForgeTimestamp.EvaluationType.IsPrimitive) {
                block.IfRefNullReturnNull("parameter");
            }

            block.MethodReturn(
                forge.IntervalOpForge.Codegen(
                    Ref("startTs"), Ref("endTs"), Ref("parameter"), forge.ForgeTimestamp.EvaluationType, methodNode,
                    exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, start, end);
        }
    }
} // end of namespace