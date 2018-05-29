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
    public class StmtNamedWindowSubqueryAggCallable : ICallable<bool> {
        private readonly EPServiceProvider _engine;
        private readonly int _numRepeats;
        private readonly int _threadNum;
        private readonly EPStatement _targetStatement;
    
        public StmtNamedWindowSubqueryAggCallable(int threadNum, EPServiceProvider engine, int numRepeats, EPStatement targetStatement) {
            this._numRepeats = numRepeats;
            this._threadNum = threadNum;
            this._engine = engine;
            this._targetStatement = targetStatement;
        }
    
        public bool Call() {
            try {
                var listener = new SupportMTUpdateListener();
                _targetStatement.Events += listener.Update;
    
                for (var loop = 0; loop < _numRepeats; loop++) {
                    var generalKey = "Key";
                    var valueExpected = _threadNum * 1000000000 + loop + 1;
    
                    // send insert event with string-value NOT specific to thread
                    SendEvent(generalKey, valueExpected);
    
                    // send subquery trigger event
                    _engine.EPRuntime.SendEvent(new SupportBean(generalKey, -1));
    
                    // assert trigger event received
                    var events = listener.NewDataListCopy;
                    var found = false;
                    foreach (var arr in events) {
                        foreach (var item in arr) {
                            var value = item.Get("val").Unwrap<int?>();
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
            } catch (Exception ex) {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            return true;
        }
    
        private void SendEvent(string key, int intupd) {
            var theEvent = new Dictionary<string, Object>();
            theEvent.Put("uekey", key);
            theEvent.Put("ueint", intupd);
            _engine.EPRuntime.SendEvent(theEvent, "UpdateEvent");
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
