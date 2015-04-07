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
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.util;


using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    public class IsolateUnisolateCallable
    {
        private readonly int _threadNum;
        private readonly EPServiceProvider _engine;
        private readonly int _loopCount;
    
        public IsolateUnisolateCallable(int threadNum, EPServiceProvider engine, int loopCount)
        {
            _threadNum = threadNum;
            _engine = engine;
            _loopCount = loopCount;
        }
    
        public bool Call()
        {
            SupportMTUpdateListener listenerIsolated = new SupportMTUpdateListener();
            SupportMTUpdateListener listenerUnisolated = new SupportMTUpdateListener();
            EPStatement stmt = _engine.EPAdministrator.CreateEPL("select * from SupportBean");
    
            try
            {
                for (int i = 0; i < _loopCount; i++)
                {
                    EPServiceProviderIsolated isolated = _engine.GetEPServiceIsolated("i1");
                    isolated.EPAdministrator.AddStatement(stmt);
    
                    listenerIsolated.Reset();
                    stmt.Events += listenerIsolated.Update;
                    Object theEvent = new SupportBean();
                    //Console.Out.WriteLine("Sensing event : " + event + " by thread " + Thread.CurrentThread.ManagedThreadId);
                    isolated.EPRuntime.SendEvent(theEvent);
                    FindEvent(listenerIsolated, i, theEvent);
                    stmt.RemoveAllEventHandlers();
    
                    isolated.EPAdministrator.RemoveStatement(stmt);

                    stmt.Events += listenerUnisolated.Update;
                    theEvent = new SupportBean();
                    _engine.EPRuntime.SendEvent(theEvent);
                    FindEvent(listenerUnisolated, i, theEvent);
                    stmt.RemoveAllEventHandlers();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("Error in thread " + _threadNum, ex);
                return false;
            }
            return true;
        }
    
        private void FindEvent(SupportMTUpdateListener listener, int loop, Object theEvent)
        {
            var message = "Failed in loop " + loop + " threads " + Thread.CurrentThread;
            Assert.IsTrue(listener.IsInvoked(), message);
            var eventBeans = listener.GetNewDataListCopy();
            var found = false;
            foreach (EventBean[] events in eventBeans)
            {
                Assert.AreEqual(1, events.Length, message);
                if (events[0].Underlying == theEvent)
                {
                    found = true;
                }
            }
            Assert.IsTrue(found, message);
            listener.Reset();
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
