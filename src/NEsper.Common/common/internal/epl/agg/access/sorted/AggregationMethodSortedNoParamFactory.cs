///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.epl.agg.access.sorted.AggregationMethodSortedKeyedFactory;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationMethodSortedNoParamFactory
    {
        public static AggregationMethodSortedNoParamEval MakeSortedAggregationNoParam(
            AggregationMethodSortedEnum method,
            Type underlyingClass)
        {
            if (method.GetFootprint() != AggregationMethodSortedFootprintEnum.NOPARAM) {
                throw new IllegalStateException("Unrecognized aggregation method " + method);
            }

            if (method == AggregationMethodSortedEnum.FIRSTEVENT) {
                return new AggregationMethodSortedNoParamEval(
                    sorted => FirstUnd(sorted.Sorted.FirstEntry),
                    sorted => FirstBean(sorted.Sorted.FirstEntry),
                    sorted => FirstColl(sorted.Sorted.FirstEntry)
                );
            }

            if (method == AggregationMethodSortedEnum.FIRSTEVENTS) {
                return new AggregationMethodSortedNoParamEval(
                    sorted => EventsArrayUnd(sorted.Sorted.FirstEntry, underlyingClass),
                    sorted => null,
                    sorted => EventsColl(sorted.Sorted.FirstEntry)
                );
            }

            if (method == AggregationMethodSortedEnum.FIRSTKEY) {
                return new AggregationMethodSortedNoParamEval(
                    sorted => sorted.Sorted.FirstEntry.Key,
                    sorted => null,
                    sorted => null
                );
            }

            if (method == AggregationMethodSortedEnum.LASTEVENT) {
                return new AggregationMethodSortedNoParamEval(
                    sorted => FirstUnd(sorted.Sorted.LastEntry),
                    sorted => FirstBean(sorted.Sorted.LastEntry),
                    sorted => FirstColl(sorted.Sorted.LastEntry)
                );
            }

            if (method == AggregationMethodSortedEnum.LASTEVENTS) {
                return new AggregationMethodSortedNoParamEval(
                    sorted => EventsArrayUnd(sorted.Sorted.LastEntry, underlyingClass),
                    sorted => null,
                    sorted => EventsColl(sorted.Sorted.LastEntry)
                );
            }

            if (method == AggregationMethodSortedEnum.LASTKEY) {
                return new AggregationMethodSortedNoParamEval(
                    sorted => sorted.Sorted.LastEntry.Key,
                    sorted => null,
                    sorted => null
                );
            }

            if (method == AggregationMethodSortedEnum.COUNTEVENTS) {
                return new AggregationMethodSortedNoParamEval(
                    sorted => sorted.Count,
                    sorted => null,
                    sorted => null
                );
            }

            if (method == AggregationMethodSortedEnum.COUNTKEYS) {
                return new AggregationMethodSortedNoParamEval(
                    sorted => sorted.Sorted.Count,
                    sorted => null,
                    sorted => null
                );
            }

            if (method == AggregationMethodSortedEnum.NAVIGABLEMAPREFERENCE) {
                return new AggregationMethodSortedNoParamEval(
                    sorted => new AggregationMethodSortedWrapperDictionary(sorted.Sorted),
                    sorted => null,
                    sorted => null
                );
            }

            throw new IllegalStateException("Unrecognized aggregation method " + method);
        }
    }
} // end of namespace