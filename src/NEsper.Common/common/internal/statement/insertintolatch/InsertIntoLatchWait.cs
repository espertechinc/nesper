///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.statement.insertintolatch
{
    /// <summary>
    ///     A suspend-and-notify implementation of a latch for use in guaranteeing delivery between
    ///     a single event produced by a single statement and consumable by another statement.
    /// </summary>
    public class InsertIntoLatchWait : InsertIntoLatch
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // The earlier latch is the latch generated before this latch
        private InsertIntoLatchWait earlier;
        private volatile bool isCompleted;

        // The later latch is the latch generated after this latch
        private InsertIntoLatchWait later;
        private readonly long msecTimeout;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="earlier">the latch before this latch that this latch should be waiting for</param>
        /// <param name="msecTimeout">the timeout after which delivery occurs</param>
        /// <param name="payload">the payload is an event to deliver</param>
        /// <param name="factory">the factory originating the latch</param>
        public InsertIntoLatchWait(
            InsertIntoLatchFactory factory,
            InsertIntoLatchWait earlier,
            long msecTimeout,
            EventBean payload)
        {
            Factory = factory;
            this.earlier = earlier;
            this.msecTimeout = msecTimeout;
            Event = payload;
        }

        /// <summary>
        ///     Ctor - use for the first and unused latch to indicate completion.
        /// </summary>
        /// <param name="factory">the latch factory</param>
        public InsertIntoLatchWait(InsertIntoLatchFactory factory)
        {
            Factory = factory;
            isCompleted = true;
            earlier = null;
            msecTimeout = 0;
        }

        /// <summary>
        ///     Returns true if the dispatch completed for this future.
        /// </summary>
        /// <value>true for completed, false if not</value>
        public bool IsCompleted => isCompleted;

        /// <summary>
        ///     Hand a later latch to use for indicating completion via notify.
        /// </summary>
        /// <value>is the later latch</value>
        public InsertIntoLatchWait Later {
            set => later = value;
        }

        public InsertIntoLatchFactory Factory { get; }

        public EventBean Event { set; get; }

        /// <summary>
        ///     Blcking call that returns only when the earlier latch completed.
        /// </summary>
        /// <returns>payload of the latch</returns>
        public EventBean Await()
        {
            if (!earlier.isCompleted) {
                lock (this) {
                    if (!earlier.isCompleted) {
                        try {
                            Monitor.Wait(this, (int)msecTimeout);
                        }
                        catch (ThreadInterruptedException e) {
                            Log.Error("Interrupted: " + e.Message, e);
                        }
                    }
                }
            }

            if (!earlier.isCompleted) {
                Log.Info("Wait timeout exceeded for insert-into dispatch with notify");
            }

            return Event;
        }

        /// <summary>
        ///     Called to indicate that the latch completed and a later latch can start.
        /// </summary>
        public void Done()
        {
            isCompleted = true;
            if (later != null) {
                lock (later) {
                    Monitor.Pulse(later);
                }
            }

            earlier = null;
            later = null;
        }
    }
} // end of namespace