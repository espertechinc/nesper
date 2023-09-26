///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.agg.groupby;
using com.espertech.esper.common.@internal.epl.agg.groupbylocal;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationServiceFactoryServiceImpl : AggregationServiceFactoryService
    {
        public static readonly AggregationServiceFactoryServiceImpl INSTANCE =
            new AggregationServiceFactoryServiceImpl();

        private AggregationServiceFactoryServiceImpl()
        {
        }

        public AggregationServiceFactory GroupAll(
            AggregationServiceFactory nonHAFactory,
            AggregationRowFactory rowFactory,
            AggregationUseFlags useFlags,
            DataInputOutputSerde<AggregationRow> serde,
            StateMgmtSetting stateMgmtSetting)
        {
            return nonHAFactory;
        }

        public AggregationServiceFactory GroupBy(
            AggregationServiceFactory nonHAFactory,
            AggregationRowFactory rowFactory,
            AggregationUseFlags useFlags,
            DataInputOutputSerde<AggregationRow> serde,
            AggSvcGroupByReclaimAgedEvalFuncFactory reclaimMaxAge,
            AggSvcGroupByReclaimAgedEvalFuncFactory reclaimFreq,
            TimeAbacus timeAbacus,
            DataInputOutputSerde groupKeySerde,
            StateMgmtSetting stateMgmtSetting)
        {
            return nonHAFactory;
        }

        public AggregationServiceFactory GroupByRollup(
            AggregationServiceFactory nonHAFactory,
            AggregationGroupByRollupDesc groupByRollupDesc,
            AggregationRowFactory rowFactory,
            AggregationUseFlags useFlags,
            DataInputOutputSerde<AggregationRow> serde,
            StateMgmtSetting stateMgmtSetting)
        {
            return nonHAFactory;
        }

        public AggregationServiceFactory GroupLocalGroupBy(
            AggregationServiceFactory nonHAFactory,
            AggregationUseFlags useFlags,
            bool hasGroupBy,
            AggregationLocalGroupByLevel optionalTop,
            AggregationLocalGroupByLevel[] levels,
            AggregationLocalGroupByColumn[] columns,
            StateMgmtSetting stateMgmtSetting)
        {
            return nonHAFactory;
        }
    }
} // end of namespace