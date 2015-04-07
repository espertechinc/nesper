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
using com.espertech.esper.util;

namespace com.espertech.esper.multithread
{
    public class SendEventWaitCallable : ICallable<bool>
    {
        private readonly int _threadNum;
        private readonly EPServiceProvider _engine;
        private readonly IEnumerator<Object> _events;
        private readonly Object _sendLock;
        private bool _isShutdown;
    
        public SendEventWaitCallable(int threadNum, EPServiceProvider engine, Object sendLock, IEnumerator<Object> events)
        {
            _threadNum = threadNum;
            _engine = engine;
            _events = events;
            _sendLock = sendLock;
        }
    
        public void SetShutdown(bool shutdown)
        {
            _isShutdown = shutdown;
        }

        public bool Call()
        {
            try
            {
                while ((_events.MoveNext() && (!_isShutdown)))
                {
                    lock(_sendLock) {
                        Monitor.Wait(_sendLock);
                    }
                    ThreadLogUtil.Info("sending event");
                    _engine.EPRuntime.SendEvent(_events.Current);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("Error in thread " + _threadNum, ex);
                return false;
            }
            return true;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
