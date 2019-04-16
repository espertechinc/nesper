///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
    public class AggregationAccessorForgeCountMinSketch : AggregationAccessorForge
    {
        public void GetValueCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.MethodReturn(ConstantNull());
        }

        public void GetEnumerableEventsCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.MethodReturn(ConstantNull());
        }

        public void GetEnumerableEventCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.MethodReturn(ConstantNull());
        }

        public void GetEnumerableScalarCodegen(AggregationAccessorForgeGetCodegenContext context)
        {
            context.Method.Block.MethodReturn(ConstantNull());
        }
    }
} // end of namespace