///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.aggregator;

namespace com.espertech.esper.epl.agg.factory
{
    public class AggregationMethodFactoryUtil
    {
        public static AggregationMethod MakeDistinctAggregator(AggregationMethod aggregationMethod, bool hasFilter)
        {
            if (hasFilter) return new AggregatorDistinctValueFilter(aggregationMethod);
            return new AggregatorDistinctValue(aggregationMethod);
        }

        public static AggregationMethod MakeFirstEver(bool hasFilter)
        {
            if (hasFilter) return new AggregatorFirstEverFilter();
            return new AggregatorFirstEver();
        }

        public static AggregationMethod MakeLastEver(bool hasFilter)
        {
            if (hasFilter) return new AggregatorLastEverFilter();
            return new AggregatorLastEver();
        }
    }
} // end of namespace