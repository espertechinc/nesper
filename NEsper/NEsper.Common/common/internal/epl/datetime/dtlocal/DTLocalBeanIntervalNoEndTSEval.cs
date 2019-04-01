///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalBeanIntervalNoEndTSEval : DTLocalEvaluator
    {
        private readonly EventPropertyGetter getter;
        private readonly DTLocalEvaluator inner;

        public DTLocalBeanIntervalNoEndTSEval(EventPropertyGetter getter, DTLocalEvaluator inner)
        {
            this.getter = getter;
            this.inner = inner;
        }

        public object Evaluate(
            object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var timestamp = getter.Get((EventBean) target);
            if (timestamp == null) {
                return null;
            }

            return inner.Evaluate(timestamp, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalBeanIntervalNoEndTSForge forge, CodegenExpression inner, Type innerType,
            CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(forge.returnType, typeof(DTLocalBeanIntervalNoEndTSEval), codegenClassScope)
                .AddParam(typeof(EventBean), "target");

            methodNode.Block
                .DeclareVar(
                    forge.getterResultType, "timestamp",
                    CodegenLegoCast.CastSafeFromObjectType(
                        forge.getterResultType,
                        forge.getter.EventBeanGetCodegen(Ref("target"), methodNode, codegenClassScope)))
                .IfRefNullReturnNull("timestamp")
                .MethodReturn(
                    forge.inner.Codegen(
                        Ref("timestamp"), forge.getterResultType, methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, inner);
        }
    }
} // end of namespace