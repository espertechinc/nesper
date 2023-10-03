///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public interface WorkQueue {
	    void Add(EventBean theEvent, EPStatementHandle epStatementHandle, bool addToFront, int precedence);
	    void Add(EventBean theEvent);
	    bool IsFrontEmpty { get; }
	    bool ProcessFront(EPEventServiceQueueProcessor epEventService);
	    bool ProcessBack(EPEventServiceQueueProcessor epEventService);
	}
} // end of namespace
