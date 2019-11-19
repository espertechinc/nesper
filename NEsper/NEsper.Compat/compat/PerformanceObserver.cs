///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compat
{
    public class PerformanceObserver
    {
        public static long NanoTime
        {
            get
            {
#if MONO
                return PerformanceObserverMono.NanoTime;
#else
                return PerformanceObserverWin.NanoTime;
#endif
            }
        }

        public static long MicroTime
        {
            get
            {
#if MONO
                return PerformanceObserverMono.MicroTime;
#else
                return PerformanceObserverWin.MicroTime;
#endif
            }
        }

        public static long MilliTime
        {
            get
            {
#if MONO
                return PerformanceObserverMono.MilliTime;
#else
                return PerformanceObserverWin.MilliTime;
#endif
            }
        }

        public static long TimeNano( Runnable r )
        {
#if MONO
            return PerformanceObserverMono.TimeNano(r);
#else
            return PerformanceObserverWin.TimeNano(r);
#endif
        }

        public static long TimeMicro(Runnable r)
        {
#if MONO
            return PerformanceObserverMono.TimeMicro(r);
#else
            return PerformanceObserverWin.TimeMicro(r);
#endif
        }

        public static long TimeMillis(Runnable r)
        {
#if MONO
            return PerformanceObserverMono.TimeMillis(r);
#else
            return PerformanceObserverWin.TimeMillis(r);
#endif
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
