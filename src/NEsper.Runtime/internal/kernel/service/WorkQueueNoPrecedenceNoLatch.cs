///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class WorkQueueNoPrecedenceNoLatch : WorkQueue
	{

		private readonly ArrayDeque<EventBean> front = new ArrayDeque<EventBean>();
		private readonly ArrayDeque<EventBean> back = new ArrayDeque<EventBean>();

		public void Add(
			EventBean theEvent,
			EPStatementHandle epStatementHandle,
			bool addToFront,
			int precedence)
		{
			if (addToFront) {
				front.AddLast(theEvent);
			}
			else {
				back.AddLast(theEvent);
			}
		}

		public void Add(EventBean theEvent)
		{
			back.Add(theEvent);
		}

		public bool IsFrontEmpty => front.IsEmpty();

		public bool ProcessFront(EPEventServiceQueueProcessor epEventService)
		{
			return Process(front, epEventService);
		}

		public bool ProcessBack(EPEventServiceQueueProcessor epEventService)
		{
			return Process(back, epEventService);
		}

		private static bool Process(
			ArrayDeque<EventBean> queue,
			EPEventServiceQueueProcessor epEventService)
		{
			EventBean item = queue.Poll();
			if (item == null) {
				return false;
			}

			epEventService.ProcessThreadWorkQueueUnlatched(item);
			return true;
		}
	}
} // end of namespace
