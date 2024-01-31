///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compat
{
    public class PerformanceObserver
    {
        public static long NanoTime => PerformanceObserverCore.NanoTime;

        public static long MicroTime => PerformanceObserverCore.MicroTime;

        public static long MilliTime => PerformanceObserverCore.MilliTime;

        public static long TimeNano( Runnable r )
        {
            return PerformanceObserverCore.TimeNano(r);
        }

        public static long TimeMicro(Runnable r)
        {
            return PerformanceObserverCore.TimeMicro(r);
        }

        public static long TimeMillis(Runnable r)
        {
            return PerformanceObserverCore.TimeMillis(r);
        }

        public static long GetTimeMillis()
        {
            return MilliTime;
        }

        public static long GetTimeMicros()
        {
            return MicroTime;
        }

        public static long GetTimeNanos()
        {
            return NanoTime;
        }
    }
}
