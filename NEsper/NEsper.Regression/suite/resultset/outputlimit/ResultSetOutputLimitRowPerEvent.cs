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

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitRowPerEvent
    {
        private const string JOIN_KEY = "KEY";
        private const string CATEGORY = "Aggregated and Un-grouped";
        private static readonly string EVENT_NAME = typeof(SupportMarketDataBean).Name;

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
            execs.Add(new ResultSet11AllHavingNoJoinHinted());
            execs.Add(new ResultSet12AllHavingJoin());
            execs.Add(new ResultSet13LastNoHavingNoJoin());
            execs.Add(new ResultSet14LastNoHavingJoin());
            execs.Add(new ResultSet15LastHavingNoJoin());
            execs.Add(new ResultSet16LastHavingJoin());
            execs.Add(new ResultSet17FirstNoHavingNoJoinIStreamOnly());
            execs.Add(new ResultSet17FirstNoHavingNoJoinIRStream());
            execs.Add(new ResultSet18SnapshotNoHavingNoJoin());
            execs.Add(new ResultSetHaving());
            execs.Add(new ResultSetHavingJoin());
            execs.Add(new ResultSetMaxTimeWindow());
            execs.Add(new ResultSetLimitSnapshot());
            execs.Add(new ResultSetLimitSnapshotJoin());
            execs.Add(new ResultSetJoinSortWindow());
            execs.Add(new ResultSetRowPerEventNoJoinLast());
            execs.Add(new ResultSetRowPerEventJoinAll());
            execs.Add(new ResultSetRowPerEventJoinLast());
            execs.Add(new ResultSetTime());
            execs.Add(new ResultSetCount());
            return execs;
        }

        private static void RunAssertion9AllNoHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select Symbol, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "output all every 1 seconds";
            TryAssertion56(env, stmtText, "all");
        }

        private static void RunAssertion10AllNoHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt hint)
        {
            var stmtText = hint.GetHint() +
                           "@Name('s0') select Symbol, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=Symbol " +
                           "output all every 1 seconds";
            TryAssertion56(env, stmtText, "all");
        }

        private static void RunAssertion11AllHavingNoJoinHinted(
            RegressionEnvironment env,
            SupportOutputLimitOpt hint)
        {
            var stmtText = hint.GetHint() +
                           "@Name('s0') select Symbol, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "having sum(Price) > 100" +
                           "output all every 1 seconds";
            TryAssertion78(env, stmtText, "all");
        }

        private static void RunAssertion12AllHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt hint)
        {
            var stmtText = hint.GetHint() +
                           "@Name('s0') select Symbol, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=Symbol " +
                           "having sum(Price) > 100" +
                           "output all every 1 seconds";
            TryAssertion78(env, stmtText, "all");
        }

        private static void RunAssertion13LastNoHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select Symbol, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec)" +
                           "output last every 1 seconds";
            TryAssertion13_14(env, stmtText, "last");
        }

        private static void RunAssertion14LastNoHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt hint)
        {
            var stmtText = hint.GetHint() +
                           "@Name('s0') select Symbol, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where TheString=Symbol " +
                           "output last every 1 seconds";
            TryAssertion13_14(env, stmtText, "last");
        }

        private static void RunAssertion15LastHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt hint)
        {
            var stmtText = hint.GetHint() +
                           "@Name('s0') select Symbol, sum(Price) " +
                           "from SupportMarketDataBean#time(5.5 sec)" +
                           "having sum(Price) > 100 " +
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
                           "having sum(Price) > 100 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last");
        }

        private static void SendEventLong(
            RegressionEnvironment env,
            long volume)
        {
            env.SendEventBean(new SupportMarketDataBean("DELL", 0.0, volume, null));
        }

        private static void CreateStmtAndListenerNoJoin(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy(epl).AddListener("s0");
        }

        private static void TryAssertAllSum(RegressionEnvironment env)
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
            Assert.AreEqual(1L, env.Listener("s0").LastNewData[0].Get("result"));
            Assert.AreEqual(2L, env.Listener("s0").LastNewData[1].Get("LongBoxed"));
            Assert.AreEqual(3L, env.Listener("s0").LastNewData[1].Get("result"));
            Assert.IsNull(env.Listener("s0").LastOldData);

            env.UndeployAll();
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
            expected.AddResultInsert(
                200,
                1,
                new[] {new object[] {"IBM", 25d}});
            expected.AddResultInsert(
                800,
                1,
                new[] {new object[] {"MSFT", 34d}});
            expected.AddResultInsert(
                1500,
                1,
                new[] {new object[] {"IBM", 58d}});
            expected.AddResultInsert(
                1500,
                2,
                new[] {new object[] {"YAH", 59d}});
            expected.AddResultInsert(
                2100,
                1,
                new[] {new object[] {"IBM", 85d}});
            expected.AddResultInsert(
                3500,
                1,
                new[] {new object[] {"YAH", 87d}});
            expected.AddResultInsert(
                4300,
                1,
                new[] {new object[] {"IBM", 109d}});
            expected.AddResultInsert(
                4900,
                1,
                new[] {new object[] {"YAH", 112d}});
            expected.AddResultRemove(
                5700,
                0,
                new[] {new object[] {"IBM", 87d}});
            expected.AddResultInsert(
                5900,
                1,
                new[] {new object[] {"YAH", 88d}});
            expected.AddResultRemove(
                6300,
                0,
                new[] {new object[] {"MSFT", 79d}});
            expected.AddResultRemove(
                7000,
                0,
                new[] {new object[] {"IBM", 54d}, new object[] {"YAH", 54d}});

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
            expected.AddResultInsert(
                4300,
                1,
                new[] {new object[] {"IBM", 109d}});
            expected.AddResultInsert(
                4900,
                1,
                new[] {new object[] {"YAH", 112d}});

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
            expected.AddResultInsert(
                1200,
                0,
                new[] {new object[] {"MSFT", 34d}});
            expected.AddResultInsert(
                2200,
                0,
                new[] {new object[] {"IBM", 85d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(
                4200,
                0,
                new[] {new object[] {"YAH", 87d}});
            expected.AddResultInsert(
                5200,
                0,
                new[] {new object[] {"YAH", 112d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {"YAH", 88d}},
                new[] {new object[] {"IBM", 87d}});
            expected.AddResultRemove(
                7200,
                0,
                new[] {new object[] {"YAH", 54d}});

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
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(
                5200,
                0,
                new[] {new object[] {"YAH", 112d}});
            expected.AddResultInsRem(6200, 0, null, null);
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

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(
                5200,
                0,
                new[] {new object[] {"IBM", 109d}, new object[] {"YAH", 112d}},
                null);
            expected.AddResultInsRem(6200, 0, null, null);
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

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new[] {new object[] {"IBM", 25d}, new object[] {"MSFT", 34d}});
            expected.AddResultInsert(
                2200,
                0,
                new[] {new object[] {"IBM", 58d}, new object[] {"YAH", 59d}, new object[] {"IBM", 85d}});
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(
                4200,
                0,
                new[] {new object[] {"YAH", 87d}});
            expected.AddResultInsert(
                5200,
                0,
                new[] {new object[] {"IBM", 109d}, new object[] {"YAH", 112d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {"YAH", 88d}},
                new[] {new object[] {"IBM", 87d}});
            expected.AddResultRemove(
                7200,
                0,
                new[] {new object[] {"MSFT", 79d}, new object[] {"IBM", 54d}, new object[] {"YAH", 54d}});

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void TryAssertion17IStreamOnly(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                200,
                1,
                new[] {new object[] {"IBM", 25d}});
            expected.AddResultInsert(
                1500,
                1,
                new[] {new object[] {"IBM", 58d}});
            expected.AddResultInsert(
                3500,
                1,
                new[] {new object[] {"YAH", 87d}});
            expected.AddResultInsert(
                4300,
                1,
                new[] {new object[] {"IBM", 109d}});
            expected.AddResultInsert(
                5900,
                1,
                new[] {new object[] {"YAH", 88d}});

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

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                200,
                1,
                new[] {new object[] {"IBM", 25d}});
            expected.AddResultInsert(
                1500,
                1,
                new[] {new object[] {"IBM", 58d}});
            expected.AddResultInsert(
                3500,
                1,
                new[] {new object[] {"YAH", 87d}});
            expected.AddResultInsert(
                4300,
                1,
                new[] {new object[] {"IBM", 109d}});
            expected.AddResultRemove(
                5700,
                0,
                new[] {new object[] {"IBM", 87d}});
            expected.AddResultRemove(
                6300,
                0,
                new[] {new object[] {"MSFT", 79d}});

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

            string[] fields = {"Symbol", "sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(
                1200,
                0,
                new[] {new object[] {"IBM", 34d}, new object[] {"MSFT", 34d}});
            expected.AddResultInsert(
                2200,
                0,
                new[] {
                    new object[] {"IBM", 85d}, new object[] {"MSFT", 85d}, new object[] {"IBM", 85d},
                    new object[] {"YAH", 85d}, new object[] {"IBM", 85d}
                });
            expected.AddResultInsert(
                3200,
                0,
                new[] {
                    new object[] {"IBM", 85d}, new object[] {"MSFT", 85d}, new object[] {"IBM", 85d},
                    new object[] {"YAH", 85d}, new object[] {"IBM", 85d}
                });
            expected.AddResultInsert(
                4200,
                0,
                new[] {
                    new object[] {"IBM", 87d}, new object[] {"MSFT", 87d}, new object[] {"IBM", 87d},
                    new object[] {"YAH", 87d}, new object[] {"IBM", 87d}, new object[] {"YAH", 87d}
                });
            expected.AddResultInsert(
                5200,
                0,
                new[] {
                    new object[] {"IBM", 112d}, new object[] {"MSFT", 112d}, new object[] {"IBM", 112d},
                    new object[] {"YAH", 112d}, new object[] {"IBM", 112d}, new object[] {"YAH", 112d},
                    new object[] {"IBM", 112d}, new object[] {"YAH", 112d}
                });
            expected.AddResultInsert(
                6200,
                0,
                new[] {
                    new object[] {"MSFT", 88d}, new object[] {"IBM", 88d}, new object[] {"YAH", 88d},
                    new object[] {"IBM", 88d}, new object[] {"YAH", 88d}, new object[] {"IBM", 88d},
                    new object[] {"YAH", 88d},
                    new object[] {"YAH", 88d}
                });
            expected.AddResultInsert(
                7200,
                0,
                new[] {
                    new object[] {"IBM", 54d}, new object[] {"YAH", 54d}, new object[] {"IBM", 54d},
                    new object[] {"YAH", 54d}, new object[] {"YAH", 54d}
                });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
        }

        private static void TryAssertionHaving(RegressionEnvironment env)
        {
            SendEvent(env, "SYM1", 10d);
            SendEvent(env, "SYM1", 11d);
            SendEvent(env, "SYM1", 9);

            SendTimer(env, 1000);
            var fields = new [] { "Symbol","avgPrice" };
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"SYM1", 10.5});

            SendEvent(env, "SYM1", 13d);
            SendEvent(env, "SYM1", 10d);
            SendEvent(env, "SYM1", 9);
            SendTimer(env, 2000);

            Assert.AreEqual(3, env.Listener("s0").LastNewData.Length);
            Assert.IsNull(env.Listener("s0").LastOldData);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {
                    new object[] {"SYM1", 43 / 4.0}, new object[] {"SYM1", 53.0 / 5.0}, new object[] {"SYM1", 62 / 6.0}
                });

            env.UndeployAll();
        }

        private static void TryAssertLastSum(RegressionEnvironment env)
        {
            // send an event
            SendEvent(env, 1);

            // check no update
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            // send another event
            SendEvent(env, 2);

            // check update, all events present
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
            Assert.AreEqual(2L, env.Listener("s0").LastNewData[0].Get("LongBoxed"));
            Assert.AreEqual(3L, env.Listener("s0").LastNewData[0].Get("result"));
            Assert.IsNull(env.Listener("s0").LastOldData);

            env.UndeployAll();
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

        private static void SendEvent(
            RegressionEnvironment env,
            long longBoxed)
        {
            SendEvent(env, longBoxed, 0, 0);
        }

        private static void SendMarketDataEvent(
            RegressionEnvironment env,
            long volume)
        {
            env.SendEventBean(new SupportMarketDataBean("SYM1", 0, volume, null));
        }

        private static void SendTimeEventRelative(
            RegressionEnvironment env,
            int timeIncrement,
            AtomicLong currentTime)
        {
            currentTime.IncrementAndGet(timeIncrement);
            env.AdvanceTime(currentTime.Get());
        }

        private static void CreateStmtAndListenerJoin(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy(epl).AddListener("s0");
            env.SendEventBean(new SupportBeanString(JOIN_KEY));
        }

        private static void AssertEvent(
            RegressionEnvironment env,
            long volume)
        {
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            Assert.IsTrue(env.Listener("s0").LastNewData != null);
            Assert.AreEqual(1, env.Listener("s0").LastNewData.Length);
            Assert.AreEqual(volume, env.Listener("s0").LastNewData[0].Get("sum(Volume)"));
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
            long time)
        {
            env.AdvanceTime(time);
        }

        internal class ResultSet1NoneNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec)";
                TryAssertion12(env, stmtText, "none");
            }
        }

        internal class ResultSet2NoneNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where TheString=Symbol";
                TryAssertion12(env, stmtText, "none");
            }
        }

        internal class ResultSet3NoneHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               " having sum(Price) > 100";
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
                               " having sum(Price) > 100";
                TryAssertion34(env, stmtText, "none");
            }
        }

        internal class ResultSet5DefaultNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output every 1 seconds";
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
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default");
            }
        }

        internal class ResultSet7DefaultHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) \n" +
                               "having sum(Price) > 100" +
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
                               "having sum(Price) > 100" +
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
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "having sum(Price) > 100" +
                               "output all every 1 seconds";
                TryAssertion78(env, stmtText, "all");
            }
        }

        internal class ResultSet11AllHavingNoJoinHinted : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion11AllHavingNoJoinHinted(env, outputLimitOpt);
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
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion13LastNoHavingNoJoin(env, outputLimitOpt);
                }
            }
        }

        internal class ResultSet14LastNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion14LastNoHavingJoin(env, outputLimitOpt);
                }
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

        internal class ResultSet17FirstNoHavingNoJoinIStreamOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output first every 1 seconds";
                TryAssertion17IStreamOnly(env, stmtText, "first");
            }
        }

        internal class ResultSet17FirstNoHavingNoJoinIRStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select irstream Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output first every 1 seconds";
                TryAssertion17IRStream(env, stmtText, "first");
            }
        }

        internal class ResultSet18SnapshotNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Symbol, sum(Price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output snapshot every 1 seconds";
                TryAssertion18(env, stmtText, "first");
            }
        }

        internal class ResultSetHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@Name('s0') select Symbol, avg(Price) as avgPrice " +
                          "from SupportMarketDataBean#time(3 sec) " +
                          "having avg(Price) > 10" +
                          "output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionHaving(env);
            }
        }

        internal class ResultSetHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@Name('s0') select Symbol, avg(Price) as avgPrice " +
                          "from SupportMarketDataBean#time(3 sec) as md, " +
                          "SupportBean#keepall as s where s.TheString = md.Symbol " +
                          "having avg(Price) > 10" +
                          "output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("SYM1", -1));

                TryAssertionHaving(env);
            }
        }

        internal class ResultSetMaxTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@Name('s0') select irstream Volume, max(Price) as maxVol" +
                          " from SupportMarketDataBean#time(1 sec) " +
                          "output every 1 seconds";
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

        internal class ResultSetLimitSnapshot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt = "@Name('s0') select Symbol, sum(Price) as sumPrice from SupportMarketDataBean" +
                                 "#time(10 seconds) output snapshot every 1 seconds order by Symbol asc";
                env.CompileDeploy(selectStmt).AddListener("s0");

                SendEvent(env, "ABC", 20);

                SendTimer(env, 500);
                SendEvent(env, "IBM", 16);
                SendEvent(env, "MSFT", 14);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 1000);
                string[] fields = {"Symbol", "sumPrice"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"ABC", 50d},
                        new object[] {"IBM", 50d},
                        new object[] {"MSFT", 50d}
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 1500);
                SendEvent(env, "YAH", 18);
                SendEvent(env, "s4", 30);

                SendTimer(env, 10000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"ABC", 98d},
                        new object[] {"IBM", 98d}, 
                        new object[] {"MSFT", 98d},
                        new object[] {"s4", 98d},
                        new object[] {"YAH", 98d}
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 11000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"s4", 48d},
                        new object[] {"YAH", 48d}
                    });
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
                    "@Name('s0') select irstream Symbol, sum(Price) as sumPrice from SupportMarketDataBean" +
                    "#time(10 seconds) as m, SupportBean" +
                    "#keepall as s where s.TheString = m.Symbol output snapshot every 1 seconds order by Symbol asc";
                env.CompileDeploy(selectStmt).AddListener("s0");

                env.SendEventBean(new SupportBean("ABC", 1));
                env.SendEventBean(new SupportBean("IBM", 2));
                env.SendEventBean(new SupportBean("MSFT", 3));
                env.SendEventBean(new SupportBean("YAH", 4));
                env.SendEventBean(new SupportBean("s4", 5));

                SendEvent(env, "ABC", 20);

                SendTimer(env, 500);
                SendEvent(env, "IBM", 16);
                SendEvent(env, "MSFT", 14);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 1000);
                string[] fields = {"Symbol", "sumPrice"};
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"ABC", 50d}, new object[] {"IBM", 50d}, new object[] {"MSFT", 50d}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 1500);
                SendEvent(env, "YAH", 18);
                SendEvent(env, "s4", 30);

                SendTimer(env, 10000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"ABC", 98d}, 
                        new object[] {"IBM", 98d},
                        new object[] {"MSFT", 98d},
                        new object[] {"s4", 98d},
                        new object[] {"YAH", 98d}
                    });
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 10500);
                SendTimer(env, 11000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"s4", 48d},
                        new object[] {"YAH", 48d}
                    });
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

        internal class ResultSetJoinSortWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@Name('s0') select irstream Volume, max(Price) as maxVol" +
                          " from SupportMarketDataBean#sort(1, Volume desc) as S0," +
                          "SupportBean#keepall as S1 " +
                          "output every 1 seconds";
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

        internal class ResultSetRowPerEventNoJoinLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionRowPerEventNoJoinLast(env, outputLimitOpt);
                }
            }

            private static void TryAssertionRowPerEventNoJoinLast(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                var epl = opt.GetHint() +
                          "@Name('s0') select LongBoxed, sum(LongBoxed) as result " +
                          "from SupportBean#length(3) " +
                          "having sum(LongBoxed) > 0 " +
                          "output last every 2 events";

                CreateStmtAndListenerNoJoin(env, epl);
                TryAssertLastSum(env);

                epl = opt.GetHint() +
                      "@Name('s0') select LongBoxed, sum(LongBoxed) as result " +
                      "from SupportBean#length(3) " +
                      "output last every 2 events";
                CreateStmtAndListenerNoJoin(env, epl);
                TryAssertLastSum(env);
            }
        }

        internal class ResultSetRowPerEventJoinAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionRowPerEventJoinAll(env, outputLimitOpt);
                }
            }

            private static void TryAssertionRowPerEventJoinAll(
                RegressionEnvironment env,
                SupportOutputLimitOpt opt)
            {
                var epl = opt.GetHint() +
                          "@Name('s0') select LongBoxed, sum(LongBoxed) as result " +
                          "from SupportBeanString#length(3) as one, " +
                          "SupportBean#length(3) as two " +
                          "having sum(LongBoxed) > 0 " +
                          "output all every 2 events";

                CreateStmtAndListenerJoin(env, epl);
                TryAssertAllSum(env);

                epl = opt.GetHint() +
                      "@Name('s0') select LongBoxed, sum(LongBoxed) as result " +
                      "from SupportBeanString#length(3) as one, " +
                      "SupportBean#length(3) as two " +
                      "output every 2 events";

                CreateStmtAndListenerJoin(env, epl);
                TryAssertAllSum(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetRowPerEventJoinLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select LongBoxed, sum(LongBoxed) as result " +
                          "from SupportBeanString#length(3) as one, " +
                          "SupportBean#length(3) as two " +
                          "having sum(LongBoxed) > 0 " +
                          "output last every 2 events";

                CreateStmtAndListenerJoin(env, epl);
                TryAssertLastSum(env);

                epl = "@Name('s0') select LongBoxed, sum(LongBoxed) as result " +
                      "from SupportBeanString#length(3) as one, " +
                      "SupportBean#length(3) as two " +
                      "output last every 2 events";

                CreateStmtAndListenerJoin(env, epl);
                TryAssertLastSum(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetTime : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Set the clock to 0
                var currentTime = new AtomicLong();
                SendTimeEventRelative(env, 0, currentTime);

                // Create the EPL statement and add a listener
                var epl = "@Name('s0') select Symbol, sum(Volume) from " +
                          EVENT_NAME +
                          "#length(5) output first every 3 seconds";
                env.CompileDeploy(epl).AddListener("s0");
                env.Listener("s0").Reset();

                // Send the first event of the batch; should be output
                SendMarketDataEvent(env, 10L);
                AssertEvent(env, 10L);

                // Send another event, not the first, for aggregation
                // update only, no output
                SendMarketDataEvent(env, 20L);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                // Update time
                SendTimeEventRelative(env, 3000, currentTime);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                // Send first event of the next batch, should be output.
                // The aggregate value is computed over all events
                // received: 10 + 20 + 30 = 60
                SendMarketDataEvent(env, 30L);
                AssertEvent(env, 60L);

                // Send the next event of the batch, no output
                SendMarketDataEvent(env, 40L);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                // Update time
                SendTimeEventRelative(env, 3000, currentTime);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                // Send first event of third batch
                SendMarketDataEvent(env, 1L);
                AssertEvent(env, 101L);

                // Update time
                SendTimeEventRelative(env, 3000, currentTime);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                // Update time: no first event this batch, so a callback
                // is made at the end of the interval
                SendTimeEventRelative(env, 3000, currentTime);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class ResultSetCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Create the EPL statement and add a listener
                var statementText = "@Name('s0') select Symbol, sum(Volume) from " +
                                    EVENT_NAME +
                                    "#length(5) output first every 3 events";
                env.CompileDeploy(statementText).AddListener("s0");
                env.Listener("s0").Reset();

                // Send the first event of the batch, should be output
                SendEventLong(env, 10L);
                AssertEvent(env, 10L);

                // Send the second event of the batch, not output, used
                // for updating the aggregate value only
                SendEventLong(env, 20L);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                // Send the third event of the batch, still not output,
                // but should reset the batch
                SendEventLong(env, 30L);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                // First event, next batch, aggregate value should be
                // 10 + 20 + 30 + 40 = 100
                SendEventLong(env, 40L);
                AssertEvent(env, 100L);

                // Next event again not output
                SendEventLong(env, 50L);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }
    }
} // end of namespace