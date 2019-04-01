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
using com.espertech.esper.compat.threading;
using NUnit.Framework;

namespace com.espertech.esper.supportregression.multithread
{
    public class SendEventRWLockCallable : ICallable<bool> {
        private readonly int _threadNum;
        private readonly EPServiceProvider _engine;
        private readonly IEnumerator<object> _events;
        private readonly ILockable _sharedStartLock;
    
        public SendEventRWLockCallable(int threadNum, ILockable sharedStartLock, EPServiceProvider engine, IEnumerator<object> events) {
            this._threadNum = threadNum;
            this._engine = engine;
            this._events = events;
            this._sharedStartLock = sharedStartLock;
        }
    
        public bool Call()
        {
            using (_sharedStartLock.Acquire())
            {
                Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " starting");
                try
                {
                    while (_events.MoveNext())
                    {
                        _engine.EPRuntime.SendEvent(_events.Current);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error in thread " + _threadNum, ex);
                    return false;
                }

                Log.Info(".call Thread " + Thread.CurrentThread.ManagedThreadId + " done");
                return true;
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
