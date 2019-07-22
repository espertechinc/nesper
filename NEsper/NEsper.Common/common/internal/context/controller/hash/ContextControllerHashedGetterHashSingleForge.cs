///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashedGetterHashSingleForge : EventPropertyValueGetterForge
    {
        private readonly ExprNode eval;
        private readonly int granularity;

        public ContextControllerHashedGetterHashSingleForge(
            ExprNode eval,
            int granularity)
        {
            this.eval = eval;
            this.granularity = granularity;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="code">code</param>
        /// <param name="granularity">granularity</param>
        /// <returns>hash</returns>
        public static int ObjectToNativeHash(
            object code,
            int granularity)
        {
            int value;
            if (code == null) {
                value = 0;
            }
            else {
                value = code.GetHashCode() % granularity;
            }

            if (value >= 0) {
                return value;
            }

            return -value;
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(object), this.GetType(), classScope)
                .AddParam(typeof(EventBean), "eventBean");
            CodegenMethod methodExpr = CodegenLegoMethodExpression.CodegenExpression(eval.Forge, method, classScope);
            method.Block
                .DeclareVar<EventBean[]>("events", NewArrayWithInit(typeof(EventBean), @Ref("eventBean")))
                .DeclareVar<object>("code", LocalMethod(methodExpr, @Ref("events"), ConstantTrue(), ConstantNull()))
                .MethodReturn(
                    StaticMethod(
                        typeof(ContextControllerHashedGetterHashSingleForge),
                        "objectToNativeHash",
                        @Ref("code"),
                        Constant(granularity)));

            return LocalMethod(method, beanExpression);
        }
    }
} // end of namespace