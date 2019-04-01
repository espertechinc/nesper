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
using com.espertech.esper.common.@internal.epl.@join.analyze;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public abstract class ExprDotNodeForge : ExprForgeInstrumentable
    {
        public abstract bool IsReturnsConstantResult { get; }

        public abstract FilterExprAnalyzerAffector FilterExprAnalyzerAffector { get; }

        public abstract int? StreamNumReferenced { get; }

        public abstract string RootPropertyName { get; }

        public virtual ExprForgeConstantType ForgeConstantType {
            get {
                if (IsReturnsConstantResult) {
                    return ExprForgeConstantType.DEPLOYCONST;
                }

                return ExprForgeConstantType.NONCONST;
            }
        }

        // --------------------------------------------------------------------------------

        public abstract ExprEvaluator ExprEvaluator { get; }

        public abstract CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        public abstract Type EvaluationType { get; }
        public abstract ExprNodeRenderable ForgeRenderable { get; }

        public abstract CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace