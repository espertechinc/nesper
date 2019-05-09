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
        private readonly TypeWidenerSPI widener;
        private readonly ExprNode validated;
        private readonly Type targetType;

        public ExprEvalWithTypeWidener(
            TypeWidenerSPI widener,
            ExprNode validated,
            Type targetType)
        {
            this.widener = widener;
            this.validated = validated;
            this.targetType = targetType;
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
            CodegenExpression inner = validated.Forge.EvaluateCodegen(
                validated.Forge.EvaluationType, codegenMethodScope, exprSymbol, codegenClassScope);
            return widener.WidenCodegen(inner, codegenMethodScope, codegenClassScope);
        }

        public Type EvaluationType {
            get => targetType;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get {
                return new ProxyExprNodeRenderable() {
                    ProcToEPL = (
                        writer,
                        parentPrecedence) => {
                        writer.Write(typeof(ExprEvalWithTypeWidener).Name);
                    },
                };
            }
        }
    }
} // end of namespace