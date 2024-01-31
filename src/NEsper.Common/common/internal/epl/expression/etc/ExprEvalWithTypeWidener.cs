///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalWithTypeWidener : ExprForge
    {
        private readonly TypeWidenerSPI _widener;
        private readonly ExprNode _validated;
        private readonly Type _targetType;

        public ExprEvalWithTypeWidener(
            TypeWidenerSPI widener,
            ExprNode validated,
            Type targetType)
        {
            _widener = widener;
            _validated = validated;
            _targetType = targetType;
        }

        public ExprEvaluator ExprEvaluator => throw new UnsupportedOperationException("Not available at compile time");

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var inner = _validated.Forge.EvaluateCodegen(
                _validated.Forge.EvaluationType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
            return _widener.WidenCodegen(inner, codegenMethodScope, codegenClassScope);
        }

        public Type EvaluationType => _targetType;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprNodeRenderable ExprForgeRenderable {
            get {
                return new ProxyExprNodeRenderable(
                    (
                        writer,
                        parentPrecedence,
                        flags) => {
                        writer.Write(nameof(ExprEvalWithTypeWidener));
                    });
            }
        }
    }
} // end of namespace