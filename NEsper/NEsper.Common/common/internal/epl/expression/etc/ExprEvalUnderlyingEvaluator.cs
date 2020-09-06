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
        private readonly int _streamNum;
        private readonly Type _resultType;

        public ExprEvalUnderlyingEvaluator(
            int streamNum,
            Type resultType)
        {
            this._streamNum = streamNum;
            this._resultType = resultType;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            EventBean @event = eventsPerStream?[_streamNum];

            return @event?.Underlying;
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
            CodegenMethod methodNode = codegenMethodScope.MakeChild(_resultType, this.GetType(), codegenClassScope);
            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .IfNullReturnNull(refEPS)
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(_streamNum)))
                .IfRefNullReturnNull("@event")
                .MethodReturn(FlexCast(requiredType, ExprDotName(Ref("@event"), "Underlying")));
            return LocalMethod(methodNode);
        }

        public Type EvaluationType {
            get => _resultType;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get {
                return new ProxyExprNodeRenderable((writer, parentPrecedence, flags) => {
                    writer.Write(this.GetType().Name);
                });
            }
        }
    }
} // end of namespace