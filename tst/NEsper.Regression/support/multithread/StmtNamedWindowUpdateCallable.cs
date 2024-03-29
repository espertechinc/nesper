///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.suite.multithread;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtNamedWindowUpdateCallable : ICallable<StmtNamedWindowUpdateCallable.UpdateResult>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly int _numRepeats;
        private readonly EPEventServiceSPI _runtime;
        private readonly string _threadName;
        private readonly IList<UpdateItem> _updates = new List<UpdateItem>();

        public StmtNamedWindowUpdateCallable(
            string threadName,
            EPRuntime runtime,
            int numRepeats)
        {
            _runtime = (EPEventServiceSPI) runtime.EventService;
            _numRepeats = numRepeats;
            _threadName = threadName;
        }

        public UpdateResult Call()
        {
            var start = PerformanceObserver.MilliTime;
            try {
                var random = new Random();
                for (var loop = 0; loop < _numRepeats; loop++) {
                    var theString = Convert.ToString(
                        Math.Abs(random.Next()) % MultithreadStmtNamedWindowUpdate.NUM_STRINGS);
                    var intPrimitive = Math.Abs(random.Next()) % MultithreadStmtNamedWindowUpdate.NUM_INTS;
                    var doublePrimitive = Math.Abs(random.Next()) % 10;
                    SendEvent(theString, intPrimitive, doublePrimitive);
                }
            }
            catch (Exception ex) {
                Log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return null;
            }

            var end = PerformanceObserver.MilliTime;
            return new UpdateResult(end - start, _updates);
        }

        private void SendEvent(
            string theString,
            int intPrimitive,
            double doublePrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.BoolPrimitive = false;
            bean.DoublePrimitive = doublePrimitive;
            ((EPEventServiceSendEvent) _runtime).SendEventBean(bean, "SupportBean");
            _updates.Add(new UpdateItem(theString, intPrimitive, doublePrimitive));
        }

        public class UpdateResult
        {
            public UpdateResult(
                long delta,
                IList<UpdateItem> updates)
            {
                Delta = delta;
                Updates = updates;
            }

            public long Delta { get; }

            public IList<UpdateItem> Updates { get; }
        }

        public class UpdateItem
        {
            public UpdateItem(
                string theString,
                int intval,
                double doublePrimitive)
            {
                TheString = theString;
                Intval = intval;
                DoublePrimitive = doublePrimitive;
            }

            public string TheString { get; }

            public int Intval { get; }

            public double DoublePrimitive { get; }
        }
    }
} // end of namespace