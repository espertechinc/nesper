///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
	/// <summary>
	/// Factory for aggregation multi-function aggregation method
	/// </summary>
	public interface AggregationMultiFunctionAggregationMethodFactory
	{
	    /// <summary>
	    /// Returns a new table reader
	    /// </summary>
	    /// <param name="context">contextual information</param>
	    /// <returns>table reader</returns>
	    AggregationMultiFunctionAggregationMethod NewMethod(AggregationMultiFunctionAggregationMethodFactoryContext context);
	}
} // end of namespace
