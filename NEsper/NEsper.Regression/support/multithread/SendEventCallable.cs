///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class SendEventCallable : ICallable<bool>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IEnumerator<object> events;
        private readonly EPRuntime runtime;
        private readonly int threadNum;

        public SendEventCallable(
            int threadNum,
            EPRuntime runtime,
            IEnumerator<object> events)
        {
            this.threadNum = threadNum;
            this.runtime = runtime;
            this.events = events;
        }

        public bool Call()
        {
            log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " starting");
            try {
                while (events.MoveNext()) {
                    var @event = events.Current;
                    runtime.EventService.SendEventBean(@event, @event.GetType().Name);
                }
            }
            catch (Exception ex) {
                log.Error("Error in thread " + threadNum, ex);
                return false;
            }

            log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " done");
            return true;
        }
    }
} // end of namespace