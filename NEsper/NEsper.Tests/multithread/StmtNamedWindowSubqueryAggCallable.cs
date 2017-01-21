///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.util;

namespace com.espertech.esper.multithread
{
    public class StmtNamedWindowSubqueryAggCallable 
        : ICallable<bool>
    {
        private readonly EPServiceProvider _engine;
        private readonly int _numRepeats;
        private readonly int _threadNum;
        private readonly EPStatement _targetStatement;
    
        public StmtNamedWindowSubqueryAggCallable(int threadNum, EPServiceProvider engine, int numRepeats, EPStatement targetStatement)
        {
            _numRepeats = numRepeats;
            _threadNum = threadNum;
            _engine = engine;
            _targetStatement = targetStatement;
        }
    
        public bool Call()
        {
            try
            {
                SupportMTUpdateListener listener = new SupportMTUpdateListener();
                _targetStatement.Events += listener.Update;
    
                for (int loop = 0; loop < _numRepeats; loop++)
                {
                    String generalKey = "Value";
                    int valueExpected = _threadNum * 1000000000 + loop + 1;
    
                    // send insert event with string-value NOT specific to thread
                    SendEvent(generalKey, valueExpected);
    
                    // send subquery trigger event
                    _engine.EPRuntime.SendEvent(new SupportBean(generalKey, -1));
    
                    // assert trigger event received
                    IList<EventBean[]> events = listener.GetNewDataListCopy();
                    bool found = false;
                    foreach (EventBean[] arr in events) {
                        foreach (EventBean item in arr) {
                            var value = (IList<int>) item.Get("val");
                            found = value.Any(valueReceived => valueReceived == valueExpected);
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
            catch (Exception ex)
            {
                Log.Fatal("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            return true;
        }
    
        private void SendEvent(String key, int intupd) {
            IDictionary<String,Object> theEvent = new Dictionary<String,Object>();
            theEvent["uekey"] = key;
            theEvent["ueint"] = intupd;
            _engine.EPRuntime.SendEvent(theEvent, "UpdateEvent");
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
