///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtNamedWindowPriorityCallable : ICallable<object>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int numRepeats;
        private readonly EPRuntime runtime;
        private readonly int threadNum;

        public StmtNamedWindowPriorityCallable(
            int threadNum,
            EPRuntime runtime,
            int numRepeats)
        {
            this.threadNum = threadNum;
            this.runtime = runtime;
            this.numRepeats = numRepeats;
        }

        public object Call()
        {
            try {
                var offset = threadNum + 1000000;
                for (var i = 0; i < numRepeats; i++) {
                    runtime.EventService.SendEventBean(
                        new SupportBean_S0(i + offset, "c0_" + i + offset, "P01_" + i + offset),
                        "SupportBean_S0");
                    runtime.EventService.SendEventBean(
                        new SupportBean_S1(i + offset, "c0_" + i + offset, "x", "y"),
                        "SupportBean_S1");
                }
            }
            catch (Exception ex) {
                var stackTraceText = ex.StackTrace.ToString();
                var innerTraceText = ex.InnerException.StackTrace.ToString();

                lock (Console.Out) {
                    Console.WriteLine("> OuterException: " + ex.GetType().CleanName());
                    Console.WriteLine(stackTraceText);
                    Console.WriteLine();
                    Console.WriteLine("> InnerException: " + ex.InnerException.GetType().CleanName());
                    Console.WriteLine(innerTraceText);
                    Console.WriteLine();
                }

                log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }

            return true;
        }
    }
} // end of namespace