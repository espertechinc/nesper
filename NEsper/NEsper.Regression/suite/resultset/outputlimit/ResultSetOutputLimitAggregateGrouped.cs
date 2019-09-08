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
    public class ResultSetOutputLimitAggregateGrouped
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";
        private const string CATEGORY = "Aggregated and Grouped";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetUnaggregatedOutputFirst());
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
            execs.Add(new ResultSetHaving());
            execs.Add(new ResultSetHavingJoin());
            execs.Add(new ResultSetJoinSortWindow());
            execs.Add(new ResultSetLimitSnapshot());
            execs.Add(new ResultSetLimitSnapshotJoin());
            execs.Add(new ResultSetMaxTimeWindow());
            execs.Add(new ResultSetNoJoinLast());
            execs.Add(new ResultSetNoOutputClauseView());
            execs.Add(new ResultSetNoJoinDefault());
            execs.Add(new ResultSetJoinDefault());
            execs.Add(new ResultSetNoJoinAll());
            execs.Add(new ResultSetJoinAll());
            execs.Add(new ResultSetJoinLast());
            execs.Add(new ResultSetOutputFirstHavingJoinNoJoin());
            return execs;
        }

        private static void RunAssertion16LastHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select Symbol, Volume, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=Symbol " +
                           "group by Symbol " +
                           "having sum(Price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last");
        }

        private static void TryAssertionJoinLast(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            var epl = opt.GetHint() +
                      "@Name('s0') select Symbol, Volume, sum(Price) as mySum " +
                      "from SupportBeanString#length(100) as one, " +
                      "SupportMarketDataBean#length(5) as two " +
                      "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                      "  and one.TheString = two.Symbol " +
                      "group by Symbol " +
                      "output last every 2 events";

            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
            env.SendEventBean(new SupportBeanString(SYMBOL_IBM));

            TryAssertionLast(env);

            env.UndeployAll();
        }

        private static void TryAssertionHavingDefault(RegressionEnvironment env)
        {
            SendEvent(env, "IBM", 1, 5);
            SendEvent(env, "IBM", 2, 6);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendEvent(env, "IBM", 3, -3);
            var fields = new [] { "symbol","Volume","sumPrice" };
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"IBM", 2L, 11.0});

            SendTimer(env, 5000);
            SendEvent(env, "IBM", 4, 10);
            SendEvent(env, "IBM", 5, 0);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendEvent(env, "IBM", 6, 1);
            Assert.AreEqual(3, env.Listener("s0").LastNewData.Length);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastNewData[0],
                fields,
                new object[] {"IBM", 4L, 18.0});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastNewData[1],
                fields,
                new object[] {"IBM", 5L, 18.0});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastNewData[2],
                fields,
                new object[] {"IBM", 6L, 19.0});
            env.Listener("s0").Reset();

            SendTimer(env, 11000);
            Assert.AreEqual(3, env.Listener("s0").LastOldData.Length);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastOldData[0],
                fields,
                new object[] {"IBM", 1L, 11.0});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastOldData[1],
                fields,
                new object[] {"IBM", 2L, 11.0});
            env.Listener("s0").Reset();
        }

        private static void TryAssertionDefault(RegressionEnvironment env)
        {
            // assert select result type
            var eventType = env.Statement("s0").EventType;
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("Volume"));
            Assert.AreEqual(typeof(double?), eventType.GetPropertyType("mySum"));

            SendEvent(env, SYMBOL_IBM, 500, 20);
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendEvent(env, SYMBOL_DELL, 10000, 51);
            var fields = new [] { "symbol","Volume","mySum" };
            var events = env.Listener("s0").DataListsFlattened;
            if (events.First[0].Get("Symbol").Equals(SYMBOL_IBM)) {
                EPAssertionUtil.AssertPropsPerRow(
                    events.First,
                    fields,
                    new[] {new object[] {SYMBOL_IBM, 500L, 20.0}, new object[] {SYMBOL_DELL, 10000L, 51.0}});
            }
            else {
                EPAssertionUtil.AssertPropsPerRow(
                    events.First,
                    fields,
                    new[] {new object[] {SYMBOL_DELL, 10000L, 51.0}, new object[] {SYMBOL_IBM, 500L, 20.0}});
            }

            Assert.IsNull(env.Listener("s0").LastOldData);

            env.Listener("s0").Reset();

            SendEvent(env, SYMBOL_DELL, 20000, 52);
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendEvent(env, SYMBOL_DELL, 40000, 45);
            events = env.Listener("s0").DataListsFlattened;
            EPAssertionUtil.AssertPropsPerRow(
                events.First,
                fields,
                new[] {
                    new object[] {SYMBOL_DELL, 20000L, 51.0 + 52.0},
                    new object[] {SYMBOL_DELL, 40000L, 51.0 + 52.0 + 45.0}
                });
            Assert.IsNull(env.Listener("s0").LastOldData);
        }

        private static void TryAssertionAll(RegressionEnvironment env)
        {
            // assert select result type
            var eventType = env.Statement("s0").EventType;
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("Volume"));
            Assert.AreEqual(typeof(double?), eventType.GetPropertyType("mySum"));

            SendEvent(env, SYMBOL_IBM, 500, 20);
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendEvent(env, SYMBOL_DELL, 10000, 51);
            var fields = new [] { "symbol","Volume","mySum" };
            var events = env.Listener("s0").DataListsFlattened;
            if (events.First[0].Get("Symbol").Equals(SYMBOL_IBM)) {
                EPAssertionUtil.AssertPropsPerRow(
                    events.First,
                    fields,
                    new[] {new object[] {SYMBOL_IBM, 500L, 20.0}, new object[] {SYMBOL_DELL, 10000L, 51.0}});
            }
            else {
                EPAssertionUtil.AssertPropsPerRow(
                    events.First,
                    fields,
                    new[] {new object[] {SYMBOL_DELL, 10000L, 51.0}, new object[] {SYMBOL_IBM, 500L, 20.0}});
            }

            Assert.IsNull(env.Listener("s0").LastOldData);
            env.Listener("s0").Reset();

            SendEvent(env, SYMBOL_DELL, 20000, 52);
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendEvent(env, SYMBOL_DELL, 40000, 45);
            events = env.Listener("s0").DataListsFlattened;
            if (events.First[0].Get("Symbol").Equals(SYMBOL_IBM)) {
                EPAssertionUtil.AssertPropsPerRow(
                    events.First,
                    fields,
                    new[] {
                        new object[] {SYMBOL_IBM, 500L, 20.0}, new object[] {SYMBOL_DELL, 20000L, 51.0 + 52.0},
                        new object[] {SYMBOL_DELL, 40000L, 51.0 + 52.0 + 45.0}
                    });
            }
            else {
                EPAssertionUtil.AssertPropsPerRow(
                    events.First,
                    fields,
                    new[] {
                        new object[] {SYMBOL_DELL, 20000L, 51.0 + 52.0},
                        new object[] {SYMBOL_DELL, 40000L, 51.0 + 52.0 + 45.0}, new object[] {SYMBOL_IBM, 500L, 20.0}
                    });
            }

            Assert.IsNull(env.Listener("s0").LastOldData);
        }

        private static void TryAssertionLast(RegressionEnvironment env)
        {
            var fields = new [] { "symbol","Volume","mySum" };
            SendEvent(env, SYMBOL_DELL, 10000, 51);
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendEvent(env, SYMBOL_DELL, 20000, 52);
            var events = env.Listener("s0").DataListsFlattened;
            EPAssertionUtil.AssertPropsPerRow(
                events.First,
                fields,
                new[] {new object[] {SYMBOL_DELL, 20000L, 103.0}});
            Assert.IsNull(env.Listener("s0").LastOldData);
            env.Listener("s0").Reset();

            SendEvent(env, SYMBOL_DELL, 30000, 70);
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendEvent(env, SYMBOL_IBM, 10000, 20);
            events = env.Listener("s0").DataListsFlattened;
            if (events.First[0].Get("Symbol").Equals(SYMBOL_DELL)) {
                EPAssertionUtil.AssertPropsPerRow(
                    events.First,
                    fields,
                    new[] {new object[] {SYMBOL_DELL, 30000L, 173.0}, new object[] {SYMBOL_IBM, 10000L, 20.0}});
            }
            else {
                EPAssertionUtil.AssertPropsPerRow(
                    events.First,
                    fields,
                    new[] {new object[] {SYMBOL_IBM, 10000L, 20.0}, new object[] {SYMBOL_DELL, 30000L, 173.0}});
            }

            Assert.IsNull(env.Listener("s0").LastOldData);
        }

        private static void TryOutputFirstHaving(
            RegressionEnvironment env,
            string statementText,
            AtomicLong milestone)
        {
            var fields = new [] { "TheString","LongPrimitive","value" };
            var fieldsLimited = new [] { "TheString","value" };
            var epl = "create window MyWindow#keepall as SupportBean;\n" +
                      "insert into MyWindow select * from SupportBean;\n" +
                      "on SupportMarketDataBean md delete from MyWindow mw where mw.IntPrimitive = md.Price;\n" +
                      statementText;
            var compiled = env.Compile(epl);
            env.Deploy(compiled).AddListener("s0");

            env.SendEventBean(new SupportBean_A("E1"));
            env.SendEventBean(new SupportBean_A("E2"));

            env.MilestoneInc(milestone);

            SendBeanEvent(env, "E1", 101, 10);
            SendBeanEvent(env, "E2", 102, 15);
            SendBeanEvent(env, "E1", 103, 10);
            SendBeanEvent(env, "E2", 104, 5);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.MilestoneInc(milestone);

            SendBeanEvent(env, "E2", 105, 5);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 105L, 25});

            SendBeanEvent(env, "E2", 106, -6); // to 19, does not count toward condition
            SendBeanEvent(env, "E2", 107, 2); // to 21, counts toward condition
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            SendBeanEvent(env, "E2", 108, 1);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 108L, 22});

            env.MilestoneInc(milestone);

            SendBeanEvent(env, "E2", 109, 1); // to 23, counts toward condition
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            SendBeanEvent(env, "E2", 110, 1); // to 24
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 110L, 24});

            SendBeanEvent(env, "E2", 111, -10); // to 14
            SendBeanEvent(env, "E2", 112, 10); // to 24, counts toward condition
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            SendBeanEvent(env, "E2", 113, 0); // to 24, counts toward condition
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 113L, 24});

            env.MilestoneInc(milestone);

            SendBeanEvent(env, "E2", 114, -10); // to 14
            SendBeanEvent(env, "E2", 115, 1); // to 15
            SendBeanEvent(env, "E2", 116, 5); // to 20
            SendBeanEvent(env, "E2", 117, 0); // to 20
            SendBeanEvent(env, "E2", 118, 1); // to 21    // counts
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendBeanEvent(env, "E2", 119, 0); // to 21
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 119L, 21});

            // remove events
            SendMDEvent(env, "E2", 0); // remove 113, 117, 119 (any order of delete!)
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fieldsLimited,
                new object[] {"E2", 21});

            env.MilestoneInc(milestone);

            // remove events
            SendMDEvent(env, "E2", -10); // remove 111, 114
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fieldsLimited,
                new object[] {"E2", 41});

            env.MilestoneInc(milestone);

            // remove events
            SendMDEvent(env, "E2", -6); // since there is 3*0 we output the next one
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fieldsLimited,
                new object[] {"E2", 47});

            SendMDEvent(env, "E2", 2);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.UndeployAll();
        }

        private static void TryAssertion12(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"Symbol", "Volume", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                200,
                1,
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(
                800,
                1,
                new[] {new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(
                1500,
                1,
                new[] {new object[] {"IBM", 150L, 49d}});
            expected.AddResultInsert(
                1500,
                2,
                new[] {new object[] {"YAH", 10000L, 1d}});
            expected.AddResultInsert(
                2100,
                1,
                new[] {new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsert(
                3500,
                1,
                new[] {new object[] {"YAH", 11000L, 3d}});
            expected.AddResultInsert(
                4300,
                1,
                new[] {new object[] {"IBM", 150L, 97d}});
            expected.AddResultInsert(
                4900,
                1,
                new[] {new object[] {"YAH", 11500L, 6d}});
            expected.AddResultRemove(
                5700,
                0,
                new[] {new object[] {"IBM", 100L, 72d}});
            expected.AddResultInsert(
                5900,
                1,
                new[] {new object[] {"YAH", 10500L, 7d}});
            expected.AddResultRemove(
                6300,
                0,
                new[] {new object[] {"MSFT", 5000L, null}});
            expected.AddResultRemove(
                7000,
                0,
                new[] {new object[] {"IBM", 150L, 48d}, new object[] {"YAH", 10000L, 6d}});

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

            string[] fields = {"Symbol", "Volume", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                2100,
                1,
                new[] {new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsert(
                4300,
                1,
                new[] {new object[] {"IBM", 150L, 97d}});
            expected.AddResultRemove(
                5700,
                0,
                new[] {new object[] {"IBM", 100L, 72d}});

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

            string[] fields = {"Symbol", "Volume", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new[] {new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(
                2200,
                0,
                new[] {new object[] {"IBM", 155L, 75d}, new object[] {"YAH", 10000L, 1d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(
                4200,
                0,
                new[] {new object[] {"YAH", 11000L, 3d}});
            expected.AddResultInsert(
                5200,
                0,
                new[] {new object[] {"IBM", 150L, 97d}, new object[] {"YAH", 11500L, 6d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {"YAH", 10500L, 7d}},
                new[] {new object[] {"IBM", 100L, 72d}});
            expected.AddResultRemove(
                7200,
                0,
                new[] {
                    new object[] {"IBM", 150L, 48d}, new object[] {"MSFT", 5000L, null},
                    new object[] {"YAH", 10000L, 6d}
                });

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

            string[] fields = {"Symbol", "Volume", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsert(
                2200,
                0,
                new[] {new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(
                5200,
                0,
                new[] {new object[] {"IBM", 150L, 97d}});
            expected.AddResultInsRem(
                6200,
                0,
                null,
                new[] {new object[] {"IBM", 100L, 72d}});
            expected.AddResultInsRem(7200, 0, null, null);

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

            string[] fields = {"Symbol", "Volume", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsert(
                2200,
                0,
                new[] {new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(
                5200,
                0,
                new[] {new object[] {"IBM", 150L, 97d}});
            expected.AddResultInsRem(
                6200,
                0,
                null,
                new[] {new object[] {"IBM", 100L, 72d}});
            expected.AddResultInsRem(7200, 0, null, null);

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

            string[] fields = {"Symbol", "Volume", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new[] {new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(
                2200,
                0,
                new[] {
                    new object[] {"IBM", 150L, 49d}, new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 75d}
                });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(
                4200,
                0,
                new[] {new object[] {"YAH", 11000L, 3d}});
            expected.AddResultInsert(
                5200,
                0,
                new[] {new object[] {"IBM", 150L, 97d}, new object[] {"YAH", 11500L, 6d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {"YAH", 10500L, 7d}},
                new[] {new object[] {"IBM", 100L, 72d}});
            expected.AddResultRemove(
                7200,
                0,
                new[] {
                    new object[] {"MSFT", 5000L, null}, new object[] {"IBM", 150L, 48d},
                    new object[] {"YAH", 10000L, 6d}
                });

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

            string[] fields = {"Symbol", "Volume", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new[] {new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(
                2200,
                0,
                new[] {
                    new object[] {"IBM", 150L, 49d}, new object[] {"IBM", 155L, 75d}, new object[] {"MSFT", 5000L, 9d},
                    new object[] {"YAH", 10000L, 1d}
                });
            expected.AddResultInsert(
                3200,
                0,
                new[] {
                    new object[] {"IBM", 155L, 75d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"YAH", 10000L, 1d}
                });
            expected.AddResultInsert(
                4200,
                0,
                new[] {
                    new object[] {"IBM", 155L, 75d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"YAH", 11000L, 3d}
                });
            expected.AddResultInsert(
                5200,
                0,
                new[] {
                    new object[] {"IBM", 150L, 97d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"YAH", 11500L, 6d}
                });
            expected.AddResultInsRem(
                6200,
                0,
                new[] {
                    new object[] {"IBM", 150L, 72d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"YAH", 10500L, 7d}
                },
                new[] {new object[] {"IBM", 100L, 72d}});
            expected.AddResultInsRem(
                7200,
                0,
                new[] {
                    new object[] {"IBM", 150L, 48d}, new object[] {"MSFT", 5000L, null},
                    new object[] {"YAH", 10500L, 6d}
                },
                new[] {
                    new object[] {"IBM", 150L, 48d}, new object[] {"MSFT", 5000L, null},
                    new object[] {"YAH", 10000L, 6d}
                });

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

            string[] fields = {"Symbol", "Volume", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsert(
                2200,
                0,
                new[] {new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsert(
                3200,
                0,
                new[] {new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsert(
                4200,
                0,
                new[] {new object[] {"IBM", 155L, 75d}});
            expected.AddResultInsert(
                5200,
                0,
                new[] {new object[] {"IBM", 150L, 97d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {"IBM", 150L, 72d}},
                new[] {new object[] {"IBM", 100L, 72d}});
            expected.AddResultInsRem(7200, 0, null, null);

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

            string[] fields = {"Symbol", "Volume", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                200,
                1,
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(
                800,
                1,
                new[] {new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(
                1500,
                1,
                new[] {new object[] {"IBM", 150L, 49d}});
            expected.AddResultInsert(
                1500,
                2,
                new[] {new object[] {"YAH", 10000L, 1d}});
            expected.AddResultInsert(
                3500,
                1,
                new[] {new object[] {"YAH", 11000L, 3d}});
            expected.AddResultInsert(
                4300,
                1,
                new[] {new object[] {"IBM", 150L, 97d}});
            expected.AddResultInsert(
                4900,
                1,
                new[] {new object[] {"YAH", 11500L, 6d}});
            expected.AddResultInsert(
                5700,
                0,
                new[] {new object[] {"IBM", 100L, 72d}});
            expected.AddResultInsert(
                5900,
                1,
                new[] {new object[] {"YAH", 10500L, 7d}});
            expected.AddResultInsert(
                6300,
                0,
                new[] {new object[] {"MSFT", 5000L, null}});
            expected.AddResultInsert(
                7000,
                0,
                new[] {new object[] {"IBM", 150L, 48d}, new object[] {"YAH", 10000L, 6d}});

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

            string[] fields = {"Symbol", "Volume", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new[] {new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(
                2200,
                0,
                new[] {
                    new object[] {"IBM", 100L, 75d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 75d},
                    new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 75d}
                });
            expected.AddResultInsert(
                3200,
                0,
                new[] {
                    new object[] {"IBM", 100L, 75d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 75d},
                    new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 75d}
                });
            expected.AddResultInsert(
                4200,
                0,
                new[] {
                    new object[] {"IBM", 100L, 75d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 75d},
                    new object[] {"YAH", 10000L, 3d}, new object[] {"IBM", 155L, 75d}, new object[] {"YAH", 11000L, 3d}
                });
            expected.AddResultInsert(
                5200,
                0,
                new[] {
                    new object[] {"IBM", 100L, 97d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 97d},
                    new object[] {"YAH", 10000L, 6d}, new object[] {"IBM", 155L, 97d}, new object[] {"YAH", 11000L, 6d},
                    new object[] {"IBM", 150L, 97d}, new object[] {"YAH", 11500L, 6d}
                });
            expected.AddResultInsert(
                6200,
                0,
                new[] {
                    new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 72d}, new object[] {"YAH", 10000L, 7d},
                    new object[] {"IBM", 155L, 72d}, new object[] {"YAH", 11000L, 7d}, new object[] {"IBM", 150L, 72d},
                    new object[] {"YAH", 11500L, 7d}, new object[] {"YAH", 10500L, 7d}
                });
            expected.AddResultInsert(
                7200,
                0,
                new[] {
                    new object[] {"IBM", 155L, 48d}, new object[] {"YAH", 11000L, 6d}, new object[] {"IBM", 150L, 48d},
                    new object[] {"YAH", 11500L, 6d}, new object[] {"YAH", 10500L, 6d}
                });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void RunAssertion15LastHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select Symbol, Volume, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec)" +
                           "group by Symbol " +
                           "having sum(Price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last");
        }

        private static void RunAssertion12AllHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select Symbol, Volume, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=Symbol " +
                           "group by Symbol " +
                           "having sum(Price) > 50 " +
                           "output all every 1 seconds";
            TryAssertion11_12(env, stmtText, "all");
        }

        private static void RunAssertion6LastHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select Symbol, Volume, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=Symbol " +
                           "group by Symbol " +
                           "having sum(Price) > 50 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last");
        }

        private static void RunAssertion11AllHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select Symbol, Volume, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "group by Symbol " +
                           "having sum(Price) > 50 " +
                           "output all every 1 seconds";
            TryAssertion11_12(env, stmtText, "all");
        }

        private static void TryAssertionNoJoinLast(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            // Every event generates a new row, this time we sum the price by symbol and output volume
            var epl = opt.GetHint() +
                      "@Name('s0') select Symbol, Volume, sum(Price) as mySum " +
                      "from SupportMarketDataBean#length(5) " +
                      "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                      "group by Symbol " +
                      "output last every 2 events";

            env.CompileDeploy(epl).AddListener("s0");

            TryAssertionLast(env);

            env.UndeployAll();
        }

        private static void AssertEvent(
            RegressionEnvironment env,
            string symbol,
            double? mySum,
            long? volume)
        {
            var newData = env.Listener("s0").LastNewData;

            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbol, newData[0].Get("Symbol"));
            Assert.AreEqual(mySum, newData[0].Get("mySum"));
            Assert.AreEqual(volume, newData[0].Get("Volume"));

            env.Listener("s0").Reset();
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void TryAssertionSingle(RegressionEnvironment env)
        {
            // assert select result type
            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("mySum"));
            Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("Volume"));

            SendEvent(env, SYMBOL_DELL, 10, 100);
            Assert.IsTrue(env.Listener("s0").IsInvoked);
            AssertEvent(env, SYMBOL_DELL, 100d, 10L);

            SendEvent(env, SYMBOL_IBM, 15, 50);
            AssertEvent(env, SYMBOL_IBM, 50d, 15L);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            long volume,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            env.SendEventBean(bean);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void SendBeanEvent(
            RegressionEnvironment env,
            string theString,
            long longPrimitive,
            int intPrimitive)
        {
            var b = new SupportBean();
            b.TheString = theString;
            b.LongPrimitive = longPrimitive;
            b.IntPrimitive = intPrimitive;
            env.SendEventBean(b);
        }

        private static void SendMDEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        internal class ResultSetUnaggregatedOutputFirst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var fields = new [] { "TheString","IntPrimitive" };
                var epl = "@Name('s0') select * from SupportBean\n" +
                          "     group by TheString\n" +
                          "     output first every 10 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E2", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 3});

                env.Milestone(1);

                SendTimer(env, 5000);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E3", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 4});

                env.SendEventBean(new SupportBean("E2", 5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 10000);

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E3", 6));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E1", 7));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 7});

                env.SendEventBean(new SupportBean("E1", 8));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E2", 9));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 9});

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 11));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputFirstHavingJoinNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var stmtText =
                    "@Name('s0') select TheString, LongPrimitive, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
                TryOutputFirstHaving(env, stmtText, milestone);

                var stmtTextJoin =
                    "@Name('s0') select TheString, LongPrimitive, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.Id = mv.TheString " +
                    "group by TheString having sum(IntPrimitive) > 20 output first every 2 events";
                TryOutputFirstHaving(env, stmtTextJoin, milestone);

                var stmtTextOrder =
                    "@Name('s0') select TheString, LongPrimitive, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";
                TryOutputFirstHaving(env, stmtTextOrder, milestone);

                var stmtTextOrderJoin =
                    "@Name('s0') select TheString, LongPrimitive, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A#keepall a where a.Id = mv.TheString " +
                    "group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";
                TryOutputFirstHaving(env, stmtTextOrderJoin, milestone);
            }
        }

        internal class ResultSet1NoneNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec)" +
                               "group by Symbol";
                TryAssertion12(env, stmtText, "none");
            }
        }

        internal class ResultSet2NoneNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "group by Symbol";
                TryAssertion12(env, stmtText, "none");
            }
        }

        internal class ResultSet3NoneHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
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
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
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
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "group by Symbol " +
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default");
            }
        }

        internal class ResultSet6DefaultNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol " +
                               "group by Symbol " +
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default");
            }
        }

        internal class ResultSet7DefaultHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
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
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
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
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
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
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
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
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
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
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
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
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
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
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
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
                var stmtText = "@Name('s0') select Symbol, Volume, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "group by Symbol " +
                               "output snapshot every 1 seconds";
                TryAssertion18(env, stmtText, "snapshot");
            }
        }

        internal class ResultSetHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@Name('s0') select irstream Symbol, Volume, sum(Price) as sumPrice" +
                          " from SupportMarketDataBean#time(10 sec) " +
                          "group by Symbol " +
                          "having sum(Price) >= 10 " +
                          "output every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionHavingDefault(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@Name('s0') select irstream Symbol, Volume, sum(Price) as sumPrice" +
                          " from SupportMarketDataBean#time(10 sec) as S0," +
                          "SupportBean#keepall as S1 " +
                          "where S0.Symbol = S1.TheString " +
                          "group by Symbol " +
                          "having sum(Price) >= 10 " +
                          "output every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("IBM", 0));

                TryAssertionHavingDefault(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetJoinSortWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@Name('s0') select irstream Symbol, Volume, max(Price) as maxVol" +
                          " from SupportMarketDataBean#sort(1, Volume) as S0," +
                          "SupportBean#keepall as S1 where S1.TheString = S0.Symbol " +
                          "group by Symbol output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("JOIN_KEY", -1));

                SendEvent(env, "JOIN_KEY", 1d);
                SendEvent(env, "JOIN_KEY", 2d);
                env.Listener("s0").Reset();

                // moves all events out of the window,
                SendTimer(env, 1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
                var result = env.Listener("s0").DataListsFlattened;
                Assert.AreEqual(2, result.First.Length);
                Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
                Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
                Assert.AreEqual(1, result.Second.Length);
                Assert.AreEqual(2.0, result.Second[0].Get("maxVol"));

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt =
                    "@Name('s0') select Symbol, Volume, sum(Price) as sumPrice from SupportMarketDataBean" +
                    "#time(10 seconds) group by Symbol output snapshot every 1 seconds";
                env.CompileDeploy(selectStmt).AddListener("s0");

                SendEvent(env, "s0", 1, 20);

                SendTimer(env, 500);
                SendEvent(env, "IBM", 2, 16);
                SendEvent(env, "s0", 3, 14);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 1000);
                string[] fields = {"Symbol", "Volume", "sumPrice"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"s0", 1L, 34d}, new object[] {"IBM", 2L, 16d}, new object[] {"s0", 3L, 34d}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 1500);
                SendEvent(env, "MSFT", 4, 18);
                SendEvent(env, "IBM", 5, 30);

                SendTimer(env, 10000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"s0", 1L, 34d}, new object[] {"IBM", 2L, 46d}, new object[] {"s0", 3L, 34d},
                        new object[] {"MSFT", 4L, 18d}, new object[] {"IBM", 5L, 46d}
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 11000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"MSFT", 4L, 18d}, new object[] {"IBM", 5L, 30d}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 12000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 13000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshotJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt =
                    "@Name('s0') select Symbol, Volume, sum(Price) as sumPrice from SupportMarketDataBean" +
                    "#time(10 seconds) as m, SupportBean" +
                    "#keepall as s where s.TheString = m.Symbol group by Symbol output snapshot every 1 seconds order by Symbol, Volume asc";
                env.CompileDeploy(selectStmt).AddListener("s0");

                env.SendEventBean(new SupportBean("ABC", 1));
                env.SendEventBean(new SupportBean("IBM", 2));
                env.SendEventBean(new SupportBean("MSFT", 3));

                SendEvent(env, "ABC", 1, 20);

                SendTimer(env, 500);
                SendEvent(env, "IBM", 2, 16);
                SendEvent(env, "ABC", 3, 14);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 1000);
                string[] fields = {"Symbol", "Volume", "sumPrice"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"ABC", 1L, 34d}, new object[] {"ABC", 3L, 34d}, new object[] {"IBM", 2L, 16d}
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 1500);
                SendEvent(env, "MSFT", 4, 18);
                SendEvent(env, "IBM", 5, 30);

                SendTimer(env, 10000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"ABC", 1L, 34d}, new object[] {"ABC", 3L, 34d}, new object[] {"IBM", 2L, 46d},
                        new object[] {"IBM", 5L, 46d}, new object[] {"MSFT", 4L, 18d}
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 10500);
                SendTimer(env, 11000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"IBM", 5L, 30d}, new object[] {"MSFT", 4L, 18d}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 11500);
                SendTimer(env, 12000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 13000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                Assert.IsNull(env.Listener("s0").LastNewData);
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ResultSetMaxTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@Name('s0') select irstream Symbol, " +
                          "Volume, max(Price) as maxVol" +
                          " from SupportMarketDataBean#time(1 sec) " +
                          "group by Symbol output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "SYM1", 1d);
                SendEvent(env, "SYM1", 2d);
                env.Listener("s0").Reset();

                // moves all events out of the window,
                SendTimer(env, 1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
                var result = env.Listener("s0").DataListsFlattened;
                Assert.AreEqual(2, result.First.Length);
                Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
                Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
                Assert.AreEqual(2, result.Second.Length);
                Assert.AreEqual(null, result.Second[0].Get("maxVol"));
                Assert.AreEqual(null, result.Second[1].Get("maxVol"));

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
        }

        internal class ResultSetNoOutputClauseView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, Volume, sum(Price) as mySum " +
                          "from SupportMarketDataBean#length(5) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol ";

                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionSingle(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetNoJoinDefault : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@Name('s0') select Symbol, Volume, sum(Price) as mySum " +
                          "from SupportMarketDataBean#length(5) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol " +
                          "output every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionDefault(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetJoinDefault : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@Name('s0') select Symbol, Volume, sum(Price) as mySum " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(5) as two " +
                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                          "  and one.TheString = two.Symbol " +
                          "group by Symbol " +
                          "output every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));

                TryAssertionDefault(env);

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
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = opt.GetHint() +
                          "@Name('s0') select Symbol, Volume, sum(Price) as mySum " +
                          "from SupportMarketDataBean#length(5) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol " +
                          "output all every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionAll(env);

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

            private static void TryAssertionJoinAll(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = opt.GetHint() +
                          "@Name('s0') select Symbol, Volume, sum(Price) as mySum " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(5) as two " +
                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                          "  and one.TheString = two.Symbol " +
                          "group by Symbol " +
                          "output all every 2 events";

                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));

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
        }
    }
} // end of namespace