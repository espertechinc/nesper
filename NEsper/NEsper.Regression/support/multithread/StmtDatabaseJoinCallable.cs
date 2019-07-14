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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtDatabaseJoinCallable : ICallable<object>
    {
        private static readonly string[] MYVARCHAR_VALUES = {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J"
        };

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int numRepeats;
        private readonly EPRuntime runtime;
        private readonly EPStatement stmt;

        public StmtDatabaseJoinCallable(
            EPRuntime runtime,
            EPStatement stmt,
            int numRepeats)
        {
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
                    var intPrimitive = loop % 10 + 1;
                    var eventS0 = MakeEvent(intPrimitive);

                    runtime.EventService.SendEventBean(eventS0, "SupportBean");

                    // Should have received one that's mine, possible multiple since the statement is used by other threads
                    var found = false;
                    var events = assertListener.GetNewDataListFlattened();
                    foreach (var theEvent in events) {
                        var s0Received = theEvent.Get("s0");
                        var s1Received = (IDictionary<string, object>) theEvent.Get("s1");
                        if (s0Received == eventS0 ||
                            s1Received.Get("myvarchar").Equals(MYVARCHAR_VALUES[intPrimitive - 1])) {
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

        private SupportBean MakeEvent(int intPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            return theEvent;
        }
    }
} // end of namespace