///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.multithread
{
    public class SendEventCallable : ICallable<object>
    {
        private readonly int _threadNum;
        private readonly EPServiceProvider _engine;
        private readonly IEnumerator<Object> _events;
    
        public SendEventCallable(int threadNum, EPServiceProvider engine, IEnumerator<Object> events)
        {
            _threadNum = threadNum;
            _engine = engine;
            _events = events;
        }
    
        public Object Call()
        {
            Log.Debug(".call Thread " + Thread.CurrentThread.ManagedThreadId + " starting");
            try
            {
                while (_events.MoveNext())
                {
                    var @event = _events.Current;
                    _engine.EPRuntime.SendEvent(@event);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("Error in thread " + _threadNum, ex);
                return false;
            }
            Log.Debug(".call Thread " + Thread.CurrentThread.ManagedThreadId + " done");
            return true;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
