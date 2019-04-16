///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprConcatNodeForge : ExprForgeInstrumentable
    {
        private ExprConcatNode _parent;
        private readonly ThreadingProfile _threadingProfile;

        public ExprConcatNodeForge(
            ExprConcatNode parent,
            ThreadingProfile threadingProfile)
        {
            _parent = parent;
            _threadingProfile = threadingProfile;
        }

        public ExprConcatNode ForgeRenderable => _parent;

        ExprNodeRenderable ExprForge.ForgeRenderable => ForgeRenderable;

        public ExprEvaluator ExprEvaluator {
            get {
                var evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(ForgeRenderable.ChildNodes);
                if (_threadingProfile == ThreadingProfile.LARGE) {
                    return new ExprConcatNodeForgeEvalWNew(this, evaluators);
                }

                return new ExprConcatNodeForgeEvalThreadLocal(this, evaluators);
            }
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public Type EvaluationType => typeof(string);

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(), this, "ExprConcat", requiredType, codegenMethodScope, exprSymbol, codegenClassScope).Build();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprConcatNodeForgeEvalWNew.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }
    }
} // end of namespace