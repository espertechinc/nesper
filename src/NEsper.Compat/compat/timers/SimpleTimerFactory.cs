///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compat.timers
{
    /// <summary>
    /// Timer that uses the System.Threading.Timer (why cant we use System.Timers.Timer)
    /// </summary>

    public class SimpleTimerFactory : ITimerFactory
    {
        public ITimer CreateTimer(
            TimerCallback timerCallback,
            long offsetInMillis,
            long intervalInMillis)
        {
            return new SimpleTimer(timerCallback, offsetInMillis, intervalInMillis);
        }

        public class SimpleTimer : ITimer
        {
            private Timer _timer;

            internal SimpleTimer(
                TimerCallback timerCallback,
                long offsetInMillis,
                long intervalInMillis)
            {
                _timer = new Timer(
                    (state) => timerCallback(null),
                    null,
                    TimeSpan.FromMilliseconds(offsetInMillis),
                    TimeSpan.FromMilliseconds(intervalInMillis));
            }
            
            public void Dispose()
            {
                _timer?.Dispose();
                _timer = null;
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}