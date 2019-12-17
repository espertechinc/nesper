///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Implementation of the performance observer turned for use on Windows.
    /// </summary>

    public class PerformanceObserverWin
    {
        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(out long lpFrequency);

        [DllImport("Kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        public static extern Int32 GetCurrentWin32ThreadId();

        public static long Frequency;
        public static double MpMilli;
        public static double MpMicro;
        public static double MpNano;
        
        public static int SpinIterationsPerMicro;

        static PerformanceObserverWin()
        {
            Calibrate();
        }

        public static void Calibrate()
        {
            QueryPerformanceFrequency(out Frequency);
            MpMilli = 1000.0 / Frequency;
            MpMicro = 1000000.0 / Frequency;
            MpNano = 1000000000.0 / Frequency;
            
            // Our goal is to increase the iterations until we get at least 100 microseconds of
            // actual spin latency.
            
            long numCounter = (long) (Frequency / 1000.0);
            
            for( int nn = 2 ;; nn *= 2 ) {
            	long timeA;
            	long timeB;
            	QueryPerformanceCounter(out timeA);
            	System.Threading.Thread.SpinWait(nn);
            	QueryPerformanceCounter(out timeB);
            	
            	var measured = timeB - timeA;
            	if (measured >= numCounter) {
            		// We have achieved at least 1000 microseconds of delay, now computer
            		// the number of iterations per microsecond.
            		var numMicros = measured * MpMicro;
            		SpinIterationsPerMicro = (int) (((double) nn) / numMicros);
					break;
            	}
            }
        }

        public static long GetCounter()
        {
            long counter;
            QueryPerformanceCounter(out counter);
            return counter;
        }

        public static long NanoTime
        {
            get
            {
                long time;
                QueryPerformanceCounter(out time);
                return (long)(time * MpNano);
            }
        }

        public static long MicroTime
        {
            get
            {
                long time;
                QueryPerformanceCounter(out time);
                return (long)(time * MpMicro);
            }
        }
        
        public static long MilliTime
        {
            get
            {
                long time;
                QueryPerformanceCounter(out time);
                return (long)(time * MpMilli);
            }
        }

        public static long TimeNano(Runnable r)
        {
            long timeA;
            long timeB;

            QueryPerformanceCounter(out timeA);
            r.Invoke();
            QueryPerformanceCounter(out timeB);
            return (long)((timeB - timeA) * MpNano);
        }

        public static long TimeMicro(Runnable r)
        {
            long timeA;
            long timeB;

            QueryPerformanceCounter(out timeA);
            r.Invoke();
            QueryPerformanceCounter(out timeB);
            return (long)((timeB - timeA) * MpMicro);
        }

        public static long TimeMillis(Runnable r)
        {
            long timeA;
            long timeB;

            QueryPerformanceCounter(out timeA);
            r.Invoke();
            QueryPerformanceCounter(out timeB);
            return (long)((timeB - timeA) * MpMilli);
        }
    }
}
