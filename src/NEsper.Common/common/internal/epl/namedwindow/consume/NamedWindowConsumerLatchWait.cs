///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    ///     A suspend-and-notify implementation of a latch for use in guaranteeing delivery between
    ///     a named window delta result and consumable by another statement.
    /// </summary>
    public class NamedWindowConsumerLatchWait : NamedWindowConsumerLatch
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // The earlier latch is the latch generated before this latch
        private readonly NamedWindowConsumerLatchFactory factory;

        private volatile bool isCompleted;

        // The later latch is the latch generated after this latch
        private NamedWindowConsumerLatchWait later;

        public NamedWindowConsumerLatchWait(
            NamedWindowDeltaData deltaData,
            IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> dispatchTo,
            NamedWindowConsumerLatchFactory factory,
            NamedWindowConsumerLatchWait earlier)
            : base(deltaData, dispatchTo)
        {
            this.factory = factory;
            EarlierLocal = earlier;
        }

        public NamedWindowConsumerLatchWait(NamedWindowConsumerLatchFactory factory)
            : base(null, null)
        {
            this.factory = factory;
            isCompleted = true;
            EarlierLocal = null;
        }

        public override NamedWindowConsumerLatch Earlier => EarlierLocal;

        public NamedWindowConsumerLatchWait EarlierLocal { get; private set; }

        /// <summary>
        ///     Returns true if the dispatch completed for this future.
        /// </summary>
        /// <returns>true for completed, false if not</returns>
        public bool IsCompleted => isCompleted;

        /// <summary>
        ///     Hand a later latch to use for indicating completion via notify.
        /// </summary>
        /// <value>is the later latch</value>
        public NamedWindowConsumerLatchWait Later {
            set => later = value;
        }

        /// <summary>
        ///     Blcking call that returns only when the earlier latch completed.
        /// </summary>
        public override void Await()
        {
            if (EarlierLocal.isCompleted) {
                return;
            }

            lock (this) {
                if (!EarlierLocal.isCompleted) {
                    try {
                        Monitor.Wait(this, factory.MsecWait);
                    }
                    catch (ThreadInterruptedException e) {
                        Log.Error("Interrupted: " + e.Message, e);
                    }
                }
            }

            if (!EarlierLocal.isCompleted) {
                Log.Info("Wait timeout exceeded for named window '" + "' consumer dispatch with notify");
            }
        }

        /// <summary>
        ///     Called to indicate that the latch completed and a later latch can start.
        /// </summary>
        public override void Done()
        {
            isCompleted = true;
            if (later != null) {
                lock (later) {
                    Monitor.Pulse(later);
                }
            }

            EarlierLocal = null;
            later = null;
        }
    }
} // end of namespace