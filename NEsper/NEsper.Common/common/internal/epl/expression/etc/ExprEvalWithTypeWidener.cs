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
            this._widener = widener;
            this._validated = validated;
            this._targetType = targetType;
        }

        public ExprEvaluator ExprEvaluator {
            get { throw new UnsupportedOperationException("Not available at compile time"); }
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression inner = _validated.Forge.EvaluateCodegen(
                _validated.Forge.EvaluationType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
            return _widener.WidenCodegen(inner, codegenMethodScope, codegenClassScope);
        }

        public Type EvaluationType {
            get => _targetType;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get {
                return new ProxyExprNodeRenderable((writer, parentPrecedence, flags) => {
                    writer.Write(typeof(ExprEvalWithTypeWidener).Name);
                });
            }
        }
    }
} // end of namespace