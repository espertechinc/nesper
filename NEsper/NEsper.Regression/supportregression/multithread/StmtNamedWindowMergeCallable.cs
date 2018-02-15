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
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.multithread
{
    public class StmtNamedWindowMergeCallable : ICallable<bool?>
    {
        private readonly EPRuntimeSPI _engine;
        private readonly int _numEvents;
    
        public StmtNamedWindowMergeCallable(EPServiceProvider engine, int numEvents) {
            this._engine = (EPRuntimeSPI) engine.EPRuntime;
            this._numEvents = numEvents;
        }
    
        public bool? Call() {
            long start = PerformanceObserver.MilliTime;
            try {
                for (int i = 0; i < _numEvents; i++) {
                    _engine.SendEvent(new SupportBean("E" + Convert.ToString(i), 0));
                }
            } catch (Exception ex) {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return null;
            }
            long end = PerformanceObserver.MilliTime;
            return true;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
