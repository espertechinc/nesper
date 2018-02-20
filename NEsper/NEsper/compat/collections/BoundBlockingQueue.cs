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

using com.espertech.esper.compat.threading;

namespace com.espertech.esper.compat.collections
{
    public class BoundBlockingQueue<T> : IBlockingQueue<T>
    {
        private readonly LinkedList<T> _queue;
        private readonly ILockable _queueLock;
        private readonly Object _queuePopWaitHandle;
        private readonly Object _queuePushWaitHandle;
        private readonly int _maxCapacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundBlockingQueue&lt;T&gt;" /> class.
        /// </summary>
        /// <param name="maxCapacity">The max capacity.</param>
        /// <param name="lockTimeout">The lock timeout.</param>
        public BoundBlockingQueue(int maxCapacity, int lockTimeout)
        {
            _queue = new LinkedList<T>();
            _queueLock = new MonitorSpinLock(lockTimeout);
            _queuePopWaitHandle = new AutoResetEvent(false);
            _queuePushWaitHandle = new AutoResetEvent(false);
            _maxCapacity = maxCapacity;
        }

        /// <summary>
        /// Determines whether this instance is empty.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsEmpty()
        {
            using (_queueLock.Acquire())
            {
                return _queue.Count == 0;
            }
        }

        /// <summary>
        /// Gets the number of items in the queue.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                using (_queueLock.Acquire())
                {
                    return _queue.Count;
                }
            }
        }

        /// <summary>
        /// Clears all items from the queue
        /// </summary>
        public void Clear()
        {
            using (_queueLock.Acquire())
            {
                _queue.Clear();

                PulsePushHandle();
                PulsePopHandle();

                //_queuePushWaitHandle.Insert();  // Push is clear
                //_queuePopWaitHandle.Reset(); // Pop now waits
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
            while (true)
            {
                using (_queueLock.Acquire()) {
                    if ((_queue.Count < _maxCapacity) || BoundBlockingQueueOverride.IsEngaged)
                    {
                        _queue.AddLast(item);
                        PulsePopHandle();
                        return;
                    }
                }

                WaitPushHandle();

                //_queuePushWaitHandle.WaitOne();
            }
        }

        /// <summary>
        /// Pops an item off the queue.  If there is nothing on the queue
        /// the call will pend until there is an item on the queue.
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            while (true)
            {
                using (_queueLock.Acquire())
                {
                    var first = _queue.First;
                    if (first != null)
                    {
                        var value = first.Value;
                        _queue.RemoveFirst();

                        PulsePushHandle();

                        //_queuePushWaitHandle.Insert(); // Push is clear
                        return value;
                    }
                }

                WaitPopHandle();

                //_queuePopWaitHandle.WaitOne();
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

            do {
                using (_queueLock.Acquire()) {
                    var first = _queue.First;
                    if (first != null) {
                        var value = first.Value;
                        _queue.RemoveFirst();

                        PulsePushHandle();

                        item = value;
                        return true;
                    }
                }

                var nowTime = DateTimeHelper.CurrentTimeMillis;
                if (nowTime >= endTime) {
                    item = default(T);
                    return false;
                }

                WaitPopHandle((int) (endTime - nowTime));
            } while (true);
        }

        private void PulsePushHandle()
        {
            Monitor.Enter(_queuePushWaitHandle);
            Monitor.PulseAll(_queuePushWaitHandle);
            Monitor.Exit(_queuePushWaitHandle);
        }

        private void PulsePopHandle()
        {
            Monitor.Enter(_queuePopWaitHandle);
            Monitor.PulseAll(_queuePopWaitHandle);
            Monitor.Exit(_queuePopWaitHandle);
        }

        private void WaitPushHandle()
        {
            Monitor.Enter(_queuePushWaitHandle);
            Monitor.Wait(_queuePushWaitHandle);
            Monitor.Exit(_queuePushWaitHandle);
        }

        private void WaitPopHandle()
        {
            Monitor.Enter(_queuePopWaitHandle);
            Monitor.Wait(_queuePopWaitHandle);
            Monitor.Exit(_queuePopWaitHandle);
        }

        private void WaitPopHandle(int timeToWait)
        {
            Monitor.Enter(_queuePopWaitHandle);
            Monitor.Wait(_queuePopWaitHandle, timeToWait);
            Monitor.Exit(_queuePopWaitHandle);
        }
    }
}
