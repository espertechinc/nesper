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
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.patternassert;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitRowPerGroupRollup
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if TEMPORARY
            WithOutputLast(execs);
            WithOutputLastSorted(execs);
            WithOutputAll(execs);
            WithOutputAllSorted(execs);
            WithOutputDefault(execs);
            WithOutputDefaultSorted(execs);
            WithOutputFirstHaving(execs);
            WithOutputFirstSorted(execs);
            WithOutputFirst(execs);
            With3OutputLimitAll(execs);
            With4OutputLimitLast(execs);
            With1NoOutputLimit(execs);
            With2OutputLimitDefault(execs);
            With5OutputLimitFirst(execs);
            With6OutputLimitSnapshot(execs);
            WithOutputSnapshotOrderWLimit(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithOutputSnapshotOrderWLimit(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputSnapshotOrderWLimit());
            return execs;
        }

        public static IList<RegressionExecution> With6OutputLimitSnapshot(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet6OutputLimitSnapshot(false));
            execs.Add(new ResultSet6OutputLimitSnapshot(true));
            return execs;
        }

        public static IList<RegressionExecution> With5OutputLimitFirst(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet5OutputLimitFirst());
            return execs;
        }

        public static IList<RegressionExecution> With2OutputLimitDefault(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet2OutputLimitDefault());
            return execs;
        }

        public static IList<RegressionExecution> With1NoOutputLimit(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet1NoOutputLimit());
            return execs;
        }

        public static IList<RegressionExecution> With4OutputLimitLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet4OutputLimitLast());
            return execs;
        }

        public static IList<RegressionExecution> With3OutputLimitAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSet3OutputLimitAll());
            return execs;
        }

        public static IList<RegressionExecution> WithOutputFirst(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputFirst(false));
            execs.Add(new ResultSetOutputFirst(true));
            return execs;
        }

        public static IList<RegressionExecution> WithOutputFirstSorted(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputFirstSorted(false));
            execs.Add(new ResultSetOutputFirstSorted(true));
            return execs;
        }

        public static IList<RegressionExecution> WithOutputFirstHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputFirstHaving(false));
            execs.Add(new ResultSetOutputFirstHaving(true));
            return execs;
        }

        public static IList<RegressionExecution> WithOutputDefaultSorted(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputDefaultSorted(false));
            execs.Add(new ResultSetOutputDefaultSorted(true));
            return execs;
        }

        public static IList<RegressionExecution> WithOutputDefault(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputDefault(false));
            execs.Add(new ResultSetOutputDefault(true));
            return execs;
        }

        public static IList<RegressionExecution> WithOutputAllSorted(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputAllSorted(false));
            execs.Add(new ResultSetOutputAllSorted(true));
            return execs;
        }

        public static IList<RegressionExecution> WithOutputAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputAll(false));
            execs.Add(new ResultSetOutputAll(true));
            return execs;
        }

        public static IList<RegressionExecution> WithOutputLastSorted(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputLastSorted(false));
            execs.Add(new ResultSetOutputLastSorted(true));
            return execs;
        }

        public static IList<RegressionExecution> WithOutputLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOutputLast(false));
            execs.Add(new ResultSetOutputLast(true));
            return execs;
        }

        internal class ResultSetOutputSnapshotOrderWLimit : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl =
                    "@name('s0') select theString as c0, sum(intPrimitive) as c1 from SupportBean group by rollup(theString) " +
                    "output snapshot every 1 seconds " +
                    "order by sum(intPrimitive) " +
                    "limit 3";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E1", 12);
                SendEvent(env, "E2", 11);
                SendEvent(env, "E3", 10);
                SendEvent(env, "E4", 13);
                SendEvent(env, "E2", 5);

                SendTimer(env, 1000);

                env.AssertPropsPerRowLastNew(
                    "s0",
                    "c0,c1".SplitCsv(),
                    new object[][] { new object[] { "E3", 10 }, new object[] { "E1", 12 }, new object[] { "E4", 13 } });

                env.UndeployAll();
            }
        }

        internal class ResultSet1NoOutputLimit : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec)" +
                               "group by rollup(symbol)";
                SendTimer(env, 0);
                env.CompileDeploy(stmtText).AddListener("s0");

                var fields = new string[] { "symbol", "sum(price)" };
                var expected = new ResultAssertTestResult("NoOutputLimit", null, fields);
                expected.AddResultInsRem(
                    200,
                    1,
                    new object[][] { new object[] { "IBM", 25d }, new object[] { null, 25d } },
                    new object[][] { new object[] { "IBM", null }, new object[] { null, null } });
                expected.AddResultInsRem(
                    800,
                    1,
                    new object[][] { new object[] { "MSFT", 9d }, new object[] { null, 34d } },
                    new object[][] { new object[] { "MSFT", null }, new object[] { null, 25d } });
                expected.AddResultInsRem(
                    1500,
                    1,
                    new object[][] { new object[] { "IBM", 49d }, new object[] { null, 58d } },
                    new object[][] { new object[] { "IBM", 25d }, new object[] { null, 34d } });
                expected.AddResultInsRem(
                    1500,
                    2,
                    new object[][] { new object[] { "YAH", 1d }, new object[] { null, 59d } },
                    new object[][] { new object[] { "YAH", null }, new object[] { null, 58d } });
                expected.AddResultInsRem(
                    2100,
                    1,
                    new object[][] { new object[] { "IBM", 75d }, new object[] { null, 85d } },
                    new object[][] { new object[] { "IBM", 49d }, new object[] { null, 59d } });
                expected.AddResultInsRem(
                    3500,
                    1,
                    new object[][] { new object[] { "YAH", 3d }, new object[] { null, 87d } },
                    new object[][] { new object[] { "YAH", 1d }, new object[] { null, 85d } });
                expected.AddResultInsRem(
                    4300,
                    1,
                    new object[][] { new object[] { "IBM", 97d }, new object[] { null, 109d } },
                    new object[][] { new object[] { "IBM", 75d }, new object[] { null, 87d } });
                expected.AddResultInsRem(
                    4900,
                    1,
                    new object[][] { new object[] { "YAH", 6d }, new object[] { null, 112d } },
                    new object[][] { new object[] { "YAH", 3d }, new object[] { null, 109d } });
                expected.AddResultInsRem(
                    5700,
                    0,
                    new object[][] { new object[] { "IBM", 72d }, new object[] { null, 87d } },
                    new object[][] { new object[] { "IBM", 97d }, new object[] { null, 112d } });
                expected.AddResultInsRem(
                    5900,
                    1,
                    new object[][] { new object[] { "YAH", 7d }, new object[] { null, 88d } },
                    new object[][] { new object[] { "YAH", 6d }, new object[] { null, 87d } });
                expected.AddResultInsRem(
                    6300,
                    0,
                    new object[][] { new object[] { "MSFT", null }, new object[] { null, 79d } },
                    new object[][] { new object[] { "MSFT", 9d }, new object[] { null, 88d } });
                expected.AddResultInsRem(
                    7000,
                    0,
                    new object[][]
                        { new object[] { "IBM", 48d }, new object[] { "YAH", 6d }, new object[] { null, 54d } },
                    new object[][]
                        { new object[] { "IBM", 72d }, new object[] { "YAH", 7d }, new object[] { null, 79d } });

                var execution = new ResultAssertExecution(stmtText, env, expected);
                execution.Execute(false, new AtomicLong());
            }
        }

        internal class ResultSet2OutputLimitDefault : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec)" +
                               "group by rollup(symbol)" +
                               "output every 1 seconds";
                SendTimer(env, 0);
                env.CompileDeploy(stmtText).AddListener("s0");

                var fields = new string[] { "symbol", "sum(price)" };
                var expected = new ResultAssertTestResult("DefaultOutputLimit", null, fields);
                expected.AddResultInsRem(
                    1200,
                    0,
                    new object[][] {
                        new object[] { "IBM", 25d }, new object[] { null, 25d }, new object[] { "MSFT", 9d },
                        new object[] { null, 34d }
                    },
                    new object[][] {
                        new object[] { "IBM", null }, new object[] { null, null }, new object[] { "MSFT", null },
                        new object[] { null, 25d }
                    });
                expected.AddResultInsRem(
                    2200,
                    0,
                    new object[][] {
                        new object[] { "IBM", 49d }, new object[] { null, 58d }, new object[] { "YAH", 1d },
                        new object[] { null, 59d }, new object[] { "IBM", 75d }, new object[] { null, 85d }
                    },
                    new object[][] {
                        new object[] { "IBM", 25d }, new object[] { null, 34d }, new object[] { "YAH", null },
                        new object[] { null, 58d }, new object[] { "IBM", 49d }, new object[] { null, 59d }
                    });
                expected.AddResultInsRem(3200, 0, null, null);
                expected.AddResultInsRem(
                    4200,
                    0,
                    new object[][] { new object[] { "YAH", 3d }, new object[] { null, 87d } },
                    new object[][] { new object[] { "YAH", 1d }, new object[] { null, 85d } });
                expected.AddResultInsRem(
                    5200,
                    0,
                    new object[][] {
                        new object[] { "IBM", 97d }, new object[] { null, 109d }, new object[] { "YAH", 6d },
                        new object[] { null, 112d }
                    },
                    new object[][] {
                        new object[] { "IBM", 75d }, new object[] { null, 87d }, new object[] { "YAH", 3d },
                        new object[] { null, 109d }
                    });
                expected.AddResultInsRem(
                    6200,
                    0,
                    new object[][] {
                        new object[] { "IBM", 72d }, new object[] { null, 87d }, new object[] { "YAH", 7d },
                        new object[] { null, 88d }
                    },
                    new object[][] {
                        new object[] { "IBM", 97d }, new object[] { null, 112d }, new object[] { "YAH", 6d },
                        new object[] { null, 87d }
                    });
                expected.AddResultInsRem(
                    7200,
                    0,
                    new object[][] {
                        new object[] { "MSFT", null }, new object[] { null, 79d }, new object[] { "IBM", 48d },
                        new object[] { "YAH", 6d }, new object[] { null, 54d }
                    },
                    new object[][] {
                        new object[] { "MSFT", 9d }, new object[] { null, 88d }, new object[] { "IBM", 72d },
                        new object[] { "YAH", 7d }, new object[] { null, 79d }
                    });

                var execution = new ResultAssertExecution(stmtText, env, expected);
                execution.Execute(false, new AtomicLong());
            }
        }

        internal class ResultSet3OutputLimitAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    if (env.IsHA && outputLimitOpt == SupportOutputLimitOpt.DISABLED) {
                        continue;
                    }

                    RunAssertion3OutputLimitAll(env, outputLimitOpt, milestone);
                }
            }
        }

        private static void RunAssertion3OutputLimitAll(
            RegressionEnvironment env,
            SupportOutputLimitOpt outputLimitOpt,
            AtomicLong milestone)
        {
            var stmtText = outputLimitOpt.GetHint() +
                           "@name('s0') select symbol, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec)" +
                           "group by rollup(symbol)" +
                           "output all every 1 seconds";
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "sum(price)" };
            var expected = new ResultAssertTestResult("AllOutputLimit", null, fields);
            expected.AddResultInsRem(
                1200,
                0,
                new object[][] { new object[] { "IBM", 25d }, new object[] { "MSFT", 9d }, new object[] { null, 34d } },
                new object[][]
                    { new object[] { "IBM", null }, new object[] { "MSFT", null }, new object[] { null, null } });
            expected.AddResultInsRem(
                2200,
                0,
                new object[][] {
                    new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 1d },
                    new object[] { null, 85d }
                },
                new object[][] {
                    new object[] { "IBM", 25d }, new object[] { "MSFT", 9d }, new object[] { "YAH", null },
                    new object[] { null, 34d }
                });
            expected.AddResultInsRem(
                3200,
                0,
                new object[][] {
                    new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 1d },
                    new object[] { null, 85d }
                },
                new object[][] {
                    new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 1d },
                    new object[] { null, 85d }
                });
            expected.AddResultInsRem(
                4200,
                0,
                new object[][] {
                    new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 3d },
                    new object[] { null, 87d }
                },
                new object[][] {
                    new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 1d },
                    new object[] { null, 85d }
                });
            expected.AddResultInsRem(
                5200,
                0,
                new object[][] {
                    new object[] { "IBM", 97d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 6d },
                    new object[] { null, 112d }
                },
                new object[][] {
                    new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 3d },
                    new object[] { null, 87d }
                });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] {
                    new object[] { "IBM", 72d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 7d },
                    new object[] { null, 88d }
                },
                new object[][] {
                    new object[] { "IBM", 97d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 6d },
                    new object[] { null, 112d }
                });
            expected.AddResultInsRem(
                7200,
                0,
                new object[][] {
                    new object[] { "IBM", 48d }, new object[] { "MSFT", null }, new object[] { "YAH", 6d },
                    new object[] { null, 54d }
                },
                new object[][] {
                    new object[] { "IBM", 72d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 7d },
                    new object[] { null, 88d }
                });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(true, milestone);
        }

        internal class ResultSet4OutputLimitLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertion4OutputLimitLast(env, outputLimitOpt, milestone);
                }
            }
        }

        private static void RunAssertion4OutputLimitLast(
            RegressionEnvironment env,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var stmtText = opt.GetHint() +
                           "@name('s0') select symbol, sum(price) " +
                           "from SupportMarketDataBean#time(5.5 sec)" +
                           "group by rollup(symbol)" +
                           "output last every 1 seconds";
            SendTimer(env, 0);
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = new string[] { "symbol", "sum(price)" };
            var expected = new ResultAssertTestResult("AllOutputLimit", null, fields);
            expected.AddResultInsRem(
                1200,
                0,
                new object[][] { new object[] { "IBM", 25d }, new object[] { "MSFT", 9d }, new object[] { null, 34d } },
                new object[][]
                    { new object[] { "IBM", null }, new object[] { "MSFT", null }, new object[] { null, null } });
            expected.AddResultInsRem(
                2200,
                0,
                new object[][] { new object[] { "IBM", 75d }, new object[] { "YAH", 1d }, new object[] { null, 85d } },
                new object[][]
                    { new object[] { "IBM", 25d }, new object[] { "YAH", null }, new object[] { null, 34d } });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(
                4200,
                0,
                new object[][] { new object[] { "YAH", 3d }, new object[] { null, 87d } },
                new object[][] { new object[] { "YAH", 1d }, new object[] { null, 85d } });
            expected.AddResultInsRem(
                5200,
                0,
                new object[][] { new object[] { "IBM", 97d }, new object[] { "YAH", 6d }, new object[] { null, 112d } },
                new object[][] { new object[] { "IBM", 75d }, new object[] { "YAH", 3d }, new object[] { null, 87d } });
            expected.AddResultInsRem(
                6200,
                0,
                new object[][] { new object[] { "IBM", 72d }, new object[] { "YAH", 7d }, new object[] { null, 88d } },
                new object[][]
                    { new object[] { "IBM", 97d }, new object[] { "YAH", 6d }, new object[] { null, 112d } });
            expected.AddResultInsRem(
                7200,
                0,
                new object[][] {
                    new object[] { "MSFT", null }, new object[] { "IBM", 48d }, new object[] { "YAH", 6d },
                    new object[] { null, 54d }
                },
                new object[][] {
                    new object[] { "MSFT", 9d }, new object[] { "IBM", 72d }, new object[] { "YAH", 7d },
                    new object[] { null, 88d }
                });

            var execution = new ResultAssertExecution(stmtText, env, expected);
            execution.Execute(true, milestone);
        }

        internal class ResultSet5OutputLimitFirst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec)" +
                               "group by rollup(symbol)" +
                               "output first every 1 seconds";
                SendTimer(env, 0);
                env.CompileDeploy(stmtText).AddListener("s0");

                var fields = new string[] { "symbol", "sum(price)" };
                var expected = new ResultAssertTestResult("AllOutputLimit", null, fields);
                expected.AddResultInsRem(
                    200,
                    1,
                    new object[][] { new object[] { "IBM", 25d }, new object[] { null, 25d } },
                    new object[][] { new object[] { "IBM", null }, new object[] { null, null } });
                expected.AddResultInsRem(
                    800,
                    1,
                    new object[][] { new object[] { "MSFT", 9d } },
                    new object[][] { new object[] { "MSFT", null } });
                expected.AddResultInsRem(
                    1500,
                    1,
                    new object[][] { new object[] { "IBM", 49d }, new object[] { null, 58d } },
                    new object[][] { new object[] { "IBM", 25d }, new object[] { null, 34d } });
                expected.AddResultInsRem(
                    1500,
                    2,
                    new object[][] { new object[] { "YAH", 1d } },
                    new object[][] { new object[] { "YAH", null } });
                expected.AddResultInsRem(
                    3500,
                    1,
                    new object[][] { new object[] { "YAH", 3d }, new object[] { null, 87d } },
                    new object[][] { new object[] { "YAH", 1d }, new object[] { null, 85d } });
                expected.AddResultInsRem(
                    4300,
                    1,
                    new object[][] { new object[] { "IBM", 97d } },
                    new object[][] { new object[] { "IBM", 75d } });
                expected.AddResultInsRem(
                    4900,
                    1,
                    new object[][] { new object[] { "YAH", 6d }, new object[] { null, 112d } },
                    new object[][] { new object[] { "YAH", 3d }, new object[] { null, 109d } });
                expected.AddResultInsRem(
                    5700,
                    0,
                    new object[][] { new object[] { "IBM", 72d } },
                    new object[][] { new object[] { "IBM", 97d } });
                expected.AddResultInsRem(
                    5900,
                    1,
                    new object[][] { new object[] { "YAH", 7d }, new object[] { null, 88d } },
                    new object[][] { new object[] { "YAH", 6d }, new object[] { null, 87d } });
                expected.AddResultInsRem(
                    6300,
                    0,
                    new object[][] { new object[] { "MSFT", null } },
                    new object[][] { new object[] { "MSFT", 9d } });
                expected.AddResultInsRem(
                    7000,
                    0,
                    new object[][]
                        { new object[] { "IBM", 48d }, new object[] { "YAH", 6d }, new object[] { null, 54d } },
                    new object[][]
                        { new object[] { "IBM", 72d }, new object[] { "YAH", 7d }, new object[] { null, 79d } });

                var execution = new ResultAssertExecution(stmtText, env, expected);
                execution.Execute(false, new AtomicLong());
            }
        }

        internal class ResultSet6OutputLimitSnapshot : RegressionExecution
        {
            private readonly bool join;

            public ResultSet6OutputLimitSnapshot(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select symbol, sum(price) " +
                               "from SupportMarketDataBean#time(5.5 sec)" +
                               (join ? ",SupportBean#keepall " : " ") +
                               "group by rollup(symbol)" +
                               "output snapshot every 1 seconds";
                SendTimer(env, 0);
                env.CompileDeploy(stmtText).AddListener("s0");
                env.SendEventBean(new SupportBean());

                if (join) { // join has different results
                    env.UndeployAll();
                    return;
                }

                var fields = new string[] { "symbol", "sum(price)" };
                var expected = new ResultAssertTestResult("AllOutputLimit", null, fields);
                expected.AddResultInsert(
                    1200,
                    0,
                    new object[][]
                        { new object[] { "IBM", 25d }, new object[] { "MSFT", 9d }, new object[] { null, 34.0 } });
                expected.AddResultInsert(
                    2200,
                    0,
                    new object[][] {
                        new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 1d },
                        new object[] { null, 85.0 }
                    });
                expected.AddResultInsert(
                    3200,
                    0,
                    new object[][] {
                        new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 1d },
                        new object[] { null, 85.0 }
                    });
                expected.AddResultInsert(
                    4200,
                    0,
                    new object[][] {
                        new object[] { "IBM", 75d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 3d },
                        new object[] { null, 87.0 }
                    });
                expected.AddResultInsert(
                    5200,
                    0,
                    new object[][] {
                        new object[] { "IBM", 97d }, new object[] { "MSFT", 9d }, new object[] { "YAH", 6d },
                        new object[] { null, 112.0 }
                    });
                expected.AddResultInsert(
                    6200,
                    0,
                    new object[][] {
                        new object[] { "MSFT", 9d }, new object[] { "IBM", 72d }, new object[] { "YAH", 7d },
                        new object[] { null, 88.0 }
                    });
                expected.AddResultInsert(
                    7200,
                    0,
                    new object[][]
                        { new object[] { "IBM", 48d }, new object[] { "YAH", 6d }, new object[] { null, 54.0 } });

                var execution = new ResultAssertExecution(stmtText, env, expected);
                execution.Execute(false, new AtomicLong());
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "join=" +
                       join +
                       '}';
            }
        }

        internal class ResultSetOutputFirstHaving : RegressionExecution
        {
            private readonly bool join;

            public ResultSetOutputFirstHaving(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                env.AdvanceTime(0);

                var epl = "@name('s0')" +
                          "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
                          "from SupportBean#time(3.5 sec) " +
                          (join ? ", SupportBean_S0#lastevent " : "") +
                          "group by rollup(theString, intPrimitive) " +
                          "having sum(longPrimitive) > 100 " +
                          "output first every 1 second";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));

                env.Milestone(0);

                env.SendEventBean(MakeEvent("E1", 1, 10L));
                env.SendEventBean(MakeEvent("E1", 2, 20L));
                env.SendEventBean(MakeEvent("E1", 1, 30L));
                env.AdvanceTime(1000);
                env.SendEventBean(MakeEvent("E2", 1, 40L));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(MakeEvent("E1", 2, 50L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", null, 110L }, new object[] { null, null, 150L } },
                    new object[][] { new object[] { "E1", null, 110L }, new object[] { null, null, 150L } });

                // pass 1 second
                env.AdvanceTime(2000);
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(MakeEvent("E1", 1, 60L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", null, 170L }, new object[] { null, null, 210L } },
                    new object[][] { new object[] { "E1", null, 170L }, new object[] { null, null, 210L } });

                // pass 1 second
                env.AdvanceTime(3000);

                env.SendEventBean(MakeEvent("E1", 1, 70L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 170L }, new object[] { "E1", null, 240L },
                        new object[] { null, null, 280L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 170L }, new object[] { "E1", null, 240L },
                        new object[] { null, null, 280L }
                    });

                env.AdvanceTime(4000); // removes the first 3 events
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 130L }, new object[] { "E1", null, 180L },
                        new object[] { null, null, 220L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 130L }, new object[] { "E1", null, 180L },
                        new object[] { null, null, 220L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 80L));
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(5000); // removes the second 2 events
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", null, 210L }, new object[] { null, null, 210L } },
                    new object[][] { new object[] { "E1", null, 210L }, new object[] { null, null, 210L } });

                env.SendEventBean(MakeEvent("E1", 1, 90L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 1, 300L } },
                    new object[][] { new object[] { "E1", 1, 300L } });

                env.AdvanceTime(6000); // removes the third 1 event
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 240L }, new object[] { "E1", null, 240L },
                        new object[] { null, null, 240L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 240L }, new object[] { "E1", null, 240L },
                        new object[] { null, null, 240L }
                    });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "join=" +
                       join +
                       '}';
            }
        }

        internal class ResultSetOutputFirst : RegressionExecution
        {
            private readonly bool join;

            public ResultSetOutputFirst(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                env.AdvanceTime(0);

                var epl = "@name('s0')" +
                          "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
                          "from SupportBean#time(3.5 sec) " +
                          (join ? ", SupportBean_S0#lastevent " : "") +
                          "group by rollup(theString, intPrimitive) " +
                          "output first every 1 second";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));

                env.Milestone(0);

                env.SendEventBean(MakeEvent("E1", 1, 10L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 10L }, new object[] { "E1", null, 10L },
                        new object[] { null, null, 10L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, null }, new object[] { "E1", null, null },
                        new object[] { null, null, null }
                    });

                env.Milestone(1);

                env.SendEventBean(MakeEvent("E1", 2, 20L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 2, 20L } },
                    new object[][] { new object[] { "E1", 2, null } });

                env.Milestone(2);

                // pass 1 second
                env.SendEventBean(MakeEvent("E1", 1, 30L));
                env.AdvanceTime(1000);
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(MakeEvent("E2", 1, 40L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E2", 1, 40L }, new object[] { "E2", null, 40L },
                        new object[] { null, null, 100L }
                    },
                    new object[][] {
                        new object[] { "E2", 1, null }, new object[] { "E2", null, null },
                        new object[] { null, null, 60L }
                    });

                env.Milestone(3);

                env.SendEventBean(MakeEvent("E1", 2, 50L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 2, 70L }, new object[] { "E1", null, 110L } },
                    new object[][] { new object[] { "E1", 2, 20L }, new object[] { "E1", null, 60L } });

                env.Milestone(4);

                // pass 1 second
                env.AdvanceTime(2000);
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(MakeEvent("E1", 1, 60L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 100L }, new object[] { "E1", null, 170L },
                        new object[] { null, null, 210L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 40L }, new object[] { "E1", null, 110L },
                        new object[] { null, null, 150L }
                    });

                env.Milestone(5);

                // pass 1 second
                env.AdvanceTime(3000);

                env.SendEventBean(MakeEvent("E1", 1, 70L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 170L }, new object[] { "E1", null, 240L },
                        new object[] { null, null, 280L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 100L }, new object[] { "E1", null, 170L },
                        new object[] { null, null, 210L }
                    });

                env.AdvanceTime(4000); // removes the first 3 events
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 130L }, new object[] { "E1", 2, 50L },
                        new object[] { "E1", null, 180L }, new object[] { null, null, 220L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 170L }, new object[] { "E1", 2, 70L },
                        new object[] { "E1", null, 240L }, new object[] { null, null, 280L }
                    });

                env.Milestone(6);

                env.SendEventBean(MakeEvent("E1", 1, 80L));
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(5000); // removes the second 2 events
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E2", 1, null }, new object[] { "E1", 2, null },
                        new object[] { "E2", null, null },
                        new object[] { "E1", null, 210L }, new object[] { null, null, 210L }
                    },
                    new object[][] {
                        new object[] { "E2", 1, 40L }, new object[] { "E1", 2, 50L }, new object[] { "E2", null, 40L },
                        new object[] { "E1", null, 260L }, new object[] { null, null, 300L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 90L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 1, 300L } },
                    new object[][] { new object[] { "E1", 1, 210L } });

                env.Milestone(7);

                env.AdvanceTime(6000); // removes the third 1 event
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 240L }, new object[] { "E1", null, 240L },
                        new object[] { null, null, 240L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 300L }, new object[] { "E1", null, 300L },
                        new object[] { null, null, 300L }
                    });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "join=" +
                       join +
                       '}';
            }
        }

        internal class ResultSetOutputFirstSorted : RegressionExecution
        {
            private readonly bool join;

            public ResultSetOutputFirstSorted(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                env.AdvanceTime(0);

                var epl = "@name('s0')" +
                          "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
                          "from SupportBean#time(3.5 sec) " +
                          (join ? ", SupportBean_S0#lastevent " : "") +
                          "group by rollup(theString, intPrimitive) " +
                          "output first every 1 second " +
                          "order by theString, intPrimitive";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));

                env.SendEventBean(MakeEvent("E1", 1, 10L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 10L }, new object[] { "E1", null, 10L },
                        new object[] { "E1", 1, 10L }
                    },
                    new object[][] {
                        new object[] { null, null, null }, new object[] { "E1", null, null },
                        new object[] { "E1", 1, null }
                    });

                env.SendEventBean(MakeEvent("E1", 2, 20L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 2, 20L } },
                    new object[][] { new object[] { "E1", 2, null } });

                // pass 1 second
                env.SendEventBean(MakeEvent("E1", 1, 30L));
                env.AdvanceTime(1000);
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(MakeEvent("E2", 1, 40L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 100L }, new object[] { "E2", null, 40L },
                        new object[] { "E2", 1, 40L }
                    },
                    new object[][] {
                        new object[] { null, null, 60L }, new object[] { "E2", null, null },
                        new object[] { "E2", 1, null }
                    });

                env.SendEventBean(MakeEvent("E1", 2, 50L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", null, 110L }, new object[] { "E1", 2, 70L } },
                    new object[][] { new object[] { "E1", null, 60L }, new object[] { "E1", 2, 20L } });

                // pass 1 second
                env.AdvanceTime(2000);
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(MakeEvent("E1", 1, 60L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { "E1", null, 170L },
                        new object[] { "E1", 1, 100L }
                    },
                    new object[][] {
                        new object[] { null, null, 150L }, new object[] { "E1", null, 110L },
                        new object[] { "E1", 1, 40L }
                    });

                // pass 1 second
                env.AdvanceTime(3000);

                env.SendEventBean(MakeEvent("E1", 1, 70L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 280L }, new object[] { "E1", null, 240L },
                        new object[] { "E1", 1, 170L }
                    },
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { "E1", null, 170L },
                        new object[] { "E1", 1, 100L }
                    });

                env.AdvanceTime(4000); // removes the first 3 events
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 220L }, new object[] { "E1", null, 180L },
                        new object[] { "E1", 1, 130L }, new object[] { "E1", 2, 50L }
                    },
                    new object[][] {
                        new object[] { null, null, 280L }, new object[] { "E1", null, 240L },
                        new object[] { "E1", 1, 170L }, new object[] { "E1", 2, 70L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 80L));
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(5000); // removes the second 2 events
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { "E1", null, 210L },
                        new object[] { "E1", 2, null },
                        new object[] { "E2", null, null }, new object[] { "E2", 1, null }
                    },
                    new object[][] {
                        new object[] { null, null, 300L }, new object[] { "E1", null, 260L },
                        new object[] { "E1", 2, 50L },
                        new object[] { "E2", null, 40L }, new object[] { "E2", 1, 40L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 90L));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 1, 300L } },
                    new object[][] { new object[] { "E1", 1, 210L } });

                env.AdvanceTime(6000); // removes the third 1 event
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 240L }, new object[] { "E1", null, 240L },
                        new object[] { "E1", 1, 240L }
                    },
                    new object[][] {
                        new object[] { null, null, 300L }, new object[] { "E1", null, 300L },
                        new object[] { "E1", 1, 300L }
                    });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "join=" +
                       join +
                       '}';
            }
        }

        internal class ResultSetOutputDefault : RegressionExecution
        {
            private readonly bool join;

            public ResultSetOutputDefault(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                env.AdvanceTime(0);

                var epl = "@name('s0')" +
                          "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
                          "from SupportBean#time(3.5 sec) " +
                          (join ? ", SupportBean_S0#lastevent " : "") +
                          "group by rollup(theString, intPrimitive) " +
                          "output every 1 second";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));

                env.SendEventBean(MakeEvent("E1", 1, 10L));
                env.SendEventBean(MakeEvent("E1", 2, 20L));
                env.SendEventBean(MakeEvent("E1", 1, 30L));
                env.AdvanceTime(1000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 10L }, new object[] { "E1", null, 10L },
                        new object[] { null, null, 10L },
                        new object[] { "E1", 2, 20L }, new object[] { "E1", null, 30L },
                        new object[] { null, null, 30L },
                        new object[] { "E1", 1, 40L }, new object[] { "E1", null, 60L },
                        new object[] { null, null, 60L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, null }, new object[] { "E1", null, null },
                        new object[] { null, null, null },
                        new object[] { "E1", 2, null }, new object[] { "E1", null, 10L },
                        new object[] { null, null, 10L },
                        new object[] { "E1", 1, 10L }, new object[] { "E1", null, 30L },
                        new object[] { null, null, 30L }
                    });

                env.SendEventBean(MakeEvent("E2", 1, 40L));
                env.SendEventBean(MakeEvent("E1", 2, 50L));
                env.AdvanceTime(2000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E2", 1, 40L }, new object[] { "E2", null, 40L },
                        new object[] { null, null, 100L },
                        new object[] { "E1", 2, 70L }, new object[] { "E1", null, 110L },
                        new object[] { null, null, 150L }
                    },
                    new object[][] {
                        new object[] { "E2", 1, null }, new object[] { "E2", null, null },
                        new object[] { null, null, 60L },
                        new object[] { "E1", 2, 20L }, new object[] { "E1", null, 60L },
                        new object[] { null, null, 100L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 60L));
                env.AdvanceTime(3000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 100L }, new object[] { "E1", null, 170L },
                        new object[] { null, null, 210L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 40L }, new object[] { "E1", null, 110L },
                        new object[] { null, null, 150L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 70L)); // removes the first 3 events
                env.AdvanceTimeSpan(4000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 170L }, new object[] { "E1", null, 240L },
                        new object[] { null, null, 280L },
                        new object[] { "E1", 1, 130L }, new object[] { "E1", 2, 50L },
                        new object[] { "E1", null, 180L }, new object[] { null, null, 220L },
                    },
                    new object[][] {
                        new object[] { "E1", 1, 100L }, new object[] { "E1", null, 170L },
                        new object[] { null, null, 210L },
                        new object[] { "E1", 1, 170L }, new object[] { "E1", 2, 70L },
                        new object[] { "E1", null, 240L }, new object[] { null, null, 280L },
                    });

                env.SendEventBean(MakeEvent("E1", 1, 80L)); // removes the second 2 events
                env.AdvanceTimeSpan(5000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 210L }, new object[] { "E1", null, 260L },
                        new object[] { null, null, 300L },
                        new object[] { "E2", 1, null }, new object[] { "E1", 2, null },
                        new object[] { "E2", null, null }, new object[] { "E1", null, 210L },
                        new object[] { null, null, 210L },
                    },
                    new object[][] {
                        new object[] { "E1", 1, 130L }, new object[] { "E1", null, 180L },
                        new object[] { null, null, 220L },
                        new object[] { "E2", 1, 40L }, new object[] { "E1", 2, 50L }, new object[] { "E2", null, 40L },
                        new object[] { "E1", null, 260L }, new object[] { null, null, 300L },
                    });

                env.SendEventBean(MakeEvent("E1", 1, 90L)); // removes the third 1 event
                env.AdvanceTimeSpan(6000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 300L }, new object[] { "E1", null, 300L },
                        new object[] { null, null, 300L },
                        new object[] { "E1", 1, 240L }, new object[] { "E1", null, 240L },
                        new object[] { null, null, 240L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 210L }, new object[] { "E1", null, 210L },
                        new object[] { null, null, 210L },
                        new object[] { "E1", 1, 300L }, new object[] { "E1", null, 300L },
                        new object[] { null, null, 300L }
                    });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "join=" +
                       join +
                       '}';
            }
        }

        internal class ResultSetOutputDefaultSorted : RegressionExecution
        {
            private readonly bool join;

            public ResultSetOutputDefaultSorted(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                env.AdvanceTime(0);

                var epl = "@name('s0')" +
                          "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
                          "from SupportBean#time(3.5 sec) " +
                          (join ? ", SupportBean_S0#lastevent " : "") +
                          "group by rollup(theString, intPrimitive) " +
                          "output every 1 second " +
                          "order by theString, intPrimitive";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));

                env.SendEventBean(MakeEvent("E1", 1, 10L));
                env.SendEventBean(MakeEvent("E1", 2, 20L));
                env.SendEventBean(MakeEvent("E1", 1, 30L));
                env.AdvanceTime(1000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 10L }, new object[] { null, null, 30L },
                        new object[] { null, null, 60L },
                        new object[] { "E1", null, 10L }, new object[] { "E1", null, 30L },
                        new object[] { "E1", null, 60L },
                        new object[] { "E1", 1, 10L }, new object[] { "E1", 1, 40L }, new object[] { "E1", 2, 20L }
                    },
                    new object[][] {
                        new object[] { null, null, null }, new object[] { null, null, 10L },
                        new object[] { null, null, 30L },
                        new object[] { "E1", null, null }, new object[] { "E1", null, 10L },
                        new object[] { "E1", null, 30L },
                        new object[] { "E1", 1, null }, new object[] { "E1", 1, 10L }, new object[] { "E1", 2, null }
                    });

                env.SendEventBean(MakeEvent("E2", 1, 40L));
                env.SendEventBean(MakeEvent("E1", 2, 50L));
                env.AdvanceTime(2000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 100L }, new object[] { null, null, 150L },
                        new object[] { "E1", null, 110L }, new object[] { "E1", 2, 70L },
                        new object[] { "E2", null, 40L }, new object[] { "E2", 1, 40L }
                    },
                    new object[][] {
                        new object[] { null, null, 60L }, new object[] { null, null, 100L },
                        new object[] { "E1", null, 60L }, new object[] { "E1", 2, 20L },
                        new object[] { "E2", null, null }, new object[] { "E2", 1, null }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 60L));
                env.AdvanceTime(3000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { "E1", null, 170L },
                        new object[] { "E1", 1, 100L }
                    },
                    new object[][] {
                        new object[] { null, null, 150L }, new object[] { "E1", null, 110L },
                        new object[] { "E1", 1, 40L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 70L)); // removes the first 3 events
                env.AdvanceTimeSpan(4000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 280L }, new object[] { null, null, 220L },
                        new object[] { "E1", null, 240L }, new object[] { "E1", null, 180L },
                        new object[] { "E1", 1, 170L }, new object[] { "E1", 1, 130L }, new object[] { "E1", 2, 50L }
                    },
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { null, null, 280L },
                        new object[] { "E1", null, 170L }, new object[] { "E1", null, 240L },
                        new object[] { "E1", 1, 100L }, new object[] { "E1", 1, 170L }, new object[] { "E1", 2, 70L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 80L)); // removes the second 2 events
                env.AdvanceTimeSpan(5000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 300L }, new object[] { null, null, 210L },
                        new object[] { "E1", null, 260L }, new object[] { "E1", null, 210L },
                        new object[] { "E1", 1, 210L }, new object[] { "E1", 2, null },
                        new object[] { "E2", null, null }, new object[] { "E2", 1, null }
                    },
                    new object[][] {
                        new object[] { null, null, 220L }, new object[] { null, null, 300L },
                        new object[] { "E1", null, 180L }, new object[] { "E1", null, 260L },
                        new object[] { "E1", 1, 130L }, new object[] { "E1", 2, 50L }, new object[] { "E2", null, 40L },
                        new object[] { "E2", 1, 40L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 90L)); // removes the third 1 event
                env.AdvanceTimeSpan(6000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 300L }, new object[] { null, null, 240L },
                        new object[] { "E1", null, 300L }, new object[] { "E1", null, 240L },
                        new object[] { "E1", 1, 300L }, new object[] { "E1", 1, 240L }
                    },
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { null, null, 300L },
                        new object[] { "E1", null, 210L }, new object[] { "E1", null, 300L },
                        new object[] { "E1", 1, 210L }, new object[] { "E1", 1, 300L }
                    });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "join=" +
                       join +
                       '}';
            }
        }

        internal class ResultSetOutputAll : RegressionExecution
        {
            private readonly bool join;

            public ResultSetOutputAll(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertionOutputAll(env, join, outputLimitOpt);
                }
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "join=" +
                       join +
                       '}';
            }
        }

        private static void RunAssertionOutputAll(
            RegressionEnvironment env,
            bool join,
            SupportOutputLimitOpt opt)
        {
            var fields = "c0,c1,c2".SplitCsv();
            env.AdvanceTime(0);

            var epl = opt.GetHint() +
                      "@name('s0')" +
                      "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
                      "from SupportBean#time(3.5 sec) " +
                      (join ? ", SupportBean_S0#lastevent " : "") +
                      "group by rollup(theString, intPrimitive) " +
                      "output all every 1 second";
            env.CompileDeploy(epl).AddListener("s0");
            env.SendEventBean(new SupportBean_S0(1));

            env.SendEventBean(MakeEvent("E1", 1, 10L));
            env.SendEventBean(MakeEvent("E1", 2, 20L));
            env.SendEventBean(MakeEvent("E1", 1, 30L));
            env.AdvanceTime(1000);
            env.AssertListener(
                "s0",
                listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    listener.GetAndResetDataListsFlattened(),
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 40L }, new object[] { "E1", 2, 20L }, new object[] { "E1", null, 60L },
                        new object[] { null, null, 60L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, null }, new object[] { "E1", 2, null },
                        new object[] { "E1", null, null }, new object[] { null, null, null }
                    }));

            env.SendEventBean(MakeEvent("E2", 1, 40L));
            env.SendEventBean(MakeEvent("E1", 2, 50L));
            env.AdvanceTime(2000);
            env.AssertListener(
                "s0",
                listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    listener.GetAndResetDataListsFlattened(),
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 40L }, new object[] { "E1", 2, 70L }, new object[] { "E2", 1, 40L },
                        new object[] { "E1", null, 110L }, new object[] { "E2", null, 40L },
                        new object[] { null, null, 150L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 40L }, new object[] { "E1", 2, 20L }, new object[] { "E2", 1, null },
                        new object[] { "E1", null, 60L }, new object[] { "E2", null, null },
                        new object[] { null, null, 60L }
                    }));

            env.SendEventBean(MakeEvent("E1", 1, 60L));
            env.AdvanceTime(3000);
            env.AssertListener(
                "s0",
                listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    listener.GetAndResetDataListsFlattened(),
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 100L }, new object[] { "E1", 2, 70L }, new object[] { "E2", 1, 40L },
                        new object[] { "E1", null, 170L }, new object[] { "E2", null, 40L },
                        new object[] { null, null, 210L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 40L }, new object[] { "E1", 2, 70L }, new object[] { "E2", 1, 40L },
                        new object[] { "E1", null, 110L }, new object[] { "E2", null, 40L },
                        new object[] { null, null, 150L }
                    }));

            env.SendEventBean(MakeEvent("E1", 1, 70L)); // removes the first 3 events
            env.AdvanceTimeSpan(4000);
            env.AssertListener(
                "s0",
                listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    listener.GetAndResetDataListsFlattened(),
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 130L }, new object[] { "E1", 2, 50L }, new object[] { "E2", 1, 40L },
                        new object[] { "E1", null, 180L }, new object[] { "E2", null, 40L },
                        new object[] { null, null, 220L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 100L }, new object[] { "E1", 2, 70L }, new object[] { "E2", 1, 40L },
                        new object[] { "E1", null, 170L }, new object[] { "E2", null, 40L },
                        new object[] { null, null, 210L }
                    }));

            env.SendEventBean(MakeEvent("E1", 1, 80L)); // removes the second 2 events
            env.AdvanceTimeSpan(5000);
            env.AssertListener(
                "s0",
                listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    listener.GetAndResetDataListsFlattened(),
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 210L }, new object[] { "E1", 2, null }, new object[] { "E2", 1, null },
                        new object[] { "E1", null, 210L }, new object[] { "E2", null, null },
                        new object[] { null, null, 210L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 130L }, new object[] { "E1", 2, 50L }, new object[] { "E2", 1, 40L },
                        new object[] { "E1", null, 180L }, new object[] { "E2", null, 40L },
                        new object[] { null, null, 220L }
                    }));

            env.SendEventBean(MakeEvent("E1", 1, 90L)); // removes the third 1 event
            env.AdvanceTimeSpan(6000);
            env.AssertListener(
                "s0",
                listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    listener.GetAndResetDataListsFlattened(),
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 240L }, new object[] { "E1", 2, null }, new object[] { "E2", 1, null },
                        new object[] { "E1", null, 240L }, new object[] { "E2", null, null },
                        new object[] { null, null, 240L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 210L }, new object[] { "E1", 2, null }, new object[] { "E2", 1, null },
                        new object[] { "E1", null, 210L }, new object[] { "E2", null, null },
                        new object[] { null, null, 210L }
                    }));

            env.UndeployAll();
        }

        internal class ResultSetOutputAllSorted : RegressionExecution
        {
            private readonly bool join;

            public ResultSetOutputAllSorted(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                env.AdvanceTime(0);

                var epl = "@name('s0')" +
                          "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
                          "from SupportBean#time(3.5 sec) " +
                          (join ? ", SupportBean_S0#lastevent " : "") +
                          "group by rollup(theString, intPrimitive) " +
                          "output all every 1 second " +
                          "order by theString, intPrimitive";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));

                env.SendEventBean(MakeEvent("E1", 1, 10L));
                env.SendEventBean(MakeEvent("E1", 2, 20L));
                env.SendEventBean(MakeEvent("E1", 1, 30L));
                env.AdvanceTime(1000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 60L }, new object[] { "E1", null, 60L },
                        new object[] { "E1", 1, 40L }, new object[] { "E1", 2, 20L }
                    },
                    new object[][] {
                        new object[] { null, null, null }, new object[] { "E1", null, null },
                        new object[] { "E1", 1, null }, new object[] { "E1", 2, null }
                    });

                env.SendEventBean(MakeEvent("E2", 1, 40L));
                env.SendEventBean(MakeEvent("E1", 2, 50L));
                env.AdvanceTime(2000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 150L }, new object[] { "E1", null, 110L },
                        new object[] { "E1", 1, 40L }, new object[] { "E1", 2, 70L }, new object[] { "E2", null, 40L },
                        new object[] { "E2", 1, 40L }
                    },
                    new object[][] {
                        new object[] { null, null, 60L }, new object[] { "E1", null, 60L },
                        new object[] { "E1", 1, 40L }, new object[] { "E1", 2, 20L }, new object[] { "E2", null, null },
                        new object[] { "E2", 1, null }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 60L));
                env.AdvanceTime(3000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { "E1", null, 170L },
                        new object[] { "E1", 1, 100L }, new object[] { "E1", 2, 70L }, new object[] { "E2", null, 40L },
                        new object[] { "E2", 1, 40L }
                    },
                    new object[][] {
                        new object[] { null, null, 150L }, new object[] { "E1", null, 110L },
                        new object[] { "E1", 1, 40L }, new object[] { "E1", 2, 70L }, new object[] { "E2", null, 40L },
                        new object[] { "E2", 1, 40L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 70L)); // removes the first 3 events
                env.AdvanceTimeSpan(4000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 220L }, new object[] { "E1", null, 180L },
                        new object[] { "E1", 1, 130L }, new object[] { "E1", 2, 50L }, new object[] { "E2", null, 40L },
                        new object[] { "E2", 1, 40L }
                    },
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { "E1", null, 170L },
                        new object[] { "E1", 1, 100L }, new object[] { "E1", 2, 70L }, new object[] { "E2", null, 40L },
                        new object[] { "E2", 1, 40L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 80L)); // removes the second 2 events
                env.AdvanceTimeSpan(5000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { "E1", null, 210L },
                        new object[] { "E1", 1, 210L }, new object[] { "E1", 2, null },
                        new object[] { "E2", null, null }, new object[] { "E2", 1, null }
                    },
                    new object[][] {
                        new object[] { null, null, 220L }, new object[] { "E1", null, 180L },
                        new object[] { "E1", 1, 130L }, new object[] { "E1", 2, 50L }, new object[] { "E2", null, 40L },
                        new object[] { "E2", 1, 40L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 90L)); // removes the third 1 event
                env.AdvanceTimeSpan(6000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 240L }, new object[] { "E1", null, 240L },
                        new object[] { "E1", 1, 240L }, new object[] { "E1", 2, null },
                        new object[] { "E2", null, null }, new object[] { "E2", 1, null }
                    },
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { "E1", null, 210L },
                        new object[] { "E1", 1, 210L }, new object[] { "E1", 2, null },
                        new object[] { "E2", null, null }, new object[] { "E2", 1, null }
                    });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "join=" +
                       join +
                       '}';
            }
        }

        internal class ResultSetOutputLast : RegressionExecution
        {
            private readonly bool join;

            public ResultSetOutputLast(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    RunAssertionOutputLast(env, join, outputLimitOpt, milestone);
                }
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "join=" +
                       join +
                       '}';
            }
        }

        private static void RunAssertionOutputLast(
            RegressionEnvironment env,
            bool join,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var fields = "c0,c1,c2".SplitCsv();
            env.AdvanceTime(0);

            var epl = opt.GetHint() +
                      "@name('s0')" +
                      "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
                      "from SupportBean#time(3.5 sec) " +
                      (join ? ", SupportBean_S0#lastevent " : "") +
                      "group by rollup(theString, intPrimitive) " +
                      "output last every 1 second";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1));

            env.SendEventBean(MakeEvent("E1", 1, 10L));
            env.SendEventBean(MakeEvent("E1", 2, 20L));
            env.SendEventBean(MakeEvent("E1", 1, 30L));

            env.MilestoneInc(milestone);

            env.AdvanceTime(1000);
            env.AssertPropsPerRowIRPairFlattened(
                "s0",
                fields,
                new object[][] {
                    new object[] { "E1", 1, 40L }, new object[] { "E1", 2, 20L }, new object[] { "E1", null, 60L },
                    new object[] { null, null, 60L }
                },
                new object[][] {
                    new object[] { "E1", 1, null }, new object[] { "E1", 2, null }, new object[] { "E1", null, null },
                    new object[] { null, null, null }
                });

            env.MilestoneInc(milestone);

            env.SendEventBean(MakeEvent("E2", 1, 40L));
            env.SendEventBean(MakeEvent("E1", 2, 50L));
            env.AdvanceTime(2000);
            env.AssertListener(
                "s0",
                listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    listener.GetAndResetDataListsFlattened(),
                    fields,
                    new object[][] {
                        new object[] { "E2", 1, 40L }, new object[] { "E1", 2, 70L }, new object[] { "E2", null, 40L },
                        new object[] { "E1", null, 110L }, new object[] { null, null, 150L }
                    },
                    new object[][] {
                        new object[] { "E2", 1, null }, new object[] { "E1", 2, 20L },
                        new object[] { "E2", null, null }, new object[] { "E1", null, 60L },
                        new object[] { null, null, 60L }
                    }));

            env.MilestoneInc(milestone);

            env.SendEventBean(MakeEvent("E1", 1, 60L));
            env.AdvanceTime(3000);
            env.AssertPropsPerRowIRPairFlattened(
                "s0",
                fields,
                new object[][] {
                    new object[] { "E1", 1, 100L }, new object[] { "E1", null, 170L }, new object[] { null, null, 210L }
                },
                new object[][] {
                    new object[] { "E1", 1, 40L }, new object[] { "E1", null, 110L }, new object[] { null, null, 150L }
                });

            env.SendEventBean(MakeEvent("E1", 1, 70L));
            env.AdvanceTimeSpan(4000); // removes the first 3 events
            env.AssertListener(
                "s0",
                listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    listener.GetAndResetDataListsFlattened(),
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 130L }, new object[] { "E1", 2, 50L },
                        new object[] { "E1", null, 180L }, new object[] { null, null, 220L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 100L }, new object[] { "E1", 2, 70L },
                        new object[] { "E1", null, 170L }, new object[] { null, null, 210L }
                    }));

            env.MilestoneInc(milestone);

            env.SendEventBean(MakeEvent("E1", 1, 80L));
            env.AdvanceTimeSpan(5000); // removes the second 2 events
            env.AssertListener(
                "s0",
                listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    listener.GetAndResetDataListsFlattened(),
                    fields,
                    new object[][] {
                        new object[] { "E1", 1, 210L }, new object[] { "E2", 1, null }, new object[] { "E1", 2, null },
                        new object[] { "E1", null, 210L }, new object[] { "E2", null, null },
                        new object[] { null, null, 210L }
                    },
                    new object[][] {
                        new object[] { "E1", 1, 130L }, new object[] { "E2", 1, 40L }, new object[] { "E1", 2, 50L },
                        new object[] { "E1", null, 180L }, new object[] { "E2", null, 40L },
                        new object[] { null, null, 220L }
                    }));

            env.SendEventBean(MakeEvent("E1", 1, 90L));
            env.AdvanceTimeSpan(6000); // removes the third 1 event
            env.AssertPropsPerRowIRPairFlattened(
                "s0",
                fields,
                new object[][] {
                    new object[] { "E1", 1, 240L }, new object[] { "E1", null, 240L }, new object[] { null, null, 240L }
                },
                new object[][] {
                    new object[] { "E1", 1, 210L }, new object[] { "E1", null, 210L }, new object[] { null, null, 210L }
                });

            env.UndeployAll();
        }

        internal class ResultSetOutputLastSorted : RegressionExecution
        {
            private readonly bool join;

            public ResultSetOutputLastSorted(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                env.AdvanceTime(0);

                var epl = "@name('s0')" +
                          "select irstream theString as c0, intPrimitive as c1, sum(longPrimitive) as c2 " +
                          "from SupportBean#time(3.5 sec) " +
                          (join ? ", SupportBean_S0#lastevent " : "") +
                          "group by rollup(theString, intPrimitive) " +
                          "output last every 1 second " +
                          "order by theString, intPrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1));

                env.SendEventBean(MakeEvent("E1", 1, 10L));
                env.SendEventBean(MakeEvent("E1", 2, 20L));
                env.SendEventBean(MakeEvent("E1", 1, 30L));
                env.AdvanceTime(1000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 60L }, new object[] { "E1", null, 60L },
                        new object[] { "E1", 1, 40L }, new object[] { "E1", 2, 20L }
                    },
                    new object[][] {
                        new object[] { null, null, null }, new object[] { "E1", null, null },
                        new object[] { "E1", 1, null }, new object[] { "E1", 2, null }
                    });

                env.SendEventBean(MakeEvent("E2", 1, 40L));
                env.SendEventBean(MakeEvent("E1", 2, 50L));
                env.AdvanceTime(2000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 150L }, new object[] { "E1", null, 110L },
                        new object[] { "E1", 2, 70L }, new object[] { "E2", null, 40L }, new object[] { "E2", 1, 40L }
                    },
                    new object[][] {
                        new object[] { null, null, 60L }, new object[] { "E1", null, 60L },
                        new object[] { "E1", 2, 20L }, new object[] { "E2", null, null }, new object[] { "E2", 1, null }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 60L));
                env.AdvanceTime(3000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { "E1", null, 170L },
                        new object[] { "E1", 1, 100L }
                    },
                    new object[][] {
                        new object[] { null, null, 150L }, new object[] { "E1", null, 110L },
                        new object[] { "E1", 1, 40L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 70L)); // removes the first 3 events
                env.AdvanceTimeSpan(4000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 220L }, new object[] { "E1", null, 180L },
                        new object[] { "E1", 1, 130L }, new object[] { "E1", 2, 50L }
                    },
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { "E1", null, 170L },
                        new object[] { "E1", 1, 100L }, new object[] { "E1", 2, 70L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 80L)); // removes the second 2 events
                env.AdvanceTimeSpan(5000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { "E1", null, 210L },
                        new object[] { "E1", 1, 210L }, new object[] { "E1", 2, null },
                        new object[] { "E2", null, null }, new object[] { "E2", 1, null }
                    },
                    new object[][] {
                        new object[] { null, null, 220L }, new object[] { "E1", null, 180L },
                        new object[] { "E1", 1, 130L }, new object[] { "E1", 2, 50L }, new object[] { "E2", null, 40L },
                        new object[] { "E2", 1, 40L }
                    });

                env.SendEventBean(MakeEvent("E1", 1, 90L)); // removes the third 1 event
                env.AdvanceTimeSpan(6000);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, 240L }, new object[] { "E1", null, 240L },
                        new object[] { "E1", 1, 240L }
                    },
                    new object[][] {
                        new object[] { null, null, 210L }, new object[] { "E1", null, 210L },
                        new object[] { "E1", 1, 210L }
                    });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "join=" +
                       join +
                       '}';
            }
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            return sb;
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }
    }
} // end of namespace