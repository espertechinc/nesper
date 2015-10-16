///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    public class StmtListenerRouteCallable : ICallable<bool>
    {
        private readonly int _numThread;
        private readonly EPServiceProvider _engine;
        private readonly EPStatement _statement;
        private readonly int _numRepeats;
    
        public StmtListenerRouteCallable(int numThread, EPServiceProvider engine, EPStatement statement, int numRepeats)
        {
            _numThread = numThread;
            _engine = engine;
            _numRepeats = numRepeats;
            _statement = statement;
        }
    
        public bool Call()
        {
            try
            {
                for (int loop = 0; loop < _numRepeats; loop++)
                {
                    var listener = new MyUpdateListener(_engine, _numThread);
                    _statement.Events += listener.Update;
                    _engine.EPRuntime.SendEvent(new SupportBean());
                    _statement.Events -= listener.Update;
                    listener.AssertCalled();
                }
            }
            catch (AssertionException ex)
            {
                Log.Fatal("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Fatal("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            return true;
        }
    
        private class MyUpdateListener
        {
            private readonly EPServiceProvider _engine;
            private readonly int _numThread;
            private bool _isCalled;
    
            public MyUpdateListener(EPServiceProvider engine, int numThread)
            {
                _engine = engine;
                _numThread = numThread;
            }

            public void Update(Object sender, UpdateEventArgs e)
            {
                var oldData = e.OldEvents;
                var newData = e.NewEvents;

                _isCalled = true;
    
                // create statement for thread - this can be called multiple times as other threads send SupportBean
                EPStatement stmt = _engine.EPAdministrator.CreateEPL(
                        "select * from " + typeof(SupportMarketDataBean).FullName + " where Volume=" + _numThread);
                SupportMTUpdateListener listener = new SupportMTUpdateListener();
                stmt.Events += listener.Update;
    
                Object theEvent = new SupportMarketDataBean("", 0, (long) _numThread, null);
                _engine.EPRuntime.SendEvent(theEvent);
                stmt.Stop();
    
                EventBean[] eventsReceived = listener.GetNewDataListFlattened();
    
                bool found = false;
                for (int i = 0; i < eventsReceived.Length; i++)
                {
                    if (eventsReceived[i].Underlying == theEvent)
                    {
                        found = true;
                    }
                }
                Assert.IsTrue(found);
            }
    
            public void AssertCalled()
            {
                Assert.IsTrue(_isCalled);
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
