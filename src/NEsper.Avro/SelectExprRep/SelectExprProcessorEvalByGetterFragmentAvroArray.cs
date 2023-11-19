///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace NEsper.Avro.SelectExprRep
{
    public class SelectExprProcessorEvalByGetterFragmentAvroArray : ExprEvaluator,
        ExprForge,
        ExprNodeRenderable
    {
        private readonly int _streamNum;
        private readonly EventPropertyGetterSPI _getter;
        private readonly Type _returnType;

        public SelectExprProcessorEvalByGetterFragmentAvroArray(
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
            EventBean @event = eventsPerStream[_streamNum];
            if (@event == null) {
                return null;
            }

            object result = _getter.Get(@event);
            if (result != null && result.GetType().IsArray) {
                return Arrays.AsList((object[]) result);
            }

            return null;
        }

        public ExprEvaluator ExprEvaluator {
            get => this;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(ICollection<object>),
                GetType(),
                codegenClassScope);
            CodegenExpressionRef refEPS = exprSymbol.GetAddEps(methodNode);

            methodNode.Block
                .DeclareVar<EventBean>(
                    "@event",
                    CodegenExpressionBuilder.ArrayAtIndex(refEPS, CodegenExpressionBuilder.Constant(_streamNum)))
                .IfRefNullReturnNull("@event")
                .DeclareVar<object[]>(
                    "result",
                    CodegenExpressionBuilder.Cast(
                        typeof(object[]),
                        _getter.EventBeanGetCodegen(
                            CodegenExpressionBuilder.Ref("@event"),
                            methodNode,
                            codegenClassScope)))
                .IfRefNullReturnNull("result")
                .MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(CompatExtensions),
                        "AsList",
                        CodegenExpressionBuilder.Ref("result")));
            return CodegenExpressionBuilder.LocalMethod(methodNode);
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

        public void ToEPL(TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace