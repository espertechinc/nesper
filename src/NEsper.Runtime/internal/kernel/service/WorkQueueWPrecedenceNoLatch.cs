///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class WorkQueueWPrecedenceNoLatch : WorkQueue
	{
		private readonly List<WorkQueueItemPrecedenced> front = new List<WorkQueueItemPrecedenced>();
		private readonly List<WorkQueueItemPrecedenced> back = new List<WorkQueueItemPrecedenced>();

		public void Add(
			EventBean theEvent,
			EPStatementHandle epStatementHandle,
			bool addToFront,
			int precedence)
		{
			WorkQueueItemPrecedenced item = new WorkQueueItemPrecedenced(theEvent, precedence);
			if (addToFront) {
				Insert(item, front);
			}
			else {
				Insert(item, back);
			}
		}

		public void Add(EventBean theEvent)
		{
			back.Add(new WorkQueueItemPrecedenced(theEvent, 0));
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

		private static void Insert(
			WorkQueueItemPrecedenced item,
			List<WorkQueueItemPrecedenced> queue)
		{
			WorkQueueUtil.Insert(item, queue);
		}

		private static bool Process(
			List<WorkQueueItemPrecedenced> queue,
			EPEventServiceQueueProcessor epEventService)
		{
			if (queue.IsEmpty()) {
				return false;
			}

			var item = queue.DeleteAt(0);
			epEventService.ProcessThreadWorkQueueUnlatched(item.LatchOrBean);
			return true;
		}
	}
} // end of namespace
