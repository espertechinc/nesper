///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "first" aggregation function without index.
    /// </summary>
    public class AggregationAccessorFirstWEval
    {
        public static void GetValueCodegen(
            AggregationAccessorFirstWEvalForge forge,
            AggregationStateLinearForge accessStateFactory,
            AggregationAccessorForgeGetCodegenContext context)
        {
            CodegenMethod childExpr = CodegenLegoMethodExpression.CodegenExpression(
                forge.ChildNode,
                context.Method,
                context.ClassScope,
                true);
            context.Method.Block
                .DeclareVar<EventBean>(
                    "bean",
                    accessStateFactory.AggregatorLinear.GetFirstValueCodegen(context.ClassScope, context.Method))
                .IfRefNullReturnNull("bean")
                .DeclareVar<EventBean[]>(
                    "eventsPerStreamBuf",
                    NewArrayByLength(typeof(EventBean), Constant(forge.StreamNum + 1)))
                .AssignArrayElement("eventsPerStreamBuf", Constant(forge.StreamNum), Ref("bean"))
                .MethodReturn(LocalMethod(childExpr, Ref("eventsPerStreamBuf"), Constant(true), ConstantNull()));
        }

        public static void GetEnumerableEventsCodegen(
            AggregationAccessorFirstWEvalForge forge,
            AggregationStateLinearForge stateForge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.DeclareVar<EventBean>(
                    "bean",
                    stateForge.AggregatorLinear.GetFirstValueCodegen(context.ClassScope, context.Method))
                .IfRefNullReturnNull("bean")
                .MethodReturn(StaticMethod(typeof(Collections), "SingletonList", Ref("bean")));
        }

        public static void GetEnumerableScalarCodegen(
            AggregationAccessorFirstWEvalForge forge,
            AggregationStateLinearForge accessStateFactory,
            AggregationAccessorForgeGetCodegenContext context)
        {
            CodegenMethod childExpr = CodegenLegoMethodExpression.CodegenExpression(
                forge.ChildNode,
                context.Method,
                context.ClassScope,
                true);
            context.Method.Block.DeclareVar<EventBean>(
                    "bean",
                    accessStateFactory.AggregatorLinear.GetFirstValueCodegen(context.ClassScope, context.Method))
                .DebugStack()
                .IfRefNullReturnNull("bean")
                .DeclareVar<EventBean[]>(
                    "eventsPerStreamBuf",
                    NewArrayByLength(typeof(EventBean), Constant(forge.StreamNum + 1)))
                .AssignArrayElement("eventsPerStreamBuf", Constant(forge.StreamNum), Ref("bean"))
                .DeclareVar<object>(
                    "value",
                    LocalMethod(childExpr, Ref("eventsPerStreamBuf"), Constant(true), ConstantNull()))
                .IfRefNullReturnNull("value")
                .MethodReturn(StaticMethod(typeof(Collections), "SingletonList", Ref("value")));
        }

        public static void GetEnumerableEventCodegen(
            AggregationAccessorFirstWEvalForge forge,
            AggregationStateLinearForge stateForge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.MethodReturn(
                stateForge.AggregatorLinear.GetFirstValueCodegen(context.ClassScope, context.Method));
        }
    }
} // end of namespace