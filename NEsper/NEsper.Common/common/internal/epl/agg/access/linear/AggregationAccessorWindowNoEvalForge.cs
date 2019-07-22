///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.agg.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    /// <summary>
    ///     Represents the aggregation accessor that provides the result for the "window" aggregation function.
    /// </summary>
    public class AggregationAccessorWindowNoEvalForge : AggregationAccessorForge
    {
        public AggregationAccessorWindowNoEvalForge(Type componentType)
        {
            ComponentType = componentType;
        }

        public Type ComponentType { get; }

        public void GetValueCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorWindowNoEval.GetValueCodegen(
                this,
                (AggregationStateLinearForge) context.AccessStateForge,
                context);
        }

        public void GetEnumerableEventsCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            AggregationAccessorWindowNoEval.GetEnumerableEventsCodegen(
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
            AggregationAccessorWindowNoEval.GetEnumerableScalarCodegen(
                this,
                (AggregationStateLinearForge) context.AccessStateForge,
                context);
        }
    }
} // end of namespace