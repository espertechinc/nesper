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
    public class PropertyDotNonLambdaMappedForgeEval : ExprEvaluator
    {
        private readonly PropertyDotNonLambdaMappedForge _forge;
        private readonly ExprEvaluator _paramEval;

        public PropertyDotNonLambdaMappedForgeEval(
            PropertyDotNonLambdaMappedForge forge,
            ExprEvaluator paramEval)
        {
            _forge = forge;
            _paramEval = paramEval;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var @event = eventsPerStream[_forge.StreamId];
            if (@event == null) {
                return null;
            }

            var key = (string)_paramEval.Evaluate(eventsPerStream, isNewData, context);
            return _forge.MappedGetter.Get(@event, key);
        }

        public static CodegenExpression Codegen(
            PropertyDotNonLambdaMappedForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(PropertyDotNonLambdaMappedForgeEval),
                codegenClassScope);

            var refEPS = exprSymbol.GetAddEps(methodNode);
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