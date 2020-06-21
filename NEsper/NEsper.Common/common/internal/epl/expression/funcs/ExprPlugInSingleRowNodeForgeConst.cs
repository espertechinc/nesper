///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprPlugInSingleRowNodeForgeConst : ExprPlugInSingleRowNodeForge,
        ExprEvaluator
    {
        private readonly ExprDotNodeForgeStaticMethod _inner;

        public ExprPlugInSingleRowNodeForgeConst(
            ExprPlugInSingleRowNode parent,
            ExprDotNodeForgeStaticMethod inner)
            : base(parent, true)
        {
            _inner = inner;
        }

        public override MethodInfo Method => _inner.StaticMethod;

        public override ExprEvaluator ExprEvaluator => this;

        public override Type EvaluationType => _inner.StaticMethod.ReturnType;

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.DEPLOYCONST;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (EvaluationType == typeof(void)) {
                return Noop();
            }

            var initMethod = codegenClassScope.NamespaceScope.InitMethod;
            var evaluate = CodegenLegoMethodExpression.CodegenExpression(_inner, initMethod, codegenClassScope, true);
            return codegenClassScope.AddDefaultFieldUnshared(
                true,
                EvaluationType,
                LocalMethod(evaluate, ConstantNull(), ConstantTrue(), ConstantNull()));
        }

        public override CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(),
                    this,
                    "ExprPlugInSingleRow",
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope)
                .Qparams(MethodAsParams)
                .Build();
        }

        public override CodegenExpression EventBeanWithCtxGet(
            CodegenExpression beanExpression,
            CodegenExpression ctxExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public override bool IsLocalInlinedClass => _inner.IsLocalInlinedClass;
    }
} // end of namespace