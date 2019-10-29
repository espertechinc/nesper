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
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitRowPerGroup
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        private const string CATEGORY = "Fully-Aggregated and Grouped";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetLastNoDataWindow());
            execs.Add(new ResultSetOutputFirstWhenThen());
            execs.Add(new ResultSetWildcardRowPerGroup());
            execs.Add(new ResultSet1NoneNoHavingNoJoin());
            execs.Add(new ResultSet2NoneNoHavingJoin());
            execs.Add(new ResultSet3NoneHavingNoJoin());
            execs.Add(new ResultSet4NoneHavingJoin());
            execs.Add(new ResultSet5DefaultNoHavingNoJoin());
            execs.Add(new ResultSet6DefaultNoHavingJoin());
            execs.Add(new ResultSet7DefaultHavingNoJoin());
            execs.Add(new ResultSet8DefaultHavingJoin());
            execs.Add(new ResultSet9AllNoHavingNoJoin());
            execs.Add(new ResultSet10AllNoHavingJoin());
            execs.Add(new ResultSet11AllHavingNoJoin());
            execs.Add(new ResultSet12AllHavingJoin());
            execs.Add(new ResultSet13LastNoHavingNoJoin());
            execs.Add(new ResultSet14LastNoHavingJoin());
            execs.Add(new ResultSet15LastHavingNoJoin());
            execs.Add(new ResultSet16LastHavingJoin());
            execs.Add(new ResultSet17FirstNoHavingNoJoin());
            execs.Add(new ResultSet17FirstNoHavingJoin());
            execs.Add(new ResultSet18SnapshotNoHavingNoJoin());
            execs.Add(new ResultSet18SnapshotNoHavingJoin());
            execs.Add(new ResultSetJoinSortWindow());
            execs.Add(new ResultSetLimitSnapshot());
            execs.Add(new ResultSetLimitSnapshotLimit());
            execs.Add(new ResultSetGroupByAll());
            execs.Add(new ResultSetGroupByDefault());
            execs.Add(new ResultSetMaxTimeWindow());
            execs.Add(new ResultSetNoJoinLast());
            execs.Add(new ResultSetNoOutputClauseView());
            execs.Add(new ResultSetNoOutputClauseJoin());
            execs.Add(new ResultSetNoJoinAll());
            execs.Add(new ResultSetJoinLast());
            execs.Add(new ResultSetJoinAll());
            execs.Add(new ResultSetCrontabNumberSetVariations());
            execs.Add(new ResultSetOutputFirstHavingJoinNoJoin());
            execs.Add(new ResultSetOutputFirstCrontab());
            execs.Add(new ResultSetOutputFirstEveryNEvents());
            return execs;
        }

        private static void RunAssertion11AllHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select Symbol, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "group by Symbol " +
                           "having sum(Price) > 50 " +
                           "output all every 1 seconds";
            TryAssertion11_12(env, stmtText, "all");
        }

        private static void RunAssertion12AllHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select Symbol, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=Symbol " +
                           "group by Symbol " +
                           "having sum(Price) > 50 " +
                           "output all every 1 seconds";
            TryAssertion11_12(env, stmtText, "all");
        }

        private static void RunAssertion15LastHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select Symbol, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec)" +
                           "group by Symbol " +
                           "having sum(Price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last");
        }

        private static void RunAssertion16LastHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select Symbol, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=Symbol " +
                           "group by Symbol " +
                           "having sum(Price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last");
        }

        private static void TryAssertionJoinAll(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var epl = opt.GetHint() +
                      "@Name('s0') select irstream Symbol," +
                      "sum(Price) as mySum," +
                      "avg(Price) as myAvg " +
                      "from SupportBeanString#length(100) as one, " +
                      "SupportMarketDataBean#length(5) as two " +
                      "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                      "       and one.TheString = two.Symbol " +
                      "group by Symbol " +
                      "output all every 2 events";

            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
            env.SendEventBean(new SupportBeanString(SYMBOL_IBM));
            env.SendEventBean(new SupportBeanString("AAA"));

            TryAssertionAll(env);

            env.UndeployAll();
        }

        private static void TryAssertionLast(RegressionEnvironment env)
        {
            // assert select result type
            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("mySum"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("myAvg"));

            SendMDEvent(env, SYMBOL_DELL, 10);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendMDEvent(env, SYMBOL_DELL, 20);
            AssertEvent(
                env,
                SYMBOL_DELL,
                null,
                null,
                30d,
                15d);
            env.Listener("s0").Reset();

            SendMDEvent(env, SYMBOL_DELL, 100);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendMDEvent(env, SYMBOL_DELL, 50);
            AssertEvent(
                env,
                SYMBOL_DELL,
                30d,
                15d,
                170d,
                170 / 3d);
        }

        private static void TryOutputFirstHaving(
            RegressionEnvironment env,
            string statementText)
        {
            var fields = new [] { "TheString","value" };
            var epl = "create window MyWindow#keepall as SupportBean;\n" +
                      "insert into MyWindow select * from SupportBean;\n" +
                      "on SupportMarketDataBean md delete from MyWindow mw where mw.IntPrimitive = md.Price;\n" +
                      statementText;
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean_A("E1"));
            env.SendEventBean(new SupportBean_A("E2"));

            SendBeanEvent(env, "E1", 10);
            SendBeanEvent(env, "E2", 15);
            SendBeanEvent(env, "E1", 10);
            SendBeanEvent(env, "E2", 5);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendBeanEvent(env, "E2", 5);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 25});

            SendBeanEvent(env, "E2", -6); // to 19, does not count toward condition
            SendBeanEvent(env, "E2", 2); // to 21, counts toward condition
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            SendBeanEvent(env, "E2", 1);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 22});

            SendBeanEvent(env, "E2", 1); // to 23, counts toward condition
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            SendBeanEvent(env, "E2", 1); // to 24
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 24});

            SendBeanEvent(env, "E2", -10); // to 14
            SendBeanEvent(env, "E2", 10); // to 24, counts toward condition
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            SendBeanEvent(env, "E2", 0); // to 24, counts toward condition
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 24});

            SendBeanEvent(env, "E2", -10); // to 14
            SendBeanEvent(env, "E2", 1); // to 15
            SendBeanEvent(env, "E2", 5); // to 20
            SendBeanEvent(env, "E2", 0); // to 20
            SendBeanEvent(env, "E2", 1); // to 21    // counts
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendBeanEvent(env, "E2", 0); // to 21
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 21});

            // remove events
            SendMDEvent(env, "E2", 0);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 21});

            // remove events
            SendMDEvent(env, "E2", -10);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 41});

            // remove events
            SendMDEvent(env, "E2", -6); // since there is 3*-10 we output the next one
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 47});

            SendMDEvent(env, "E2", 2);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.UndeployAll();
        }

        private static void TryAssertionSingle(RegressionEnvironment env)
        {
            // assert select result type
            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("mySum"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("myAvg"));

            SendMDEvent(env, SYMBOL_DELL, 10);
            Assert.IsTrue(env.Listener("s0").IsInvoked);
            AssertEvent(
                env,
                SYMBOL_DELL,
                null,
                null,
                10d,
                10d);

            SendMDEvent(env, SYMBOL_IBM, 20);
            Assert.IsTrue(env.Listener("s0").IsInvoked);
            AssertEvent(
                env,
                SYMBOL_IBM,
                null,
                null,
                20d,
                20d);
        }

        private static void TryAssertionAll(RegressionEnvironment env)
        {
            // assert select result type
            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("mySum"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("myAvg"));

            SendMDEvent(env, SYMBOL_IBM, 70);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendMDEvent(env, SYMBOL_DELL, 10);
            AssertEvents(
                env,
                SYMBOL_IBM,
                null,
                null,
                70d,
                70d,
                SYMBOL_DELL,
                null,
                null,
                10d,
                10d);
            env.Listener("s0").Reset();

            SendMDEvent(env, SYMBOL_DELL, 20);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendMDEvent(env, SYMBOL_DELL, 100);
            AssertEvents(
                env,
                SYMBOL_IBM,
                70d,
                70d,
                70d,
                70d,
                SYMBOL_DELL,
                10d,
                10d,
                130d,
                130d / 3d);
        }

        private static void TryAssertion12(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                200,
                1,
                new[] {new object[] {"IBM", 25d}},
                new[] {new object[] {"IBM", null}});
            expected.AddResultInsRem(
                800,
                1,
                new[] {new object[] {"MSFT", 9d}},
                new[] {new object[] {"MSFT", null}});
            expected.AddResultInsRem(
                1500,
                1,
                new[] {new object[] {"IBM", 49d}},
                new[] {new object[] {"IBM", 25d}});
            expected.AddResultInsRem(
                1500,
                2,
                new[] {new object[] {"YAH", 1d}},
                new[] {new object[] {"YAH", null}});
            expected.AddResultInsRem(
                2100,
                1,
                new[] {new object[] {"IBM", 75d}},
                new[] {new object[] {"IBM", 49d}});
            expected.AddResultInsRem(
                3500,
                1,
                new[] {new object[] {"YAH", 3d}},
                new[] {new object[] {"YAH", 1d}});
            expected.AddResultInsRem(
                4300,
                1,
                new[] {new object[] {"IBM", 97d}},
                new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(
                4900,
                1,
                new[] {new object[] {"YAH", 6d}},
                new[] {new object[] {"YAH", 3d}});
            expected.AddResultInsRem(
                5700,
                0,
                new[] {new object[] {"IBM", 72d}},
                new[] {new object[] {"IBM", 97d}});
            expected.AddResultInsRem(
                5900,
                1,
                new[] {new object[] {"YAH", 7d}},
                new[] {new object[] {"YAH", 6d}});
            expected.AddResultInsRem(
                6300,
                0,
                new[] {new object[] {"MSFT", null}},
                new[] {new object[] {"MSFT", 9d}});
            expected.AddResultInsRem(
                7000,
                0,
                new[] {new object[] {"IBM", 48d}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 72d}, new object[] {"YAH", 7d}});

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void TryAssertion34(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                2100,
                1,
                new[] {new object[] {"IBM", 75d}},
                null);
            expected.AddResultInsRem(
                4300,
                1,
                new[] {new object[] {"IBM", 97d}},
                new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(
                5700,
                0,
                new[] {new object[] {"IBM", 72d}},
                new[] {new object[] {"IBM", 97d}});
            expected.AddResultInsRem(
                7000,
                0,
                null,
                new[] {new object[] {"IBM", 72d}});

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void TryAssertion13_14(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200,
                0,
                new[] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}},
                new[] {new object[] {"IBM", null}, new object[] {"MSFT", null}});
            expected.AddResultInsRem(
                2200,
                0,
                new[] {new object[] {"IBM", 75d}, new object[] {"YAH", 1d}},
                new[] {new object[] {"IBM", 25d}, new object[] {"YAH", null}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(
                4200,
                0,
                new[] {new object[] {"YAH", 3d}},
                new[] {new object[] {"YAH", 1d}});
            expected.AddResultInsRem(
                5200,
                0,
                new[] {new object[] {"IBM", 97d}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 75d}, new object[] {"YAH", 3d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {"IBM", 72d}, new object[] {"YAH", 7d}},
                new[] {new object[] {"IBM", 97d}, new object[] {"YAH", 6d}});
            expected.AddResultInsRem(
                7200,
                0,
                new[] {new object[] {"IBM", 48d}, new object[] {"MSFT", null}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}});

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void TryAssertion15_16(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(
                2200,
                0,
                new[] {new object[] {"IBM", 75d}},
                null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(
                5200,
                0,
                new[] {new object[] {"IBM", 97d}},
                new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {"IBM", 72d}},
                new[] {new object[] {"IBM", 97d}});
            expected.AddResultInsRem(
                7200,
                0,
                null,
                new[] {new object[] {"IBM", 72d}});

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void TryAssertion78(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(
                2200,
                0,
                new[] {new object[] {"IBM", 75d}},
                null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(
                5200,
                0,
                new[] {new object[] {"IBM", 97d}},
                new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {"IBM", 72d}},
                new[] {new object[] {"IBM", 97d}});
            expected.AddResultInsRem(
                7200,
                0,
                null,
                new[] {new object[] {"IBM", 72d}});

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void TryAssertion56(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200,
                0,
                new[] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}},
                new[] {new object[] {"IBM", null}, new object[] {"MSFT", null}});
            expected.AddResultInsRem(
                2200,
                0,
                new[] {new object[] {"IBM", 49d}, new object[] {"IBM", 75d}, new object[] {"YAH", 1d}},
                new[] {new object[] {"IBM", 25d}, new object[] {"IBM", 49d}, new object[] {"YAH", null}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(
                4200,
                0,
                new[] {new object[] {"YAH", 3d}},
                new[] {new object[] {"YAH", 1d}});
            expected.AddResultInsRem(
                5200,
                0,
                new[] {new object[] {"IBM", 97d}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 75d}, new object[] {"YAH", 3d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {"IBM", 72d}, new object[] {"YAH", 7d}},
                new[] {new object[] {"IBM", 97d}, new object[] {"YAH", 6d}});
            expected.AddResultInsRem(
                7200,
                0,
                new[] {new object[] {"IBM", 48d}, new object[] {"MSFT", null}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}});

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void TryAssertion9_10(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200,
                0,
                new[] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}},
                new[] {new object[] {"IBM", null}, new object[] {"MSFT", null}});
            expected.AddResultInsRem(
                2200,
                0,
                new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}},
                new[] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}, new object[] {"YAH", null}});
            expected.AddResultInsRem(
                3200,
                0,
                new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}},
                new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}});
            expected.AddResultInsRem(
                4200,
                0,
                new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 3d}},
                new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}});
            expected.AddResultInsRem(
                5200,
                0,
                new[] {new object[] {"IBM", 97d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 3d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}},
                new[] {new object[] {"IBM", 97d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 6d}});
            expected.AddResultInsRem(
                7200,
                0,
                new[] {new object[] {"IBM", 48d}, new object[] {"MSFT", null}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}});

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void TryAssertion11_12(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(
                2200,
                0,
                new[] {new object[] {"IBM", 75d}},
                null);
            expected.AddResultInsRem(
                3200,
                0,
                new[] {new object[] {"IBM", 75d}},
                new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(
                4200,
                0,
                new[] {new object[] {"IBM", 75d}},
                new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(
                5200,
                0,
                new[] {new object[] {"IBM", 97d}},
                new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {"IBM", 72d}},
                new[] {new object[] {"IBM", 97d}});
            expected.AddResultInsRem(
                7200,
                0,
                null,
                new[] {new object[] {"IBM", 72d}});

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void TryAssertion17(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                200,
                1,
                new[] {new object[] {"IBM", 25d}},
                new[] {new object[] {"IBM", null}});
            expected.AddResultInsRem(
                800,
                1,
                new[] {new object[] {"MSFT", 9d}},
                new[] {new object[] {"MSFT", null}});
            expected.AddResultInsRem(
                1500,
                1,
                new[] {new object[] {"IBM", 49d}},
                new[] {new object[] {"IBM", 25d}});
            expected.AddResultInsRem(
                1500,
                2,
                new[] {new object[] {"YAH", 1d}},
                new[] {new object[] {"YAH", null}});
            expected.AddResultInsRem(
                3500,
                1,
                new[] {new object[] {"YAH", 3d}},
                new[] {new object[] {"YAH", 1d}});
            expected.AddResultInsRem(
                4300,
                1,
                new[] {new object[] {"IBM", 97d}},
                new[] {new object[] {"IBM", 75d}});
            expected.AddResultInsRem(
                4900,
                1,
                new[] {new object[] {"YAH", 6d}},
                new[] {new object[] {"YAH", 3d}});
            expected.AddResultInsRem(
                5700,
                0,
                new[] {new object[] {"IBM", 72d}},
                new[] {new object[] {"IBM", 97d}});
            expected.AddResultInsRem(
                5900,
                1,
                new[] {new object[] {"YAH", 7d}},
                new[] {new object[] {"YAH", 6d}});
            expected.AddResultInsRem(
                6300,
                0,
                new[] {new object[] {"MSFT", null}},
                new[] {new object[] {"MSFT", 9d}});
            expected.AddResultInsRem(
                7000,
                0,
                new[] {new object[] {"IBM", 48d}, new object[] {"YAH", 6d}},
                new[] {new object[] {"IBM", 72d}, new object[] {"YAH", 7d}});

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void TryAssertion18(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new[] {new object[] {"IBM", 25d}, new object[] {"MSFT", 9d}});
            expected.AddResultInsert(
                2200,
                0,
                new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}});
            expected.AddResultInsert(
                3200,
                0,
                new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 1d}});
            expected.AddResultInsert(
                4200,
                0,
                new[] {new object[] {"IBM", 75d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 3d}});
            expected.AddResultInsert(
                5200,
                0,
                new[] {new object[] {"IBM", 97d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 6d}});
            expected.AddResultInsert(
                6200,
                0,
                new[] {new object[] {"IBM", 72d}, new object[] {"MSFT", 9d}, new object[] {"YAH", 7d}});
            expected.AddResultInsert(
                7200,
                0,
                new[] {new object[] {"IBM", 48d}, new object[] {"YAH", 6d}});

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void AssertEvent(
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

        private static void SendMDEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        private static void SendBeanEvent(
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

        internal class ResultSetCrontabNumberSetVariations : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "select TheString from SupportBean output all at (*/2, 8:17, lastweekday, [1, 1], *)");
                env.SendEventBean(new SupportBean());
                env.UndeployAll();

                env.CompileDeploy("select TheString from SupportBean output all at (*/2, 8:17, 30 weekday, [1, 1], *)");
                env.SendEventBean(new SupportBean());
                env.UndeployAll();
            }
        }

        internal class ResultSetLastNoDataWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@Name('s0') select TheString, IntPrimitive as intp from SupportBean group by TheString output last every 1 seconds order by TheString asc";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E3", 31));
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E1", 2));
                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E2", 22));
                env.SendEventBean(new SupportBean("E2", 21));
                env.SendEventBean(new SupportBean("E1", 3));
                env.AdvanceTime(1000);

                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    new[] {"TheString", "intp"},
                    new[] {new object[] {"E1", 3}, new object[] {"E2", 21}, new object[] {"E3", 31}});

                env.SendEventBean(new SupportBean("E3", 31));
                env.SendEventBean(new SupportBean("E1", 5));
                env.SendEventBean(new SupportBean("E3", 33));
                env.AdvanceTime(2000);

                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    new[] {"TheString", "intp"},
                    new[] {new object[] {"E1", 5}, new object[] {"E3", 33}});

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputFirstHavingJoinNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select TheString, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
                TryOutputFirstHaving(env, stmtText);

                var stmtTextJoin =
                    "@Name('s0') select TheString, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.Id = mv.TheString " +
                    "group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
                TryOutputFirstHaving(env, stmtTextJoin);

                var stmtTextOrder =
                    "@Name('s0') select TheString, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";
                TryOutputFirstHaving(env, stmtTextOrder);

                var stmtTextOrderJoin =
                    "@Name('s0') select TheString, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.Id = mv.TheString " +
                    "group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";
                TryOutputFirstHaving(env, stmtTextOrderJoin);
            }
        }

        internal class ResultSetOutputFirstCrontab : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var fields = new [] { "TheString","value" };
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportMarketDataBean md delete from MyWindow mw where mw.IntPrimitive = md.Price;\n" +
                          "@Name('s0') select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first at (*/2, *, *, *, *)";
                env.CompileDeploy(epl).AddListener("s0");

                SendBeanEvent(env, "E1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10});

                SendTimer(env, 2 * 60 * 1000 - 1);
                SendBeanEvent(env, "E1", 11);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 2 * 60 * 1000);
                SendBeanEvent(env, "E1", 12);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 33});

                SendBeanEvent(env, "E2", 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 20});

                SendBeanEvent(env, "E2", 21);
                SendTimer(env, 4 * 60 * 1000 - 1);
                SendBeanEvent(env, "E2", 22);
                SendBeanEvent(env, "E1", 13);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 4 * 60 * 1000);
                SendBeanEvent(env, "E2", 23);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 86});
                SendBeanEvent(env, "E1", 14);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 60});

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputFirstWhenThen : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString","value" };
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportMarketDataBean md delete from MyWindow mw where mw.IntPrimitive = md.Price;\n" +
                          "@Name('s0') select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first when varoutone then set varoutone = false;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendBeanEvent(env, "E1", 10);
                SendBeanEvent(env, "E1", 11);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Runtime.VariableService.SetVariableValue(null, "varoutone", true);
                SendBeanEvent(env, "E1", 12);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 33});
                Assert.AreEqual(false, env.Runtime.VariableService.GetVariableValue(null, "varoutone"));

                env.Runtime.VariableService.SetVariableValue(null, "varoutone", true);
                SendBeanEvent(env, "E2", 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 20});
                Assert.AreEqual(false, env.Runtime.VariableService.GetVariableValue(null, "varoutone"));

                SendBeanEvent(env, "E1", 13);
                SendBeanEvent(env, "E2", 21);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputFirstEveryNEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString","value" };
                var path = new RegressionPath();
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "on SupportMarketDataBean md delete from MyWindow mw where mw.IntPrimitive = md.Price;\n";
                env.CompileDeploy(epl, path);

                epl =
                    "@Name('s0') select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first every 3 events;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                SendBeanEvent(env, "E1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10});

                SendBeanEvent(env, "E1", 12);
                SendBeanEvent(env, "E1", 11);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBeanEvent(env, "E1", 13);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 46});

                SendMDEvent(env, "S1", 12);
                SendMDEvent(env, "S1", 11);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendMDEvent(env, "S1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 13});

                SendBeanEvent(env, "E1", 14);
                SendBeanEvent(env, "E1", 15);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBeanEvent(env, "E2", 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 20});
                env.UndeployModuleContaining("s0");

                // test variable
                env.CompileDeploy("@Name('var') create variable int myvar_local = 1", path);
                env.CompileDeploy(
                    "@Name('s0') select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first every myvar_local events",
                    path);
                env.AddListener("s0");

                SendBeanEvent(env, "E3", 10);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E3", 10}});

                SendBeanEvent(env, "E1", 5);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", 47}});

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("var"), "myvar_local", 2);

                SendBeanEvent(env, "E1", 6);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBeanEvent(env, "E1", 7);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", 60}});

                SendBeanEvent(env, "E1", 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBeanEvent(env, "E1", 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", 62}});

                env.UndeployAll();
            }
        }

        internal class ResultSetWildcardRowPerGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select * from SupportBean group by TheString output last every 3 events order by TheString asc";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("IBM", 10));
                env.SendEventBean(new SupportBean("ATT", 11));
                env.SendEventBean(new SupportBean("IBM", 100));

                var events = env.Listener("s0").NewDataListFlattened;
                env.Listener("s0").Reset();
                Assert.AreEqual(2, events.Length);
                Assert.AreEqual("ATT", events[0].Get("TheString"));
                Assert.AreEqual(11, events[0].Get("IntPrimitive"));
                Assert.AreEqual("IBM", events[1].Get("TheString"));
                Assert.AreEqual(100, events[1].Get("IntPrimitive"));
                env.UndeployAll();

                // All means each event
                epl = "@Name('s0') select * from SupportBean group by TheString output all every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("IBM", 10));
                env.SendEventBean(new SupportBean("ATT", 11));
                env.SendEventBean(new SupportBean("IBM", 100));

                events = env.Listener("s0").NewDataListFlattened;
                Assert.AreEqual(3, events.Length);
                Assert.AreEqual("IBM", events[0].Get("TheString"));
                Assert.AreEqual(10, events[0].Get("IntPrimitive"));
                Assert.AreEqual("ATT", events[1].Get("TheString"));
                Assert.AreEqual(11, events[1].Get("IntPrimitive"));
                Assert.AreEqual("IBM", events[2].Get("TheString"));
                Assert.AreEqual(100, events[2].Get("IntPrimitive"));

                env.UndeployAll();
            }
        }

        internal class ResultSet1NoneNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec)" +
                               "group by Symbol " +
                               "order by Symbol asc";
                TryAssertion12(env, stmtText, "none");
            }
        }

        internal class ResultSet2NoneNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "group by Symbol " +
                               "order by Symbol asc";
                TryAssertion12(env, stmtText, "none");
            }
        }

        internal class ResultSet3NoneHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "group by Symbol " +
                               " having sum(Price) > 50";
                TryAssertion34(env, stmtText, "none");
            }
        }

        internal class ResultSet4NoneHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "group by Symbol " +
                               "having sum(Price) > 50";
                TryAssertion34(env, stmtText, "none");
            }
        }

        internal class ResultSet5DefaultNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "group by Symbol " +
                               "output every 1 seconds order by Symbol asc";
                TryAssertion56(env, stmtText, "default");
            }
        }

        internal class ResultSet6DefaultNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "group by Symbol " +
                               "output every 1 seconds order by Symbol asc";
                TryAssertion56(env, stmtText, "default");
            }
        }

        internal class ResultSet7DefaultHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) \n" +
                               "group by Symbol " +
                               "having sum(Price) > 50" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default");
            }
        }

        internal class ResultSet8DefaultHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "group by Symbol " +
                               "having sum(Price) > 50" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default");
            }
        }

        internal class ResultSet9AllNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "group by Symbol " +
                               "output all every 1 seconds " +
                               "order by Symbol";
                TryAssertion9_10(env, stmtText, "all");
            }
        }

        internal class ResultSet10AllNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "group by Symbol " +
                               "output all every 1 seconds " +
                               "order by Symbol";
                TryAssertion9_10(env, stmtText, "all");
            }
        }

        internal class ResultSet11AllHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion11AllHavingNoJoin(env, outputLimitOpt);
                }
            }
        }

        internal class ResultSet12AllHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion12AllHavingJoin(env, outputLimitOpt);
                }
            }
        }

        internal class ResultSet13LastNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec)" +
                               "group by Symbol " +
                               "output last every 1 seconds " +
                               "order by Symbol";
                TryAssertion13_14(env, stmtText, "last");
            }
        }

        internal class ResultSet14LastNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "group by Symbol " +
                               "output last every 1 seconds " +
                               "order by Symbol";
                TryAssertion13_14(env, stmtText, "last");
            }
        }

        internal class ResultSet15LastHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion15LastHavingNoJoin(env, outputLimitOpt);
                }
            }
        }

        internal class ResultSet16LastHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion16LastHavingJoin(env, outputLimitOpt);
                }
            }
        }

        internal class ResultSet17FirstNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "group by Symbol " +
                               "output first every 1 seconds";
                TryAssertion17(env, stmtText, "first");
            }
        }

        internal class ResultSet17FirstNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "group by Symbol " +
                               "output first every 1 seconds";
                TryAssertion17(env, stmtText, "first");
            }
        }

        internal class ResultSet18SnapshotNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "group by Symbol " +
                               "output snapshot every 1 seconds " +
                               "order by Symbol";
                TryAssertion18(env, stmtText, "snapshot");
            }
        }

        internal class ResultSet18SnapshotNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "group by Symbol " +
                               "output snapshot every 1 seconds " +
                               "order by Symbol";
                TryAssertion18(env, stmtText, "snapshot");
            }
        }

        internal class ResultSetJoinSortWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var fields = new [] { "Symbol","maxVol" };
                var epl = "@Name('s0') select irstream Symbol, max(Price) as maxVol" +
                          " from SupportMarketDataBean#sort(1, Volume desc) as S0," +
                          "SupportBean#keepall as S1 " +
                          "group by Symbol output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("JOIN_KEY", -1));

                SendMDEvent(env, "JOIN_KEY", 1d);
                SendMDEvent(env, "JOIN_KEY", 2d);
                env.Listener("s0").Reset();

                // moves all events out of the window,
                SendTimer(env, 1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
                var result = env.Listener("s0").DataListsFlattened;
                Assert.AreEqual(2, result.First.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    result.First,
                    fields,
                    new[] {new object[] {"JOIN_KEY", 1.0}, new object[] {"JOIN_KEY", 2.0}});
                Assert.AreEqual(2, result.Second.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Second,
                    fields,
                    new[] {new object[] {"JOIN_KEY", null}, new object[] {"JOIN_KEY", 1.0}});

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt = "@Name('s0') select Symbol, min(Price) as minPrice from SupportMarketDataBean" +
                                 "#time(10 seconds) group by Symbol output snapshot every 1 seconds order by Symbol asc";

                env.CompileDeploy(selectStmt).AddListener("s0");

                SendMDEvent(env, "ABC", 20);

                SendTimer(env, 500);
                SendMDEvent(env, "IBM", 16);
                SendMDEvent(env, "ABC", 14);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 1000);
                string[] fields = {"Symbol", "minPrice"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"ABC", 14d}, new object[] {"IBM", 16d}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 1500);
                SendMDEvent(env, "IBM", 18);
                SendMDEvent(env, "MSFT", 30);

                SendTimer(env, 10000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"ABC", 14d}, new object[] {"IBM", 16d}, new object[] {"MSFT", 30d}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 11000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"IBM", 18d}, new object[] {"MSFT", 30d}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 12000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshotLimit : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt = "@Name('s0') select Symbol, min(Price) as minPrice from SupportMarketDataBean" +
                                 "#time(10 seconds) as m, " +
                                 "SupportBean#keepall as s where s.TheString = m.Symbol " +
                                 "group by Symbol output snapshot every 1 seconds order by Symbol asc";
                env.CompileDeploy(selectStmt).AddListener("s0");

                foreach (var theString in new [] { "ABC","IBM","MSFT" }) {
                    env.SendEventBean(new SupportBean(theString, 1));
                }

                SendMDEvent(env, "ABC", 20);

                SendTimer(env, 500);
                SendMDEvent(env, "IBM", 16);
                SendMDEvent(env, "ABC", 14);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 1000);
                string[] fields = {"Symbol", "minPrice"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"ABC", 14d}, new object[] {"IBM", 16d}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 1500);
                SendMDEvent(env, "IBM", 18);
                SendMDEvent(env, "MSFT", 30);

                SendTimer(env, 10000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"ABC", 14d}, new object[] {"IBM", 16d}, new object[] {"MSFT", 30d}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 10500);
                SendTimer(env, 11000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"IBM", 18d}, new object[] {"MSFT", 30d}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 11500);
                SendTimer(env, 12000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ResultSetGroupByAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "Symbol","sum(Price)" };
                var statementString =
                    "@Name('s0') select irstream Symbol, sum(Price) from SupportMarketDataBean#length(5) group by Symbol output all every 5 events";
                env.CompileDeploy(statementString).AddListener("s0");

                // send some events and check that only the most recent
                // ones are kept
                SendMDEvent(env, "IBM", 1D);
                SendMDEvent(env, "IBM", 2D);
                SendMDEvent(env, "HP", 1D);
                SendMDEvent(env, "IBM", 3D);
                SendMDEvent(env, "MAC", 1D);

                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                var newData = env.Listener("s0").LastNewData;
                Assert.AreEqual(3, newData.Length);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    newData,
                    fields,
                    new[] {
                        new object[] {"IBM", 6d}, new object[] {"HP", 1d}, new object[] {"MAC", 1d}
                    });
                var oldData = env.Listener("s0").LastOldData;
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    oldData,
                    fields,
                    new[] {
                        new object[] {"IBM", null}, new object[] {"HP", null}, new object[] {"MAC", null}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetGroupByDefault : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "Symbol","sum(Price)" };
                var epl =
                    "@Name('s0') select irstream Symbol, sum(Price) from SupportMarketDataBean#length(5) group by Symbol output every 5 events";
                env.CompileDeploy(epl).AddListener("s0");

                // send some events and check that only the most recent
                // ones are kept
                SendMDEvent(env, "IBM", 1D);
                SendMDEvent(env, "IBM", 2D);
                SendMDEvent(env, "HP", 1D);
                SendMDEvent(env, "IBM", 3D);
                SendMDEvent(env, "MAC", 1D);

                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                var newData = env.Listener("s0").LastNewData;
                var oldData = env.Listener("s0").LastOldData;
                Assert.AreEqual(5, newData.Length);
                Assert.AreEqual(5, oldData.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    newData,
                    fields,
                    new[] {
                        new object[] {"IBM", 1d}, new object[] {"IBM", 3d}, new object[] {"HP", 1d},
                        new object[] {"IBM", 6d}, new object[] {"MAC", 1d}
                    });
                EPAssertionUtil.AssertPropsPerRow(
                    oldData,
                    fields,
                    new[] {
                        new object[] {"IBM", null}, new object[] {"IBM", 1d}, new object[] {"HP", null},
                        new object[] {"IBM", 3d}, new object[] {"MAC", null}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetMaxTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var fields = new [] { "Symbol","maxVol" };
                var epl = "@Name('s0') select irstream Symbol, max(Price) as maxVol" +
                          " from SupportMarketDataBean#time(1 sec) " +
                          "group by Symbol output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                SendMDEvent(env, "SYM1", 1d);
                SendMDEvent(env, "SYM1", 2d);
                env.Listener("s0").Reset();

                // moves all events out of the window,
                SendTimer(env, 1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
                var result = env.Listener("s0").DataListsFlattened;
                Assert.AreEqual(3, result.First.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    result.First,
                    fields,
                    new[] {new object[] {"SYM1", 1.0}, new object[] {"SYM1", 2.0}, new object[] {"SYM1", null}});
                Assert.AreEqual(3, result.Second.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Second,
                    fields,
                    new[] {new object[] {"SYM1", null}, new object[] {"SYM1", 1.0}, new object[] {"SYM1", 2.0}});

                env.UndeployAll();
            }
        }

        internal class ResultSetNoJoinLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionNoJoinLast(env, outputLimitOpt);
                }
            }

            private static void TryAssertionNoJoinLast(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                var epl = opt.GetHint() +
                          "@Name('s0') select irstream Symbol," +
                          "sum(Price) as mySum," +
                          "avg(Price) as myAvg " +
                          "from SupportMarketDataBean#length(3) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol " +
                          "output last every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionLast(env);
                env.UndeployAll();
            }
        }

        internal class ResultSetNoOutputClauseView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream Symbol," +
                          "sum(Price) as mySum," +
                          "avg(Price) as myAvg " +
                          "from SupportMarketDataBean#length(3) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol";

                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionSingle(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetNoOutputClauseJoin : RegressionExecution
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

                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));
                env.SendEventBean(new SupportBeanString("AAA"));

                TryAssertionSingle(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetNoJoinAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionNoJoinAll(env, outputLimitOpt);
                }
            }

            private static void TryAssertionNoJoinAll(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                var epl = opt.GetHint() +
                          "@Name('s0') select irstream Symbol," +
                          "sum(Price) as mySum," +
                          "avg(Price) as myAvg " +
                          "from SupportMarketDataBean#length(5) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol " +
                          "output all every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionAll(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetJoinLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionJoinLast(env, outputLimitOpt);
                }
            }

            private static void TryAssertionJoinLast(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                var epl = opt.GetHint() +
                          "@Name('s0') select irstream Symbol," +
                          "sum(Price) as mySum," +
                          "avg(Price) as myAvg " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(3) as two " +
                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                          "       and one.TheString = two.Symbol " +
                          "group by Symbol " +
                          "output last every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));
                env.SendEventBean(new SupportBeanString("AAA"));

                TryAssertionLast(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetJoinAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionJoinAll(env, outputLimitOpt);
                }
            }
        }
    }
} // end of namespace