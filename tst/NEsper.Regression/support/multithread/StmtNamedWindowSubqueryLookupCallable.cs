///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
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
    public class StmtNamedWindowSubqueryLookupCallable : ICallable<bool?>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly int _numRepeats;
        private readonly EPRuntime _runtime;
        private readonly EPStatement _targetStatement;
        private readonly int _threadNum;

        public StmtNamedWindowSubqueryLookupCallable(
            int threadNum,
            EPRuntime runtime,
            int numRepeats,
            EPStatement targetStatement)
        {
            _numRepeats = numRepeats;
            _threadNum = threadNum;
            _runtime = runtime;
            _targetStatement = targetStatement;
        }

        public bool? Call()
        {
            try {
                var listener = new SupportMTUpdateListener();
                _targetStatement.AddListener(listener);

                for (var loop = 0; loop < _numRepeats; loop++) {
                    var threadKey = "K" + loop + "_" + _threadNum;
                    var valueExpected = _threadNum * 1000000000 + loop + 1;

                    // send insert event with string-value specific to thread
                    SendEvent(threadKey, valueExpected);

                    // send subquery trigger event with string-value specific to thread
                    _runtime.EventService.SendEventBean(new SupportBean(threadKey, -1), "SupportBean");

                    // assert trigger event received
                    var events = listener.NewDataListCopy;
                    var found = false;
                    foreach (var arr in events) {
                        found = arr
                            .Select(item => item.Get("val").AsInt32())
                            .Any(value => value == valueExpected);
                        if (found) {
                            break;
                        }
                    }

                    listener.Reset();

                    if (!found) {
                        return false;
                    }

                    // send delete event with string-value specific to thread
                    SendEvent(threadKey, 0);
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
            theEvent.Put("key", key);
            theEvent.Put("intupd", intupd);
            _runtime.EventService.SendEventMap(theEvent, "MyUpdateEvent");
        }
    }
} // end of namespace