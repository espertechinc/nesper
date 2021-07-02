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

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public interface ExprForge
    {
        ExprEvaluator ExprEvaluator { get; }

        CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        Type EvaluationType { get; }

        ExprForgeConstantType ForgeConstantType { get; }

        ExprNodeRenderable ExprForgeRenderable { get; }
    }

    public class ProxyExprForge : ExprForge
    {
        public Func<ExprEvaluator> procExprEvaluator;

        public Func<Type, CodegenMethodScope, ExprForgeCodegenSymbol, CodegenClassScope, CodegenExpression>
            procEvaluateCodegen;

        public Func<Type> procEvaluationType;
        public Func<ExprForgeConstantType> procForgeConstantType;
        public Func<ExprNodeRenderable> procForgeRenderable;

        public ExprEvaluator ExprEvaluator => procExprEvaluator?.Invoke();

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
            => procEvaluateCodegen?.Invoke(requiredType, codegenMethodScope, exprSymbol, codegenClassScope);

        public Type EvaluationType
            => procEvaluationType?.Invoke();

        public ExprForgeConstantType ForgeConstantType
            => procForgeConstantType?.Invoke();

        public ExprNodeRenderable ExprForgeRenderable
            => procForgeRenderable?.Invoke();
    }
} // end of namespace