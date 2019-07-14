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

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtListenerCreateStmtCallable : ICallable<object>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int numRoutes;
        private readonly int numThread;
        private readonly ISet<SupportMarketDataBean> routed;
        private readonly EPRuntime runtime;
        private readonly EPStatement statement;

        public StmtListenerCreateStmtCallable(
            int numThread,
            EPRuntime runtime,
            EPStatement statement,
            int numRoutes,
            ISet<SupportMarketDataBean> routed)
        {
            this.numThread = numThread;
            this.runtime = runtime;
            this.numRoutes = numRoutes;
            this.statement = statement;
            this.routed = routed;
        }

        public object Call()
        {
            try {
                // add listener to triggering statement
                var listener = new MyUpdateListener(runtime, numRoutes, numThread, routed);
                statement.AddListener(listener);
                Thread.Sleep(100); // wait to send trigger event, other threads receive all other's events

                runtime.EventService.SendEventBean(new SupportBean(), "SupportBean");
            }
            catch (AssertionException ex) {
                log.Error("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            catch (Exception ex) {
                log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }

            return true;
        }

        private class MyUpdateListener : UpdateListener
        {
            private readonly int numRepeats;
            private readonly long numThread;
            private readonly ISet<SupportMarketDataBean> routed;
            private readonly EPRuntime runtime;

            public MyUpdateListener(
                EPRuntime runtime,
                int numRepeats,
                int numThread,
                ISet<SupportMarketDataBean> routed)
            {
                this.runtime = runtime;
                this.numRepeats = numRepeats;
                this.numThread = numThread;
                this.routed = routed;
            }

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                for (var i = 0; i < numRepeats; i++) {
                    var theEvent = new SupportMarketDataBean(
                        "",
                        0,
                        numThread,
                        null);
                    runtime.EventService.RouteEventBean(theEvent, theEvent.GetType().Name);
                    routed.Add(theEvent);
                }
            }
        }
    }
} // end of namespace