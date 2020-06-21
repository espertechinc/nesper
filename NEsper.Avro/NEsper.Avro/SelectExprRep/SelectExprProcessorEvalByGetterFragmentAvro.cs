///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

namespace NEsper.Avro.SelectExprRep
{
    public class SelectExprProcessorEvalByGetterFragmentAvro : ExprEvaluator,
        ExprForge,
        ExprNodeRenderable
    {
        private readonly int _streamNum;
        private readonly EventPropertyGetterSPI _getter;
        private readonly Type _returnType;

        public SelectExprProcessorEvalByGetterFragmentAvro(
            int streamNum,
            EventPropertyGetterSPI getter,
            Type returnType)
        {
            _streamNum = streamNum;
            _getter = getter;
            _returnType = returnType;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean streamEvent = eventsPerStream[_streamNum];
            if (streamEvent == null) {
                return null;
            }

            return _getter.Get(streamEvent);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope.MakeChild(_returnType, GetType(), codegenClassScope);

            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>(
                    "streamEvent",
                    CodegenExpressionBuilder.ArrayAtIndex(refEPS, CodegenExpressionBuilder.Constant(_streamNum)))
                .IfRefNullReturnNull("streamEvent")
                .MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        _returnType,
                        _getter.EventBeanGetCodegen(
                            CodegenExpressionBuilder.Ref("streamEvent"),
                            methodNode,
                            codegenClassScope)));
            return CodegenExpressionBuilder.LocalMethod(methodNode);
        }

        public void ToEPL(TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(GetType().Name);
        }

        public ExprEvaluator ExprEvaluator {
            get => this;
        }

        public Type EvaluationType {
            get => _returnType;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public ExprNodeRenderable ForgeRenderable {
            get => this;
        }

        public ExprNodeRenderable ExprForgeRenderable => ForgeRenderable;
    }
} // end of namespace