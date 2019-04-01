using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
namespace com.espertech.esper.common.client.hook.aggfunc
{
	///////////////////////////////////////////////////////////////////////////////////////
	// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
	// http://esper.codehaus.org                                                          /
	// ---------------------------------------------------------------------------------- /
	// The software in this package is published under the terms of the GPL license       /
	// a copy of which has been included with this distribution in the license.txt file.  /
	///////////////////////////////////////////////////////////////////////////////////////

	
	/// <summary>
	/// Interface to implement for factories of aggregation functions.
	/// </summary>
	public interface AggregationFunctionFactory {

	    /// <summary>
	    /// Make a new initalized aggregation state.
	    /// </summary>
	    /// <param name="ctx">contextual information</param>
	    /// <returns>initialized aggregator</returns>
	    AggregationFunction NewAggregator(AggregationFunctionFactoryContext ctx);
	}
} // end of namespace