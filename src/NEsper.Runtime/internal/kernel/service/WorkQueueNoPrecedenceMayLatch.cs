///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.statement.insertintolatch;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class WorkQueueNoPrecedenceMayLatch : WorkQueue
	{
		private readonly ArrayDeque<object> front = new ArrayDeque<object>();
		private readonly ArrayDeque<object> back = new ArrayDeque<object>();

		public void Add(
			EventBean theEvent,
			EPStatementHandle epStatementHandle,
			bool addToFront,
			int precedence)
		{
			if (addToFront) {
				var latch = epStatementHandle.InsertIntoFrontLatchFactory.NewLatch(theEvent);
				front.AddLast(latch);
			}
			else {
				var latch = epStatementHandle.InsertIntoBackLatchFactory.NewLatch(theEvent);
				back.AddLast(latch);
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
			ArrayDeque<object> queue,
			EPEventServiceQueueProcessor epEventService)
		{
			var item = queue.Poll();
			if (item == null) {
				return false;
			}

			ProcessMayLatched(item, epEventService);
			return true;
		}

		internal static void ProcessMayLatched(
			object item,
			EPEventServiceQueueProcessor epEventService)
		{
			if (item is InsertIntoLatchSpin spin) {
				epEventService.ProcessThreadWorkQueueLatchedSpin(spin);
			}
			else if (item is InsertIntoLatchWait wait) {
				epEventService.ProcessThreadWorkQueueLatchedWait(wait);
			}
			else {
				epEventService.ProcessThreadWorkQueueUnlatched(item);
			}
		}
	}
} // end of namespace
