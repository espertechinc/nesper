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

using NUnit.Framework;

namespace com.espertech.esper.supportregression.multithread
{
    public class StmtNamedWindowConsumeCallable : ICallable<IList<string>> {
        private readonly EPServiceProvider _engine;
        private readonly int _numRepeats;
        private readonly string _threadKey;
    
        public StmtNamedWindowConsumeCallable(string threadKey, EPServiceProvider engine, int numRepeats) {
            this._engine = engine;
            this._numRepeats = numRepeats;
            this._threadKey = threadKey;
        }
    
        public IList<string> Call() {
            var eventKeys = new List<string>(_numRepeats);
            try {
                for (int loop = 0; loop < _numRepeats; loop++) {
                    // Insert event into named window
                    string theEvent = "E" + _threadKey + "_" + loop;
                    eventKeys.Add(theEvent);
                    SendMarketBean(theEvent, 0);
                }
            } catch (Exception ex) {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return null;
            }
            return eventKeys;
        }
    
        private void SendMarketBean(string symbol, long volume) {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            _engine.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
