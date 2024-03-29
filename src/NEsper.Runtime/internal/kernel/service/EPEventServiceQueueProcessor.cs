///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.statement.insertintolatch;



namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public interface EPEventServiceQueueProcessor {
	    void ProcessThreadWorkQueueLatchedWait(InsertIntoLatchWait insertIntoLatch);
	    void ProcessThreadWorkQueueLatchedSpin(InsertIntoLatchSpin insertIntoLatch);
	    void ProcessThreadWorkQueueUnlatched(object item);
	}
} // end of namespace
