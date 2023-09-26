///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgeStaticMethod : ExprDotNodeForge
    {
        private readonly ExprNode parent;
        private readonly bool isReturnsConstantResult;
        private readonly string classOrPropertyName;
        private readonly MethodInfo staticMethod;
        private readonly ExprForge[] childForges;
        private readonly bool isConstantParameters;
        private readonly ExprDotForge[] chainForges;
        private readonly ExprDotStaticMethodWrap resultWrapLambda;
        private readonly bool rethrowExceptions;
        private readonly ValueAndFieldDesc targetObject;
        private readonly string optionalStatementName;
        private readonly bool localInlinedClass;

        public ExprDotNodeForgeStaticMethod(
            ExprNode parent,
            bool isReturnsConstantResult,
            string classOrPropertyName,
            MethodInfo staticMethod,
            ExprForge[] childForges,
            bool isConstantParameters,
            ExprDotForge[] chainForges,
            ExprDotStaticMethodWrap resultWrapLambda,
            bool rethrowExceptions,
            ValueAndFieldDesc targetObject,
            string optionalStatementName,
            bool localInlinedClass)
        {
            this.parent = parent;
            this.isReturnsConstantResult = isReturnsConstantResult;
            this.classOrPropertyName = classOrPropertyName;
            this.staticMethod = staticMethod;
            this.childForges = childForges;
            if (chainForges.Length > 0) {
                this.isConstantParameters = false;
            }
            else {
                this.isConstantParameters = isConstantParameters;
            }

            this.chainForges = chainForges;
            this.resultWrapLambda = resultWrapLambda;
            this.rethrowExceptions = rethrowExceptions;
            this.targetObject = targetObject;
            this.optionalStatementName = optionalStatementName;
            this.localInlinedClass = localInlinedClass;
        }

        public override ExprEvaluator ExprEvaluator {
            get {
                var childEvals = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(childForges);
                return new ExprDotNodeForgeStaticMethodEval(
                    this,
                    childEvals,
                    ExprDotNodeUtility.GetEvaluators(chainForges));
            }
        }

        public override Type EvaluationType {
            get {
                Type type;
                if (chainForges.Length == 0) {
                    type = staticMethod.ReturnType;
                }
                else {
                    var lastInChain = chainForges[^1];
                    var chainableType = lastInChain.TypeInfo;
                    type = chainableType.GetNormalizedType();
                }

                return type.GetBoxedType();
            }
        }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotNodeForgeStaticMethodEval.CodegenExprEval(
                this,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public override CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(),
                this,
                "ExprDot",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotNodeForgeStaticMethodEval.CodegenGet(
                beanExpression,
                this,
                codegenMethodScope,
                codegenClassScope);
        }

        public string ClassOrPropertyName => classOrPropertyName;

        public MethodInfo StaticMethod => staticMethod;

        public ExprForge[] ChildForges => childForges;

        public bool IsConstantParameters => isConstantParameters;

        public ExprDotForge[] ChainForges => chainForges;

        public ExprDotStaticMethodWrap ResultWrapLambda => resultWrapLambda;

        public bool IsRethrowExceptions => rethrowExceptions;

        public ValueAndFieldDesc TargetObject => targetObject;

        public override ExprNodeRenderable ExprForgeRenderable => parent;

        public override bool IsReturnsConstantResult => isReturnsConstantResult;

        public override FilterExprAnalyzerAffector FilterExprAnalyzerAffector => null;

        public override int? StreamNumReferenced => null;

        public override string RootPropertyName => null;

        public string OptionalStatementName => optionalStatementName;

        public override bool IsLocalInlinedClass => localInlinedClass;
    }
} // end of namespace