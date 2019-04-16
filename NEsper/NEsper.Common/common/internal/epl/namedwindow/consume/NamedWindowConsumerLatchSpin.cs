///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.namedwindow.consume
{
    /// <summary>
    /// A spin-locking implementation of a latch for use in guaranteeing delivery between
    /// a delta stream produced by a named window and consumable by another statement.
    /// </summary>
    public class NamedWindowConsumerLatchSpin : NamedWindowConsumerLatch
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // The earlier latch is the latch generated before this latch
        private NamedWindowConsumerLatchFactory factory;

        private NamedWindowConsumerLatchSpin earlier;

        private volatile bool isCompleted;

        public NamedWindowConsumerLatchSpin(
            NamedWindowDeltaData deltaData,
            IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> dispatchTo,
            NamedWindowConsumerLatchFactory factory,
            NamedWindowConsumerLatchSpin earlier)
            : base(deltaData, dispatchTo)
        {
            this.factory = factory;
            this.earlier = earlier;
        }

        public NamedWindowConsumerLatchSpin(NamedWindowConsumerLatchFactory factory)
            : base(null, null)
        {
            this.factory = factory;
            isCompleted = true;
            earlier = null;
        }

        public override NamedWindowConsumerLatch Earlier {
            get => earlier;
        }

        /// <summary>
        /// Returns true if the dispatch completed for this future.
        /// </summary>
        /// <returns>true for completed, false if not</returns>
        public bool IsCompleted {
            get => isCompleted;
        }

        /// <summary>
        /// Blocking call that returns only when the earlier latch completed.
        /// </summary>
        public override void Await()
        {
            if (earlier.isCompleted) {
                return;
            }

            long spinStartTime = factory.TimeSourceService.GetTimeMillis();
            while (!earlier.isCompleted) {
                Thread.Yield();
                long spinDelta = factory.TimeSourceService.GetTimeMillis() - spinStartTime;
                if (spinDelta > factory.MsecWait) {
                    Log.Info(
                        "Spin wait timeout exceeded in named window '" + factory.Name + "' consumer dispatch at " + factory.MsecWait + "ms for " +
                        factory.Name + ", consider disabling named window consumer dispatch latching for better performance");
                    break;
                }
            }
        }

        /// <summary>
        /// Called to indicate that the latch completed and a later latch can start.
        /// </summary>
        public override void Done()
        {
            isCompleted = true;
            earlier = null;
        }
    }
} // end of namespace