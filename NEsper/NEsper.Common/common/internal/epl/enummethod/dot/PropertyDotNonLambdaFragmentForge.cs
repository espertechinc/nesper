///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class PropertyDotNonLambdaFragmentForge : ExprForge,
        ExprEvaluator,
        ExprNodeRenderable
    {
        private readonly EventPropertyGetterSPI getter;

        private readonly int streamId;

        public PropertyDotNonLambdaFragmentForge(
            int streamId,
            EventPropertyGetterSPI getter)
        {
            this.streamId = streamId;
            this.getter = getter;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var @event = eventsPerStream[streamId];
            if (@event == null) {
                return null;
            }

            return getter.GetFragment(@event);
        }

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => typeof(EventBean);

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean), typeof(PropertyDotNonLambdaFragmentForge), codegenClassScope);

            var refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .DeclareVar(typeof(EventBean), "event", ArrayAtIndex(refEPS, Constant(streamId)))
                .IfRefNullReturnNull("event")
                .MethodReturn(
                    Cast(
                        typeof(EventBean),
                        getter.EventBeanFragmentCodegen(Ref("event"), methodNode, codegenClassScope)));
            return LocalMethod(methodNode);
        }

        public ExprNodeRenderable ForgeRenderable => this;

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace