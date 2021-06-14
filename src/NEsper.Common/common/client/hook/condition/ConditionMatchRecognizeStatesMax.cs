///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.client.hook.condition
{
    /// <summary>
    ///     Indicates that on the engine level the match-recognize has reached the configured engine-wide limit at runtime.
    /// </summary>
    public class ConditionMatchRecognizeStatesMax : BaseCondition
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="max">limit reached</param>
        /// <param name="counts">the number of state counts per statement</param>
        public ConditionMatchRecognizeStatesMax(
            long max,
            IDictionary<DeploymentIdNamePair, long> counts)
        {
            Max = max;
            Counts = counts;
        }

        /// <summary>
        ///     Returns the limit reached.
        /// </summary>
        /// <value>limit</value>
        public long Max { get; }

        /// <summary>
        ///     Returns the per-statement count.
        /// </summary>
        /// <value>count</value>
        public IDictionary<DeploymentIdNamePair, long> Counts { get; }
    }
} // end of namespace