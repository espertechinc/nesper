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
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitRowForAll
    {
        private const string CATEGORY = "Fully-Aggregated and Un-grouped";

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
            execs.Add(new ResultSet17FirstNoHavingNoJoin());
            execs.Add(new ResultSet18SnapshotNoHavingNoJoin());
            execs.Add(new ResultSetOutputLastWithInsertInto());
            execs.Add(new ResultSetAggAllHaving());
            execs.Add(new ResultSetAggAllHavingJoin());
            execs.Add(new ResultSetJoinSortWindow());
            execs.Add(new ResultSetMaxTimeWindow());
            execs.Add(new ResultSetTimeWindowOutputCountLast());
            execs.Add(new ResultSetTimeBatchOutputCount());
            execs.Add(new ResultSetLimitSnapshot());
            execs.Add(new ResultSetLimitSnapshotJoin());
            execs.Add(new ResultSetOutputSnapshotGetValue());
            return execs;
        }

        private static void RunAssertion9AllNoHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "output all every 1 seconds";
            TryAssertion56(env, stmtText, "all");
        }

        private static void RunAssertion10AllNoHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select sum(price) " +
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
                           "@Name('s0') select sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec) " +
                           "having sum(price) > 100" +
                           "output all every 1 seconds";
            TryAssertion78(env, stmtText, "all");
        }

        private static void RunAssertion12AllHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where theString=symbol " +
                           "having sum(price) > 100" +
                           "output all every 1 seconds";
            TryAssertion78(env, stmtText, "all");
        }

        private static void RunAssertion13LastNoHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec)" +
                           "output last every 1 seconds";
            TryAssertion13_14(env, stmtText, "last");
        }

        private static void RunAssertion14LastNoHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where theString=symbol " +
                           "output last every 1 seconds";
            TryAssertion13_14(env, stmtText, "last");
        }

        private static void RunAssertion15LastHavingNoJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec)" +
                           "having sum(price) > 100 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last");
        }

        private static void RunAssertion16LastHavingJoin(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var stmtText = opt.GetHint() +
                           "@Name('s0') select sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec), " +
                           "SupportBean#keepall where theString=symbol " +
                           "having sum(price) > 100 " +
                           "output last every 1 seconds";
            TryAssertion15_16(env, stmtText, "last");
        }

        private static void TryAssertionOutputSnapshotGetValue(
            RegressionEnvironment env,
            bool join)
        {
            var epl = "@Name('s0') select customagg(IntPrimitive) as c0 from SupportBean" +
                      (join ? "#keepall, SupportBean_S0#lastevent" : "") +
                      " output snapshot every 3 events";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1));

            SupportInvocationCountFunction.ResetGetValueInvocationCount();

            env.SendEventBean(new SupportBean("E1", 10));
            env.SendEventBean(new SupportBean("E2", 20));
            Assert.AreEqual(0, SupportInvocationCountFunction.GetValueInvocationCount);

            env.SendEventBean(new SupportBean("E3", 30));
            Assert.AreEqual(60, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
            Assert.AreEqual(1, SupportInvocationCountFunction.GetValueInvocationCount);

            env.SendEventBean(new SupportBean("E3", 40));
            env.SendEventBean(new SupportBean("E4", 50));
            env.SendEventBean(new SupportBean("E5", 60));
            Assert.AreEqual(210, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
            Assert.AreEqual(2, SupportInvocationCountFunction.GetValueInvocationCount);

            env.UndeployAll();
        }

        private static void TryAssertionOuputLastWithInsertInto(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt)
        {
            var epl = opt.GetHint() +
                      "insert into MyStream select sum(IntPrimitive) as thesum from SupportBean#keepall " +
                      "output last every 2 events;\n" +
                      "@Name('s0') select * from MyStream;\n";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 10));
            env.SendEventBean(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                "thesum".SplitCsv(),
                new object[] {30});

            env.UndeployAll();
        }

        private static void TryAssertion12(
            RegressionEnvironment env,
            string stmtText,
            string outputLimit)
        {
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            string[] fields = {"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                200,
                1,
                new[] {new object[] {25d}},
                new[] {new object[] {null}});
            expected.AddResultInsRem(
                800,
                1,
                new[] {new object[] {34d}},
                new[] {new object[] {25d}});
            expected.AddResultInsRem(
                1500,
                1,
                new[] {new object[] {58d}},
                new[] {new object[] {34d}});
            expected.AddResultInsRem(
                1500,
                2,
                new[] {new object[] {59d}},
                new[] {new object[] {58d}});
            expected.AddResultInsRem(
                2100,
                1,
                new[] {new object[] {85d}},
                new[] {new object[] {59d}});
            expected.AddResultInsRem(
                3500,
                1,
                new[] {new object[] {87d}},
                new[] {new object[] {85d}});
            expected.AddResultInsRem(
                4300,
                1,
                new[] {new object[] {109d}},
                new[] {new object[] {87d}});
            expected.AddResultInsRem(
                4900,
                1,
                new[] {new object[] {112d}},
                new[] {new object[] {109d}});
            expected.AddResultInsRem(
                5700,
                0,
                new[] {new object[] {87d}},
                new[] {new object[] {112d}});
            expected.AddResultInsRem(
                5900,
                1,
                new[] {new object[] {88d}},
                new[] {new object[] {87d}});
            expected.AddResultInsRem(
                6300,
                0,
                new[] {new object[] {79d}},
                new[] {new object[] {88d}});
            expected.AddResultInsRem(
                7000,
                0,
                new[] {new object[] {54d}},
                new[] {new object[] {79d}});

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

            string[] fields = {"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                4300,
                1,
                new[] {new object[] {109d}},
                null);
            expected.AddResultInsRem(
                4900,
                1,
                new[] {new object[] {112d}},
                new[] {new object[] {109d}});
            expected.AddResultInsRem(
                5700,
                0,
                null,
                new[] {new object[] {112d}});

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

            string[] fields = {"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200,
                0,
                new[] {new object[] {34d}},
                new[] {new object[] {null}});
            expected.AddResultInsRem(
                2200,
                0,
                new[] {new object[] {85d}},
                new[] {new object[] {34d}});
            expected.AddResultInsRem(
                3200,
                0,
                new[] {new object[] {85d}},
                new[] {new object[] {85d}});
            expected.AddResultInsRem(
                4200,
                0,
                new[] {new object[] {87d}},
                new[] {new object[] {85d}});
            expected.AddResultInsRem(
                5200,
                0,
                new[] {new object[] {112d}},
                new[] {new object[] {87d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {88d}},
                new[] {new object[] {112d}});
            expected.AddResultInsRem(
                7200,
                0,
                new[] {new object[] {54d}},
                new[] {new object[] {88d}});

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

            string[] fields = {"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(
                5200,
                0,
                new[] {new object[] {112d}},
                new[] {new object[] {109d}});
            expected.AddResultInsRem(
                6200,
                0,
                null,
                new[] {new object[] {112d}});
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

            string[] fields = {"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(
                5200,
                0,
                new[] {new object[] {109d}, new object[] {112d}},
                new[] {new object[] {109d}});
            expected.AddResultInsRem(
                6200,
                0,
                null,
                new[] {new object[] {112d}});
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

            string[] fields = {"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200,
                0,
                new[] {new object[] {25d}, new object[] {34d}},
                new[] {new object[] {null}, new object[] {25d}});
            expected.AddResultInsRem(
                2200,
                0,
                new[] {new object[] {58d}, new object[] {59d}, new object[] {85d}},
                new[] {new object[] {34d}, new object[] {58d}, new object[] {59d}});
            expected.AddResultInsRem(
                3200,
                0,
                new[] {new object[] {85d}},
                new[] {new object[] {85d}});
            expected.AddResultInsRem(
                4200,
                0,
                new[] {new object[] {87d}},
                new[] {new object[] {85d}});
            expected.AddResultInsRem(
                5200,
                0,
                new[] {new object[] {109d}, new object[] {112d}},
                new[] {new object[] {87d}, new object[] {109d}});
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {87d}, new object[] {88d}},
                new[] {new object[] {112d}, new object[] {87d}});
            expected.AddResultInsRem(
                7200,
                0,
                new[] {new object[] {79d}, new object[] {54d}},
                new[] {new object[] {88d}, new object[] {79d}});

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

            string[] fields = {"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                200,
                1,
                new[] {new object[] {25d}},
                new[] {new object[] {null}});
            expected.AddResultInsRem(
                1500,
                1,
                new[] {new object[] {58d}},
                new[] {new object[] {34d}});
            expected.AddResultInsRem(
                3500,
                1,
                new[] {new object[] {87d}},
                new[] {new object[] {85d}});
            expected.AddResultInsRem(
                4300,
                1,
                new[] {new object[] {109d}},
                new[] {new object[] {87d}});
            expected.AddResultInsRem(
                5700,
                0,
                new[] {new object[] {87d}},
                new[] {new object[] {112d}});
            expected.AddResultInsRem(
                6300,
                0,
                new[] {new object[] {79d}},
                new[] {new object[] {88d}});

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

            string[] fields = {"sum(price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(
                1200,
                0,
                new[] {new object[] {34d}},
                null);
            expected.AddResultInsRem(
                2200,
                0,
                new[] {new object[] {85d}},
                null);
            expected.AddResultInsRem(
                3200,
                0,
                new[] {new object[] {85d}},
                null);
            expected.AddResultInsRem(
                4200,
                0,
                new[] {new object[] {87d}},
                null);
            expected.AddResultInsRem(
                5200,
                0,
                new[] {new object[] {112d}},
                null);
            expected.AddResultInsRem(
                6200,
                0,
                new[] {new object[] {88d}},
                null);
            expected.AddResultInsRem(
                7200,
                0,
                new[] {new object[] {54d}},
                null);

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(false);
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
            string s,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long time)
        {
            env.AdvanceTime(time);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        private static void SendMDEvent(
            RegressionEnvironment env,
            long volume)
        {
            var bean = new SupportMarketDataBean("S0", 0, volume, null);
            env.SendEventBean(bean);
        }

        internal class ResultSet1NoneNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec)";
                TryAssertion12(env, stmtText, "none");
            }
        }

        internal class ResultSet2NoneNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol";
                TryAssertion12(env, stmtText, "none");
            }
        }

        internal class ResultSet3NoneHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               " having sum(price) > 100";
                TryAssertion34(env, stmtText, "none");
            }
        }

        internal class ResultSet4NoneHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               " having sum(price) > 100";
                TryAssertion34(env, stmtText, "none");
            }
        }

        internal class ResultSet5DefaultNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output every 1 seconds";
                TryAssertion56(env, stmtText, "default");
            }
        }

        internal class ResultSet6DefaultNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select sum(price) " +
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
                var stmtText = "@Name('s0') select sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) \n" +
                               "having sum(price) > 100" +
                               "output every 1 seconds";
                TryAssertion78(env, stmtText, "default");
            }
        }

        internal class ResultSet8DefaultHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec), " +
                               "SupportBean#keepall where theString=symbol " +
                               "having sum(price) > 100" +
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

        internal class ResultSet17FirstNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output first every 1 seconds";
                TryAssertion17(env, stmtText, "first");
            }
        }

        internal class ResultSet18SnapshotNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec) " +
                               "output snapshot every 1 seconds";
                TryAssertion18(env, stmtText, "first");
            }
        }

        internal class ResultSetOutputLastWithInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionOuputLastWithInsertInto(env, outputLimitOpt);
                }
            }
        }

        internal class ResultSetAggAllHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select sum(volume) as result " +
                               "from SupportMarketDataBean#length(10) as two " +
                               "having sum(volume) > 0 " +
                               "output every 5 events";
                env.CompileDeploy(stmtText).AddListener("s0");

                string[] fields = {"result"};

                SendMDEvent(env, 20);
                SendMDEvent(env, -100);
                SendMDEvent(env, 0);
                SendMDEvent(env, 0);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendMDEvent(env, 0);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {20L}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {20L}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ResultSetAggAllHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select sum(volume) as result " +
                               "from SupportMarketDataBean#length(10) as one," +
                               "SupportBean#length(10) as two " +
                               "where one.symbol=two.TheString " +
                               "having sum(volume) > 0 " +
                               "output every 5 events";
                env.CompileDeploy(stmtText).AddListener("s0");

                string[] fields = {"result"};
                env.SendEventBean(new SupportBean("S0", 0));

                SendMDEvent(env, 20);
                SendMDEvent(env, -100);
                SendMDEvent(env, 0);
                SendMDEvent(env, 0);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendMDEvent(env, 0);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {20L}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {20L}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ResultSetJoinSortWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@Name('s0') select irstream max(price) as maxVol" +
                          " from SupportMarketDataBean#sort(1,volume desc) as s0, " +
                          "SupportBean#keepall as s1 where s1.TheString=s0.symbol " +
                          "output every 1.0d seconds";
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
                Assert.AreEqual(2, result.Second.Length);
                Assert.AreEqual(null, result.Second[0].Get("maxVol"));
                Assert.AreEqual(1.0, result.Second[1].Get("maxVol"));

                // statement object model test
                var model = env.EplToModel(epl);
                env.CopyMayFail(model);
                Assert.AreEqual(epl, model.ToEPL());

                env.UndeployAll();
            }
        }

        internal class ResultSetMaxTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@Name('s0') select irstream max(price) as maxVol" +
                          " from SupportMarketDataBean#time(1.1 sec) " +
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
                Assert.AreEqual(1.0, result.Second[1].Get("maxVol"));

                env.UndeployAll();
            }
        }

        internal class ResultSetTimeWindowOutputCountLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select count(*) as cnt from SupportBean#time(10 seconds) output every 10 seconds";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendTimer(env, 0);
                SendTimer(env, 10000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 20000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "e1");
                SendTimer(env, 30000);
                var newEvents = env.Listener("s0").GetAndResetLastNewData();
                Assert.AreEqual(2, newEvents.Length);
                Assert.AreEqual(1L, newEvents[0].Get("cnt"));
                Assert.AreEqual(0L, newEvents[1].Get("cnt"));

                SendTimer(env, 31000);

                SendEvent(env, "e2");
                SendEvent(env, "e3");
                SendTimer(env, 40000);
                newEvents = env.Listener("s0").GetAndResetLastNewData();
                Assert.AreEqual(2, newEvents.Length);
                Assert.AreEqual(1L, newEvents[0].Get("cnt"));
                Assert.AreEqual(2L, newEvents[1].Get("cnt"));

                env.UndeployAll();
            }
        }

        internal class ResultSetTimeBatchOutputCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select count(*) as cnt from SupportBean#time_batch(10 seconds) output every 10 seconds";
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
                Assert.AreEqual(2, newEvents.Length);
                // output limiting starts 10 seconds after, therefore the old batch was posted already and the cnt is zero
                Assert.AreEqual(1L, newEvents[0].Get("cnt"));
                Assert.AreEqual(0L, newEvents[1].Get("cnt"));

                SendTimer(env, 50000);
                var newData = env.Listener("s0").LastNewData;
                Assert.AreEqual(0L, newData[0].Get("cnt"));
                env.Listener("s0").Reset();

                SendEvent(env, "e2");
                SendEvent(env, "e3");
                SendTimer(env, 60000);
                newEvents = env.Listener("s0").GetAndResetLastNewData();
                Assert.AreEqual(1, newEvents.Length);
                Assert.AreEqual(2L, newEvents[0].Get("cnt"));

                env.UndeployAll();
            }
        }

        internal class ResultSetLimitSnapshot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                var selectStmt =
                    "@Name('s0') select count(*) as cnt from SupportBean#time(10 seconds) where IntPrimitive > 0 output snapshot every 1 seconds";
                env.CompileDeploy(selectStmt).AddListener("s0");

                SendEvent(env, "s0", 1);

                SendTimer(env, 500);
                SendEvent(env, "s1", 1);
                SendEvent(env, "s2", -1);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 1000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"cnt"},
                    new[] {new object[] {2L}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 1500);
                SendEvent(env, "s4", 2);
                SendEvent(env, "s5", 3);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 2000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"cnt"},
                    new[] {new object[] {4L}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendEvent(env, "s5", 4);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 9000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"cnt"},
                    new[] {new object[] {5L}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 10000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"cnt"},
                    new[] {new object[] {4L}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 10999);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 11000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"cnt"},
                    new[] {new object[] {3L}});
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
                var selectStmt = "@Name('s0') select count(*) as cnt from " +
                                 "SupportBean#time(10 seconds) as s, " +
                                 "SupportMarketDataBean#keepall as m where m.symbol = s.TheString and IntPrimitive > 0 output snapshot every 1 seconds";
                env.CompileDeploy(selectStmt).AddListener("s0");

                env.SendEventBean(new SupportMarketDataBean("s0", 0, 0L, ""));
                env.SendEventBean(new SupportMarketDataBean("s1", 0, 0L, ""));
                env.SendEventBean(new SupportMarketDataBean("s2", 0, 0L, ""));
                env.SendEventBean(new SupportMarketDataBean("s4", 0, 0L, ""));
                env.SendEventBean(new SupportMarketDataBean("s5", 0, 0L, ""));

                SendEvent(env, "s0", 1);

                SendTimer(env, 500);
                SendEvent(env, "s1", 1);
                SendEvent(env, "s2", -1);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 1000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"cnt"},
                    new[] {new object[] {2L}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 1500);
                SendEvent(env, "s4", 2);
                SendEvent(env, "s5", 3);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 2000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"cnt"},
                    new[] {new object[] {4L}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendEvent(env, "s5", 4);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 9000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"cnt"},
                    new[] {new object[] {5L}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                // The execution of the join is after the snapshot, as joins are internal dispatch
                SendTimer(env, 10000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"cnt"},
                    new[] {new object[] {5L}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendTimer(env, 10999);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                SendTimer(env, 11000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new[] {"cnt"},
                    new[] {new object[] {3L}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputSnapshotGetValue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionOutputSnapshotGetValue(env, true);
                TryAssertionOutputSnapshotGetValue(env, false);
            }
        }
    }
} // end of namespace