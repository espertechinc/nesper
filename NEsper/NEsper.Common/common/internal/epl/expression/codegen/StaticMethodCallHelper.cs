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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.dot.core.ExprDotNodeForgeStaticMethodEval;

namespace com.espertech.esper.common.@internal.epl.expression.codegen
{
    public class StaticMethodCallHelper
    {
        public static StaticMethodCodegenArgDesc[] AllArgumentExpressions(
            ExprForge[] forges,
            MethodInfo method,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var parameterTypes = method.GetParameterTypes();
            var args = new StaticMethodCodegenArgDesc[forges.Length];
            for (var i = 0; i < forges.Length; i++) {
                var child = forges[i];
                var childType = child.EvaluationType;
                var name = "r" + i;
                if (childType == null) {
                    args[i] = new StaticMethodCodegenArgDesc(name, parameterTypes[i], ConstantNull());
                }
                else {
                    args[i] = new StaticMethodCodegenArgDesc(
                        name,
                        childType,
                        child.EvaluateCodegen(childType, codegenMethodScope, exprSymbol, codegenClassScope));
                }
            }

            return args;
        }

        public static void AppendArgExpressions(
            StaticMethodCodegenArgDesc[] args,
            CodegenBlock block)
        {
            for (var i = 0; i < args.Length; i++) {
                block.DeclareVar(args[i].DeclareType, args[i].BlockRefName, args[i].ArgExpression);
            }
        }

        public static void AppendCatch(
            CodegenBlock tryBlock,
            MethodInfo reflectionMethod,
            string statementName,
            string classOrPropertyName,
            bool rethrow,
            StaticMethodCodegenArgDesc[] args)
        {
            var catchBlock = tryBlock.TryEnd()
                .AddCatch(typeof(Exception), "ex")
                .DeclareVar<object[]>("argArray", NewArrayByLength(typeof(object), Constant(args.Length)));
            for (var i = 0; i < args.Length; i++) {
                catchBlock.AssignArrayElement("argArray", Constant(i), Ref(args[i].BlockRefName));
            }

            Type[] paramTypes = reflectionMethod.GetParameterTypes();
            catchBlock.StaticMethod(
                typeof(ExprDotNodeForgeStaticMethodEval),
                METHOD_STATICMETHODEVALHANDLEINVOCATIONEXCEPTION,
                Constant(statementName),
                Constant(reflectionMethod.Name),
                Constant(paramTypes),
                Constant(classOrPropertyName),
                Ref("argArray"),
                Ref("ex"),
                Constant(rethrow));
        }

        public static CodegenExpression CodegenInvokeExpression(
            object optionalTargetObject,
            MethodInfo reflectionMethod,
            StaticMethodCodegenArgDesc[] args,
            CodegenClassScope codegenClassScope)
        {
            var expressions = new CodegenExpression[args.Length];
            for (var i = 0; i < expressions.Length; i++) {
                expressions[i] = Ref(args[i].BlockRefName);
            }

            if (optionalTargetObject == null) {
                return StaticMethod(reflectionMethod.DeclaringType, reflectionMethod.Name, expressions);
            }

            if (optionalTargetObject.GetType().IsEnum) {
                return ExprDotMethod(
                    EnumValue(optionalTargetObject.GetType(), optionalTargetObject.ToString()),
                    reflectionMethod.Name,
                    expressions);
            }

            return ExprDotMethod(Constant(optionalTargetObject), reflectionMethod.Name, expressions);
        }
    }
} // end of namespace