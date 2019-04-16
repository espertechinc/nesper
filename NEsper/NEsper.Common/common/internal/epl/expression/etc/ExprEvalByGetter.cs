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

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalByGetter : ExprForge,
        ExprEvaluator,
        ExprNodeRenderable
    {
        private readonly EventPropertyGetterSPI getter;
        private readonly int streamNum;

        public ExprEvalByGetter(
            int streamNum,
            EventPropertyGetterSPI getter,
            Type returnType)
        {
            this.streamNum = streamNum;
            this.getter = getter;
            EvaluationType = returnType;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var @event = eventsPerStream[streamNum];
            if (@event == null) {
                return null;
            }

            return getter.Get(@event);
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprEvaluator ExprEvaluator => this;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(EvaluationType, typeof(ExprEvalByGetter), codegenClassScope);

            var refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .DeclareVar(typeof(EventBean), "event", ArrayAtIndex(refEPS, Constant(streamNum)))
                .IfRefNullReturnNull("event")
                .MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        EvaluationType, getter.EventBeanGetCodegen(Ref("event"), methodNode, codegenClassScope)));
            return LocalMethod(methodNode);
        }

        public Type EvaluationType { get; }

        public ExprNodeRenderable ForgeRenderable => this;

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace