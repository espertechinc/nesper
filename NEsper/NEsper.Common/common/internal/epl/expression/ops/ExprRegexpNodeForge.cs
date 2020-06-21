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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

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
            this._parent = parent;
            this._isNumericValue = isNumericValue;
        }

        public ExprRegexpNode ForgeRenderable {
            get => _parent;
        }

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public bool IsNumericValue {
            get => _isNumericValue;
        }

        public Type EvaluationType {
            get => typeof(bool?);
        }
    }
} // end of namespace