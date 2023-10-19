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
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectMulticolumn
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithMulticolumnAgg(execs);
            WithInvalid(execs);
            WithColumnsUncorrelated(execs);
            WithCorrelatedAggregation(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCorrelatedAggregation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectCorrelatedAggregation());
            return execs;
        }

        public static IList<RegressionExecution> WithColumnsUncorrelated(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectColumnsUncorrelated());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithMulticolumnAgg(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectMulticolumnAgg());
            return execs;
        }

        public class EPLSubselectMulticolumnAgg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "id", "s1totals.v1", "s1totals.v2" };
                var text =
                    "@name('s0') select id, (select count(*) as v1, sum(id) as v2 from SupportBean_S1#length(3)) as s1totals " +
                    "from SupportBean_S0 s0";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "G1"));
                env.AssertPropsNew("s0", fields, new object[] { 1, 0L, null });

                env.SendEventBean(new SupportBean_S1(200, "G2"));
                env.SendEventBean(new SupportBean_S0(2, "G2"));
                env.AssertPropsNew("s0", fields, new object[] { 2, 1L, 200 });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(210, "G2"));
                env.SendEventBean(new SupportBean_S0(3, "G2"));
                env.AssertPropsNew("s0", fields, new object[] { 3, 2L, 410 });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S1(220, "G2"));
                env.SendEventBean(new SupportBean_S0(4, "G2"));
                env.AssertPropsNew("s0", fields, new object[] { 4, 3L, 630 });

                env.UndeployAll();
            }
        }

        private class EPLSubselectInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "select (select theString, sum(intPrimitive) from SupportBean#lastevent as sb) from SupportBean_S0";
                env.TryInvalidCompile(
                    epl,
                    "Failed to plan subquery number 1 querying SupportBean: Subquery with multi-column select requires that either all or none of the selected columns are under aggregation, unless a group-by clause is also specified [select (select theString, sum(intPrimitive) from SupportBean#lastevent as sb) from SupportBean_S0]");

                epl = "select (select theString, theString from SupportBean#lastevent as sb) from SupportBean_S0";
                env.TryInvalidCompile(
                    epl,
                    "Column 1 in subquery does not have a unique column name assigned [select (select theString, theString from SupportBean#lastevent as sb) from SupportBean_S0]");

                epl =
                    "select * from SupportBean_S0(p00 = (select theString, theString from SupportBean#lastevent as sb))";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate subquery number 1 querying SupportBean: Subquery multi-column select is not allowed in this context. [select * from SupportBean_S0(p00 = (select theString, theString from SupportBean#lastevent as sb))]");

                epl =
                    "select exists(select sb.* as v1, intPrimitive*2 as v3 from SupportBean#lastevent as sb) as subrow from SupportBean_S0 as s0";
                env.TryInvalidCompile(
                    epl,
                    "Failed to plan subquery number 1 querying SupportBean: Subquery multi-column select does not allow wildcard or stream wildcard when selecting multiple columns. [select exists(select sb.* as v1, intPrimitive*2 as v3 from SupportBean#lastevent as sb) as subrow from SupportBean_S0 as s0]");

                epl =
                    "select (select sb.* as v1, intPrimitive*2 as v3 from SupportBean#lastevent as sb) as subrow from SupportBean_S0 as s0";
                env.TryInvalidCompile(
                    epl,
                    "Failed to plan subquery number 1 querying SupportBean: Subquery multi-column select does not allow wildcard or stream wildcard when selecting multiple columns. [select (select sb.* as v1, intPrimitive*2 as v3 from SupportBean#lastevent as sb) as subrow from SupportBean_S0 as s0]");

                epl =
                    "select (select *, intPrimitive from SupportBean#lastevent as sb) as subrow from SupportBean_S0 as s0";
                env.TryInvalidCompile(
                    epl,
                    "Failed to plan subquery number 1 querying SupportBean: Subquery multi-column select does not allow wildcard or stream wildcard when selecting multiple columns. [select (select *, intPrimitive from SupportBean#lastevent as sb) as subrow from SupportBean_S0 as s0]");

                epl =
                    "select * from SupportBean_S0(p00 in (select theString, theString from SupportBean#lastevent as sb))";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate subquery number 1 querying SupportBean: Subquery multi-column select is not allowed in this context. [select * from SupportBean_S0(p00 in (select theString, theString from SupportBean#lastevent as sb))]");
            }
        }

        private class EPLSubselectColumnsUncorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var stmtText = "@name('s0') select " +
                               "(select theString as v1, intPrimitive as v2 from SupportBean#lastevent) as subrow " +
                               "from SupportBean_S0 as s0";
                env.CompileDeployAddListenerMile(stmtText, "s0", milestone.GetAndIncrement());

                TryAssertion(env);

                env.UndeployAll();

                env.EplToModelCompileDeploy(stmtText).AddListener("s0").Milestone(milestone.GetAndIncrement());

                TryAssertion(env);

                env.UndeployAll();
            }
        }

        private class EPLSubselectCorrelatedAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select p00, " +
                               "(select " +
                               "  sum(intPrimitive) as v1, " +
                               "  sum(intPrimitive + 1) as v2, " +
                               "  window(intPrimitive) as v3, " +
                               "  window(sb.*) as v4 " +
                               "  from SupportBean#keepall sb " +
                               "  where theString = s0.p00) as subrow " +
                               "from SupportBean_S0 as s0";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var rows = new object[][] {
                            new object[] { "p00", typeof(string), false },
                            new object[] { "subrow", typeof(IDictionary<string, object>), true }
                        };
                        for (var i = 0; i < rows.Length; i++) {
                            var message = "Failed assertion for " + rows[i][0];
                            var prop = statement.EventType.PropertyDescriptors[i];
                            Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                            Assert.AreEqual(rows[i][1], prop.PropertyType, message);
                            Assert.AreEqual(rows[i][2], prop.IsFragment, message);
                        }

                        var fragmentType = statement.EventType.GetFragmentType("subrow");
                        Assert.IsFalse(fragmentType.IsIndexed);
                        Assert.IsFalse(fragmentType.IsNative);
                        rows = new object[][] {
                            new object[] { "v1", typeof(int?) },
                            new object[] { "v2", typeof(int?) },
                            new object[] { "v3", typeof(int?[]) },
                            new object[] { "v4", typeof(SupportBean[]) },
                        };
                        for (var i = 0; i < rows.Length; i++) {
                            var message = "Failed assertion for " + rows[i][0];
                            var prop = fragmentType.FragmentType.PropertyDescriptors[i];
                            Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                            Assert.AreEqual(rows[i][1], prop.PropertyType, message);
                        }
                    });

                var fields = "p00,subrow.v1,subrow.v2".SplitCsv();

                env.SendEventBean(new SupportBean_S0(1, "T1"));
                env.AssertEventNew(
                    "s0",
                    row => {
                        EPAssertionUtil.AssertProps(row, fields, new object[] { "T1", null, null });
                        Assert.IsNull(row.Get("subrow.v3"));
                        Assert.IsNull(row.Get("subrow.v4"));
                    });

                var sb1 = new SupportBean("T1", 10);
                env.SendEventBean(sb1);
                env.SendEventBean(new SupportBean_S0(2, "T1"));
                env.AssertEventNew(
                    "s0",
                    row => {
                        EPAssertionUtil.AssertProps(row, fields, new object[] { "T1", 10, 11 });
                        EPAssertionUtil.AssertEqualsAnyOrder((int?[])row.Get("subrow.v3"), new int?[] { 10 });
                        EPAssertionUtil.AssertEqualsAnyOrder((object[])row.Get("subrow.v4"), new object[] { sb1 });
                    });

                var sb2 = new SupportBean("T1", 20);
                env.SendEventBean(sb2);
                env.SendEventBean(new SupportBean_S0(3, "T1"));
                env.AssertEventNew(
                    "s0",
                    row => {
                        EPAssertionUtil.AssertProps(row, fields, new object[] { "T1", 30, 32 });
                        EPAssertionUtil.AssertEqualsAnyOrder((int?[])row.Get("subrow.v3"), new int?[] { 10, 20 });
                        EPAssertionUtil.AssertEqualsAnyOrder((object[])row.Get("subrow.v4"), new object[] { sb1, sb2 });
                    });

                env.UndeployAll();
            }
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            env.AssertStatement(
                "s0",
                statement => {
                    var fragmentType = statement.EventType.GetFragmentType("subrow");
                    Assert.IsFalse(fragmentType.IsIndexed);
                    Assert.IsFalse(fragmentType.IsNative);
                    var rows = new object[][] {
                        new object[] { "v1", typeof(string) },
                        new object[] { "v2", typeof(int?) },
                    };
                    for (var i = 0; i < rows.Length; i++) {
                        var message = "Failed assertion for " + rows[i][0];
                        var prop = fragmentType.FragmentType.PropertyDescriptors[i];
                        Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                        Assert.AreEqual(rows[i][1], prop.PropertyType, message);
                    }
                });

            var fields = "subrow.v1,subrow.v2".SplitCsv();

            env.SendEventBean(new SupportBean_S0(1));
            env.AssertPropsNew("s0", fields, new object[] { null, null });

            env.SendEventBean(new SupportBean("E1", 10));
            env.SendEventBean(new SupportBean_S0(2));
            env.AssertPropsNew("s0", fields, new object[] { "E1", 10 });

            env.SendEventBean(new SupportBean("E2", 20));
            env.SendEventBean(new SupportBean_S0(3));
            env.AssertPropsNew("s0", fields, new object[] { "E2", 20 });
        }
    }
} // end of namespace