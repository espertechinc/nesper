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
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotMethodForgeDuckEval : ExprDotEval
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDictionary<Type, MethodInfo> cache;

        private readonly ExprDotMethodForgeDuck forge;
        private readonly ExprEvaluator[] parameters;

        internal ExprDotMethodForgeDuckEval(
            ExprDotMethodForgeDuck forge,
            ExprEvaluator[] parameters)
        {
            this.forge = forge;
            this.parameters = parameters;
            cache = new Dictionary<Type, MethodInfo>();
        }

        public EPType TypeInfo => forge.TypeInfo;

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target == null) {
                return null;
            }

            var method = DotMethodDuckGetMethod(
                target.GetType(),
                cache,
                forge.MethodName,
                forge.ParameterTypes,
                new bool[forge.Parameters.Length]);
            if (method == null) {
                return null;
            }

            var args = new object[parameters.Length];
            for (var i = 0; i < args.Length; i++) {
                args[i] = parameters[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            }

            return DotMethodDuckInvokeMethod(method, target, args, forge.StatementName);
        }

        public ExprDotForge DotForge => forge;

        public static CodegenExpression Codegen(
            ExprDotMethodForgeDuck forge,
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression mCache = codegenClassScope.AddFieldUnshared<IDictionary<Type, MethodInfo>>(
                true,
                NewInstance(typeof(Dictionary<Type, MethodInfo>)));
            var methodNode = codegenMethodScope
                .MakeChild(typeof(object), typeof(ExprDotMethodForgeDuckEval), codegenClassScope)
                .AddParam(innerType, "target");

            var block = methodNode.Block
                .IfRefNullReturnNull("target")
                .DeclareVar<MethodInfo>(
                    "method",
                    StaticMethod(
                        typeof(ExprDotMethodForgeDuckEval),
                        "DotMethodDuckGetMethod",
                        ExprDotMethod(Ref("target"), "GetType"),
                        mCache,
                        Constant(forge.MethodName),
                        Constant(forge.ParameterTypes),
                        Constant(new bool[forge.ParameterTypes.Length])))
                .IfRefNullReturnNull("method")
                .DeclareVar<object[]>(
                    "args",
                    NewArrayByLength(typeof(object), Constant(forge.Parameters.Length)));
            for (var i = 0; i < forge.Parameters.Length; i++) {
                block.AssignArrayElement(
                    "args",
                    Constant(i),
                    forge.Parameters[i].EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope));
            }

            var statementName = ExprDotName(exprSymbol.GetAddExprEvalCtx(methodNode), "StatementName");
            block.MethodReturn(
                StaticMethod(
                    typeof(ExprDotMethodForgeDuckEval),
                    "DotMethodDuckInvokeMethod",
                    Ref("method"),
                    Ref("target"),
                    Ref("args"),
                    statementName));
            return LocalMethod(methodNode, inner);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="targetClass">clazz</param>
        /// <param name="cache">cache</param>
        /// <param name="methodName">name</param>
        /// <param name="paramTypes">params</param>
        /// <param name="allFalse">all-false boolean same size as params</param>
        /// <returns>method</returns>
        public static MethodInfo DotMethodDuckGetMethod(
            Type targetClass,
            IDictionary<Type, MethodInfo> cache,
            string methodName,
            Type[] paramTypes,
            bool[] allFalse)
        {
            MethodInfo method;
            lock (cache) {
                if (cache.ContainsKey(targetClass)) {
                    method = cache.Get(targetClass);
                }
                else {
                    method = GetMethod(targetClass, methodName, paramTypes, allFalse);
                    cache.Put(targetClass, method);
                }
            }

            return method;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="method">method</param>
        /// <param name="target">target</param>
        /// <param name="args">args</param>
        /// <param name="statementName">statementName</param>
        /// <returns>result</returns>
        public static object DotMethodDuckInvokeMethod(
            MethodInfo method,
            object target,
            object[] args,
            string statementName)
        {
            try {
                return method.Invoke(target, args);
            }
            catch (Exception e) when (e is TargetException || e is MemberAccessException) {
                var message = TypeHelper.GetMessageInvocationTarget(
                    statementName,
                    method,
                    target.GetType().CleanName(),
                    args,
                    e);
                Log.Error(message, e);
            }

            return null;
        }

        private static MethodInfo GetMethod(
            Type clazz,
            string methodName,
            Type[] paramTypes,
            bool[] allFalse)
        {
            try {
                return MethodResolver.ResolveMethod(clazz, methodName, paramTypes, true, allFalse, allFalse);
            }
            catch (Exception) {
                Log.Debug("Not resolved for class '" + clazz.Name + "' method '" + methodName + "'");
            }

            return null;
        }
    }
} // end of namespace