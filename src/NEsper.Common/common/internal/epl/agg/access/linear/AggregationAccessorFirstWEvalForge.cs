///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    ///     Represents the aggregation accessor that provides the result for the "first" aggregation function without index.
    /// </summary>
    public class AggregationAccessorFirstWEvalForge : AggregationAccessorForge
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">stream id</param>
        /// <param name="childNode">expression</param>
        public AggregationAccessorFirstWEvalForge(
            int streamNum,
            ExprForge childNode)
        {
            StreamNum = streamNum;
            ChildNode = childNode;
        }

        public int StreamNum { get; }

        public ExprForge ChildNode { get; }

        public void GetValueCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorFirstWEval.GetValueCodegen(
                this,
                (AggregationStateLinearForge)context.AccessStateForge,
                context);
        }

        public void GetEnumerableEventsCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorFirstWEval.GetEnumerableEventsCodegen(
                this,
                (AggregationStateLinearForge)context.AccessStateForge,
                context);
        }

        public void GetEnumerableEventCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorFirstWEval.GetEnumerableEventCodegen(
                this,
                (AggregationStateLinearForge)context.AccessStateForge,
                context);
        }

        public void GetEnumerableScalarCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorFirstWEval.GetEnumerableScalarCodegen(
                this,
                (AggregationStateLinearForge)context.AccessStateForge,
                context);
        }
    }
} // end of namespace