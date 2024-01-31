///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    ///     Represents the aggregation accessor that provides the result for the "window" aggregation function.
    /// </summary>
    public class AggregationAccessorWindowNoEval
    {
        public static void GetValueCodegen(
            AggregationAccessorWindowNoEvalForge forge,
            AggregationStateLinearForge accessStateFactory,
            AggregationAccessorForgeGetCodegenContext context)
        {
            var size = accessStateFactory.AggregatorLinear.SizeCodegen();
            var enumerator = accessStateFactory.AggregatorLinear.EnumeratorCodegen(
                context.ClassScope,
                context.Method,
                context.NamedMethods);

            var arrayType = TypeHelper.GetArrayType(forge.ComponentType);
            context.Method.Block
                .IfCondition(EqualsIdentity(size, Constant(0)))
                .BlockReturn(ConstantNull())
                .DeclareVar(arrayType, "array", NewArrayByLength(forge.ComponentType, size))
                .DeclareVar<int>("count", Constant(0))
                .DeclareVar<IEnumerator<EventBean>>("enumerator", enumerator)
                .WhileLoop(ExprDotMethod(Ref("enumerator"), "MoveNext"))
                .DeclareVar<EventBean>("bean", Cast(typeof(EventBean), ExprDotName(Ref("enumerator"), "Current")))
                .AssignArrayElement(
                    Ref("array"),
                    Ref("count"),
                    Cast(forge.ComponentType, ExprDotUnderlying(Ref("bean"))))
                .IncrementRef("count")
                .BlockEnd()
                .MethodReturn(Ref("array"));
        }

        public static void GetEnumerableEventsCodegen(
            AggregationAccessorWindowNoEvalForge forge,
            AggregationStateLinearForge stateForge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.IfCondition(EqualsIdentity(stateForge.AggregatorLinear.SizeCodegen(), Constant(0)))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    stateForge.AggregatorLinear.CollectionReadOnlyCodegen(
                        context.Method,
                        context.ClassScope,
                        context.NamedMethods));
        }

        public static void GetEnumerableScalarCodegen(
            AggregationAccessorWindowNoEvalForge forge,
            AggregationStateLinearForge stateForge,
            AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.DeclareVar<int>("size", stateForge.AggregatorLinear.SizeCodegen())
                .IfCondition(EqualsIdentity(Ref("size"), Constant(0)))
                .BlockReturn(ConstantNull())
                .DeclareVar<IList<object>>("values", NewInstance<List<object>>(Ref("size")))
                .DeclareVar<IEnumerator<EventBean>>(
                    "enumerator",
                    stateForge.AggregatorLinear.EnumeratorCodegen(
                        context.ClassScope,
                        context.Method,
                        context.NamedMethods))
                .WhileLoop(ExprDotMethod(Ref("enumerator"), "MoveNext"))
                .DeclareVar<EventBean>("bean", Cast(typeof(EventBean), ExprDotName(Ref("enumerator"), "Current")))
                .DeclareVar(forge.ComponentType, "value", Cast(forge.ComponentType, ExprDotUnderlying(Ref("bean"))))
                .ExprDotMethod(Ref("values"), "Add", Ref("value"))
                .BlockEnd()
                .MethodReturn(Ref("values"));
        }
    }
} // end of namespace