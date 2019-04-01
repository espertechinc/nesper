///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
	    AGGREGATED_GROUPED,
	}
} // end of namespace