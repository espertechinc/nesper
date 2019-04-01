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

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public abstract class ExprLikeNodeForge : ExprForgeInstrumentable
    {
        public ExprLikeNodeForge(ExprLikeNode parent, bool isNumericValue)
        {
            ForgeRenderable = parent;
            IsNumericValue = isNumericValue;
        }

        public ExprLikeNode ForgeRenderable { get; }

        public bool IsNumericValue { get; }

        public abstract ExprEvaluator ExprEvaluator { get; }

        public abstract ExprForgeConstantType ForgeConstantType { get; }

        public abstract CodegenExpression EvaluateCodegen(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        public abstract CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        ExprNodeRenderable ExprForge.ForgeRenderable => ForgeRenderable;

        public Type EvaluationType => typeof(bool?);
    }
} // end of namespace