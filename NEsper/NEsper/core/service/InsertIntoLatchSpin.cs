///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// A spin-locking implementation of a latch for use in guaranteeing delivery between a single event produced by a single statement and consumable by another statement.
    /// </summary>
    public class InsertIntoLatchSpin
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        // The earlier latch is the latch generated before this latch
        private readonly InsertIntoLatchFactory _factory;
        private InsertIntoLatchSpin _earlier;
        private readonly long _msecTimeout;
        private readonly EventBean _payload;
    
        private bool _isCompleted;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="earlier">the latch before this latch that this latch should be waiting for</param>
        /// <param name="msecTimeout">the timeout after which delivery occurs</param>
        /// <param name="payload">the payload is an event to deliver</param>
        public InsertIntoLatchSpin(InsertIntoLatchFactory factory, InsertIntoLatchSpin earlier, long msecTimeout, EventBean payload)
        {
            _factory = factory;
            _earlier = earlier;
            _msecTimeout = msecTimeout;
            _payload = payload;
        }
    
        /// <summary>Ctor - use for the first and unused latch to indicate completion. </summary>
        public InsertIntoLatchSpin(InsertIntoLatchFactory factory)
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
    
        /// <summary>Blocking call that returns only when the earlier latch completed. </summary>
        /// <returns>payload of the latch</returns>
        public EventBean Await()
        {
            if (!_earlier._isCompleted)
            {
                long spinStartTime = _factory.TimeSourceService.GetTimeMillis();
    
                while(!_earlier._isCompleted)
                {
                    Thread.Yield();
    
                    long spinDelta = _factory.TimeSourceService.GetTimeMillis() - spinStartTime;
                    if (spinDelta > _msecTimeout)
                    {
                        Log.Info("Spin wait timeout exceeded in insert-into dispatch at " + _msecTimeout + "ms for " + _factory.Name + ", consider disabling insert-into between-statement latching for better performance");
                        break;
                    }
                }
            }
    
            return _payload;
        }
    
        /// <summary>Called to indicate that the latch completed and a later latch can start. </summary>
        public void Done()
        {
            _isCompleted = true;
            _earlier = null;
        }
    }
}
