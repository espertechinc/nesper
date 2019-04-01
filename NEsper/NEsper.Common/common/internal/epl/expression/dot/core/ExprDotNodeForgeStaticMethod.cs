///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.analyze;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgeStaticMethod : ExprDotNodeForge
    {
        public ExprDotNodeForgeStaticMethod(
            ExprNode parent, bool isReturnsConstantResult, string classOrPropertyName, MethodInfo staticMethod,
            ExprForge[] childForges, bool isConstantParameters,
            ExprDotForge[] chainForges,
            ExprDotStaticMethodWrap resultWrapLambda, bool rethrowExceptions, object targetObject,
            string optionalStatementName)
        {
            ForgeRenderable = parent;
            IsReturnsConstantResult = isReturnsConstantResult;
            ClassOrPropertyName = classOrPropertyName;
            StaticMethod = staticMethod;
            ChildForges = childForges;
            if (chainForges.Length > 0) {
                IsConstantParameters = false;
            }
            else {
                IsConstantParameters = isConstantParameters;
            }

            ChainForges = chainForges;
            ResultWrapLambda = resultWrapLambda;
            IsRethrowExceptions = rethrowExceptions;
            TargetObject = targetObject;
            OptionalStatementName = optionalStatementName;
        }

        public string ClassOrPropertyName { get; }

        public MethodInfo StaticMethod { get; }

        public ExprForge[] ChildForges { get; }

        public bool IsConstantParameters { get; }

        public ExprDotForge[] ChainForges { get; }

        public ExprDotStaticMethodWrap ResultWrapLambda { get; }

        public object TargetObject { get; }

        public override ExprNodeRenderable ForgeRenderable { get; }

        public override bool IsReturnsConstantResult { get; }

        public override FilterExprAnalyzerAffector FilterExprAnalyzerAffector => null;

        public override int? StreamNumReferenced => null;

        public override string RootPropertyName => null;

        public string OptionalStatementName { get; }

        public override ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public override Type EvaluationType {
            get {
                if (ChainForges.Length == 0) {
                    return StaticMethod.ReturnType.GetBoxedType();
                }

                return ChainForges[ChainForges.Length - 1].TypeInfo.GetNormalizedClass();
            }
        }

        public bool IsRethrowExceptions { get; }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotNodeForgeStaticMethodEval.CodegenExprEval(
                this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public override CodegenExpression EvaluateCodegen(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(), this, "ExprDot", requiredType, codegenMethodScope, exprSymbol, codegenClassScope).Build();
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotNodeForgeStaticMethodEval.CodegenGet(
                beanExpression, this, codegenMethodScope, codegenClassScope);
        }
    }
} // end of namespace