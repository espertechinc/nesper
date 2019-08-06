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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

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
            string fieldName,
            EventBean @event,
            string sortKey,
            string[] names,
            object[][] values)
        {
            var subq = (ICollection<IDictionary<string, object>>) @event.Get(fieldName);
            if (values == null && subq == null) {
                return;
            }

            var maps = subq.ToArray();
            Array.Sort(
                maps,
                (
                    o1,
                    o2) => {
                    return ((IComparable) o1.Get(sortKey)).CompareTo(o2.Get(sortKey));
                });
            EPAssertionUtil.AssertPropsPerRow(maps, names, values);
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
                "select (select TheString, sum(LongPrimitive) from SupportBean#keepall group by TheString, s0.Id) from SupportBean_S0 as s0";
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                epl,
                "Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires that group-by properties are provIded by the subselect stream only (property 'Id' is not) [select (select TheString, sum(LongPrimitive) from SupportBean#keepall group by TheString, s0.Id) from SupportBean_S0 as s0]");
            epl =
                "select (select TheString, sum(LongPrimitive) from SupportBean#keepall group by TheString, s0.getP00()) from SupportBean_S0 as s0";
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                epl,
                "Failed to plan subquery number 1 querying SupportBean: Subselect with group-by requires that group-by properties are provIded by the subselect stream only (expression 's0.getP00()' against stream 1 is not)");

            // aggregations not allowed in group-by
            epl =
                "select (select IntPrimitive, sum(LongPrimitive) from SupportBean#keepall group by sum(IntPrimitive)) from SupportBean_S0 as s0";
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                epl,
                "Failed to plan subquery number 1 querying SupportBean: Group-by expressions in a subselect may not have an aggregation function [select (select IntPrimitive, sum(LongPrimitive) from SupportBean#keepall group by sum(IntPrimitive)) from SupportBean_S0 as s0]");

            // "prev" not allowed in group-by
            epl =
                "select (select IntPrimitive, sum(LongPrimitive) from SupportBean#keepall group by prev(1, IntPrimitive)) from SupportBean_S0 as s0";
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                epl,
                "Failed to plan subquery number 1 querying SupportBean: Group-by expressions in a subselect may not have a function that requires view resources (prior, prev) [select (select IntPrimitive, sum(LongPrimitive) from SupportBean#keepall group by prev(1, IntPrimitive)) from SupportBean_S0 as s0]");
        }
    }

    internal class EPLSubselectMulticolumnGroupedWHaving : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var fields = "c0,c1".SplitCsv();
            var epl =
                "@Name('s0') @name('s0')select (select TheString as c0, sum(IntPrimitive) as c1 from SupportBean#keepall group by TheString having sum(IntPrimitive) > 10) as subq from SupportBean_S0";
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
            var fields = "c0,c1".SplitCsv();

            var epl =
                "create context MyCtx partition by TheString from SupportBean, P00 from SupportBean_S0;\n" +
                "@Name('s0') context MyCtx select " +
                "(select TheString as c0, sum(IntPrimitive) as c1 " +
                " from SupportBean#keepall " +
                " group by TheString) as subq " +
                "from SupportBean_S0 as s0";
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
            var fields = "c0,c1".SplitCsv();
            var path = new RegressionPath();

            var eplNoDelete = "@Name('s0') select " +
                              "(select TheString as c0, sum(IntPrimitive) as c1 " +
                              "from SupportBean#keepall " +
                              "group by TheString) as subq " +
                              "from SupportBean_S0 as s0";
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
                      " from MyWindow group by TheString) as subq from SupportBean_S0 as s0";
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
            var fieldsMultiGroup = "c0,c1,c2,c3,c4".SplitCsv();
            var eplMultiGroup = "@Name('s0') select " +
                                "(select TheString as c0, IntPrimitive as c1, TheString||'x' as c2, " +
                                "    IntPrimitive * 1000 as c3, sum(LongPrimitive) as c4 " +
                                " from SupportBean#keepall " +
                                " group by TheString, IntPrimitive) as subq " +
                                "from SupportBean_S0 as s0";
            env.CompileDeploy(eplMultiGroup, path).AddListener("s0");

            SendSBEventAndTrigger(env, "G1", 1, 100L);
            AssertMapFieldAndReset(
                env,
                fieldName,
                fieldsMultiGroup,
                new object[] {"G1", 1, "G1x", 1000, 100L});

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
            var fields = "c0,c1".SplitCsv();

            var eplEnumCorrelated = "@Name('s0') select " +
                                    "(select TheString as c0, sum(IntPrimitive) as c1 " +
                                    " from SupportBean#keepall " +
                                    " where IntPrimitive = s0.Id " +
                                    " group by TheString" +
                                    " having sum(IntPrimitive) > 10).take(100) as subq " +
                                    "from SupportBean_S0 as s0";
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
            var fields = "c0,c1".SplitCsv();

            var eplEnumCorrelated = "@Name('s0') select " +
                                    "(select TheString as c0, sum(IntPrimitive) as c1 " +
                                    " from SupportBean#keepall " +
                                    " where IntPrimitive = s0.Id " +
                                    " group by TheString).take(100) as subq " +
                                    "from SupportBean_S0 as s0";
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
                "c0,c1".SplitCsv(),
                new[] {new object[] {"E1", 30}});

            env.SendEventBean(new SupportBean("E2", 200));
            env.SendEventBean(new SupportBean_S0(2));
            AssertMapMultiRow(
                "e1",
                env.Listener("s0").AssertOneGetNewAndReset(),
                "c0",
                "c0,c1".SplitCsv(),
                new[] {new object[] {"E1", 30}, new object[] {"E2", 200}});
            env.UndeployModuleContaining("s0");

            // test correlated
            var eplTwo = "@Name('s0') select " +
                         "(select TheString as c0, sum(IntPrimitive) as c1 from SBWindow where TheString = s0.P00 group by TheString).take(10) as e1 from SupportBean_S0 as s0";
            env.CompileDeploy(eplTwo, path).AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1, "E1"));
            AssertMapMultiRow(
                "e1",
                env.Listener("s0").AssertOneGetNewAndReset(),
                "c0",
                "c0,c1".SplitCsv(),
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

            var fields = "c0,c1".SplitCsv();

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
            var fields = "c0,c1".SplitCsv();
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
            var fields = "c0,c1".SplitCsv();

            // test unfiltered
            var eplEnumUnfiltered = "@Name('s0') select " +
                                    "(select TheString as c0, sum(IntPrimitive) as c1 " +
                                    " from SupportBean#keepall " +
                                    " group by TheString).take(100) as subq " +
                                    "from SupportBean_S0 as s0";
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
                                  "from SupportBean_S0 as s0";
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
                "c0,c1".SplitCsv(),
                null);

            env.Milestone(1);

            env.SendEventBean(new SupportBean("E1", 10));
            env.SendEventBean(new SupportBean_S0(2));
            AssertMapMultiRow(
                "e1",
                env.Listener("s0").AssertOneGetNewAndReset(),
                "c0",
                "c0,c1".SplitCsv(),
                new[] {new object[] {"E1", 10}});

            env.Milestone(2);

            env.SendEventBean(new SupportBean("E2", 200));
            env.SendEventBean(new SupportBean_S0(3));
            AssertMapMultiRow(
                "e1",
                env.Listener("s0").AssertOneGetNewAndReset(),
                "c0",
                "c0,c1".SplitCsv(),
                new[] {new object[] {"E1", 10}, new object[] {"E2", 200}});

            env.Milestone(3);

            env.SendEventBean(new SupportBean("E1", 20));
            env.SendEventBean(new SupportBean_S0(4));
            AssertMapMultiRow(
                "e1",
                env.Listener("s0").AssertOneGetNewAndReset(),
                "c0",
                "c0,c1".SplitCsv(),
                new[] {new object[] {"E1", 30}, new object[] {"E2", 200}});

            env.UndeployAll();
        }
    }
} // end of namespace