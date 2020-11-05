///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.epl.agg.groupby;
using com.espertech.esper.common.@internal.epl.agg.groupbylocal;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public interface AggregationServiceFactoryService
    {
        AggregationServiceFactory GroupAll(
            AggregationServiceFactory nonHAFactory,
            AggregationRowFactory rowFactory,
            AggregationUseFlags useFlags,
            DataInputOutputSerde<AggregationRow> serde);

        AggregationServiceFactory GroupBy(
            AggregationServiceFactory nonHAFactory,
            AggregationRowFactory rowFactory,
            AggregationUseFlags useFlags,
            DataInputOutputSerde<AggregationRow> serde,
            Type[] groupByTypes,
            AggSvcGroupByReclaimAgedEvalFuncFactory reclaimMaxAge,
            AggSvcGroupByReclaimAgedEvalFuncFactory reclaimFreq,
            TimeAbacus timeAbacus,
            DataInputOutputSerde groupKeySerde);

        AggregationServiceFactory GroupByRollup(
            AggregationServiceFactory nonHAFactory,
            AggregationGroupByRollupDesc groupByRollupDesc,
            AggregationRowFactory rowFactory,
            AggregationUseFlags useFlags,
            DataInputOutputSerde<AggregationRow> serde,
            Type[] groupByTypes);

        AggregationServiceFactory GroupLocalGroupBy(
            AggregationServiceFactory nonHAFactory,
            AggregationUseFlags useFlags,
            bool hasGroupBy,
            AggregationLocalGroupByLevel optionalTop,
            AggregationLocalGroupByLevel[] levels,
            AggregationLocalGroupByColumn[] columns);
    }
} // end of namespace