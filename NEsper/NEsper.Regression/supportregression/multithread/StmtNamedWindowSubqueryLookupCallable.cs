///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.multithread
{
    public class StmtNamedWindowSubqueryLookupCallable : ICallable<bool>
    {
        private readonly EPServiceProvider _engine;
        private readonly int _numRepeats;
        private readonly int _threadNum;
        private readonly EPStatement _targetStatement;
    
        public StmtNamedWindowSubqueryLookupCallable(int threadNum, EPServiceProvider engine, int numRepeats, EPStatement targetStatement) {
            this._numRepeats = numRepeats;
            this._threadNum = threadNum;
            this._engine = engine;
            this._targetStatement = targetStatement;
        }
    
        public bool Call() {
            try {
                var listener = new SupportMTUpdateListener();
                _targetStatement.Events += listener.Update;
    
                for (int loop = 0; loop < _numRepeats; loop++) {
                    string threadKey = "K" + loop + "_" + _threadNum;
                    int valueExpected = _threadNum * 1000000000 + loop + 1;
    
                    // send insert event with string-value specific to thread
                    SendEvent(threadKey, valueExpected);
    
                    // send subquery trigger event with string-value specific to thread
                    _engine.EPRuntime.SendEvent(new SupportBean(threadKey, -1));
    
                    // assert trigger event received
                    IList<EventBean[]> events = listener.NewDataListCopy;
                    bool found = false;
                    foreach (EventBean[] arr in events) {
                        foreach (EventBean item in arr) {
                            int? value = (int?) item.Get("val");
                            if (value == valueExpected) {
                                found = true;
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
                    SendEvent(threadKey, 0);
                }
            } catch (Exception ex) {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            return true;
        }
    
        private void SendEvent(string key, int intupd) {
            var theEvent = new Dictionary<string, Object>();
            theEvent.Put("key", key);
            theEvent.Put("intupd", intupd);
            _engine.EPRuntime.SendEvent(theEvent, "MyUpdateEvent");
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
