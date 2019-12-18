///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Threading;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Use this to coordinate an event that has multiple participants.
    /// To use it, have each participant increment the coordinator during
    /// their initialization and have each participant signal the coordinator
    /// when they are ready.  An application that is pending coordination
    /// should call the WaitAll() method to wait for all participants.
    /// </summary>

    public class EventCoordinator
    {
        private int numCounter = 0;
        private readonly Object subLock = new object();

        /// <summary>
        /// Signals this instance.
        /// </summary>
        public void Signal()
        {
            lock( subLock )
            {
                if ( --numCounter == 0 )
                {
                    Monitor.PulseAll(subLock);
                }
            }
        }

        /// <summary>
        /// Increments the counter.
        /// </summary>
        public void Increment()
        {
            lock( subLock )
            {
                numCounter++;
            }
        }

        /// <summary>
        /// Waits all.
        /// </summary>
        public void WaitAll()
        {
            lock( subLock )
            {
                while( numCounter != 0 )
                {
                    Monitor.Wait(subLock);
                }
            }
        }
    }
}
