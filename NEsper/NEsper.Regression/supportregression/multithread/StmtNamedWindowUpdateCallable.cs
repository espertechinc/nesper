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
using com.espertech.esper.regression.multithread;
using com.espertech.esper.supportregression.bean;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.multithread
{
    public class StmtNamedWindowUpdateCallable : ICallable<StmtNamedWindowUpdateCallable.UpdateResult> {
        private readonly EPRuntimeSPI _engine;
        private readonly int _numRepeats;
        private readonly string _threadName;
        private readonly List<UpdateItem> _updates = new List<UpdateItem>();
    
        public StmtNamedWindowUpdateCallable(string threadName, EPServiceProvider engine, int numRepeats) {
            _engine = (EPRuntimeSPI) engine.EPRuntime;
            _numRepeats = numRepeats;
            _threadName = threadName;
        }
    
        public UpdateResult Call() {
            long start = PerformanceObserver.MilliTime;
            try {
                var random = new Random();
                for (int loop = 0; loop < _numRepeats; loop++) {
                    string theString = Convert.ToString(Math.Abs(random.Next()) % ExecMTStmtNamedWindowUpdate.NUM_STRINGS);
                    int intPrimitive = Math.Abs(random.Next()) % ExecMTStmtNamedWindowUpdate.NUM_INTS;
                    int doublePrimitive = Math.Abs(random.Next()) % 10;
                    SendEvent(theString, intPrimitive, doublePrimitive);
                }
            } catch (Exception ex) {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return null;
            }
            long end = PerformanceObserver.MilliTime;
            return new UpdateResult(end - start, _updates);
        }
    
        private void SendEvent(string theString, int intPrimitive, double doublePrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.BoolPrimitive = false;
            bean.DoublePrimitive = doublePrimitive;
            _engine.SendEvent(bean);
            _updates.Add(new UpdateItem(theString, intPrimitive, doublePrimitive));
        }
    
        public class UpdateResult {
            public UpdateResult(long delta, IList<UpdateItem> updates) {
                Delta = delta;
                Updates = updates;
            }

            public long Delta { get; }

            public IList<UpdateItem> Updates { get; }
        }
    
        public class UpdateItem {
            public UpdateItem(string theString, int intval, double doublePrimitive) {
                TheString = theString;
                Intval = intval;
                DoublePrimitive = doublePrimitive;
            }

            public string TheString { get; }

            public int Intval { get; }

            public double DoublePrimitive { get; }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
