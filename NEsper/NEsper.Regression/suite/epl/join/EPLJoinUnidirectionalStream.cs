///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinUnidirectionalStream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithPatternUnidirectionalOuterJoinNoOn(execs);
            With2TableJoinGrouped(execs);
            With2TableJoinRowForAll(execs);
            With3TableOuterJoinVar1(execs);
            With3TableOuterJoinVar2(execs);
            WithPatternJoin(execs);
            WithPatternJoinOutputRate(execs);
            With3TableJoinVar1(execs);
            With3TableJoinVar2A(execs);
            With3TableJoinVar2B(execs);
            With3TableJoinVar3(execs);
            With2TableFullOuterJoin(execs);
            With2TableFullOuterJoinCompile(execs);
            With2TableFullOuterJoinOM(execs);
            With2TableFullOuterJoinBackwards(execs);
            With2TableJoin(execs);
            With2TableBackwards(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinInvalid());
            return execs;
        }

        public static IList<RegressionExecution> With2TableBackwards(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin2TableBackwards());
            return execs;
        }

        public static IList<RegressionExecution> With2TableJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin2TableJoin());
            return execs;
        }

        public static IList<RegressionExecution> With2TableFullOuterJoinBackwards(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin2TableFullOuterJoinBackwards());
            return execs;
        }

        public static IList<RegressionExecution> With2TableFullOuterJoinOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin2TableFullOuterJoinOM());
            return execs;
        }

        public static IList<RegressionExecution> With2TableFullOuterJoinCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin2TableFullOuterJoinCompile());
            return execs;
        }

        public static IList<RegressionExecution> With2TableFullOuterJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin2TableFullOuterJoin());
            return execs;
        }

        public static IList<RegressionExecution> With3TableJoinVar3(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin3TableJoinVar3());
            return execs;
        }

        public static IList<RegressionExecution> With3TableJoinVar2B(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin3TableJoinVar2B());
            return execs;
        }

        public static IList<RegressionExecution> With3TableJoinVar2A(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin3TableJoinVar2A());
            return execs;
        }

        public static IList<RegressionExecution> With3TableJoinVar1(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin3TableJoinVar1());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternJoinOutputRate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPatternJoinOutputRate());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPatternJoin());
            return execs;
        }

        public static IList<RegressionExecution> With3TableOuterJoinVar2(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin3TableOuterJoinVar2());
            return execs;
        }

        public static IList<RegressionExecution> With3TableOuterJoinVar1(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin3TableOuterJoinVar1());
            return execs;
        }

        public static IList<RegressionExecution> With2TableJoinRowForAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin2TableJoinRowForAll());
            return execs;
        }

        public static IList<RegressionExecution> With2TableJoinGrouped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoin2TableJoinGrouped());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternUnidirectionalOuterJoinNoOn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPatternUnidirectionalOuterJoinNoOn());
            return execs;
        }

        private static void TryFullOuterPassive2Stream(RegressionEnvironment env)
        {
            TryUnsupportedIterator(env.Statement("s0"));

            // send event, expect result
            SendEventMD(env, "E1", 1L);
            var fields = new[] {"Symbol", "Volume", "TheString", "IntPrimitive"};
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 1L, null, null});

            SendEvent(env, "E1", 10);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendEventMD(env, "E1", 2L);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 2L, "E1", 10});

            SendEvent(env, "E1", 20);
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void TryJoinPassive2Stream(
            RegressionEnvironment env,
            string stmtText)
        {
            env.CompileDeployAddListenerMileZero(stmtText, "s0");
            TryUnsupportedIterator(env.Statement("s0"));

            // send event, expect result
            SendEventMD(env, "E1", 1L);
            var fields = new[] {"Symbol", "Volume", "TheString", "IntPrimitive"};
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendEvent(env, "E1", 10);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendEventMD(env, "E1", 2L);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 2L, "E1", 10});

            SendEvent(env, "E1", 20);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.UndeployAll();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string s,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendEventMD(
            RegressionEnvironment env,
            string symbol,
            long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            env.SendEventBean(bean);
        }

        private static void TryAssertionPatternUniOuterJoinNoOn(
            RegressionEnvironment env,
            long startTime)
        {
            var fields = new[] {"c0", "c1"};
            env.AdvanceTime(startTime + 2000);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, 1L});

            env.SendEventBean(new SupportBean("E1", 10));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.AdvanceTime(startTime + 3000);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {10, 1L});

            env.SendEventBean(new SupportBean("E2", 11));

            env.AdvanceTime(startTime + 4000);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {21, 2L});

            env.SendEventBean(new SupportBean("E3", 12));

            env.AdvanceTime(startTime + 5000);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {33, 3L});
        }

        private static void TryAssertion2StreamInnerWGroupBy(RegressionEnvironment env)
        {
            var epl = "create objectarray schema E1 (id string, grp string, value int);\n" +
                      "create objectarray schema E2 (id string, value2 int);\n" +
                      "@Name('s0') select count(*) as c0, sum(E1.value) as c1, E1.id as c2 " +
                      "from E1 unidirectional inner join E2#keepall on E1.id = E2.id group by E1.grp";
            env.CompileDeployWBusPublicType(epl, new RegressionPath());
            env.AddListener("s0");
            var fields = new[] {"c0", "c1", "c2"};

            env.SendEventObjectArray(new object[] {"A", 100}, "E2");
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventObjectArray(new object[] {"A", "X", 10}, "E1");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {1L, 10, "A"});

            env.SendEventObjectArray(new object[] {"A", "Y", 20}, "E1");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {1L, 20, "A"});

            env.UndeployAll();
        }

        private static void Try3TableOuterJoin(RegressionEnvironment env)
        {
            var fields = new[] {"S0.Id", "S1.Id", "S2.Id"};

            env.SendEventBean(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {1, null, null});
            env.SendEventBean(new SupportBean_S1(2, "E1"));
            env.SendEventBean(new SupportBean_S2(3, "E1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(20, "E2"));
            env.SendEventBean(new SupportBean_S0(10, "E2"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {10, 20, null});
            env.SendEventBean(new SupportBean_S2(30, "E2"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(300, "E3"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            env.SendEventBean(new SupportBean_S0(100, "E3"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {100, null, null});
            env.SendEventBean(new SupportBean_S1(200, "E3"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(31, "E4"));
            env.SendEventBean(new SupportBean_S1(21, "E4"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            env.SendEventBean(new SupportBean_S0(11, "E4"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {11, 21, 31});

            env.SendEventBean(new SupportBean_S2(32, "E4"));
            env.SendEventBean(new SupportBean_S1(22, "E4"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void Try3TableJoin(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBean_S0(1, "E1"));
            env.SendEventBean(new SupportBean_S1(2, "E1"));
            env.SendEventBean(new SupportBean_S2(3, "E1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(20, "E2"));
            env.SendEventBean(new SupportBean_S0(10, "E2"));
            env.SendEventBean(new SupportBean_S2(30, "E2"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(300, "E3"));
            env.SendEventBean(new SupportBean_S0(100, "E3"));
            env.SendEventBean(new SupportBean_S1(200, "E3"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S2(31, "E4"));
            env.SendEventBean(new SupportBean_S1(21, "E4"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S0(11, "E4"));
            var fields = new[] {"S0.Id", "S1.Id", "S2.Id"};
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {11, 21, 31});

            env.SendEventBean(new SupportBean_S2(32, "E4"));
            env.SendEventBean(new SupportBean_S1(22, "E4"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void TryUnsupportedIterator(EPStatement stmt)
        {
            try {
                stmt.GetEnumerator();
                Assert.Fail();
            }
            catch (UnsupportedOperationException ex) {
                Assert.AreEqual("Iteration over a unidirectional join is not supported", ex.Message);
            }
        }

        internal class EPLJoinPatternUnidirectionalOuterJoinNoOn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test 2-stream left outer join and SODA
                //
                var milestone = new AtomicLong();
                env.AdvanceTime(1000);

                var stmtTextLO = "@Name('s0') select sum(IntPrimitive) as c0, count(*) as c1 " +
                                 "from pattern [every timer:interval(1)] unidirectional " +
                                 "left outer join " +
                                 "SupportBean#keepall";
                env.CompileDeployAddListenerMile(stmtTextLO, "s0", milestone.GetAndIncrement());

                TryAssertionPatternUniOuterJoinNoOn(env, 0);

                env.UndeployAll();

                env.EplToModelCompileDeploy(stmtTextLO).AddListener("s0").Milestone(milestone.GetAndIncrement());

                TryAssertionPatternUniOuterJoinNoOn(env, 100000);

                env.UndeployAll();

                // test 2-stream inner join
                //
                var fieldsIJ = new[] {"c0", "c1"};
                var stmtTextIJ = "@Name('s0') select sum(IntPrimitive) as c0, count(*) as c1 " +
                                 "from SupportBean_S0 unidirectional " +
                                 "inner join " +
                                 "SupportBean#keepall";
                env.CompileDeployAddListenerMile(stmtTextIJ, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean_S0(1, "S0_1"));
                env.SendEventBean(new SupportBean("E1", 100));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(2, "S0_2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsIJ,
                    new object[] {100, 1L});

                env.SendEventBean(new SupportBean("E2", 200));

                env.SendEventBean(new SupportBean_S0(3, "S0_3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsIJ,
                    new object[] {300, 2L});
                env.UndeployAll();

                // test 2-stream inner join with group-by
                TryAssertion2StreamInnerWGroupBy(env);

                // test 3-stream inner join
                //
                var fields3IJ = new[] {"c0", "c1"};
                var stmtText3IJ = "@Name('s0') select sum(IntPrimitive) as c0, count(*) as c1 " +
                                  "from " +
                                  "SupportBean_S0#keepall " +
                                  "inner join " +
                                  "SupportBean_S1#keepall " +
                                  "inner join " +
                                  "SupportBean#keepall";
                env.CompileDeployAddListenerMile(stmtText3IJ, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean_S0(1, "S0_1"));
                env.SendEventBean(new SupportBean("E1", 50));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S1(10, "S1_1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields3IJ,
                    new object[] {50, 1L});

                env.SendEventBean(new SupportBean("E2", 51));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields3IJ,
                    new object[] {101, 2L});

                env.UndeployAll();

                // test 3-stream full outer join
                //
                var fields3FOJ = new[] {"P00", "P10", "TheString"};
                var stmtText3FOJ = "@Name('s0') select P00, P10, TheString " +
                                   "from " +
                                   "SupportBean_S0#keepall " +
                                   "full outer join " +
                                   "SupportBean_S1#keepall " +
                                   "full outer join " +
                                   "SupportBean#keepall";
                env.CompileDeployAddListenerMile(stmtText3FOJ, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean_S0(1, "S0_1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields3FOJ,
                    new object[] {"S0_1", null, null});

                env.SendEventBean(new SupportBean("E10", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields3FOJ,
                    new object[] {null, null, "E10"});

                env.SendEventBean(new SupportBean_S0(2, "S0_2"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields3FOJ,
                    new[] {new object[] {"S0_2", null, null}});

                env.SendEventBean(new SupportBean_S1(1, "S1_0"));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields3FOJ,
                    new[] {new object[] {"S0_1", "S1_0", "E10"}, new object[] {"S0_2", "S1_0", "E10"}});

                env.SendEventBean(new SupportBean_S0(2, "S0_3"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields3FOJ,
                    new[] {new object[] {"S0_3", "S1_0", "E10"}});

                env.SendEventBean(new SupportBean("E11", 0));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields3FOJ,
                    new[] {
                        new object[] {"S0_1", "S1_0", "E11"}, new object[] {"S0_2", "S1_0", "E11"},
                        new object[] {"S0_3", "S1_0", "E11"}
                    });
                Assert.AreEqual(6, EPAssertionUtil.EnumeratorCount(env.GetEnumerator("s0")));

                env.UndeployAll();

                // test 3-stream full outer join with where-clause
                //
                var fields3FOJW = new[] {"P00", "P10", "TheString"};
                var stmtText3FOJW = "@Name('s0') select P00, P10, TheString " +
                                    "from " +
                                    "SupportBean_S0#keepall as S0 " +
                                    "full outer join " +
                                    "SupportBean_S1#keepall as S1 " +
                                    "full outer join " +
                                    "SupportBean#keepall as sb " +
                                    "where S0.P00 = S1.P10";
                env.CompileDeployAddListenerMile(stmtText3FOJW, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean_S0(1, "X1"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S1(1, "Y1"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(1, "Y1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields3FOJW,
                    new object[] {"Y1", "Y1", null});

                env.UndeployAll();
            }
        }

        internal class EPLJoin2TableJoinGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select irstream Symbol, count(*) as cnt " +
                               "from SupportMarketDataBean unidirectional, SupportBean#keepall " +
                               "where TheString = Symbol group by TheString, Symbol";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // send event, expect result
                SendEventMD(env, "E1", 1L);
                var fields = new[] {"Symbol", "cnt"};
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "E1", 10);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEventMD(env, "E1", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E1", 0L});
                env.Listener("s0").Reset();

                SendEvent(env, "E1", 20);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEventMD(env, "E1", 3L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E1", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E1", 0L});
                env.Listener("s0").Reset();

                try {
                    env.Statement("s0").GetEnumerator();
                    Assert.Fail();
                }
                catch (UnsupportedOperationException ex) {
                    Assert.AreEqual("Iteration over a unidirectional join is not supported", ex.Message);
                }
                // assure lock given up by sending more events

                SendEvent(env, "E2", 40);
                SendEventMD(env, "E2", 4L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E2", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E2", 0L});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class EPLJoin2TableJoinRowForAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select irstream count(*) as cnt " +
                               "from SupportMarketDataBean unidirectional, SupportBean#keepall " +
                               "where TheString = Symbol";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");
                TryUnsupportedIterator(env.Statement("s0"));

                // send event, expect result
                SendEventMD(env, "E1", 1L);
                var fields = new[] {"cnt"};
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "E1", 10);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEventMD(env, "E1", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {0L});
                env.Listener("s0").Reset();

                SendEvent(env, "E1", 20);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEventMD(env, "E1", 3L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {0L});
                env.Listener("s0").Reset();

                SendEvent(env, "E2", 40);
                SendEventMD(env, "E2", 4L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {0L});

                env.UndeployAll();
            }
        }

        internal class EPLJoin3TableOuterJoinVar1 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select S0.Id, S1.Id, S2.Id " +
                               "from SupportBean_S0 as S0 unidirectional " +
                               " full outer join SupportBean_S1#keepall as S1" +
                               " on P00 = P10 " +
                               " full outer join SupportBean_S2#keepall as S2" +
                               " on P10 = P20";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");
                Try3TableOuterJoin(env);
                env.UndeployAll();
            }
        }

        internal class EPLJoin3TableOuterJoinVar2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select S0.Id, S1.Id, S2.Id from SupportBean_S0 as S0 unidirectional " +
                               " left outer join SupportBean_S1#keepall as S1 " +
                               " on P00 = P10 " +
                               " left outer join SupportBean_S2#keepall as S2 " +
                               " on P10 = P20";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");
                Try3TableOuterJoin(env);
                env.UndeployAll();
            }
        }

        internal class EPLJoinPatternJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(1000);

                // no iterator allowed
                var stmtText = "@Name('s0') select count(*) as num " +
                               "from pattern [every timer:at(*/1,*,*,*,*)] unidirectional,\n" +
                               "SupportBean(IntPrimitive=1)#unique(TheString) a,\n" +
                               "SupportBean(IntPrimitive=2)#unique(TheString) b\n" +
                               "where a.TheString = b.TheString";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendEvent(env, "A", 1);
                SendEvent(env, "A", 2);
                SendEvent(env, "B", 1);
                SendEvent(env, "B", 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(70000);
                Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("num"));

                env.AdvanceTime(140000);
                Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("num"));

                env.UndeployAll();
            }
        }

        internal class EPLJoinPatternJoinOutputRate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(1000);

                // no iterator allowed
                var stmtText = "@Name('s0') select count(*) as num " +
                               "from pattern [every timer:at(*/1,*,*,*,*)] unidirectional,\n" +
                               "SupportBean(IntPrimitive=1)#unique(TheString) a,\n" +
                               "SupportBean(IntPrimitive=2)#unique(TheString) b\n" +
                               "where a.TheString = b.TheString output every 2 minutes";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                SendEvent(env, "A", 1);
                SendEvent(env, "A", 2);
                SendEvent(env, "B", 1);
                SendEvent(env, "B", 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(70000);
                env.AdvanceTime(140000);

                env.AdvanceTime(210000);
                Assert.AreEqual(2L, env.Listener("s0").LastNewData[0].Get("num"));
                Assert.AreEqual(2L, env.Listener("s0").LastNewData[1].Get("num"));

                env.UndeployAll();
            }
        }

        internal class EPLJoin3TableJoinVar1 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select S0.Id, S1.Id, S2.Id " +
                               "from " +
                               "SupportBean_S0 as S0 unidirectional, " +
                               "SupportBean_S1#keepall as S1, " +
                               "SupportBean_S2#keepall as S2 " +
                               "where P00 = P10 and P10 = P20";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");
                Try3TableJoin(env);
                env.UndeployAll();
            }
        }

        internal class EPLJoin3TableJoinVar2A : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select S0.Id, S1.Id, S2.Id " +
                               "from " +
                               "SupportBean_S1#keepall as S1, " +
                               "SupportBean_S0 as S0 unidirectional, " +
                               "SupportBean_S2#keepall as S2 " +
                               "where P00 = P10 and P10 = P20";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");
                Try3TableJoin(env);
                env.UndeployAll();
            }
        }

        internal class EPLJoin3TableJoinVar2B : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select S0.Id, S1.Id, S2.Id " +
                               "from " +
                               "SupportBean_S2#keepall as S2, " +
                               "SupportBean_S0 as S0 unidirectional, " +
                               "SupportBean_S1#keepall as S1 " +
                               "where P00 = P10 and P10 = P20";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");
                Try3TableJoin(env);
                env.UndeployAll();
            }
        }

        internal class EPLJoin3TableJoinVar3 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select S0.Id, S1.Id, S2.Id " +
                               "from " +
                               "SupportBean_S1#keepall as S1, " +
                               "SupportBean_S2#keepall as S2, " +
                               "SupportBean_S0 as S0 unidirectional " +
                               "where P00 = P10 and P10 = P20";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");
                Try3TableJoin(env);
                env.UndeployAll();
            }
        }

        internal class EPLJoin2TableFullOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, Volume, TheString, IntPrimitive " +
                               "from SupportMarketDataBean unidirectional " +
                               "full outer join SupportBean#keepall on TheString = Symbol";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");
                TryFullOuterPassive2Stream(env);
                env.UndeployAll();
            }
        }

        internal class EPLJoin2TableFullOuterJoinCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, Volume, TheString, IntPrimitive " +
                               "from SupportMarketDataBean unidirectional " +
                               "full outer join SupportBean#keepall on TheString = Symbol";
                env.EplToModelCompileDeploy(stmtText).AddListener("s0");

                TryFullOuterPassive2Stream(env);

                env.UndeployAll();
            }
        }

        internal class EPLJoin2TableFullOuterJoinOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("Symbol", "Volume", "TheString", "IntPrimitive");
                model.FromClause = FromClause.Create(
                    FilterStream.Create(typeof(SupportMarketDataBean).Name).Unidirectional(true));
                model.FromClause.Add(FilterStream.Create(typeof(SupportBean).Name).AddView("keepall"));
                model.FromClause.Add(OuterJoinQualifier.Create("TheString", OuterJoinType.FULL, "Symbol"));

                var stmtText = "select Symbol, Volume, TheString, IntPrimitive " +
                               "from SupportMarketDataBean unidirectional " +
                               "full outer join SupportBean" +
                               "#keepall on TheString = Symbol";
                Assert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                TryFullOuterPassive2Stream(env);

                env.UndeployAll();
            }
        }

        internal class EPLJoin2TableFullOuterJoinBackwards : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, Volume, TheString, IntPrimitive " +
                               "from SupportBean#keepall full outer join " +
                               "SupportMarketDataBean unidirectional " +
                               "on TheString = Symbol";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                TryFullOuterPassive2Stream(env);

                env.UndeployAll();
            }
        }

        internal class EPLJoin2TableJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, Volume, TheString, IntPrimitive " +
                               "from SupportMarketDataBean unidirectional, SupportBean" +
                               "#keepall where TheString = Symbol";

                TryJoinPassive2Stream(env, stmtText);
            }
        }

        internal class EPLJoin2TableBackwards : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, Volume, TheString, IntPrimitive " +
                               "from SupportBean#keepall, SupportMarketDataBean unidirectional " +
                               "where TheString = Symbol";

                TryJoinPassive2Stream(env, stmtText);
            }
        }

        internal class EPLJoinInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "select * from SupportBean unidirectional " +
                           "full outer join SupportMarketDataBean#keepall unidirectional " +
                           "on TheString = Symbol";
                TryInvalidCompile(
                    env,
                    text,
                    "The unidirectional keyword requires that no views are declared onto the stream (applies to stream 1)");

                text = "select * from SupportBean#length(2) unidirectional " +
                       "full outer join SupportMarketDataBean#keepall " +
                       "on TheString = Symbol";
                TryInvalidCompile(
                    env,
                    text,
                    "The unidirectional keyword requires that no views are declared onto the stream");
            }
        }
    }
} // end of namespace