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
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.analyze;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgeVariable : ExprDotNodeForge
    {
        public ExprDotNodeForgeVariable(
            ExprDotNodeImpl parent,
            VariableMetaData variable,
            ExprDotStaticMethodWrap resultWrapLambda,
            ExprDotForge[] chainForge)
        {
            ExprForgeRenderable = parent;
            Variable = variable;
            ResultWrapLambda = resultWrapLambda;
            ChainForge = chainForge;
        }

        public override ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public override Type EvaluationType {
            get {
                if (ChainForge.Length == 0) {
                    return Variable.Type;
                }

                return EPTypeHelper.GetClassSingleValued(ChainForge[ChainForge.Length - 1].TypeInfo);
            }
        }

        public VariableMetaData Variable { get; }

        public ExprDotStaticMethodWrap ResultWrapLambda { get; }

        public override ExprNodeRenderable ExprForgeRenderable { get; }

        public override bool IsReturnsConstantResult => false;

        public override FilterExprAnalyzerAffector FilterExprAnalyzerAffector => null;

        public override int? StreamNumReferenced => null;

        public override string RootPropertyName => null;

        public ExprDotForge[] ChainForge { get; }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotNodeForgeVariableEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public override CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(), this, "ExprDot", requiredType, codegenMethodScope, exprSymbol, codegenClassScope).Build();
        }
    }
} // end of namespace