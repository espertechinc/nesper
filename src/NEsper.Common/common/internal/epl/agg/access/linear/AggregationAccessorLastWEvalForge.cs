///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    ///     Represents the aggregation accessor that provides the result for the "last" aggregation function without index.
    /// </summary>
    public class AggregationAccessorLastWEvalForge : AggregationAccessorForge
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">stream id</param>
        /// <param name="childNode">expression</param>
        public AggregationAccessorLastWEvalForge(
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
            AggregationAccessorLastWEval.GetValueCodegen(
                this,
                (AggregationStateLinearForge) context.AccessStateForge,
                context);
        }

        public void GetEnumerableEventsCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorLastWEval.GetEnumerableEventsCodegen(
                this,
                (AggregationStateLinearForge) context.AccessStateForge,
                context);
        }

        public void GetEnumerableEventCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorLastWEval.GetEnumerableEventCodegen(
                this,
                (AggregationStateLinearForge) context.AccessStateForge,
                context);
        }

        public void GetEnumerableScalarCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorLastWEval.GetEnumerableScalarCodegen(
                this,
                (AggregationStateLinearForge) context.AccessStateForge,
                context);
        }
    }
} // end of namespace