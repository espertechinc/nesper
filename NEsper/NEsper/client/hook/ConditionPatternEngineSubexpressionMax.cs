///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// Indicates that on the engine level the followed-by pattern operator, regardless 
    /// whether parameterized with a max number of sub-expressions or not, has reached 
    /// the configured engine-wide limit at runtime.
    /// </summary>
    public class ConditionPatternEngineSubexpressionMax : BaseCondition
    {
        /// <summary>Ctor. </summary>
        /// <param name="max">limit reached</param>
        /// <param name="counts">the number of subexpression counts per statement</param>
        public ConditionPatternEngineSubexpressionMax(long max, IDictionary<String, long?> counts)
        {
            Max = max;
            Counts = counts;
        }

        /// <summary>Returns the limit reached. </summary>
        /// <value>limit</value>
        public long Max { get; private set; }

        /// <summary>Returns the per-statement count. </summary>
        /// <value>count</value>
        public IDictionary<string, long?> Counts { get; private set; }
    }
}
