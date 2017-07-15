///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// A spin-locking implementation of a latch for use in guaranteeing delivery between
    /// a single event produced by a single statement and consumable by another statement.
    /// </summary>
    public class InsertIntoLatchSpin {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        // The earlier latch is the latch generated before this latch
        private InsertIntoLatchFactory factory;
        private InsertIntoLatchSpin earlier;
        private long msecTimeout;
        private EventBean payload;
    
        private volatile bool isCompleted;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="factory">the latch factory</param>
        /// <param name="earlier">the latch before this latch that this latch should be waiting for</param>
        /// <param name="msecTimeout">the timeout after which delivery occurs</param>
        /// <param name="payload">the payload is an event to deliver</param>
        public InsertIntoLatchSpin(InsertIntoLatchFactory factory, InsertIntoLatchSpin earlier, long msecTimeout, EventBean payload) {
            this.factory = factory;
            this.earlier = earlier;
            this.msecTimeout = msecTimeout;
            this.payload = payload;
        }
    
        /// <summary>
        /// Ctor - use for the first and unused latch to indicate completion.
        /// </summary>
        /// <param name="factory">the latch factory</param>
        public InsertIntoLatchSpin(InsertIntoLatchFactory factory) {
            this.factory = factory;
            isCompleted = true;
            earlier = null;
            msecTimeout = 0;
        }
    
        /// <summary>
        /// Returns true if the dispatch completed for this future.
        /// </summary>
        /// <returns>true for completed, false if not</returns>
        public bool IsCompleted() {
            return isCompleted;
        }
    
        /// <summary>
        /// Blocking call that returns only when the earlier latch completed.
        /// </summary>
        /// <returns>payload of the latch</returns>
        public EventBean Await() {
            if (!earlier.isCompleted) {
                long spinStartTime = factory.TimeSourceService.TimeMillis;
    
                while (!earlier.isCompleted) {
                    Thread.Yield();
    
                    long spinDelta = factory.TimeSourceService.TimeMillis - spinStartTime;
                    if (spinDelta > msecTimeout) {
                        Log.Info("Spin wait timeout exceeded in insert-into dispatch at " + msecTimeout + "ms for " + factory.Name + ", consider disabling insert-into between-statement latching for better performance");
                        break;
                    }
                }
            }
    
            return payload;
        }
    
        /// <summary>
        /// Called to indicate that the latch completed and a later latch can start.
        /// </summary>
        public void Done() {
            isCompleted = true;
            earlier = null;
        }
    }
} // end of namespace
