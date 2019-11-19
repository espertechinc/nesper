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

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class SendEventWaitCallable : ICallable<object>
    {
        private static readonly ILog log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEnumerator<object> events;
        private readonly EPRuntime runtime;
        private readonly object sendLock;
        private readonly int threadNum;
        private bool isShutdown;

        public SendEventWaitCallable(
            int threadNum,
            EPRuntime runtime,
            object sendLock,
            IEnumerator<object> events)
        {
            this.threadNum = threadNum;
            this.runtime = runtime;
            this.events = events;
            this.sendLock = sendLock;
        }

        public void Shutdown() {
            isShutdown = true;
        }

        public object Call()
        {
            try {
                while (events.MoveNext() && !isShutdown) {
                    lock (sendLock) {
                        Monitor.Wait(sendLock);
                    }

                    ThreadLogUtil.Info("sending event");
                    var @event = events.Current;
                    runtime.EventService.SendEventBean(@event, @event.GetType().FullName);
                }
            }
            catch (Exception ex) {
                log.Error("Error in thread " + threadNum, ex);
                return false;
            }

            return true;
        }
    }
} // end of namespace