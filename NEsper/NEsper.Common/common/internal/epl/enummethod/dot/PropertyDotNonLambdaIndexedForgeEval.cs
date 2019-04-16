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

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class PropertyDotNonLambdaIndexedForgeEval : ExprEvaluator
    {
        private readonly PropertyDotNonLambdaIndexedForge forge;
        private readonly ExprEvaluator paramEval;

        public PropertyDotNonLambdaIndexedForgeEval(
            PropertyDotNonLambdaIndexedForge forge,
            ExprEvaluator paramEval)
        {
            this.forge = forge;
            this.paramEval = paramEval;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var @event = eventsPerStream[forge.StreamId];
            if (@event == null) {
                return null;
            }

            var key = (int) paramEval.Evaluate(eventsPerStream, isNewData, context);
            return forge.IndexedGetter.Get(@event, key);
        }

        public static CodegenExpression Codegen(
            PropertyDotNonLambdaIndexedForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType, typeof(PropertyDotNonLambdaIndexedForgeEval), codegenClassScope);

            var refEPS = exprSymbol.GetAddEPS(methodNode);
            var evaluationType = forge.ParamForge.EvaluationType;
            methodNode.Block
                .DeclareVar(typeof(EventBean), "event", ArrayAtIndex(refEPS, Constant(forge.StreamId)))
                .IfRefNullReturnNull("event")
                .DeclareVar(
                    evaluationType, "key",
                    forge.ParamForge.EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope))
                .MethodReturn(
                    forge.IndexedGetter.EventBeanGetIndexedCodegen(
                        methodNode, codegenClassScope, Ref("event"), Ref("key")));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace