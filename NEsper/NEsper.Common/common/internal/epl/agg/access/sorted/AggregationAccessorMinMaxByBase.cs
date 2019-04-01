///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    /// <summary>
    ///     Represents the aggregation accessor that provides the result for the "maxBy" aggregation function.
    /// </summary>
    public abstract class AggregationAccessorMinMaxByBase : AggregationAccessorForge
    {
        internal readonly bool max;

        protected AggregationAccessorMinMaxByBase(bool max)
        {
            this.max = max;
        }

        public void GetEnumerableEventsCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            var forge = (AggregatorAccessSorted) context.AccessStateForge.Aggregator;
            context.Method.Block
                .DeclareVar(
                    typeof(EventBean), "bean",
                    max
                        ? forge.GetLastValueCodegen(context.ClassScope, context.Method)
                        : forge.GetFirstValueCodegen(context.ClassScope, context.Method))
                .IfRefNullReturnNull("bean")
                .MethodReturn(StaticMethod(typeof(Collections), "SingletonList", Ref("bean")));
        }

        public void GetEnumerableScalarCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.MethodReturn(ConstantNull());
        }

        public void GetEnumerableEventCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            var sorted = (AggregatorAccessSorted) context.AccessStateForge.Aggregator;
            if (max) {
                context.Method.Block.MethodReturn(sorted.GetLastValueCodegen(context.ClassScope, context.Method));
            }
            else {
                context.Method.Block.MethodReturn(sorted.GetFirstValueCodegen(context.ClassScope, context.Method));
            }
        }

        public abstract void GetValueCodegen(AggregationAccessorForgeGetCodegenContext context);
    }
} // end of namespace