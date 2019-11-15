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
        public const string METHOD_HANDLETARGETEXCEPTION = "HandleTargetException";
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal readonly ExprDotMethodForgeNoDuck forge;
        private readonly ExprEvaluator[] parameters;

        internal ExprDotMethodForgeNoDuckEvalPlain(
            ExprDotMethodForgeNoDuck forge,
            ExprEvaluator[] parameters)
        {
            this.forge = forge;
            this.parameters = parameters;
        }

        public virtual EPType TypeInfo => forge.TypeInfo;

        public virtual object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target == null) {
                return null;
            }

            var args = new object[parameters.Length];
            for (var i = 0; i < args.Length; i++) {
                args[i] = parameters[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            }

            try {
                return forge.Method.Invoke(target, args);
            }
            catch (Exception e) when (e is TargetException || e is MemberAccessException) {
                HandleTargetException(
                    forge.OptionalStatementName,
                    forge.Method,
                    target.GetType().FullName,
                    args,
                    e);
            }

            return null;
        }

        public ExprDotForge DotForge => forge;

        public static CodegenExpression CodegenPlain(
            ExprDotMethodForgeNoDuck forge,
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var returnType = forge.Method.ReturnType.GetBoxedType();
            var methodNode = codegenMethodScope
                .MakeChild(returnType, typeof(ExprDotMethodForgeNoDuckEvalPlain), codegenClassScope)
                .AddParam(forge.Method.DeclaringType, "target");

            var block = methodNode.Block;

            if (!innerType.IsPrimitive && returnType != typeof(void)) {
                block.IfRefNullReturnNull("target");
            }

            var args = new CodegenExpression[forge.Parameters.Length];
            for (var i = 0; i < forge.Parameters.Length; i++) {
                var name = "p" + i;
                var evaluationType = forge.Parameters[i].EvaluationType;
                block.DeclareVar(
                    evaluationType,
                    name,
                    forge.Parameters[i].EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope));
                args[i] = Ref(name);
            }

            var tryBlock = block.TryCatch();
            var invocation = ExprDotMethod(Ref("target"), forge.Method.Name, args);
         
            CodegenStatementTryCatch tryCatch;
            if (returnType == typeof(void)) {
                tryCatch = tryBlock.Expression(invocation).TryEnd();
            }
            else {
                tryCatch = tryBlock.TryReturn(invocation);
            }

            var catchBlock = tryCatch.AddCatch(typeof(Exception), "t");
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
                Constant(forge.Method.Name),
                Constant(forge.Method.GetParameterTypes()),
                ExprDotMethodChain(Ref("target")).Add("GetType").Get("FullName"),
                Ref("args"),
                Ref("t"));
            if (returnType == typeof(void)) {
                block.MethodEnd();
            }
            else {
                block.MethodReturn(ConstantNull());
            }

            return LocalMethod(methodNode, inner);
        }

        public static void HandleTargetException(
            string optionalStatementName,
            MethodInfo method,
            string targetClassName,
            object[] args,
            Exception t)
        {
            if (t is TargetException) {
                t = ((TargetException) t).InnerException;
            }

            var message = TypeHelper.GetMessageInvocationTarget(
                optionalStatementName,
                method,
                targetClassName,
                args,
                t);
            Log.Error(message, t);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="optionalStatementName">name</param>
        /// <param name="methodName">method name</param>
        /// <param name="methodParams">method parameters (types only)</param>
        /// <param name="targetClassName">target class name</param>
        /// <param name="args">args</param>
        /// <param name="t">throwable</param>
        public static void HandleTargetException(
            string optionalStatementName,
            string methodName,
            Type[] methodParams,
            string targetClassName,
            object[] args,
            Exception t)
        {
            if (t is TargetException)
            {
                t = ((TargetException) t).InnerException;
            }

            var message = TypeHelper.GetMessageInvocationTarget(
                optionalStatementName,
                methodName,
                methodParams,
                targetClassName,
                args,
                t);
            Log.Error(message, t);
        }


    }
} // end of namespace