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
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewTimeOrderAndTimeToLive
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSceneOne(execs);
            WithSceneTwo(execs);
            WithTTLTimeToLive(execs);
            WithTTLMonthScoped(execs);
            WithTTLTimeOrderRemoveStream(execs);
            WithTTLTimeOrder(execs);
            WithTTLGroupedWindow(execs);
            WithTTLInvalid(execs);
            WithTTLPreviousAndPriorSceneOne(execs);
            WithTTLPreviousAndPriorSceneTwo(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTTLPreviousAndPriorSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLPreviousAndPriorSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLPreviousAndPriorSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLPreviousAndPriorSceneOne());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLGroupedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLGroupedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLTimeOrder(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLTimeOrder());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLTimeOrderRemoveStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLTimeOrderRemoveStream());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLMonthScoped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLMonthScoped());
            return execs;
        }

        public static IList<RegressionExecution> WithTTLTimeToLive(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderTTLTimeToLive());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewTimeOrderSceneOne());
            return execs;
        }

        private static SupportBeanTimestamp SendEvent(
            RegressionEnvironment env,
            string id,
            string groupId,
            long timestamp)
        {
            var theEvent = new SupportBeanTimestamp(id, groupId, timestamp);
            env.SendEventBean(theEvent);
            return theEvent;
        }

        private static SupportBeanTimestamp SendEvent(
            RegressionEnvironment env,
            string id,
            long timestamp)
        {
            var theEvent = new SupportBeanTimestamp(id, timestamp);
            env.SendEventBean(theEvent);
            return theEvent;
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
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void SendCurrentTimeWithMinus(
            RegressionEnvironment env,
            string time,
            long minus)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time) - minus);
        }

        private static void AssertData(
            EventBean @event,
            string id,
            string prevId,
            string priorId,
            string prevTailId,
            long? prevCountId,
            object[] prevWindowId)
        {
            Assert.AreEqual(id, @event.Get("Id"));
            Assert.AreEqual(prevId, @event.Get("prevId"));
            Assert.AreEqual(priorId, @event.Get("priorId"));
            Assert.AreEqual(prevTailId, @event.Get("prevtail"));
            Assert.AreEqual(prevCountId, @event.Get("prevCountId"));
            EPAssertionUtil.AssertEqualsExactOrder(prevWindowId, (object[]) @event.Get("prevWindowId"));
        }

        private static void SendSupportBeanWLong(
            RegressionEnvironment env,
            string @string,
            long longPrimitive)
        {
            var sb = new SupportBean(@string, 0);
            sb.LongPrimitive = longPrimitive;
            env.SendEventBean(sb);
        }

        public class ViewTimeOrderSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(1000);

                var text = "@Name('s0') select irstream * from SupportBeanTimestamp#time_order(Timestamp, 10 sec)";
                env.CompileDeploy(text).AddListener("s0").Milestone(0);

                // 1st event
                env.AdvanceTime(1000);
                SendEvent(env, "E1", 3000);
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(1);

                // 2nd event
                env.AdvanceTime(2000);
                SendEvent(env, "E2", 2000);
                Assert.AreEqual("E2", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new[] {"Id"},
                    new[] {new object[] {"E2"}, new object[] {"E1"}});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new[] {"Id"},
                    new[] {new object[] {"E2"}, new object[] {"E1"}});

                // 3rd event
                env.AdvanceTime(3000);
                SendEvent(env, "E3", 3000);
                Assert.AreEqual("E3", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(3);

                // 4th event
                env.AdvanceTime(4000);
                SendEvent(env, "E4", 2500);
                Assert.AreEqual("E4", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(4);

                // Window pushes out event E2
                env.AdvanceTime(11999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(12000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                var oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Id"},
                    new[] {new object[] {"E2"}});
                env.Listener("s0").Reset();

                env.Milestone(5);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new[] {"Id"},
                    new[] {new object[] {"E4"}, new object[] {"E1"}, new object[] {"E3"}});

                // Window pushes out event E4
                env.AdvanceTime(12499);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(12500);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Id"},
                    new[] {new object[] {"E4"}});
                env.Listener("s0").Reset();

                env.Milestone(6);

                // Window pushes out event E1 and E3
                env.AdvanceTime(13000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Id"},
                    new[] {new object[] {"E1"}, new object[] {"E3"}});
                env.Listener("s0").Reset();

                env.Milestone(7);

                // E5
                env.AdvanceTime(14000);
                SendEvent(env, "E5", 14200);
                Assert.AreEqual("E5", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(8);

                // E6
                env.AdvanceTime(14000);
                SendEvent(env, "E6", 14100);
                Assert.AreEqual("E6", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(9);

                // E7
                env.AdvanceTime(15000);
                SendEvent(env, "E7", 15000);
                Assert.AreEqual("E7", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(10);

                // E8
                env.AdvanceTime(15000);
                SendEvent(env, "E8", 14150);
                Assert.AreEqual("E8", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(11);

                // Window pushes out events
                env.AdvanceTime(24500);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Id"},
                    new[] {new object[] {"E6"}, new object[] {"E8"}, new object[] {"E5"}});
                env.Listener("s0").Reset();

                env.Milestone(12);

                // Window pushes out events
                env.AdvanceTime(25000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Id"},
                    new[] {new object[] {"E7"}});
                env.Listener("s0").Reset();

                env.Milestone(13);

                // E9 is very old
                env.AdvanceTime(25000);
                SendEvent(env, "E9", 15000);
                var newData = env.Listener("s0").LastNewData;
                EPAssertionUtil.AssertPropsPerRow(
                    newData,
                    new[] {"Id"},
                    new[] {new object[] {"E9"}});
                oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Id"},
                    new[] {new object[] {"E9"}});
                env.Listener("s0").Reset();

                env.Milestone(14);

                // E10 at 26 sec
                env.AdvanceTime(26000);
                SendEvent(env, "E10", 26000);
                Assert.AreEqual("E10", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(15);

                // E11 at 27 sec
                env.AdvanceTime(27000);
                SendEvent(env, "E11", 27000);
                Assert.AreEqual("E11", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(16);

                // E12 and E13 at 25 sec
                env.AdvanceTime(28000);
                SendEvent(env, "E12", 25000);
                Assert.AreEqual("E12", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                SendEvent(env, "E13", 25000);
                Assert.AreEqual("E13", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(17);

                // Window pushes out events
                env.AdvanceTime(35000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Id"},
                    new[] {new object[] {"E12"}, new object[] {"E13"}});
                env.Listener("s0").Reset();

                env.Milestone(18);

                // E10 at 26 sec
                env.AdvanceTime(35000);
                SendEvent(env, "E14", 26500);
                Assert.AreEqual("E14", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));

                env.Milestone(19);

                // Window pushes out events
                env.AdvanceTime(36000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    new[] {"Id"},
                    new[] {new object[] {"E10"}});
                env.Listener("s0").Reset();

                env.Milestone(20);
                // leaving 1 event in the window

                env.UndeployAll();
            }
        }

        public class ViewTimeOrderSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"TheString", "LongPrimitive"};

                env.AdvanceTime(0);
                var epl = "@Name('s0') select irstream * from SupportBean.ext:time_order(LongPrimitive, 10 sec)";
                env.CompileDeploy(epl).AddListener("s0");

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                env.AdvanceTime(1000);
                SendSupportBeanWLong(env, "E1", 5000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 5000L});

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 5000L}});
                env.AdvanceTime(2000);
                SendSupportBeanWLong(env, "E2", 4000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 4000L});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2", 4000L}, new object[] {"E1", 5000L}});
                env.AdvanceTime(13999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                env.AdvanceTime(14000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 4000L});

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 5000L}});
                SendSupportBeanWLong(env, "E3", 5000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 5000L});

                env.Milestone(5);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 5000L}, new object[] {"E3", 5000L}});
                env.AdvanceTime(14999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(15000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E1", 5000L}, new object[] {"E3", 5000L}});

                env.Milestone(6);

                SendSupportBeanWLong(env, "E4", 2500);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E4", 2500L},
                    new object[] {"E4", 2500L});

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);
                env.AdvanceTime(99999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ViewTimeOrderTTLTimeToLive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var fields = new[] {"Id"};
                var epl = "@Name('s0') select irstream * from SupportBeanTimestamp#timetolive(Timestamp)";
                env.CompileDeploy(epl).AddListener("s0").Milestone(0);

                SendEvent(env, "E1", 1000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}});

                env.Milestone(1);

                SendEvent(env, "E2", 500);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E1"}});

                env.Milestone(2);

                env.AdvanceTime(499);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.AdvanceTime(500);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2"});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}});

                env.Milestone(3);

                SendEvent(env, "E3", 200);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E3"},
                    new object[] {"E3"});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}});

                env.Milestone(4);

                SendEvent(env, "E4", 1200);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4"});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E4"}});

                env.Milestone(5);

                SendEvent(env, "E5", 1000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5"});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E4"}, new object[] {"E5"}});

                env.AdvanceTime(999);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.AdvanceTime(1000);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    null,
                    new[] {new object[] {"E1"}, new object[] {"E5"}});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E4"}});

                env.Milestone(6);

                env.AdvanceTime(1199);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.AdvanceTime(1200);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E4"});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, null);

                SendEvent(env, "E6", 1200);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E6"},
                    new object[] {"E6"});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, null);

                env.Milestone(7);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                env.UndeployAll();
            }
        }

        internal class ViewTimeOrderTTLMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");
                env.CompileDeploy(
                        "@Name('s0') select rstream * from SupportBeanTimestamp#time_order(Timestamp, 1 month)")
                    .AddListener("s0");

                SendEvent(env, "E1", DateTimeParsingFunctions.ParseDefaultMSec("2002-02-01T09:00:00.000"));
                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    new[] {"Id"},
                    new[] {new object[] {"E1"}});

                env.UndeployAll();
            }
        }

        internal class ViewTimeOrderTTLTimeOrderRemoveStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 1000);
                var epl =
                    "insert rstream into OrderedStream select rstream Id from SupportBeanTimestamp#time_order(Timestamp, 10 sec);\n" +
                    "@Name('s0') select * from OrderedStream";
                env.CompileDeploy(epl).AddListener("s0");

                // 1st event at 21 sec
                SendTimer(env, 21000);
                SendEvent(env, "E1", 21000);

                // 2nd event at 22 sec
                SendTimer(env, 22000);
                SendEvent(env, "E2", 22000);

                env.Milestone(0);

                // 3nd event at 28 sec
                SendTimer(env, 28000);
                SendEvent(env, "E3", 28000);

                // 4th event at 30 sec, however is 27 sec (old 3 sec)
                SendTimer(env, 30000);
                SendEvent(env, "E4", 27000);

                env.Milestone(1);

                // 5th event at 30 sec, however is 22 sec (old 8 sec)
                SendEvent(env, "E5", 22000);

                // flush one
                SendTimer(env, 30999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 31000);
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual("E1", env.Listener("s0").LastNewData[0].Get("Id"));
                env.Listener("s0").Reset();

                // 6th event at 31 sec, however is 21 sec (old 10 sec)
                SendEvent(env, "E6", 21000);
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual("E6", env.Listener("s0").LastNewData[0].Get("Id"));
                env.Listener("s0").Reset();

                // 7th event at 31 sec, however is 21.3 sec (old 9.7 sec)
                SendEvent(env, "E7", 21300);

                // flush one
                SendTimer(env, 31299);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 31300);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual("E7", env.Listener("s0").LastNewData[0].Get("Id"));
                env.Listener("s0").Reset();

                // flush two
                SendTimer(env, 31999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 32000);

                var result = env.Listener("s0").NewDataListFlattened;
                Assert.AreEqual(2, result.Length);
                Assert.AreEqual("E2", result[0].Get("Id"));
                Assert.AreEqual("E5", result[1].Get("Id"));
                env.Listener("s0").Reset();

                // flush one
                SendTimer(env, 36999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 37000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual("E4", env.Listener("s0").LastNewData[0].Get("Id"));
                env.Listener("s0").Reset();

                // rather old event
                SendEvent(env, "E8", 21000);
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual("E8", env.Listener("s0").LastNewData[0].Get("Id"));
                env.Listener("s0").Reset();

                // 9-second old event for posting at 38 sec
                SendEvent(env, "E9", 28000);

                // flush two
                SendTimer(env, 37999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 38000);
                result = env.Listener("s0").NewDataListFlattened;
                Assert.AreEqual(2, result.Length);
                Assert.AreEqual("E3", result[0].Get("Id"));
                Assert.AreEqual("E9", result[1].Get("Id"));
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewTimeOrderTTLTimeOrder : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 1000);

                var epl = "@Name('s0') select irstream * from SupportBeanTimestamp#time_order(Timestamp, 10 sec)";
                env.CompileDeploy(epl).AddListener("s0");
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetEnumerator(), new[] {"Id"}, null);

                SendTimer(env, 21000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetEnumerator(), new[] {"Id"}, null);

                env.Milestone(0);

                // 1st event at 21 sec
                SendEvent(env, "E1", 21000);
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E1"}});

                // 2nd event at 22 sec
                SendTimer(env, 22000);
                SendEvent(env, "E2", 22000);
                Assert.AreEqual("E2", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                // 3nd event at 28 sec
                SendTimer(env, 28000);
                SendEvent(env, "E3", 28000);
                Assert.AreEqual("E3", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                env.Milestone(1);

                // 4th event at 30 sec, however is 27 sec (old 3 sec)
                SendTimer(env, 30000);
                SendEvent(env, "E4", 27000);
                Assert.AreEqual("E4", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E4"}, new object[] {"E3"}});

                // 5th event at 30 sec, however is 22 sec (old 8 sec)
                SendEvent(env, "E5", 22000);
                Assert.AreEqual("E5", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {
                        new object[] {"E1"}, new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"},
                        new object[] {"E3"}
                    });

                // flush one
                SendTimer(env, 30999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 31000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                Assert.AreEqual("E1", env.Listener("s0").LastOldData[0].Get("Id"));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});

                // 6th event at 31 sec, however is 21 sec (old 10 sec)
                SendEvent(env, "E6", 21000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual("E6", env.Listener("s0").LastNewData[0].Get("Id"));
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                Assert.AreEqual("E6", env.Listener("s0").LastOldData[0].Get("Id"));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});

                // 7th event at 31 sec, however is 21.3 sec (old 9.7 sec)
                SendEvent(env, "E7", 21300);
                Assert.AreEqual("E7", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {
                        new object[] {"E7"}, new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"},
                        new object[] {"E3"}
                    });

                // flush one
                SendTimer(env, 31299);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 31300);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                Assert.AreEqual("E7", env.Listener("s0").LastOldData[0].Get("Id"));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});

                // flush two
                SendTimer(env, 31999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 32000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(2, env.Listener("s0").LastOldData.Length);
                Assert.AreEqual("E2", env.Listener("s0").LastOldData[0].Get("Id"));
                Assert.AreEqual("E5", env.Listener("s0").LastOldData[1].Get("Id"));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E4"}, new object[] {"E3"}});

                // flush one
                SendTimer(env, 36999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 37000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                Assert.AreEqual("E4", env.Listener("s0").LastOldData[0].Get("Id"));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E3"}});

                // rather old event
                SendEvent(env, "E8", 21000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual("E8", env.Listener("s0").LastNewData[0].Get("Id"));
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                Assert.AreEqual("E8", env.Listener("s0").LastOldData[0].Get("Id"));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E3"}});

                // 9-second old event for posting at 38 sec
                SendEvent(env, "E9", 28000);
                Assert.AreEqual("E9", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E3"}, new object[] {"E9"}});

                // flush two
                SendTimer(env, 37999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 38000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(2, env.Listener("s0").LastOldData.Length);
                Assert.AreEqual("E3", env.Listener("s0").LastOldData[0].Get("Id"));
                Assert.AreEqual("E9", env.Listener("s0").LastOldData[1].Get("Id"));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetEnumerator(), new[] {"Id"}, null);

                // new event
                SendEvent(env, "E10", 38000);
                Assert.AreEqual("E10", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E10"}});

                // flush last
                SendTimer(env, 47999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 48000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                Assert.AreEqual("E10", env.Listener("s0").LastOldData[0].Get("Id"));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetEnumerator(), new[] {"Id"}, null);

                // last, in the future
                SendEvent(env, "E11", 70000);
                Assert.AreEqual("E11", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E11"}});

                SendTimer(env, 80000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                Assert.AreEqual("E11", env.Listener("s0").LastOldData[0].Get("Id"));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetEnumerator(), new[] {"Id"}, null);

                SendTimer(env, 100000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetEnumerator(), new[] {"Id"}, null);

                env.UndeployAll();
            }
        }

        internal class ViewTimeOrderTTLGroupedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 20000);
                var epl =
                    "@Name('s0') select irstream * from SupportBeanTimestamp#groupwin(GroupId)#time_order(Timestamp, 10 sec)";
                env.CompileDeploy(epl).AddListener("s0");

                // 1st event is old
                SendEvent(env, "E1", "G1", 10000);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.AreEqual("E1", env.Listener("s0").LastNewData[0].Get("Id"));
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                Assert.AreEqual("E1", env.Listener("s0").LastOldData[0].Get("Id"));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetEnumerator(), new[] {"Id"}, null);

                env.Milestone(0);

                // 2nd just fits
                SendEvent(env, "E2", "G2", 10001);
                Assert.AreEqual("E2", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E2"}});

                SendEvent(env, "E3", "G3", 20000);
                Assert.AreEqual("E3", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E2"}, new object[] {"E3"}});

                SendEvent(env, "E4", "G2", 20000);
                Assert.AreEqual("E4", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E2"}, new object[] {"E4"}, new object[] {"E3"}});

                SendTimer(env, 20001);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                Assert.AreEqual("E2", env.Listener("s0").LastOldData[0].Get("Id"));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E4"}, new object[] {"E3"}});

                env.Milestone(1);

                SendTimer(env, 22000);
                SendEvent(env, "E5", "G2", 19000);
                Assert.AreEqual("E5", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});

                SendTimer(env, 29000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                Assert.AreEqual("E5", env.Listener("s0").LastOldData[0].Get("Id"));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E4"}, new object[] {"E3"}});

                SendTimer(env, 30000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(2, env.Listener("s0").LastOldData.Length);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").LastOldData,
                    new[] {"Id"},
                    new[] {new object[] {"E4"}, new object[] {"E3"}});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetEnumerator(), new[] {"Id"}, null);

                SendTimer(env, 100000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ViewTimeOrderTTLInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBeanTimestamp#time_order(bump, 10 sec)",
                    "Failed to validate data window declaration: Invalid parameter expression 0 for Time-Order view: Failed to validate view parameter expression 'bump': Property named 'bump' is not valid in any stream [");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBeanTimestamp#time_order(10 sec)",
                    "Failed to validate data window declaration: Time-Order view requires the expression supplying timestamp values, and a numeric or time period parameter for interval size [");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBeanTimestamp#time_order(Timestamp, abc)",
                    "Failed to validate data window declaration: Invalid parameter expression 1 for Time-Order view: Failed to validate view parameter expression 'abc': Property named 'abc' is not valid in any stream (did you mean 'Id'?) [");
            }
        }

        internal class ViewTimeOrderTTLPreviousAndPriorSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 1000);

                var epl = "@Name('s0') select irstream Id, " +
                          " prev(0, Id) as prevIdZero, " +
                          " prev(1, Id) as prevIdOne, " +
                          " prior(1, Id) as priorIdOne," +
                          " prevtail(0, Id) as prevTailIdZero, " +
                          " prevtail(1, Id) as prevTailIdOne, " +
                          " prevcount(Id) as prevCountId, " +
                          " prevwindow(Id) as prevWindowId " +
                          " from SupportBeanTimestamp#time_order(Timestamp, 10 sec)";
                env.CompileDeploy(epl).AddListener("s0");
                string[] fields =
                    {"Id", "prevIdZero", "prevIdOne", "priorIdOne", "prevTailIdZero", "prevTailIdOne", "prevCountId"};

                SendTimer(env, 20000);
                SendEvent(env, "E1", 25000);
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new[] {"Id"},
                    new[] {new object[] {"E1"}});

                env.Milestone(0);

                SendEvent(env, "E2", 21000);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("E2", theEvent.Get("Id"));
                Assert.AreEqual("E2", theEvent.Get("prevIdZero"));
                Assert.AreEqual("E1", theEvent.Get("prevIdOne"));
                Assert.AreEqual("E1", theEvent.Get("priorIdOne"));
                Assert.AreEqual("E1", theEvent.Get("prevTailIdZero"));
                Assert.AreEqual("E2", theEvent.Get("prevTailIdOne"));
                Assert.AreEqual(2L, theEvent.Get("prevCountId"));
                EPAssertionUtil.AssertEqualsExactOrder(
                    (object[]) theEvent.Get("prevWindowId"),
                    new object[] {"E2", "E1"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E2", "E2", "E1", "E1", "E1", "E2", 2L},
                        new object[] {"E1", "E2", "E1", null, "E1", "E2", 2L}
                    });

                SendEvent(env, "E3", 22000);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual("E3", theEvent.Get("Id"));
                Assert.AreEqual("E2", theEvent.Get("prevIdZero"));
                Assert.AreEqual("E3", theEvent.Get("prevIdOne"));
                Assert.AreEqual("E2", theEvent.Get("priorIdOne"));
                Assert.AreEqual("E1", theEvent.Get("prevTailIdZero"));
                Assert.AreEqual("E3", theEvent.Get("prevTailIdOne"));
                Assert.AreEqual(3L, theEvent.Get("prevCountId"));
                EPAssertionUtil.AssertEqualsExactOrder(
                    (object[]) theEvent.Get("prevWindowId"),
                    new object[] {"E2", "E3", "E1"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E2", "E2", "E3", "E1", "E1", "E3", 3L},
                        new object[] {"E3", "E2", "E3", "E2", "E1", "E3", 3L},
                        new object[] {"E1", "E2", "E3", null, "E1", "E3", 3L}
                    });

                SendTimer(env, 31000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.AreEqual(1, env.Listener("s0").OldDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                theEvent = env.Listener("s0").LastOldData[0];
                Assert.AreEqual("E2", theEvent.Get("Id"));
                Assert.IsNull(theEvent.Get("prevIdZero"));
                Assert.IsNull(theEvent.Get("prevIdOne"));
                Assert.AreEqual("E1", theEvent.Get("priorIdOne"));
                Assert.IsNull(theEvent.Get("prevTailIdZero"));
                Assert.IsNull(theEvent.Get("prevTailIdOne"));
                Assert.IsNull(theEvent.Get("prevCountId"));
                Assert.IsNull(theEvent.Get("prevWindowId"));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E3", "E3", "E1", "E2", "E1", "E3", 2L},
                        new object[] {"E1", "E3", "E1", null, "E1", "E3", 2L}
                    });

                env.UndeployAll();
            }
        }

        public class ViewTimeOrderTTLPreviousAndPriorSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(1000);

                var text = "@Name('s0') select irstream Id, " +
                           "prev(1, Id) as prevId, " +
                           "prior(1, Id) as priorId, " +
                           "prevtail(0, Id) as prevtail, " +
                           "prevcount(Id) as prevCountId, " +
                           "prevwindow(Id) as prevWindowId " +
                           "from SupportBeanTimestamp#time_order(Timestamp, 10 sec)";
                env.CompileDeploy(text).AddListener("s0").Milestone(0);

                // event
                env.AdvanceTime(1000);
                SendEvent(env, "E1", 1000);
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                AssertData(
                    @event,
                    "E1",
                    null,
                    null,
                    "E1",
                    1L,
                    new object[] {"E1"});

                env.Milestone(1);

                // event
                env.AdvanceTime(10000);
                SendEvent(env, "E2", 10000);
                @event = env.Listener("s0").AssertOneGetNewAndReset();
                AssertData(
                    @event,
                    "E2",
                    "E2",
                    "E1",
                    "E2",
                    2L,
                    new object[] {"E1", "E2"});

                env.Milestone(2);

                // event
                env.AdvanceTime(10500);
                SendEvent(env, "E3", 8000);
                @event = env.Listener("s0").AssertOneGetNewAndReset();
                AssertData(
                    @event,
                    "E3",
                    "E3",
                    "E2",
                    "E2",
                    3L,
                    new object[] {"E1", "E3", "E2"});

                env.Milestone(3);

                env.AdvanceTime(11000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                var oldData = env.Listener("s0").LastOldData;
                AssertData(oldData[0], "E1", null, null, null, null, null);
                env.Listener("s0").Reset();

                env.Milestone(4);

                // event
                env.AdvanceTime(12000);
                SendEvent(env, "E4", 7000);
                @event = env.Listener("s0").AssertOneGetNewAndReset();
                AssertData(
                    @event,
                    "E4",
                    "E3",
                    "E3",
                    "E2",
                    3L,
                    new object[] {"E4", "E3", "E2"});

                env.Milestone(5);

                env.AdvanceTime(16999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(17000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                AssertData(oldData[0], "E4", null, "E3", null, null, null);
                env.Listener("s0").Reset();

                env.Milestone(6);

                env.AdvanceTime(17999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(18000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                AssertData(oldData[0], "E3", null, "E2", null, null, null);
                env.Listener("s0").Reset();

                env.Milestone(7);

                env.UndeployAll();
            }
        }
    }
} // end of namespace