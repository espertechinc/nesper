///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewTimeAccum
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSceneOne(execs);
            WithSceneTwo(execs);
            WithSceneThree(execs);
            WithRStream(execs);
            WithPreviousAndPriorSceneOne(execs);
            WithPreviousAndPriorSceneTwo(execs);
            WithMonthScoped(execs);
            WithSum(execs);
            WithGroupedWindow(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeAccumGroupedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithSum(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeAccumSum());
            return execs;
        }

        public static IList<RegressionExecution> WithMonthScoped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeAccumMonthScoped());
            return execs;
        }

        public static IList<RegressionExecution> WithPreviousAndPriorSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeAccumPreviousAndPriorSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithPreviousAndPriorSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeAccumPreviousAndPriorSceneOne());
            return execs;
        }

        public static IList<RegressionExecution> WithRStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeAccumRStream());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneThree(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeAccumSceneThree());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeAccumSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeAccumSceneOne());
            return execs;
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void AssertData(
            EventBean theEvent,
            double price,
            double? prevPrice,
            double? priorPrice)
        {
            Assert.AreEqual(price, theEvent.Get("Price"));
            Assert.AreEqual(prevPrice, theEvent.Get("prevPrice"));
            Assert.AreEqual(priorPrice, theEvent.Get("priorPrice"));
        }

        private static void AssertData(
            EventBean theEvent,
            double? sumPrice)
        {
            Assert.AreEqual(sumPrice, theEvent.Get("sumPrice"));
        }

        private static void SendCurrentTime(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void SendCurrentTimeWithMinus(
            RegressionEnvironment env,
            string time,
            long minus)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time) - minus);
        }

        private static SupportMarketDataBean[] Get100Events()
        {
            var events = new SupportMarketDataBean[100];
            for (var i = 0; i < events.Length; i++) {
                var group = i % 10;
                events[i] = new SupportMarketDataBean("S" + Convert.ToString(group), "id_" + Convert.ToString(i), i);
            }

            return events;
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString)
        {
            env.SendEventBean(new SupportBean(theString, 0));
        }

        private static SupportMarketDataBean SendEvent(
            RegressionEnvironment env,
            string symbol)
        {
            return SendEvent(env, symbol, 0);
        }

        private static SupportMarketDataBean SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
            return bean;
        }

        private static void AssertData(
            EventBean @event,
            double price,
            double? prevPrice,
            double? priorPrice,
            double? prevtailPrice,
            long? prevCountPrice,
            object[] prevWindowPrice)
        {
            Assert.AreEqual(price, @event.Get("Price"));
            Assert.AreEqual(prevPrice, @event.Get("prevPrice"));
            Assert.AreEqual(priorPrice, @event.Get("priorPrice"));
            Assert.AreEqual(prevtailPrice, @event.Get("prevtailPrice"));
            Assert.AreEqual(prevCountPrice, @event.Get("prevCountPrice"));
            CollectionAssert.AreEqual(prevWindowPrice, (IEnumerable) @event.Get("prevWindowPrice"));
        }

        internal class ViewTimeAccumMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");
                var epl = "@Name('s0') select rstream * from SupportBean#time_accum(1 month)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));

                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    new[] {"TheString"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.UndeployAll();
            }
        }

        internal class ViewTimeAccumSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);
                var events = Get100Events();

                var epl = "@Name('s0') select irstream * from SupportMarketDataBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendTimer(env, startTime + 10000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // 1st at 10 sec
                env.SendEventBean(events[0]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[0]);

                // 2nd event at 14 sec
                SendTimer(env, startTime + 14000);
                env.SendEventBean(events[1]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[1]);

                // 3nd event at 14 sec
                SendTimer(env, startTime + 14000);
                env.SendEventBean(events[2]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[2]);

                // 3rd event at 23 sec
                SendTimer(env, startTime + 23000);
                env.SendEventBean(events[3]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[3]);

                // no event till 33 sec
                SendTimer(env, startTime + 32999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 33000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(4, env.Listener("s0").LastOldData.Length);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[0], events[1], events[2], events[3]},
                    env.Listener("s0").OldDataListFlattened);
                env.Listener("s0").Reset();

                // no events till 50 sec
                SendTimer(env, startTime + 50000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // next two events at 55 sec
                SendTimer(env, startTime + 55000);
                env.SendEventBean(events[4]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[4]);
                env.SendEventBean(events[5]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[5]);

                // no event till 65 sec
                SendTimer(env, startTime + 64999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 65000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(2, env.Listener("s0").LastOldData.Length);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[4], events[5]},
                    env.Listener("s0").OldDataListFlattened);
                env.Listener("s0").Reset();

                // next window
                env.SendEventBean(events[6]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[6]);

                SendTimer(env, startTime + 74999);
                env.SendEventBean(events[7]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[7]);

                SendTimer(env, startTime + 74999 + 10000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(2, env.Listener("s0").LastOldData.Length);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[6], events[7]},
                    env.Listener("s0").OldDataListFlattened);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        public class ViewTimeAccumSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 1000);
                var fields = new[] {"TheString"};

                var text = "@Name('s0') select irstream * from SupportMarketDataBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(text, "s0");
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                // 1st event
                SendTimer(env, 1000);
                SendEvent(env, "E1");
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("Symbol"));

                env.Milestone(1);

                // 2nd event
                SendTimer(env, 5000);
                SendEvent(env, "E2");
                Assert.AreEqual("E2", env.Listener("s0").AssertOneGetNewAndReset().Get("Symbol"));

                env.Milestone(2);

                SendTimer(env, 14999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // Window pushes out events
                SendTimer(env, 15000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                var oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                env.Listener("s0").Reset();

                env.Milestone(3);

                // No events for a while
                SendTimer(env, 30000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                // 3rd and 4th event
                SendTimer(env, 31000);
                SendEvent(env, "E3");
                Assert.AreEqual("E3", env.Listener("s0").AssertOneGetNewAndReset().Get("Symbol"));

                env.Milestone(5);

                SendTimer(env, 31000);
                SendEvent(env, "E4");
                Assert.AreEqual("E4", env.Listener("s0").AssertOneGetNewAndReset().Get("Symbol"));

                // Window pushes out events
                env.Milestone(6);

                SendTimer(env, 40999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 41000);
                Assert.AreEqual(null, env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E3"}, new object[] {"E4"}});
                env.Listener("s0").Reset();

                // 5th event
                SendEvent(env, "E5");
                Assert.AreEqual("E5", env.Listener("s0").AssertOneGetNewAndReset().Get("Symbol"));

                env.Milestone(7);

                // 6th and 7th event
                SendTimer(env, 41000);
                SendEvent(env, "E6");
                Assert.AreEqual("E6", env.Listener("s0").AssertOneGetNewAndReset().Get("Symbol"));

                SendTimer(env, 49000);
                SendEvent(env, "E7");
                Assert.AreEqual("E7", env.Listener("s0").AssertOneGetNewAndReset().Get("Symbol"));

                env.Milestone(8);

                SendTimer(env, 59000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        public class ViewTimeAccumSceneThree : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"TheString"};
                SendTimer(env, 1000);
                var epl = "@Name('s0') select irstream * from SupportBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                SendSupportBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}});

                SendTimer(env, 5000);
                SendSupportBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                SendTimer(env, 14999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                SendTimer(env, 15000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.Milestone(4);

                SendTimer(env, 18000);
                SendSupportBean(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});
                SendSupportBean(env, "E4");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4"});

                env.Milestone(5);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}});
                SendTimer(env, 19000);
                SendSupportBean(env, "E5");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5"});

                env.Milestone(6);

                SendTimer(env, 28999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 29000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});

                env.Milestone(7);

                SendTimer(env, 39000);
                SendTimer(env, 99000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ViewTimeAccumRStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);
                var events = Get100Events();

                var epl = "@Name('s0') select rstream * from SupportMarketDataBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendTimer(env, startTime + 10000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // some events at 10 sec
                env.SendEventBean(events[0]);
                env.SendEventBean(events[1]);
                env.SendEventBean(events[2]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // flush out of the window
                SendTimer(env, startTime + 20000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[0], events[1], events[2]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewTimeAccumPreviousAndPriorSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);
                var events = Get100Events();

                var epl =
                    "@Name('s0') select irstream Price, prev(1, Price) as prevPrice, prior(1, Price) as priorPrice " +
                    "from SupportMarketDataBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // 1st event
                SendTimer(env, startTime + 20000);
                env.SendEventBean(events[5]);
                AssertData(env.Listener("s0").AssertOneGetNewAndReset(), 5d, null, null);

                // 2nd event
                SendTimer(env, startTime + 25000);
                env.SendEventBean(events[6]);
                AssertData(env.Listener("s0").AssertOneGetNewAndReset(), 6d, 5d, 5d);

                // 3nd event
                SendTimer(env, startTime + 34000);
                env.SendEventBean(events[7]);
                AssertData(env.Listener("s0").AssertOneGetNewAndReset(), 7d, 6d, 6d);

                SendTimer(env, startTime + 43999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 44000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(3, env.Listener("s0").LastOldData.Length);
                AssertData(env.Listener("s0").LastOldData[0], 5d, null, null);
                AssertData(env.Listener("s0").LastOldData[1], 6d, null, 5d);
                AssertData(env.Listener("s0").LastOldData[2], 7d, null, 6d);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        public class ViewTimeAccumPreviousAndPriorSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 1000);

                var text = "@Name('s0') select irstream Price, " +
                           "prev(1, Price) as prevPrice, " +
                           "prior(1, Price) as priorPrice, " +
                           "prevtail(Price) as prevtailPrice, " +
                           "prevcount(Price) as prevCountPrice, " +
                           "prevwindow(Price) as prevWindowPrice " +
                           "from SupportMarketDataBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                // 1st event S1 group
                SendTimer(env, 1000);
                SendEvent(env, "S1", 10);
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                AssertData(
                    @event,
                    10d,
                    null,
                    null,
                    10d,
                    1L,
                    new object[] {10d});

                env.Milestone(1);

                // 2nd event S1 group
                SendTimer(env, 5000);
                SendEvent(env, "S1", 20);
                @event = env.Listener("s0").AssertOneGetNewAndReset();
                AssertData(
                    @event,
                    20d,
                    10d,
                    10d,
                    10d,
                    2L,
                    new object[] {20d, 10d});

                env.Milestone(2);

                // 1st event S2 group
                SendTimer(env, 10000);
                SendEvent(env, "S2", 30);
                @event = env.Listener("s0").AssertOneGetNewAndReset();
                AssertData(
                    @event,
                    30d,
                    20d,
                    20d,
                    10d,
                    3L,
                    new object[] {30d, 20d, 10d});

                env.Milestone(3);

                SendTimer(env, 20000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                var oldData = env.Listener("s0").LastOldData;
                AssertData(oldData[0], 10d, null, null, null, null, null);
                AssertData(oldData[1], 20d, null, 10d, null, null, null);
                AssertData(oldData[2], 30d, null, 20d, null, null, null);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewTimeAccumSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);
                var events = Get100Events();

                var epl =
                    "@Name('s0') select irstream sum(Price) as sumPrice from SupportMarketDataBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // 1st event
                SendTimer(env, startTime + 20000);
                env.SendEventBean(events[5]);
                AssertData(env.Listener("s0").LastNewData[0], 5d);
                AssertData(env.Listener("s0").LastOldData[0], null);
                env.Listener("s0").Reset();

                // 2nd event
                SendTimer(env, startTime + 25000);
                env.SendEventBean(events[6]);
                AssertData(env.Listener("s0").LastNewData[0], 11d);
                AssertData(env.Listener("s0").LastOldData[0], 5d);
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 34999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 35000);
                AssertData(env.Listener("s0").LastNewData[0], null);
                AssertData(env.Listener("s0").LastOldData[0], 11d);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewTimeAccumGroupedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);
                var events = Get100Events();

                var epl =
                    "@Name('s0') select irstream * from SupportMarketDataBean#groupwin(Symbol)#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // 1st S1 event
                SendTimer(env, startTime + 10000);
                env.SendEventBean(events[1]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[1]);

                // 1st S2 event
                SendTimer(env, startTime + 12000);
                env.SendEventBean(events[2]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[2]);

                // 2nd S1 event
                SendTimer(env, startTime + 15000);
                env.SendEventBean(events[11]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[11]);

                // 2nd S2 event
                SendTimer(env, startTime + 18000);
                env.SendEventBean(events[12]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[12]);

                // 3rd S1 event
                SendTimer(env, startTime + 21000);
                env.SendEventBean(events[21]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[21]);

                SendTimer(env, startTime + 28000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(2, env.Listener("s0").LastOldData.Length);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[2], events[12]},
                    env.Listener("s0").OldDataListFlattened);
                env.Listener("s0").Reset();

                // 3rd S2 event
                SendTimer(env, startTime + 29000);
                env.SendEventBean(events[32]);
                Assert.AreSame(env.Listener("s0").AssertOneGetNewAndReset().Underlying, events[32]);

                SendTimer(env, startTime + 31000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(3, env.Listener("s0").LastOldData.Length);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[1], events[11], events[21]},
                    env.Listener("s0").OldDataListFlattened);
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 39000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[32]},
                    env.Listener("s0").OldDataListFlattened);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }
    }
} // end of namespace