///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtNamedWindowSubqueryAggCallable : ICallable<bool?>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int numRepeats;
        private readonly EPRuntime runtime;
        private readonly EPStatement targetStatement;
        private readonly int threadNum;

        public StmtNamedWindowSubqueryAggCallable(
            int threadNum,
            EPRuntime runtime,
            int numRepeats,
            EPStatement targetStatement)
        {
            this.numRepeats = numRepeats;
            this.threadNum = threadNum;
            this.runtime = runtime;
            this.targetStatement = targetStatement;
        }

        public bool? Call()
        {
            try {
                var listener = new SupportMTUpdateListener();
                targetStatement.AddListener(listener);

                for (var loop = 0; loop < numRepeats; loop++) {
                    var generalKey = "Key";
                    var valueExpected = threadNum * 1000000000 + loop + 1;

                    // send insert event with string-value NOT specific to thread
                    SendEvent(generalKey, valueExpected);

                    // send subquery trigger event
                    runtime.EventService.SendEventBean(new SupportBean(generalKey, -1), "SupportBean");

                    // assert trigger event received
                    var events = listener.NewDataListCopy;
                    var found = false;
                    foreach (var arr in events) {
                        foreach (var item in arr) {
                            var value = item.Get("val").UnwrapIntoList<int>();
                            foreach (var valueReceived in value) {
                                if (valueReceived == valueExpected) {
                                    found = true;
                                    break;
                                }
                            }

                            if (found) {
                                break;
                            }
                        }

                        if (found) {
                            break;
                        }
                    }

                    listener.Reset();

                    if (!found) {
                        return false;
                    }

                    // send delete event with string-value specific to thread
                    SendEvent(generalKey, valueExpected);
                }
            }
            catch (Exception ex) {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }

            return true;
        }

        private void SendEvent(
            string key,
            int intupd)
        {
            IDictionary<string, object> theEvent = new Dictionary<string, object>();
            theEvent.Put("uekey", key);
            theEvent.Put("ueint", intupd);
            runtime.EventService.SendEventMap(theEvent, "UpdateEvent");
        }
    }
} // end of namespace