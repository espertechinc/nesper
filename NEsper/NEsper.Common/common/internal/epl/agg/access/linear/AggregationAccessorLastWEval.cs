///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "last" aggregation function without index.
    /// </summary>
    public class AggregationAccessorLastWEval
    {
        public static void GetValueCodegen(
            AggregationAccessorLastWEvalForge forge,
            AggregationStateLinearForge factoryLinear,
            AggregationAccessorForgeGetCodegenContext context)
        {
            CodegenMethod childExpr = CodegenLegoMethodExpression.CodegenExpression(
                forge.ChildNode,
                context.Method,
                context.ClassScope);
            context.Method.Block.DeclareVar<EventBean>(
                    "bean",
                    factoryLinear.AggregatorLinear.GetLastValueCodegen(
                        context.ClassScope,
                        context.Method,
                        context.NamedMethods))
                .IfRefNullReturnNull("bean")
                .DeclareVar<EventBean[]>(
                    "eventsPerStreamBuf",
                    NewArrayByLength(typeof(EventBean), Constant(forge.StreamNum + 1)))
                .AssignArrayElement("eventsPerStreamBuf", Constant(forge.StreamNum), @Ref("bean"))
                .MethodReturn(LocalMethod(childExpr, @Ref("eventsPerStreamBuf"), Constant(true), ConstantNull()));
        }

        public static void GetEnumerableEventsCodegen(
            AggregationAccessorLastWEvalForge forge,
            AggregationStateLinearForge factoryLinear,
            AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.DeclareVar<EventBean>(
                    "bean",
                    factoryLinear.AggregatorLinear.GetLastValueCodegen(
                        context.ClassScope,
                        context.Method,
                        context.NamedMethods))
                .IfRefNullReturnNull("bean")
                .MethodReturn(StaticMethod(typeof(Collections), "singletonList", @Ref("bean")));
        }

        public static void GetEnumerableScalarCodegen(
            AggregationAccessorLastWEvalForge forge,
            AggregationStateLinearForge stateForge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            CodegenMethod childExpr = CodegenLegoMethodExpression.CodegenExpression(
                forge.ChildNode,
                context.Method,
                context.ClassScope);
            context.Method.Block.DeclareVar<EventBean>(
                    "bean",
                    stateForge.AggregatorLinear.GetLastValueCodegen(
                        context.ClassScope,
                        context.Method,
                        context.NamedMethods))
                .IfRefNullReturnNull("bean")
                .DeclareVar<EventBean[]>(
                    "eventsPerStreamBuf",
                    NewArrayByLength(typeof(EventBean), Constant(forge.StreamNum + 1)))
                .AssignArrayElement("eventsPerStreamBuf", Constant(forge.StreamNum), @Ref("bean"))
                .DeclareVar<object>(
                    "value",
                    LocalMethod(childExpr, @Ref("eventsPerStreamBuf"), Constant(true), ConstantNull()))
                .IfRefNullReturnNull("value")
                .MethodReturn(StaticMethod(typeof(Collections), "singletonList", @Ref("value")));
        }

        public static void GetEnumerableEventCodegen(
            AggregationAccessorLastWEvalForge forge,
            AggregationStateLinearForge stateForge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.MethodReturn(
                stateForge.AggregatorLinear.GetLastValueCodegen(
                    context.ClassScope,
                    context.Method,
                    context.NamedMethods));
        }
    }
} // end of namespace