///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.threading;

namespace com.espertech.esper.collection
{
	/// <summary>
    /// Simple queue implementation based on a Linked List per thread.
	/// Objects can be added to the queue tail or queue head.
	/// </summary>

    public class ThreadWorkQueue
    {
        private readonly IThreadLocal<DualWorkQueue<Object>> _threadQueue;

        /// <summary>
        /// Gets the thread queue.
        /// </summary>
        /// <value>The thread queue.</value>
        public DualWorkQueue<Object> ThreadQueue
        {
            get { return _threadQueue.GetOrCreate(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadWorkQueue"/> class.
        /// </summary>
        public ThreadWorkQueue(IThreadLocalManager threadLocalManager)
        {
            _threadQueue = threadLocalManager.Create(
                () => new DualWorkQueue<object>());
        }

	    /// <summary>Adds event to the end of the event queue.</summary>
        /// <param name="ev">event to add</param>
        public void AddBack(Object ev)
        {
            DualWorkQueue<Object> queue = ThreadQueue;
            queue.BackQueue.AddLast(ev);
        }

        /// <summary>Adds event to the front of the queue.</summary>
        /// <param name="ev">event to add</param>
        public void AddFront(Object ev)
        {
            DualWorkQueue<Object> queue = ThreadQueue;
            queue.FrontQueue.AddLast(ev);
        }
    }
}
