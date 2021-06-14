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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtJoinCallable : ICallable<object>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int numRepeats;
        private readonly EPRuntime runtime;
        private readonly EPStatement stmt;
        private readonly int threadNum;

        public StmtJoinCallable(
            int threadNum,
            EPRuntime runtime,
            EPStatement stmt,
            int numRepeats)
        {
            this.threadNum = threadNum;
            this.runtime = runtime;
            this.stmt = stmt;
            this.numRepeats = numRepeats;
        }

        public object Call()
        {
            try {
                // Add assertListener
                var assertListener = new SupportMTUpdateListener();
                ThreadLogUtil.Trace("adding listeners ", assertListener);
                stmt.AddListener(assertListener);

                for (var loop = 0; loop < numRepeats; loop++) {
                    long id = threadNum * 100000000 + loop;
                    object eventS0 = MakeEvent("s0", id);
                    object eventS1 = MakeEvent("s1", id);

                    ThreadLogUtil.Trace("SENDING s0 event ", id, eventS0);
                    runtime.EventService.SendEventBean(eventS0, eventS0.GetType().Name);
                    ThreadLogUtil.Trace("SENDING s1 event ", id, eventS1);
                    runtime.EventService.SendEventBean(eventS1, eventS1.GetType().Name);

                    //ThreadLogUtil.info("sent", eventS0, eventS1);
                    // Should have received one that's mine, possible multiple since the statement is used by other threads
                    var found = false;
                    var events = assertListener.GetNewDataListFlattened();
                    foreach (var theEvent in events) {
                        var s0Received = theEvent.Get("S0");
                        var s1Received = theEvent.Get("S1");
                        //ThreadLogUtil.info("received", event.Get("S0"), event.Get("S1"));
                        if (s0Received == eventS0 && s1Received == eventS1) {
                            found = true;
                        }
                    }

                    if (!found) {
                    }

                    Assert.IsTrue(found);
                    assertListener.Reset();
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

        private SupportBean MakeEvent(
            string theString,
            long longPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.LongPrimitive = longPrimitive;
            theEvent.TheString = theString;
            return theEvent;
        }
    }
} // end of namespace