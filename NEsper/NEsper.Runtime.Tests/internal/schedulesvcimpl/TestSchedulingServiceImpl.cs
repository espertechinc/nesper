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

using com.espertech.esper.common;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.timer;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.schedulesvcimpl
{
    [TestFixture]
    public class TestSchedulingServiceImpl : AbstractTestBase
    {
        [SetUp]
        public void SetUp()
        {
            service = new SchedulingServiceImpl(new TimeSourceServiceImpl());

            // 2-by-2 table of buckets and slots
            var buckets = new ScheduleBucket[3];
            slots = new long[buckets.Length][];
            slots.Fill(() => new long[2]);
            for (var i = 0; i < buckets.Length; i++)
            {
                buckets[i] = new ScheduleBucket(i);
                slots[i] = new long[2];
                for (var j = 0; j < slots[i].Length; j++)
                {
                    slots[i][j] = buckets[i].AllocateSlot();
                }
            }

            callbacks = new SupportScheduleCallback[5];
            for (var i = 0; i < callbacks.Length; i++)
            {
                callbacks[i] = new SupportScheduleCallback();
            }
        }

        private SchedulingServiceImpl service;
        private long[][] slots;
        private SupportScheduleCallback[] callbacks;

        private void CheckCallbacks(
            SupportScheduleCallback[] callbacks,
            int[] results)
        {
            Assert.IsTrue(callbacks.Length == results.Length);

            for (var i = 0; i < callbacks.Length; i++)
            {
                Assert.AreEqual(results[i], callbacks[i].ClearAndGetOrderTriggered());
            }
        }

        private void EvaluateSchedule()
        {
            ICollection<ScheduleHandle> handles = new LinkedList<ScheduleHandle>();
            service.Evaluate(handles);

            foreach (var handle in handles)
            {
                var cb = (ScheduleHandleCallback) handle;
                cb.ScheduledTrigger();
            }
        }

        public class SupportScheduleCallback : ScheduleHandle,
            ScheduleHandleCallback
        {
            private static int orderAllCallbacks;

            private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            private int orderTriggered;

            public int StatementId => 1;

            public int AgentInstanceId => 0;

            public void ScheduledTrigger()
            {
                log.Debug(".scheduledTrigger");
                orderAllCallbacks++;
                orderTriggered = orderAllCallbacks;
            }

            public int ClearAndGetOrderTriggered()
            {
                var result = orderTriggered;
                orderTriggered = 0;
                return result;
            }

            public static void SetCallbackOrderNum(int orderAllCallbacks)
            {
                SupportScheduleCallback.orderAllCallbacks = orderAllCallbacks;
            }
        }

        [Test]
        public void TestAddTwice()
        {
            service.Add(100, callbacks[0], slots[0][0]);
            Assert.IsTrue(service.IsScheduled(callbacks[0]));
            service.Add(100, callbacks[0], slots[0][0]);

            service.Add(
                ScheduleComputeHelper.ComputeNextOccurance(new ScheduleSpec(), service.Time, TimeZoneInfo.Local, TimeAbacusMilliseconds.INSTANCE),
                callbacks[1],
                slots[0][0]);
            service.Add(
                ScheduleComputeHelper.ComputeNextOccurance(new ScheduleSpec(), service.Time, TimeZoneInfo.Local, TimeAbacusMilliseconds.INSTANCE),
                callbacks[1],
                slots[0][0]);
        }

        [Test]
        public void TestIncorrectRemove()
        {
            var evaluator = new SchedulingServiceImpl(new TimeSourceServiceImpl());
            var callback = new SupportScheduleCallback();
            evaluator.Remove(callback, 0);
        }

        [Test]
        public void TestTrigger()
        {
            long startTime = 0;

            service.Time = 0;

            // Add callbacks
            service.Add(20, callbacks[3], slots[1][1]);
            service.Add(20, callbacks[2], slots[1][0]);
            service.Add(20, callbacks[1], slots[0][1]);
            service.Add(21, callbacks[0], slots[0][0]);
            Assert.IsTrue(service.IsScheduled(callbacks[3]));
            Assert.IsTrue(service.IsScheduled(callbacks[0]));

            // Evaluate before the within time, expect not results
            startTime += 19;
            service.Time = startTime;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 0, 0, 0, 0, 0 });
            Assert.IsTrue(service.IsScheduled(callbacks[3]));

            // Evaluate exactly on the within time, expect a result
            startTime += 1;
            service.Time = startTime;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 0, 1, 2, 3, 0 });
            Assert.IsFalse(service.IsScheduled(callbacks[3]));

            // Evaluate after already evaluated once, no result
            startTime += 1;
            service.Time = startTime;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 4, 0, 0, 0, 0 });
            Assert.IsFalse(service.IsScheduled(callbacks[3]));

            startTime += 1;
            service.Time = startTime;
            EvaluateSchedule();
            Assert.AreEqual(0, callbacks[3].ClearAndGetOrderTriggered());

            // Adding the same callback more than once should cause an exception
            service.Add(20, callbacks[0], slots[0][0]);
            service.Add(28, callbacks[0], slots[0][0]);
            service.Remove(callbacks[0], slots[0][0]);

            service.Add(20, callbacks[2], slots[1][0]);
            service.Add(25, callbacks[1], slots[0][1]);
            service.Remove(callbacks[1], slots[0][1]);
            service.Add(21, callbacks[0], slots[0][0]);
            service.Add(21, callbacks[3], slots[1][1]);
            service.Add(20, callbacks[1], slots[0][1]);
            SupportScheduleCallback.SetCallbackOrderNum(0);

            startTime += 20;
            service.Time = startTime;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 0, 1, 2, 0, 0 });

            startTime += 1;
            service.Time = startTime;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 3, 0, 0, 4, 0 });

            service.Time = startTime + int.MaxValue;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 0, 0, 0, 0, 0 });
        }

        [Test]
        public void TestWaitAndSpecTogether()
        {
            var dateTimeEx = DateTimeEx.GetInstance(
                TimeZoneInfo.Local,
                new DateTime(2004, 11, 9, 15, 27, 10));
            dateTimeEx.SetMillis(500);
            var startTime = dateTimeEx.TimeInMillis;

            service.Time = startTime;

            // Add a specification
            var spec = new ScheduleSpec();
            spec.AddValue(ScheduleUnit.MONTHS, 12);
            spec.AddValue(ScheduleUnit.DAYS_OF_MONTH, 9);
            spec.AddValue(ScheduleUnit.HOURS, 15);
            spec.AddValue(ScheduleUnit.MINUTES, 27);
            spec.AddValue(ScheduleUnit.SECONDS, 20);

            service.Add(
                ScheduleComputeHelper.ComputeDeltaNextOccurance(spec, service.Time, TimeZoneInfo.Local, TimeAbacusMilliseconds.INSTANCE),
                callbacks[3],
                slots[1][1]);

            spec.AddValue(ScheduleUnit.SECONDS, 15);
            service.Add(
                ScheduleComputeHelper.ComputeDeltaNextOccurance(spec, service.Time, TimeZoneInfo.Local, TimeAbacusMilliseconds.INSTANCE),
                callbacks[4],
                slots[2][0]);

            // Add some more callbacks
            service.Add(5000, callbacks[0], slots[0][0]);
            service.Add(10000, callbacks[1], slots[0][1]);
            service.Add(15000, callbacks[2], slots[1][0]);

            // Now send a times reflecting various seconds later and check who got a callback
            service.Time = startTime + 1000;
            SupportScheduleCallback.SetCallbackOrderNum(0);
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 0, 0, 0, 0, 0 });

            service.Time = startTime + 2000;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 0, 0, 0, 0, 0 });

            service.Time = startTime + 4000;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 0, 0, 0, 0, 0 });

            service.Time = startTime + 5000;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 1, 0, 0, 0, 2 });

            service.Time = startTime + 9000;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 0, 0, 0, 0, 0 });

            service.Time = startTime + 10000;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 0, 3, 0, 4, 0 });

            service.Time = startTime + 11000;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 0, 0, 0, 0, 0 });

            service.Time = startTime + 15000;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 0, 0, 5, 0, 0 });

            service.Time = startTime + int.MaxValue;
            EvaluateSchedule();
            CheckCallbacks(callbacks, new[] { 0, 0, 0, 0, 0 });
        }
    }
} // end of namespace
