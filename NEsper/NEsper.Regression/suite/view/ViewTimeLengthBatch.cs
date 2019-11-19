///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewTimeLengthBatch
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ViewTimeLengthBatchSceneOne());
            execs.Add(new ViewTimeLengthBatchSceneTwo());
            execs.Add(new ViewTimeLengthBatchForceOutputOne());
            execs.Add(new ViewTimeLengthBatchForceOutputTwo());
            execs.Add(new ViewTimeLengthBatchForceOutputSum());
            execs.Add(new ViewTimeLengthBatchStartEager());
            execs.Add(new ViewTimeLengthBatchForceOutputStartEagerSum());
            execs.Add(new ViewTimeLengthBatchForceOutputStartNoEagerSum());
            execs.Add(new ViewTimeLengthBatchPreviousAndPrior());
            execs.Add(new ViewTimeLengthBatchGroupBySumStartEager());
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

        private static SupportMarketDataBean[] Get100Events()
        {
            var events = new SupportMarketDataBean[100];
            for (var i = 0; i < events.Length; i++) {
                events[i] = new SupportMarketDataBean("S" + Convert.ToString(i), "id_" + Convert.ToString(i), i);
            }

            return events;
        }

        private static SupportMarketDataBean SendEvent(
            RegressionEnvironment env,
            string symbol)
        {
            var bean = new SupportMarketDataBean(symbol, 0, 0L, null);
            env.SendEventBean(bean);
            return bean;
        }

        public class ViewTimeLengthBatchSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 1000);

                var text = "@Name('s0') select irstream * from SupportMarketDataBean#time_length_batch(10 sec, 3)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                SendTimer(env, 1000);
                SendEvent(env, "E1");

                env.Milestone(1);

                SendTimer(env, 5000);
                SendEvent(env, "E2");

                env.Milestone(2);

                SendTimer(env, 10999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 11000);
                var newData = env.Listener("s0").LastNewData;
                EPAssertionUtil.AssertPropsPerRow(
                    newData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                env.Listener("s0").Reset();

                env.Milestone(3);

                SendTimer(env, 12000);
                SendEvent(env, "E3");
                SendEvent(env, "E4");

                env.Milestone(4);

                SendTimer(env, 15000);
                SendEvent(env, "E5");
                newData = env.Listener("s0").LastNewData;
                var oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    newData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                env.Listener("s0").Reset();

                env.Milestone(5);

                SendTimer(env, 24999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // wait 10 second, check call
                SendTimer(env, 25000);
                newData = env.Listener("s0").LastNewData;
                oldData = env.Listener("s0").LastOldData;
                Assert.IsNull(newData);
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});
                env.Listener("s0").Reset();

                // wait 10 second, check no call received, no events
                SendTimer(env, 35000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ViewTimeLengthBatchSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                var events = Get100Events();

                SendTimer(env, startTime);
                var epl = "@Name('s0') select irstream * from SupportMarketDataBean#time_length_batch(10 sec, 3)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // Send 3 events in batch
                env.SendEventBean(events[0]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[1]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[2]);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[0], events[1], events[2]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                // Send another 3 events in batch
                env.SendEventBean(events[3]);
                env.SendEventBean(events[4]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[5]);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[0], events[1], events[2]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[3], events[4], events[5]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                // Expire the last 3 events by moving time
                SendTimer(env, startTime + 9999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 10000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[3], events[4], events[5]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] { },
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 10001);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // Send an event, let the timer send the batch
                SendTimer(env, startTime + 10100);
                env.SendEventBean(events[6]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 19999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 20000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] { },
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[6]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 20001);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // Send two events, let the timer send the batch
                SendTimer(env, startTime + 29998);
                env.SendEventBean(events[7]);
                env.SendEventBean(events[8]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 29999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 30000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[6]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[7], events[8]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                // Send three events, the the 3 events batch
                SendTimer(env, startTime + 30001);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[9]);
                env.SendEventBean(events[10]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 39000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[11]);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[7], events[8]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[9], events[10], events[11]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                // Send 1 event, let the timer to do the batch
                SendTimer(env, startTime + 39000 + 9999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[12]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 39000 + 10000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[9], events[10], events[11]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[12]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 39000 + 10001);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // Send no events, let the timer to do the batch
                SendTimer(env, startTime + 39000 + 19999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 39000 + 20000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[12]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] { },
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 39000 + 20001);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // Send no events, let the timer to do NO batch
                SendTimer(env, startTime + 39000 + 29999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 39000 + 30000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 39000 + 30001);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // Send 1 more event
                SendTimer(env, startTime + 90000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[13]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 99999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 100000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] { },
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[13]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        public class ViewTimeLengthBatchForceOutputOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 1000);

                var text =
                    "@Name('s0') select irstream * from SupportMarketDataBean#time_length_batch(10 sec, 3, 'force_update')";
                env.CompileDeployAddListenerMileZero(text, "s0");

                SendTimer(env, 1000);
                SendEvent(env, "E1");

                env.Milestone(1);

                SendTimer(env, 5000);
                SendEvent(env, "E2");

                env.Milestone(2);

                SendTimer(env, 10999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 11000);
                var newData = env.Listener("s0").LastNewData;
                EPAssertionUtil.AssertPropsPerRow(
                    newData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                env.Listener("s0").Reset();

                env.Milestone(3);

                SendTimer(env, 12000);
                SendEvent(env, "E3");
                SendEvent(env, "E4");

                env.Milestone(4);

                SendTimer(env, 15000);
                SendEvent(env, "E5");
                newData = env.Listener("s0").LastNewData;
                var oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    newData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                env.Listener("s0").Reset();

                env.Milestone(5);

                SendTimer(env, 24999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // wait 10 second, check call
                SendTimer(env, 25000);
                newData = env.Listener("s0").LastNewData;
                oldData = env.Listener("s0").LastOldData;
                Assert.IsNull(newData);
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});
                env.Listener("s0").Reset();

                env.Milestone(6);

                // wait 10 second, check call, should receive event
                SendTimer(env, 35000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.IsNull(env.Listener("s0").LastOldData);

                env.UndeployAll();
            }
        }

        internal class ViewTimeLengthBatchForceOutputTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                var events = Get100Events();
                SendTimer(env, startTime);

                var epl =
                    "@Name('s0') select irstream * from SupportMarketDataBean#time_length_batch(10 sec, 3, 'FORCE_UPDATE')";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // Send 3 events in batch
                env.SendEventBean(events[0]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[1]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[2]);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[0], events[1], events[2]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                // Send another 3 events in batch
                env.SendEventBean(events[3]);
                env.SendEventBean(events[4]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[5]);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[0], events[1], events[2]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[3], events[4], events[5]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                // Expire the last 3 events by moving time
                SendTimer(env, startTime + 9999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 10000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[3], events[4], events[5]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] { },
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 10001);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // Send an event, let the timer send the batch
                SendTimer(env, startTime + 10100);
                env.SendEventBean(events[6]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 19999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 20000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] { },
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[6]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 20001);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // Send two events, let the timer send the batch
                SendTimer(env, startTime + 29998);
                env.SendEventBean(events[7]);
                env.SendEventBean(events[8]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 29999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 30000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[6]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[7], events[8]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                // Send three events, the the 3 events batch
                SendTimer(env, startTime + 30001);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[9]);
                env.SendEventBean(events[10]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 39000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[11]);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[7], events[8]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[9], events[10], events[11]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                // Send 1 event, let the timer to do the batch
                SendTimer(env, startTime + 39000 + 9999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[12]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 39000 + 10000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[9], events[10], events[11]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[12]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 39000 + 10001);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // Send no events, let the timer to do the batch
                SendTimer(env, startTime + 39000 + 19999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 39000 + 20000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[12]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] { },
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 39000 + 20001);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // Send no events, let the timer do a batch
                SendTimer(env, startTime + 39000 + 29999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 39000 + 30000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] { },
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] { },
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 39000 + 30001);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // Send no events, let the timer do a batch
                SendTimer(env, startTime + 39000 + 39999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 39000 + 40000);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] { },
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] { },
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 39000 + 40001);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // Send 1 more event
                SendTimer(env, startTime + 80000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(events[13]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 88999); // 10 sec from last batch
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 89000);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] { },
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[13]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                // Send 3 more events
                SendTimer(env, startTime + 90000);
                env.SendEventBean(events[14]);
                env.SendEventBean(events[15]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 92000);
                env.SendEventBean(events[16]);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[13]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[14], events[15], events[16]},
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                // Send no events, let the timer do a batch
                SendTimer(env, startTime + 101999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 102000);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] {events[14], events[15], events[16]},
                    env.Listener("s0").OldDataListFlattened);
                EPAssertionUtil.AssertEqualsExactOrderUnderlying(
                    new object[] { },
                    env.Listener("s0").NewDataListFlattened);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewTimeLengthBatchForceOutputSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);
                var events = Get100Events();

                var epl =
                    "@Name('s0') select sum(Price) from SupportMarketDataBean#time_length_batch(10 sec, 3, 'FORCE_UPDATE')";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // Send 1 events in batch
                env.SendEventBean(events[10]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 10000);
                Assert.AreEqual(10.0, env.Listener("s0").LastNewData[0].Get("sum(Price)"));
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 20000);
                Assert.AreEqual(null, env.Listener("s0").LastNewData[0].Get("sum(Price)"));
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 30000);
                Assert.AreEqual(null, env.Listener("s0").LastNewData[0].Get("sum(Price)"));
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 40000);
                Assert.AreEqual(null, env.Listener("s0").LastNewData[0].Get("sum(Price)"));
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        public class ViewTimeLengthBatchStartEager : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 1000);

                var text =
                    "@Name('s0') select irstream * from SupportMarketDataBean#time_length_batch(10 sec, 3, 'start_eager')";
                env.CompileDeployAddListenerMileZero(text, "s0");

                SendTimer(env, 10999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 11000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                env.Milestone(1);

                // Time period without events
                SendTimer(env, 20999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 21000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                // 3 events in batch
                SendTimer(env, 22000);
                SendEvent(env, "E1");
                SendEvent(env, "E2");

                env.Milestone(2);

                SendTimer(env, 25000);
                SendEvent(env, "E3");
                var newData = env.Listener("s0").LastNewData;
                Assert.IsNull(env.Listener("s0").LastOldData);
                EPAssertionUtil.AssertPropsPerRow(
                    newData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
                env.Listener("s0").Reset();

                env.Milestone(3);

                // Time period without events
                SendTimer(env, 34999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 35000);
                var oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
                Assert.IsNull(env.Listener("s0").LastNewData);
                env.Listener("s0").Reset();

                env.Milestone(4);

                // 1 events in time period
                SendTimer(env, 44999);
                SendEvent(env, "E4");

                env.Milestone(5);

                SendTimer(env, 45000);
                newData = env.Listener("s0").LastNewData;
                Assert.IsNull(env.Listener("s0").LastOldData);
                EPAssertionUtil.AssertPropsPerRow(
                    newData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E4"}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewTimeLengthBatchForceOutputStartEagerSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);
                var events = Get100Events();

                var epl =
                    "@Name('s0') select sum(Price) from SupportMarketDataBean#time_length_batch(10 sec, 3, 'force_update, start_eager')";
                env.CompileDeployAddListenerMileZero(epl, "s0");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 9999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // Send batch off
                SendTimer(env, startTime + 10000);
                Assert.AreEqual(null, env.Listener("s0").LastNewData[0].Get("sum(Price)"));
                env.Listener("s0").Reset();

                // Send batch off
                SendTimer(env, startTime + 20000);
                Assert.AreEqual(null, env.Listener("s0").LastNewData[0].Get("sum(Price)"));
                env.Listener("s0").Reset();

                env.SendEventBean(events[11]);
                env.SendEventBean(events[12]);
                SendTimer(env, startTime + 30000);
                Assert.AreEqual(23.0, env.Listener("s0").LastNewData[0].Get("sum(Price)"));
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewTimeLengthBatchForceOutputStartNoEagerSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);

                var epl =
                    "@Name('s0') select sum(Price) from SupportMarketDataBean#time_length_batch(10 sec, 3, 'force_update')";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // No batch as we are not start eager
                SendTimer(env, startTime + 10000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // No batch as we are not start eager
                SendTimer(env, startTime + 20000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ViewTimeLengthBatchPreviousAndPrior : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);
                var premades = Get100Events();

                var epl =
                    "@Name('s0') select Price, prev(1, Price) as prevPrice, prior(1, Price) as priorPrice from SupportMarketDataBean#time_length_batch(10 sec, 3)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                // Send 3 events in batch
                env.SendEventBean(premades[0]);
                env.SendEventBean(premades[1]);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(premades[2]);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                var events = env.Listener("s0").LastNewData;
                AssertData(events[0], 0, null, null);
                AssertData(events[1], 1.0, 0.0, 0.0);
                AssertData(events[2], 2.0, 1.0, 1.0);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewTimeLengthBatchGroupBySumStartEager : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                long startTime = 1000;
                SendTimer(env, startTime);

                var epl =
                    "@Name('s0') select Symbol, sum(Price) as s from SupportMarketDataBean#time_length_batch(5, 10, \"START_EAGER\") group by Symbol order by Symbol asc";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendTimer(env, startTime + 4000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 6000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                var events = env.Listener("s0").LastNewData;
                Assert.IsNull(events);
                env.Listener("s0").Reset();

                SendTimer(env, startTime + 7000);
                env.SendEventBean(new SupportMarketDataBean("S1", "e1", 10d));

                SendTimer(env, startTime + 8000);
                env.SendEventBean(new SupportMarketDataBean("S2", "e2", 77d));

                SendTimer(env, startTime + 9000);
                env.SendEventBean(new SupportMarketDataBean("S1", "e3", 1d));

                SendTimer(env, startTime + 10000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, startTime + 11000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                events = env.Listener("s0").LastNewData;
                Assert.AreEqual(2, events.Length);
                Assert.AreEqual("S1", events[0].Get("Symbol"));
                Assert.AreEqual(11d, events[0].Get("s"));
                Assert.AreEqual("S2", events[1].Get("Symbol"));
                Assert.AreEqual(77d, events[1].Get("s"));
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }
    }
} // end of namespace