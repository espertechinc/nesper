///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.epl.util;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public enum AggregationMethodSortedFootprintEnum
    {
        KEYONLY,
        NOPARAM,
        SUBMAP
    }

    public static class AggregationMethodSortedFootprintEnumExtensions
    {
        private static readonly DotMethodFP[] FP_KEYONLY = new DotMethodFP[] {
            new DotMethodFP(DotMethodFPInputEnum.ANY, new DotMethodFPParam("the key value", EPLExpressionParamType.ANY))
        };

        private static readonly DotMethodFP[] FP_NOPARAM = new DotMethodFP[] {
            new DotMethodFP(DotMethodFPInputEnum.ANY)
        };

        private static readonly DotMethodFP[] FP_SUBMAP = new DotMethodFP[] {
            new DotMethodFP(
                DotMethodFPInputEnum.ANY,
                new DotMethodFPParam("the from-key value", EPLExpressionParamType.ANY),
                new DotMethodFPParam("the from-inclusive flag", EPLExpressionParamType.BOOLEAN),
                new DotMethodFPParam("the to-key value", EPLExpressionParamType.ANY),
                new DotMethodFPParam("the to-inclusive flag", EPLExpressionParamType.BOOLEAN))
        };

        public static DotMethodFP[] GetFP(this AggregationMethodSortedFootprintEnum value)
        {
            switch (value) {
                case AggregationMethodSortedFootprintEnum.KEYONLY:
                    return FP_KEYONLY;

                case AggregationMethodSortedFootprintEnum.NOPARAM:
                    return FP_NOPARAM;

                case AggregationMethodSortedFootprintEnum.SUBMAP:
                    return FP_SUBMAP;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
} // end of namespace