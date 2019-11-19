///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    /// <summary>
    ///     Represents the aggregation accessor that provides the result for the "maxBy" aggregation function.
    /// </summary>
    public class AggregationAccessorMinMaxByNonTable : AggregationAccessorMinMaxByBase,
        AggregationAccessorForge
    {
        public AggregationAccessorMinMaxByNonTable(bool max)
            : base(max)
        {
        }

        public override void GetValueCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            var forge = (AggregatorAccessSorted) context.AccessStateForge.Aggregator;
            context.Method.Block.DeclareVar<EventBean>(
                    "@event",
                    max
                        ? forge.GetLastValueCodegen(context.ClassScope, context.Method)
                        : forge.GetFirstValueCodegen(context.ClassScope, context.Method))
                .IfRefNullReturnNull("@event")
                .MethodReturn(ExprDotUnderlying(Ref("@event")));
        }
    }
} // end of namespace