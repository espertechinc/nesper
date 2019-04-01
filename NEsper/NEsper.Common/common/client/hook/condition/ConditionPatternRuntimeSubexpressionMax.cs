///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.hook.condition
{
	/// <summary>
	/// Indicates that on the runtimelevel the followed-by pattern operator, regardless whether parameterized with a max number of sub-expressions or not,
	/// has reached the configured runtime-wide limit at runtime.
	/// </summary>
	public class ConditionPatternRuntimeSubexpressionMax : BaseCondition {
	    private readonly long max;
	    private readonly IDictionary<string, long?> counts;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="max">limit reached</param>
	    /// <param name="counts">the number of subexpression counts per statement</param>
	    public ConditionPatternRuntimeSubexpressionMax(long max, IDictionary<string, long?> counts) {
	        this.max = max;
	        this.counts = counts;
	    }

	    /// <summary>
	    /// Returns the limit reached.
	    /// </summary>
	    /// <returns>limit</returns>
	    public long Max {
	        get => max;	    }

	    /// <summary>
	    /// Returns the per-statement count.
	    /// </summary>
	    /// <returns>count</returns>
	    public IDictionary<string, long?> GetCounts() {
	        return counts;
	    }
	}
} // end of namespace