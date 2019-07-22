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

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalUnderlyingEvaluator : ExprEvaluator,
        ExprForge
    {
        private readonly int streamNum;
        private readonly Type resultType;

        public ExprEvalUnderlyingEvaluator(
            int streamNum,
            Type resultType)
        {
            this.streamNum = streamNum;
            this.resultType = resultType;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (eventsPerStream == null) {
                return null;
            }

            EventBean @event = eventsPerStream[streamNum];
            if (@event == null) {
                return null;
            }

            return @event.Underlying;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
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
            CodegenMethod methodNode = codegenMethodScope.MakeChild(resultType, this.GetType(), codegenClassScope);
            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block.IfRefNullReturnNull(refEPS)
                .DeclareVar<EventBean>("event", ArrayAtIndex(refEPS, Constant(streamNum)))
                .IfRefNullReturnNull("event")
                .MethodReturn(Cast(requiredType, ExprDotMethod(@Ref("event"), "getUnderlying")));
            return LocalMethod(methodNode);
        }

        public Type EvaluationType {
            get => resultType;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get {
                return new ProxyExprNodeRenderable() {
                    ProcToEPL = (
                        writer,
                        parentPrecedence) => {
                        writer.Write(this.GetType().Name);
                    },
                };
            }
        }
    }
} // end of namespace