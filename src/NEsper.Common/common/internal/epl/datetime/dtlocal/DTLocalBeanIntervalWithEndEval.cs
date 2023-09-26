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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalBeanIntervalWithEndEval : DTLocalEvaluator
    {
        private readonly EventPropertyGetter getterEndTimestamp;
        private readonly EventPropertyGetter getterStartTimestamp;
        private readonly DTLocalEvaluatorIntervalComp inner;

        public DTLocalBeanIntervalWithEndEval(
            EventPropertyGetter getterStartTimestamp,
            EventPropertyGetter getterEndTimestamp,
            DTLocalEvaluatorIntervalComp inner)
        {
            this.getterStartTimestamp = getterStartTimestamp;
            this.getterEndTimestamp = getterEndTimestamp;
            this.inner = inner;
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var start = getterStartTimestamp.Get((EventBean)target);
            if (start == null) {
                return null;
            }

            var end = getterEndTimestamp.Get((EventBean)target);
            if (end == null) {
                return null;
            }

            return inner.Evaluate(start, end, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalBeanIntervalWithEndForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalBeanIntervalWithEndEval), codegenClassScope)
                .AddParam<EventBean>("target");

            var block = methodNode.Block;
            block.DeclareVar(
                forge.getterStartReturnType,
                "start",
                CodegenLegoCast.CastSafeFromObjectType(
                    forge.getterStartReturnType,
                    forge.getterStartTimestamp.EventBeanGetCodegen(
                        Ref("target"),
                        methodNode,
                        codegenClassScope)));
            if (forge.getterStartReturnType.CanBeNull()) {
                block.IfRefNullReturnNull("start");
            }

            block.DeclareVar(
                forge.getterEndReturnType,
                "end",
                CodegenLegoCast.CastSafeFromObjectType(
                    forge.getterEndReturnType,
                    forge.getterEndTimestamp.EventBeanGetCodegen(
                        Ref("target"),
                        methodNode,
                        codegenClassScope)));
            if (forge.getterEndReturnType.CanBeNull()) {
                block.IfRefNullReturnNull("end");
            }

            var startValue = Unbox(Ref("start"), forge.getterStartReturnType);
            var endValue = Unbox(Ref("end"), forge.getterEndReturnType);

            block.MethodReturn(
                forge.inner.Codegen(
                    startValue,
                    endValue,
                    methodNode,
                    exprSymbol,
                    codegenClassScope));

            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace