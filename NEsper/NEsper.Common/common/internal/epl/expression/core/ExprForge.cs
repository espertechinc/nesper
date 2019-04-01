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

        ExprNodeRenderable ForgeRenderable { get; }
    }

    public class ProxyExprForge : ExprForge
    {
        public Func<ExprEvaluator> ProcExprEvaluator;
        public Func<Type, CodegenMethodScope, ExprForgeCodegenSymbol, CodegenClassScope, CodegenExpression> ProcEvaluateCodegen;
        public Func<Type> ProcEvaluationType;
        public Func<ExprForgeConstantType> ProcForgeConstantType;
        public Func<ExprNodeRenderable> ProcForgeRenderable;

        public ExprEvaluator ExprEvaluator => ProcExprEvaluator?.Invoke();

        public CodegenExpression EvaluateCodegen(
            Type requiredType, 
            CodegenMethodScope codegenMethodScope, 
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
            => ProcEvaluateCodegen?.Invoke(requiredType, codegenMethodScope, exprSymbol, codegenClassScope);

        public Type EvaluationType
            => ProcEvaluationType?.Invoke();
        public ExprForgeConstantType ForgeConstantType
            => ProcForgeConstantType?.Invoke();
        public ExprNodeRenderable ForgeRenderable 
            => ProcForgeRenderable?.Invoke();
    }
} // end of namespace