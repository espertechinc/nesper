///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.compat.collections
{
    /// <summary>
    /// ImperfectBlockingQueue is a blocking queue designed for very high performance exchanges
    /// between threads.  Multiple readers and writers can exchange information using the
    /// ImperfectBlockingQueue.  The design allows for a read node and a write node.  Both the
    /// read node and write node are assumed to be probablistically incorrect at any given time.
    /// Specifically, that means that the read node may have actually been processed and the
    /// write node may not actually be the tail.  Rather than attempting to correct for this 
    /// imperfection in the data structure, we leverage it.
    /// <para/>
    /// When a writer attempts to write to the tail, the tail uses an atomic compare-exchange
    /// to exchange the next node with the newly allocated node.  If the exchange fails, the 
    /// thread will iterate through the next member until it finds null and the cycle continue
    /// again with the atomic compare-exchange.  Using this method, the writer will succeed
    /// in writing to the tail atomically.  The write node does not need to accurately reflect
    /// the true end of tail, so adjusting the write node to the written node is "reasonably"
    /// accurate.
    /// <para/>
    /// When a reader attempts to read from the head, an atomic compare exchange is used to
    /// test against the "IsProcessed" field of the node.  If the node has been processed, then
    /// the reader moves on to the next node until it can successfully perform a CAS against
    /// the node.  If none can be found, the method will force a sleep to simulate a block.
    /// Once found, the reader extracts the value for return and sets the head equal to the
    /// node just read.  Again, since we're probablistic, this is fine.  Since we've successfully
    /// read from the node, we're assured that all nodes before us have been processed.  Being
    /// "reasonably" accurate with the read node is fine since the next reader will simply
    /// advance from this point.
    /// <para/>
    /// This class was tested against various concurrent reader/writer models was equal to or
    /// outperformed all other models in all cases.  However, it still appears that during
    /// tight iterations that there is about a 4-1 call ratio between CAS and the Push method
    /// which means there is still some efficiency to be squeezed out.
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public sealed class ImperfectBlockingQueue<T> : IBlockingQueue<T>
    {
        private long _count;
        private Node _rnode;
        private Node _wnode;

        private long _slowLockInterest;
        private readonly object _slowLock;

        private readonly long _maxLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImperfectBlockingQueue&lt;T&gt;"/> class.
        /// </summary>
        public ImperfectBlockingQueue()
            : this(int.MaxValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImperfectBlockingQueue&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="maxLength">Length of the max.</param>
        public ImperfectBlockingQueue(int maxLength)
        {
            _slowLockInterest = 0;
            _slowLock = new object();
            _count = 0;
            _wnode = _rnode = new Node(default(T)) { IsProcessed = 1 };
            _maxLength = maxLength;
        }

        /// <summary>
        /// Gets the number of items in the queue.
        /// </summary>
        /// <value>The count.</value>
        public int Count => (int)Interlocked.Read(ref _count);

        public bool IsEmpty()
        {
            return Interlocked.Read(ref _count) == 0L;
        }

        /// <summary>
        /// Clears all items from the queue
        /// </summary>
        public void Clear()
        {
            _rnode = _wnode = new Node(default(T)) { IsProcessed = 1 };
        }

        /// <summary>
        /// Pushes an item onto the queue.  If the queue has reached
        /// capacity, the call will pend until the queue has space to
        /// receive the request.
        /// </summary>
        /// <param name="item"></param>
        public void Push(T item)
        {
            if (_maxLength != int.MaxValue)
            {
                if (!BoundBlockingQueueOverride.IsEngaged)
                {
                    for (int ii = 0; Interlocked.Read(ref _count) > _maxLength;)
                    {
                        SlimLock.SmartWait(++ii);
                    }
                }
            }

            // Create the new node
            var node = new Node(item);
            // Get the write node for the thread
            Node branch = _wnode;

            for (; ;)
            {
                Node temp;
                while ((temp = branch.Next) != null)
                {
                    branch = temp; // temp is guaranteed to not be null
                }

                var pnode = Interlocked.CompareExchange(
                    ref branch.Next,
                    node,
                    null);
                if (pnode == null)
                {
                    _wnode = node;
                    // Check for threads that have been waiting a long time ... these
                    // threads will be using a slowLockInterest rather than a tight spin
                    // loop.
                    if (Interlocked.Read(ref _slowLockInterest) > 0)
                    {
                        lock (_slowLock)
                        {
                            Monitor.Pulse(_slowLock);
                        }
                    }
                    // Increment the counter
                    Interlocked.Increment(ref _count);
                    return;
                }
            }
        }

        /// <summary>
        /// Pops an item off the queue.  If there is nothing on the queue
        /// the call will pend until there is an item on the queue.
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            long iteration = 0;

            do
            {
                var node = _rnode;
                while (node.IsProcessed == 1)
                {
                    var temp = node.Next;
                    if (temp != null)
                    {
                        node = temp;
                        continue;
                    }

                    // Simulate a blocking event - but be careful.  The tighter the spin,
                    // the more cycles you steal from threads that may eventually have to
                    // push data.
                    //
                    // For iterations 1-3: SpinWait
                    // For iterations 4-10: Sleep(1)

                    iteration++;
                    if (iteration <= 3)
                    {
                        Thread.SpinWait(10);
                    }
                    else if (iteration <= 6)
                    {
                        Thread.Sleep(1);
                    }
                    else
                    {
                        lock (_slowLock)
                        {
                            Interlocked.Increment(ref _slowLockInterest);
                            Monitor.Wait(_slowLock, 200);
                            Interlocked.Decrement(ref _slowLockInterest);
                        }
                    }

                    // Node was being processed, recycle the loop
                }

                if (Interlocked.CompareExchange(ref node.IsProcessed, 1, 0) == 0)
                {
                    // found a node that has not been processed and we have
                    // obtained the IsProcessed flag for this node.  Set the rnode
                    // to the node we have just processed.  Worst case, its a little
                    // stale, but the Pop() will advance past stale nodes anyway.
                    _rnode = node;
                    Interlocked.Decrement(ref _count);
                    return node.Value;
                }
            } while (true);
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
            long endTime = Environment.TickCount + maxTimeoutInMillis;
            long iteration = 0;

            do {
                var node = _rnode;
                while (node.IsProcessed == 1) {
                    var temp = node.Next;
                    if (temp != null) {
                        node = temp;
                        continue;
                    }

                    // Simulate a blocking event - but be careful.  The tighter the spin,
                    // the more cycles you steal from threads that may eventually have to
                    // push data.
                    //
                    // For iterations 1-3: SpinWait
                    // For iterations 4-10: Sleep(1)

                    iteration++;
                    if ( iteration <= 3 ) {
                        Thread.SpinWait(10);
                    } else if (iteration <= 6 ) {
                        Thread.Sleep(1);
                    } else {
                        lock(_slowLock) {
                            Interlocked.Increment(ref _slowLockInterest);
                            Monitor.Wait(_slowLock, 200);
                            Interlocked.Decrement(ref _slowLockInterest);
                        }
                    }

                    // Node was being processed, recycle the loop
                    var nowTime = Environment.TickCount;
                    if (nowTime >= endTime)
                    {
                        item = default(T);
                        return false;
                    }
                }

                if (Interlocked.CompareExchange(ref node.IsProcessed, 1, 0) == 0) {
                    // found a node that has not been processed and we have
                    // obtained the IsProcessed flag for this node.  Set the rnode
                    // to the node we have just processed.  Worst case, its a little
                    // stale, but the Pop() will advance past stale nodes anyway.
                    _rnode = node;
                    Interlocked.Decrement(ref _count);
                    item = node.Value;
                    return true;
                }
            } while (true);
        }

        public sealed class Node
        {
            /// <summary>
            /// Indicates whether the node has been processed
            /// </summary>
            public int IsProcessed;
            /// <summary>
            /// Value at this node
            /// </summary>
            public T Value;
            /// <summary>
            /// Next node in list
            /// </summary>
            public Node Next;

            /// <summary>
            /// Initializes a new instance of the <see cref="ImperfectBlockingQueue&lt;T&gt;.Node"/> class.
            /// </summary>
            /// <param name="value">The value.</param>
            public Node(T value)
            {
                Value = value;
            }
        }
    }
}
