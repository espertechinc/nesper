///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.supportunit.timer
{
    public class SupportTimerCallback
    {
        private long numInvoked = 0;
    
        public void HandleTimerEvent()
        {
            long current = Interlocked.Increment(ref numInvoked);
            Log.Debug(".timerCallback numInvoked=" + current + " thread=" + Thread.CurrentThread);
        }

        public long Count
        {
            get { return Interlocked.Read(ref numInvoked); }
        }

        public long GetAndResetCount()
        {
            long count = Interlocked.Exchange(ref numInvoked, 0L);
            return count;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
