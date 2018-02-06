///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.named
{
    /// <summary>
    /// A spin-locking implementation of a latch for use in guaranteeing delivery between
    /// a delta stream produced by a named window and consumable by another statement.
    /// </summary>
    public class NamedWindowConsumerLatchSpin : NamedWindowConsumerLatch
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // The earlier latch is the latch generated before this latch
        private readonly NamedWindowConsumerLatchFactory _factory;
        private NamedWindowConsumerLatchSpin _earlier;

        private volatile bool _isCompleted;

#if DEBUG && DEVELOPMENT
        private Thread _execThread;
        private long _allocTime;
#endif

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="deltaData">The delta data.</param>
        /// <param name="dispatchTo">The dispatch to.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="earlier">the latch before this latch that this latch should be waiting for</param>
        public NamedWindowConsumerLatchSpin(NamedWindowDeltaData deltaData, IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> dispatchTo, NamedWindowConsumerLatchFactory factory, NamedWindowConsumerLatchSpin earlier)
            : base(deltaData, dispatchTo)
        {
            _factory = factory;
            _earlier = earlier;
#if DEBUG && DEVELOPMENT
            _allocTime = _factory.TimeSourceService.GetTimeMillis();
#endif
        }

        /// <summary>
        /// Ctor - use for the first and unused latch to indicate completion.
        /// </summary>
        public NamedWindowConsumerLatchSpin(NamedWindowConsumerLatchFactory factory)
            : base(null, null)
        {
            _factory = factory;
            _isCompleted = true;
            _earlier = null;
        }

#if DEBUG && DEVELOPMENT
        public int EarlierChainDepth
        {
            get => _earlier == null ? 0 : 1 + _earlier.EarlierChainDepth;
        }

        public NamedWindowConsumerLatch EarlierChainTail
        {
            get => _earlier != null ? _earlier.EarlierChainTail : this;
        }

        public string EarlierChainInfo
        {
            get
            {
                if (_earlier == null) return this.AllocThread.ManagedThreadId.ToString();
                return this.AllocThread.ManagedThreadId.ToString() + " > " + _earlier.EarlierChainInfo;
            }
        }

        public NamedWindowConsumerLatchSpin[] ScanCircularChain
        {
            get
            {
                var traversalSet = new HashSet<NamedWindowConsumerLatchSpin>();
                var traversalPath = new LinkedList<NamedWindowConsumerLatchSpin>();

                traversalSet.Add(this);
                traversalPath.AddLast(this);

                for(var next = _earlier; next != null; next = next._earlier)
                {
                    if (traversalSet.Contains(next))
                    {
                        break;
                    }

                    traversalSet.Add(next);
                    traversalPath.AddLast(next);
                }

                return traversalPath.ToArray();
            }
        }

        internal IList<string> AddChainIds(IList<string> collection)
        {
            collection.Add(Id.ToString());
            if (_earlier != null)
                _earlier.AddChainIds(collection);
            return collection;
        }

        public IList<string> EarlierChainIds
        {
            get => AddChainIds(new List<string>());
        }
#endif

        public override NamedWindowConsumerLatch Earlier => _earlier;

        /// <summary>
        /// Returns true if the dispatch completed for this future.
        /// </summary>
        /// <value>true for completed, false if not</value>
        public bool IsCompleted => _isCompleted;

        /// <summary>
        /// Blocking call that returns only when the earlier latch completed.
        /// </summary>
        /// <returns>unit of the latch</returns>
        public override void Await()
        {
#if DEBUG && DEVELOPMENT
            Log.Info("AWAIT: {0}", Id);
#endif

            if (_earlier._isCompleted)
            {
#if DEBUG && DEVELOPMENT
                long waitDeltaX = _factory.TimeSourceService.GetTimeMillis() - _allocTime;
                if (waitDeltaX > 10)
                {
                    Log.Info("Startup Time: {0} {1} ms", Id, waitDeltaX);
                }
#endif
                return;
            }

#if DEBUG && DEVELOPMENT
            _execThread = Thread.CurrentThread;
#endif

            long spinStartTime = _factory.TimeSourceService.GetTimeMillis();
            while (!_earlier._isCompleted)
            {
                Thread.Yield();
                long spinDelta = _factory.TimeSourceService.GetTimeMillis() - spinStartTime;
                if (spinDelta > _factory.MsecWait)
                {
                    Log.Info("Spin wait timeout exceeded in named window '{0}' consumer dispatch at {1}ms for {0}, consider disabling named window consumer dispatch latching for better performance", _factory.Name, _factory.MsecWait);
                    break;
                }
            }

#if DEBUG && DEVELOPMENT
            long spinDeltaX = _factory.TimeSourceService.GetTimeMillis() - spinStartTime;
            Log.Info("Acquisition Time: {0} {1} ms", Id, spinDeltaX);
#endif
        }

        /// <summary>
        /// Called to indicate that the latch completed and a later latch can start.
        /// </summary>
        public override void Done()
        {
#if DEBUG && DEVELOPMENT
            long timeDeltaX = _factory.TimeSourceService.GetTimeMillis() - _allocTime;
            if (timeDeltaX > 10)
            {
                Log.Info("DONE-ALARM: {0} - {1} ms", Id, timeDeltaX);
            }
            else
            {
                Log.Info("DONE: {0}", Id);
            }
#endif
            _isCompleted = true;
            _earlier = null;
        }
    }
} // end of namespace
