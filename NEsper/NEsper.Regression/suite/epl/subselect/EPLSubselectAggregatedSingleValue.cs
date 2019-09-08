///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectAggregatedSingleValue
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLSubselectUngroupedUncorrelatedInSelect());
            execs.Add(new EPLSubselectUngroupedUncorrelatedTwoAggStopStart());
            execs.Add(new EPLSubselectUngroupedUncorrelatedNoDataWindow());
            execs.Add(new EPLSubselectUngroupedUncorrelatedWHaving());
            execs.Add(new EPLSubselectUngroupedUncorrelatedInWhereClause());
            execs.Add(new EPLSubselectUngroupedUncorrelatedInSelectClause());
            execs.Add(new EPLSubselectUngroupedUncorrelatedFiltered());
            execs.Add(new EPLSubselectUngroupedUncorrelatedWWhereClause());
            execs.Add(new EPLSubselectUngroupedCorrelated());
            execs.Add(new EPLSubselectUngroupedCorrelatedSceneTwo());
            execs.Add(new EPLSubselectUngroupedCorrelatedInWhereClause());
            execs.Add(new EPLSubselectUngroupedCorrelatedWHaving());
            execs.Add(new EPLSubselectUngroupedJoin3StreamKeyRangeCoercion());
            execs.Add(new EPLSubselectUngroupedJoin2StreamRangeCoercion());
            execs.Add(new EPLSubselectGroupedUncorrelatedWHaving());
            execs.Add(new EPLSubselectGroupedCorrelatedWHaving());
            execs.Add(new EPLSubselectGroupedCorrelationInsideHaving());
            execs.Add(new EPLSubselectAggregatedInvalid());
            execs.Add(new EPLSubselectUngroupedCorrelationInsideHaving());
            execs.Add(new EPLSubselectUngroupedTableWHaving());
            execs.Add(new EPLSubselectGroupedTableWHaving());
            return execs;
        }

        private static void RunAssertionSumFilter(RegressionEnvironment env)
        {
            SendEventS0(env, 1);
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

            SendEventS1(env, 1);
            SendEventS0(env, 2);
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

            SendEventS1(env, 0);
            SendEventS0(env, 3);
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

            SendEventS1(env, -1);
            SendEventS0(env, 4);
            Assert.AreEqual(-1, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

            SendEventS1(env, -3);
            SendEventS0(env, 5);
            Assert.AreEqual(-4, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

            SendEventS1(env, -5);
            SendEventS0(env, 6);
            Assert.AreEqual(-9, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

            SendEventS1(env, -2); // note event leaving window
            SendEventS0(env, 6);
            Assert.AreEqual(-10, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));
        }

        private static void TryAssertion2StreamRangeCoercion(
            RegressionEnvironment env,
            AtomicLong milestone,
            string epl,
            bool isHasRangeReversal)
        {
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            env.SendEventBean(new SupportBean_ST0("ST01", 10L));
            env.SendEventBean(new SupportBean_ST1("ST11", 20L));
            env.SendEventBean(new SupportBean("E1", 9));
            env.SendEventBean(new SupportBean("E1", 21));
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi")); // range 10 to 20

            env.SendEventBean(new SupportBean("E1", 13));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_ST0("ST0_1", 10L)); // range 10 to 20
            Assert.AreEqual(13, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST1("ST1_1", 13L)); // range 10 to 13
            Assert.AreEqual(13, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST0("ST0_2", 13L)); // range 13 to 13
            Assert.AreEqual(13, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 14));
            env.SendEventBean(new SupportBean("E3", 12));
            env.SendEventBean(new SupportBean_ST1("ST1_3", 13L)); // range 13 to 13
            Assert.AreEqual(13, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST1("ST1_4", 20L)); // range 13 to 20
            Assert.AreEqual(27, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_ST0("ST0_3", 11L)); // range 11 to 20
            Assert.AreEqual(39, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST0("ST0_4", null)); // range null to 16
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_ST1("ST1_5", null)); // range null to null
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST0("ST0_5", 20L)); // range 20 to null
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST1("ST1_6", 13L)); // range 20 to 13
            if (isHasRangeReversal) {
                Assert.AreEqual(27, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));
            }
            else {
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));
            }

            env.UndeployAll();
        }

        private static void TryAssertion3StreamKeyRangeCoercion(
            RegressionEnvironment env,
            AtomicLong milestone,
            string epl,
            bool isHasRangeReversal)
        {
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            env.SendEventBean(new SupportBean("G", -1));
            env.SendEventBean(new SupportBean("G", 9));
            env.SendEventBean(new SupportBean("G", 21));
            env.SendEventBean(new SupportBean("G", 13));
            env.SendEventBean(new SupportBean("G", 17));
            env.SendEventBean(new SupportBean_ST2("ST21", "X", 0));
            env.SendEventBean(new SupportBean_ST0("ST01", 10L));
            env.SendEventBean(new SupportBean_ST1("ST11", 20L));
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi")); // range 10 to 20

            env.SendEventBean(new SupportBean_ST2("ST22", "G", 0));
            Assert.AreEqual(30, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST0("ST01", 0L)); // range 0 to 20
            Assert.AreEqual(39, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST2("ST21", null, 0));
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST2("ST21", "G", 0));
            Assert.AreEqual(39, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST1("ST11", 100L)); // range 0 to 100
            Assert.AreEqual(60, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST1("ST11", null)); // range 0 to null
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST0("ST01", null)); // range null to null
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST1("ST11", -1L)); // range null to -1
            Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

            env.SendEventBean(new SupportBean_ST0("ST01", 10L)); // range 10 to -1
            if (isHasRangeReversal) {
                Assert.AreEqual(8, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));
            }
            else {
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));
            }

            env.UndeployAll();
        }

        private static void RunAssertionCorrAggWhereGreater(RegressionEnvironment env)
        {
            var fields = new [] { "P00" };

            env.SendEventBean(new SupportBean_S0(1, "T1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean("T1", 10));

            env.SendEventBean(new SupportBean_S0(10, "T1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S0(11, "T1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"T1"});

            env.SendEventBean(new SupportBean("T1", 11));
            env.SendEventBean(new SupportBean_S0(21, "T1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S0(22, "T1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"T1"});
        }

        private static void SendEventS0(
            RegressionEnvironment env,
            int id)
        {
            env.SendEventBean(new SupportBean_S0(id));
        }

        private static void SendEventS0(
            RegressionEnvironment env,
            int id,
            string p00)
        {
            env.SendEventBean(new SupportBean_S0(id, p00));
        }

        private static void SendEventS1(
            RegressionEnvironment env,
            int id,
            string p10,
            string p11)
        {
            env.SendEventBean(new SupportBean_S1(id, p10, p11));
        }

        private static void SendEventS1(
            RegressionEnvironment env,
            int id)
        {
            env.SendEventBean(new SupportBean_S1(id));
        }

        private static object SendEventMD(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            object theEvent = new SupportMarketDataBean(symbol, price, 0L, "");
            env.SendEventBean(theEvent);
            return theEvent;
        }

        private static void SendSB(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }

        private static void SendEventS0Assert(
            RegressionEnvironment env,
            object expected)
        {
            SendEventS0Assert(env, 0, expected);
        }

        private static void SendEventS0Assert(
            RegressionEnvironment env,
            int id,
            object expected)
        {
            SendEventS0(env, id, null);
            Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
        }

        private static void SendEventS0Assert(
            RegressionEnvironment env,
            string p00,
            object expected)
        {
            SendEventS0(env, 0, p00);
            Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
        }

        private static void SendEventS0Assert(
            RegressionEnvironment env,
            string[] fields,
            object[] expected)
        {
            env.SendEventBean(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);
        }

        public class EPLSubselectAggregatedInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // invalid tests
                string stmtText;

                stmtText =
                    "select (select sum(S0.Id) from SupportBean_S1#length(3) as S1) as value from SupportBean_S0 as S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    stmtText,
                    "Failed to plan subquery number 1 querying SupportBean_S1: Subselect aggregation functions cannot aggregate across correlated properties");

                stmtText =
                    "select (select S1.Id + sum(S1.Id) from SupportBean_S1#length(3) as S1) as value from SupportBean_S0 as S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    stmtText,
                    "Failed to plan subquery number 1 querying SupportBean_S1: Subselect properties must all be within aggregation functions");

                stmtText =
                    "select (select sum(S0.Id + S1.Id) from SupportBean_S1#length(3) as S1) as value from SupportBean_S0 as S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    stmtText,
                    "Failed to plan subquery number 1 querying SupportBean_S1: Subselect aggregation functions cannot aggregate across correlated properties");

                // having-clause cannot aggregate over properties from other streams
                stmtText =
                    "select (select TheString from SupportBean#keepall having sum(S0.P00) = 1) as c0 from SupportBean_S0 as S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    stmtText,
                    "Failed to plan subquery number 1 querying SupportBean: Failed to validate having-clause expression '(sum(S0.P00))=1': Implicit conversion from datatype 'String' to numeric is not allowed for aggregation function 'sum' [");

                // having-clause properties must be aggregated
                stmtText =
                    "select (select TheString from SupportBean#keepall having sum(IntPrimitive) = IntPrimitive) as c0 from SupportBean_S0 as S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    stmtText,
                    "Failed to plan subquery number 1 querying SupportBean: Subselect having-clause requires that all properties are under aggregation, consider using the 'first' aggregation function instead");

                // having-clause not returning boolean
                stmtText =
                    "select (select TheString from SupportBean#keepall having sum(IntPrimitive)) as c0 from SupportBean_S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    stmtText,
                    "Failed to plan subquery number 1 querying SupportBean: Subselect having-clause expression must return a boolean value ");
            }
        }

        internal class EPLSubselectGroupedCorrelationInsideHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') @Name('s0')select (select TheString from SupportBean#keepall group by TheString having sum(IntPrimitive) = S0.Id) as c0 from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendSB(env, "E1", 100);
                SendSB(env, "E2", 5);
                SendSB(env, "E3", 20);
                SendEventS0Assert(env, 1, null);
                SendEventS0Assert(env, 5, "E2");

                SendSB(env, "E2", 3);
                SendEventS0Assert(env, 5, null);
                SendEventS0Assert(env, 8, "E2");
                SendEventS0Assert(env, 20, "E3");

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedCorrelationInsideHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') @Name('s0')select (select last(TheString) from SupportBean#keepall having sum(IntPrimitive) = S0.Id) as c0 from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendSB(env, "E1", 100);
                SendEventS0Assert(env, 1, null);
                SendEventS0Assert(env, 100, "E1");

                SendSB(env, "E2", 5);
                SendEventS0Assert(env, 100, null);
                SendEventS0Assert(env, 105, "E2");

                env.UndeployAll();
            }
        }

        internal class EPLSubselectGroupedTableWHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create table MyTableWith2Keys(k1 string primary key, k2 string primary key, total sum(int));\n" +
                    "into table MyTableWith2Keys select P10 as k1, P11 as k2, sum(Id) as total from SupportBean_S1 group by P10, P11;\n" +
                    "@Name('s0') @Name('s0')select (select sum(total) from MyTableWith2Keys group by k1 having sum(total) > 100) as c0 from SupportBean_S0;\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEventS1(env, 50, "G1", "S1");
                SendEventS1(env, 50, "G1", "S2");
                SendEventS1(env, 50, "G2", "S1");
                SendEventS1(env, 50, "G2", "S2");
                SendEventS0Assert(env, null);

                SendEventS1(env, 1, "G2", "S3");
                SendEventS0Assert(env, 101);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedTableWHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table MyTable(total sum(int))", path);
                env.CompileDeploy("into table MyTable select sum(IntPrimitive) as total from SupportBean", path);
                env.CompileDeploy(
                    "@Name('s0') select (select sum(total) from MyTable having sum(total) > 100) as c0 from SupportBean_S0",
                    path);
                env.AddListener("s0");

                SendEventS0Assert(env, null);

                SendSB(env, "E1", 50);
                SendEventS0Assert(env, null);

                SendSB(env, "E2", 55);
                SendEventS0Assert(env, 105);

                SendSB(env, "E3", -5);
                SendEventS0Assert(env, null);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectGroupedCorrelatedWHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') @Name('s0')select (select sum(IntPrimitive) from SupportBean#keepall where S0.Id = IntPrimitive group by TheString having sum(IntPrimitive) > 10) as c0 from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEventS0Assert(env, 10, null);

                SendSB(env, "G1", 10);
                SendSB(env, "G2", 10);
                SendSB(env, "G2", 2);
                SendSB(env, "G1", 9);
                SendEventS0Assert(env, null);

                SendSB(env, "G2", 10);
                SendEventS0Assert(env, 10, 20);

                SendSB(env, "G1", 10);
                SendEventS0Assert(env, 10, null);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectGroupedUncorrelatedWHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') @Name('s0')select (select sum(IntPrimitive) from SupportBean#keepall group by TheString having sum(IntPrimitive) > 10) as c0 from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEventS0Assert(env, null);

                SendSB(env, "G1", 10);
                SendSB(env, "G2", 9);
                SendEventS0Assert(env, null);

                SendSB(env, "G2", 2);
                SendEventS0Assert(env, 11);

                SendSB(env, "G1", 3);
                SendEventS0Assert(env, null);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedCorrelatedWHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') @Name('s0')select (select sum(IntPrimitive) from SupportBean#keepall where TheString = S0.P00 having sum(IntPrimitive) > 10) as c0 from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEventS0Assert(env, "G1", null);

                SendSB(env, "G1", 10);
                SendEventS0Assert(env, "G1", null);

                SendSB(env, "G2", 11);
                SendEventS0Assert(env, "G1", null);
                SendEventS0Assert(env, "G2", 11);

                SendSB(env, "G1", 12);
                SendEventS0Assert(env, "G1", 22);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedUncorrelatedFiltered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select (select sum(Id) from SupportBean_S1(Id < 0)#length(3)) as value from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                RunAssertionSumFilter(env);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedUncorrelatedWWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select (select sum(Id) from SupportBean_S1#length(3) where Id < 0) as value from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                RunAssertionSumFilter(env);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedUncorrelatedNoDataWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select P00 as c0, (select sum(IntPrimitive) from SupportBean) as c1 from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var fields = new [] { "c0", "c1" };

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", null});

                env.SendEventBean(new SupportBean("", 10));
                env.SendEventBean(new SupportBean_S0(2, "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 10});

                env.SendEventBean(new SupportBean("", 20));
                env.SendEventBean(new SupportBean_S0(3, "E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 30});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedUncorrelatedWHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1" };
                var epl = "@Name('s0') @Name('s0')select *, " +
                          "(select sum(IntPrimitive) from SupportBean#keepall having sum(IntPrimitive) > 100) as c0," +
                          "exists (select sum(IntPrimitive) from SupportBean#keepall having sum(IntPrimitive) > 100) as c1 " +
                          "from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEventS0Assert(
                    env,
                    fields,
                    new object[] {null, false});
                SendSB(env, "E1", 10);
                SendEventS0Assert(
                    env,
                    fields,
                    new object[] {null, false});
                SendSB(env, "E1", 91);
                SendEventS0Assert(
                    env,
                    fields,
                    new object[] {101, true});
                SendSB(env, "E1", 2);
                SendEventS0Assert(
                    env,
                    fields,
                    new object[] {103, true});

                env.UndeployAll();
            }
        }

        public class EPLSubselectUngroupedCorrelatedSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Id", "mycount"};
                var text =
                    "@Name('s0') select Id, (select count(*) from SupportBean_S1#length(3) S1 where S1.P10 = S0.P00) as mycount from SupportBean_S0 S0";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "G1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, 0L});

                env.SendEventBean(new SupportBean_S1(200, "G2"));
                env.SendEventBean(new SupportBean_S0(2, "G2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, 1L});

                env.SendEventBean(new SupportBean_S1(201, "G2"));
                env.SendEventBean(new SupportBean_S0(3, "G2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3, 2L});

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(4, "G1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {4, 0L});

                env.SendEventBean(new SupportBean_S0(5, "G2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5, 2L});

                env.SendEventBean(new SupportBean_S0(6, "G3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {6, 0L});

                env.SendEventBean(new SupportBean_S1(202, "G2"));
                env.SendEventBean(new SupportBean_S0(7, "G2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {7, 3L});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(8, "G2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {8, 3L});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedCorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;
                var milestone = new AtomicLong();

                epl = "@Name('s0') select P00, " +
                      "(select sum(IntPrimitive) from SupportBean#keepall where TheString = S0.P00) as sumP00 " +
                      "from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var fields = new [] { "P00","sumP00" };

                env.SendEventBean(new SupportBean_S0(1, "T1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"T1", null});

                env.SendEventBean(new SupportBean("T1", 10));
                env.SendEventBean(new SupportBean_S0(2, "T1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"T1", 10});

                env.SendEventBean(new SupportBean("T1", 11));
                env.SendEventBean(new SupportBean_S0(3, "T1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"T1", 21});

                env.SendEventBean(new SupportBean_S0(4, "T2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"T2", null});

                env.SendEventBean(new SupportBean("T2", -2));
                env.SendEventBean(new SupportBean("T2", -7));
                env.SendEventBean(new SupportBean_S0(5, "T2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"T2", -9});
                env.UndeployAll();

                // test distinct
                fields = new [] { "TheString","c0","c1","c2","c3" };
                epl = "@Name('s0') @Name('s0')select TheString, " +
                      "(select count(sb.IntPrimitive) from SupportBean()#keepall as sb where bean.TheString = sb.TheString) as c0, " +
                      "(select count(distinct sb.IntPrimitive) from SupportBean()#keepall as sb where bean.TheString = sb.TheString) as c1, " +
                      "(select count(sb.IntPrimitive, true) from SupportBean()#keepall as sb where bean.TheString = sb.TheString) as c2, " +
                      "(select count(distinct sb.IntPrimitive, true) from SupportBean()#keepall as sb where bean.TheString = sb.TheString) as c3 " +
                      "from SupportBean as bean";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L, 1L, 1L, 1L});

                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 1L, 1L, 1L, 1L});

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2L, 2L, 2L, 2L});

                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 3L, 2L, 3L, 2L});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedJoin3StreamKeyRangeCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl = "@Name('s0') @Name('s0')select (" +
                          "select sum(IntPrimitive) as sumi from SupportBean#keepall where TheString = st2.Key2 and IntPrimitive between S0.P01Long and S1.P11Long) " +
                          "from SupportBean_ST2#lastevent st2, SupportBean_ST0#lastevent s0, SupportBean_ST1#lastevent s1";
                TryAssertion3StreamKeyRangeCoercion(env, milestone, epl, true);

                epl = "@Name('s0') select (" +
                      "select sum(IntPrimitive) as sumi from SupportBean#keepall where TheString = st2.Key2 and S1.P11Long >= IntPrimitive and S0.P01Long <= IntPrimitive) " +
                      "from SupportBean_ST2#lastevent st2, SupportBean_ST0#lastevent s0, SupportBean_ST1#lastevent s1";
                TryAssertion3StreamKeyRangeCoercion(env, milestone, epl, false);

                epl = "@Name('s0') select (" +
                      "select sum(IntPrimitive) as sumi from SupportBean#keepall where TheString = st2.Key2 and S1.P11Long > IntPrimitive) " +
                      "from SupportBean_ST2#lastevent st2, SupportBean_ST0#lastevent s0, SupportBean_ST1#lastevent s1";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean("G", 21));
                env.SendEventBean(new SupportBean("G", 13));
                env.SendEventBean(new SupportBean_ST2("ST2", "G", 0));
                env.SendEventBean(new SupportBean_ST0("ST0", -1L));
                env.SendEventBean(new SupportBean_ST1("ST1", 20L));
                Assert.AreEqual(13, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

                env.UndeployAll();
                epl = "@Name('s0') select (" +
                      "select sum(IntPrimitive) as sumi from SupportBean#keepall where TheString = st2.Key2 and S1.P11Long < IntPrimitive) " +
                      "from SupportBean_ST2#lastevent st2, SupportBean_ST0#lastevent s0, SupportBean_ST1#lastevent s1";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean("G", 21));
                env.SendEventBean(new SupportBean("G", 13));
                env.SendEventBean(new SupportBean_ST2("ST2", "G", 0));
                env.SendEventBean(new SupportBean_ST0("ST0", -1L));
                env.SendEventBean(new SupportBean_ST1("ST1", 20L));
                Assert.AreEqual(21, env.Listener("s0").AssertOneGetNewAndReset().Get("sumi"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedJoin2StreamRangeCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                // between and 'in' automatically revert the range (20 to 10 is the same as 10 to 20)
                var epl = "@Name('s0') select (" +
                          "select sum(IntPrimitive) as sumi from SupportBean#keepall where IntPrimitive between S0.P01Long and S1.P11Long) " +
                          "from SupportBean_ST0#lastevent s0, SupportBean_ST1#lastevent s1";
                TryAssertion2StreamRangeCoercion(env, milestone, epl, true);

                epl = "@Name('s0') select (" +
                      "select sum(IntPrimitive) as sumi from SupportBean#keepall where IntPrimitive between S1.P11Long and S0.P01Long) " +
                      "from SupportBean_ST1#lastevent s1, SupportBean_ST0#lastevent s0";
                TryAssertion2StreamRangeCoercion(env, milestone, epl, true);

                // >= and <= should not automatically revert the range
                epl = "@Name('s0') select (" +
                      "select sum(IntPrimitive) as sumi from SupportBean#keepall where IntPrimitive >= S0.P01Long and IntPrimitive <= S1.P11Long) " +
                      "from SupportBean_ST0#lastevent s0, SupportBean_ST1#lastevent s1";
                TryAssertion2StreamRangeCoercion(env, milestone, epl, false);
            }
        }

        internal class EPLSubselectUngroupedCorrelatedInWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select P00 from SupportBean_S0 as S0 where Id > " +
                          "(select sum(IntPrimitive) from SupportBean#keepall where TheString = S0.P00)";
                env.CompileDeployAddListenerMile(epl, "s0", 0);

                RunAssertionCorrAggWhereGreater(env);
                env.UndeployAll();

                epl = "@Name('s0') select P00 from SupportBean_S0 as S0 where Id > " +
                      "(select sum(IntPrimitive) from SupportBean#keepall where TheString||'X' = S0.P00||'X')";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                RunAssertionCorrAggWhereGreater(env);
                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedUncorrelatedInWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportMarketDataBean " +
                          "where Price > (select max(Price) from SupportMarketDataBean(Symbol='GOOG')#lastevent) ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEventMD(env, "GOOG", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEventMD(env, "GOOG", 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                var theEvent = SendEventMD(env, "IBM", 3);
                Assert.AreEqual(theEvent, env.Listener("s0").AssertOneGetNewAndReset().Underlying);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedUncorrelatedInSelectClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select (select S0.Id + max(S1.Id) from SupportBean_S1#length(3) as S1) as value from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEventS0(env, 1);
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                SendEventS1(env, 100);
                SendEventS0(env, 2);
                Assert.AreEqual(102, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                SendEventS1(env, 30);
                SendEventS0(env, 3);
                Assert.AreEqual(103, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedUncorrelatedInSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select (select max(Id) from SupportBean_S1#length(3)) as value from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEventS0(env, 1);
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                SendEventS1(env, 100);
                SendEventS0(env, 2);
                Assert.AreEqual(100, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                SendEventS1(env, 200);
                SendEventS0(env, 3);
                Assert.AreEqual(200, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                SendEventS1(env, 190);
                SendEventS0(env, 4);
                Assert.AreEqual(200, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                SendEventS1(env, 180);
                SendEventS0(env, 5);
                Assert.AreEqual(200, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                SendEventS1(env, 170); // note event leaving window
                SendEventS0(env, 6);
                Assert.AreEqual(190, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectUngroupedUncorrelatedTwoAggStopStart : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select (select avg(Id) + max(Id) from SupportBean_S1#length(3)) as value from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEventS0(env, 1);
                Assert.AreEqual(null, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                SendEventS1(env, 100);
                SendEventS0(env, 2);
                Assert.AreEqual(200.0, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                SendEventS1(env, 200);
                SendEventS0(env, 3);
                Assert.AreEqual(350.0, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                var listener = env.Listener("s0");
                env.UndeployAll();
                SendEventS1(env, 10000);
                SendEventS0(env, 4);
                Assert.IsFalse(listener.IsInvoked);
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEventS1(env, 10);
                SendEventS0(env, 5);
                Assert.AreEqual(20.0, env.Listener("s0").AssertOneGetNewAndReset().Get("value"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace