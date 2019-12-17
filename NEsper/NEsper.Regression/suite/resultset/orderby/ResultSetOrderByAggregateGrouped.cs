///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.orderby
{
    public class ResultSetOrderByAggregateGrouped
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetAliasesAggregationCompile());
            execs.Add(new ResultSetAliasesAggregationOM());
            execs.Add(new ResultSetAliases());
            execs.Add(new ResultSetGroupBySwitch());
            execs.Add(new ResultSetGroupBySwitchJoin());
            execs.Add(new ResultSetLastJoin());
            execs.Add(new ResultSetIterator());
            execs.Add(new ResultSetLast());
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

            var fields = new [] { "Symbol","Volume","mySum" };
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {
                    new object[] {"CMU", 130L, 1.0}, new object[] {"CMU", 140L, 3.0}, new object[] {"IBM", 110L, 3.0},
                    new object[] {"CAT", 150L, 5.0}, new object[] {"IBM", 120L, 7.0}, new object[] {"CAT", 160L, 11.0}
                });
            Assert.IsNull(env.Listener("s0").LastOldData);
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

            var fields = new [] { "Symbol","sum(Price)" };
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {
                    new object[] {"CMU", 1.0}, new object[] {"CMU", 3.0}, new object[] {"IBM", 3.0},
                    new object[] {"CAT", 5.0}, new object[] {"IBM", 7.0}, new object[] {"CAT", 11.0}
                });
            Assert.IsNull(env.Listener("s0").LastOldData);
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

            var fields = new [] { "Symbol","Volume","sum(Price)" };
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {
                    new object[] {"CMU", 104L, 3.0}, new object[] {"IBM", 102L, 7.0}, new object[] {"CAT", 106L, 11.0}
                });
            Assert.IsNull(env.Listener("s0").LastOldData);

            SendEvent(env, "IBM", 201, 3);
            SendEvent(env, "IBM", 202, 4);

            env.Milestone(2);

            SendEvent(env, "CMU", 203, 5);
            SendEvent(env, "CMU", 204, 5);
            SendEvent(env, "DOG", 205, 0);
            SendEvent(env, "DOG", 206, 1);

            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {
                    new object[] {"DOG", 206L, 1.0}, new object[] {"CMU", 204L, 13.0}, new object[] {"IBM", 202L, 14.0}
                });
            Assert.IsNull(env.Listener("s0").LastOldData);
        }

        internal class ResultSetAliasesAggregationCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, Volume, sum(Price) as mySum from " +
                          "SupportMarketDataBean#length(20) " +
                          "group by Symbol " +
                          "output every 6 events " +
                          "order by sum(Price), Symbol";
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertionDefault(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetAliasesAggregationOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("Symbol", "Volume").Add(Expressions.Sum("Price"), "mySum");
                model.FromClause = FromClause.Create(
                    FilterStream.Create(typeof(SupportMarketDataBean).Name)
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
                Assert.AreEqual(epl, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                TryAssertionDefault(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetAliases : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, Volume, sum(Price) as mySum from " +
                          "SupportMarketDataBean#length(20) " +
                          "group by Symbol " +
                          "output every 6 events " +
                          "order by mySum, Symbol";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionDefault(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetGroupBySwitch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Instead of the row-per-group behavior, these should
                // get row-per-event behavior since there are properties
                // in the order-by that are not in the select expression.
                var epl = "@Name('s0') select Symbol, sum(Price) from " +
                          "SupportMarketDataBean#length(20) " +
                          "group by Symbol " +
                          "output every 6 events " +
                          "order by sum(Price), Symbol, Volume";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionDefaultNoVolume(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetGroupBySwitchJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select Symbol, sum(Price) from " +
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

        internal class ResultSetLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, Volume, sum(Price) from " +
                          "SupportMarketDataBean#length(20) " +
                          "group by Symbol " +
                          "output last every 6 events " +
                          "order by sum(Price)";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionLast(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetLastJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select Symbol, Volume, sum(Price) from " +
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

        internal class ResultSetIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Symbol", "TheString", "sumPrice"};
                var epl = "@Name('s0') select Symbol, TheString, sum(Price) as sumPrice from " +
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
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"CAT", "CAT", 65d},
                        new object[] {"CAT", "CAT", 65d},
                        new object[] {"IBM", "IBM", 149d},
                        new object[] {"IBM", "IBM", 149d}
                    });

                SendEvent(env, "KGB", 75);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"CAT", "CAT", 65d},
                        new object[] {"CAT", "CAT", 65d},
                        new object[] {"IBM", "IBM", 149d},
                        new object[] {"IBM", "IBM", 149d},
                        new object[] {"KGB", "KGB", 75d}
                    });

                env.UndeployAll();
            }
        }
    }
} // end of namespace