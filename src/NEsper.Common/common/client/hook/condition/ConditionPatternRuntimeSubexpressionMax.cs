///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.client.hook.condition
{
    /// <summary>
    ///     Indicates that on the runtime level the followed-by pattern operator, regardless
    ///     whether parameterized with a max number of sub-expressions or not,
    ///     has reached the configured runtime-wide limit at runtime.
    /// </summary>
    public class ConditionPatternRuntimeSubexpressionMax : BaseCondition
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="max">limit reached</param>
        /// <param name="counts">the number of subexpression counts per statement</param>
        public ConditionPatternRuntimeSubexpressionMax(
            long max,
            IDictionary<string, long> counts)
        {
            Max = max;
            Counts = counts;
        }

        /// <summary>
        ///     Returns the limit reached.
        /// </summary>
        /// <returns>limit</returns>
        public long Max { get; }

        /// <summary>
        ///     Returns the per-statement count.
        /// </summary>
        /// <value>count</value>
        public IDictionary<string, long> Counts { get; }
    }
} // end of namespace