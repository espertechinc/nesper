///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.resultset.orderby
{
    public class ResultSetOrderByRowPerEvent
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithIteratorAggregateRowPerEvent(execs);
            WithAliases(execs);
            WithRowPerEventJoinOrderFunction(execs);
            WithRowPerEventOrderFunction(execs);
            WithRowPerEventSum(execs);
            WithRowPerEventMaxSum(execs);
            WithRowPerEventSumHaving(execs);
            WithAggOrderWithSum(execs);
            WithRowPerEventJoin(execs);
            WithRowPerEventJoinMax(execs);
            WithAggHaving(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAggHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventJoinMax(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetRowPerEventJoinMax());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetRowPerEventJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithAggOrderWithSum(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggOrderWithSum());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventSumHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetRowPerEventSumHaving());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventMaxSum(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetRowPerEventMaxSum());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventSum(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetRowPerEventSum());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventOrderFunction(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetRowPerEventOrderFunction());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEventJoinOrderFunction(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetRowPerEventJoinOrderFunction());
            return execs;
        }

        public static IList<RegressionExecution> WithAliases(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAliases());
            return execs;
        }

        public static IList<RegressionExecution> WithIteratorAggregateRowPerEvent(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetIteratorAggregateRowPerEvent());
            return execs;
        }

        private class ResultSetIteratorAggregateRowPerEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "sumPrice" };
                var epl = "@name('s0') select Symbol, sum(Price) as sumPrice from " +
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
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT", 214d },
                        new object[] { "CAT", 214d },
                        new object[] { "IBM", 214d },
                        new object[] { "IBM", 214d },
                    });

                SendEvent(env, "KGB", 75);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT", 289d },
                        new object[] { "CAT", 289d },
                        new object[] { "IBM", 289d },
                        new object[] { "IBM", 289d },
                        new object[] { "KGB", 289d },
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetAliases : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol as mySymbol, sum(Price) as mySum from " +
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
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT", 15.0 }, new object[] { "CAT", 21.0 }, new object[] { "CMU", 8.0 },
                        new object[] { "CMU", 10.0 }, new object[] { "IBM", 3.0 }, new object[] { "IBM", 7.0 }
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetRowPerEventJoinOrderFunction : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol, sum(Price) from " +
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
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT" }, new object[] { "CAT" }, new object[] { "CMU" }, new object[] { "IBM" },
                        new object[] { "IBM" }, new object[] { "KGB" }
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetRowPerEventOrderFunction : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol, sum(Price) from " +
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
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT" }, new object[] { "CAT" }, new object[] { "CMU" }, new object[] { "IBM" },
                        new object[] { "IBM" }, new object[] { "KGB" }
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetRowPerEventSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol, sum(Price) from " +
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

                var fields = "Symbol,sum(Price)".SplitCsv();
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT", 15.0 }, new object[] { "CAT", 21.0 }, new object[] { "CMU", 8.0 },
                        new object[] { "CMU", 10.0 }, new object[] { "IBM", 3.0 }, new object[] { "IBM", 7.0 }
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetRowPerEventMaxSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol, max(sum(Price)) from " +
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

                var fields = "Symbol,max(sum(Price))".SplitCsv();
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT", 15.0 }, new object[] { "CAT", 21.0 }, new object[] { "CMU", 8.0 },
                        new object[] { "CMU", 10.0 }, new object[] { "IBM", 3.0 }, new object[] { "IBM", 7.0 }
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetRowPerEventSumHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol, sum(Price) from " +
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

                var fields = "Symbol,sum(Price)".SplitCsv();
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT", 15.0 }, new object[] { "CAT", 21.0 }, new object[] { "CMU", 8.0 },
                        new object[] { "CMU", 10.0 }, new object[] { "IBM", 3.0 }, new object[] { "IBM", 7.0 }
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetAggOrderWithSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol, sum(Price) from " +
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

                var fields = "Symbol,sum(Price)".SplitCsv();
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT", 15.0 }, new object[] { "CAT", 21.0 }, new object[] { "CMU", 8.0 },
                        new object[] { "CMU", 10.0 }, new object[] { "IBM", 3.0 }, new object[] { "IBM", 7.0 }
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetRowPerEventJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol, sum(Price) from " +
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

                var fields = "Symbol,sum(Price)".SplitCsv();
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT", 11.0 }, new object[] { "CAT", 11.0 }, new object[] { "CMU", 21.0 },
                        new object[] { "CMU", 21.0 }, new object[] { "IBM", 18.0 }, new object[] { "IBM", 18.0 }
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetRowPerEventJoinMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol, max(sum(Price)) from " +
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

                var fields = "Symbol,max(sum(Price))".SplitCsv();
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT", 11.0 }, new object[] { "CAT", 11.0 }, new object[] { "CMU", 21.0 },
                        new object[] { "CMU", 21.0 }, new object[] { "IBM", 18.0 }, new object[] { "IBM", 18.0 }
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetAggHaving : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol, sum(Price) from " +
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

                var fields = "Symbol,sum(Price)".SplitCsv();
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT", 11.0 }, new object[] { "CAT", 11.0 }, new object[] { "CMU", 21.0 },
                        new object[] { "CMU", 21.0 }, new object[] { "IBM", 18.0 }, new object[] { "IBM", 18.0 }
                    });

                env.UndeployAll();
            }
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }
    }
} // end of namespace