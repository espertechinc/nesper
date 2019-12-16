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

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.StaticMethodCallHelper;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashedGetterSingleRowForge : EventPropertyValueGetterForge
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            ExprNodeUtilMethodDesc staticMethodDesc = ExprNodeUtilityResolve.ResolveMethodAllowWildcardAndStream(
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
            this.nodes = staticMethodDesc.ChildForges;
            this.reflectionMethod = staticMethodDesc.ReflectionMethod;
            this.statementName = statementRawInfo.StatementName;
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(object), this.GetType(), classScope)
                .AddParam(typeof(EventBean), "eventBean");
            method.Block.DeclareVar<EventBean[]>("events", NewArrayWithInit(typeof(EventBean), Ref("eventBean")));

            // method to evaluate expressions and compute hash
            ExprForgeCodegenSymbol exprSymbol = new ExprForgeCodegenSymbol(true, true);
            CodegenMethod exprMethod = method
                .MakeChildWithScope(
                    reflectionMethod.ReturnType,
                    typeof(CodegenLegoMethodExpression),
                    exprSymbol,
                    classScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);

            // generate args
            StaticMethodCodegenArgDesc[] args = AllArgumentExpressions(
                nodes,
                reflectionMethod,
                exprMethod,
                exprSymbol,
                classScope);
            AppendArgExpressions(args, exprMethod.Block);

            // try block
            CodegenBlock tryBlock = exprMethod.Block.TryCatch();
            CodegenExpression invoke = CodegenInvokeExpression(null, reflectionMethod, args, classScope);
            tryBlock.BlockReturn(invoke);

            // exception handling
            AppendCatch(
                tryBlock,
                reflectionMethod,
                statementName,
                reflectionMethod.DeclaringType.CleanName(),
                true,
                args);

            exprMethod.Block.MethodReturn(Constant(0));

            method.Block.DeclareVar(
                reflectionMethod.ReturnType,
                "result",
                LocalMethod(exprMethod, Ref("events"), ConstantTrue(), ConstantNull()));
            if (reflectionMethod.ReturnType.CanBeNull()) {
                method.Block.IfRefNull("result").BlockReturn(Constant(0));
            }

            method.Block.DeclareVar<int>(
                    "value",
                    SimpleNumberCoercerFactory.GetCoercer(reflectionMethod.ReturnType, typeof(int?))
                        .CoerceCodegen(Ref("result"), reflectionMethod.ReturnType))
                .IfCondition(Relational(Ref("value"), CodegenExpressionRelational.CodegenRelational.GE, Constant(0)))
                .BlockReturn(Op(Ref("value"), "%", Constant(granularity)))
                .MethodReturn(Op(Op(Ref("value"), "%", Constant(granularity)), "*", Constant(-1)));
            return LocalMethod(method, beanExpression);
        }
    }
} // end of namespace