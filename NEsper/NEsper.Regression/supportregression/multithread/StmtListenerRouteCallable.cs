///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.util;

namespace com.espertech.esper.supportregression.multithread
{
    public class StmtListenerRouteCallable : ICallable<bool>
    {
        private readonly int _numThread;
        private readonly EPServiceProvider _engine;
        private readonly EPStatement _statement;
        private readonly int _numRepeats;
    
        public StmtListenerRouteCallable(int numThread, EPServiceProvider engine, EPStatement statement, int numRepeats) {
            _numThread = numThread;
            _engine = engine;
            _numRepeats = numRepeats;
            _statement = statement;
        }
    
        public bool Call() {
            try {
                for (var loop = 0; loop < _numRepeats; loop++) {
                    var listener = new MyUpdateListener(_engine, _numThread);
                    _statement.Events += listener.Update;
                    _engine.EPRuntime.SendEvent(new SupportBean());
                    _statement.Events -= listener.Update;
                    listener.AssertCalled();
                }
            } catch (AssertionException ex) {
                Log.Error("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            } catch (Exception ex) {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            return true;
        }
    
        private class MyUpdateListener
        {
            private readonly EPServiceProvider _engine;
            private readonly int _numThread;
            private bool _isCalled;
    
            public MyUpdateListener(EPServiceProvider engine, int numThread) {
                _engine = engine;
                _numThread = numThread;
            }
    
            public void Update(object sender, UpdateEventArgs e)
            { 
                _isCalled = true;
    
                // create statement for thread - this can be called multiple times as other threads send SupportBean
                var stmt = _engine.EPAdministrator.CreateEPL(
                        "select * from " + typeof(SupportMarketDataBean).FullName + " where volume=" + _numThread);
                var listener = new SupportMTUpdateListener();
                stmt.Events += listener.Update;
    
                var theEvent = new SupportMarketDataBean("", 0, (long) _numThread, null);
                _engine.EPRuntime.SendEvent(theEvent);
                stmt.Stop();
    
                var eventsReceived = listener.GetNewDataListFlattened();
    
                var found = false;
                for (var i = 0; i < eventsReceived.Length; i++) {
                    if (eventsReceived[i].Underlying == theEvent) {
                        found = true;
                    }
                }
                Assert.IsTrue(found);
            }
    
            public void AssertCalled() {
                Assert.IsTrue(_isCalled);
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
