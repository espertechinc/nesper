///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.suite.epl.subselect.EPLSubselectAggregatedMultirowAndColumn;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectAggregatedMultirowAndColumn
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLSubselectMultirowGroupedNoDataWindowUncorrelated());
            execs.Add(new EPLSubselectMultirowGroupedCorrelatedWithEnumMethod());
            execs.Add(new EPLSubselectMultirowGroupedUncorrelatedWithEnumerationMethod());
            execs.Add(new EPLSubselectMultirowGroupedCorrelatedWHaving());
            execs.Add(new EPLSubselectMultirowGroupedNamedWindowSubqueryIndexShared());
            execs.Add(new EPLSubselectMulticolumnGroupedUncorrelatedUnfiltered());
            execs.Add(new EPLSubselectMultirowGroupedUncorrelatedIteratorAndExpressionDef());
            execs.Add(new EPLSubselectMulticolumnGroupedContextPartitioned());
            execs.Add(new EPLSubselectMulticolumnGroupedWHaving());
            execs.Add(new EPLSubselectMulticolumnInvalid());
            execs.Add(new EPLSubselectMulticolumnGroupBy());
            execs.Add(new EPLSubselectMultirowGroupedMultikeyWArray());
            execs.Add(new EPLSubselectMultirowGroupedIndexSharedMultikeyWArray());
            return execs;
        }

        internal static void RunAssertionNoDelete(
            RegressionEnvironment env,
            string fieldName,
            string[] fields)
        {
            env.SendEventBean(new SupportBean_S0(1));
            AssertMapFieldAndReset(env, fieldName, fields, null);

            SendSBEventAndTrigger(env, "E1", 10);
            AssertMapFieldAndReset(
                env,
                fieldName,
                fields,
                new object[] {"E1", 10});

            SendSBEventAndTrigger(env, "E1", 20);
            AssertMapFieldAndReset(
                env,
                fieldName,
                fields,
                new object[] {"E1", 30});

            // second group - this returns null as subquerys cannot return multiple rows (unless enumerated) (sql standard)
            SendSBEventAndTrigger(env, "E2", 5);
            AssertMapFieldAndReset(env, fieldName, fields, null);
        }

        internal static void SendSBEventAndTrigger(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            SendSBEventAndTrigger(env, theString, intPrimitive, 0);
        }

        internal static void SendSBEventAndTrigger(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
            env.SendEventBean(new SupportBean_S0(0));
        }

        internal static void SendS1EventAndTrigger(
            RegressionEnvironment env,
            int id)
        {
            env.SendEventBean(new SupportBean_S1(id, "x"));
            env.SendEventBean(new SupportBean_S0(0));
        }

        internal static void AssertMapFieldAndReset(
            RegressionEnvironment env,
            string fieldName,
            string[] names,
            object[] values)
        {
            AssertMapField(fieldName, env.Listener("s0").AssertOneGetNew(), names, values);
            env.Listener("s0").Reset();
        }

        internal static void AssertMapMultiRowAndReset(
            RegressionEnvironment env,
            string fieldName,
            string sortKey,
            string[] names,
            object[][] values)
        {
            AssertMapMultiRow(fieldName, env.Listener("s0").AssertOneGetNew(), sortKey, names, values);
            env.Listener("s0").Reset();
        }

        internal static void AssertMapField(
            string fieldName,
            EventBean @event,
            string[] names,
            object[] values)
        {
            var subq = (IDictionary<string, object>) @event.Get(fieldName);
            if (values == null && subq == null) {
                return;
            }

            EPAssertionUtil.AssertPropsMap(subq, names, values);
        }

        internal static void AssertMapMultiRow(
            String fieldName,
            EventBean @event,
            String sortKey,
            String[] names,
            Object[][] values)
        {
            var maps = GetSortMapMultiRow(fieldName, @event, sortKey);
            if (values == null && maps == null) {
                return;
            }

            EPAssertionUtil.AssertPropsPerRow(maps, names, values);
        }

        internal static IDictionary<string, object>[] GetSortMapMultiRow(
            String fieldName,
            EventBean @event,
            String sortKey)
        {
            var subq = @event.Get(fieldName) as ICollection<IDictionary<string, object>>;
            return subq?.OrderBy(v => v.Get(sortKey)).ToArray();
        }

        internal static void SendManyArray(
            RegressionEnvironment env,
            String id,
            int[] ints,
            int value)
        {
            env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(ints).WithValue(value));
        }

        internal class EPLSubselectMultirowGroupedIndexSharedMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test uncorrelated
                RegressionPath path = new RegressionPath();
                string epl = "@Hint('enable_window_subquery_indexshare') create window MyWindow#keepall as SupportEventWithManyArray;\n" +
                             "insert into MyWindow select * from SupportEventWithManyArray;\n";
                env.CompileDeploy(epl, path);

                SendManyArray(env, "E1", new int[] {1, 2}, 10);
                SendManyArray(env, "E2", new int[] {1}, 20);
                SendManyArray(env, "E3", new int[] {1}, 21);
                SendManyArray(env, "E4", new int[] {1, 2}, 11);

                epl = "@Name('s0') select " +
                      "(select IntOne as c0, sum(value) as c1 from MyWindow group by IntOne).take(10) as e1 from SupportBean_S0";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1));
                var maps = GetSortMapMultiRow("e1", env.Listener("s0").AssertOneGetNewAndReset(), "c1");
                Assert.IsTrue(Arrays.Equals(new int[] {1, 2}, (int[]) maps[0].Get("c0")));
                Assert.AreEqual(21, maps[0].Get("c1"));
                Assert.IsTrue(Arrays.Equals(new int[] {1}, (int[]) maps[1].Get("c0")));
                Assert.AreEqual(41, maps[1].Get("c1"));

                env.UndeployAll();
            }
        }

        internal class EPLSubselectMultirowGroupedMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('s0') select (select sum(value) as c0 from SupportEventWithIntArray#keepall group by array) as subq from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportEventWithIntArray("E1", new int[] {1, 2}, 10));
                env.SendEventBean(new SupportEventWithIntArray("E2", new int[] {1, 2}, 11));

                env.Milestone(0);

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "subq".SplitCsv(), new object[] {21});

                env.SendEventBean(new SupportEventWithIntArray("E3", new int[] {1, 2}, 12));
                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "subq".SplitCsv(), new object[] {33});

                env.Milestone(1);

                env.SendEventBean(new SupportEventWithIntArray("E4", new int[] {1}, 13));
                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "subq".SplitCsv(), new object[] {null});

                env.UndeployAll();
            }
        }

        public class EPLSubselectMulticolumnInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Invalid tests
                string epl;
                // not fully aggregated
                epl =
                    "select (select TheString, sum(LongPrimitive) from SupportBean#keepall group by IntPrimitive) from SupportBean_S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires non-aggregated properties in the select-clause to also appear in the group-by clause [select (select TheString, sum(LongPrimitive) from SupportBean#keepall group by IntPrimitive) from SupportBean_S0]");

                // correlated group-by not allowed
                epl =
                    "select (select TheString, sum(LongPrimitive) from SupportBean#keepall group by TheString, S0.Id) from SupportBean_S0 as S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires that group-by properties are provided by the subselect stream only (property 'Id' is not) [select (select TheString, sum(LongPrimitive) from SupportBean#keepall group by TheString, S0.Id) from SupportBean_S0 as S0]");

                epl =
                    "select (select TheString, sum(LongPrimitive) from SupportBean#keepall group by TheString, S0.GetP00()) from SupportBean_S0 as S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires that group-by properties are provided by the subselect stream only (expression 'S0.GetP00()' against stream 1 is not)");

                // aggregations not allowed in group-by
                epl =
                    "select (select IntPrimitive, sum(LongPrimitive) from SupportBean#keepall group by sum(IntPrimitive)) from SupportBean_S0 as S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to plan subquery number 1 querying SupportBean: Group-by expressions in a subselect may not have an aggregation function [select (select IntPrimitive, sum(LongPrimitive) from SupportBean#keepall group by sum(IntPrimitive)) from SupportBean_S0 as S0]");

                // "prev" not allowed in group-by
                epl =
                    "select (select IntPrimitive, sum(LongPrimitive) from SupportBean#keepall group by prev(1, IntPrimitive)) from SupportBean_S0 as S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to plan subquery number 1 querying SupportBean: Group-by expressions in a subselect may not have a function that requires view resources (prior, prev) [select (select IntPrimitive, sum(LongPrimitive) from SupportBean#keepall group by prev(1, IntPrimitive)) from SupportBean_S0 as S0]");
            }
        }

        internal class EPLSubselectMulticolumnGroupedWHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1"};
                var epl =
                    "@Name('s0') @Name('s0')select (select TheString as c0, sum(IntPrimitive) as c1 from SupportBean#keepall group by TheString having sum(IntPrimitive) > 10) as subq from SupportBean_S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendSBEventAndTrigger(env, "E1", 10);
                AssertMapFieldAndReset(env, "subq", fields, null);

                SendSBEventAndTrigger(env, "E2", 5);
                AssertMapFieldAndReset(env, "subq", fields, null);

                SendSBEventAndTrigger(env, "E2", 6);
                AssertMapFieldAndReset(
                    env,
                    "subq",
                    fields,
                    new object[] {"E2", 11});

                SendSBEventAndTrigger(env, "E1", 1);
                AssertMapFieldAndReset(env, "subq", fields, null);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectMulticolumnGroupedContextPartitioned : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldName = "subq";
                var fields = new[] {"c0", "c1"};

                var epl =
                    "create context MyCtx partition by TheString from SupportBean, P00 from SupportBean_S0;\n" +
                    "@Name('s0') context MyCtx select " +
                    "(select TheString as c0, sum(IntPrimitive) as c1 " +
                    " from SupportBean#keepall " +
                    " group by TheString) as subq " +
                    "from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("P1", 100));
                env.SendEventBean(new SupportBean_S0(1, "P1"));
                AssertMapFieldAndReset(
                    env,
                    fieldName,
                    fields,
                    new object[] {"P1", 100});

                env.SendEventBean(new SupportBean_S0(2, "P2"));
                AssertMapFieldAndReset(env, fieldName, fields, null);

                env.SendEventBean(new SupportBean("P2", 200));
                env.SendEventBean(new SupportBean_S0(3, "P2"));
                AssertMapFieldAndReset(
                    env,
                    fieldName,
                    fields,
                    new object[] {"P2", 200});

                env.SendEventBean(new SupportBean("P2", 205));
                env.SendEventBean(new SupportBean_S0(4, "P2"));
                AssertMapFieldAndReset(
                    env,
                    fieldName,
                    fields,
                    new object[] {"P2", 405});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectMulticolumnGroupedUncorrelatedUnfiltered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var fieldName = "subq";
                var fields = new[] {"c0", "c1"};
                var path = new RegressionPath();

                var eplNoDelete = "@Name('s0') select " +
                                  "(select TheString as c0, sum(IntPrimitive) as c1 " +
                                  "from SupportBean#keepall " +
                                  "group by TheString) as subq " +
                                  "from SupportBean_S0 as S0";
                env.CompileDeploy(eplNoDelete, path).AddListener("s0").MilestoneInc(milestone);
                RunAssertionNoDelete(env, fieldName, fields);
                env.UndeployAll();

                // try SODA
                var model = env.EplToModel(eplNoDelete);
                Assert.AreEqual(eplNoDelete, model.ToEPL());
                env.CompileDeploy(model, path).AddListener("s0").MilestoneInc(milestone);
                RunAssertionNoDelete(env, fieldName, fields);
                env.UndeployAll();

                // test named window with delete/remove
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportBean_S1 delete from MyWindow where Id = IntPrimitive;\n" +
                          "@Name('s0') @Hint('disable_reclaim_group') select (select TheString as c0, sum(IntPrimitive) as c1 " +
                          " from MyWindow group by TheString) as subq from SupportBean_S0 as S0";
                env.CompileDeploy(epl, path).AddListener("s0").MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(1));
                AssertMapFieldAndReset(env, fieldName, fields, null);

                SendSBEventAndTrigger(env, "E1", 10);
                AssertMapFieldAndReset(
                    env,
                    fieldName,
                    fields,
                    new object[] {"E1", 10});

                SendS1EventAndTrigger(env, 10); // delete 10
                AssertMapFieldAndReset(env, fieldName, fields, null);

                SendSBEventAndTrigger(env, "E2", 20);
                AssertMapFieldAndReset(
                    env,
                    fieldName,
                    fields,
                    new object[] {"E2", 20});

                SendSBEventAndTrigger(env, "E2", 21);
                AssertMapFieldAndReset(
                    env,
                    fieldName,
                    fields,
                    new object[] {"E2", 41});

                SendSBEventAndTrigger(env, "E1", 30);
                AssertMapFieldAndReset(env, fieldName, fields, null);

                SendS1EventAndTrigger(env, 30); // delete 30
                AssertMapFieldAndReset(
                    env,
                    fieldName,
                    fields,
                    new object[] {"E2", 41});

                SendS1EventAndTrigger(env, 20); // delete 20
                AssertMapFieldAndReset(
                    env,
                    fieldName,
                    fields,
                    new object[] {"E2", 21});

                SendSBEventAndTrigger(env, "E1", 31); // two groups
                AssertMapFieldAndReset(env, fieldName, fields, null);

                SendS1EventAndTrigger(env, 21); // delete 21
                AssertMapFieldAndReset(
                    env,
                    fieldName,
                    fields,
                    new object[] {"E1", 31});
                env.UndeployAll();

                // test multiple group-by criteria
                var fieldsMultiGroup = new[] {"c0", "c1", "c2", "c3", "c4"};
                var eplMultiGroup = "@Name('s0') select " +
                                    "(select TheString as c0, IntPrimitive as c1, TheString||'x' as c2, " +
                                    "    IntPrimitive * 1000 as c3, sum(LongPrimitive) as c4 " +
                                    " from SupportBean#keepall " +
                                    " group by TheString, IntPrimitive) as subq " +
                                    "from SupportBean_S0 as S0";
                env.CompileDeploy(eplMultiGroup, path).AddListener("s0");

                SendSBEventAndTrigger(env, "G1", 1, 100L);
                AssertMapFieldAndReset(
                    env,
                    fieldName,
                    fieldsMultiGroup,
                    new object[] {"G1", 1, "G1x", 1000, 100L});

                env.MilestoneInc(milestone);

                SendSBEventAndTrigger(env, "G1", 1, 101L);
                AssertMapFieldAndReset(
                    env,
                    fieldName,
                    fieldsMultiGroup,
                    new object[] {"G1", 1, "G1x", 1000, 201L});

                SendSBEventAndTrigger(env, "G2", 1, 200L);
                AssertMapFieldAndReset(env, fieldName, fieldsMultiGroup, null);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectMultirowGroupedCorrelatedWHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldName = "subq";
                var fields = new[] {"c0", "c1"};

                var eplEnumCorrelated = "@Name('s0') select " +
                                        "(select TheString as c0, sum(IntPrimitive) as c1 " +
                                        " from SupportBean#keepall " +
                                        " where IntPrimitive = S0.Id " +
                                        " group by TheString" +
                                        " having sum(IntPrimitive) > 10).take(100) as subq " +
                                        "from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMileZero(eplEnumCorrelated, "s0");

                env.SendEventBean(new SupportBean_S0(1));
                AssertMapMultiRowAndReset(env, fieldName, "c0", fields, null);

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 10));
                env.SendEventBean(new SupportBean("E3", 10));
                env.SendEventBean(new SupportBean_S0(10));
                AssertMapMultiRowAndReset(env, fieldName, "c0", fields, null);

                env.SendEventBean(new SupportBean("E2", 10));
                env.SendEventBean(new SupportBean_S0(10));
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E2", 20}});

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean_S0(10));
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E1", 20}, new object[] {"E2", 20}});

                env.SendEventBean(new SupportBean("E3", 55));
                env.SendEventBean(new SupportBean_S0(10));
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E1", 20}, new object[] {"E2", 20}});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectMultirowGroupedCorrelatedWithEnumMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldName = "subq";
                var fields = new[] {"c0", "c1"};

                var eplEnumCorrelated = "@Name('s0') select " +
                                        "(select TheString as c0, sum(IntPrimitive) as c1 " +
                                        " from SupportBean#keepall " +
                                        " where IntPrimitive = S0.Id " +
                                        " group by TheString).take(100) as subq " +
                                        "from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMileZero(eplEnumCorrelated, "s0");

                env.SendEventBean(new SupportBean_S0(1));
                AssertMapMultiRowAndReset(env, fieldName, "c0", fields, null);

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean_S0(10));
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E1", 10}});

                env.SendEventBean(new SupportBean_S0(11));
                AssertMapMultiRowAndReset(env, fieldName, "c0", fields, null);

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean_S0(10));
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E1", 20}});

                env.SendEventBean(new SupportBean("E2", 100));
                env.SendEventBean(new SupportBean_S0(100));
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E2", 100}});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectMultirowGroupedNamedWindowSubqueryIndexShared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test uncorrelated
                var path = new RegressionPath();
                var epl = "@Hint('enable_window_subquery_indexshare')" +
                          "create window SBWindow#keepall as SupportBean;\n" +
                          "insert into SBWindow select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E1", 20));

                var stmtUncorrelated = "@Name('s0') select " +
                                       "(select TheString as c0, sum(IntPrimitive) as c1 from SBWindow group by TheString).take(10) as e1 from SupportBean_S0";
                env.CompileDeploy(stmtUncorrelated, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1));
                AssertMapMultiRow(
                    "e1",
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0",
                    new[] {"c0", "c1"},
                    new[] {new object[] {"E1", 30}});

                env.SendEventBean(new SupportBean("E2", 200));
                env.SendEventBean(new SupportBean_S0(2));
                AssertMapMultiRow(
                    "e1",
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0",
                    new[] {"c0", "c1"},
                    new[] {new object[] {"E1", 30}, new object[] {"E2", 200}});
                env.UndeployModuleContaining("s0");

                // test correlated
                var eplTwo = "@Name('s0') select " +
                             "(select TheString as c0, sum(IntPrimitive) as c1 from SBWindow where TheString = S0.P00 group by TheString).take(10) as e1 from SupportBean_S0 as S0";
                env.CompileDeploy(eplTwo, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                AssertMapMultiRow(
                    "e1",
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0",
                    new[] {"c0", "c1"},
                    new[] {new object[] {"E1", 30}});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectMultirowGroupedNoDataWindowUncorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select (select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString).take(10) as subq from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = new[] {"c0", "c1"};

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                AssertMapMultiRow("subq", env.Listener("s0").AssertOneGetNewAndReset(), "c0", fields, null);

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean_S0(2, "E2"));
                AssertMapMultiRow(
                    "subq",
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0",
                    fields,
                    new[] {new object[] {"G1", 10}});

                env.SendEventBean(new SupportBean("G2", 20));
                env.SendEventBean(new SupportBean_S0(3, "E3"));
                AssertMapMultiRow(
                    "subq",
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0",
                    fields,
                    new[] {new object[] {"G1", 10}, new object[] {"G2", 20}});

                env.UndeployAll();
            }
        }

        internal class EPLSubselectMultirowGroupedUncorrelatedIteratorAndExpressionDef : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1"};
                var epl = "@Name('s0') expression getGroups {" +
                          "(select TheString as c0, sum(IntPrimitive) as c1 " +
                          "  from SupportBean#keepall group by TheString)" +
                          "}" +
                          "select getGroups() as e1, getGroups().take(10) as e2 from SupportBean_S0#lastevent()";
                env.CompileDeploy(epl).AddListener("s0");

                SendSBEventAndTrigger(env, "E1", 20);
                foreach (var @event in new[] {env.Listener("s0").AssertOneGetNew(), env.Statement("s0").First()}) {
                    AssertMapField(
                        "e1",
                        @event,
                        fields,
                        new object[] {"E1", 20});
                    AssertMapMultiRow(
                        "e2",
                        @event,
                        "c0",
                        fields,
                        new[] {new object[] {"E1", 20}});
                }

                env.Listener("s0").Reset();

                SendSBEventAndTrigger(env, "E2", 30);
                foreach (var @event in new[] {env.Listener("s0").AssertOneGetNew(), env.Statement("s0").First()}) {
                    AssertMapField("e1", @event, fields, null);
                    AssertMapMultiRow(
                        "e2",
                        @event,
                        "c0",
                        fields,
                        new[] {new object[] {"E1", 20}, new object[] {"E2", 30}});
                }

                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class EPLSubselectMultirowGroupedUncorrelatedWithEnumerationMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldName = "subq";
                var fields = new[] {"c0", "c1"};

                // test unfiltered
                var eplEnumUnfiltered = "@Name('s0') select " +
                                        "(select TheString as c0, sum(IntPrimitive) as c1 " +
                                        " from SupportBean#keepall " +
                                        " group by TheString).take(100) as subq " +
                                        "from SupportBean_S0 as S0";
                env.CompileDeploy(eplEnumUnfiltered).AddListener("s0").Milestone(0);

                env.SendEventBean(new SupportBean_S0(1));
                AssertMapMultiRowAndReset(env, fieldName, "c0", fields, null);

                SendSBEventAndTrigger(env, "E1", 10);
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E1", 10}});

                SendSBEventAndTrigger(env, "E1", 20);
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E1", 30}});

                SendSBEventAndTrigger(env, "E2", 100);
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E1", 30}, new object[] {"E2", 100}});

                SendSBEventAndTrigger(env, "E3", 2000);
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E1", 30}, new object[] {"E2", 100}, new object[] {"E3", 2000}});
                env.UndeployAll();

                // test filtered
                var eplEnumFiltered = "@Name('s0') select " +
                                      "(select TheString as c0, sum(IntPrimitive) as c1 " +
                                      " from SupportBean#keepall " +
                                      " where IntPrimitive > 100 " +
                                      " group by TheString).take(100) as subq " +
                                      "from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMile(eplEnumFiltered, "s0", 1);

                env.SendEventBean(new SupportBean_S0(1));
                AssertMapMultiRowAndReset(env, fieldName, "c0", fields, null);

                SendSBEventAndTrigger(env, "E1", 10);
                AssertMapMultiRowAndReset(env, fieldName, "c0", fields, null);

                SendSBEventAndTrigger(env, "E1", 200);
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E1", 200}});

                SendSBEventAndTrigger(env, "E1", 11);
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E1", 200}});

                SendSBEventAndTrigger(env, "E1", 201);
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E1", 401}});

                SendSBEventAndTrigger(env, "E2", 300);
                AssertMapMultiRowAndReset(
                    env,
                    fieldName,
                    "c0",
                    fields,
                    new[] {new object[] {"E1", 401}, new object[] {"E2", 300}});

                env.UndeployAll();
            }
        }

        public class EPLSubselectMulticolumnGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select (select TheString as c0, sum(IntPrimitive) as c1 " +
                          "from SupportBean#keepall() group by TheString).take(10) as e1 from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1));
                AssertMapMultiRow(
                    "e1",
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0",
                    new[] {"c0", "c1"},
                    null);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean_S0(2));
                AssertMapMultiRow(
                    "e1",
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0",
                    new[] {"c0", "c1"},
                    new[] {new object[] {"E1", 10}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 200));
                env.SendEventBean(new SupportBean_S0(3));
                AssertMapMultiRow(
                    "e1",
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0",
                    new[] {"c0", "c1"},
                    new[] {new object[] {"E1", 10}, new object[] {"E2", 200}});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E1", 20));
                env.SendEventBean(new SupportBean_S0(4));
                AssertMapMultiRow(
                    "e1",
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0",
                    new[] {"c0", "c1"},
                    new[] {new object[] {"E1", 30}, new object[] {"E2", 200}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace