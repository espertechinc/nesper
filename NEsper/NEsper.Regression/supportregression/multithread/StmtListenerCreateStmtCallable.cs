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

namespace com.espertech.esper.supportregression.multithread
{
    public class StmtListenerCreateStmtCallable : ICallable<bool>
    {
        private readonly int _numThread;
        private readonly EPServiceProvider _engine;
        private readonly EPStatement _statement;
        private readonly int _numRoutes;
        private readonly ISet<SupportMarketDataBean> _routed;

        public StmtListenerCreateStmtCallable(
            int numThread,
            EPServiceProvider engine,
            EPStatement statement,
            int numRoutes,
            ISet<SupportMarketDataBean> routed)
        {
            _numThread = numThread;
            _engine = engine;
            _numRoutes = numRoutes;
            _statement = statement;
            _routed = routed;
        }

        public bool Call()
        {
            try
            {
                // add listener to triggering statement
                var listener = new MyUpdateListener(_engine, _numRoutes, _routed, _numThread);
                _statement.Events += listener.Update;
                Thread.Sleep(100); // wait to send trigger event, other threads receive all other's events

                _engine.EPRuntime.SendEvent(new SupportBean());

            }
            catch (AssertionException ex)
            {
                Log.Error("Assertion error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }

            return true;
        }

        private class MyUpdateListener
        {
            private readonly int _numThread;
            private readonly EPServiceProvider _engine;
            private readonly int _numRepeats;
            private readonly ISet<SupportMarketDataBean> _routed;

            public MyUpdateListener(
                EPServiceProvider engine,
                int numRepeats,
                ISet<SupportMarketDataBean> routed,
                int numThread)
            {
                _engine = engine;
                _numRepeats = numRepeats;
                _routed = routed;
                _numThread = numThread;
            }

            public void Update(object sender, UpdateEventArgs args)
            {
                for (int i = 0; i < _numRepeats; i++)
                {
                    var theEvent = new SupportMarketDataBean("", 0, (long) _numThread, null);
                    _engine.EPRuntime.Route(theEvent);
                    _routed.Add(theEvent);
                }
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
