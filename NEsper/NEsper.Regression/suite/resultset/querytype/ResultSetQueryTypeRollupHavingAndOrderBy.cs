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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeRollupHavingAndOrderBy
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeHaving(false));
            execs.Add(new ResultSetQueryTypeHaving(true));
            execs.Add(new ResultSetQueryTypeIteratorWindow(false));
            execs.Add(new ResultSetQueryTypeIteratorWindow(true));
            execs.Add(new ResultSetQueryTypeOrderByTwoCriteriaAsc(false));
            execs.Add(new ResultSetQueryTypeOrderByTwoCriteriaAsc(true));
            execs.Add(new ResultSetQueryTypeUnidirectional());
            execs.Add(new ResultSetQueryTypeOrderByOneCriteriaDesc());
            return execs;
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

        internal class ResultSetQueryTypeIteratorWindow : RegressionExecution
        {
            private readonly bool join;

            public ResultSetQueryTypeIteratorWindow(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1" };
                var epl = "@Name('s0')" +
                          "select TheString as c0, sum(IntPrimitive) as c1 " +
                          "from SupportBean#length(3) " +
                          (join ? ", SupportBean_S0#keepall " : "") +
                          "group by rollup(TheString)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1));

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {null, 1}});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {null, 3}});

                env.SendEventBean(new SupportBean("E1", 3));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 4}, new object[] {"E2", 2}, new object[] {null, 6}});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 4));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", 6}, new object[] {"E1", 3}, new object[] {null, 9}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeUnidirectional : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1", "c2" };

                var epl = "@Name('s0')" +
                          "select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                          "from SupportBean_S0 unidirectional, SupportBean#keepall " +
                          "group by cube(TheString, IntPrimitive)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(MakeEvent("E1", 10, 100));
                env.SendEventBean(MakeEvent("E2", 20, 200));
                env.SendEventBean(MakeEvent("E1", 11, 300));
                env.SendEventBean(MakeEvent("E2", 20, 400));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 100L},
                        new object[] {"E2", 20, 600L},
                        new object[] {"E1", 11, 300L},
                        new object[] {"E1", null, 400L},
                        new object[] {"E2", null, 600L},
                        new object[] {null, 10, 100L},
                        new object[] {null, 20, 600L},
                        new object[] {null, 11, 300L},
                        new object[] {null, null, 1000L}
                    });

                env.SendEventBean(MakeEvent("E1", 10, 1));
                env.SendEventBean(new SupportBean_S0(2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 101L},
                        new object[] {"E2", 20, 600L},
                        new object[] {"E1", 11, 300L},
                        new object[] {"E1", null, 401L},
                        new object[] {"E2", null, 600L},
                        new object[] {null, 10, 101L},
                        new object[] {null, 20, 600L},
                        new object[] {null, 11, 300L},
                        new object[] {null, null, 1001L}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeHaving : RegressionExecution
        {
            private readonly bool join;

            public ResultSetQueryTypeHaving(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                // test having on the aggregation alone
                var fields = new [] { "c0", "c1", "c2" };

                var epl = "@Name('s0')" +
                          "select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                          "from SupportBean#keepall " +
                          (join ? ", SupportBean_S0#lastevent " : "") +
                          "group by rollup(TheString, IntPrimitive)" +
                          "having sum(LongPrimitive) > 1000";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));

                env.SendEventBean(MakeEvent("E1", 10, 100));
                env.SendEventBean(MakeEvent("E2", 20, 200));
                env.SendEventBean(MakeEvent("E1", 11, 300));
                env.SendEventBean(MakeEvent("E2", 20, 400));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(MakeEvent("E1", 11, 500));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {null, null, 1500L}});

                env.SendEventBean(MakeEvent("E2", 20, 600));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 20, 1200L}, new object[] {"E2", null, 1200L},
                        new object[] {null, null, 2100L}
                    });
                env.UndeployAll();

                // test having on the aggregation alone
                var fieldsC0C1 = new [] { "c0", "c1" };
                epl = "@Name('s0')" +
                      "select TheString as c0, sum(IntPrimitive) as c1 " +
                      "from SupportBean#keepall " +
                      (join ? ", SupportBean_S0#lastevent " : "") +
                      "group by rollup(TheString) " +
                      "having " +
                      "(TheString is null and sum(IntPrimitive) > 100) " +
                      "or " +
                      "(TheString is not null and sum(IntPrimitive) > 200)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1));

                env.SendEventBean(new SupportBean("E1", 50));
                env.SendEventBean(new SupportBean("E2", 50));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fieldsC0C1,
                    new[] {new object[] {null, 120}});

                env.SendEventBean(new SupportBean("E3", -300));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E1", 200));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fieldsC0C1,
                    new[] {new object[] {"E1", 250}});

                env.SendEventBean(new SupportBean("E2", 500));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fieldsC0C1,
                    new[] {new object[] {"E2", 570}, new object[] {null, 520}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeOrderByTwoCriteriaAsc : RegressionExecution
        {
            private readonly bool join;

            public ResultSetQueryTypeOrderByTwoCriteriaAsc(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var fields = new [] { "c0", "c1", "c2" };

                var epl = "@Name('s0')" +
                          "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                          "from SupportBean#time_batch(1 sec) " +
                          (join ? ", SupportBean_S0#lastevent " : "") +
                          "group by rollup(TheString, IntPrimitive) " +
                          "order by TheString, IntPrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1));

                env.SendEventBean(MakeEvent("E2", 10, 100));
                env.SendEventBean(MakeEvent("E1", 11, 200));

                env.Milestone(0);

                env.SendEventBean(MakeEvent("E1", 10, 300));
                env.SendEventBean(MakeEvent("E1", 11, 400));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.AdvanceTime(1000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {null, null, 1000L},
                        new object[] {"E1", null, 900L},
                        new object[] {"E1", 10, 300L},
                        new object[] {"E1", 11, 600L},
                        new object[] {"E2", null, 100L},
                        new object[] {"E2", 10, 100L}
                    },
                    new[] {
                        new object[] {null, null, null},
                        new object[] {"E1", null, null},
                        new object[] {"E1", 10, null},
                        new object[] {"E1", 11, null},
                        new object[] {"E2", null, null},
                        new object[] {"E2", 10, null}
                    });

                env.SendEventBean(MakeEvent("E1", 11, 500));
                env.SendEventBean(MakeEvent("E1", 10, 600));
                env.SendEventBean(MakeEvent("E1", 12, 700));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                env.AdvanceTime(2000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {null, null, 1800L},
                        new object[] {"E1", null, 1800L},
                        new object[] {"E1", 10, 600L},
                        new object[] {"E1", 11, 500L},
                        new object[] {"E1", 12, 700L},
                        new object[] {"E2", null, null},
                        new object[] {"E2", 10, null}
                    },
                    new[] {
                        new object[] {null, null, 1000L},
                        new object[] {"E1", null, 900L},
                        new object[] {"E1", 10, 300L},
                        new object[] {"E1", 11, 600L},
                        new object[] {"E1", 12, null},
                        new object[] {"E2", null, 100L},
                        new object[] {"E2", 10, 100L}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeOrderByOneCriteriaDesc : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var fields = new [] { "c0", "c1", "c2" };

                var epl = "@Name('s0')" +
                          "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean#time_batch(1 sec) " +
                          "group by rollup(TheString, IntPrimitive) " +
                          "order by TheString desc;";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(MakeEvent("E2", 10, 100));
                env.SendEventBean(MakeEvent("E1", 11, 200));
                env.SendEventBean(MakeEvent("E1", 10, 300));

                env.Milestone(0);

                env.SendEventBean(MakeEvent("E1", 11, 400));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(1000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E2", 10, 100L},
                        new object[] {"E2", null, 100L},
                        new object[] {"E1", 11, 600L},
                        new object[] {"E1", 10, 300L},
                        new object[] {"E1", null, 900L},
                        new object[] {null, null, 1000L}
                    },
                    new[] {
                        new object[] {"E2", 10, null},
                        new object[] {"E2", null, null},
                        new object[] {"E1", 11, null},
                        new object[] {"E1", 10, null},
                        new object[] {"E1", null, null},
                        new object[] {null, null, null}
                    });

                env.SendEventBean(MakeEvent("E1", 11, 500));
                env.SendEventBean(MakeEvent("E1", 10, 600));

                env.Milestone(1);

                env.SendEventBean(MakeEvent("E1", 12, 700));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(2000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E2", 10, null},
                        new object[] {"E2", null, null},
                        new object[] {"E1", 11, 500L},
                        new object[] {"E1", 10, 600L},
                        new object[] {"E1", 12, 700L},
                        new object[] {"E1", null, 1800L},
                        new object[] {null, null, 1800L}
                    },
                    new[] {
                        new object[] {"E2", 10, 100L},
                        new object[] {"E2", null, 100L},
                        new object[] {"E1", 11, 600L},
                        new object[] {"E1", 10, 300L},
                        new object[] {"E1", 12, null},
                        new object[] {"E1", null, 900L},
                        new object[] {null, null, 1000L}
                    });

                env.UndeployAll();
            }
        }
    }
} // end of namespace