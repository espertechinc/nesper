///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const string METHOD_STATICMETHODEVALHANDLEINVOCATIONEXCEPTION =
            "staticMethodEvalHandleInvocationException";

        private readonly ExprDotNodeForgeStaticMethod forge;
        private readonly ExprEvaluator[] childEvals;
        private readonly ExprDotEval[] chainEval;

        private bool isCachedResult;
        private object cachedResult;

        public ExprDotNodeForgeStaticMethodEval(
            ExprDotNodeForgeStaticMethod forge,
            ExprEvaluator[] childEvals,
            ExprDotEval[] chainEval)
        {
            this.forge = forge;
            this.childEvals = childEvals;
            this.chainEval = chainEval;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
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
                isCachedMember = codegenClassScope.AddFieldUnshared(false, typeof(bool), ConstantFalse());
                cachedResultMember = codegenClassScope.AddFieldUnshared(false, typeof(object), ConstantNull());
            }

            Type returnType = forge.StaticMethod.ReturnType;

            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType, typeof(ExprDotNodeForgeStaticMethodEval), codegenClassScope);

            CodegenBlock block = methodNode.Block;

            // check cached
            if (forge.IsConstantParameters) {
                CodegenBlock ifCached = block.IfCondition(isCachedMember);
                if (returnType == typeof(void)) {
                    ifCached.BlockReturnNoValue();
                }
                else {
                    ifCached.BlockReturn(Cast(forge.EvaluationType, cachedResultMember));
                }
            }

            // generate args
            StaticMethodCodegenArgDesc[] args = AllArgumentExpressions(
                forge.ChildForges, forge.StaticMethod, methodNode, exprSymbol, codegenClassScope);
            AppendArgExpressions(args, methodNode.Block);

            // try block
            CodegenBlock tryBlock = block.TryCatch();
            CodegenExpression invoke = CodegenInvokeExpression(
                forge.TargetObject, forge.StaticMethod, args, codegenClassScope);
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
                    CodegenExpression typeInformation = ConstantNull();
                    if (codegenClassScope.IsInstrumented) {
                        typeInformation = codegenClassScope.AddOrGetFieldSharable(
                            new EPTypeCodegenSharable(new ClassEPType(forge.EvaluationType), codegenClassScope));
                    }

                    tryBlock.Apply(
                        InstrumentationCode.Instblock(
                            codegenClassScope, "qExprDotChain", typeInformation, @Ref("result"), Constant(0)));
                    if (forge.IsConstantParameters) {
                        tryBlock.AssignRef(cachedResultMember, @Ref("result"));
                        tryBlock.AssignRef(isCachedMember, ConstantTrue());
                    }

                    tryBlock.Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                        .BlockReturn(@Ref("result"));
                }
                else {
                    EPType typeInfo;
                    if (forge.ResultWrapLambda != null) {
                        typeInfo = forge.ResultWrapLambda.TypeInfo;
                    }
                    else {
                        typeInfo = new ClassEPType(typeof(object));
                    }

                    CodegenExpression typeInformation = ConstantNull();
                    if (codegenClassScope.IsInstrumented) {
                        typeInformation = codegenClassScope.AddOrGetFieldSharable(
                            new EPTypeCodegenSharable(typeInfo, codegenClassScope));
                    }

                    tryBlock.Apply(
                            InstrumentationCode.Instblock(
                                codegenClassScope, "qExprDotChain", typeInformation, @Ref("result"),
                                Constant(forge.ChainForges.Length)))
                        .DeclareVar(
                            forge.EvaluationType, "chain",
                            ExprDotNodeUtility.EvaluateChainCodegen(
                                methodNode, exprSymbol, codegenClassScope, @Ref("result"), returnType,
                                forge.ChainForges, forge.ResultWrapLambda));
                    if (forge.IsConstantParameters) {
                        tryBlock.AssignRef(cachedResultMember, @Ref("chain"));
                        tryBlock.AssignRef(isCachedMember, ConstantTrue());
                    }

                    tryBlock.Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprDotChain"))
                        .BlockReturn(@Ref("chain"));
                }
            }

            // exception handling
            AppendCatch(
                tryBlock, forge.StaticMethod, forge.OptionalStatementName, forge.ClassOrPropertyName,
                forge.IsRethrowExceptions, args);

            // end method
            if (returnType == typeof(void)) {
                block.MethodEnd();
            }
            else {
                block.MethodReturn(ConstantNull());
            }

            return LocalMethod(methodNode);
        }

        public object Get(EventBean eventBean)
        {
            object[] args = new object[childEvals.Length];
            for (int i = 0; i < args.Length; i++) {
                args[i] = childEvals[i].Evaluate(new EventBean[] {eventBean}, true, null);
            }

            // The method is static so the object it is invoked on
            // can be null
            try {
                return forge.StaticMethod.Invoke(forge.TargetObject, args);
            }
            catch (Exception e) when (e is TargetException || e is MemberAccessException) {
                StaticMethodEvalHandleInvocationException(
                    forge.OptionalStatementName, forge.StaticMethod.Name,
                    forge.StaticMethod.GetParameterTypes(),
                    forge.ClassOrPropertyName, args, e, forge.IsRethrowExceptions);
            }

            return null;
        }

        public static CodegenExpression CodegenGet(
            CodegenExpression beanExpression,
            ExprDotNodeForgeStaticMethod forge,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            ExprForgeCodegenSymbol exprSymbol = new ExprForgeCodegenSymbol(true, null);
            CodegenMethod methodNode = codegenMethodScope.MakeChildWithScope(
                    forge.EvaluationType, typeof(ExprDotNodeForgeStaticMethodEval), exprSymbol, codegenClassScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);

            StaticMethodCodegenArgDesc[] args = AllArgumentExpressions(
                forge.ChildForges, forge.StaticMethod, methodNode, exprSymbol, codegenClassScope);
            exprSymbol.DerivedSymbolsCodegen(methodNode, methodNode.Block, codegenClassScope);
            AppendArgExpressions(args, methodNode.Block);

            // try block
            CodegenBlock tryBlock = methodNode.Block.TryCatch();
            CodegenExpression invoke = CodegenInvokeExpression(
                forge.TargetObject, forge.StaticMethod, args, codegenClassScope);
            tryBlock.BlockReturn(invoke);

            // exception handling
            AppendCatch(
                tryBlock, forge.StaticMethod, forge.OptionalStatementName, forge.ClassOrPropertyName,
                forge.IsRethrowExceptions, args);

            // end method
            methodNode.Block.MethodReturn(ConstantNull());

            return LocalMethod(
                methodNode, NewArrayWithInit(typeof(EventBean), beanExpression), ConstantTrue(), ConstantNull());
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return false;
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="optionalStatementName">stmt name</param>
        /// <param name="methodName">methodName</param>
        /// <param name="parameterTypes">param types</param>
        /// <param name="classOrPropertyName">target name</param>
        /// <param name="args">args</param>
        /// <param name="thrown">exception</param>
        /// <param name="rethrow">indicator whether to rethrow</param>
        public static void StaticMethodEvalHandleInvocationException(
            string optionalStatementName,
            string methodName,
            Type[] parameterTypes,
            string classOrPropertyName,
            object[] args,
            Exception thrown,
            bool rethrow)
        {
            var indication = thrown is TargetException
                ? ((TargetException) thrown).TargetException
                : thrown;
            string message = TypeHelper.GetMessageInvocationTarget(
                optionalStatementName, methodName, parameterTypes, classOrPropertyName, args, indication);
            Log.Error(message, indication);
            if (rethrow) {
                throw new EPException(message, indication);
            }
        }
    }
} // end of namespace