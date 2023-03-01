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
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseJoin
    {
        private const string ALL_FIELDS =
            "mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal";

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSimpleJoinLeft(execs);
            With2HistoricalStar(execs);
            With2HistoricalStarInner(execs);
            With3Stream(execs);
            WithTimeBatch(execs);
            WithTimeBatchOM(execs);
            WithTimeBatchCompile(execs);
            WithVariables(execs);
            WithInvalidSQL(execs);
            WithInvalidBothHistorical(execs);
            WithInvalidPropertyEvent(execs);
            WithInvalidPropertyHistorical(execs);
            WithInvalid1Stream(execs);
            WithInvalidSubviews(execs);
            WithStreamNamesAndRename(execs);
            WithWithPattern(execs);
            WithPropertyResolution(execs);
            WithRestartStatement(execs);
            WithSimpleJoinRight(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSimpleJoinRight(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseSimpleJoinRight());
            return execs;
        }

        public static IList<RegressionExecution> WithRestartStatement(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseRestartStatement());
            return execs;
        }

        public static IList<RegressionExecution> WithPropertyResolution(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabasePropertyResolution());
            return execs;
        }

        public static IList<RegressionExecution> WithWithPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseWithPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithStreamNamesAndRename(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseStreamNamesAndRename());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidSubviews(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseInvalidSubviews());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid1Stream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseInvalid1Stream());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidPropertyHistorical(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseInvalidPropertyHistorical());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidPropertyEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseInvalidPropertyEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidBothHistorical(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseInvalidBothHistorical());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidSQL(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseInvalidSQL());
            return execs;
        }

        public static IList<RegressionExecution> WithVariables(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseVariables());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatchCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseTimeBatchCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatchOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseTimeBatchOM());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseTimeBatch());
            return execs;
        }

        public static IList<RegressionExecution> With3Stream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabase3Stream());
            return execs;
        }

        public static IList<RegressionExecution> With2HistoricalStarInner(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabase2HistoricalStarInner());
            return execs;
        }

        public static IList<RegressionExecution> With2HistoricalStar(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabase2HistoricalStar());
            return execs;
        }

        public static IList<RegressionExecution> WithSimpleJoinLeft(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDatabaseSimpleJoinLeft());
            return execs;
        }

        private static void RuntestTimeBatch(RegressionEnvironment env)
        {
            string[] fields = {"myint"};

            env.AdvanceTime(0);
            EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

            SendSupportBeanEvent(env, 10);
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {100}});

            SendSupportBeanEvent(env, 5);
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {100}, new object[] {50}});

            env.Milestone(0);

            SendSupportBeanEvent(env, 2);
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {100}, new object[] {50}, new object[] {20}});

            env.AdvanceTime(10000);
            var received = env.Listener("s0").LastNewData;
            Assert.AreEqual(3, received.Length);
            Assert.AreEqual(100, received[0].Get("myint"));
            Assert.AreEqual(50, received[1].Get("myint"));
            Assert.AreEqual(20, received[2].Get("myint"));

            EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

            SendSupportBeanEvent(env, 9);
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {90}});

            SendSupportBeanEvent(env, 8);
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {90}, new object[] {80}});

            env.UndeployAll();
        }

        private static void AssertReceived(
            RegressionEnvironment env,
            long mybigint,
            int myint,
            string myvarchar,
            string mychar,
            bool? mybool,
            decimal? mynumeric,
            decimal? mydecimal,
            double? mydouble,
            float? myreal)
        {
            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            AssertReceived(
                theEvent,
                mybigint,
                myint,
                myvarchar,
                mychar,
                mybool,
                mynumeric,
                mydecimal,
                mydouble,
                myreal);
        }

        private static void AssertReceived(
            EventBean theEvent,
            long? mybigint,
            int? myint,
            string myvarchar,
            string mychar,
            bool? mybool,
            decimal? mynumeric,
            decimal? mydecimal,
            double? mydouble,
            float? myreal)
        {
            Assert.AreEqual(mybigint, theEvent.Get("mybigint"));
            Assert.AreEqual(myint, theEvent.Get("myint"));
            Assert.AreEqual(myvarchar, theEvent.Get("myvarchar"));
            Assert.AreEqual(mychar, theEvent.Get("mychar"));
            Assert.AreEqual(mybool, theEvent.Get("mybool"));
            Assert.AreEqual(mynumeric, theEvent.Get("mynumeric"));
            Assert.AreEqual(mydecimal, theEvent.Get("mydecimal"));
            Assert.AreEqual(mydouble, theEvent.Get("mydouble"));
            Assert.AreEqual(myreal, theEvent.Get("myreal"));
        }

        private static void SendEventS0(
            RegressionEnvironment env,
            int id)
        {
            var bean = new SupportBean_S0(id);
            env.SendEventBean(bean);
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        internal class EPLDatabase3Stream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select * from SupportBean#lastevent sb, SupportBeanTwo#lastevent sbt, " +
                               "sql:MyDBWithRetain ['select myint from mytesttable'] as S1 " +
                               "  where sb.TheString = sbt.StringTwo and S1.myint = sbt.IntPrimitiveTwo";
                env.CompileDeploy(stmtText).AddListener("s0");
                AssertStatelessStmt(env, "s0", false);

                env.SendEventBean(new SupportBeanTwo("T1", 2));
                env.SendEventBean(new SupportBean("T1", -1));

                env.SendEventBean(new SupportBeanTwo("T2", 30));
                env.SendEventBean(new SupportBean("T2", -1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"sb.TheString", "sbt.StringTwo", "S1.myint"},
                    new object[] {"T2", "T2", 30});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("T3", -1));
                env.SendEventBean(new SupportBeanTwo("T3", 40));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"sb.TheString", "sbt.StringTwo", "S1.myint"},
                    new object[] {"T3", "T3", 40});

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseTimeBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select " +
                               ALL_FIELDS +
                               " from " +
                               " sql:MyDBWithRetain ['select " +
                               ALL_FIELDS +
                               " from mytesttable \n\r where ${IntPrimitive} = mytesttable.mybigint'] as S0," +
                               "SupportBean#time_batch(10 sec) as S1";
                env.CompileDeploy(stmtText).AddListener("s0");
                RuntestTimeBatch(env);
            }
        }

        internal class EPLDatabase2HistoricalStar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"IntPrimitive", "myint", "myvarchar"};
                var stmtText = "@Name('s0') select IntPrimitive, myint, myvarchar from " +
                               "SupportBean#keepall as S0, " +
                               " sql:MyDBWithRetain ['select myint from mytesttable where ${IntPrimitive} = mytesttable.mybigint'] as S1," +
                               " sql:MyDBWithRetain ['select myvarchar from mytesttable where ${IntPrimitive} = mytesttable.mybigint'] as S2 ";
                env.CompileDeploy(stmtText).AddListener("s0");

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                SendSupportBeanEvent(env, 6);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {6, 60, "F"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {6, 60, "F"}});

                SendSupportBeanEvent(env, 9);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {9, 90, "I"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {6, 60, "F"}, new object[] {9, 90, "I"}});

                env.Milestone(0);

                SendSupportBeanEvent(env, 20);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {6, 60, "F"}, new object[] {9, 90, "I"}});

                env.UndeployAll();
            }
        }

        internal class EPLDatabase2HistoricalStarInner : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"a", "b", "c", "d"};
                var stmtText =
                    "@Name('s0') select TheString as a, IntPrimitive as b, S1.myvarchar as c, S2.myvarchar as d from " +
                    "SupportBean#keepall as S0 " +
                    " inner join " +
                    " sql:MyDBWithRetain ['select myvarchar from mytesttable where ${IntPrimitive} <> mytesttable.mybigint'] as S1 " +
                    " on S1.myvarchar=S0.TheString " +
                    " inner join " +
                    " sql:MyDBWithRetain ['select myvarchar from mytesttable where ${IntPrimitive} <> mytesttable.myint'] as S2 " +
                    " on S2.myvarchar=S0.TheString ";
                env.CompileDeploy(stmtText).AddListener("s0");

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("A", 1));
                env.SendEventBean(new SupportBean("A", 10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("B", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 3, "B", "B"});

                env.SendEventBean(new SupportBean("D", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseVariables : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create variable int queryvar", path);
                env.CompileDeploy("on SupportBean set queryvar=IntPrimitive", path);
                var stmtText = "@Name('s0') select myint from " +
                               " sql:MyDBWithRetain ['select myint from mytesttable where ${queryvar} = mytesttable.mybigint'] as S0, " +
                               "SupportBean_A#keepall as S1";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                SendSupportBeanEvent(env, 5);
                env.SendEventBean(new SupportBean_A("A1"));

                var received = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(50, received.Get("myint"));
                env.UndeployModuleContaining("s0");

                stmtText = "@Name('s0') select myint from " +
                           "SupportBean_A#keepall as S1, " +
                           "sql:MyDBWithRetain ['select myint from mytesttable where ${queryvar} = mytesttable.mybigint'] as S0";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                SendSupportBeanEvent(env, 6);
                env.SendEventBean(new SupportBean_A("A1"));

                received = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(60, received.Get("myint"));

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseTimeBatchOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = ALL_FIELDS.SplitCsv();
                var sql = "select " + ALL_FIELDS + " from mytesttable where ${IntPrimitive} = mytesttable.mybigint";

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create(fields);
                var fromClause = FromClause.Create(
                    SQLStream.Create("MyDBWithRetain", sql, "s0"),
                    FilterStream.Create(nameof(SupportBean), "s1")
                        .AddView(
                            View.Create("time_batch", Expressions.Constant(10))
                        ));
                model.FromClause = fromClause;
                env.CopyMayFail(model);
                Assert.AreEqual(
                    "select mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal from sql:MyDBWithRetain[\"select mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal from mytesttable where ${IntPrimitive} = mytesttable.mybigint\"] as s0, " +
                    "SupportBean#time_batch(10) as s1",
                    model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");
                RuntestTimeBatch(env);
            }
        }

        internal class EPLDatabaseTimeBatchCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select " +
                               ALL_FIELDS +
                               " from " +
                               " sql:MyDBWithRetain ['select " +
                               ALL_FIELDS +
                               " from mytesttable where ${IntPrimitive} = mytesttable.mybigint'] as S0," +
                               "SupportBean#time_batch(10 sec) as S1";

                var model = env.EplToModel(stmtText);
                env.CopyMayFail(model);
                env.CompileDeploy(model).AddListener("s0");
                RuntestTimeBatch(env);
            }
        }

        internal class EPLDatabaseInvalidSQL : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select myvarchar from " +
                               " sql:MyDBWithRetain ['select mychar,, from mytesttable where '] as S0," +
                               "SupportBeanComplexProps as S1";
                TryInvalidCompile(
                    env,
                    stmtText,
                    "Error in statement 'select mychar,, from mytesttable where ', failed to obtain result metadata, consider turning off metadata interrogation via configuration, please check the statement, " +
                    "reason: 42601: syntax error at or near \",\"");
            }
        }

        internal class EPLDatabaseInvalidBothHistorical : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var sqlOne =
                    "sql:MyDBWithRetain ['select myvarchar from mytesttable where ${mychar} = mytesttable.mybigint']";
                var sqlTwo =
                    "sql:MyDBWithRetain ['select mychar from mytesttable where ${myvarchar} = mytesttable.mybigint']";
                var stmtText = "@Name('s0') select S0.myvarchar as S0Name, S1.mychar as S1Name from " +
                               sqlOne +
                               " as S0, " +
                               sqlTwo +
                               "  as S1";

                TryInvalidCompile(env, stmtText, "Circular dependency detected between historical streams");
            }
        }

        internal class EPLDatabaseInvalidPropertyEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select myvarchar from " +
                               " sql:MyDBWithRetain ['select mychar from mytesttable where ${S1.xxx[0]} = mytesttable.mybigint'] as S0," +
                               "SupportBeanComplexProps as S1";
                TryInvalidCompile(
                    env,
                    stmtText,
                    "Failed to validate from-clause database-access parameter expression 'S1.xxx[0]': Failed to resolve property 'S1.xxx[0]' to a stream or nested property in a stream");

                stmtText = "@Name('s0') select myvarchar from " +
                           " sql:MyDBWithRetain ['select mychar from mytesttable where ${} = mytesttable.mybigint'] as S0," +
                           "SupportBeanComplexProps as S1";
                TryInvalidCompile(
                    env,
                    stmtText,
                    "Missing expression within ${...} in SQL statement [");
            }
        }

        internal class EPLDatabaseInvalidPropertyHistorical : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select myvarchar from " +
                               " sql:MyDBWithRetain ['select myvarchar from mytesttable where ${myvarchar} = mytesttable.mybigint'] as S0," +
                               "SupportBeanComplexProps as S1";
                TryInvalidCompile(
                    env,
                    stmtText,
                    "Invalid expression 'myvarchar' resolves to the historical data itself");
            }
        }

        internal class EPLDatabaseInvalid1Stream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var sql =
                    "sql:MyDBWithRetain ['select myvarchar, mybigint from mytesttable where ${mybigint} = myint']";
                var stmtText = "@Name('s0') select myvarchar as S0Name from " + sql + " as S0";
                TryInvalidCompile(
                    env,
                    stmtText,
                    "Invalid expression 'mybigint' resolves to the historical data itself");
            }
        }

        internal class EPLDatabaseInvalidSubviews : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var sql =
                    "sql:MyDBWithRetain ['select myvarchar from mytesttable where ${IntPrimitive} = mytesttable.myint']#time(30 sec)";
                var stmtText = "@Name('s0') select myvarchar as S0Name from " +
                               sql +
                               " as S0, " +
                               "SupportBean as S1";
                TryInvalidCompile(
                    env,
                    stmtText,
                    "Historical data joins do not allow views onto the data, view 'time' is not valid in this context");
            }
        }

        internal class EPLDatabaseStreamNamesAndRename : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select S1.a as mybigint, " +
                               " S1.b as myint," +
                               " S1.c as myvarchar," +
                               " S1.d as mychar," +
                               " S1.e as mybool," +
                               " S1.f as mynumeric," +
                               " S1.g as mydecimal," +
                               " S1.h as mydouble," +
                               " S1.i as myreal " +
                               " from SupportBean_S0 as S0," +
                               " sql:MyDBWithRetain ['select mybigint as a, " +
                               " myint as b," +
                               " myvarchar as c," +
                               " mychar as d," +
                               " mybool as e," +
                               " mynumeric as f," +
                               " mydecimal as g," +
                               " mydouble as h," +
                               " myreal as i " +
                               "from mytesttable where ${Id} = mytesttable.mybigint'] as S1";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEventS0(env, 1);
                AssertReceived(env, 1, 10, "A", "Z", true, 5000m, 100m, 1.2, 1.3f);

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseWithPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var stmtText = "@Name('s0') select mychar from " +
                               " sql:MyDBWithRetain ['select mychar from mytesttable where mytesttable.mybigint = 2'] as S0," +
                               " pattern [every timer:interval(5 sec) ]";
                env.CompileDeploy(stmtText).AddListener("s0");

                env.AdvanceTime(5000);
                Assert.AreEqual("Y", env.Listener("s0").AssertOneGetNewAndReset().Get("mychar"));

                env.AdvanceTime(9999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(10000);
                Assert.AreEqual("Y", env.Listener("s0").AssertOneGetNewAndReset().Get("mychar"));

                // with variable
                var path = new RegressionPath();
                env.CompileDeploy("create variable long VarLastTimestamp = 0", path);
                var epl = "@Name('Poll every 5 seconds') insert into PollStream" +
                          " select * from pattern[every timer:interval(5 sec)]," +
                          " sql:MyDBWithRetain ['select mychar from mytesttable where mytesttable.mybigint > ${VarLastTimestamp}'] as S0";
                var model = env.EplToModel(epl);
                env.CompileDeploy(model, path);
                env.UndeployAll();
            }
        }

        internal class EPLDatabasePropertyResolution : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select " +
                               ALL_FIELDS +
                               " from " +
                               " sql:MyDBWithRetain ['select " +
                               ALL_FIELDS +
                               " from mytesttable where ${S1.ArrayProperty[0]} = mytesttable.mybigint'] as S0," +
                               "SupportBeanComplexProps as S1";
                // S1.ArrayProperty[0] returns 10 for that bean
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
                AssertReceived(env, 10, 100, "J", "P", true, null, 1000m, 10.2, 10.3f);

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseSimpleJoinLeft : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select " +
                               ALL_FIELDS +
                               " from " +
                               "SupportBean_S0 as S0," +
                               " sql:MyDBWithRetain ['select " +
                               ALL_FIELDS +
                               " from mytesttable where ${Id} = mytesttable.mybigint'] as S1";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEventS0(env, 1);
                AssertReceived(env, 1, 10, "A", "Z", true, 5000m, 100m, 1.2, 1.3f);

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseRestartStatement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select mychar from " +
                               "SupportBean_S0 as S0," +
                               " sql:MyDBWithRetain ['select mychar from mytesttable where ${Id} = mytesttable.mybigint'] as S1";
                var compiled = env.Compile(stmtText);
                env.Deploy(compiled);

                // Too many connections unless the stop actually relieves them
                for (var i = 0; i < 100; i++) {
                    env.UndeployModuleContaining("s0");

                    SendEventS0(env, 1);

                    env.Deploy(compiled).AddListener("s0");
                    SendEventS0(env, 1);
                    Assert.AreEqual("Z", env.Listener("s0").AssertOneGetNewAndReset().Get("mychar"));
                }

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseSimpleJoinRight : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select " +
                               ALL_FIELDS +
                               " from " +
                               " sql:MyDBWithRetain ['select " +
                               ALL_FIELDS +
                               " from mytesttable where ${Id} = mytesttable.mybigint'] as S0," +
                               "SupportBean_S0 as S1";
                env.CompileDeploy(stmtText).AddListener("s0");

                var eventType = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(long?), eventType.GetPropertyType("mybigint"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("myint"));
                Assert.AreEqual(typeof(string), eventType.GetPropertyType("myvarchar"));
                Assert.AreEqual(typeof(string), eventType.GetPropertyType("mychar"));
                Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("mybool"));
                Assert.AreEqual(typeof(decimal?), eventType.GetPropertyType("mynumeric"));
                Assert.AreEqual(typeof(decimal?), eventType.GetPropertyType("mydecimal"));
                Assert.AreEqual(typeof(double?), eventType.GetPropertyType("mydouble"));
                Assert.AreEqual(typeof(float?), eventType.GetPropertyType("myreal"));

                SendEventS0(env, 1);
                AssertReceived(env, 1, 10, "A", "Z", true, 5000m, 100m, 1.2, 1.3f);

                env.UndeployAll();
            }
        }
    }
} // end of namespace