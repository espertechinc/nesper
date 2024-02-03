///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.statement.insertintolatch;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.runtime.@internal.kernel.service.WorkQueueNoPrecedenceMayLatch;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class WorkQueueWPrecedenceMayLatch : WorkQueue
	{

		private readonly List<WorkQueueItemPrecedenced> front = new List<WorkQueueItemPrecedenced>();
		private readonly List<WorkQueueItemPrecedenced> back = new List<WorkQueueItemPrecedenced>();

		public void Add(
			EventBean theEvent,
			EPStatementHandle epStatementHandle,
			bool addToFront,
			int precedence)
		{
			if (addToFront) {
				var latch = epStatementHandle.InsertIntoFrontLatchFactory.NewLatch(theEvent);
				var item = new WorkQueueItemPrecedenced(latch, precedence);
				Insert(item, front);
			}
			else {
				var latch = epStatementHandle.InsertIntoBackLatchFactory.NewLatch(theEvent);
				var item = new WorkQueueItemPrecedenced(latch, precedence);
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

		private static bool Process(
			List<WorkQueueItemPrecedenced> queue,
			EPEventServiceQueueProcessor epEventService)
		{
			if (queue.IsEmpty()) {
				return false;
			}

			var item = queue.DeleteAt(0);
			ProcessMayLatched(item.LatchOrBean, epEventService);
			return true;
		}

		private static void Insert(
			WorkQueueItemPrecedenced item,
			List<WorkQueueItemPrecedenced> queue)
		{
			var insertionIndex = WorkQueueUtil.Insert(item, queue);

			// Latch shuffling.
			// We are done if there is no latch for the current event
			ShuffleLatches(queue, item, insertionIndex);
		}

		private static void ShuffleLatches(
			List<WorkQueueItemPrecedenced> queue,
			WorkQueueItemPrecedenced inserted,
			int insertionIndex)
		{
			if (!(inserted.LatchOrBean is InsertIntoLatch insertIntoLatch)) {
				return;
			}

			var latchFactory = insertIntoLatch.Factory;

			var currentItem = inserted;
			for (var i = insertionIndex + 1; i < queue.Count; i++) {
				var olderItem = queue[i];
				if (!(olderItem.LatchOrBean is InsertIntoLatch olderItemLatch)) {
					continue;
				}

				if (olderItemLatch.Factory != latchFactory) {
					continue;
				}

				// swap latches keeping payload
				var olderItemPayload = olderItemLatch.Event;
				var currentItemPayload = ((InsertIntoLatch)(currentItem.LatchOrBean)).Event;
				olderItem.LatchOrBean = currentItem.LatchOrBean;
				currentItem.LatchOrBean = olderItemLatch;
				((InsertIntoLatch)(olderItem.LatchOrBean)).Event = olderItemPayload;
				((InsertIntoLatch)(currentItem.LatchOrBean)).Event = currentItemPayload;

				currentItem = olderItem;
			}
		}
	}
} // end of namespace
