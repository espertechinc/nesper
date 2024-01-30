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
        private readonly ExprNode _parent;
        private readonly bool _isReturnsConstantResult;
        private readonly string _classOrPropertyName;
        private readonly MethodInfo _staticMethod;
        private readonly ExprForge[] _childForges;
        private readonly bool _isConstantParameters;
        private readonly ExprDotForge[] _chainForges;
        private readonly ExprDotStaticMethodWrap _resultWrapLambda;
        private readonly bool _rethrowExceptions;
        private readonly ValueAndFieldDesc _targetObject;
        private readonly string _optionalStatementName;
        private readonly bool _localInlinedClass;

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
            _parent = parent;
            _isReturnsConstantResult = isReturnsConstantResult;
            _classOrPropertyName = classOrPropertyName;
            _staticMethod = staticMethod;
            _childForges = childForges;
            if (chainForges.Length > 0) {
                _isConstantParameters = false;
            }
            else {
                _isConstantParameters = isConstantParameters;
            }

            _chainForges = chainForges;
            _resultWrapLambda = resultWrapLambda;
            _rethrowExceptions = rethrowExceptions;
            _targetObject = targetObject;
            _optionalStatementName = optionalStatementName;
            _localInlinedClass = localInlinedClass;
        }

        public override ExprEvaluator ExprEvaluator {
            get {
                var childEvals = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(_childForges);
                return new ExprDotNodeForgeStaticMethodEval(
                    this,
                    childEvals,
                    ExprDotNodeUtility.GetEvaluators(_chainForges));
            }
        }

        public override Type EvaluationType {
            get {
                Type type;
                if (_chainForges.Length == 0) {
                    type = _staticMethod.ReturnType;
                }
                else {
                    var lastInChain = _chainForges[^1];
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

        public string ClassOrPropertyName => _classOrPropertyName;

        public MethodInfo StaticMethod => _staticMethod;

        public ExprForge[] ChildForges => _childForges;

        public bool IsConstantParameters => _isConstantParameters;

        public ExprDotForge[] ChainForges => _chainForges;

        public ExprDotStaticMethodWrap ResultWrapLambda => _resultWrapLambda;

        public bool IsRethrowExceptions => _rethrowExceptions;

        public ValueAndFieldDesc TargetObject => _targetObject;

        public override ExprNodeRenderable ExprForgeRenderable => _parent;

        public override bool IsReturnsConstantResult => _isReturnsConstantResult;

        public override FilterExprAnalyzerAffector FilterExprAnalyzerAffector => null;

        public override int? StreamNumReferenced => null;

        public override string RootPropertyName => null;

        public string OptionalStatementName => _optionalStatementName;

        public override bool IsLocalInlinedClass => _localInlinedClass;
    }
} // end of namespace