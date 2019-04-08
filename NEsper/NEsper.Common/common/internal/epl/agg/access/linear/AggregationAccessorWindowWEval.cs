///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    ///     Represents the aggregation accessor that provides the result for the "window" aggregation function.
    /// </summary>
    public class AggregationAccessorWindowWEval
    {
        public static void GetValueCodegen(
            AggregationAccessorWindowWEvalForge forge,
            AggregationStateLinearForge accessStateFactory,
            AggregationAccessorForgeGetCodegenContext context)
        {
            var size = accessStateFactory.AggregatorLinear.SizeCodegen();
            var iterator = accessStateFactory.AggregatorLinear.IteratorCodegen(context.ClassScope, context.Method, context.NamedMethods);
            var childExpr = CodegenLegoMethodExpression.CodegenExpression(forge.ChildNode, context.Method, context.ClassScope);

            context.Method.Block.IfCondition(EqualsIdentity(size, Constant(0))).BlockReturn(ConstantNull())
                .DeclareVar(TypeHelper.GetArrayType(forge.ComponentType), "array", NewArrayByLength(forge.ComponentType, size))
                .DeclareVar(typeof(int), "count", Constant(0))
                .DeclareVar(typeof(IEnumerator<EventBean>), "it", iterator)
                .DeclareVar(typeof(EventBean[]), "eventsPerStreamBuf", NewArrayByLength(typeof(EventBean), Constant(forge.StreamNum + 1)))
                .WhileLoop(ExprDotMethod(Ref("it"), "hasNext"))
                .DeclareVar(typeof(EventBean), "bean", Cast(typeof(EventBean), ExprDotMethod(Ref("it"), "next")))
                .AssignArrayElement("eventsPerStreamBuf", Constant(forge.StreamNum), Ref("bean"))
                .AssignArrayElement(Ref("array"), Ref("count"), LocalMethod(childExpr, Ref("eventsPerStreamBuf"), Constant(true), ConstantNull()))
                .Increment("count")
                .BlockEnd()
                .MethodReturn(Ref("array"));
        }

        public static void GetEnumerableEventsCodegen(
            AggregationAccessorWindowWEvalForge forge,
            AggregationStateLinearForge stateForge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.IfCondition(EqualsIdentity(stateForge.AggregatorLinear.SizeCodegen(), Constant(0)))
                .BlockReturn(ConstantNull())
                .MethodReturn(stateForge.AggregatorLinear.CollectionReadOnlyCodegen(context.Method, context.ClassScope, context.NamedMethods));
        }

        public static void GetEnumerableScalarCodegen(
            AggregationAccessorWindowWEvalForge forge,
            AggregationStateLinearForge stateForge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.DeclareVar(typeof(int), "size", stateForge.AggregatorLinear.SizeCodegen())
                .IfCondition(EqualsIdentity(Ref("size"), Constant(0))).BlockReturn(ConstantNull())
                .DeclareVar(typeof(IList<EventBean>), "values", NewInstance(typeof(List<EventBean>), Ref("size")))
                .DeclareVar(
                    typeof(IEnumerator<EventBean>), "it",
                    stateForge.AggregatorLinear.IteratorCodegen(context.ClassScope, context.Method, context.NamedMethods))
                .DeclareVar(typeof(EventBean[]), "eventsPerStreamBuf", NewArrayByLength(typeof(EventBean), Constant(forge.StreamNum + 1)))
                .WhileLoop(ExprDotMethod(Ref("it"), "hasNext"))
                .DeclareVar(typeof(EventBean), "bean", Cast(typeof(EventBean), ExprDotMethod(Ref("it"), "next")))
                .AssignArrayElement("eventsPerStreamBuf", Constant(forge.StreamNum), Ref("bean"))
                .DeclareVar(
                    forge.ChildNode.EvaluationType.GetBoxedType(), "value",
                    LocalMethod(
                        CodegenLegoMethodExpression.CodegenExpression(forge.ChildNode, context.Method, context.ClassScope), Ref("eventsPerStreamBuf"),
                        ConstantTrue(), ConstantNull()))
                .ExprDotMethod(Ref("values"), "add", Ref("value"))
                .BlockEnd()
                .MethodReturn(Ref("values"));
        }
    }
} // end of namespace