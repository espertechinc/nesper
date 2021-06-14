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

namespace com.espertech.esper.common.@internal.epl.expression.dot.propertydot
{
    public class PropertyDotNonLambdaMappedForgeEval : ExprEvaluator
    {
        private readonly PropertyDotNonLambdaMappedForge forge;
        private readonly ExprEvaluator paramEval;

        public PropertyDotNonLambdaMappedForgeEval(
            PropertyDotNonLambdaMappedForge forge,
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
            EventBean @event = eventsPerStream[forge.StreamId];
            if (@event == null) {
                return null;
            }

            string key = (string) paramEval.Evaluate(eventsPerStream, isNewData, context);
            return forge.MappedGetter.Get(@event, key);
        }

        public static CodegenExpression Codegen(
            PropertyDotNonLambdaMappedForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(PropertyDotNonLambdaMappedForgeEval),
                codegenClassScope);

            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(forge.StreamId)))
                .IfRefNullReturnNull("@event")
                .DeclareVar<string>(
                    "key",
                    forge.ParamForge.EvaluateCodegen(typeof(string), methodNode, exprSymbol, codegenClassScope))
                .MethodReturn(
                    forge.MappedGetter.EventBeanGetMappedCodegen(
                        methodNode,
                        codegenClassScope,
                        Ref("@event"),
                        Ref("key")));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace