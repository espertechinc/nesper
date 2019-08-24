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
            "myBigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal";

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLDatabaseSimpleJoinLeft());
            execs.Add(new EPLDatabase2HistoricalStar());
            execs.Add(new EPLDatabase2HistoricalStarInner());
            execs.Add(new EPLDatabase3Stream());
            execs.Add(new EPLDatabaseTimeBatch());
            execs.Add(new EPLDatabaseTimeBatchOM());
            execs.Add(new EPLDatabaseTimeBatchCompile());
            execs.Add(new EPLDatabaseVariables());
            execs.Add(new EPLDatabaseInvalidSQL());
            execs.Add(new EPLDatabaseInvalidBothHistorical());
            execs.Add(new EPLDatabaseInvalidPropertyEvent());
            execs.Add(new EPLDatabaseInvalidPropertyHistorical());
            execs.Add(new EPLDatabaseInvalid1Stream());
            execs.Add(new EPLDatabaseInvalidSubviews());
            execs.Add(new EPLDatabaseStreamNamesAndRename());
            execs.Add(new EPLDatabaseWithPattern());
            execs.Add(new EPLDatabasePropertyResolution());
            execs.Add(new EPLDatabaseRestartStatement());
            execs.Add(new EPLDatabaseSimpleJoinRight());
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
            double? myreal)
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
            double? myreal)
        {
            Assert.AreEqual(mybigint, theEvent.Get("myBigint"));
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
                               "sql:MyDBWithRetain ['select myint from mytesttable'] as s1 " +
                               "  where sb.TheString = sbt.stringTwo and s1.myint = sbt.IntPrimitiveTwo";
                env.CompileDeploy(stmtText).AddListener("s0");
                AssertStatelessStmt(env, "s0", false);

                env.SendEventBean(new SupportBeanTwo("T1", 2));
                env.SendEventBean(new SupportBean("T1", -1));

                env.SendEventBean(new SupportBeanTwo("T2", 30));
                env.SendEventBean(new SupportBean("T2", -1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "sb.TheString,sbt.stringTwo,s1.myint".SplitCsv(),
                    new object[] {"T2", "T2", 30});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("T3", -1));
                env.SendEventBean(new SupportBeanTwo("T3", 40));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "sb.TheString,sbt.stringTwo,s1.myint".SplitCsv(),
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
                               " from mytesttable \n\r where ${IntPrimitive} = mytesttable.myBigint'] as s0," +
                               "SupportBean#time_batch(10 sec) as s1";
                env.CompileDeploy(stmtText).AddListener("s0");
                RuntestTimeBatch(env);
            }
        }

        internal class EPLDatabase2HistoricalStar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "IntPrimitive,myint,myvarchar".SplitCsv();
                var stmtText = "@Name('s0') select IntPrimitive, myint, myvarchar from " +
                               "SupportBean#keepall as s0, " +
                               " sql:MyDBWithRetain ['select myint from mytesttable where ${IntPrimitive} = mytesttable.myBigint'] as s1," +
                               " sql:MyDBWithRetain ['select myvarchar from mytesttable where ${IntPrimitive} = mytesttable.myBigint'] as s2 ";
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
                var fields = "a,b,c,d".SplitCsv();
                var stmtText =
                    "@Name('s0') select TheString as a, IntPrimitive as b, s1.myvarchar as c, s2.myvarchar as d from " +
                    "SupportBean#keepall as s0 " +
                    " inner join " +
                    " sql:MyDBWithRetain ['select myvarchar from mytesttable where ${IntPrimitive} <> mytesttable.myBigint'] as s1 " +
                    " on s1.myvarchar=s0.TheString " +
                    " inner join " +
                    " sql:MyDBWithRetain ['select myvarchar from mytesttable where ${IntPrimitive} <> mytesttable.myint'] as s2 " +
                    " on s2.myvarchar=s0.TheString ";
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
                               " sql:MyDBWithRetain ['select myint from mytesttable where ${queryvar} = mytesttable.myBigint'] as s0, " +
                               "SupportBean_A#keepall as s1";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                SendSupportBeanEvent(env, 5);
                env.SendEventBean(new SupportBean_A("A1"));

                var received = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(50, received.Get("myint"));
                env.UndeployModuleContaining("s0");

                stmtText = "@Name('s0') select myint from " +
                           "SupportBean_A#keepall as s1, " +
                           "sql:MyDBWithRetain ['select myint from mytesttable where ${queryvar} = mytesttable.myBigint'] as s0";
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
                var sql = "select " + ALL_FIELDS + " from mytesttable where ${IntPrimitive} = mytesttable.myBigint";

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create(fields);
                var fromClause = FromClause.Create(
                    SQLStream.Create("MyDBWithRetain", sql, "s0"),
                    FilterStream.Create(typeof(SupportBean).Name, "s1")
                        .AddView(
                            View.Create("time_batch", Expressions.Constant(10))
                        ));
                model.FromClause = fromClause;
                env.CopyMayFail(model);
                Assert.AreEqual(
                    "select myBigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal from sql:MyDBWithRetain[\"select myBigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal from mytesttable where ${IntPrimitive} = mytesttable.myBigint\"] as s0, " +
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
                               " from mytesttable where ${IntPrimitive} = mytesttable.myBigint'] as s0," +
                               "SupportBean#time_batch(10 sec) as s1";

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
                               " sql:MyDBWithRetain ['select mychar,, from mytesttable where '] as s0," +
                               "SupportBeanComplexProps as s1";
                TryInvalidCompile(
                    env,
                    stmtText,
                    "Error in statement 'select mychar,, from mytesttable where ', failed to obtain result metadata, consIder turning off metadata interrogation via configuration, please check the statement, reason: You have an error in your SQL syntax; check the manual that corresponds to your MySQL server version for the right syntax to use near ' from mytesttable where' at line 1");
            }
        }

        internal class EPLDatabaseInvalidBothHistorical : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var sqlOne =
                    "sql:MyDBWithRetain ['select myvarchar from mytesttable where ${mychar} = mytesttable.myBigint']";
                var sqlTwo =
                    "sql:MyDBWithRetain ['select mychar from mytesttable where ${myvarchar} = mytesttable.myBigint']";
                var stmtText = "@Name('s0') select s0.myvarchar as s0Name, s1.mychar as s1Name from " +
                               sqlOne +
                               " as s0, " +
                               sqlTwo +
                               "  as s1";

                TryInvalidCompile(env, stmtText, "Circular dependency detected between historical streams");
            }
        }

        internal class EPLDatabaseInvalidPropertyEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select myvarchar from " +
                               " sql:MyDBWithRetain ['select mychar from mytesttable where ${s1.xxx[0]} = mytesttable.myBigint'] as s0," +
                               "SupportBeanComplexProps as s1";
                TryInvalidCompile(
                    env,
                    stmtText,
                    "Failed to validate from-clause database-access parameter expression 's1.xxx[0]': Failed to resolve property 's1.xxx[0]' to a stream or nested property in a stream");

                stmtText = "@Name('s0') select myvarchar from " +
                           " sql:MyDBWithRetain ['select mychar from mytesttable where ${} = mytesttable.myBigint'] as s0," +
                           "SupportBeanComplexProps as s1";
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
                               " sql:MyDBWithRetain ['select myvarchar from mytesttable where ${myvarchar} = mytesttable.myBigint'] as s0," +
                               "SupportBeanComplexProps as s1";
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
                    "sql:MyDBWithRetain ['select myvarchar, myBigint from mytesttable where ${myBigint} = myint']";
                var stmtText = "@Name('s0') select myvarchar as s0Name from " + sql + " as s0";
                TryInvalidCompile(
                    env,
                    stmtText,
                    "Invalid expression 'myBigint' resolves to the historical data itself");
            }
        }

        internal class EPLDatabaseInvalidSubviews : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var sql =
                    "sql:MyDBWithRetain ['select myvarchar from mytesttable where ${IntPrimitive} = mytesttable.myint']#time(30 sec)";
                var stmtText = "@Name('s0') select myvarchar as s0Name from " +
                               sql +
                               " as s0, " +
                               "SupportBean as s1";
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
                var stmtText = "@Name('s0') select s1.a as myBigint, " +
                               " s1.b as myint," +
                               " s1.c as myvarchar," +
                               " s1.d as mychar," +
                               " s1.e as mybool," +
                               " s1.f as mynumeric," +
                               " s1.g as mydecimal," +
                               " s1.h as mydouble," +
                               " s1.i as myreal " +
                               " from SupportBean_S0 as s0," +
                               " sql:MyDBWithRetain ['select myBigint as a, " +
                               " myint as b," +
                               " myvarchar as c," +
                               " mychar as d," +
                               " mybool as e," +
                               " mynumeric as f," +
                               " mydecimal as g," +
                               " mydouble as h," +
                               " myreal as i " +
                               "from mytesttable where ${Id} = mytesttable.myBigint'] as s1";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEventS0(env, 1);
                AssertReceived(env, 1, 10, "A", "Z", true, 5000m, 100m, 1.2, 1.3);

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseWithPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var stmtText = "@Name('s0') select mychar from " +
                               " sql:MyDBWithRetain ['select mychar from mytesttable where mytesttable.myBigint = 2'] as s0," +
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
                          " sql:MyDBWithRetain ['select mychar from mytesttable where mytesttable.myBigint > ${VarLastTimestamp}'] as s0";
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
                               " from mytesttable where ${s1.ArrayProperty[0]} = mytesttable.myBigint'] as s0," +
                               "SupportBeanComplexProps as s1";
                // s1.ArrayProperty[0] returns 10 for that bean
                env.CompileDeploy(stmtText).AddListener("s0");

                env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
                AssertReceived(env, 10, 100, "J", "P", true, null, 1000m, 10.2, 10.3);

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
                               "SupportBean_S0 as s0," +
                               " sql:MyDBWithRetain ['select " +
                               ALL_FIELDS +
                               " from mytesttable where ${Id} = mytesttable.myBigint'] as s1";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEventS0(env, 1);
                AssertReceived(env, 1, 10, "A", "Z", true, 5000m, 100m, 1.2, 1.3);

                env.UndeployAll();
            }
        }

        internal class EPLDatabaseRestartStatement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select mychar from " +
                               "SupportBean_S0 as s0," +
                               " sql:MyDBWithRetain ['select mychar from mytesttable where ${Id} = mytesttable.myBigint'] as s1";
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
                               " from mytesttable where ${Id} = mytesttable.myBigint'] as s0," +
                               "SupportBean_S0 as s1";
                env.CompileDeploy(stmtText).AddListener("s0");

                var eventType = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(long?), eventType.GetPropertyType("myBigint"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("myint"));
                Assert.AreEqual(typeof(string), eventType.GetPropertyType("myvarchar"));
                Assert.AreEqual(typeof(string), eventType.GetPropertyType("mychar"));
                Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("mybool"));
                Assert.AreEqual(typeof(decimal), eventType.GetPropertyType("mynumeric"));
                Assert.AreEqual(typeof(decimal), eventType.GetPropertyType("mydecimal"));
                Assert.AreEqual(typeof(double?), eventType.GetPropertyType("mydouble"));
                Assert.AreEqual(typeof(double?), eventType.GetPropertyType("myreal"));

                SendEventS0(env, 1);
                AssertReceived(env, 1, 10, "A", "Z", true, 5000m, 100m, 1.2, 1.3);

                env.UndeployAll();
            }
        }
    }
} // end of namespace