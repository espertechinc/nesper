///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Implementation of the performance observer turned for use on Windows.
    /// </summary>

    public class PerformanceObserverMono
    {
        public static long NanoTime
        {
            get { return DateTime.Now.Ticks*100; }
        }

        public static long MicroTime
        {
            get { return DateTime.Now.Ticks / 10; }
        }

        public static long MilliTime
        {
            get { return DateTime.Now.Ticks / 10000; }
        }
        
        public static long TimeNano(Runnable r)
        {
            long timeA = DateTime.Now.Ticks;
            r.Invoke();
            long timeB = DateTime.Now.Ticks;
            return 100*(timeB - timeA);
        }

        public static long TimeMicro(Runnable r)
        {
            long timeA = DateTime.Now.Ticks;
            r.Invoke();
            long timeB = DateTime.Now.Ticks;
            return (timeB - timeA)/10;
        }

        public static long TimeMillis(Runnable r)
        {
            long timeA = DateTime.Now.Ticks;
            r.Invoke();
            long timeB = DateTime.Now.Ticks;
            return (timeB - timeA)/10000;
        }
    }
}
