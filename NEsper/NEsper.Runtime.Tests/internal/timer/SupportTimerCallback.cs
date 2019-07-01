///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.runtime.@internal.timer
{
    public class SupportTimerCallback : ITimerCallback
    {
        private AtomicLong numInvoked = new AtomicLong();

        public void TimerCallback()
        {
            var current = numInvoked.IncrementAndGet();
            log.Debug(".timerCallback numInvoked=" + current + " thread=" + Thread.CurrentThread);
        }

        public long Count
        {
            get => numInvoked.Get();
        }

        public long GetAndResetCount()
        {
            return numInvoked.GetAndSet(0);
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace