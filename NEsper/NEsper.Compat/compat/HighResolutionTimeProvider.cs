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
    public class HighResolutionTimeProvider
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        public const long DriftAllowance = 5000000000L;

        public static readonly HighResolutionTimeProvider Instance = new HighResolutionTimeProvider();

        /// <summary>
        /// Gets the # of nano-seconds that were reported by DateTime.GetInstance
        /// when we reset the baseline.  It is used to determine the starting
        /// point from which all other performance measurements are calculated.
        /// </summary>
        public long BaseNano
        {
            get { return _baseNano; }
        }

        /// <summary>
        /// Gets the # of nano-seconds reported by NanoTime when we
        /// initialized the timer.
        /// </summary>
        /// <value>The base time.</value>
        public long BaseTime
        {
            get { return _baseTime; }
        }

        private long _frequency;

        /// <summary>
        /// Gets the current time.
        /// </summary>
        /// <value>The current time.</value>
        public long CurrentTime
        {
            get
            {
                long time;
                QueryPerformanceCounter(out time);
                double nanoTime = (time * 1000000000.0) / _frequency;
                if (nanoTime > _resetTime) {
                    ResetBaseline();
                }

                return (long) (_baseDelta + nanoTime);
            }
        }

        /// <summary>
        /// Represents the # of nano-seconds that were reported by DateTime.GetInstance
        /// when we reset the baseline.  It is used to determine the starting
        /// point from which all other performance measurements are calculated.
        /// </summary>
        private long _baseNano;
        /// <summary>
        /// Represents the # of nano-seconds reported by NanoTime when we
        /// initialized the timer.
        /// </summary>
        private long _baseTime;
        /// <summary>
        /// Represents the # of nano-seconds at which we will reset the baseline.
        /// This accounts for drift between the high resolution timer and the
        /// internal clock.
        /// </summary>
        private double _resetTime;

        private long _baseDelta;

        /// <summary>
        /// Initializes a new instance of the <see cref="HighResolutionTimeProvider"/> class.
        /// </summary>
        public HighResolutionTimeProvider()
        {
            ResetBaseline();
        }

        private void ResetBaseline()
        {
            long time;
            DateTime now = DateTime.Now;
            QueryPerformanceFrequency(out _frequency);
            QueryPerformanceCounter(out time);
            // ~1-5us can pass calling perf counter
            _baseNano = now.Ticks * 100;
            _baseTime = (long)(time * 1000000000.0m / _frequency);
            _baseDelta = _baseNano - _baseTime;
            _resetTime = _baseTime + DriftAllowance;
        }
    }
}
