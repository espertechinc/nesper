#if DEBUG
#define STATISTICS
#endif

using System;
using System.Threading;

namespace com.espertech.esper.compat.threading
{
	public class FifoReaderWriterLock 
		: IReaderWriterLock
		, IReaderWriterLockCommon<FifoReaderWriterLock.Node>
    {
        private Node _rnode;
        private Node _wnode;

#if STATISTICS
		private readonly string _id;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="FairReaderWriterLock"/> class.
        /// </summary>
        public FifoReaderWriterLock(int lockTimeout)
        {
#if STATISTICS
			_id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
#endif

            _wnode = _rnode = new Node(NodeFlags.None);

#if STATISTICS
			_wnode.LockId = _id;
#endif
            
            ReadLock = new CommonReadLock<Node>(this, lockTimeout);
            WriteLock = new CommonWriteLock<Node>(this, lockTimeout);
        }

        /// <summary>
        /// Gets the read-side lockable
        /// </summary>
        /// <value></value>
        public ILockable ReadLock { get; private set; }

        /// <summary>
        /// Gets the write-side lockable
        /// </summary>
        /// <value></value>
        public ILockable WriteLock { get; private set; }

        public IDisposable AcquireReadLock()
        {
            return ReadLock.Acquire();
        }

        public IDisposable AcquireWriteLock()
        {
            return WriteLock.Acquire();
        }

        /// <summary>
        /// Indicates if the writer lock is held.
        /// </summary>
        /// <value>
        /// The is writer lock held.
        /// </value>
        public bool IsWriterLockHeld
        {
            get
            {
                var node = _rnode;
                return
                    node != null &&
                    node.ThreadId == Thread.CurrentThread.ManagedThreadId &&
                    node.Flags == NodeFlags.Exclusive;
            }
        }

#if DEBUG
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FifoReaderWriterLock"/> is TRACE.
        /// </summary>
        /// <value><c>true</c> if TRACE; otherwise, <c>false</c>.</value>
        public bool Trace { get; set; }
#endif
        
        /// <summary>
        /// Pushes a node onto the end of the chain.
        /// </summary>
        /// <param name="node"></param>
        private Node PushNode(Node node)
        {
        	var curr = _wnode;

#if STATISTICS
			node.LockId = _id;
#endif
        	
            for (; ;)
            {
            	Node temp;
            	
            	while((temp = curr.Next) != null)
            	{
            		curr = temp; // temp is guaranteed to not be null
            	}
            	
            	var pnode = Interlocked.CompareExchange(
            		ref curr.Next,
            		node,
            		null);
            	if (pnode == null)
            	{
            		_wnode = node;
            		return node;
            	}
            }
        }

	    /// <summary>
	    /// Acquires the reader lock.
	    /// </summary>
	    /// <param name="timeout">The timeout.</param>
	    public Node AcquireReaderLock(long timeout)
        {
            var timeCur = DateTimeHelper.CurrentTimeMillis;
            var timeEnd = timeCur + timeout;

            var curr = _rnode;
            var node = PushNode(new Node(NodeFlags.Shared));
            var iter = 0;
            
            
            for( ;; )
            {
#if STATISTICS
				node.Iterations++;
#endif
            	if (curr == node) {
#if STATISTICS
					node.TimeAcquire = PerformanceObserver.MicroTime;
#endif
            		return _rnode = node;
            	} else if (curr.Flags == NodeFlags.Shared) {
            		curr = curr.Next;
#if STATISTICS
					node.ChainLength++;
#endif
            	} else if (curr.Flags == NodeFlags.Exclusive) {
                    SlimLock.SmartWait(++iter);
                } else if (curr.Flags == NodeFlags.None) {
            		curr = curr.Next; // dead node
#if STATISTICS
					node.ChainLength++;
#endif
            	} else {
            		throw new IllegalStateException();
            	}
            }
        }

	    /// <summary>
	    /// Acquires the writer lock.
	    /// </summary>
	    /// <param name="timeout">The timeout.</param>
	    public Node AcquireWriterLock(long timeout)
        {
        	var timeCur = DateTimeHelper.CurrentTimeMillis;
            var timeEnd = timeCur + timeout;

            var curr = _rnode;
            var node = PushNode(new Node(NodeFlags.Exclusive));
            var iter = 0;

            for( ;; )
            {
#if STATISTICS
				node.Iterations++;
#endif

				if (curr == node) {
#if STATISTICS
					node.TimeAcquire = PerformanceObserver.MicroTime;
#endif
            		return _rnode = node;
            	} else if (curr.Flags == NodeFlags.Shared) {
                    SlimLock.SmartWait(++iter);
            	} else if (curr.Flags == NodeFlags.Exclusive) {
                    SlimLock.SmartWait(++iter);
                } else if (curr.Flags == NodeFlags.None) {
            		iter = 0; // clear wait cycling
            		curr = curr.Next; // dead node
#if STATISTICS
					node.ChainLength++;
#endif
            	} else {
            		throw new IllegalStateException();
            	}
            }
        }

        /// <summary>
        /// Releases the reader lock.
        /// </summary>
        public void ReleaseReaderLock(Node node)
        {
#if STATISTICS
			node.TimeRelease = PerformanceObserver.MicroTime;
#endif

        	node.Flags = NodeFlags.None;
        	
#if (DEBUG && STATISTICS)
			node.DumpStatistics();
#endif
        }

        /// <summary>
        /// Releases the writer lock.
        /// </summary>
        public void ReleaseWriterLock(Node node)
        {
#if STATISTICS
			node.TimeRelease = PerformanceObserver.MicroTime;
#endif

			node.Flags = NodeFlags.None;

#if (DEBUG && STATISTICS)
			node.DumpStatistics();
#endif
        }

        internal enum NodeFlags
        {
        	None = 0,
        	Shared = 1,
        	Exclusive = 2,
        }

        public sealed class Node
		{
#if STATISTICS
			internal String LockId;
        	internal NodeFlags OrigFlags;
			internal long TimeRequest;
			internal long TimeAcquire;
			internal long TimeRelease;
			internal int ChainLength;
			internal int Iterations;
#endif

            internal int ThreadId;
            internal Node Next;
            internal NodeFlags Flags;

#if STATISTICS
            internal void DumpStatistics()
            {
            	#if false
            	if (Iterations > 10)
            	{
					System.Diagnostics.Debug.WriteLine("E:{0}:{1}:{2}:{3}:{4}:{5}",
						LockId,
						OrigFlags,
						Iterations,
						TimeAcquire - TimeRequest,
						TimeRelease - TimeRequest,
						TimeRelease - TimeAcquire
						);
            	}
            	#endif
        	}
#endif

            /// <summary>
            /// Initializes a new instance of a node
            /// </summary>
            /// <param name="flags"></param>
            internal Node(NodeFlags flags)
            {
                Flags = flags;
                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

#if STATISTICS
            	OrigFlags = flags;
				TimeRequest = PerformanceObserver.MicroTime;
#endif
            }
        }
    }
}
