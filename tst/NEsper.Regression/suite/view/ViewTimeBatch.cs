///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewTimeBatch
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSceneOne(execs);
            With10Sec(execs);
            WithStartEagerForceUpdateSceneTwo(execs);
            WithMonthScoped(execs);
            WithStartEagerForceUpdate(execs);
            WithLonger(execs);
            WithMultirow(execs);
            WithMultiBatch(execs);
            WithNoRefPoint(execs);
            WithRefPoint(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithRefPoint(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeBatchRefPoint());
            return execs;
        }

        public static IList<RegressionExecution> WithNoRefPoint(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeBatchNoRefPoint());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeBatchMultiBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithMultirow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeBatchMultirow());
            return execs;
        }

        public static IList<RegressionExecution> WithLonger(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeBatchLonger());
            return execs;
        }

        public static IList<RegressionExecution> WithStartEagerForceUpdate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeBatchStartEagerForceUpdate());
            return execs;
        }

        public static IList<RegressionExecution> WithMonthScoped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeBatchMonthScoped());
            return execs;
        }

        public static IList<RegressionExecution> WithStartEagerForceUpdateSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeBatchStartEagerForceUpdateSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> With10Sec(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeBatch10Sec());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeBatchSceneOne());
            return execs;
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void SendCurrentTime(
            RegressionEnvironment env,
            string time)
        {
            SendTimer(env, DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void SendCurrentTimeWithMinus(
            RegressionEnvironment env,
            string time,
            long minus)
        {
            SendTimer(env, DateTimeParsingFunctions.ParseDefaultMSec(time) - minus);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string e1)
        {
            env.SendEventBean(new SupportBean(e1, 0));
        }

        private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, 0L, null);
        }

        private static void SendEvent(RegressionEnvironment env)
        {
            var theEvent = new SupportBean();
            env.SendEventBean(theEvent);
        }

        private static void SendTimerAssertNotInvoked(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void SendTimerAssertInvoked(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
            Assert.IsTrue(env.Listener("s0").IsInvoked);
            env.Listener("s0").Reset();
        }

        internal class ViewTimeBatchSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var fields = new[] {"Symbol"};
                var text = "@Name('s0') select irstream * from SupportMarketDataBean#time_batch(1 sec)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                SendTimer(env, 1500);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                // Tell the runtime the time after a join point as using external timer
                SendTimer(env, 1500);
                env.SendEventBean(MakeMarketDataEvent("E1"));

                env.Milestone(2);

                SendTimer(env, 1700);

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("E2"));

                env.Milestone(4);

                SendTimer(env, 2499);

                env.Milestone(5);

                SendTimer(env, 2500);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                env.Milestone(6);

                env.SendEventBean(MakeMarketDataEvent("E3"));
                env.SendEventBean(MakeMarketDataEvent("E4"));

                env.Milestone(7);

                SendTimer(env, 2600);
                env.SendEventBean(MakeMarketDataEvent("E5"));

                env.Milestone(8);

                // test iterator
                var events = EPAssertionUtil.EnumeratorToArray(env.Statement("s0").GetEnumerator());
                EPAssertionUtil.AssertPropsPerRow(
                    events,
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});

                SendTimer(env, 3500);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").OldDataListFlattened,
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                env.Listener("s0").Reset();

                env.Milestone(9);

                SendTimer(env, 4500);
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").NewDataListFlattened, fields, null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").OldDataListFlattened,
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});
                env.Listener("s0").Reset();

                env.Milestone(10);

                SendTimer(env, 5500);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.Listener("s0").Reset();

                env.Milestone(11);

                env.SendEventBean(MakeMarketDataEvent("E6"));

                env.Milestone(12);

                SendTimer(env, 6500);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {new object[] {"E6"}});
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").OldDataListFlattened, fields, null);
                env.Listener("s0").Reset();

                env.Milestone(13);

                SendTimer(env, 7500);
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").NewDataListFlattened, fields, null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").OldDataListFlattened,
                    fields,
                    new[] {new object[] {"E6"}});
                env.Listener("s0").Reset();

                env.Milestone(14);

                env.SendEventBean(MakeMarketDataEvent("E7"));

                env.Milestone(15);

                SendTimer(env, 8500);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {new object[] {"E7"}});
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").OldDataListFlattened, fields, null);
                env.Listener("s0").Reset();

                env.Milestone(16);

                env.UndeployAll();
            }
        }

        public class ViewTimeBatch10Sec : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"TheString"};

                SendTimer(env, 0);
                var epl = "@Name('s0') select irstream * from SupportBean#time_batch(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                SendTimer(env, 1000);
                SendSupportBean(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}});
                SendTimer(env, 2000);
                SendSupportBean(env, "E2");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                SendTimer(env, 10999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                SendTimer(env, 11000); // push a batch
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);
                SendSupportBean(env, "E3");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(5);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E3"}});
                SendTimer(env, 21000); // push a batch
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E3"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.Milestone(6);

                SendTimer(env, 31000); // push a batch
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").LastNewData, fields, null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E3"}});

                env.Milestone(7);

                SendTimer(env, 41000); // push a batch
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ViewTimeBatchMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");

                env.CompileDeployAddListenerMileZero("@Name('s0') select * from SupportBean#time_batch(1 month)", "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"TheString"},
                    new object[] {"E1"});

                env.SendEventBean(new SupportBean("E2", 1));
                SendCurrentTimeWithMinus(env, "2002-04-01T09:00:00.000", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendCurrentTime(env, "2002-04-01T09:00:00.000");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"TheString"},
                    new object[] {"E2"});

                env.SendEventBean(new SupportBean("E3", 1));
                SendCurrentTime(env, "2002-05-01T09:00:00.000");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"TheString"},
                    new object[] {"E3"});

                env.UndeployAll();
            }
        }

        internal class ViewTimeBatchStartEagerForceUpdate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 1000);

                var epl = "@Name('s0') select irstream * from SupportBean#time_batch(1, \"START_EAGER,FORCE_UPDATE\")";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendTimer(env, 1999);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 2000);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 2999);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 3000);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 4000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"TheString"},
                    new object[] {"E1"});

                SendTimer(env, 5000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    new[] {"TheString"},
                    new object[] {"E1"});

                SendTimer(env, 5999);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 6000);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 7000);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class ViewTimeBatchStartEagerForceUpdateSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var text =
                    "@Name('s0') select irstream Symbol from SupportMarketDataBean#time_batch(1 sec, \"START_EAGER, FORCE_UPDATE\")";
                env.CompileDeployAddListenerMileZero(text, "s0");

                SendTimer(env, 1000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                env.Listener("s0").Reset();

                SendTimer(env, 2000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                env.Listener("s0").Reset();

                SendTimer(env, 2700);
                env.SendEventBean(MakeMarketDataEvent("E1"));
                SendTimer(env, 2900);
                env.SendEventBean(MakeMarketDataEvent("E2"));

                env.Milestone(1);

                SendTimer(env, 3000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                env.Listener("s0").Reset();

                env.Milestone(2);

                SendTimer(env, 4000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastOldData,
                    new[] {"Symbol"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                env.Listener("s0").Reset();

                SendTimer(env, 5000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ViewTimeBatchLonger : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select irstream * from SupportMarketDataBean#time_batch(1 sec)";
                env.CompileDeploy(text).AddListener("s0");

                var random = new Random();
                var count = 0;
                var sec = 0;
                SendTimer(env, 0);

                for (var i = 0; i < 20; i++) {
                    var numEvents = random.Next() % 10;
                    if (numEvents > 6) {
                        numEvents = 0;
                    }

                    SendTimer(env, sec);
                    for (var j = 0; j < numEvents; j++) {
                        env.SendEventBean(MakeMarketDataEvent("E_" + count));
                        count++;
                    }

                    env.Milestone(i);
                    sec += 1000;
                }

                env.UndeployAll();
            }
        }

        internal class ViewTimeBatchMultirow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"TheString"};

                SendTimer(env, 0);
                var epl = "@Name('s0') select irstream * from SupportBean#time_batch(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                SendTimer(env, 1000);
                SendSupportBean(env, "E1");
                SendSupportBean(env, "E2");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                SendTimer(env, 2000);
                SendSupportBean(env, "E3");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                SendTimer(env, 3000);
                SendSupportBean(env, "E4");
                env.Milestone(3);

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}});
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 11000);
                Assert.IsNull(env.Listener("s0").LastOldData);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}});

                env.Milestone(5);

                SendTimer(env, 21000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}});

                env.Milestone(6);

                SendTimer(env, 31000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendSupportBean(env, "E5");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(7);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E5"}});

                SendTimer(env, 41000);
                Assert.IsNull(env.Listener("s0").LastOldData);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E5"}});

                env.UndeployAll();
            }
        }

        internal class ViewTimeBatchMultiBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"TheString"};

                SendTimer(env, 0);
                var epl = "@Name('s0') select irstream * from SupportBean#time_batch(10 sec)";
                env.CompileDeploy(epl).AddListener("s0");

                SendTimer(env, 1000);
                SendSupportBean(env, "E1");
                SendSupportBean(env, "E2");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                SendTimer(env, 11000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                SendSupportBean(env, "E3");
                SendSupportBean(env, "E4");

                env.Milestone(1);

                SendTimer(env, 21000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                SendSupportBean(env, "E5");
                SendSupportBean(env, "E6");

                env.Milestone(2);

                SendTimer(env, 31000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E5"}, new object[] {"E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}});

                env.Milestone(3);

                SendSupportBean(env, "E7");
                SendSupportBean(env, "E8");
                SendTimer(env, 41000);

                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E7"}, new object[] {"E8"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E5"}, new object[] {"E6"}});

                env.Milestone(4);

                SendTimer(env, 51000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E7"}, new object[] {"E8"}});

                SendTimer(env, 61000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ViewTimeBatchNoRefPoint : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean#time_batch(10 minutes)";
                env.CompileDeploy(epl).AddListener("s0");

                env.AdvanceTime(0);

                SendEvent(env);

                SendTimerAssertNotInvoked(env, 10 * 60 * 1000 - 1);
                SendTimerAssertInvoked(env, 10 * 60 * 1000);

                env.UndeployAll();
            }
        }

        internal class ViewTimeBatchRefPoint : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(00);

                var epl = "@Name('s0') select * from SupportBean#time_batch(10 minutes, 10L)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AdvanceTime(10);

                SendEvent(env);

                SendTimerAssertNotInvoked(env, 10 * 60 * 1000 - 1 + 10);
                SendTimerAssertInvoked(env, 10 * 60 * 1000 + 10);

                env.UndeployAll();
            }
        }
    }
} // end of namespace