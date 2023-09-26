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
    ///     A spin-locking implementation of a latch for use in guaranteeing delivery between
    ///     a single event produced by a single statement and consumable by another statement.
    /// </summary>
    public class InsertIntoLatchSpin : InsertIntoLatch
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // The earlier latch is the latch generated before this latch
        private readonly long msecTimeout;
        private InsertIntoLatchSpin earlier;

        private volatile bool isCompleted;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="factory">the latch factory</param>
        /// <param name="earlier">the latch before this latch that this latch should be waiting for</param>
        /// <param name="msecTimeout">the timeout after which delivery occurs</param>
        /// <param name="payload">the payload is an event to deliver</param>
        public InsertIntoLatchSpin(
            InsertIntoLatchFactory factory,
            InsertIntoLatchSpin earlier,
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
        public InsertIntoLatchSpin(InsertIntoLatchFactory factory)
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

        public InsertIntoLatchFactory Factory { get; }

        public EventBean Event { set; get; }

        /// <summary>
        ///     Blocking call that returns only when the earlier latch completed.
        /// </summary>
        /// <returns>payload of the latch</returns>
        public EventBean Await()
        {
            if (!earlier.isCompleted) {
                var spinStartTime = Factory.TimeSourceService.TimeMillis;

                while (!earlier.isCompleted) {
                    Thread.Yield();

                    var spinDelta = Factory.TimeSourceService.TimeMillis - spinStartTime;
                    if (spinDelta > msecTimeout) {
                        Log.Info(
                            "Spin wait timeout exceeded in insert-into dispatch at " +
                            msecTimeout +
                            "ms for " +
                            Factory.Name +
                            ", consider disabling insert-into between-statement latching for better performance");
                        break;
                    }
                }
            }

            return Event;
        }

        /// <summary>
        ///     Called to indicate that the latch completed and a later latch can start.
        /// </summary>
        public void Done()
        {
            isCompleted = true;
            earlier = null;
        }
    }
} // end of namespace