///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprForgeWildcard : ExprForge
    {
        public ExprForgeWildcard(Type underlyingTypeStream0)
        {
            EvaluationType = underlyingTypeStream0;
        }

        public ExprEvaluator ExprEvaluator => ExprEvaluatorWildcard.INSTANCE;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprEvaluatorWildcard.Codegen(
                requiredType,
                EvaluationType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public Type EvaluationType { get; }

        public ExprNodeRenderable ExprForgeRenderable => ExprForgeWildcardRenderable.INSTANCE;

        private sealed class ExprForgeWildcardRenderable : ExprNodeRenderable
        {
            internal static readonly ExprForgeWildcardRenderable INSTANCE = new ExprForgeWildcardRenderable();

            private ExprForgeWildcardRenderable()
            {
            }

            public void ToEPL(
                TextWriter writer,
                ExprPrecedenceEnum parentPrecedence,
                ExprNodeRenderableFlags flags)
            {
                writer.Write("underlying-stream-0");
            }
        }
    }
} // end of namespace