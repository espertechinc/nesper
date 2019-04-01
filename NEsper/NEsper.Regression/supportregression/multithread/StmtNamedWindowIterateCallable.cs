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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.multithread
{
    public class StmtNamedWindowIterateCallable : ICallable<bool>
    {
        private readonly EPServiceProvider _engine;
        private readonly int _numRepeats;
        private readonly string _threadKey;
        private EPStatement _statement;
    
        public StmtNamedWindowIterateCallable(string threadKey, EPServiceProvider engine, int numRepeats) {
            this._engine = engine;
            this._numRepeats = numRepeats;
            this._threadKey = threadKey;
    
            _statement = engine.EPAdministrator.CreateEPL("select TheString, sum(LongPrimitive) as sumLong from MyWindow group by TheString");
        }
    
        public bool Call() {
            try {
                long total = 0;
                for (int loop = 0; loop < _numRepeats; loop++) {
                    // Insert event into named window
                    SendMarketBean(_threadKey, loop + 1);
                    total += loop + 1;
    
                    // iterate over private statement
                    var safeIter = _statement.GetSafeEnumerator();
                    EventBean[] received = EPAssertionUtil.EnumeratorToArray(safeIter);
                    safeIter.Dispose();
    
                    for (int i = 0; i < received.Length; i++) {
                        if (received[i].Get("TheString").Equals(_threadKey)) {
                            long sum = (long) received[i].Get("sumLong");
                            Assert.AreEqual(total, sum);
                        }
                    }
                }
            } catch (Exception ex) {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            return true;
        }
    
        private void SendMarketBean(string symbol, long volume) {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            _engine.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
