///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.agg.groupby;
using com.espertech.esper.common.@internal.epl.agg.groupbylocal;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
	public class AggregationServiceFactoryServiceImpl : AggregationServiceFactoryService {

	    public readonly static AggregationServiceFactoryServiceImpl INSTANCE = new AggregationServiceFactoryServiceImpl();

	    private AggregationServiceFactoryServiceImpl() {
	    }

	    public AggregationServiceFactory GroupAll(AggregationServiceFactory nonHAFactory, AggregationRowFactory rowFactory, AggregationUseFlags useFlags, DataInputOutputSerdeWCollation<AggregationRow> serde) {
	        return nonHAFactory;
	    }

	    public AggregationServiceFactory GroupBy(AggregationServiceFactory nonHAFactory, AggregationRowFactory rowFactory, AggregationUseFlags useFlags, DataInputOutputSerdeWCollation<AggregationRow> serde, Type[] groupByTypes, AggSvcGroupByReclaimAgedEvalFuncFactory reclaimMaxAge, AggSvcGroupByReclaimAgedEvalFuncFactory reclaimFreq, TimeAbacus timeAbacus) {
	        return nonHAFactory;
	    }

	    public AggregationServiceFactory GroupByRollup(AggregationServiceFactory nonHAFactory, AggregationGroupByRollupDesc groupByRollupDesc, AggregationRowFactory rowFactory, AggregationUseFlags useFlags, DataInputOutputSerdeWCollation<AggregationRow> serde, Type[] groupByTypes) {
	        return nonHAFactory;
	    }

	    public AggregationServiceFactory GroupLocalGroupBy(AggregationServiceFactory nonHAFactory, AggregationUseFlags useFlags, bool hasGroupBy, AggregationLocalGroupByLevel optionalTop, AggregationLocalGroupByLevel[] levels, AggregationLocalGroupByColumn[] columns) {
	        return nonHAFactory;
	    }
	}
} // end of namespace