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
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitSimple
    {
        private const string JOIN_KEY = "KEY";
        private const string CATEGORY = "Un-aggregated and Un-grouped";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
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
            execs.Add(new ResultSet17FirstNoHavingNoJoinIStream());
            execs.Add(new ResultSet17FirstNoHavingJoinIStream());
            execs.Add(new ResultSet17FirstNoHavingNoJoinIRStream());
            execs.Add(new ResultSet17FirstNoHavingJoinIRStream());
            execs.Add(new ResultSet18SnapshotNoHavingNoJoin());
            execs.Add(new ResultSetOutputEveryTimePeriod());
            execs.Add(new ResultSetOutputEveryTimePeriodVariable());
            execs.Add(new ResultSetAggAllHaving());
            execs.Add(new ResultSetAggAllHavingJoin());
            execs.Add(new ResultSetIterator());
            execs.Add(new ResultSetLimitEventJoin());
            execs.Add(new ResultSetLimitTime());
            execs.Add(new ResultSetTimeBatchOutputEvents());
            execs.Add(new ResultSetSimpleNoJoinAll());
            execs.Add(new ResultSetSimpleNoJoinLast());
            execs.Add(new ResultSetSimpleJoinAll());
            execs.Add(new ResultSetSimpleJoinLast());
            execs.Add(new ResultSetLimitEventSimple());
            execs.Add(new ResultSetLimitSnapshot());
            execs.Add(new ResultSetFirstSimpleHavingAndNoHaving());
            execs.Add(new ResultSetLimitSnapshotJoin());
            execs.Add(new ResultSetSnapshotMonthScoped());
            execs.Add(new ResultSetFirstMonthScoped());
            execs.Add(new ResultSetOutputFirstUnidirectionalJoinNamedWindow());
            return execs;
        }

        private static void RunAssertion9AllNoHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select symbol, volume, price " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "output all every 1 seconds";
            TryAssertion56(env, stmtText, "all");
        }

        private static void RunAssertion10AllNoHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select symbol, volume, price " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where theString=symbol " +
                           "output all every 1 seconds";
            TryAssertion56(env, stmtText, "all");
        }

        private static void RunAssertion11AllHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select symbol, volume, price " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "having price > 10" +
                           "output all every 1 seconds";
            TryAssertion78(env, stmtText, "all");
        }

        private static void RunAssertion12AllHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select symbol, volume, price " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where theString=symbol " +
                           "having price > 10" +
                           "output all every 1 seconds";
            TryAssertion78(env, stmtText, "all");
        }

        private static void CreateStmtAndListenerJoin(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy(epl).AddListener("s0");
            env.SendEventBean(new SupportBeanString(JOIN_KEY));
        }

        private static void TryAssertLast(RegressionEnvironment env)
        {
            // send an event
            SendEvent(env, 1);

            // check no update
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            // send another event
            SendEvent(env, 2);

            // check update, only the last event present
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
            Assert.AreEqual(2L, env.Listener("s0").LastNewData[0].Get("LongBoxed"));
            Assert.IsNull(env.Listener("s0").LastOldData);

            env.UndeployAll();
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long time)
        {
            env.AdvanceTime(time);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string s)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.DoubleBoxed = 0.0;
            bean.IntPrimitive = 0;
            bean.IntBoxed = 0;
            env.SendEventBean(bean);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            long longBoxed)
        {
            SendEvent(env, longBoxed, 0, 0);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            long longBoxed,
            int intBoxed,
            short shortBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            env.SendEventBean(bean);
        }

        private static void TimeCallback(
            RegressionEnvironment env,
            string epl,
            int timeToCallback)
        {
            // set the clock to 0
            var currentTime = new AtomicLong();
            SendTimeEvent(env, 0, currentTime);

            // create the EPL statement and add a listener
            env.CompileDeploy(epl).AddListener("s0");

            // send an event
            SendEvent(env, "IBM");

            // check that the listener hasn't been updated
            SendTimeEvent(env, timeToCallback - 1, currentTime);
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            // update the clock
            SendTimeEvent(env, timeToCallback, currentTime);

            // check that the listener has been updated
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
            Assert.IsNull(env.Listener("s0").LastOldData);

            // send another event
            SendEvent(env, "MSFT");

            // check that the listener hasn't been updated
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            // update the clock
            SendTimeEvent(env, timeToCallback, currentTime);

            // check that the listener has been updated
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
            Assert.IsNull(env.Listener("s0").LastOldData);

            // don't send an event
            // check that the listener hasn't been updated
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            // update the clock
            SendTimeEvent(env, timeToCallback, currentTime);

            // check that the listener has been updated
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            Assert.IsNull(env.Listener("s0").LastNewData);
            Assert.IsNull(env.Listener("s0").LastOldData);

            // don't send an event
            // check that the listener hasn't been updated
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            // update the clock
            SendTimeEvent(env, timeToCallback, currentTime);

            // check that the listener has been updated
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            Assert.IsNull(env.Listener("s0").LastNewData);
            Assert.IsNull(env.Listener("s0").LastOldData);

            // send several events
            SendEvent(env, "YAH");
            SendEvent(env, "s4");
            SendEvent(env, "s5");

            // check that the listener hasn't been updated
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            // update the clock
            SendTimeEvent(env, timeToCallback, currentTime);

            // check that the listener has been updated
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            Assert.AreEqual(3, env.Listener("s0").LastNewData.Length);
            Assert.IsNull(env.Listener("s0").LastOldData);

            env.UndeployAll();
        }

        private static void TryAssertionSimpleJoinAll(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var epl = opt.GetHint() +
                      "@Name('s0') select longBoxed  " +
                      "from SupportBeanString#length(3) as one, " +
                      "SupportBean#length(3) as two " +
                      "output all every 2 events";

            CreateStmtAndListenerJoin(env, epl);
            TryAssertAll(env);

            env.UndeployAll();
        }

        private static void TryAssertAll(RegressionEnvironment env)
        {
            // send an event
            SendEvent(env, 1);

            // check no update
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            // send another event
            SendEvent(env, 2);

            // check update, all events present
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            Assert.AreEqual(2, env.Listener("s0").LastNewData.Length);
            Assert.AreEqual(1L, env.Listener("s0").LastNewData[0].Get("LongBoxed"));
            Assert.AreEqual(2L, env.Listener("s0").LastNewData[1].Get("LongBoxed"));
            Assert.IsNull(env.Listener("s0").LastOldData);

            env.UndeployAll();
        }

        private static void TryAssertion34(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"symbol", "volume", "price"};

            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                200,
                1,
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(
                1500,
                1,
                new[] {new object[] {"IBM", 150L, 24d}});
            expected.AddResultInsert(
                2100,
                1,
                new[] {new object[] {"IBM", 155L, 26d}});
            expected.AddResultInsert(
                4300,
                1,
                new[] {new object[] {"IBM", 150L, 22d}});
            expected.AddResultRemove(
                5700,
                0,
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultRemove(
                7000,
                0,
                new[] {new object[] {"IBM", 150L, 24d}});

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

            string[] fields = {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);

            expected.AddResultInsert(
                1200,
                0,
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(
                2200,
                0,
                new[] {new object[] {"IBM", 155L, 26d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(
                5200,
                0,
                new[] {new object[] {"IBM", 150L, 22d}});
            expected.AddResultInsRem(
                6200,
                0,
                null,
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultRemove(
                7200,
                0,
                new[] {new object[] {"IBM", 150L, 24d}});

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void TryAssertion12(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"symbol", "volume", "price"};
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
                new[] {new object[] {"IBM", 150L, 24d}});
            expected.AddResultInsert(
                1500,
                2,
                new[] {new object[] {"YAH", 10000L, 1d}});
            expected.AddResultInsert(
                2100,
                1,
                new[] {new object[] {"IBM", 155L, 26d}});
            expected.AddResultInsert(
                3500,
                1,
                new[] {new object[] {"YAH", 11000L, 2d}});
            expected.AddResultInsert(
                4300,
                1,
                new[] {new object[] {"IBM", 150L, 22d}});
            expected.AddResultInsert(
                4900,
                1,
                new[] {new object[] {"YAH", 11500L, 3d}});
            expected.AddResultRemove(
                5700,
                0,
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(
                5900,
                1,
                new[] {new object[] {"YAH", 10500L, 1d}});
            expected.AddResultRemove(
                6300,
                0,
                new[] {new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultRemove(
                7000,
                0,
                new[] {new object[] {"IBM", 150L, 24d}, new object[] {"YAH", 10000L, 1d}});

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

            string[] fields = {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new[] {new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(
                2200,
                0,
                new[] {new object[] {"IBM", 155L, 26d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(
                4200,
                0,
                new[] {new object[] {"YAH", 11000L, 2d}});
            expected.AddResultInsert(
                5200,
                0,
                new[] {new object[] {"YAH", 11500L, 3d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {"YAH", 10500L, 1d}},
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultRemove(
                7200,
                0,
                new[] {new object[] {"YAH", 10000L, 1d}});

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

            string[] fields = {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(
                2200,
                0,
                new[] {new object[] {"IBM", 150L, 24d}, new object[] {"IBM", 155L, 26d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(
                5200,
                0,
                new[] {new object[] {"IBM", 150L, 22d}});
            expected.AddResultInsRem(
                6200,
                0,
                null,
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultRemove(
                7200,
                0,
                new[] {new object[] {"IBM", 150L, 24d}});

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

            string[] fields = {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new[] {new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(
                2200,
                0,
                new[] {
                    new object[] {"IBM", 150L, 24d}, new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 26d}
                });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(
                4200,
                0,
                new[] {new object[] {"YAH", 11000L, 2d}});
            expected.AddResultInsert(
                5200,
                0,
                new[] {new object[] {"IBM", 150L, 22d}, new object[] {"YAH", 11500L, 3d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {"YAH", 10500L, 1d}},
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultRemove(
                7200,
                0,
                new[] {
                    new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 24d}, new object[] {"YAH", 10000L, 1d}
                });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void TryAssertion17IStream(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                200,
                1,
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(
                1500,
                1,
                new[] {new object[] {"IBM", 150L, 24d}});
            expected.AddResultInsert(
                3500,
                1,
                new[] {new object[] {"YAH", 11000L, 2d}});
            expected.AddResultInsert(
                4300,
                1,
                new[] {new object[] {"IBM", 150L, 22d}});
            expected.AddResultInsert(
                5900,
                1,
                new[] {new object[] {"YAH", 10500L, 1.0d}});

            var execution = new ResultAssertExecution(
                stmtText,
                env,
                expected,
                ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
            execution.Execute(false);
        }

        private static void TryAssertion17IRStream(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                200,
                1,
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultInsert(
                1500,
                1,
                new[] {new object[] {"IBM", 150L, 24d}});
            expected.AddResultInsert(
                3500,
                1,
                new[] {new object[] {"YAH", 11000L, 2d}});
            expected.AddResultInsert(
                4300,
                1,
                new[] {new object[] {"IBM", 150L, 22d}});
            expected.AddResultRemove(
                5700,
                0,
                new[] {new object[] {"IBM", 100L, 25d}});
            expected.AddResultRemove(
                6300,
                0,
                new[] {new object[] {"MSFT", 5000L, 9d}});

            var execution = new ResultAssertExecution(
                stmtText,
                env,
                expected,
                ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
            execution.Execute(false);
        }

        private static void TryAssertion18(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"symbol", "volume", "price"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new[] {new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}});
            expected.AddResultInsert(
                2200,
                0,
                new[] {
                    new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 24d},
                    new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 26d}
                });
            expected.AddResultInsert(
                3200,
                0,
                new[] {
                    new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 24d},
                    new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 26d}
                });
            expected.AddResultInsert(
                4200,
                0,
                new[] {
                    new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 24d},
                    new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 26d}, new object[] {"YAH", 11000L, 2d}
                });
            expected.AddResultInsert(
                5200,
                0,
                new[] {
                    new object[] {"IBM", 100L, 25d}, new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 24d},
                    new object[] {"YAH", 10000L, 1d}, new object[] {"IBM", 155L, 26d}, new object[] {"YAH", 11000L, 2d},
                    new object[] {"IBM", 150L, 22d}, new object[] {"YAH", 11500L, 3d}
                });
            expected.AddResultInsert(
                6200,
                0,
                new[] {
                    new object[] {"MSFT", 5000L, 9d}, new object[] {"IBM", 150L, 24d}, new object[] {"YAH", 10000L, 1d},
                    new object[] {"IBM", 155L, 26d}, new object[] {"YAH", 11000L, 2d}, new object[] {"IBM", 150L, 22d},
                    new object[] {"YAH", 11500L, 3d}, new object[] {"YAH", 10500L, 1d}
                });
            expected.AddResultInsert(
                7200,
                0,
                new[] {
                    new object[] {"IBM", 155L, 26d}, new object[] {"YAH", 11000L, 2d}, new object[] {"IBM", 150L, 22d},
                    new object[] {"YAH", 11500L, 3d}, new object[] {"YAH", 10500L, 1d}
                });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void SendTimeEvent(
            RegressionEnvironment env,
            int timeIncrement,
            AtomicLong currentTime)
        {
            currentTime.IncrementAndGet(timeIncrement);
            env.AdvanceTime(currentTime.Get());
        }

        private static void SendJoinEvents(
            RegressionEnvironment env,
            string s)
        {
            var event1 = new SupportBean();
            event1.TheString = s;
            event1.DoubleBoxed = 0.0;
            event1.IntPrimitive = 0;
            event1.IntBoxed = 0;

            var event2 = new SupportBean_A(s);

            env.SendEventBean(event1);
            env.SendEventBean(event2);
        }

        private static void SendMDEvent(
            RegressionEnvironment env,
            string symbol,
            long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, null);
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

        private static void SendCurrentTime(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void SendCurrentTimeWithMinus(
            RegressionEnvironment env,
            string time,
            long minus)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time) - minus);
        }

        internal class ResultSet1NoneNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec)";
                TryAssertion12(env, stmtText, "none");
            }
        }

        internal class ResultSet2NoneNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol";
                TryAssertion12(env, stmtText, "none");
            }
        }

        internal class ResultSet3NoneHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               " having price > 10";
                TryAssertion34(env, stmtText, "none");
            }
        }

        internal class ResultSet4NoneHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               " having price > 10";
                TryAssertion34(env, stmtText, "none");
            }
        }

        internal class ResultSet5DefaultNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default");
            }
        }

        internal class ResultSet6DefaultNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default");
            }
        }

        internal class ResultSet7DefaultHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec) \n" +
                               "having price > 10" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default");
            }
        }

        internal class ResultSet8DefaultHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "having price > 10" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default");
            }
        }

        internal class ResultSet9AllNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion9AllNoHavingNoJoin(env, outputLimitOpt);
                }
            }
        }

        internal class ResultSet10AllNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion10AllNoHavingJoin(env, outputLimitOpt);
                }
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
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec)" +
                               "output last every 1 seconds";
                TryAssertion13_14(env, stmtText, "last");
            }
        }

        internal class ResultSet14LastNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "output last every 1 seconds";
                TryAssertion13_14(env, stmtText, "last");
            }
        }

        internal class ResultSet15LastHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec)" +
                               "having price > 10 " +
                               "output last every 1 seconds";
                TryAssertion15_16(env, stmtText, "last");
            }
        }

        internal class ResultSet16LastHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "having price > 10 " +
                               "output last every 1 seconds";
                TryAssertion15_16(env, stmtText, "last");
            }
        }

        internal class ResultSet17FirstNoHavingNoJoinIStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output first every 1 seconds";
                TryAssertion17IStream(env, stmtText, "first");
            }
        }

        internal class ResultSet17FirstNoHavingJoinIStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec)," +
                               "SupportBean#keepall where theString=symbol " +
                               "output first every 1 seconds";
                TryAssertion17IStream(env, stmtText, "first");
            }
        }

        internal class ResultSet17FirstNoHavingNoJoinIRStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select irstream symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output first every 1 seconds";
                TryAssertion17IRStream(env, stmtText, "first");
            }
        }

        internal class ResultSet17FirstNoHavingJoinIRStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select irstream symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "output first every 1 seconds";
                TryAssertion17IRStream(env, stmtText, "first");
            }
        }

        internal class ResultSet18SnapshotNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume, price " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output snapshot every 1 seconds";
                TryAssertion18(env, stmtText, "first");
            }
        }

        internal class ResultSetOutputFirstUnidirectionalJoinNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var fields = "c0,c1".SplitCsv();
                var epl =
                    "create window MyWindow#keepall as SupportBean_S0;\n" +
                    "insert into MyWindow select * from SupportBean_S0;\n" +
                    "@Name('s0') select myWindow.id as c0, s1.id as c1\n" +
                    "from SupportBean_S1 as s1 unidirectional, MyWindow as myWindow\n" +
                    "where myWindow.p00 = s1.p10\n" +
                    "output first every 1 minutes;";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(10, "a"));
                env.SendEventBean(new SupportBean_S0(20, "b"));
                env.SendEventBean(new SupportBean_S1(1000, "b"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {20, 1000});

                env.SendEventBean(new SupportBean_S1(1001, "b"));
                env.SendEventBean(new SupportBean_S1(1002, "a"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(60 * 1000);
                env.SendEventBean(new SupportBean_S1(1003, "a"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, 1003});

                env.SendEventBean(new SupportBean_S1(1004, "a"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(120 * 1000);
                env.SendEventBean(new SupportBean_S1(1005, "a"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, 1005});

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputEveryTimePeriod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(2000);

                var stmtText =
                    "@Name('s0') select symbol from SupportMarketDataBean#keepall output snapshot every 1 day 2 hours 3 minutes 4 seconds 5 milliseconds";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendMDEvent(env, "E1", 0);

                long deltaSec = 26 * 60 * 60 + 3 * 60 + 4;
                var deltaMSec = deltaSec * 1000 + 5 + 2000;
                env.AdvanceTime(deltaMSec - 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(deltaMSec);
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("symbol"));

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputEveryTimePeriodVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(2000);

                var stmtText =
                    "@Name('s0') select symbol from SupportMarketDataBean#keepall output snapshot every D days H hours M minutes S seconds MS milliseconds";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendMDEvent(env, "E1", 0);

                long deltaSec = 26 * 60 * 60 + 3 * 60 + 4;
                var deltaMSec = deltaSec * 1000 + 5 + 2000;
                env.AdvanceTime(deltaMSec - 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(deltaMSec);
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("symbol"));

                // test statement model
                var model = env.EplToModel(stmtText);
                Assert.AreEqual(stmtText, model.ToEPL());

                env.UndeployAll();
            }
        }

        internal class ResultSetAggAllHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume " +
                               "from SupportMarketDataBean#length(10) as two " +
                               "having volume > 0 " +
                               "output every 5 events";
                env.CompileDeploy(stmtText).AddListener("s0");

                string[] fields = {"symbol", "volume"};

                SendMDEvent(env, "S0", 20);
                SendMDEvent(env, "IBM", -1);
                SendMDEvent(env, "MSFT", -2);
                SendMDEvent(env, "YAH", 10);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendMDEvent(env, "IBM", 0);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"S0", 20L}, new object[] {"YAH", 10L}});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggAllHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select symbol, volume " +
                               "from SupportMarketDataBean#length(10) as one," +
                               "SupportBean#length(10) as two " +
                               "where one.symbol=two.TheString " +
                               "having volume > 0 " +
                               "output every 5 events";
                env.CompileDeploy(stmtText).AddListener("s0");

                string[] fields = {"symbol", "volume"};
                env.SendEventBean(new SupportBean("S0", 0));
                env.SendEventBean(new SupportBean("IBM", 0));
                env.SendEventBean(new SupportBean("MSFT", 0));
                env.SendEventBean(new SupportBean("YAH", 0));

                SendMDEvent(env, "S0", 20);
                SendMDEvent(env, "IBM", -1);
                SendMDEvent(env, "MSFT", -2);
                SendMDEvent(env, "YAH", 10);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendMDEvent(env, "IBM", 0);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"S0", 20L}, new object[] {"YAH", 10L}});

                env.UndeployAll();
            }
        }

        internal class ResultSetIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"symbol", "price"};
                var epl = "@Name('s0') select symbol, theString, price from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.symbol = two.TheString " +
                          "output every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString("CAT"));
                env.SendEventBean(new SupportBeanString("IBM"));

                // Output limit clause ignored when iterating, for both joins and no-join
                SendEvent(env, "CAT", 50);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"CAT", 50d}});

                SendEvent(env, "CAT", 60);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"CAT", 50d}, new object[] {"CAT", 60d}});

                SendEvent(env, "IBM", 70);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"CAT", 50d}, new object[] {"CAT", 60d}, new object[] {"IBM", 70d}});

                SendEvent(env, "IBM", 90);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"CAT", 50d}, new object[] {"CAT", 60d}, new object[] {"IBM", 70d},
                        new object[] {"IBM", 90d}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitEventJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement =
                    "select * from SupportBean#length(5) as event1," +
                    "SupportBean_A#length(5) as event2" +
                    " where event1.TheString = event2.id";
                var outputStmt1 = joinStatement + " output every 1 events";
                var outputStmt3 = joinStatement + " output every 3 events";

                env.CompileDeploy("@Name('s1') " + outputStmt1).AddListener("s1");
                env.CompileDeploy("@Name('s3') " + outputStmt3).AddListener("s3");

                // send event 1
                SendJoinEvents(env, "IBM");

                Assert.IsTrue(env.Listener("s1").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("s1").LastNewData.Length);
                Assert.IsNull(env.Listener("s1").LastOldData);

                Assert.IsFalse(env.Listener("s3").GetAndClearIsInvoked());
                Assert.IsNull(env.Listener("s3").LastNewData);
                Assert.IsNull(env.Listener("s3").LastOldData);

                // send event 2
                SendJoinEvents(env, "MSFT");

                Assert.IsTrue(env.Listener("s1").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("s1").LastNewData.Length);
                Assert.IsNull(env.Listener("s1").LastOldData);

                Assert.IsFalse(env.Listener("s3").GetAndClearIsInvoked());
                Assert.IsNull(env.Listener("s3").LastNewData);
                Assert.IsNull(env.Listener("s3").LastOldData);

                // send event 3
                SendJoinEvents(env, "YAH");

                Assert.IsTrue(env.Listener("s1").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("s1").LastNewData.Length);
                Assert.IsNull(env.Listener("s1").LastOldData);

                Assert.IsTrue(env.Listener("s3").GetAndClearIsInvoked());
                Assert.AreEqual(3, env.Listener("s3").LastNewData.Length);
                Assert.IsNull(env.Listener("s3").LastOldData);

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitTime : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var selectStatement = "@Name('s0') select * from SupportBean#length(5)";

                // test integer seconds
                var statementString1 = selectStatement +
                                       " output every 3 seconds";
                TimeCallback(env, statementString1, 3000);

                // test fractional seconds
                var statementString2 = selectStatement +
                                       " output every 3.3 seconds";
                TimeCallback(env, statementString2, 3300);

                // test integer minutes
                var statementString3 = selectStatement +
                                       " output every 2 minutes";
                TimeCallback(env, statementString3, 120000);

                // test fractional minutes
                var statementString4 =
                    "@Name('s0') select * from SupportBean#length(5) output every .05 minutes";
                TimeCallback(env, statementString4, 3000);
            }
        }

        internal class ResultSetTimeBatchOutputEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select * from SupportBean#time_batch(10 seconds) output every 10 seconds";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendTimer(env, 0);
                SendTimer(env, 10000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 20000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "e1");
                SendTimer(env, 30000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 40000);
                var newEvents = env.Listener("s0").GetAndResetLastNewData();
                Assert.AreEqual(1, newEvents.Length);
                Assert.AreEqual("e1", newEvents[0].Get("TheString"));
                env.Listener("s0").Reset();

                SendTimer(env, 50000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                env.Listener("s0").Reset();

                SendTimer(env, 60000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                env.Listener("s0").Reset();

                SendTimer(env, 70000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                env.Listener("s0").Reset();

                SendEvent(env, "e2");
                SendEvent(env, "e3");
                SendTimer(env, 80000);
                newEvents = env.Listener("s0").GetAndResetLastNewData();
                Assert.AreEqual(2, newEvents.Length);
                Assert.AreEqual("e2", newEvents[0].Get("TheString"));
                Assert.AreEqual("e3", newEvents[1].Get("TheString"));

                SendTimer(env, 90000);
                Assert.IsTrue(env.Listener("s0").IsInvoked);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ResultSetSimpleNoJoinAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionSimpleNoJoinAll(env, outputLimitOpt);
                }
            }

            private static void TryAssertionSimpleNoJoinAll(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                var epl = opt.GetHint() +
                          "@Name('s0') select longBoxed " +
                          "from SupportBean#length(3) " +
                          "output all every 2 events";

                env.CompileDeploy(epl).AddListener("s0");
                TryAssertAll(env);

                epl = opt.GetHint() +
                      "@Name('s0') select longBoxed " +
                      "from SupportBean#length(3) " +
                      "output every 2 events";

                env.CompileDeploy(epl).AddListener("s0");
                TryAssertAll(env);

                epl = opt.GetHint() +
                      "@Name('s0') select * " +
                      "from SupportBean#length(3) " +
                      "output every 2 events";

                env.CompileDeploy(epl).AddListener("s0");
                TryAssertAll(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetSimpleNoJoinLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select longBoxed " +
                          "from SupportBean#length(3) " +
                          "output last every 2 events";

                env.CompileDeploy(epl).AddListener("s0");
                TryAssertLast(env);

                epl = "@Name('s0') select * " +
                      "from SupportBean#length(3) " +
                      "output last every 2 events";

                env.CompileDeploy(epl).AddListener("s0");
                TryAssertLast(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetSimpleJoinAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionSimpleJoinAll(env, outputLimitOpt);
                }
            }
        }

        internal class ResultSetSimpleJoinLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select longBoxed " +
                          "from SupportBeanString#length(3) as one, " +
                          "SupportBean#length(3) as two " +
                          "output last every 2 events";

                CreateStmtAndListenerJoin(env, epl);
                TryAssertLast(env);
                env.UndeployAll();
            }
        }

        internal class ResultSetLimitEventSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var selectStmt = "select * from SupportBean#length(5)";
                var statement1 = "@Name('s0') " + selectStmt + " output every 1 events";
                var statement2 = "@Name('s1') " + selectStmt + " output every 2 events";
                var statement3 = "@Name('s2') " + selectStmt + " output every 3 events";

                env.CompileDeploy(statement1).AddListener("s0");
                env.CompileDeploy(statement2).AddListener("s1");
                env.CompileDeploy(statement3).AddListener("s2");

                // send event 1
                SendEvent(env, "IBM");

                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.IsNull(env.Listener("s0").LastOldData);

                Assert.IsFalse(env.Listener("s1").GetAndClearIsInvoked());
                Assert.IsNull(env.Listener("s1").LastNewData);
                Assert.IsNull(env.Listener("s1").LastOldData);

                Assert.IsFalse(env.Listener("s2").GetAndClearIsInvoked());
                Assert.IsNull(env.Listener("s2").LastNewData);
                Assert.IsNull(env.Listener("s2").LastOldData);

                // send event 2
                SendEvent(env, "MSFT");

                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.IsNull(env.Listener("s0").LastOldData);

                Assert.IsTrue(env.Listener("s1").GetAndClearIsInvoked());
                Assert.AreEqual(2, env.Listener("s1").LastNewData.Length);
                Assert.IsNull(env.Listener("s1").LastOldData);

                Assert.IsFalse(env.Listener("s2").GetAndClearIsInvoked());

                // send event 3
                SendEvent(env, "YAH");

                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
                Assert.IsNull(env.Listener("s0").LastOldData);

                Assert.IsFalse(env.Listener("s1").GetAndClearIsInvoked());

                Assert.IsTrue(env.Listener("s2").GetAndClearIsInvoked());
                Assert.AreEqual(3, env.Listener("s2").LastNewData.Length);
                Assert.IsNull(env.Listener("s2").LastOldData);

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt = "@Name('s0') select * from SupportBean#time(10) output snapshot every 3 events";
                env.CompileDeploy(selectStmt).AddListener("s0");

                SendTimer(env, 1000);
                SendEvent(env, "IBM");
                SendEvent(env, "MSFT");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 2000);
                SendEvent(env, "YAH");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"TheString"},
                    new[] {new object[] {"IBM"}, new object[] {"MSFT"}, new object[] {"YAH"}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 3000);
                SendEvent(env, "s4");
                SendEvent(env, "s5");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 10000);
                SendEvent(env, "s6");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"TheString"},
                    new[] {
                        new object[] {"IBM"}, new object[] {"MSFT"}, new object[] {"YAH"}, new object[] {"s4"},
                        new object[] {"s5"}, new object[] {"s6"}
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 11000);
                SendEvent(env, "s7");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "s8");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "s9");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"TheString"},
                    new[] {
                        new object[] {"YAH"}, new object[] {"s4"}, new object[] {"s5"}, new object[] {"s6"},
                        new object[] {"s7"}, new object[] {"s8"}, new object[] {"s9"}
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 14000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"TheString"},
                    new[] {new object[] {"s6"}, new object[] {"s7"}, new object[] {"s8"}, new object[] {"s9"}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendEvent(env, "s10");
                SendEvent(env, "s11");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 23000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"TheString"},
                    new[] {new object[] {"s10"}, new object[] {"s11"}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendEvent(env, "s12");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ResultSetFirstSimpleHavingAndNoHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionFirstSimpleHavingAndNoHaving(env, "");
                TryAssertionFirstSimpleHavingAndNoHaving(env, "having intPrimitive != 0");
            }

            private static void TryAssertionFirstSimpleHavingAndNoHaving(
                RegressionEnvironment env,
                string having)
            {
                var epl = "@Name('s0') select TheString from SupportBean " + having + " output first every 3 events";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "TheString".SplitCsv(),
                    new object[] {"E1"});

                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E4", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "TheString".SplitCsv(),
                    new object[] {"E4"});

                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshotJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt = "@Name('s0') select TheString from SupportBean#time(10) as s," +
                                 "SupportMarketDataBean#keepall as m where s.TheString = m.symbol output snapshot every 3 events order by symbol asc";
                env.CompileDeploy(selectStmt).AddListener("s0");

                foreach (var symbol in "s0,s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11".SplitCsv()) {
                    env.SendEventBean(new SupportMarketDataBean(symbol, 0, 0L, ""));
                }

                SendTimer(env, 1000);
                SendEvent(env, "s0");
                SendEvent(env, "s1");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 2000);
                SendEvent(env, "s2");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"TheString"},
                    new[] {new object[] {"s0"}, new object[] {"s1"}, new object[] {"s2"}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 3000);
                SendEvent(env, "s4");
                SendEvent(env, "s5");
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 10000);
                SendEvent(env, "s6");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"TheString"},
                    new[] {
                        new object[] {"s0"}, new object[] {"s1"}, new object[] {"s2"}, new object[] {"s4"},
                        new object[] {"s5"}, new object[] {"s6"}
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 11000);
                SendEvent(env, "s7");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "s8");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "s9");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"TheString"},
                    new[] {
                        new object[] {"s2"}, new object[] {"s4"}, new object[] {"s5"}, new object[] {"s6"},
                        new object[] {"s7"}, new object[] {"s8"}, new object[] {"s9"}
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 14000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"TheString"},
                    new[] {new object[] {"s6"}, new object[] {"s7"}, new object[] {"s8"}, new object[] {"s9"}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendEvent(env, "s10");
                SendEvent(env, "s11");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 23000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"TheString"},
                    new[] {new object[] {"s10"}, new object[] {"s11"}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendEvent(env, "s12");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ResultSetSnapshotMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");

                var epl = "@Name('s0') select * from SupportBean#lastevent output snapshot every 1 month";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    "TheString".SplitCsv(),
                    new[] {new object[] {"E1"}});

                env.UndeployAll();
            }
        }

        internal class ResultSetFirstMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");

                var epl = "@Name('s0') select * from SupportBean#lastevent output first every 1 month";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean("E2", 2));
                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                env.SendEventBean(new SupportBean("E3", 3));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                env.SendEventBean(new SupportBean("E4", 4));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    "TheString".SplitCsv(),
                    new[] {new object[] {"E4"}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace