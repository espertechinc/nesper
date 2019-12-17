///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

namespace com.espertech.esper.compat.collections
{
    public class LinkedBlockingQueue<T> : IBlockingQueue<T>
    {
        private readonly LinkedList<T> _queue;
        private readonly Object _queueLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedBlockingQueue&lt;T&gt;"/> class.
        /// </summary>
        public LinkedBlockingQueue()
        {
            _queue = new LinkedList<T>();
            _queueLock = new Object();
        }

        /// <summary>
        /// Gets the number of items in the queue.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                Monitor.Enter(_queueLock);

                try
                {
                    return _queue.Count;
                }
                finally
                {
                    Monitor.Exit(_queueLock);
                }
            }
        }

        public bool IsEmpty()
        {
            Monitor.Enter(_queueLock);

            try
            {
                return _queue.Count == 0;
            }
            finally
            {
                Monitor.Exit(_queueLock);
            }
        }

        /// <summary>
        /// Clears all items from the queue
        /// </summary>
        public void Clear()
        {
            Monitor.Enter(_queueLock);

            try
            {
                _queue.Clear();
            }
            finally
            {
                Monitor.Exit(_queueLock);
            }
        }

        /// <summary>
        /// Pushes an item onto the queue.  If the queue has reached
        /// capacity, the call will pend until the queue has space to
        /// receive the request.
        /// </summary>
        /// <param name="item"></param>
        public void Push(T item)
        {
            Monitor.Enter(_queueLock);

            try
            {
                _queue.AddLast(item);
                Monitor.Pulse(_queueLock);
            }
            finally
            {
                Monitor.Exit(_queueLock);
            }
        }

        /// <summary>
        /// Pops an item off the queue.  If there is nothing on the queue
        /// the call will pend until there is an item on the queue.
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            Monitor.Enter(_queueLock);
            try
            {
                for (;;)
                {
                    var first = _queue.First;
                    if (first != null)
                    {
                        var value = first.Value;
                        _queue.RemoveFirst();
                        return value;
                    }

                    Monitor.Wait(_queueLock);
                }
            }
            finally
            {
                Monitor.Exit(_queueLock);
            }
        }

        /// <summary>
        /// Pops an item off the queue.  If there is nothing on the queue
        /// the call will pend until there is an item on the queue or
        /// the timeout has expired.  If the timeout has expired, the
        /// method will return false.
        /// </summary>
        /// <param name="maxTimeoutInMillis">The max timeout in millis.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool Pop(int maxTimeoutInMillis, out T item)
        {
            long endTime = DateTimeHelper.CurrentTimeMillis + maxTimeoutInMillis;

            Monitor.Enter(_queueLock);
            try
            {
                for (;;)
                {
                    var first = _queue.First;
                    if (first != null)
                    {
                        var value = first.Value;
                        _queue.RemoveFirst();
                        item = value;
                        return true;
                    }

                    var nowTime = DateTimeHelper.CurrentTimeMillis;
                    if (nowTime >= endTime)
                    {
                        item = default(T);
                        return false;
                    }

                    Monitor.Wait(_queueLock, (int) (endTime - nowTime));
                }
            }
            finally
            {
                Monitor.Exit(_queueLock);
            }
        }
    }
}
