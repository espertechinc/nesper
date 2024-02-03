///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.util
{
    /// <summary>
    /// <para />When a thread throws an Exception that was not caught, a DeathRattleExceptionHandler will
    /// increment a counter signalling a thread has died and print out the name and stack trace of the
    /// thread.
    /// This makes it easy to build alerts on unexpected Thread deaths and fine grained used quickens
    /// debugging in production.
    /// <para />You can also set a DeathRattleExceptionHandler as the default exception handler on all threads,
    /// allowing you to get information on Threads you do not have direct control over.
    /// <para />Usage is straightforward:
    /// final Counter c = Metrics.newCounter(MyRunnable.class, "thread-deaths");
    /// Thread.UncaughtExceptionHandler exHandler = new DeathRattleExceptionHandler(c);
    /// final Thread myThread = new Thread(myRunnable, "MyRunnable");
    /// myThread.setUncaughtExceptionHandler(exHandler);
    /// <para />Setting the global default exception handler should be done first, like so:
    /// final Counter c = Metrics.newCounter(MyMainClass.class, "unhandled-thread-deaths");
    /// Thread.UncaughtExceptionHandler ohNoIDidntKnowAboutThis = new DeathRattleExceptionHandler(c);
    /// Thread.setDefaultUncaughtExceptionHandler(ohNoIDidntKnowAboutThis);
    /// </summary>
    public class DeathRattleExceptionHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Counter counter;

        /// <summary>
        /// Creates a new <seealso cref="DeathRattleExceptionHandler" /> with the given <seealso cref="Counter" />.
        /// </summary>
        /// <param name="counter">the <seealso cref="Counter" /> which will be used to record the number of uncaughtexceptions
        /// </param>
        public DeathRattleExceptionHandler(Counter counter)
        {
            this.counter = counter;
        }

        public void UncaughtException(Thread t, Exception e)
        {
            counter.Inc();
            Log.Error("Uncaught exception on thread " + t + ": " + e.Message, e);
        }
    }
} // end of namespace