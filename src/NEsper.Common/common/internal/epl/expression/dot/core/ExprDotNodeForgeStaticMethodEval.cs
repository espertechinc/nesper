///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.StaticMethodCallHelper;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeForgeStaticMethodEval : ExprEvaluator,
        EventPropertyGetter
    {
        public const string METHOD_STATICMETHODEVALHANDLEINVOCATIONEXCEPTION =
            "StaticMethodEvalHandleInvocationException";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ExprDotEval[] _chainEval;
        private readonly ExprEvaluator[] _childEvals;
        private readonly ExprDotNodeForgeStaticMethod _forge;
        public ExprDotNodeForgeStaticMethodEval(
            ExprDotNodeForgeStaticMethod forge,
            ExprEvaluator[] childEvals,
            ExprDotEval[] chainEval)
        {
            _forge = forge;
            _childEvals = childEvals;
            _chainEval = chainEval;
        }

        public object Get(EventBean eventBean)
        {
            var args = new object[_childEvals.Length];
            for (var i = 0; i < args.Length; i++) {
                args[i] = _childEvals[i].Evaluate(new[] {eventBean}, true, null);
            }

            // The method is static so the object it is invoked on
            // can be null
            try {
                return _forge.StaticMethod.Invoke(_forge.TargetObject?.Value, args);
            }
            catch (Exception e) when (e is TargetException || e is MemberAccessException) {
                StaticMethodEvalHandleInvocationException(
                    _forge.OptionalStatementName,
                    _forge.StaticMethod,
                    _forge.ClassOrPropertyName,
                    args,
                    e,
                    _forge.IsRethrowExceptions);
            }

            return null;
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return false;
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var args = new object[_childEvals.Length];
            for (var i = 0; i < args.Length; i++) {
                args[i] = _childEvals[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            }

            // The method is static so the object it is invoked on can be null
            try {
                var target = _forge.TargetObject?.Value;
                var argList = new List<object>(args);
                if (_forge.StaticMethod.IsExtensionMethod()) {
                    argList.Insert(0, target);
                    target = null;
                }
                
                var result = _forge.StaticMethod.Invoke(target, argList.ToArray());

                result = ExprDotNodeUtility.EvaluateChainWithWrap(
                    _forge.ResultWrapLambda,
                    result,
                    null,
                    _forge.StaticMethod.ReturnType,
                    _chainEval,
                    _forge.ChainForges,
                    eventsPerStream,
                    isNewData,
                    exprEvaluatorContext);

                return result;
            }
            catch (TargetInvocationException e) {
                StaticMethodEvalHandleInvocationException(
                    null,
                    _forge.StaticMethod.Name,
                    _forge.StaticMethod.GetParameterTypes(),
                    _forge.ClassOrPropertyName,
                    args,
                    e,
                    _forge.IsRethrowExceptions);
            }
            catch (TargetException e) {
                StaticMethodEvalHandleInvocationException(
                    null,
                    _forge.StaticMethod.Name,
                    _forge.StaticMethod.GetParameterTypes(),
                    _forge.ClassOrPropertyName,
                    args,
                    e,
                    _forge.IsRethrowExceptions);
            }
            return null;
        }

        public static CodegenExpression CodegenExprEval(
            ExprDotNodeForgeStaticMethod forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression isCachedMember = null;
            CodegenExpression cachedResultMember = null;
            if (forge.IsConstantParameters) {
                isCachedMember = codegenClassScope.AddDefaultFieldUnshared(false, typeof(bool), ConstantFalse());
                cachedResultMember = codegenClassScope.AddDefaultFieldUnshared(false, typeof(object), ConstantNull());
            }

            var returnType = forge.StaticMethod.ReturnType;

            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprDotNodeForgeStaticMethodEval),
                codegenClassScope);

            var block = methodNode.Block;

            // check cached
            if (forge.IsConstantParameters) {
                var ifCached = block.IfCondition(isCachedMember);
                if (returnType == typeof(void)) {
                    ifCached.BlockReturnNoValue();
                }
                else {
                    ifCached.BlockReturn(Cast(forge.EvaluationType, cachedResultMember));
                }
            }

            // generate args
            var args = AllArgumentExpressions(
                forge.ChildForges,
                forge.StaticMethod,
                methodNode,
                exprSymbol,
                codegenClassScope);
            AppendArgExpressions(args, methodNode.Block);

            // try block
            var tryBlock = block.TryCatch();
            var invoke = CodegenInvokeExpression(
                forge.TargetObject,
                forge.StaticMethod,
                args,
                codegenClassScope);
            if (returnType == typeof(void)) {
                tryBlock.Expression(invoke);
                if (forge.IsConstantParameters) {
                    tryBlock.AssignRef(isCachedMember, ConstantTrue());
                }

                tryBlock.BlockReturnNoValue();
            }
            else {
                tryBlock.DeclareVar(returnType, "result", invoke);

                if (forge.ChainForges.Length == 0) {
                    var typeInformation = ConstantNull();
                    if (codegenClassScope.IsInstrumented) {
                        typeInformation = codegenClassScope.AddOrGetDefaultFieldSharable(
                            new EPTypeCodegenSharable(new ClassEPType(forge.EvaluationType), codegenClassScope));
                    }

                    tryBlock.Apply(
                        InstrumentationCode.Instblock(
                            codegenClassScope,
                            "qExprDotChain",
                            typeInformation,
                            Ref("result"),
                            Constant(0)));
                    if (forge.IsConstantParameters) {
                        tryBlock.AssignRef(cachedResultMember, Ref("result"));
                        tryBlock.AssignRef(isCachedMember, ConstantTrue());
                    }

                    tryBlock.Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                        .BlockReturn(Ref("result"));
                }
                else {
                    EPType typeInfo;
                    if (forge.ResultWrapLambda != null) {
                        typeInfo = forge.ResultWrapLambda.TypeInfo;
                    }
                    else {
                        typeInfo = new ClassEPType(typeof(object));
                    }

                    var typeInformation = ConstantNull();
                    if (codegenClassScope.IsInstrumented) {
                        typeInformation = codegenClassScope.AddOrGetDefaultFieldSharable(
                            new EPTypeCodegenSharable(typeInfo, codegenClassScope));
                    }

                    tryBlock.Apply(
                            InstrumentationCode.Instblock(
                                codegenClassScope,
                                "qExprDotChain",
                                typeInformation,
                                Ref("result"),
                                Constant(forge.ChainForges.Length)))
                        .DeclareVar(
                            forge.EvaluationType,
                            "chain",
                            ExprDotNodeUtility.EvaluateChainCodegen(
                                methodNode,
                                exprSymbol,
                                codegenClassScope,
                                Ref("result"),
                                returnType,
                                forge.ChainForges,
                                forge.ResultWrapLambda));
                    if (forge.IsConstantParameters) {
                        tryBlock.AssignRef(cachedResultMember, Ref("chain"));
                        tryBlock.AssignRef(isCachedMember, ConstantTrue());
                    }

                    tryBlock.Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                        .BlockReturn(Ref("chain"));
                }
            }

            // exception handling
            AppendCatch(
                tryBlock,
                forge.StaticMethod,
                forge.OptionalStatementName,
                forge.ClassOrPropertyName,
                forge.IsRethrowExceptions,
                args);

            // end method
            if (returnType == typeof(void)) {
                block.MethodEnd();
            }
            else {
                block.MethodReturn(ConstantNull());
            }

            return LocalMethod(methodNode);
        }

        public static CodegenExpression CodegenGet(
            CodegenExpression beanExpression,
            ExprDotNodeForgeStaticMethod forge,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var exprSymbol = new ExprForgeCodegenSymbol(true, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    forge.EvaluationType,
                    typeof(ExprDotNodeForgeStaticMethodEval),
                    exprSymbol,
                    codegenClassScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);

            var args = AllArgumentExpressions(
                forge.ChildForges,
                forge.StaticMethod,
                methodNode,
                exprSymbol,
                codegenClassScope);
            exprSymbol.DerivedSymbolsCodegen(methodNode, methodNode.Block, codegenClassScope);
            AppendArgExpressions(args, methodNode.Block);

            // try block
            var tryBlock = methodNode.Block.TryCatch();
            var invoke = CodegenInvokeExpression(
                forge.TargetObject,
                forge.StaticMethod,
                args,
                codegenClassScope);
            tryBlock.BlockReturn(invoke);

            // exception handling
            AppendCatch(
                tryBlock,
                forge.StaticMethod,
                forge.OptionalStatementName,
                forge.ClassOrPropertyName,
                forge.IsRethrowExceptions,
                args);

            // end method
            methodNode.Block.MethodReturn(ConstantNull());

            return LocalMethod(
                methodNode,
                NewArrayWithInit(typeof(EventBean), beanExpression),
                ConstantTrue(),
                ConstantNull());
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="optionalStatementName">stmt name</param>
        /// <param name="method">method</param>
        /// <param name="classOrPropertyName">target name</param>
        /// <param name="args">args</param>
        /// <param name="thrown">exception</param>
        /// <param name="rethrow">indicator whether to rethrow</param>
        public static void StaticMethodEvalHandleInvocationException(
            string optionalStatementName,
            MethodInfo method,
            string classOrPropertyName,
            object[] args,
            Exception thrown,
            bool rethrow)
        {
            var indication = thrown is TargetException
                ? ((TargetException) thrown).InnerException
                : thrown;
            var message = TypeHelper.GetMessageInvocationTarget(
                optionalStatementName,
                method,
                classOrPropertyName,
                args,
                indication);
            Log.Error(message, indication);
            if (rethrow) {
                throw new EPException(message, indication);
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="optionalStatementName">stmt name</param>
        /// <param name="methodName">name of the method</param>
        /// <param name="parameterTypes">parameters to method</param>
        /// <param name="classOrPropertyName">target name</param>
        /// <param name="args">args</param>
        /// <param name="thrown">exception</param>
        /// <param name="rethrow">indicator whether to rethrow</param>

        public static void StaticMethodEvalHandleInvocationException(
            string optionalStatementName,
            string methodName,
            Type[] parameterTypes,
            String classOrPropertyName,
            Object[] args,
            Exception thrown,
            bool rethrow)
        {
            var indication = thrown is TargetException
                ? ((TargetException) thrown).InnerException
                : thrown;

            var message = TypeHelper.GetMessageInvocationTarget(
                optionalStatementName,
                methodName,
                parameterTypes,
                classOrPropertyName,
                args,
                indication);
            Log.Error(message, indication);
            if (rethrow) {
                throw new EPException(message, indication);
            }
        }
    }
} // end of namespace