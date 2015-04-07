///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.core.service
{
    /// <summary>A suspend-and-notify implementation of a latch for use in guaranteeing delivery between a single event produced by a single statement and consumable by another statement. </summary>
    public class InsertIntoLatchWait
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // The earlier latch is the latch generated before this latch
        private InsertIntoLatchFactory _factory;
        private InsertIntoLatchWait _earlier;
        private readonly int _msecTimeout;
        private readonly EventBean _payload;

        // The later latch is the latch generated after this latch
        private volatile bool _isCompleted;
        private InsertIntoLatchWait _later;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="earlier">the latch before this latch that this latch should be waiting for</param>
        /// <param name="msecTimeout">the timeout after which delivery occurs</param>
        /// <param name="payload">the payload is an event to deliver</param>
        public InsertIntoLatchWait(InsertIntoLatchFactory factory, InsertIntoLatchWait earlier, long msecTimeout, EventBean payload)
        {
            _factory = factory;
            _earlier = earlier;
            _msecTimeout = (int) msecTimeout;
            _payload = payload;
        }

        /// <summary>Ctor - use for the first and unused latch to indicate completion. </summary>
        public InsertIntoLatchWait(InsertIntoLatchFactory factory)
        {
            _factory = factory;
            _isCompleted = true;
            _earlier = null;
            _msecTimeout = 0;
        }

        /// <summary>Returns true if the dispatch completed for this future. </summary>
        /// <returns>true for completed, false if not</returns>
        public bool IsCompleted()
        {
            return _isCompleted;
        }

        /// <summary>Hand a later latch to use for indicating completion via notify. </summary>
        /// <param name="later">is the later latch</param>
        public void SetLater(InsertIntoLatchWait later)
        {
            _later = later;
        }

        /// <summary>Blcking call that returns only when the earlier latch completed. </summary>
        /// <returns>payload of the latch</returns>
        public EventBean Await()
        {
            if (!_earlier._isCompleted)
            {
                lock(this)
                {
                    if (!_earlier._isCompleted)
                    {
                        Monitor.Wait(this, _msecTimeout);
                    }
                }
            }

            if (!_earlier._isCompleted)
            {
                Log.Info("Wait timeout exceeded for insert-into dispatch with notify");
            }

            return _payload;
        }

        /// <summary>Called to indicate that the latch completed and a later latch can start. </summary>
        public void Done()
        {
            _isCompleted = true;
            if (_later != null)
            {
                lock(_later)
                {
                    Monitor.Pulse(_later);
                }
            }
            _earlier = null;
            _later = null;
        }
    }
}