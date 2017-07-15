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
using com.espertech.esper.timer;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Type to hold a current latch per statement that uses an insert-into stream (per statement and insert-into stream
    /// relationship).
    /// </summary>
    public class InsertIntoLatchFactory {
        private readonly string name;
        private readonly bool stateless;
        private readonly bool useSpin;
        private readonly TimeSourceService timeSourceService;
        private readonly long msecWait;
    
        private InsertIntoLatchSpin currentLatchSpin;
        private InsertIntoLatchWait currentLatchWait;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">the factory name</param>
        /// <param name="msecWait">the number of milliseconds latches will await maximually</param>
        /// <param name="locking">the blocking strategy to employ</param>
        /// <param name="timeSourceService">time source provider</param>
        /// <param name="stateless">indicator whether stateless</param>
        public InsertIntoLatchFactory(string name, bool stateless, long msecWait, ConfigurationEngineDefaults.ThreadingConfig.Locking locking,
                                      TimeSourceService timeSourceService) {
            this.name = name;
            this.msecWait = msecWait;
            this.timeSourceService = timeSourceService;
            this.stateless = stateless;
    
            useSpin = locking == ConfigurationEngineDefaults.ThreadingConfig.Locking.SPIN;
    
            // construct a completed latch as an initial root latch
            if (useSpin) {
                currentLatchSpin = new InsertIntoLatchSpin(this);
            } else {
                currentLatchWait = new InsertIntoLatchWait(this);
            }
        }
    
        /// <summary>
        /// Returns a new latch.
        /// <para>
        /// Need not be synchronized as there is one per statement and execution is during statement lock.
        /// </para>
        /// </summary>
        /// <param name="payload">is the object returned by the await.</param>
        /// <returns>latch</returns>
        public Object NewLatch(EventBean payload) {
            if (stateless) {
                return payload;
            }
            if (useSpin) {
                var nextLatch = new InsertIntoLatchSpin(this, currentLatchSpin, msecWait, payload);
                currentLatchSpin = nextLatch;
                return nextLatch;
            } else {
                var nextLatch = new InsertIntoLatchWait(currentLatchWait, msecWait, payload);
                currentLatchWait.Later = nextLatch;
                currentLatchWait = nextLatch;
                return nextLatch;
            }
        }
    
        public TimeSourceService GetTimeSourceService() {
            return timeSourceService;
        }
    
        public string GetName() {
            return name;
        }
    }
} // end of namespace
