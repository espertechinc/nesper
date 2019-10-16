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
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeRowPerGroup
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowPerGroupSimple());
            execs.Add(new ResultSetQueryTypeRowPerGroupSumOneView());
            execs.Add(new ResultSetQueryTypeRowPerGroupSumJoin());
            execs.Add(new ResultSetQueryTypeCriteriaByDotMethod());
            execs.Add(new ResultSetQueryTypeNamedWindowDelete());
            execs.Add(new ResultSetQueryTypeUnboundStreamUnlimitedKey());
            execs.Add(new ResultSetQueryTypeAggregateGroupedProps());
            execs.Add(new ResultSetQueryTypeAggregateGroupedPropsPerGroup());
            execs.Add(new ResultSetQueryTypeAggregationOverGroupedProps());
            execs.Add(new ResultSetQueryTypeUniqueInBatch());
            execs.Add(new ResultSetQueryTypeSelectAvgExprGroupBy());
            execs.Add(new ResultSetQueryTypeUnboundStreamIterate());
            execs.Add(new ResultSetQueryTypeReclaimSideBySide());
            return execs;
        }

        private static void TryAssertionNamedWindowDelete(
            RegressionEnvironment env,
            string[] fields,
            AtomicLong milestone)
        {
            env.SendEventBean(new SupportBean("A", 100));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"A", 100});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("B", 20));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"B", 20});

            env.SendEventBean(new SupportBean("A", 101));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"A", 201});

            env.SendEventBean(new SupportBean("B", 21));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"B", 41});
            EPAssertionUtil.AssertPropsPerRow(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"A", 201}, new object[] {"B", 41}});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_A("A"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"A", null});
            EPAssertionUtil.AssertPropsPerRow(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"B", 41}});

            env.SendEventBean(new SupportBean("A", 102));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"A", 102});
            EPAssertionUtil.AssertPropsPerRow(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"A", 102}, new object[] {"B", 41}});

            env.SendEventBean(new SupportBean_A("B"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"B", null});
            EPAssertionUtil.AssertPropsPerRow(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"A", 102}});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("B", 22));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"B", 22});
            EPAssertionUtil.AssertPropsPerRow(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"A", 102}, new object[] {"B", 22}});
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbol,
            double? oldSum,
            double? oldAvg,
            double? newSum,
            double? newAvg)
        {
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbol, oldData[0].Get("Symbol"));
            Assert.AreEqual(oldSum, oldData[0].Get("mySum"));
            Assert.AreEqual(oldAvg, oldData[0].Get("myAvg"));

            Assert.AreEqual(symbol, newData[0].Get("Symbol"));
            Assert.AreEqual(newSum, newData[0].Get("mySum"));
            Assert.AreEqual(newAvg, newData[0].Get("myAvg"));

            env.Listener("s0").Reset();
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void TryAssertionSum(RegressionEnvironment env)
        {
            string[] fields = {"Symbol", "mySum", "myAvg"};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, null);

            // assert select result type
            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("mySum"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("myAvg"));

            SendEvent(env, SYMBOL_DELL, 10);
            AssertEvents(
                env,
                SYMBOL_DELL,
                null,
                null,
                10d,
                10d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"DELL", 10d, 10d}});

            env.Milestone(1);

            SendEvent(env, SYMBOL_DELL, 20);
            AssertEvents(
                env,
                SYMBOL_DELL,
                10d,
                10d,
                30d,
                15d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"DELL", 30d, 15d}});

            env.Milestone(2);

            SendEvent(env, SYMBOL_DELL, 100);
            AssertEvents(
                env,
                SYMBOL_DELL,
                30d,
                15d,
                130d,
                130d / 3d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"DELL", 130d, 130d / 3d}});

            env.Milestone(3);

            SendEvent(env, SYMBOL_DELL, 50);
            AssertEvents(
                env,
                SYMBOL_DELL,
                130d,
                130 / 3d,
                170d,
                170 / 3d); // 20 + 100 + 50
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"DELL", 170d, 170d / 3d}});

            env.Milestone(4);

            SendEvent(env, SYMBOL_DELL, 5);
            AssertEvents(
                env,
                SYMBOL_DELL,
                170d,
                170 / 3d,
                155d,
                155 / 3d); // 100 + 50 + 5
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"DELL", 155d, 155d / 3d}});

            env.Milestone(5);

            SendEvent(env, "AAA", 1000);
            AssertEvents(
                env,
                SYMBOL_DELL,
                155d,
                155d / 3,
                55d,
                55d / 2); // 50 + 5
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"DELL", 55d, 55d / 2d}});

            env.Milestone(6);

            SendEvent(env, SYMBOL_IBM, 70);
            AssertEvents(
                env,
                SYMBOL_DELL,
                55d,
                55 / 2d,
                5,
                5,
                SYMBOL_IBM,
                null,
                null,
                70,
                70); // Dell:5
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"DELL", 5d, 5d}, new object[] {"IBM", 70d, 70d}});

            env.Milestone(7);

            SendEvent(env, "AAA", 2000);
            AssertEvents(
                env,
                SYMBOL_DELL,
                5d,
                5d,
                null,
                null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"IBM", 70d, 70d}});

            env.Milestone(8);

            SendEvent(env, "AAA", 3000);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendEvent(env, "AAA", 4000);
            AssertEvents(
                env,
                SYMBOL_IBM,
                70d,
                70d,
                null,
                null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, null);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        private static void AssertEvents(
            RegressionEnvironment env,
            string symbolOne,
            double? oldSumOne,
            double? oldAvgOne,
            double newSumOne,
            double newAvgOne,
            string symbolTwo,
            double? oldSumTwo,
            double? oldAvgTwo,
            double newSumTwo,
            double newAvgTwo)
        {
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Listener("s0").GetAndResetDataListsFlattened(),
                new [] { "mySum","myAvg" },
                new[] {new object[] {newSumOne, newAvgOne}, new object[] {newSumTwo, newAvgTwo}},
                new[] {new object[] {oldSumOne, oldAvgOne}, new object[] {oldSumTwo, oldAvgTwo}});
        }

        private static void SendEventSB(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        public class ResultSetQueryTypeRowPerGroupSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1", "c2", "c3" };

                var epl = "@Name('s0') select TheString as c0, sum(IntPrimitive) as c1," +
                          "min(IntPrimitive) as c2, max(IntPrimitive) as c3 from SupportBean group by TheString";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventSB(env, "E1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10, 10, 10});

                env.Milestone(1);

                SendEventSB(env, "E2", 100);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 100, 100, 100});

                env.Milestone(2);

                SendEventSB(env, "E1", 11);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 21, 10, 11});

                env.Milestone(3);

                SendEventSB(env, "E1", 9);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 30, 9, 11});

                env.Milestone(4);

                SendEventSB(env, "E2", 99);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 199, 99, 100});

                env.Milestone(5);

                SendEventSB(env, "E2", 97);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 296, 97, 100});

                env.Milestone(6);

                SendEventSB(env, "E3", 1000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 1000, 1000, 1000});

                env.Milestone(7);

                SendEventSB(env, "E2", 96);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 392, 96, 100});

                env.Milestone(8);

                env.Milestone(9);

                SendEventSB(env, "E2", 101);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 493, 96, 101});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeSelectAvgExprGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select istream avg(Price) as aPrice, Symbol from SupportMarketDataBean" +
                               "#length(2) group by Symbol";
                env.CompileDeploy(stmtText).AddListener("s0");

                var fields = new [] { "aPrice","Symbol" };

                SendEvent(env, "A", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1.0, "A"});

                SendEvent(env, "B", 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3.0, "B"});

                env.Milestone(0);

                SendEvent(env, "B", 5);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {null, "A"}, new object[] {4.0, "B"}});

                env.Milestone(1);

                SendEvent(env, "A", 10);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {10.0, "A"}, new object[] {5.0, "B"}});

                SendEvent(env, "A", 20);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {15.0, "A"}, new object[] {null, "B"}});

                env.UndeployAll();
            }
        }

        public class ResultSetQueryTypeReclaimSideBySide : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne =
                    "@Name('S0') @Hint('disable_reclaim_group') select sum(IntPrimitive) as val from SupportBean.win:keepall() group by TheString";
                env.CompileDeploy(eplOne).AddListener("S0");
                var eplTwo =
                    "@Name('S1') @Hint('disable_reclaim_group') select window(IntPrimitive) as val from SupportBean.win:keepall() group by TheString";
                env.CompileDeploy(eplTwo).AddListener("S1");
                var eplThree =
                    "@Name('S2') @Hint('disable_reclaim_group') select sum(IntPrimitive) as val1, window(IntPrimitive) as val2 from SupportBean.win:keepall() group by TheString";
                env.CompileDeploy(eplThree).AddListener("S2");
                var eplFour =
                    "@Name('S3') @Hint('reclaim_group_aged=10,reclaim_group_freq=5') select sum(IntPrimitive) as val1, window(IntPrimitive) as val2 from SupportBean.win:keepall() group by TheString";
                env.CompileDeploy(eplFour).AddListener("S3");

                var fieldsOne = new [] { "val" };
                var fieldsTwo = new [] { "val" };
                var fieldsThree = new [] { "val1","val2" };
                var fieldsFour = new [] { "val1","val2" };

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("S0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {1});
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {new[] {1}});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fieldsThree,
                    new object[] {1, new[] {1}});
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fieldsFour,
                    new object[] {1, new[] {1}});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("S0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {3});
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {new[] {1, 2}});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fieldsThree,
                    new object[] {3, new[] {1, 2}});
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fieldsFour,
                    new object[] {3, new[] {1, 2}});

                env.SendEventBean(new SupportBean("E2", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("S0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {4});
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {new[] {4}});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fieldsThree,
                    new object[] {4, new[] {4}});
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fieldsFour,
                    new object[] {4, new[] {4}});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("S0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {9});
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {new[] {4, 5}});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fieldsThree,
                    new object[] {9, new[] {4, 5}});
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fieldsFour,
                    new object[] {9, new[] {4, 5}});

                env.SendEventBean(new SupportBean("E1", 6));
                EPAssertionUtil.AssertProps(
                    env.Listener("S0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {9});
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {new[] {1, 2, 6}});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fieldsThree,
                    new object[] {9, new[] {1, 2, 6}});
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fieldsFour,
                    new object[] {9, new[] {1, 2, 6}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeCriteriaByDotMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select sb.GetTheString() as c0, sum(IntPrimitive) as c1 " +
                          "from SupportBean#length_batch(2) as sb group by sb.GetTheString()";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E1", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "c0", "c1" },
                    new object[] {"E1", 30});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeUnboundStreamIterate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1" };
                var milestone = new AtomicLong();

                // with output snapshot
                var epl =
                    "@Name('s0') select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
                    "output snapshot every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 10}});
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 10}, new object[] {"E2", 20}});
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E1", 11));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 21}, new object[] {"E2", 20}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", 21}, new object[] {"E2", 20}});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E0", 30));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 21}, new object[] {"E2", 20}, new object[] {"E0", 30}});
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();

                // with order-by
                epl =
                    "@Name('s0') select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
                    "output snapshot every 3 events order by TheString asc";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E1", 11));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 21}, new object[] {"E2", 20}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", 21}, new object[] {"E2", 20}});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E0", 30));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E0", 30}, new object[] {"E1", 21}, new object[] {"E2", 20}});
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E3", 40));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E0", 30}, new object[] {"E1", 21}, new object[] {"E2", 20},
                        new object[] {"E3", 40}
                    });
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();

                // test un-grouped case
                epl =
                    "@Name('s0') select null as c0, sum(IntPrimitive) as c1 from SupportBean output snapshot every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {null, 10}});
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {null, 30}});
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 11));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {null, 41}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {null, 41}});

                env.UndeployAll();

                // test reclaim
                env.AdvanceTime(1000);
                epl =
                    "@Name('s0') @Hint('reclaim_group_aged=1,reclaim_group_freq=1') select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
                    "output snapshot every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));

                env.MilestoneInc(milestone);

                env.AdvanceTime(1500);
                env.SendEventBean(new SupportBean("E0", 11));

                env.MilestoneInc(milestone);

                env.AdvanceTime(1800);
                env.SendEventBean(new SupportBean("E2", 12));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", 10}, new object[] {"E0", 11}, new object[] {"E2", 12}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 10}, new object[] {"E0", 11}, new object[] {"E2", 12}});

                env.MilestoneInc(milestone);

                env.AdvanceTime(2200);
                env.SendEventBean(new SupportBean("E2", 13));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E0", 11}, new object[] {"E2", 25}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeNamedWindowDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var path = new RegressionPath();
                var epl = "create window MyWindow#keepall as select * from SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportBean_A a delete from MyWindow w where w.TheString = a.Id;\n" +
                          "on SupportBean_B delete from MyWindow;\n";
                env.CompileDeploy(epl, path);

                epl =
                    "@Hint('DISABLE_RECLAIM_GROUP') @Name('s0') select TheString, sum(IntPrimitive) as mysum from MyWindow group by TheString order by TheString";
                env.CompileDeploy(epl, path).AddListener("s0");
                var fields = new [] { "TheString","mysum" };

                TryAssertionNamedWindowDelete(env, fields, milestone);

                env.UndeployModuleContaining("s0");
                env.SendEventBean(new SupportBean_B("delete"));

                epl =
                    "@Name('s0') select TheString, sum(IntPrimitive) as mysum from MyWindow group by TheString order by TheString";
                env.CompileDeploy(epl, path).AddListener("s0");

                TryAssertionNamedWindowDelete(env, fields, milestone);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeUnboundStreamUnlimitedKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // ESPER-396 Unbound stream and aggregating/grouping by unlimited key (i.e. timestamp) configurable state drop
                SendTimer(env, 0);

                // After the oldest group is 60 second old, reclaim group older then  30 seconds
                var epl =
                    "@Name('s0') @Hint('reclaim_group_aged=30,reclaim_group_freq=5') select LongPrimitive, count(*) from SupportBean group by LongPrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                for (var i = 0; i < 1000; i++) {
                    SendTimer(env, 1000 + i * 1000); // reduce factor if sending more events
                    var theEvent = new SupportBean();
                    theEvent.LongPrimitive = i * 1000;
                    env.SendEventBean(theEvent);

                    //if (i % 100000 == 0)
                    //{
                    //    System.out.println("Sending event number " + i);
                    //}
                }

                env.Listener("s0").Reset();

                for (var i = 0; i < 964; i++) {
                    var theEvent = new SupportBean();
                    theEvent.LongPrimitive = i * 1000;
                    env.SendEventBean(theEvent);
                    Assert.AreEqual(
                        1L,
                        env.Listener("s0").AssertOneGetNewAndReset().Get("count(*)"),
                        "Failed at " + i);
                }

                for (var i = 965; i < 1000; i++) {
                    var theEvent = new SupportBean();
                    theEvent.LongPrimitive = i * 1000;
                    env.SendEventBean(theEvent);
                    Assert.AreEqual(
                        2L,
                        env.Listener("s0").AssertOneGetNewAndReset().Get("count(*)"),
                        "Failed at " + i);
                }

                env.UndeployAll();

                // no frequency provided
                epl =
                    "@Name('s0') @Hint('reclaim_group_aged=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean());
                env.UndeployAll();

                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('define-age') create variable int myAge = 10;\n" +
                    "@Name('define-freq') create variable int myFreq = 10;\n",
                    path);
                var deploymentIdVariables = env.DeploymentId("define-age");

                epl =
                    "@Name('s0') @Hint('reclaim_group_aged=myAge,reclaim_group_freq=myFreq') select LongPrimitive, count(*) from SupportBean group by LongPrimitive";
                env.CompileDeploy(epl, path).AddListener("s0");

                for (var i = 0; i < 1000; i++) {
                    SendTimer(env, 2000000 + 1000 + i * 1000); // reduce factor if sending more events
                    var theEvent = new SupportBean();
                    theEvent.LongPrimitive = i * 1000;
                    env.SendEventBean(theEvent);

                    if (i == 500) {
                        env.Runtime.VariableService.SetVariableValue(deploymentIdVariables, "myAge", 60);
                        env.Runtime.VariableService.SetVariableValue(deploymentIdVariables, "myFreq", 90);
                    }

                    if (i % 100000 == 0) {
                        Console.Out.WriteLine("Sending event number " + i);
                    }
                }

                env.Listener("s0").Reset();

                for (var i = 0; i < 900; i++) {
                    var theEvent = new SupportBean();
                    theEvent.LongPrimitive = i * 1000;
                    env.SendEventBean(theEvent);
                    Assert.AreEqual(
                        1L,
                        env.Listener("s0").AssertOneGetNewAndReset().Get("count(*)"),
                        "Failed at " + i);
                }

                for (var i = 900; i < 1000; i++) {
                    var theEvent = new SupportBean();
                    theEvent.LongPrimitive = i * 1000;
                    env.SendEventBean(theEvent);
                    Assert.AreEqual(
                        2L,
                        env.Listener("s0").AssertOneGetNewAndReset().Get("count(*)"),
                        "Failed at " + i);
                }

                env.UndeployAll();

                // invalid tests
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "@Hint('reclaim_group_aged=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Failed to parse hint parameter value 'xyz' as a double-typed seconds value or variable name [@Hint('reclaim_group_aged=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "@Hint('reclaim_group_aged=30,reclaim_group_freq=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Failed to parse hint parameter value 'xyz' as a double-typed seconds value or variable name [@Hint('reclaim_group_aged=30,reclaim_group_freq=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "@Hint('reclaim_group_aged=MyVar') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Variable type of variable 'MyVar' is not numeric [@Hint('reclaim_group_aged=MyVar') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "@Hint('reclaim_group_aged=-30,reclaim_group_freq=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Hint parameter value '-30' is an invalid value, expecting a double-typed seconds value or variable name [@Hint('reclaim_group_aged=-30,reclaim_group_freq=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
            }
        }

        internal class ResultSetQueryTypeAggregateGroupedProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test for ESPER-185
                var fields = new [] { "mycount" };
                var epl = "@Name('s0') select irstream count(Price) as mycount " +
                          "from SupportMarketDataBean#length(5) " +
                          "group by Price";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, SYMBOL_DELL, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1L}});
                env.Listener("s0").Reset();

                env.Milestone(0);

                SendEvent(env, SYMBOL_DELL, 11);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1L}, new object[] {1L}});
                env.Listener("s0").Reset();

                SendEvent(env, SYMBOL_IBM, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {2L}, new object[] {1L}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeAggregateGroupedPropsPerGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test for ESPER-185
                var fields = new [] { "mycount" };
                var epl = "@Name('s0') select irstream count(Price) as mycount " +
                          "from SupportMarketDataBean#length(5) " +
                          "group by Symbol, Price";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, SYMBOL_DELL, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1L}});
                env.Listener("s0").Reset();

                SendEvent(env, SYMBOL_DELL, 11);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1L}, new object[] {1L}});
                env.Listener("s0").Reset();

                env.Milestone(0);

                SendEvent(env, SYMBOL_DELL, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {2L}, new object[] {1L}});
                env.Listener("s0").Reset();

                SendEvent(env, SYMBOL_IBM, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {2L}, new object[] {1L}, new object[] {1L}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeAggregationOverGroupedProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test for ESPER-185
                var fields = new [] { "Symbol","Price","mycount" };
                var epl = "@Name('s0') select irstream Symbol,Price,count(Price) as mycount " +
                          "from SupportMarketDataBean#length(5) " +
                          "group by Symbol, Price order by Symbol asc";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, SYMBOL_DELL, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"DELL", 10.0, 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"DELL", 10.0, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"DELL", 10.0, 1L}});
                env.Listener("s0").Reset();

                SendEvent(env, SYMBOL_DELL, 11);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"DELL", 11.0, 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"DELL", 11.0, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"DELL", 10.0, 1L}, new object[] {"DELL", 11.0, 1L}});
                env.Listener("s0").Reset();

                env.Milestone(0);

                SendEvent(env, SYMBOL_DELL, 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"DELL", 10.0, 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"DELL", 10.0, 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"DELL", 10.0, 2L}, new object[] {"DELL", 11.0, 1L}});
                env.Listener("s0").Reset();

                SendEvent(env, SYMBOL_IBM, 5);
                Assert.AreEqual(1, env.Listener("s0").NewDataList.Count);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"IBM", 5.0, 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"IBM", 5.0, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"DELL", 10.0, 2L}, new object[] {"DELL", 11.0, 1L}, new object[] {"IBM", 5.0, 1L}
                    });
                env.Listener("s0").Reset();

                SendEvent(env, SYMBOL_IBM, 5);
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"IBM", 5.0, 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"IBM", 5.0, 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"DELL", 10.0, 2L}, new object[] {"DELL", 11.0, 1L}, new object[] {"IBM", 5.0, 2L}
                    });
                env.Listener("s0").Reset();

                env.Milestone(1);

                SendEvent(env, SYMBOL_IBM, 5);
                Assert.AreEqual(2, env.Listener("s0").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[1],
                    fields,
                    new object[] {"IBM", 5.0, 3L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[1],
                    fields,
                    new object[] {"IBM", 5.0, 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"DELL", 10.0, 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"DELL", 10.0, 2L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"DELL", 11.0, 1L}, new object[] {"DELL", 10.0, 1L}, new object[] {"IBM", 5.0, 3L}
                    });
                env.Listener("s0").Reset();

                SendEvent(env, SYMBOL_IBM, 5);
                Assert.AreEqual(2, env.Listener("s0").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[1],
                    fields,
                    new object[] {"IBM", 5.0, 4L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[1],
                    fields,
                    new object[] {"IBM", 5.0, 3L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"DELL", 11.0, 0L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"DELL", 11.0, 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"DELL", 10.0, 1L}, new object[] {"IBM", 5.0, 4L}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeUniqueInBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var epl = "insert into MyStream select Symbol, Price from SupportMarketDataBean#time_batch(1 sec);\n" +
                          "@Name('s0') select Symbol " +
                          "from MyStream#time_batch(1 sec)#unique(Symbol) " +
                          "group by Symbol";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "IBM", 100);
                SendEvent(env, "IBM", 101);

                env.Milestone(1);

                SendEvent(env, "IBM", 102);
                SendTimer(env, 1000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 2000);
                var received = env.Listener("s0").DataListsFlattened;
                Assert.AreEqual("IBM", received.First[0].Get("Symbol"));

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerGroupSumJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream Symbol," +
                          "sum(Price) as mySum," +
                          "avg(Price) as myAvg " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(3) as two " +
                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                          "       and one.TheString = two.Symbol " +
                          "group by Symbol";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));
                env.SendEventBean(new SupportBeanString("AAA"));

                TryAssertionSum(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeRowPerGroupSumOneView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream Symbol," +
                          "sum(Price) as mySum," +
                          "avg(Price) as myAvg " +
                          "from SupportMarketDataBean#length(3) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertionSum(env);

                env.UndeployAll();
            }
        }
    }
} // end of namespace