///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprPlugInSingleRowNodeForgeNC : ExprPlugInSingleRowNodeForge
    {
        private readonly ExprDotNodeForgeStaticMethod _inner;

        public ExprPlugInSingleRowNodeForgeNC(
            ExprPlugInSingleRowNode parent,
            ExprDotNodeForgeStaticMethod inner)
            : base(parent, false)
        {
            this._inner = inner;
        }

        public override MethodInfo Method => _inner.StaticMethod;

        public override ExprEvaluator ExprEvaluator => _inner.ExprEvaluator;

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override Type EvaluationType => _inner.EvaluationType;

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

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return _inner.EvaluateCodegen(requiredType, codegenMethodScope, exprSymbol, codegenClassScope);
        }
        
        public CodegenExpression EventBeanWithCtxGet(
            CodegenExpression beanExpression,
            CodegenExpression ctxExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return _inner.EventBeanGetCodegen(beanExpression, codegenMethodScope, codegenClassScope);
        }

        public override bool IsLocalInlinedClass => _inner.IsLocalInlinedClass;
    }
} // end of namespace