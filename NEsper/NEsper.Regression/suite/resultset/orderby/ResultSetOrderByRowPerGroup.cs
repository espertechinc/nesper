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

namespace com.espertech.esper.regressionlib.suite.resultset.orderby
{
    public class ResultSetOrderByRowPerGroup
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetNoHavingNoJoin());
            execs.Add(new ResultSetHavingNoJoin());
            execs.Add(new ResultSetNoHavingJoin());
            execs.Add(new ResultSetHavingJoin());
            execs.Add(new ResultSetHavingJoinAlias());
            execs.Add(new ResultSetLast());
            execs.Add(new ResultSetLastJoin());
            execs.Add(new ResultSetIteratorRowPerGroup());
            execs.Add(new ResultSetOrderByLast());
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        private static void TryAssertionLast(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = new [] { "symbol","mysum" };

            SendEvent(env, "IBM", 3);
            SendEvent(env, "IBM", 4);
            SendEvent(env, "CMU", 1);

            env.MilestoneInc(milestone);

            SendEvent(env, "CMU", 2);
            SendEvent(env, "CAT", 5);

            env.MilestoneInc(milestone);

            SendEvent(env, "CAT", 6);

            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {new object[] {"CMU", 3.0}, new object[] {"IBM", 7.0}, new object[] {"CAT", 11.0}});
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastOldData,
                fields,
                new[] {new object[] {"CAT", null}, new object[] {"CMU", null}, new object[] {"IBM", null}});

            SendEvent(env, "IBM", 3);
            SendEvent(env, "IBM", 4);
            SendEvent(env, "CMU", 5);
            SendEvent(env, "CMU", 5);
            SendEvent(env, "DOG", 0);

            env.MilestoneInc(milestone);

            SendEvent(env, "DOG", 1);

            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {new object[] {"DOG", 1.0}, new object[] {"CMU", 13.0}, new object[] {"IBM", 14.0}});
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastOldData,
                fields,
                new[] {new object[] {"DOG", null}, new object[] {"CMU", 3.0}, new object[] {"IBM", 7.0}});
        }

        private static void TryAssertionNoHaving(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = new [] { "symbol","mysum" };

            SendEvent(env, "IBM", 3);
            SendEvent(env, "IBM", 4);

            env.MilestoneInc(milestone);

            SendEvent(env, "CMU", 1);
            SendEvent(env, "CMU", 2);
            SendEvent(env, "CAT", 5);
            SendEvent(env, "CAT", 6);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {
                    new object[] {"CMU", 1.0}, new object[] {"CMU", 3.0}, new object[] {"IBM", 3.0},
                    new object[] {"CAT", 5.0}, new object[] {"IBM", 7.0}, new object[] {"CAT", 11.0}
                });
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastOldData,
                fields,
                new[] {
                    new object[] {"CAT", null}, new object[] {"CMU", null}, new object[] {"IBM", null},
                    new object[] {"CMU", 1.0}, new object[] {"IBM", 3.0}, new object[] {"CAT", 5.0}
                });
            env.Listener("s0").Reset();

            env.MilestoneInc(milestone);

            SendEvent(env, "IBM", 3);
            SendEvent(env, "IBM", 4);
            SendEvent(env, "CMU", 5);
            SendEvent(env, "CMU", 5);
            SendEvent(env, "DOG", 0);

            env.MilestoneInc(milestone);

            SendEvent(env, "DOG", 1);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {
                    new object[] {"DOG", 0.0}, new object[] {"DOG", 1.0}, new object[] {"CMU", 8.0},
                    new object[] {"IBM", 10.0}, new object[] {"CMU", 13.0}, new object[] {"IBM", 14.0}
                });
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastOldData,
                fields,
                new[] {
                    new object[] {"DOG", null}, new object[] {"DOG", 0.0}, new object[] {"CMU", 3.0},
                    new object[] {"IBM", 7.0}, new object[] {"CMU", 8.0}, new object[] {"IBM", 10.0}
                });
        }

        private static void TryAssertionHaving(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = new [] { "symbol","mysum" };

            SendEvent(env, "IBM", 3);
            SendEvent(env, "IBM", 4);
            SendEvent(env, "CMU", 1);
            SendEvent(env, "CMU", 2);
            SendEvent(env, "CAT", 5);

            env.MilestoneInc(milestone);

            SendEvent(env, "CAT", 6);

            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {
                    new object[] {"CMU", 1.0}, new object[] {"CMU", 3.0}, new object[] {"IBM", 3.0},
                    new object[] {"CAT", 5.0}, new object[] {"IBM", 7.0}, new object[] {"CAT", 11.0}
                });
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastOldData,
                fields,
                new[] {new object[] {"CMU", 1.0}, new object[] {"IBM", 3.0}, new object[] {"CAT", 5.0}});
            env.Listener("s0").Reset();

            SendEvent(env, "IBM", 3);

            env.MilestoneInc(milestone);

            SendEvent(env, "IBM", 4);
            SendEvent(env, "CMU", 5);
            SendEvent(env, "CMU", 5);
            SendEvent(env, "DOG", 0);

            env.MilestoneInc(milestone);

            SendEvent(env, "DOG", 1);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {
                    new object[] {"DOG", 1.0}, new object[] {"CMU", 8.0}, new object[] {"IBM", 10.0},
                    new object[] {"CMU", 13.0}, new object[] {"IBM", 14.0}
                });
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastOldData,
                fields,
                new[] {
                    new object[] {"CMU", 3.0}, new object[] {"IBM", 7.0}, new object[] {"CMU", 8.0},
                    new object[] {"IBM", 10.0}
                });
        }

        internal class ResultSetNoHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select irstream Symbol, sum(Price) as mysum from " +
                          "SupportMarketDataBean#length(20) " +
                          "group by Symbol " +
                          "output every 6 events " +
                          "order by sum(Price), Symbol";
                env.CompileDeploy(epl).AddListener("s0");
                TryAssertionNoHaving(env, milestone);
                env.UndeployAll();
            }
        }

        internal class ResultSetHavingNoJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select irstream Symbol, sum(Price) as mysum from " +
                          "SupportMarketDataBean#length(20) " +
                          "group by Symbol " +
                          "having sum(Price) > 0 " +
                          "output every 6 events " +
                          "order by sum(Price), Symbol";
                env.CompileDeploy(epl).AddListener("s0");
                TryAssertionHaving(env, milestone);
                env.UndeployAll();
            }
        }

        internal class ResultSetNoHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select irstream Symbol, sum(Price) as mysum from " +
                          "SupportMarketDataBean#length(20) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "group by Symbol " +
                          "output every 6 events " +
                          "order by sum(Price), Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString("CAT"));
                env.SendEventBean(new SupportBeanString("IBM"));
                env.SendEventBean(new SupportBeanString("CMU"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBeanString("KGB"));
                env.SendEventBean(new SupportBeanString("DOG"));

                TryAssertionNoHaving(env, milestone);

                env.UndeployAll();
            }
        }

        internal class ResultSetHavingJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select irstream Symbol, sum(Price) as mysum from " +
                          "SupportMarketDataBean#length(20) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "group by Symbol " +
                          "having sum(Price) > 0 " +
                          "output every 6 events " +
                          "order by sum(Price), Symbol";

                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString("CAT"));
                env.SendEventBean(new SupportBeanString("IBM"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBeanString("CMU"));
                env.SendEventBean(new SupportBeanString("KGB"));
                env.SendEventBean(new SupportBeanString("DOG"));

                TryAssertionHaving(env, milestone);

                env.UndeployAll();
            }
        }

        internal class ResultSetHavingJoinAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select irstream Symbol, sum(Price) as mysum from " +
                          "SupportMarketDataBean#length(20) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "group by Symbol " +
                          "having sum(Price) > 0 " +
                          "output every 6 events " +
                          "order by mysum, Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString("CAT"));
                env.SendEventBean(new SupportBeanString("IBM"));
                env.SendEventBean(new SupportBeanString("CMU"));
                env.SendEventBean(new SupportBeanString("KGB"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBeanString("DOG"));

                TryAssertionHaving(env, milestone);

                env.UndeployAll();
            }
        }

        internal class ResultSetLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select irstream Symbol, sum(Price) as mysum from " +
                          "SupportMarketDataBean#length(20) " +
                          "group by Symbol " +
                          "output last every 6 events " +
                          "order by sum(Price), Symbol";
                env.CompileDeploy(epl).AddListener("s0");
                TryAssertionLast(env, milestone);
                env.UndeployAll();
            }
        }

        internal class ResultSetLastJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select irstream Symbol, sum(Price) as mysum from " +
                          "SupportMarketDataBean#length(20) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "group by Symbol " +
                          "output last every 6 events " +
                          "order by sum(Price), Symbol";

                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString("CAT"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBeanString("IBM"));
                env.SendEventBean(new SupportBeanString("CMU"));
                env.SendEventBean(new SupportBeanString("KGB"));
                env.SendEventBean(new SupportBeanString("DOG"));

                TryAssertionLast(env, milestone);

                env.UndeployAll();
            }
        }

        internal class ResultSetIteratorRowPerGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                string[] fields = {"Symbol", "sumPrice"};
                var epl = "@Name('s0') select Symbol, sum(Price) as sumPrice from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "group by Symbol " +
                          "order by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString("CAT"));
                env.SendEventBean(new SupportBeanString("IBM"));
                env.SendEventBean(new SupportBeanString("CMU"));
                env.SendEventBean(new SupportBeanString("KGB"));
                env.SendEventBean(new SupportBeanString("DOG"));

                SendEvent(env, "CAT", 50);
                SendEvent(env, "IBM", 49);
                SendEvent(env, "CAT", 15);
                SendEvent(env, "IBM", 100);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"CAT", 65d},
                        new object[] {"IBM", 149d}
                    });

                SendEvent(env, "KGB", 75);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"CAT", 65d},
                        new object[] {"IBM", 149d},
                        new object[] {"KGB", 75d}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetOrderByLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select last(IntPrimitive) as c0, TheString as c1  " +
                          "from SupportBean#length_batch(5) group by TheString order by last(IntPrimitive) desc";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 11));
                env.SendEventBean(new SupportBean("E3", 12));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 13));
                env.SendEventBean(new SupportBean("E1", 14));

                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    new [] { "c0", "c1" },
                    new[] {new object[] {14, "E1"}, new object[] {13, "E2"}, new object[] {12, "E3"}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace