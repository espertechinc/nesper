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

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtListenerAddRemoveCallable : ICallable<object>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly bool isEPL;
        private readonly int numRepeats;
        private readonly EPRuntime runtime;
        private readonly EPStatement stmt;

        public StmtListenerAddRemoveCallable(
            EPRuntime runtime,
            EPStatement stmt,
            bool isEPL,
            int numRepeats)
        {
            this.runtime = runtime;
            this.stmt = stmt;
            this.isEPL = isEPL;
            this.numRepeats = numRepeats;
        }

        public object Call()
        {
            try {
                for (var loop = 0; loop < numRepeats; loop++) {
                    // Add assertListener
                    var assertListener = new SupportMTUpdateListener();
                    LogUpdateListener logListener;
                    if (isEPL) {
                        logListener = new LogUpdateListener(null);
                    }
                    else {
                        logListener = new LogUpdateListener("a");
                    }

                    ThreadLogUtil.Trace("adding listeners ", assertListener, logListener);
                    stmt.AddListener(assertListener);
                    stmt.AddListener(logListener);

                    // send event
                    object theEvent = MakeEvent();
                    ThreadLogUtil.Trace("sending event ", theEvent);
                    runtime.EventService.SendEventBean(theEvent, theEvent.GetType().Name);

                    // Should have received one or more events, one of them must be mine
                    var newEvents = assertListener.GetNewDataListFlattened();
                    ThreadLogUtil.Trace("assert received, size is", newEvents.Length);
                    var found = false;
                    for (var i = 0; i < newEvents.Length; i++) {
                        var underlying = newEvents[i].Underlying;
                        if (!isEPL) {
                            underlying = newEvents[i].Get("a");
                        }

                        if (underlying == theEvent) {
                            found = true;
                        }
                    }

                    Assert.IsTrue(found);
                    assertListener.Reset();

                    // Remove assertListener
                    ThreadLogUtil.Trace("removing assertListener");
                    stmt.RemoveListener(assertListener);
                    stmt.RemoveListener(logListener);

                    // Send another event
                    theEvent = MakeEvent();
                    ThreadLogUtil.Trace("send non-matching event ", theEvent);
                    runtime.EventService.SendEventBean(theEvent, theEvent.GetType().Name);

                    // Make sure the event was not received
                    newEvents = assertListener.GetNewDataListFlattened();
                    found = false;
                    for (var i = 0; i < newEvents.Length; i++) {
                        var underlying = newEvents[i].Underlying;
                        if (!isEPL) {
                            underlying = newEvents[i].Get("a");
                        }

                        if (underlying == theEvent) {
                            found = true;
                        }
                    }

                    Assert.IsFalse(found);
                }
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

        private SupportMarketDataBean MakeEvent()
        {
            var theEvent = new SupportMarketDataBean("IBM", 50, 1000L, "RT");
            return theEvent;
        }
    }
} // end of namespace