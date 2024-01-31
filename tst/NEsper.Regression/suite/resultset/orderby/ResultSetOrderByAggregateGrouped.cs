///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.resultset.orderby
{
    public class ResultSetOrderByAggregateGrouped
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithAliasesAggregationCompile(execs);
            WithAliasesAggregationOM(execs);
            WithAliases(execs);
            WithGroupBySwitch(execs);
            WithGroupBySwitchJoin(execs);
            WithLastJoin(execs);
            WithIterator(execs);
            WithLast(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithLast(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLast());
            return execs;
        }

        public static IList<RegressionExecution> WithIterator(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetIterator());
            return execs;
        }

        public static IList<RegressionExecution> WithLastJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetLastJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupBySwitchJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetGroupBySwitchJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupBySwitch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetGroupBySwitch());
            return execs;
        }

        public static IList<RegressionExecution> WithAliases(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAliases());
            return execs;
        }

        public static IList<RegressionExecution> WithAliasesAggregationOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAliasesAggregationOM());
            return execs;
        }

        public static IList<RegressionExecution> WithAliasesAggregationCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAliasesAggregationCompile());
            return execs;
        }

        private class ResultSetAliasesAggregationCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol, Volume, sum(Price) as mySum from " +
                          "SupportMarketDataBean#length(20) " +
                          "group by Symbol " +
                          "output every 6 events " +
                          "order by sum(Price), Symbol";
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertionDefault(env);

                env.UndeployAll();
            }
        }

        private class ResultSetAliasesAggregationOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("Symbol", "Volume").Add(Expressions.Sum("Price"), "mySum");
                model.FromClause = FromClause.Create(
                    FilterStream.Create(nameof(SupportMarketDataBean))
                        .AddView(View.Create("length", Expressions.Constant(20))));
                model.GroupByClause = GroupByClause.Create("Symbol");
                model.OutputLimitClause = OutputLimitClause.Create(6);
                model.OrderByClause = OrderByClause.Create(Expressions.Sum("Price")).Add("Symbol", false);
                model = env.CopyMayFail(model);

                var epl = "select Symbol, Volume, sum(Price) as mySum from " +
                          "SupportMarketDataBean#length(20) " +
                          "group by Symbol " +
                          "output every 6 events " +
                          "order by sum(Price), Symbol";
                ClassicAssert.AreEqual(epl, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                TryAssertionDefault(env);

                env.UndeployAll();
            }
        }

        private class ResultSetAliases : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol, Volume, sum(Price) as mySum from " +
                          "SupportMarketDataBean#length(20) " +
                          "group by Symbol " +
                          "output every 6 events " +
                          "order by mySum, Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionDefault(env);

                env.UndeployAll();
            }
        }

        private class ResultSetGroupBySwitch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Instead of the row-per-group behavior, these should
                // get row-per-event behavior since there are properties
                // in the order-by that are not in the select expression.
                var epl = "@name('s0') select Symbol, sum(Price) from " +
                          "SupportMarketDataBean#length(20) " +
                          "group by Symbol " +
                          "output every 6 events " +
                          "order by sum(Price), Symbol, Volume";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionDefaultNoVolume(env);

                env.UndeployAll();
            }
        }

        private class ResultSetGroupBySwitchJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select Symbol, sum(Price) from " +
                    "SupportMarketDataBean#length(20) as one, " +
                    "SupportBeanString#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "group by Symbol " +
                    "output every 6 events " +
                    "order by sum(Price), Symbol, Volume";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString("CAT"));
                env.SendEventBean(new SupportBeanString("IBM"));
                env.SendEventBean(new SupportBeanString("CMU"));
                env.SendEventBean(new SupportBeanString("KGB"));
                env.SendEventBean(new SupportBeanString("DOG"));

                TryAssertionDefaultNoVolume(env);

                env.UndeployAll();
            }
        }

        private class ResultSetLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol, Volume, sum(Price) from " +
                          "SupportMarketDataBean#length(20) " +
                          "group by Symbol " +
                          "output last every 6 events " +
                          "order by sum(Price)";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionLast(env);

                env.UndeployAll();
            }
        }

        private class ResultSetLastJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select Symbol, Volume, sum(Price) from " +
                          "SupportMarketDataBean#length(20) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "group by Symbol " +
                          "output last every 6 events " +
                          "order by sum(Price)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString("CAT"));
                env.SendEventBean(new SupportBeanString("IBM"));
                env.SendEventBean(new SupportBeanString("CMU"));
                env.SendEventBean(new SupportBeanString("KGB"));
                env.SendEventBean(new SupportBeanString("DOG"));

                TryAssertionLast(env);

                env.UndeployAll();
            }
        }

        private class ResultSetIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "Symbol", "TheString", "sumPrice" };
                var epl = "@name('s0') select Symbol, TheString, sum(Price) as sumPrice from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "group by Symbol " +
                          "order by Symbol";
                env.CompileDeploy(epl).AddListener("s0");
                SendJoinEvents(env);
                SendEvent(env, "CAT", 50);
                SendEvent(env, "IBM", 49);
                SendEvent(env, "CAT", 15);
                SendEvent(env, "IBM", 100);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT", "CAT", 65d },
                        new object[] { "CAT", "CAT", 65d },
                        new object[] { "IBM", "IBM", 149d },
                        new object[] { "IBM", "IBM", 149d },
                    });

                SendEvent(env, "KGB", 75);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "CAT", "CAT", 65d },
                        new object[] { "CAT", "CAT", 65d },
                        new object[] { "IBM", "IBM", 149d },
                        new object[] { "IBM", "IBM", 149d },
                        new object[] { "KGB", "KGB", 75d },
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

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            long volume,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            env.SendEventBean(bean);
        }

        private static void SendJoinEvents(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBeanString("CAT"));
            env.SendEventBean(new SupportBeanString("IBM"));
            env.SendEventBean(new SupportBeanString("CMU"));
            env.SendEventBean(new SupportBeanString("KGB"));
            env.SendEventBean(new SupportBeanString("DOG"));
        }

        private static void TryAssertionDefault(RegressionEnvironment env)
        {
            SendEvent(env, "IBM", 110, 3);

            env.Milestone(0);

            SendEvent(env, "IBM", 120, 4);
            SendEvent(env, "CMU", 130, 1);

            env.Milestone(1);

            SendEvent(env, "CMU", 140, 2);
            SendEvent(env, "CAT", 150, 5);

            env.Milestone(2);

            SendEvent(env, "CAT", 160, 6);

            var fields = "Symbol,Volume,mySum".SplitCsv();
            env.AssertPropsPerRowNewOnly(
                "s0",
                fields,
                new object[][] {
                    new object[] { "CMU", 130L, 1.0 }, new object[] { "CMU", 140L, 3.0 },
                    new object[] { "IBM", 110L, 3.0 },
                    new object[] { "CAT", 150L, 5.0 }, new object[] { "IBM", 120L, 7.0 },
                    new object[] { "CAT", 160L, 11.0 }
                });
        }

        private static void TryAssertionDefaultNoVolume(RegressionEnvironment env)
        {
            SendEvent(env, "IBM", 110, 3);
            SendEvent(env, "IBM", 120, 4);
            SendEvent(env, "CMU", 130, 1);

            env.Milestone(0);

            SendEvent(env, "CMU", 140, 2);
            SendEvent(env, "CAT", 150, 5);

            env.Milestone(1);

            SendEvent(env, "CAT", 160, 6);

            var fields = "Symbol,sum(Price)".SplitCsv();
            env.AssertPropsPerRowNewOnly(
                "s0",
                fields,
                new object[][] {
                    new object[] { "CMU", 1.0 }, new object[] { "CMU", 3.0 }, new object[] { "IBM", 3.0 },
                    new object[] { "CAT", 5.0 }, new object[] { "IBM", 7.0 }, new object[] { "CAT", 11.0 }
                });
        }

        private static void TryAssertionLast(RegressionEnvironment env)
        {
            SendEvent(env, "IBM", 101, 3);
            SendEvent(env, "IBM", 102, 4);

            env.Milestone(0);

            SendEvent(env, "CMU", 103, 1);
            SendEvent(env, "CMU", 104, 2);

            env.Milestone(1);

            SendEvent(env, "CAT", 105, 5);
            SendEvent(env, "CAT", 106, 6);

            var fields = "Symbol,Volume,sum(Price)".SplitCsv();
            env.AssertPropsPerRowNewOnly(
                "s0",
                fields,
                new object[][] {
                    new object[] { "CMU", 104L, 3.0 }, new object[] { "IBM", 102L, 7.0 },
                    new object[] { "CAT", 106L, 11.0 }
                });

            SendEvent(env, "IBM", 201, 3);
            SendEvent(env, "IBM", 202, 4);

            env.Milestone(2);

            SendEvent(env, "CMU", 203, 5);
            SendEvent(env, "CMU", 204, 5);
            SendEvent(env, "DOG", 205, 0);
            SendEvent(env, "DOG", 206, 1);

            env.AssertPropsPerRowNewOnly(
                "s0",
                fields,
                new object[][] {
                    new object[] { "DOG", 206L, 1.0 }, new object[] { "CMU", 204L, 13.0 },
                    new object[] { "IBM", 202L, 14.0 }
                });
        }
    }
} // end of namespace