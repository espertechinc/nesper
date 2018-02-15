///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.supportregression.multithread
{
    public class IsolateUnisolateCallable : ICallable<bool>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EPServiceProvider _engine;
        private readonly int _loopCount;
        private readonly int _threadNum;

        public IsolateUnisolateCallable(int threadNum, EPServiceProvider engine, int loopCount)
        {
            _threadNum = threadNum;
            _engine = engine;
            _loopCount = loopCount;
        }

        public bool Call()
        {
            var listenerIsolated = new SupportMTUpdateListener();
            var listenerUnisolated = new SupportMTUpdateListener();
            var stmt = _engine.EPAdministrator.CreateEPL("select * from SupportBean");

            try
            {
                for (var i = 0; i < _loopCount; i++)
                {
                    var isolated = _engine.GetEPServiceIsolated("i1");
                    isolated.EPAdministrator.AddStatement(stmt);

                    listenerIsolated.Reset();
                    stmt.Events += listenerIsolated.Update;
                    var theEvent = new SupportBean();
                    //Log.Info("Sensing event : " + event + " by thread " + Thread.CurrentThread.ManagedThreadId);
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
                Log.Error("Error in thread " + _threadNum, ex);
                return false;
            }

            return true;
        }

        private void FindEvent(SupportMTUpdateListener listener, int loop, object theEvent)
        {
            var message = "Failed in loop " + loop + " threads " + Thread.CurrentThread;
            Assert.IsTrue(listener.IsInvoked, message);
            var eventBeans = listener.NewDataListCopy;
            var found = false;
            foreach (var events in eventBeans)
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
    }
} // end of namespace