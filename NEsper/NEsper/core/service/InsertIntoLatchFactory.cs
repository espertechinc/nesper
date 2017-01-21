///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.timer;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Class to hold a current latch per statement that uses an insert-into stream (per statement and insert-into stream relationship).
    /// </summary>
    public class InsertIntoLatchFactory
    {
        private readonly String _name;
        private readonly bool _stateless;
        private readonly bool _useSpin;
        private readonly TimeSourceService _timeSourceService;
        private readonly long _msecWait;
    
        private InsertIntoLatchSpin _currentLatchSpin;
        private InsertIntoLatchWait _currentLatchWait;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">the factory name</param>
        /// <param name="stateless">if set to <c>true</c> [stateless].</param>
        /// <param name="msecWait">the number of milliseconds latches will await maximually</param>
        /// <param name="locking">the blocking strategy to employ</param>
        /// <param name="timeSourceService">time source provider</param>
        public InsertIntoLatchFactory(String name, bool stateless, long msecWait, ConfigurationEngineDefaults.Threading.Locking locking, TimeSourceService timeSourceService)
        {
            _name = name;
            _msecWait = msecWait;
            _timeSourceService = timeSourceService;
            _stateless = stateless;
    
            _useSpin = (locking == ConfigurationEngineDefaults.Threading.Locking.SPIN);
    
            // construct a completed latch as an initial root latch
            if (_useSpin)
            {
                _currentLatchSpin = new InsertIntoLatchSpin(this);
            }
            else
            {
                _currentLatchWait = new InsertIntoLatchWait(this);
            }
        }
    
        /// <summary>Returns a new latch.
        /// <para>
        /// Need not be synchronized as there is one per statement and execution is during statement lock.
        /// </para>
        /// </summary>
        /// <param name="payload">is the object returned by the await.</param>
        /// <returns>latch</returns>
        public Object NewLatch(EventBean payload)
        {
            if (_stateless)
            {
                return payload;
            }

            if (_useSpin)
            {
                var nextLatch = new InsertIntoLatchSpin(this, _currentLatchSpin, _msecWait, payload);
                _currentLatchSpin = nextLatch;
                return nextLatch;
            }
            else
            {
                var nextLatch = new InsertIntoLatchWait(_currentLatchWait, _msecWait, payload);
                _currentLatchWait.SetLater(nextLatch);
                _currentLatchWait = nextLatch;
                return nextLatch;
            }
        }

        public TimeSourceService TimeSourceService
        {
            get { return _timeSourceService; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
