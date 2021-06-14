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
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtNamedWindowMergeCallable : ICallable<bool?>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int numEvents;
        private readonly EPEventServiceSPI runtime;

        public StmtNamedWindowMergeCallable(
            EPRuntime runtime,
            int numEvents)
        {
            this.runtime = (EPEventServiceSPI) runtime.EventService;
            this.numEvents = numEvents;
        }

        public bool? Call()
        {
            var start = PerformanceObserver.MilliTime;
            try {
                for (var i = 0; i < numEvents; i++) {
                    ((EPEventServiceSendEvent) runtime).SendEventBean(
                        new SupportBean("E" + Convert.ToString(i), 0),
                        "SupportBean");
                }
            }
            catch (Exception ex) {
                log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return null;
            }

            var end = PerformanceObserver.MilliTime;
            return true;
        }
    }
} // end of namespace