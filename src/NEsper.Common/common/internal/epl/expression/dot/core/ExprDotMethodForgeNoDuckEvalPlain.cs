///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.exception;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.model.statement;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotMethodForgeNoDuckEvalPlain : ExprDotEval
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string METHOD_HANDLETARGETEXCEPTION = "handleTargetException";

        private readonly ExprDotMethodForgeNoDuck _forge;
        private readonly ExprEvaluator[] _parameters;

        public ExprDotMethodForgeNoDuck Forge => _forge;

        public ExprDotMethodForgeNoDuckEvalPlain(
            ExprDotMethodForgeNoDuck forge,
            ExprEvaluator[] parameters)
        {
            _forge = forge;
            _parameters = parameters;
        }

        public virtual object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target == null) {
                return null;
            }

            var args = new object[_parameters.Length];
            for (var i = 0; i < args.Length; i++) {
                args[i] = _parameters[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            }

            try {
                return _forge.Method.Invoke(target, args);
            }
            catch (Exception e) when (e is TargetException || e is MemberAccessException) {
                HandleTargetException(
                    _forge.OptionalStatementName,
                    _forge.Method.Name,
                    _forge.Method.GetParameterTypes(),
                    target.GetType().Name,
                    args,
                    e,
                    exprEvaluatorContext);
            }

            return null;
        }

        public virtual EPChainableType TypeInfo => _forge.TypeInfo;

        public ExprDotForge DotForge => _forge;

        public static CodegenExpression CodegenPlain(
            ExprDotMethodForgeNoDuck forge,
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            Type returnType;
            if (forge.GetWrapType() == ExprDotMethodForgeNoDuck.WrapType.WRAPARRAY) {
                returnType = forge.Method.ReturnType;
            }
            else {
                var result = forge.TypeInfo.GetNormalizedType();
                if (result == null) {
                    return ConstantNull();
                }

                returnType = result.GetBoxedType();
            }
            
            var method = forge.Method;

            IList<Type> methodParameters;
            Type instanceType;
            
            if (method.IsExtensionMethod()) {
                methodParameters = method.GetParameterTypes().Skip(1).ToList();
                instanceType = method.GetParameters()[0].ParameterType;
            }
            else {
                methodParameters = method.GetParameterTypes().ToList();
                instanceType = method.DeclaringType;
            }

            var declaringType = forge.Method.DeclaringType;
            var methodNode = codegenMethodScope
                .MakeChild(returnType, typeof(ExprDotMethodForgeNoDuckEvalPlain), codegenClassScope)
                .AddParam(declaringType, "target");

            var block = methodNode.Block;

            if (innerType.CanBeNull() && returnType != typeof(void)) {
                block.IfRefNullReturnNull("target");
            }

            var args = new List<CodegenExpression>(); // [forge.Parameters.Length];
            for (var i = 0; i < forge.Parameters.Length; i++) {
                var name = "p" + i;
                var evaluationType = forge.Parameters[i].EvaluationType;
                if (evaluationType == null) {
                    block.DeclareVar<object>(name, ConstantNull());
                }
                else {
                    block.DeclareVar(
                        evaluationType,
                        name,
                        forge.Parameters[i]
                            .EvaluateCodegen(
                                evaluationType,
                                methodNode,
                                exprSymbol,
                                codegenClassScope));
                }

                CodegenExpression reference = Ref(name);
                if (evaluationType.IsNullable() && !methodParameters[i].IsNullable()) {
                    reference = Unbox(reference);
                }

                args.Add(reference);
            }

            CodegenExpression target = Ref("target");
            if ((instanceType != innerType) && (innerType == instanceType.GetBoxedType())) {
                target = CodegenExpressionBuilder.Unbox(target);
            }
            
            var tryBlock = block.TryCatch();
 
            CodegenExpression invocation;
            if (method.IsExtensionMethod()) {
                args.Insert(0, target);
                invocation = StaticMethod(method.DeclaringType, method.Name, args.ToArray());
            }
            else if (method.IsStatic) {
                if (method.IsSpecialName && method.Name.StartsWith("get_")) {
                    invocation = EnumValue(method.DeclaringType, forge.Method.Name.Substring(4));
                }
                else {
                    invocation = StaticMethod(method.DeclaringType, method.Name, args.ToArray());
                }
            }
            else if (method.IsSpecialName && method.Name.StartsWith("get_")) {
                invocation = ExprDotName(Ref("target"), forge.Method.Name.Substring(4));
            }
            else {
                invocation = ExprDotMethod(Ref("target"), forge.Method.Name, args.ToArray());
            }

            CodegenStatementTryCatch tryCatch;
            if (returnType == typeof(void)) {
                tryCatch = tryBlock.Expression(invocation).TryEnd();
            } else {
                tryCatch = tryBlock.TryReturn(invocation);
            }
            
			var catchBlock = tryCatch.AddCatch(typeof(Exception), "ex");
            catchBlock.DeclareVar<object[]>(
                "args",
                NewArrayByLength(typeof(object), Constant(forge.Parameters.Length)));
            for (var i = 0; i < forge.Parameters.Length; i++) {
                catchBlock.AssignArrayElement("args", Constant(i), args[i]);
            }

            catchBlock.StaticMethod(
                typeof(ExprDotMethodForgeNoDuckEvalPlain),
                METHOD_HANDLETARGETEXCEPTION,
                Constant(forge.OptionalStatementName),
                Constant(method.Name),
                Constant(method.GetParameterTypes()),
                ExprDotMethodChain(Ref("target")).Add("GetType").Get("FullName"),
                Ref("args"),
                Ref("ex"),
                exprSymbol.GetAddExprEvalCtx(methodNode));
            if (returnType.IsTypeVoid()) {
                block.MethodEnd();
            }
            else {
                block.MethodReturn(ConstantNull());
            }

            return LocalMethod(methodNode, inner);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="optionalStatementName">name</param>
        /// <param name="methodName">method name</param>
        /// <param name="methodParams">params</param>
        /// <param name="targetClassName">target class name</param>
        /// <param name="args">args</param>
        /// <param name="ex">exception</param>
        /// <param name="exprEvaluatorContext">expr context</param>
        public static void HandleTargetException(
            string optionalStatementName,
            string methodName,
            Type[] methodParams,
            string targetClassName,
            object[] args,
            Exception ex,
            ExprEvaluatorContext exprEvaluatorContext)
        {
			if (ex is TargetException targetException) {
				ex = targetException.InnerException;
            }

            var message = TypeHelper.GetMessageInvocationTarget(
                optionalStatementName,
                methodName,
                methodParams,
                targetClassName,
                args,
                ex);
            Log.Error(message, ex);
            exprEvaluatorContext.ExceptionHandlingService.HandleException(
                ex,
                exprEvaluatorContext.DeploymentId,
                exprEvaluatorContext.StatementName,
                null,
                ExceptionHandlerExceptionType.PROCESS,
                null);
        }
    }
} // end of namespace