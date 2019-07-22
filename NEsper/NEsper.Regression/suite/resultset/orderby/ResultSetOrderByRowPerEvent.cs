///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.resultset.orderby
{
    public class ResultSetOrderByRowPerEvent
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetIteratorAggregateRowPerEvent());
            execs.Add(new ResultSetAliases());
            execs.Add(new ResultSetRowPerEventJoinOrderFunction());
            execs.Add(new ResultSetRowPerEventOrderFunction());
            execs.Add(new ResultSetRowPerEventSum());
            execs.Add(new ResultSetRowPerEventMaxSum());
            execs.Add(new ResultSetRowPerEventSumHaving());
            execs.Add(new ResultSetAggOrderWithSum());
            execs.Add(new ResultSetRowPerEventJoin());
            execs.Add(new ResultSetRowPerEventJoinMax());
            execs.Add(new ResultSetAggHaving());
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

        internal class ResultSetIteratorAggregateRowPerEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "sumPrice"};
                var epl = "@Name('s0') select Symbol, sum(Price) as sumPrice from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "order by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString("CAT"));
                env.SendEventBean(new SupportBeanString("IBM"));
                env.SendEventBean(new SupportBeanString("KGB"));

                SendEvent(env, "CAT", 50);
                SendEvent(env, "IBM", 49);
                SendEvent(env, "CAT", 15);
                SendEvent(env, "IBM", 100);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"CAT", 214d},
                        new object[] {"CAT", 214d},
                        new object[] {"IBM", 214d},
                        new object[] {"IBM", 214d}
                    });

                SendEvent(env, "KGB", 75);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {
                        new object[] {"CAT", 289d},
                        new object[] {"CAT", 289d},
                        new object[] {"IBM", 289d},
                        new object[] {"IBM", 289d},
                        new object[] {"KGB", 289d}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetAliases : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol as mySymbol, sum(Price) as mySum from " +
                          "SupportMarketDataBean#length(10) " +
                          "output every 6 events " +
                          "order by mySymbol";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "IBM", 3);
                SendEvent(env, "IBM", 4);

                env.Milestone(0);

                SendEvent(env, "CMU", 1);
                SendEvent(env, "CMU", 2);
                SendEvent(env, "CAT", 5);

                env.Milestone(1);

                SendEvent(env, "CAT", 6);

                var fields = "mySymbol,mySum".SplitCsv();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"CAT", 15.0}, new object[] {"CAT", 21.0}, new object[] {"CMU", 8.0},
                        new object[] {"CMU", 10.0}, new object[] {"IBM", 3.0}, new object[] {"IBM", 7.0}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetRowPerEventJoinOrderFunction : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, sum(Price) from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "output every 6 events " +
                          "order by Volume*sum(Price), Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "IBM", 2);

                env.Milestone(0);

                SendEvent(env, "KGB", 1);
                SendEvent(env, "CMU", 3);
                SendEvent(env, "IBM", 6);
                SendEvent(env, "CAT", 6);

                env.Milestone(1);

                SendEvent(env, "CAT", 5);

                env.SendEventBean(new SupportBeanString("CAT"));
                env.SendEventBean(new SupportBeanString("IBM"));
                env.SendEventBean(new SupportBeanString("CMU"));

                env.Milestone(2);

                env.SendEventBean(new SupportBeanString("KGB"));
                env.SendEventBean(new SupportBeanString("DOG"));

                var fields = "Symbol".SplitCsv();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"CAT"}, new object[] {"CAT"}, new object[] {"CMU"}, new object[] {"IBM"},
                        new object[] {"IBM"}, new object[] {"KGB"}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetRowPerEventOrderFunction : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, sum(Price) from " +
                          "SupportMarketDataBean#length(10) " +
                          "output every 6 events " +
                          "order by Volume*sum(Price), Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "IBM", 2);
                SendEvent(env, "KGB", 1);
                SendEvent(env, "CMU", 3);
                SendEvent(env, "IBM", 6);

                env.Milestone(0);

                SendEvent(env, "CAT", 6);
                SendEvent(env, "CAT", 5);

                var fields = "Symbol".SplitCsv();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"CAT"}, new object[] {"CAT"}, new object[] {"CMU"}, new object[] {"IBM"},
                        new object[] {"IBM"}, new object[] {"KGB"}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetRowPerEventSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, sum(Price) from " +
                          "SupportMarketDataBean#length(10) " +
                          "output every 6 events " +
                          "order by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "IBM", 3);
                SendEvent(env, "IBM", 4);
                SendEvent(env, "CMU", 1);
                SendEvent(env, "CMU", 2);

                env.Milestone(0);

                SendEvent(env, "CAT", 5);
                SendEvent(env, "CAT", 6);

                var fields = "symbol,sum(Price)".SplitCsv();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"CAT", 15.0}, new object[] {"CAT", 21.0}, new object[] {"CMU", 8.0},
                        new object[] {"CMU", 10.0}, new object[] {"IBM", 3.0}, new object[] {"IBM", 7.0}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetRowPerEventMaxSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, max(sum(Price)) from " +
                          "SupportMarketDataBean#length(10) " +
                          "output every 6 events " +
                          "order by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "IBM", 3);
                SendEvent(env, "IBM", 4);

                env.Milestone(0);

                SendEvent(env, "CMU", 1);
                SendEvent(env, "CMU", 2);
                SendEvent(env, "CAT", 5);

                env.Milestone(1);

                SendEvent(env, "CAT", 6);

                var fields = "symbol,max(sum(Price))".SplitCsv();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"CAT", 15.0}, new object[] {"CAT", 21.0}, new object[] {"CMU", 8.0},
                        new object[] {"CMU", 10.0}, new object[] {"IBM", 3.0}, new object[] {"IBM", 7.0}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetRowPerEventSumHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, sum(Price) from " +
                          "SupportMarketDataBean#length(10) " +
                          "having sum(Price) > 0 " +
                          "output every 6 events " +
                          "order by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "IBM", 3);
                SendEvent(env, "IBM", 4);
                SendEvent(env, "CMU", 1);

                env.Milestone(0);

                SendEvent(env, "CMU", 2);
                SendEvent(env, "CAT", 5);
                SendEvent(env, "CAT", 6);

                var fields = "symbol,sum(Price)".SplitCsv();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"CAT", 15.0}, new object[] {"CAT", 21.0}, new object[] {"CMU", 8.0},
                        new object[] {"CMU", 10.0}, new object[] {"IBM", 3.0}, new object[] {"IBM", 7.0}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggOrderWithSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, sum(Price) from " +
                          "SupportMarketDataBean#length(10) " +
                          "output every 6 events " +
                          "order by Symbol, sum(Price)";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "IBM", 3);
                SendEvent(env, "IBM", 4);

                env.Milestone(0);

                SendEvent(env, "CMU", 1);
                SendEvent(env, "CMU", 2);
                SendEvent(env, "CAT", 5);
                SendEvent(env, "CAT", 6);

                var fields = "symbol,sum(Price)".SplitCsv();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"CAT", 15.0}, new object[] {"CAT", 21.0}, new object[] {"CMU", 8.0},
                        new object[] {"CMU", 10.0}, new object[] {"IBM", 3.0}, new object[] {"IBM", 7.0}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetRowPerEventJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, sum(Price) from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "output every 6 events " +
                          "order by Symbol, sum(Price)";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "IBM", 3);
                SendEvent(env, "IBM", 4);
                SendEvent(env, "CMU", 1);
                SendEvent(env, "CMU", 2);
                SendEvent(env, "CAT", 5);

                env.Milestone(0);

                SendEvent(env, "CAT", 6);

                env.SendEventBean(new SupportBeanString("CAT"));
                env.SendEventBean(new SupportBeanString("IBM"));
                env.SendEventBean(new SupportBeanString("CMU"));

                var fields = "symbol,sum(Price)".SplitCsv();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"CAT", 11.0}, new object[] {"CAT", 11.0}, new object[] {"CMU", 21.0},
                        new object[] {"CMU", 21.0}, new object[] {"IBM", 18.0}, new object[] {"IBM", 18.0}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetRowPerEventJoinMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, max(sum(Price)) from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "output every 6 events " +
                          "order by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "IBM", 3);
                SendEvent(env, "IBM", 4);
                SendEvent(env, "CMU", 1);
                SendEvent(env, "CMU", 2);

                env.Milestone(0);

                SendEvent(env, "CAT", 5);
                SendEvent(env, "CAT", 6);

                env.SendEventBean(new SupportBeanString("CAT"));
                env.SendEventBean(new SupportBeanString("IBM"));

                env.Milestone(1);

                env.SendEventBean(new SupportBeanString("CMU"));

                var fields = "symbol,max(sum(Price))".SplitCsv();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"CAT", 11.0}, new object[] {"CAT", 11.0}, new object[] {"CMU", 21.0},
                        new object[] {"CMU", 21.0}, new object[] {"IBM", 18.0}, new object[] {"IBM", 18.0}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, sum(Price) from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "having sum(Price) > 0 " +
                          "output every 6 events " +
                          "order by Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                SendEvent(env, "IBM", 3);
                SendEvent(env, "IBM", 4);
                SendEvent(env, "CMU", 1);
                SendEvent(env, "CMU", 2);

                env.Milestone(1);

                SendEvent(env, "CAT", 5);
                SendEvent(env, "CAT", 6);

                env.SendEventBean(new SupportBeanString("CAT"));
                env.SendEventBean(new SupportBeanString("IBM"));
                env.SendEventBean(new SupportBeanString("CMU"));

                var fields = "symbol,sum(Price)".SplitCsv();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {
                        new object[] {"CAT", 11.0}, new object[] {"CAT", 11.0}, new object[] {"CMU", 21.0},
                        new object[] {"CMU", 21.0}, new object[] {"IBM", 18.0}, new object[] {"IBM", 18.0}
                    });

                env.UndeployAll();
            }
        }
    }
} // end of namespace