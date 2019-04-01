///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.supportregression.multithread
{
    public class SendEventCallable : ICallable<bool>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EPServiceProvider _engine;
        private readonly IEnumerator<object> _events;
        private readonly int _threadNum;

        public SendEventCallable(int threadNum, EPServiceProvider engine, IEnumerator<object> events)
        {
            _threadNum = threadNum;
            _engine = engine;
            _events = events;
        }

        public bool Call()
        {
            Log.Debug(".call Thread " + Thread.CurrentThread.ManagedThreadId + " starting");
            try
            {
                for (var counter = 0 ; _events.MoveNext() ; counter++)
                {
                    var @event = _events.Current;
                    _engine.EPRuntime.SendEvent(@event);
                    Debug.WriteLine("Counter: {0}", counter);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in thread " + _threadNum, ex);
                return false;
            }

            Log.Debug(".call Thread " + Thread.CurrentThread.ManagedThreadId + " done");
            return true;
        }
    }
} // end of namespace