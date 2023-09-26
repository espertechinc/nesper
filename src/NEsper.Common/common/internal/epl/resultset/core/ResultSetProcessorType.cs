///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public enum ResultSetProcessorType
    {
        HANDTHROUGH,
        UNAGGREGATED_UNGROUPED,
        FULLYAGGREGATED_UNGROUPED,
        AGGREGATED_UNGROUPED,
        FULLYAGGREGATED_GROUPED,
        FULLYAGGREGATED_GROUPED_ROLLUP,
        AGGREGATED_GROUPED
    }

    public static class ResultSetProcessorTypeExtensions
    {
        public static bool IsAggregated(this ResultSetProcessorType value)
        {
            switch (value) {
                case ResultSetProcessorType.HANDTHROUGH:
                case ResultSetProcessorType.UNAGGREGATED_UNGROUPED:
                    return false;

                case ResultSetProcessorType.FULLYAGGREGATED_UNGROUPED:
                case ResultSetProcessorType.AGGREGATED_UNGROUPED:
                case ResultSetProcessorType.FULLYAGGREGATED_GROUPED:
                case ResultSetProcessorType.FULLYAGGREGATED_GROUPED_ROLLUP:
                case ResultSetProcessorType.AGGREGATED_GROUPED:
                    return true;

                default:
                    throw new ArgumentException("invalid argument", nameof(value));
            }
        }

        public static bool IsGrouped(this ResultSetProcessorType value)
        {
            switch (value) {
                case ResultSetProcessorType.HANDTHROUGH:
                case ResultSetProcessorType.UNAGGREGATED_UNGROUPED:
                case ResultSetProcessorType.FULLYAGGREGATED_UNGROUPED:
                case ResultSetProcessorType.AGGREGATED_UNGROUPED:
                    return false;

                case ResultSetProcessorType.FULLYAGGREGATED_GROUPED:
                case ResultSetProcessorType.FULLYAGGREGATED_GROUPED_ROLLUP:
                case ResultSetProcessorType.AGGREGATED_GROUPED:
                    return true;

                default:
                    throw new ArgumentException("invalid argument", nameof(value));
            }
        }

        public static bool IsUnaggregatedUngrouped(this ResultSetProcessorType value)
        {
            return !IsAggregated(value) && !IsGrouped(value);
        }
    }
} // end of namespace