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
    public abstract class ExprRegexpNodeForge : ExprForgeInstrumentable
    {
        private readonly ExprRegexpNode _parent;
        private readonly bool _isNumericValue;

        public abstract ExprEvaluator ExprEvaluator { get; }

        public abstract CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        public abstract ExprForgeConstantType ForgeConstantType { get; }

        public abstract CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        public ExprRegexpNodeForge(
            ExprRegexpNode parent,
            bool isNumericValue)
        {
            _parent = parent;
            _isNumericValue = isNumericValue;
        }

        public ExprRegexpNode ForgeRenderable => _parent;

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public bool IsNumericValue => _isNumericValue;

        public Type EvaluationType => typeof(bool?);
    }
} // end of namespace