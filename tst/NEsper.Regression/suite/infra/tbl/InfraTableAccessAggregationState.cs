///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    /// NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableAccessAggregationState
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithAccessAggShare(execs);
            WithTableAccessGroupedMixed(execs);
            WithTableAccessGroupedThreeKey(execs);
            WithNestedMultivalueAccess(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNestedMultivalueAccess(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNestedMultivalueAccess(false, false));
            execs.Add(new InfraNestedMultivalueAccess(true, false));
            execs.Add(new InfraNestedMultivalueAccess(false, true));
            execs.Add(new InfraNestedMultivalueAccess(true, true));
            return execs;
        }

        public static IList<RegressionExecution> WithTableAccessGroupedThreeKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableAccessGroupedThreeKey());
            return execs;
        }

        public static IList<RegressionExecution> WithTableAccessGroupedMixed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableAccessGroupedMixed());
            return execs;
        }

        public static IList<RegressionExecution> WithAccessAggShare(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraAccessAggShare());
            return execs;
        }

        private class InfraNestedMultivalueAccess : RegressionExecution
        {
            private readonly bool grouped;
            private readonly bool soda;

            public InfraNestedMultivalueAccess(
                bool grouped,
                bool soda)
            {
                this.grouped = grouped;
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplDeclare = "@public create table varagg (" +
                                 (grouped ? "key string primary key, " : "") +
                                 "windowSupportBean window(*) @type('SupportBean'))";
                env.CompileDeploy(soda, eplDeclare, path);

                var eplInto = "into table varagg " +
                              "select window(*) as windowSupportBean from SupportBean#length(2)" +
                              (grouped ? " group by theString" : "");
                env.CompileDeploy(soda, eplInto, path);

                var key = grouped ? "[\"E1\"]" : "";
                var eplSelect = "@name('s0') select " +
                                "varagg" +
                                key +
                                ".windowSupportBean.last(*) as c0, " +
                                "varagg" +
                                key +
                                ".windowSupportBean.window(*) as c1, " +
                                "varagg" +
                                key +
                                ".windowSupportBean.first(*) as c2, " +
                                "varagg" +
                                key +
                                ".windowSupportBean.last(intPrimitive) as c3, " +
                                "varagg" +
                                key +
                                ".windowSupportBean.window(intPrimitive) as c4, " +
                                "varagg" +
                                key +
                                ".windowSupportBean.first(intPrimitive) as c5" +
                                " from SupportBean_S0";
                env.CompileDeploy(soda, eplSelect, path).AddListener("s0");

                var expectedAggType = new object[][] {
                    new object[] { "c0", typeof(SupportBean) }, new object[] { "c1", typeof(SupportBean[]) },
                    new object[] { "c2", typeof(SupportBean) },
                    new object[] { "c3", typeof(int?) }, new object[] { "c4", typeof(int?[]) },
                    new object[] { "c5", typeof(int?) }
                };
                env.AssertStatement(
                    "s0",
                    statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        expectedAggType,
                        statement.EventType,
                        SupportEventTypeAssertionEnum.NAME,
                        SupportEventTypeAssertionEnum.TYPE));

                var fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var b1 = MakeSendBean(env, "E1", 10);
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { b1, new object[] { b1 }, b1, 10, new int?[] { 10 }, 10 });

                var b2 = MakeSendBean(env, "E1", 20);
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { b2, new object[] { b1, b2 }, b1, 20, new int?[] { 10, 20 }, 10 });

                env.Milestone(0);

                var b3 = MakeSendBean(env, "E1", 30);
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { b3, new object[] { b2, b3 }, b2, 30, new int?[] { 20, 30 }, 20 });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "grouped=" +
                       grouped +
                       ", soda=" +
                       soda +
                       '}';
            }
        }

        private class InfraAccessAggShare : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create table varagg (mywin window(*) @type(SupportBean))", path);

                env.CompileDeploy(
                        "@name('into') into table varagg " +
                        "select window(sb.*) as mywin from SupportBean#time(10 sec) as sb",
                        path)
                    .AddListener("into");
                env.AssertStatement(
                    "into",
                    statement => Assert.AreEqual(typeof(SupportBean[]), statement.EventType.GetPropertyType("mywin")));

                env.CompileDeploy("@name('s0') select varagg.mywin as c0 from SupportBean_S0", path).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(SupportBean[]), statement.EventType.GetPropertyType("c0")));

                var b1 = MakeSendBean(env, "E1", 10);
                env.AssertPropsNew("into", "mywin".SplitCsv(), new object[] { new SupportBean[] { b1 } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { new object[] { b1 } });

                var b2 = MakeSendBean(env, "E2", 20);
                env.AssertPropsNew("into", "mywin".SplitCsv(), new object[] { new SupportBean[] { b1, b2 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { new object[] { b1, b2 } });

                env.UndeployAll();
            }
        }

        public class InfraTableAccessGroupedThreeKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplDeclare = "@public create table varTotal (key0 string primary key, key1 int primary key," +
                                 "key2 long primary key, total sum(double), cnt count(*))";
                env.CompileDeploy(eplDeclare, path);

                var eplBind = "into table varTotal " +
                              "select sum(doublePrimitive) as total, count(*) as cnt " +
                              "from SupportBean group by theString, intPrimitive, longPrimitive";
                env.CompileDeploy(eplBind, path);

                env.Milestone(0);

                var fields = "c0,c1".SplitCsv();
                var eplUse =
                    "@name('s0') select varTotal[p00, id, 100L].total as c0, varTotal[p00, id, 100L].cnt as c1 from SupportBean_S0";
                env.CompileDeploy(eplUse, path).AddListener("s0");

                MakeSendBean(env, "E1", 10, 100, 1000);

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(10, "E1"));
                env.AssertPropsNew("s0", fields, new object[] { 1000.0, 1L });

                env.Milestone(2);

                MakeSendBean(env, "E1", 10, 100, 1001);

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(10, "E1"));
                env.AssertPropsNew("s0", fields, new object[] { 2001.0, 2L });

                env.UndeployAll();
            }
        }

        private class InfraTableAccessGroupedMixed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // create table
                var path = new RegressionPath();
                var eplDeclare = "@public create table varMyAgg (" +
                                 "key string primary key, " +
                                 "c0 count(*), " +
                                 "c1 count(distinct int), " +
                                 "c2 window(*) @type('SupportBean'), " +
                                 "c3 sum(long)" +
                                 ")";
                env.CompileDeploy(eplDeclare, path);

                env.Milestone(0);

                // create into-table aggregation
                var eplBind = "into table varMyAgg select " +
                              "count(*) as c0, " +
                              "count(distinct intPrimitive) as c1, " +
                              "window(*) as c2, " +
                              "sum(longPrimitive) as c3 " +
                              "from SupportBean#length(3) group by theString";
                env.CompileDeploy(eplBind, path);

                env.Milestone(1);

                // create query for state
                var eplSelect = "@name('s0') select " +
                                "varMyAgg[p00].c0 as c0, " +
                                "varMyAgg[p00].c1 as c1, " +
                                "varMyAgg[p00].c2 as c2, " +
                                "varMyAgg[p00].c3 as c3" +
                                " from SupportBean_S0";
                env.CompileDeploy(eplSelect, path).AddListener("s0");
                var fields = "c0,c1,c2,c3".SplitCsv();

                env.Milestone(2);

                var b1 = MakeSendBean(env, "E1", 10, 100);
                var b2 = MakeSendBean(env, "E1", 11, 101);

                env.Milestone(3);

                var b3 = MakeSendBean(env, "E1", 10, 102);

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(0, "E1"));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 3L, 2L, new SupportBean[] { b1, b2, b3 }, 303L });

                env.Milestone(5);

                env.SendEventBean(new SupportBean_S0(0, "E2"));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { null, null, null, null });

                env.Milestone(6);

                var b4 = MakeSendBean(env, "E2", 20, 200);
                env.SendEventBean(new SupportBean_S0(0, "E2"));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 1L, 1L, new SupportBean[] { b4 }, 200L });

                env.UndeployAll();
            }
        }

        private static SupportBean MakeSendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            return MakeSendBean(env, theString, intPrimitive, longPrimitive, -1);
        }

        private static SupportBean MakeSendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive,
            double doublePrimitive)
        {
            return MakeSendBean(env, theString, intPrimitive, longPrimitive, doublePrimitive, -1);
        }

        private static SupportBean MakeSendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive,
            double doublePrimitive,
            float floatPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            bean.DoublePrimitive = doublePrimitive;
            bean.FloatPrimitive = floatPrimitive;
            env.SendEventBean(bean);
            return bean;
        }

        private static SupportBean MakeSendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            env.SendEventBean(bean);
            return bean;
        }
    }
} // end of namespace