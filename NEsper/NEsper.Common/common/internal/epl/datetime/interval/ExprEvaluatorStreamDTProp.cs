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

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public class ExprEvaluatorStreamDTProp : ExprForge,
        ExprEvaluator,
        ExprNodeRenderable
    {
        private readonly EventPropertyGetterSPI getter;

        private readonly int streamId;

        public ExprEvaluatorStreamDTProp(int streamId, EventPropertyGetterSPI getter, Type getterReturnTypeBoxed)
        {
            this.streamId = streamId;
            this.getter = getter;
            EvaluationType = getterReturnTypeBoxed;
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var @event = eventsPerStream[streamId];
            if (@event == null) {
                return null;
            }

            return getter.Get(@event);
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprEvaluator ExprEvaluator => this;

        public CodegenExpression EvaluateCodegen(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                EvaluationType, typeof(ExprEvaluatorStreamDTProp), codegenClassScope);

            var refEPS = exprSymbol.GetAddEPS(methodNode);

            methodNode.Block
                .DeclareVar(typeof(EventBean), "event", ArrayAtIndex(refEPS, Constant(streamId)))
                .IfRefNullReturnNull("event")
                .MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        EvaluationType, getter.EventBeanGetCodegen(Ref("event"), methodNode, codegenClassScope)));
            return LocalMethod(methodNode);
        }

        public Type EvaluationType { get; }

        public ExprNodeRenderable ForgeRenderable => this;

        public void ToEPL(StringWriter writer, ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace