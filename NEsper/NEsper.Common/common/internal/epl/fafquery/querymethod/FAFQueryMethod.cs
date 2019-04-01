///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
	/// <summary>
	/// FAF query execute.
	/// </summary>
	public interface FAFQueryMethod {
	    void Ready();

	    EPPreparedQueryResult Execute(AtomicBoolean serviceStatusProvider, FAFQueryMethodAssignerSetter assignerSetter, ContextPartitionSelector[] contextPartitionSelectors, ContextManagementService contextManagementService);

	    EventType EventType { get; }
	}
} // end of namespace