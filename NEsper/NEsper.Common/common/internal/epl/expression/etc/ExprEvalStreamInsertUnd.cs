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
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalStreamInsertUnd : ExprForgeInstrumentable
    {
        private readonly ExprStreamUnderlyingNode undNode;
        private readonly int streamNum;
        private readonly Type returnType;

        public ExprEvalStreamInsertUnd(
            ExprStreamUnderlyingNode undNode,
            int streamNum,
            Type returnType)
        {
            this.undNode = undNode;
            this.streamNum = streamNum;
            this.returnType = returnType;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                this.GetType(),
                this,
                "ExprStreamUndSelectClause",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                typeof(ExprEvalStreamInsertUnd),
                codegenClassScope);

            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .IfCondition(EqualsNull(refEPS))
                .BlockReturn(ConstantNull())
                .MethodReturn(ArrayAtIndex(refEPS, Constant(streamNum)));
            return LocalMethod(methodNode);
        }

        public int StreamNum {
            get => streamNum;
        }

        public ExprEvaluator ExprEvaluator {
            get { throw new IllegalStateException("Evaluator not available"); }
        }

        public Type EvaluationType {
            get => typeof(EventBean);
        }

        public Type UnderlyingReturnType {
            get => returnType;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => undNode;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }
    }
} // end of namespace