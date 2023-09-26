///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    /// <summary>
    /// Represents the aggregation accessor that provides the result for the "maxBy" aggregation function.
    /// </summary>
    public class AggregationAccessorSortedTable : AggregationAccessorForge
    {
        private readonly bool max;
        private readonly Type componentType;
        private readonly TableMetaData table;

        public AggregationAccessorSortedTable(
            bool max,
            Type componentType,
            TableMetaData table)
        {
            this.max = max;
            this.componentType = componentType;
            this.table = table;
        }

        public void GetValueCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            var eventToPublic = TableDeployTimeResolver.MakeTableEventToPublicField(table, context.ClassScope, GetType());
            var sorted = (AggregatorAccessSorted)context.AccessStateForge.Aggregator;
            var size = sorted.SizeCodegen();
            var enumerator = max ? sorted.ReverseEnumeratorCodegen() : sorted.EnumeratorCodegen();

            var arrayType = TypeHelper.GetArrayType(componentType);
            context.Method.Block
                .IfCondition(EqualsIdentity(size, Constant(0)))
                .BlockReturn(ConstantNull())
                .DeclareVar(arrayType, "array", NewArrayByLength(componentType, size))
                .DeclareVar<int>("count", Constant(0))
                .DeclareVar<IEnumerator<EventBean>>("enumerator", enumerator)
                .WhileLoop(ExprDotMethod(Ref("enumerator"), "MoveNext"))
                .DeclareVar<EventBean>("bean", Cast(typeof(EventBean), ExprDotName(Ref("enumerator"), "Current")))
                .AssignArrayElement(
                    Ref("array"),
                    Ref("count"),
                    ExprDotMethod(
                        eventToPublic,
                        "ConvertToUnd",
                        Ref("bean"),
                        REF_EPS,
                        REF_ISNEWDATA,
                        REF_EXPREVALCONTEXT))
                .IncrementRef("count")
                .BlockEnd()
                .MethodReturn(Ref("array"));
        }

        public void GetEnumerableEventsCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            var sorted = (AggregatorAccessSorted)context.AccessStateForge.Aggregator;
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