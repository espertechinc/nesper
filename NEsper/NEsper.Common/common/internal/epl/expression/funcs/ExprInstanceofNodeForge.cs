///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprInstanceofNodeForge : ExprForgeInstrumentable
    {
        internal ExprInstanceofNodeForge(ExprInstanceofNode parent, Type[] classes)
        {
            ForgeRenderableInstance = parent;
            Classes = classes;
        }

        public ExprNodeRenderable ForgeRenderable => ForgeRenderableInstance;

        public ExprInstanceofNode ForgeRenderableInstance { get; }

        public ExprEvaluator ExprEvaluator => new ExprInstanceofNodeForgeEval(
            this, ForgeRenderableInstance.ChildNodes[0].Forge.ExprEvaluator);

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public Type EvaluationType => typeof(bool?);

        public Type[] Classes { get; }

        public CodegenExpression EvaluateCodegen(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(), this, "ExprInstanceof", requiredType, codegenMethodScope, exprSymbol, codegenClassScope)
                .Build();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprInstanceofNodeForgeEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }
    }
} // end of namespace