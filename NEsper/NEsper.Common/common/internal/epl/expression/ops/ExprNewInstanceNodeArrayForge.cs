///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprNewInstanceNodeArrayForge : ExprForgeInstrumentable
    {
        public ExprNewInstanceNodeArrayForge(
            ExprNewInstanceNode parent,
            Type targetClass,
            Type targetClassArrayed)
        {
            Parent = parent;
            TargetClass = targetClass;
            TargetClassArrayed = targetClassArrayed;
        }

        public ExprNodeRenderable ExprForgeRenderable => Parent;

        public ExprNewInstanceNode Parent { get; }

        public Type TargetClass { get; }

        public Type TargetClassArrayed { get; }

        public ExprEvaluator ExprEvaluator => new ExprNewInstanceNodeArrayForgeEval(this);

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(GetType(), this, "ExprNewInstance", requiredType, codegenMethodScope, exprSymbol, codegenClassScope).Build();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprNewInstanceNodeArrayForgeEval.EvaluateCodegen(requiredType, this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public Type EvaluationType => TargetClassArrayed;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;
    }
} // end of namespace