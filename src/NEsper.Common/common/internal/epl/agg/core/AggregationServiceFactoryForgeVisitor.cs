///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.groupall;
using com.espertech.esper.common.@internal.epl.agg.groupby;
using com.espertech.esper.common.@internal.epl.agg.groupbylocal;
using com.espertech.esper.common.@internal.epl.agg.rollup;
using com.espertech.esper.common.@internal.epl.agg.table;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public interface AggregationServiceFactoryForgeVisitor<T>
    {
        T Visit(AggregationServiceGroupAllForge forge);

        T Visit(AggSvcLocalGroupByForge forge);

        T Visit(AggregationServiceFactoryForgeTable forge);

        T Visit(AggregationServiceNullFactory forge);

        T Visit(AggSvcGroupByRollupForge forge);

        T Visit(AggregationServiceGroupByForge forge);
    }
} // end of namespace