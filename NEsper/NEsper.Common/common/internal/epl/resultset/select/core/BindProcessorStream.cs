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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class BindProcessorStream : ExprForge,
        ExprEvaluator,
        ExprNodeRenderable
    {
        private readonly int streamNum;
        private readonly Type returnType;

        public BindProcessorStream(
            int streamNum,
            Type returnType)
        {
            this.streamNum = streamNum;
            this.returnType = returnType;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = eventsPerStream[streamNum];
            if (theEvent != null) {
                return theEvent.Underlying;
            }

            return null;
        }

        public ExprNodeRenderable ForgeRenderable {
            get => this;
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
            CodegenMethod methodNode = codegenMethodScope.MakeChild(returnType, this.GetType(), codegenClassScope);
            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .DeclareVar(typeof(EventBean), "event", ArrayAtIndex(refEPS, Constant(streamNum)))
                .IfRefNullReturnNull("event")
                .MethodReturn(CodegenLegoCast.CastSafeFromObjectType(returnType, ExprDotMethod(@Ref("event"), "getUnderlying")));
            return LocalMethod(methodNode);
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public Type EvaluationType {
            get => returnType;
        }

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(this.GetType().Name + " stream " + streamNum);
        }
    }
} // end of namespace