///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.statement.insertintolatch
{
    /// <summary>
    ///     Class to hold a current latch per statement that uses an insert-into stream (per statement and insert-into stream
    ///     relationship).
    /// </summary>
    public class InsertIntoLatchFactory
    {
        private readonly int msecWait;
        private readonly bool stateless;
        private readonly bool useSpin;

        private InsertIntoLatchSpin currentLatchSpin;
        private InsertIntoLatchWait currentLatchWait;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="name">the factory name</param>
        /// <param name="msecWait">the number of milliseconds latches will await maximally</param>
        /// <param name="locking">the blocking strategy to employ</param>
        /// <param name="timeSourceService">time source provider</param>
        /// <param name="stateless">indicator whether stateless</param>
        public InsertIntoLatchFactory(
            string name,
            bool stateless,
            int msecWait,
            Locking locking,
            TimeSourceService timeSourceService)
        {
            Name = name;
            this.msecWait = msecWait;
            TimeSourceService = timeSourceService;
            this.stateless = stateless;

            useSpin = locking == Locking.SPIN;

            // construct a completed latch as an initial root latch
            if (useSpin) {
                currentLatchSpin = new InsertIntoLatchSpin(this);
            }
            else {
                currentLatchWait = new InsertIntoLatchWait(this);
            }
        }

        public TimeSourceService TimeSourceService { get; }

        public string Name { get; }

        /// <summary>
        ///     Returns a new latch.
        ///     <para />
        ///     Need not be synchronized as there is one per statement and execution is during statement lock.
        /// </summary>
        /// <param name="payload">is the object returned by the await.</param>
        /// <returns>latch</returns>
        public object NewLatch(EventBean payload)
        {
            if (stateless) {
                return payload;
            }

            if (useSpin) {
                var nextLatch = new InsertIntoLatchSpin(this, currentLatchSpin, msecWait, payload);
                currentLatchSpin = nextLatch;
                return nextLatch;
            }
            else {
                var nextLatch = new InsertIntoLatchWait(currentLatchWait, msecWait, payload);
                currentLatchWait.Later = nextLatch;
                currentLatchWait = nextLatch;
                return nextLatch;
            }
        }
    }
} // end of namespace