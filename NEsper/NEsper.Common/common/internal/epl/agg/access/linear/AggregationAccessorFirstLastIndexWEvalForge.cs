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
    ///     Represents the aggregation accessor that provides the result for the "first" and "last" aggregation function with
    ///     index.
    /// </summary>
    public class AggregationAccessorFirstLastIndexWEvalForge : AggregationAccessorForge
    {
        private readonly bool isFirst;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">stream id</param>
        /// <param name="childNode">expression</param>
        /// <param name="indexNode">index expression</param>
        /// <param name="constant">constant index</param>
        /// <param name="isFirst">true if returning first, false for returning last</param>
        public AggregationAccessorFirstLastIndexWEvalForge(
            int streamNum, ExprForge childNode, ExprForge indexNode, int constant, bool isFirst)
        {
            StreamNum = streamNum;
            ChildNode = childNode;
            IndexNode = indexNode;
            Constant = constant;
            this.isFirst = isFirst;
        }

        public int StreamNum { get; }

        public ExprForge ChildNode { get; }

        public ExprForge IndexNode { get; }

        public int Constant { get; }

        public void GetValueCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorFirstLastIndexWEval.GetValueCodegen(this, context);
        }

        public void GetEnumerableEventsCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorFirstLastIndexWEval.GetEnumerableEventsCodegen(this, context);
        }

        public void GetEnumerableEventCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorFirstLastIndexWEval.GetEnumerableEventCodegen(this, context);
        }

        public void GetEnumerableScalarCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorFirstLastIndexWEval.GetEnumerableScalarCodegen(this, context);
        }

        public bool IsFirst()
        {
            return isFirst;
        }
    }
} // end of namespace