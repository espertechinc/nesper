///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.namedwindow.consume
{
    /// <summary>
    ///     Class to hold a current latch per named window.
    /// </summary>
    public class NamedWindowConsumerLatchFactory
    {
        internal readonly bool enabled;
        internal readonly int msecWait;
        internal readonly string name;
        internal readonly TimeSourceService timeSourceService;
        internal readonly bool useSpin;

        private NamedWindowConsumerLatchSpin currentLatchSpin;
        private NamedWindowConsumerLatchWait currentLatchWait;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="name">the factory name</param>
        /// <param name="msecWait">the number of milliseconds latches will await maximally</param>
        /// <param name="locking">the blocking strategy to employ</param>
        /// <param name="timeSourceService">time source provider</param>
        /// <param name="initializeNow">for initializing</param>
        /// <param name="enabled">for active indicator</param>
        public NamedWindowConsumerLatchFactory(
            string name,
            bool enabled,
            int msecWait,
            Locking locking,
            TimeSourceService timeSourceService,
            bool initializeNow)
        {
            this.name = name;
            this.enabled = enabled;
            this.msecWait = msecWait;
            this.timeSourceService = timeSourceService;

            useSpin = enabled && locking == Locking.SPIN;

            // construct a completed latch as an initial root latch
            if (initializeNow && useSpin) {
                currentLatchSpin = new NamedWindowConsumerLatchSpin(this);
            }
            else if (initializeNow && enabled) {
                currentLatchWait = new NamedWindowConsumerLatchWait(this);
            }
        }

        public TimeSourceService TimeSourceService => timeSourceService;

        public string Name => name;

        public int MsecWait => msecWait;

        /// <summary>
        ///     Returns a new latch.
        ///     <para />
        ///     Need not be synchronized as there is one per statement and execution is during statement lock.
        /// </summary>
        /// <param name="delta">the delta</param>
        /// <param name="consumers">consumers</param>
        /// <returns>latch</returns>
        public NamedWindowConsumerLatch NewLatch(
            NamedWindowDeltaData delta,
            IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> consumers)
        {
            if (useSpin) {
                var nextLatch = new NamedWindowConsumerLatchSpin(delta, consumers, this, currentLatchSpin);
                currentLatchSpin = nextLatch;
                return nextLatch;
            }

            if (enabled) {
                var nextLatch = new NamedWindowConsumerLatchWait(delta, consumers, this, currentLatchWait);
                currentLatchWait.Later = nextLatch;
                currentLatchWait = nextLatch;
                return nextLatch;
            }

            return new NamedWindowConsumerLatchNone(delta, consumers);
        }
    }
} // end of namespace