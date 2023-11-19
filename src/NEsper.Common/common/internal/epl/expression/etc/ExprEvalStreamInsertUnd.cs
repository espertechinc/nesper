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
        private readonly ExprStreamUnderlyingNode _undNode;
        private readonly int _streamNum;
        private readonly Type _returnType;

        public ExprEvalStreamInsertUnd(
            ExprStreamUnderlyingNode undNode,
            int streamNum,
            Type returnType)
        {
            _undNode = undNode;
            _streamNum = streamNum;
            _returnType = returnType;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(),
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
            var methodNode = codegenMethodScope.MakeChild(
                _returnType,
                typeof(ExprEvalStreamInsertUnd),
                codegenClassScope);

            var refEPS = exprSymbol.GetAddEps(methodNode);
            methodNode.Block
                .IfCondition(EqualsNull(refEPS))
                .BlockReturn(ConstantNull())
                .DeclareVar<EventBean>("bean", ArrayAtIndex(refEPS, Constant(_streamNum)))
                .IfRefNullReturnNull("bean")
                .MethodReturn(FlexCast(_returnType, ExprDotUnderlying(Ref("bean"))));

            return LocalMethod(methodNode);
        }

        public int StreamNum => _streamNum;

        public ExprEvaluator ExprEvaluator => throw new IllegalStateException("Evaluator not available");

        public Type EvaluationType => _returnType;

        public Type UnderlyingReturnType => _returnType;

        public ExprNodeRenderable ExprForgeRenderable => _undNode;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;
    }
} // end of namespace