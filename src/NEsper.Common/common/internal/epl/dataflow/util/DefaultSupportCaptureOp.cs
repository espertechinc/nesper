///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
    public class DefaultSupportCaptureOp : DefaultSupportCaptureOp<object>
    {
        public DefaultSupportCaptureOp()
            : base(0, new MonitorLock(60000))
        {
        }

        public DefaultSupportCaptureOp(ILockManager lockManager)
            : base(lockManager)
        {
        }

        public DefaultSupportCaptureOp(
            int latchedNumRows,
            ILockManager lockManager)
            : base(latchedNumRows, lockManager)
        {
        }
    }

    public class DefaultSupportCaptureOp<T> : DataFlowOperator,
        EPDataFlowSignalHandler,
        IFuture<object[]>
    {
        private IList<IList<T>> _received = new List<IList<T>>();
        private IList<T> _current = new List<T>();

        private readonly CountDownLatch _numRowLatch;

        private readonly ILockable _iLock;

        public DefaultSupportCaptureOp(ILockManager lockManager)
            : this(0, lockManager)
        {
        }

        public DefaultSupportCaptureOp(
            int latchedNumRows,
            ILockManager lockManager)
            : this(
                latchedNumRows,
                lockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType))
        {
        }

        public DefaultSupportCaptureOp(
            int latchedNumRows,
            ILockable iLock)
        {
            _numRowLatch = new CountDownLatch(latchedNumRows);
            _iLock = iLock;
        }

        public void OnInput(T theEvent)
        {
            using (_iLock.Acquire()) {
                _current.Add(theEvent);
                _numRowLatch?.CountDown();
            }
        }

        public void OnSignal(EPDataFlowSignal signal)
        {
            using (_iLock.Acquire()) {
                _received.Add(_current);
                _current = new List<T>();
            }
        }

        public IList<IList<T>> GetAndReset()
        {
            using (_iLock.Acquire()) {
                var resultEvents = _received;
                _received = new List<IList<T>>();
                _current.Clear();
                return resultEvents;
            }
        }

        public object[] Current {
            get {
                using (_iLock.Acquire()) {
                    return _current.UnwrapIntoArray<object>();
                }
            }
        }

        public object[] GetCurrentAndReset()
        {
            using (_iLock.Acquire()) {
                var currentArray = _current.UnwrapIntoArray<object>();
                _current.Clear();
                return currentArray;
            }
        }

        public bool HasValue => _numRowLatch.Count <= 0;

        public bool IsDone()
        {
            return _numRowLatch.Count <= 0;
        }

        public object[] Get()
        {
            return GetValue(TimeSpan.FromSeconds(60));
        }

        public bool Wait(TimeSpan timeOut)
        {
            return _numRowLatch.Await(timeOut);
        }

        public object[] GetValueOrDefault()
        {
            return !IsDone() ? null : Current;
        }

        public object[] GetValue(TimeSpan timeOut)
        {
            var result = Wait(timeOut);
            if (!result) {
                throw new TimeoutException("latch timed out");
            }

            return Current;
        }

        public object[] GetValue(
            int units,
            TimeUnit timeUnit)
        {
            return GetValue(TimeUnitHelper.ToTimeSpan(units, timeUnit));
        }

        public object[] GetPunctuated()
        {
            var result = _numRowLatch.Await(TimeSpan.FromSeconds(10));
            if (!result) {
                throw new TimeoutException("latch timed out");
            }

            return _received[0].Cast<object>().ToArray();
        }

        /// <summary>Wait for the listener invocation for up to the given number of milliseconds. </summary>
        /// <param name="msecWait">to wait</param>
        /// <param name="numberOfNewEvents">in any number of separate invocations required before returning</param>
        /// <throws>RuntimeException when no results or insufficient number of events were received</throws>
        public void WaitForInvocation(
            long msecWait,
            int numberOfNewEvents)
        {
            var startTime = DateTimeHelper.CurrentTimeMillis;
            while (true) {
                using (_iLock.Acquire()) {
                    if ((DateTimeHelper.CurrentTimeMillis - startTime) > msecWait) {
                        throw new ApplicationException(
                            "No events or less then the number of expected events received, expected " +
                            numberOfNewEvents +
                            " received " +
                            _current.Count);
                    }

                    if (_current.Count >= numberOfNewEvents) {
                        return;
                    }
                }

                try {
                    Thread.Sleep(50);
                }
                catch (ThreadInterruptedException) {
                    return;
                }
            }
        }

        public bool Cancel(bool force)
        {
            throw new NotSupportedException();
        }
    }
}