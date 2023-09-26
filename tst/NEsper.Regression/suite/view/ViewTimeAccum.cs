///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;


namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewTimeAccum
    {
        public static ICollection<RegressionExecution> Executions()
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

        private class ViewTimeAccumMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");
                var epl = "@name('s0') select rstream * from SupportBean#time_accum(1 month)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));

                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                env.AssertListenerNotInvoked("s0");

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                env.AssertPropsPerRowLastNew(
                    "s0",
                    "theString".SplitCsv(),
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.UndeployAll();
            }
        }

        private class ViewTimeAccumSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);
                var events = Get100Events();

                var epl = "@name('s0') select irstream * from SupportMarketDataBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendTimer(env, startTime + 10000);
                env.AssertListenerNotInvoked("s0");

                // 1st at 10 sec
                env.SendEventBean(events[0]);
                AssertUnderlying(env, events[0]);

                // 2nd event at 14 sec
                SendTimer(env, startTime + 14000);
                env.SendEventBean(events[1]);
                AssertUnderlying(env, events[1]);

                // 3nd event at 14 sec
                SendTimer(env, startTime + 14000);
                env.SendEventBean(events[2]);
                AssertUnderlying(env, events[2]);

                // 3rd event at 23 sec
                SendTimer(env, startTime + 23000);
                env.SendEventBean(events[3]);
                AssertUnderlying(env, events[3]);

                // no event till 33 sec
                SendTimer(env, startTime + 32999);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, startTime + 33000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastNewData);
                        Assert.AreEqual(1, listener.OldDataList.Count);
                        Assert.AreEqual(4, listener.LastOldData.Length);
                        EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                            new object[] { events[0], events[1], events[2], events[3] },
                            listener.OldDataListFlattened);
                        listener.Reset();
                    });

                // no events till 50 sec
                SendTimer(env, startTime + 50000);
                env.AssertListenerNotInvoked("s0");

                // next two events at 55 sec
                SendTimer(env, startTime + 55000);
                env.SendEventBean(events[4]);
                AssertUnderlying(env, events[4]);
                env.SendEventBean(events[5]);
                AssertUnderlying(env, events[5]);

                // no event till 65 sec
                SendTimer(env, startTime + 64999);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, startTime + 65000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastNewData);
                        Assert.AreEqual(1, listener.OldDataList.Count);
                        Assert.AreEqual(2, listener.LastOldData.Length);
                        EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                            new object[] { events[4], events[5] },
                            listener.OldDataListFlattened);
                        listener.Reset();
                    });

                // next window
                env.SendEventBean(events[6]);
                AssertUnderlying(env, events[6]);

                SendTimer(env, startTime + 74999);
                env.SendEventBean(events[7]);
                AssertUnderlying(env, events[7]);

                SendTimer(env, startTime + 74999 + 10000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastNewData);
                        Assert.AreEqual(1, listener.OldDataList.Count);
                        Assert.AreEqual(2, listener.LastOldData.Length);
                        EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                            new object[] { events[6], events[7] },
                            listener.OldDataListFlattened);
                        listener.Reset();
                    });

                env.UndeployAll();
            }
        }

        public class ViewTimeAccumSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 1000);
                var fields = "symbol".SplitCsv();

                var text = "@name('s0') select irstream * from SupportMarketDataBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(text, "s0");
                env.AssertPropsPerRowIterator("s0", fields, null);

                // 1st event
                SendTimer(env, 1000);
                SendEvent(env, "E1");
                env.AssertEqualsNew("s0", "symbol", "E1");

                env.Milestone(1);

                // 2nd event
                SendTimer(env, 5000);
                SendEvent(env, "E2");
                env.AssertEqualsNew("s0", "symbol", "E2");

                env.Milestone(2);

                SendTimer(env, 14999);
                env.AssertListenerNotInvoked("s0");

                // Window pushes out events
                SendTimer(env, 15000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.Milestone(3);

                // No events for a while
                SendTimer(env, 30000);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                // 3rd and 4th event
                SendTimer(env, 31000);
                SendEvent(env, "E3");
                env.AssertEqualsNew("s0", "symbol", "E3");

                env.Milestone(5);

                SendTimer(env, 31000);
                SendEvent(env, "E4");
                env.AssertEqualsNew("s0", "symbol", "E4");

                // Window pushes out events
                env.Milestone(6);

                SendTimer(env, 40999);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 41000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { "E3" }, new object[] { "E4" } });

                // 5th event
                SendEvent(env, "E5");
                env.AssertEqualsNew("s0", "symbol", "E5");

                env.Milestone(7);

                // 6th and 7th event
                SendTimer(env, 41000);
                SendEvent(env, "E6");
                env.AssertEqualsNew("s0", "symbol", "E6");

                SendTimer(env, 49000);
                SendEvent(env, "E7");
                env.AssertEqualsNew("s0", "symbol", "E7");

                env.Milestone(8);

                SendTimer(env, 59000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { "E5" }, new object[] { "E6" }, new object[] { "E7" } });

                env.UndeployAll();
            }
        }

        public class ViewTimeAccumSceneThree : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "theString".SplitCsv();
                SendTimer(env, 1000);
                var epl = "@name('s0') select irstream * from SupportBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AssertPropsPerRowIterator("s0", fields, null);

                SendSupportBean(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(1);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });

                SendTimer(env, 5000);
                SendSupportBean(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(2);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });
                SendTimer(env, 14999);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                SendTimer(env, 15000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.Milestone(4);

                SendTimer(env, 18000);
                SendSupportBean(env, "E3");
                env.AssertPropsNew("s0", fields, new object[] { "E3" });
                SendSupportBean(env, "E4");
                env.AssertPropsNew("s0", fields, new object[] { "E4" });

                env.Milestone(5);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E4" } });
                SendTimer(env, 19000);
                SendSupportBean(env, "E5");
                env.AssertPropsNew("s0", fields, new object[] { "E5" });

                env.Milestone(6);

                SendTimer(env, 28999);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 29000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { "E3" }, new object[] { "E4" }, new object[] { "E5" } });

                env.Milestone(7);

                SendTimer(env, 39000);
                SendTimer(env, 99000);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ViewTimeAccumRStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);
                var events = Get100Events();

                var epl = "@name('s0') select rstream * from SupportMarketDataBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendTimer(env, startTime + 10000);
                env.AssertListenerNotInvoked("s0");

                // some events at 10 sec
                env.SendEventBean(events[0]);
                env.SendEventBean(events[1]);
                env.SendEventBean(events[2]);
                env.AssertListenerNotInvoked("s0");

                // flush out of the window
                SendTimer(env, startTime + 20000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(1, listener.NewDataList.Count);
                        EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                            new object[] { events[0], events[1], events[2] },
                            listener.NewDataListFlattened);
                        listener.Reset();
                    });

                env.UndeployAll();
            }
        }

        private class ViewTimeAccumPreviousAndPriorSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);
                var events = Get100Events();

                var epl =
                    "@name('s0') select irstream price, prev(1, price) as prevPrice, prior(1, price) as priorPrice " +
                    "from SupportMarketDataBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // 1st event
                SendTimer(env, startTime + 20000);
                env.SendEventBean(events[5]);
                env.AssertEventNew("s0", @event => AssertData(@event, 5d, null, null));

                // 2nd event
                SendTimer(env, startTime + 25000);
                env.SendEventBean(events[6]);
                env.AssertEventNew("s0", @event => AssertData(@event, 6d, 5d, 5d));

                // 3nd event
                SendTimer(env, startTime + 34000);
                env.SendEventBean(events[7]);
                env.AssertEventNew("s0", @event => AssertData(@event, 7d, 6d, 6d));

                SendTimer(env, startTime + 43999);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, startTime + 44000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastNewData);
                        Assert.AreEqual(1, listener.OldDataList.Count);
                        Assert.AreEqual(3, listener.LastOldData.Length);
                        AssertData(listener.LastOldData[0], 5d, null, null);
                        AssertData(listener.LastOldData[1], 6d, null, 5d);
                        AssertData(listener.LastOldData[2], 7d, null, 6d);
                        listener.Reset();
                    });

                env.UndeployAll();
            }
        }

        public class ViewTimeAccumPreviousAndPriorSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 1000);

                var text = "@name('s0') select irstream price, " +
                           "prev(1, price) as prevPrice, " +
                           "prior(1, price) as priorPrice, " +
                           "prevtail(price) as prevtailPrice, " +
                           "prevcount(price) as prevCountPrice, " +
                           "prevwindow(price) as prevWindowPrice " +
                           "from SupportMarketDataBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                // 1st event S1 group
                SendTimer(env, 1000);
                SendEvent(env, "S1", 10);
                env.AssertEventNew("s0", @event => AssertData(@event, 10d, null, null, 10d, 1L, new object[] { 10d }));

                env.Milestone(1);

                // 2nd event S1 group
                SendTimer(env, 5000);
                SendEvent(env, "S1", 20);
                env.AssertEventNew(
                    "s0",
                    @event => AssertData(@event, 20d, 10d, 10d, 10d, 2L, new object[] { 20d, 10d }));

                env.Milestone(2);

                // 1st event S2 group
                SendTimer(env, 10000);
                SendEvent(env, "S2", 30);
                env.AssertEventNew(
                    "s0",
                    @event => AssertData(@event, 30d, 20d, 20d, 10d, 3L, new object[] { 30d, 20d, 10d }));

                env.Milestone(3);

                SendTimer(env, 20000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastNewData);
                        var oldData = listener.LastOldData;
                        AssertData(oldData[0], 10d, null, null, null, null, null);
                        AssertData(oldData[1], 20d, null, 10d, null, null, null);
                        AssertData(oldData[2], 30d, null, 20d, null, null, null);
                        listener.Reset();
                    });

                env.UndeployAll();
            }
        }

        private class ViewTimeAccumSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);
                var events = Get100Events();

                var epl =
                    "@name('s0') select irstream sum(price) as sumPrice from SupportMarketDataBean#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // 1st event
                SendTimer(env, startTime + 20000);
                env.SendEventBean(events[5]);
                env.AssertListener(
                    "s0",
                    listener => {
                        AssertData(listener.LastNewData[0], 5d);
                        AssertData(listener.LastOldData[0], null);
                        listener.Reset();
                    });

                // 2nd event
                SendTimer(env, startTime + 25000);
                env.SendEventBean(events[6]);
                env.AssertListener(
                    "s0",
                    listener => {
                        AssertData(listener.LastNewData[0], 11d);
                        AssertData(listener.LastOldData[0], 5d);
                        listener.Reset();
                    });

                SendTimer(env, startTime + 34999);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, startTime + 35000);
                env.AssertListener(
                    "s0",
                    listener => {
                        AssertData(listener.LastNewData[0], null);
                        AssertData(listener.LastOldData[0], 11d);
                        listener.Reset();
                    });

                env.UndeployAll();
            }
        }

        private class ViewTimeAccumGroupedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);
                var events = Get100Events();

                var epl =
                    "@name('s0') select irstream * from SupportMarketDataBean#groupwin(symbol)#time_accum(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // 1st S1 event
                SendTimer(env, startTime + 10000);
                env.SendEventBean(events[1]);
                AssertUnderlying(env, events[1]);

                // 1st S2 event
                SendTimer(env, startTime + 12000);
                env.SendEventBean(events[2]);
                AssertUnderlying(env, events[2]);

                // 2nd S1 event
                SendTimer(env, startTime + 15000);
                env.SendEventBean(events[11]);
                AssertUnderlying(env, events[11]);

                // 2nd S2 event
                SendTimer(env, startTime + 18000);
                env.SendEventBean(events[12]);
                AssertUnderlying(env, events[12]);

                // 3rd S1 event
                SendTimer(env, startTime + 21000);
                env.SendEventBean(events[21]);
                AssertUnderlying(env, events[21]);

                SendTimer(env, startTime + 28000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastNewData);
                        Assert.AreEqual(1, listener.OldDataList.Count);
                        Assert.AreEqual(2, listener.LastOldData.Length);
                        EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                            new object[] { events[2], events[12] },
                            listener.OldDataListFlattened);
                        listener.Reset();
                    });

                // 3rd S2 event
                SendTimer(env, startTime + 29000);
                env.SendEventBean(events[32]);
                AssertUnderlying(env, events[32]);

                SendTimer(env, startTime + 31000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastNewData);
                        Assert.AreEqual(1, listener.OldDataList.Count);
                        Assert.AreEqual(3, listener.LastOldData.Length);
                        EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                            new object[] { events[1], events[11], events[21] },
                            listener.OldDataListFlattened);
                        listener.Reset();
                    });

                SendTimer(env, startTime + 39000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.IsNull(listener.LastNewData);
                        Assert.AreEqual(1, listener.LastOldData.Length);
                        EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                            new object[] { events[32] },
                            listener.OldDataListFlattened);
                        listener.Reset();
                    });

                env.UndeployAll();
            }
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
            Assert.AreEqual(price, theEvent.Get("price"));
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
                events[i] = new SupportMarketDataBean("S" + group.ToString(), "id_" + i.ToString(), i);
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
            Assert.AreEqual(price, @event.Get("price"));
            Assert.AreEqual(prevPrice, @event.Get("prevPrice"));
            Assert.AreEqual(priorPrice, @event.Get("priorPrice"));
            Assert.AreEqual(prevtailPrice, @event.Get("prevtailPrice"));
            Assert.AreEqual(prevCountPrice, @event.Get("prevCountPrice"));
            EPAssertionUtil.AssertEqualsExactOrder(prevWindowPrice, (object[])@event.Get("prevWindowPrice"));
        }

        private static void AssertUnderlying(
            RegressionEnvironment env,
            object underlying)
        {
            env.AssertListener(
                "s0",
                listener => { Assert.AreEqual(listener.AssertOneGetNewAndReset().Underlying, underlying); });
        }
    }
} // end of namespace