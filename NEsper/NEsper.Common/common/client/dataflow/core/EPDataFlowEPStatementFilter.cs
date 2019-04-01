using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
namespace com.espertech.esper.common.client.dataflow.core
{
	///////////////////////////////////////////////////////////////////////////////////////
	// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
	// http://esper.codehaus.org                                                          /
	// ---------------------------------------------------------------------------------- /
	// The software in this package is published under the terms of the GPL license       /
	// a copy of which has been included with this distribution in the license.txt file.  /
	///////////////////////////////////////////////////////////////////////////////////////

	
	/// <summary>
	/// Filter for use with the EPStatementSource operator.
	/// </summary>
	public interface EPDataFlowEPStatementFilter {
	    /// <summary>
	    /// Pass or skip the statement.
	    /// </summary>
	    /// <param name="statement">to test</param>
	    /// <returns>indicator whether to include (true) or exclude (false) the statement.</returns>
	    bool Pass(EPDataFlowEPStatementFilterContext statement);
	}
} // end of namespace