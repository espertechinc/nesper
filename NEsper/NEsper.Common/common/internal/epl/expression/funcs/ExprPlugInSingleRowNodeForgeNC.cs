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
        private readonly ExprDotNodeForgeStaticMethod inner;

        public ExprPlugInSingleRowNodeForgeNC(
            ExprPlugInSingleRowNode parent,
            ExprDotNodeForgeStaticMethod inner)
            : base(parent, false)
        {
            this.inner = inner;
        }

        public override MethodInfo Method => inner.StaticMethod;

        public override ExprEvaluator ExprEvaluator => inner.ExprEvaluator;

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override Type EvaluationType => inner.EvaluationType;

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
            return inner.EvaluateCodegen(requiredType, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public override CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return inner.EventBeanGetCodegen(beanExpression, codegenMethodScope, codegenClassScope);
        }
    }
} // end of namespace