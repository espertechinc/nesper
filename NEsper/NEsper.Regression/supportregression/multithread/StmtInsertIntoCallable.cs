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
    public class StmtInsertIntoCallable : ICallable<bool> {
        private readonly EPServiceProvider _engine;
        private readonly int _numRepeats;
        private readonly string _threadKey;
    
        public StmtInsertIntoCallable(string threadKey, EPServiceProvider engine, int numRepeats) {
            this._engine = engine;
            this._numRepeats = numRepeats;
            this._threadKey = threadKey;
        }
    
        public bool Call() {
            try {
                for (int loop = 0; loop < _numRepeats; loop++) {
                    var eventOne = new SupportBean();
                    eventOne.TheString = "E1_" + _threadKey;
                    _engine.EPRuntime.SendEvent(eventOne);
    
                    var eventTwo = new SupportMarketDataBean("E2_" + _threadKey, 0d, null, null);
                    _engine.EPRuntime.SendEvent(eventTwo);
                }
            } catch (Exception ex) {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            return true;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
