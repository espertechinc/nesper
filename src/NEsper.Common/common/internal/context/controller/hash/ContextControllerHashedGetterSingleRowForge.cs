///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.StaticMethodCallHelper;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashedGetterSingleRowForge : EventPropertyValueGetterForge
    {
        private readonly MethodInfo reflectionMethod;
        private readonly ExprForge[] nodes;
        private readonly int granularity;
        private readonly string statementName;

        public ContextControllerHashedGetterSingleRowForge(
            Pair<Type, ImportSingleRowDesc> func,
            IList<ExprNode> parameters,
            int granularity,
            EventType eventType,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var staticMethodDesc = ExprNodeUtilityResolve.ResolveMethodAllowWildcardAndStream(
                func.First.Name,
                null,
                func.Second.MethodName,
                parameters,
                true,
                eventType,
                new ExprNodeUtilResolveExceptionHandlerDefault(func.Second.MethodName, true),
                func.Second.MethodName,
                statementRawInfo,
                services);
            this.granularity = granularity;
            nodes = staticMethodDesc.ChildForges;
            reflectionMethod = staticMethodDesc.ReflectionMethod;
            statementName = statementRawInfo.StatementName;
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(object), GetType(), classScope)
                .AddParam<EventBean>("eventBean");
            method.Block.DeclareVar<EventBean[]>("events", NewArrayWithInit(typeof(EventBean), Ref("eventBean")));

            // method to evaluate expressions and compute hash
            var exprSymbol = new ExprForgeCodegenSymbol(true, true);
            var returnType = reflectionMethod.ReturnType;
            var exprMethod = method
                .MakeChildWithScope(returnType, typeof(CodegenLegoMethodExpression), exprSymbol, classScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);

            // generate args
            var args = AllArgumentExpressions(
                nodes,
                reflectionMethod,
                exprMethod,
                exprSymbol,
                classScope);
            AppendArgExpressions(args, exprMethod.Block);

            // try block
            var tryBlock = exprMethod.Block.TryCatch();
            var invoke = CodegenInvokeExpression(null, reflectionMethod, args, classScope);
            tryBlock.BlockReturn(invoke);

            // exception handling
            AppendCatch(tryBlock, reflectionMethod, statementName, reflectionMethod.DeclaringType.Name, true, args);

            exprMethod.Block.MethodReturn(Constant(0));

            var returnTypeMethod = reflectionMethod.ReturnType;
            method.Block.DeclareVar(
                returnTypeMethod,
                "result",
                LocalMethod(exprMethod, Ref("events"), ConstantTrue(), ConstantNull()));
            if (!reflectionMethod.ReturnType.IsPrimitive) {
                method.Block.IfRefNull("result").BlockReturn(Constant(0));
            }

            method.Block.DeclareVar<int>(
                    "value",
                    SimpleNumberCoercerFactory.GetCoercer(returnTypeMethod, typeof(int?))
                        .CoerceCodegen(Ref("result"), returnTypeMethod, method, classScope))
                .IfCondition(Relational(Ref("value"), CodegenExpressionRelational.CodegenRelational.GE, Constant(0)))
                .BlockReturn(Op(Ref("value"), "%", Constant(granularity)))
                .MethodReturn(Op(Op(Ref("value"), "%", Constant(granularity)), "*", Constant(-1)));
            return LocalMethod(method, beanExpression);
        }
    }
} // end of namespace