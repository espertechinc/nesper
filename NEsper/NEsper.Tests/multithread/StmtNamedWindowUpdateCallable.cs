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
using com.espertech.esper.core.service;
using com.espertech.esper.support.bean;

namespace com.espertech.esper.multithread
{
    public class StmtNamedWindowUpdateCallable : ICallable<StmtNamedWindowUpdateCallable.UpdateResult>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPRuntimeSPI _engine;
        private readonly int _numRepeats;
        private readonly String _threadName;
        private readonly List<UpdateItem> _updates = new List<UpdateItem>();

        public StmtNamedWindowUpdateCallable(String threadName,
                                             EPServiceProvider engine,
                                             int numRepeats)
        {
            _engine = (EPRuntimeSPI) engine.EPRuntime;
            _numRepeats = numRepeats;
            _threadName = threadName;
        }

        #region ICallable<UpdateResult> Members

        public UpdateResult Call()
        {
            long start = PerformanceObserver.MilliTime;
            try
            {
                var random = new Random();
                for (int loop = 0; loop < _numRepeats; loop++)
                {
                    string stringValue = Convert.ToString(Math.Abs(random.Next())%TestMTStmtNamedWindowUpdate.NUM_STRINGS);
                    int intPrimitive = Math.Abs(random.Next())%TestMTStmtNamedWindowUpdate.NUM_INTS;
                    int doublePrimitive = Math.Abs(random.Next())%10;
                    SendEvent(stringValue, intPrimitive, doublePrimitive);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return null;
            }
            long end = PerformanceObserver.MilliTime;
            return new UpdateResult(end - start, _updates);
        }

        #endregion

        private void SendEvent(String stringValue,
                               int intPrimitive,
                               double doublePrimitive)
        {
            var bean = new SupportBean(stringValue, intPrimitive);
            bean.BoolPrimitive = false;
            bean.DoublePrimitive = doublePrimitive;
            _engine.SendEvent(bean);
            _updates.Add(new UpdateItem(stringValue, intPrimitive, doublePrimitive));
        }

        #region Nested type: UpdateItem

        public class UpdateItem
        {
            public UpdateItem(String stringValue,
                              int intval,
                              double doublePrimitive)
            {
                TheString = stringValue;
                Intval = intval;
                DoublePrimitive = doublePrimitive;
            }

            public string TheString { get; private set; }

            public int Intval { get; private set; }

            public double DoublePrimitive { get; private set; }
        }

        #endregion

        #region Nested type: UpdateResult

        public class UpdateResult
        {
            private readonly long _delta;
            private readonly List<UpdateItem> _updates;

            public UpdateResult(long delta,
                                List<UpdateItem> updates)
            {
                _delta = delta;
                _updates = updates;
            }

            public long Delta
            {
                get { return _delta; }
            }

            public List<UpdateItem> Updates
            {
                get { return _updates; }
            }
        }

        #endregion
    }
}