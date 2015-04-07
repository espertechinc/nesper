///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    public class StmtListenerCreateStmtCallable : ICallable<bool>
    {
        private readonly int _numThread;
        private readonly EPServiceProvider _engine;
        private readonly EPStatement _statement;
        private readonly int _numRoutes;
        private readonly ICollection<SupportMarketDataBean> _routed;

        public StmtListenerCreateStmtCallable(int numThread,
                                              EPServiceProvider engine,
                                              EPStatement statement,
                                              int numRoutes,
                                              ICollection<SupportMarketDataBean> routed)
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
                MyUpdateListener listener = new MyUpdateListener(_engine, _numRoutes, () => _numThread, _routed);
                _statement.Events += listener.Update;
                Thread.Sleep(100);      // wait to send trigger event, other threads receive all other's events
    
                _engine.EPRuntime.SendEvent(new SupportBean());            
    
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
            private readonly int _numRepeats;
            private readonly Func<long> _getNumThread;
            private readonly ICollection<SupportMarketDataBean> _routed;

            public MyUpdateListener(EPServiceProvider engine, int numRepeats, Func<long> getNumThread, ICollection<SupportMarketDataBean> routed)
            {
                _engine = engine;
                _numRepeats = numRepeats;
                _getNumThread = getNumThread;
                _routed = routed;
            }

            public void Update(Object sender, UpdateEventArgs e)
            {
                for (int i = 0; i < _numRepeats; i++)
                {
                    var theEvent = new SupportMarketDataBean("", 0, _getNumThread.Invoke(), null);
                    _engine.EPRuntime.Route(theEvent);
                    _routed.Add(theEvent);
                }
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
