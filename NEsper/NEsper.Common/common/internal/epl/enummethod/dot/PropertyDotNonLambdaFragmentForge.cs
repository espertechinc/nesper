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
        private readonly bool array;

        public PropertyDotNonLambdaFragmentForge(
            int streamId,
            EventPropertyGetterSPI getter,
            bool array)
        {
            this.streamId = streamId;
            this.getter = getter;
            this.array = array;
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

        public Type EvaluationType => array? typeof(EventBean[]) : typeof(EventBean);

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var returnType = array ? typeof(EventBean[]) : typeof(EventBean);
            var methodNode = codegenMethodScope
                .MakeChild(returnType, typeof(PropertyDotNonLambdaFragmentForge), codegenClassScope);

            var refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(streamId)))
                .IfRefNullReturnNull("@event")
                .MethodReturn(Cast(returnType, getter.EventBeanFragmentCodegen(Ref("@event"), methodNode, codegenClassScope)));
            return LocalMethod(methodNode);
        }

        public ExprNodeRenderable ExprForgeRenderable => this;

        public void ToEPL(TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace