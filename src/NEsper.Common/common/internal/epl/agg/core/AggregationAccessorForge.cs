///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public interface AggregationAccessorForge
    {
        void GetValueCodegen(AggregationAccessorForgeGetCodegenContext context);

        void GetEnumerableEventsCodegen(AggregationAccessorForgeGetCodegenContext context);

        void GetEnumerableEventCodegen(AggregationAccessorForgeGetCodegenContext context);

        void GetEnumerableScalarCodegen(AggregationAccessorForgeGetCodegenContext context);
    }
} // end of namespace