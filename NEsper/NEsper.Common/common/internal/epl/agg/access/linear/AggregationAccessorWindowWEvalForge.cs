///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    ///     Represents the aggregation accessor that provides the result for the "window" aggregation function.
    /// </summary>
    public class AggregationAccessorWindowWEvalForge : AggregationAccessorForge
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">stream id</param>
        /// <param name="childNode">expression</param>
        /// <param name="componentType">type</param>
        public AggregationAccessorWindowWEvalForge(
            int streamNum,
            ExprForge childNode,
            Type componentType)
        {
            StreamNum = streamNum;
            ChildNode = childNode;
            ComponentType = componentType;
        }

        public int StreamNum { get; }

        public ExprForge ChildNode { get; }

        public Type ComponentType { get; }

        public void GetValueCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorWindowWEval.GetValueCodegen(
                this,
                (AggregationStateLinearForge) context.AccessStateForge,
                context);
        }

        public void GetEnumerableEventsCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorWindowWEval.GetEnumerableEventsCodegen(
                this,
                (AggregationStateLinearForge) context.AccessStateForge,
                context);
        }

        public void GetEnumerableEventCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.MethodReturn(ConstantNull());
        }

        public void GetEnumerableScalarCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorWindowWEval.GetEnumerableScalarCodegen(
                this,
                (AggregationStateLinearForge) context.AccessStateForge,
                context);
        }
    }
} // end of namespace