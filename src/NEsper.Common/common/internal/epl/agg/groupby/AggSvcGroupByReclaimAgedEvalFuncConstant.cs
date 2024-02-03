///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;

namespace com.espertech.esper.common.@internal.epl.agg.groupby
{
    /// <summary>
    ///     Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByReclaimAgedEvalFuncConstant : AggSvcGroupByReclaimAgedEvalFunc
    {
        public AggSvcGroupByReclaimAgedEvalFuncConstant(double longValue)
        {
            LongValue = longValue;
        }

        public double? LongValue { get; }
    }
} // end of namespace