///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class WorkQueueUtil
	{
		public static int Insert(
			WorkQueueItemPrecedenced item,
			List<WorkQueueItemPrecedenced> queue)
		{
			int insertionIndex = queue.BinarySearch(item);
			if (insertionIndex < 0) {
				insertionIndex = -(insertionIndex + 1);
			}
			else {
				insertionIndex++;
			}

			// bump insertion index to get to last same-precedence item
			while (insertionIndex < queue.Count) {
				var atInsert = queue[insertionIndex];
				if (atInsert.Precedence == item.Precedence) {
					insertionIndex++;
				}
				else {
					break;
				}
			}

			if (insertionIndex >= queue.Count) {
				queue.Add(item);
			}
			else {
				queue.Insert(insertionIndex, item);
			}

			return insertionIndex;
		}
	}
} // end of namespace
