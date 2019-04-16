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
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.util;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    /// <summary>
    ///     Represents the aggregation accessor that provides the result for the "maxBy" aggregation function.
    /// </summary>
    public class AggregationAccessorSortedNonTable : AggregationAccessorForge
    {
        private readonly Type componentType;
        private readonly bool max;

        public AggregationAccessorSortedNonTable(
            bool max,
            Type componentType)
        {
            this.max = max;
            this.componentType = componentType;
        }

        public void GetValueCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            var sorted = (AggregatorAccessSorted) context.AccessStateForge.Aggregator;
            var size = sorted.SizeCodegen();
            var iterator = max ? sorted.ReverseIteratorCodegen : sorted.IteratorCodegen();

            context.Method.Block.IfCondition(EqualsIdentity(size, Constant(0))).BlockReturn(ConstantNull())
                .DeclareVar(TypeHelper.GetArrayType(componentType), "array", NewArrayByLength(componentType, size))
                .DeclareVar(typeof(int), "count", Constant(0))
                .DeclareVar(typeof(IEnumerator<EventBean>), "it", iterator)
                .WhileLoop(ExprDotMethod(Ref("it"), "hasNext"))
                .DeclareVar(typeof(EventBean), "bean", Cast(typeof(EventBean), ExprDotMethod(Ref("it"), "next")))
                .AssignArrayElement(Ref("array"), Ref("count"), Cast(componentType, ExprDotUnderlying(Ref("bean"))))
                .Increment("count")
                .BlockEnd()
                .MethodReturn(Ref("array"));
        }

        public void GetEnumerableEventsCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            var sorted = (AggregatorAccessSorted) context.AccessStateForge.Aggregator;
            context.Method.Block.MethodReturn(sorted.CollectionReadOnlyCodegen());
        }

        public void GetEnumerableScalarCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.MethodReturn(ConstantNull());
        }

        public void GetEnumerableEventCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.MethodReturn(ConstantNull());
        }
    }
} // end of namespace