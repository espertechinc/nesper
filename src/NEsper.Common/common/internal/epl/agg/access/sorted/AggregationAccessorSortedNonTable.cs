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
using com.espertech.esper.common.client.util;
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
        private readonly Type _componentType;
        private readonly bool _max;

        public AggregationAccessorSortedNonTable(
            bool max,
            Type componentType)
        {
            _max = max;
            _componentType = componentType;
        }

        public void GetValueCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            var sorted = (AggregatorAccessSorted) context.AccessStateForge.Aggregator;
            var size = sorted.SizeCodegen();
            var enumerator = _max ? sorted.ReverseEnumeratorCodegen() : sorted.EnumeratorCodegen();

            var arrayType = TypeHelper.GetArrayType(_componentType);
            context.Method.Block
                .IfCondition(EqualsIdentity(size, Constant(0)))
                .BlockReturn(ConstantNull())
                .DeclareVar(arrayType, "array", NewArrayByLength(_componentType, size))
                .DeclareVar<int>("count", Constant(0))
                .DeclareVar<IEnumerator<EventBean>>("enumerator", enumerator)
                .WhileLoop(ExprDotMethod(Ref("enumerator"), "MoveNext"))
                .DeclareVar<EventBean>("bean", Cast(typeof(EventBean), ExprDotName(Ref("enumerator"), "Current")))
                .AssignArrayElement(Ref("array"), Ref("count"), FlexCast(_componentType, ExprDotUnderlying(Ref("bean"))))
                .IncrementRef("count")
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