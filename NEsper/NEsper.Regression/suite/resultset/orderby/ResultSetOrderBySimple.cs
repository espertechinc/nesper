///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.resultset.orderby
{
    public class ResultSetOrderBySimple
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetOrderByMultiDelivery());
            execs.Add(new ResultSetIterator());
            execs.Add(new ResultSetAcrossJoin());
            execs.Add(new ResultSetDescendingOM());
            execs.Add(new ResultSetDescending());
            execs.Add(new ResultSetExpressions());
            execs.Add(new ResultSetAliasesSimple());
            execs.Add(new ResultSetExpressionsJoin());
            execs.Add(new ResultSetMultipleKeys());
            execs.Add(new ResultSetAliases());
            execs.Add(new ResultSetMultipleKeysJoin());
            execs.Add(new ResultSetSimple());
            execs.Add(new ResultSetSimpleJoin());
            execs.Add(new ResultSetWildcard());
            execs.Add(new ResultSetWildcardJoin());
            execs.Add(new ResultSetNoOutputClauseView());
            execs.Add(new ResultSetNoOutputClauseJoin());
            execs.Add(new ResultSetInvalid());
            return execs;
        }

        private static void AssertOnlyProperties(
            RegressionEnvironment env,
            IList<string> requiredProperties)
        {
            var events = env.Listener("s0").LastNewData;
            if (events == null || events.Length == 0) {
                return;
            }

            var type = events[0].EventType;
            IList<string> actualProperties = new List<string>(Arrays.AsList(type.PropertyNames));
            log.Debug(".assertOnlyProperties actualProperties==" + actualProperties);
            Assert.IsTrue(actualProperties.ContainsAll(requiredProperties));
            actualProperties.RemoveAll(requiredProperties);
            Assert.IsTrue(actualProperties.IsEmpty());
        }

        private static void AssertSymbolsJoinWildCard(
            RegressionEnvironment env,
            IList<string> symbols)
        {
            var events = env.Listener("s0").LastNewData;
            log.Debug(".assertValuesMayConvert event type = " + events[0].EventType);
            log.Debug(".assertValuesMayConvert values: " + symbols);
            log.Debug(".assertValuesMayConvert events.Length==" + events.Length);
            for (var i = 0; i < events.Length; i++) {
                var theEvent = (SupportMarketDataBean) events[i].Get("one");
                Assert.AreEqual(symbols[i], theEvent.Symbol);
            }
        }

        private static void AssertValues<T>(
            RegressionEnvironment env,
            IList<T> values,
            string valueName)
        {
            var events = env.Listener("s0").LastNewData;
            Assert.AreEqual(values.Count, events.Length);
            log.Debug(".assertValuesMayConvert values: " + values);
            for (var i = 0; i < events.Length; i++) {
                log.Debug(".assertValuesMayConvert events[" + i + "]==" + events[i].Get(valueName));
                Assert.AreEqual(values[i], events[i].Get(valueName));
            }
        }

        private static void ClearValuesDropStmt(
            RegressionEnvironment env,
            SymbolPricesVolumes spv)
        {
            env.UndeployAll();
            ClearValues(spv);
        }

        private static void ClearValues(SymbolPricesVolumes spv)
        {
            spv.Prices.Clear();
            spv.Volumes.Clear();
            spv.Symbols.Clear();
        }

        private static void CreateAndSend(
            RegressionEnvironment env,
            string epl,
            AtomicLong milestone)
        {
            env.CompileDeploy(epl).AddListener("s0");
            SendEvent(env, "IBM", 2);
            SendEvent(env, "KGB", 1);
            SendEvent(env, "CMU", 3);
            SendEvent(env, "IBM", 6);

            env.MilestoneInc(milestone);

            SendEvent(env, "CAT", 6);
            SendEvent(env, "CAT", 5);
        }

        private static void OrderValuesByPrice(SymbolPricesVolumes spv)
        {
            spv.Symbols.Insert(0, "KGB");
            spv.Symbols.Insert(1, "IBM");
            spv.Symbols.Insert(2, "CMU");
            spv.Symbols.Insert(3, "CAT");
            spv.Symbols.Insert(4, "IBM");
            spv.Symbols.Insert(5, "CAT");
            spv.Prices.Insert(0, 1d);
            spv.Prices.Insert(1, 2d);
            spv.Prices.Insert(2, 3d);
            spv.Prices.Insert(3, 5d);
            spv.Prices.Insert(4, 6d);
            spv.Prices.Insert(5, 6d);
            spv.Volumes.Insert(0, 0L);
            spv.Volumes.Insert(1, 0L);
            spv.Volumes.Insert(2, 0L);
            spv.Volumes.Insert(3, 0L);
            spv.Volumes.Insert(4, 0L);
            spv.Volumes.Insert(5, 0L);
        }

        private static void OrderValuesByPriceDesc(SymbolPricesVolumes spv)
        {
            spv.Symbols.Insert(0, "IBM");
            spv.Symbols.Insert(1, "CAT");
            spv.Symbols.Insert(2, "CAT");
            spv.Symbols.Insert(3, "CMU");
            spv.Symbols.Insert(4, "IBM");
            spv.Symbols.Insert(5, "KGB");
            spv.Prices.Insert(0, 6d);
            spv.Prices.Insert(1, 6d);
            spv.Prices.Insert(2, 5d);
            spv.Prices.Insert(3, 3d);
            spv.Prices.Insert(4, 2d);
            spv.Prices.Insert(5, 1d);
            spv.Volumes.Insert(0, 0L);
            spv.Volumes.Insert(1, 0L);
            spv.Volumes.Insert(2, 0L);
            spv.Volumes.Insert(3, 0L);
            spv.Volumes.Insert(4, 0L);
            spv.Volumes.Insert(5, 0L);
        }

        private static void OrderValuesByPriceJoin(SymbolPricesVolumes spv)
        {
            spv.Symbols.Insert(0, "KGB");
            spv.Symbols.Insert(1, "IBM");
            spv.Symbols.Insert(2, "CMU");
            spv.Symbols.Insert(3, "CAT");
            spv.Symbols.Insert(4, "CAT");
            spv.Symbols.Insert(5, "IBM");
            spv.Prices.Insert(0, 1d);
            spv.Prices.Insert(1, 2d);
            spv.Prices.Insert(2, 3d);
            spv.Prices.Insert(3, 5d);
            spv.Prices.Insert(4, 6d);
            spv.Prices.Insert(5, 6d);
            spv.Volumes.Insert(0, 0L);
            spv.Volumes.Insert(1, 0L);
            spv.Volumes.Insert(2, 0L);
            spv.Volumes.Insert(3, 0L);
            spv.Volumes.Insert(4, 0L);
            spv.Volumes.Insert(5, 0L);
        }

        private static void OrderValuesByPriceSymbol(SymbolPricesVolumes spv)
        {
            spv.Symbols.Insert(0, "KGB");
            spv.Symbols.Insert(1, "IBM");
            spv.Symbols.Insert(2, "CMU");
            spv.Symbols.Insert(3, "CAT");
            spv.Symbols.Insert(4, "CAT");
            spv.Symbols.Insert(5, "IBM");
            spv.Prices.Insert(0, 1d);
            spv.Prices.Insert(1, 2d);
            spv.Prices.Insert(2, 3d);
            spv.Prices.Insert(3, 5d);
            spv.Prices.Insert(4, 6d);
            spv.Prices.Insert(5, 6d);
            spv.Volumes.Insert(0, 0L);
            spv.Volumes.Insert(1, 0L);
            spv.Volumes.Insert(2, 0L);
            spv.Volumes.Insert(3, 0L);
            spv.Volumes.Insert(4, 0L);
            spv.Volumes.Insert(5, 0L);
        }

        private static void OrderValuesBySymbol(SymbolPricesVolumes spv)
        {
            spv.Symbols.Insert(0, "CAT");
            spv.Symbols.Insert(1, "CAT");
            spv.Symbols.Insert(2, "CMU");
            spv.Symbols.Insert(3, "IBM");
            spv.Symbols.Insert(4, "IBM");
            spv.Symbols.Insert(5, "KGB");
            spv.Prices.Insert(0, 6d);
            spv.Prices.Insert(1, 5d);
            spv.Prices.Insert(2, 3d);
            spv.Prices.Insert(3, 2d);
            spv.Prices.Insert(4, 6d);
            spv.Prices.Insert(5, 1d);
            spv.Volumes.Insert(0, 0L);
            spv.Volumes.Insert(1, 0L);
            spv.Volumes.Insert(2, 0L);
            spv.Volumes.Insert(3, 0L);
            spv.Volumes.Insert(4, 0L);
            spv.Volumes.Insert(5, 0L);
        }

        private static void OrderValuesBySymbolJoin(SymbolPricesVolumes spv)
        {
            spv.Symbols.Insert(0, "CAT");
            spv.Symbols.Insert(1, "CAT");
            spv.Symbols.Insert(2, "CMU");
            spv.Symbols.Insert(3, "IBM");
            spv.Symbols.Insert(4, "IBM");
            spv.Symbols.Insert(5, "KGB");
            spv.Prices.Insert(0, 5d);
            spv.Prices.Insert(1, 6d);
            spv.Prices.Insert(2, 3d);
            spv.Prices.Insert(3, 2d);
            spv.Prices.Insert(4, 6d);
            spv.Prices.Insert(5, 1d);
            spv.Volumes.Insert(0, 0L);
            spv.Volumes.Insert(1, 0L);
            spv.Volumes.Insert(2, 0L);
            spv.Volumes.Insert(3, 0L);
            spv.Volumes.Insert(4, 0L);
            spv.Volumes.Insert(5, 0L);
        }

        private static void OrderValuesBySymbolPrice(SymbolPricesVolumes spv)
        {
            spv.Symbols.Insert(0, "CAT");
            spv.Symbols.Insert(1, "CAT");
            spv.Symbols.Insert(2, "CMU");
            spv.Symbols.Insert(3, "IBM");
            spv.Symbols.Insert(4, "IBM");
            spv.Symbols.Insert(5, "KGB");
            spv.Prices.Insert(0, 5d);
            spv.Prices.Insert(1, 6d);
            spv.Prices.Insert(2, 3d);
            spv.Prices.Insert(3, 2d);
            spv.Prices.Insert(4, 6d);
            spv.Prices.Insert(5, 1d);
            spv.Volumes.Insert(0, 0L);
            spv.Volumes.Insert(1, 0L);
            spv.Volumes.Insert(2, 0L);
            spv.Volumes.Insert(3, 0L);
            spv.Volumes.Insert(4, 0L);
            spv.Volumes.Insert(5, 0L);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        private static void SendTimeEvent(
            RegressionEnvironment env,
            int millis)
        {
            env.AdvanceTime(millis);
        }

        private static void SendJoinEvents(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            env.SendEventBean(new SupportBeanString("CAT"));
            env.SendEventBean(new SupportBeanString("IBM"));
            env.MilestoneInc(milestone);
            env.SendEventBean(new SupportBeanString("CMU"));
            env.SendEventBean(new SupportBeanString("KGB"));
            env.SendEventBean(new SupportBeanString("DOG"));
        }

        internal class ResultSetOrderByMultiDelivery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test for QWY-933597 or ESPER-409
                env.AdvanceTime(0);

                // try pattern
                var epl =
                    "@Name('s0') select a.TheString from pattern [every a=SupportBean(theString like 'A%') => b=SupportBean(theString like 'B%')] order by a.TheString desc";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("A1", 1));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("A2", 2));
                env.SendEventBean(new SupportBean("B", 3));

                var received = env.Listener("s0").NewDataListFlattened;
                Assert.AreEqual(2, received.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    received,
                    "a.TheString".SplitCsv(),
                    new[] {new object[] {"A2"}, new object[] {"A1"}});

                env.UndeployAll();

                // try pattern with output limit
                epl =
                    "@Name('s0') select a.TheString from pattern [every a=SupportBean(theString like 'A%') => b=SupportBean(theString like 'B%')] " +
                    "output every 3 events order by a.TheString desc";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("A1", 1));
                env.SendEventBean(new SupportBean("A2", 2));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("A3", 3));
                env.SendEventBean(new SupportBean("B", 3));

                var receivedThree = env.Listener("s0").NewDataListFlattened;
                Assert.AreEqual(3, receivedThree.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    receivedThree,
                    "a.TheString".SplitCsv(),
                    new[] {new object[] {"A3"}, new object[] {"A2"}, new object[] {"A1"}});

                env.UndeployAll();

                // try grouped time window
                epl =
                    "@Name('s0') select rstream theString from SupportBean#groupwin(TheString)#time(10) order by theString desc";
                env.CompileDeploy(epl).AddListener("s0");

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("A1", 1));
                env.SendEventBean(new SupportBean("A2", 1));

                env.Milestone(2);

                env.AdvanceTime(11000);
                var receivedTwo = env.Listener("s0").NewDataListFlattened;
                Assert.AreEqual(2, receivedTwo.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    receivedTwo,
                    "TheString".SplitCsv(),
                    new[] {new object[] {"A2"}, new object[] {"A1"}});

                env.UndeployAll();
            }
        }

        internal class ResultSetIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select symbol, theString, price from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.symbol = two.TheString " +
                          "order by price";
                env.CompileDeploy(epl).AddListener("s0");

                SendJoinEvents(env, milestone);
                SendEvent(env, "CAT", 50);

                env.MilestoneInc(milestone);

                SendEvent(env, "IBM", 49);
                SendEvent(env, "CAT", 15);
                SendEvent(env, "IBM", 100);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new[] {"symbol", "TheString", "price"},
                    new[] {
                        new object[] {"CAT", "CAT", 15d},
                        new object[] {"IBM", "IBM", 49d},
                        new object[] {"CAT", "CAT", 50d},
                        new object[] {"IBM", "IBM", 100d}
                    });

                env.MilestoneInc(milestone);

                SendEvent(env, "KGB", 75);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new[] {"symbol", "TheString", "price"},
                    new[] {
                        new object[] {"CAT", "CAT", 15d},
                        new object[] {"IBM", "IBM", 49d},
                        new object[] {"CAT", "CAT", 50d},
                        new object[] {"KGB", "KGB", 75d},
                        new object[] {"IBM", "IBM", 100d}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetAcrossJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select symbol, theString from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.symbol = two.TheString " +
                          "output every 6 events " +
                          "order by price";
                var spv = new SymbolPricesVolumes();
                CreateAndSend(env, epl, milestone);

                env.MilestoneInc(milestone);

                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Symbols, "TheString");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "TheString"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by theString, price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesBySymbolPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol"));
                ClearValuesDropStmt(env, spv);

                env.UndeployAll();
            }
        }

        internal class ResultSetDescendingOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "select symbol from " +
                               "SupportMarketDataBean#length(5) " +
                               "output every 6 events " +
                               "order by price desc";

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("symbol");
                model.FromClause = FromClause.Create(
                    FilterStream.Create(typeof(SupportMarketDataBean).Name).AddView("length", Expressions.Constant(5)));
                model.OutputLimitClause = OutputLimitClause.Create(6);
                model.OrderByClause = OrderByClause.Create().Add("price", true);
                model = env.CopyMayFail(model);
                Assert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                SendEvent(env, "IBM", 2);
                SendEvent(env, "KGB", 1);

                env.Milestone(0);

                SendEvent(env, "CMU", 3);
                SendEvent(env, "IBM", 6);
                SendEvent(env, "CAT", 6);

                env.Milestone(1);

                SendEvent(env, "CAT", 5);

                var spv = new SymbolPricesVolumes();
                OrderValuesByPriceDesc(spv);
                AssertValues(env, spv.Symbols, "symbol");

                env.UndeployAll();
            }
        }

        internal class ResultSetDescending : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select symbol from " +
                          "SupportMarketDataBean#length(5) " +
                          "output every 6 events " +
                          "order by price desc";
                CreateAndSend(env, epl, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesByPriceDesc(spv);
                AssertValues(env, spv.Symbols, "symbol");
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by price desc, symbol asc";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                spv.Symbols.Reverse();
                AssertValues(env, spv.Symbols, "symbol");
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by price asc";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, volume from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by symbol desc";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbol(spv);
                spv.Symbols.Reverse();
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Volumes, "volume");
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, price from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by symbol desc, price desc";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbolPrice(spv);
                spv.Symbols.Reverse();
                spv.Prices.Reverse();
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Prices, "price");
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, price from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by symbol, price";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbolPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Prices, "price");
                ClearValuesDropStmt(env, spv);
            }
        }

        internal class ResultSetExpressions : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select symbol from " +
                          "SupportMarketDataBean#length(10) " +
                          "output every 6 events " +
                          "order by (price * 6) + 5";
                CreateAndSend(env, epl, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, price from " +
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
                      "order by (price * 6) + 5, price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "price"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, 1+volume*23 from " +
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
                      "order by (price * 6) + 5, price, volume";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "1+volume*23"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol from " +
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
                      "order by volume*price, symbol";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol"));
                ClearValuesDropStmt(env, spv);
            }
        }

        internal class ResultSetAliasesSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select symbol as mySymbol from " +
                          "SupportMarketDataBean#length(5) " +
                          "output every 6 events " +
                          "order by mySymbol";
                var listener = new SupportUpdateListener();
                CreateAndSend(env, epl, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.Symbols, "mySymbol");
                AssertOnlyProperties(env, Arrays.AsList("mySymbol"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol as mySymbol, price as myPrice from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by myPrice";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "mySymbol");
                AssertValues(env, spv.Prices, "myPrice");
                AssertOnlyProperties(env, Arrays.AsList("mySymbol", "myPrice"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, price as myPrice from " +
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
                      "order by (myPrice * 6) + 5, price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "myPrice"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, 1+volume*23 as myVol from " +
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
                      "order by (price * 6) + 5, price, myVol";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "myVol"));
                ClearValuesDropStmt(env, spv);
            }
        }

        internal class ResultSetExpressionsJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select symbol from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.symbol = two.TheString " +
                          "output every 6 events " +
                          "order by (price * 6) + 5";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, price from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by (price * 6) + 5, price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.Prices, "price");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "price"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, 1+volume*23 from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by (price * 6) + 5, price, volume";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "1+volume*23"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by volume*price, symbol";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol"));
                ClearValuesDropStmt(env, spv);
            }
        }

        internal class ResultSetInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var message = "Aggregate functions in the order-by clause must also occur in the select expression";
                var epl = "@Name('s0') select symbol from " +
                          "SupportMarketDataBean#length(5) " +
                          "output every 6 events " +
                          "order by sum(price)";
                TryInvalidCompile(env, epl, message);

                epl = "@Name('s0') select sum(price) from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by sum(price + 6)";
                TryInvalidCompile(env, epl, message);

                epl = "@Name('s0') select sum(price + 6) from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by sum(price)";
                TryInvalidCompile(env, epl, message);

                epl = "@Name('s0') select symbol from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by sum(price)";
                TryInvalidCompile(env, epl, message);

                epl = "@Name('s0') select sum(price) from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by sum(price + 6)";
                TryInvalidCompile(env, epl, message);

                epl = "@Name('s0') select sum(price + 6) from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by sum(price)";
                TryInvalidCompile(env, epl, message);
            }
        }

        internal class ResultSetMultipleKeys : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select symbol from " +
                          "SupportMarketDataBean#length(10) " +
                          "output every 6 events " +
                          "order by symbol, price";
                CreateAndSend(env, epl, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesBySymbolPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol from " +
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
                      "order by price, symbol, volume";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPriceSymbol(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, volume*2 from " +
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
                      "order by price, volume";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "volume*2"));
                ClearValuesDropStmt(env, spv);
            }
        }

        internal class ResultSetAliases : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select symbol as mySymbol from " +
                          "SupportMarketDataBean#length(5) " +
                          "output every 6 events " +
                          "order by mySymbol";
                CreateAndSend(env, epl, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.Symbols, "mySymbol");
                AssertOnlyProperties(env, Arrays.AsList("mySymbol"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol as mySymbol, price as myPrice from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by myPrice";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "mySymbol");
                AssertValues(env, spv.Prices, "myPrice");
                AssertOnlyProperties(env, Arrays.AsList("mySymbol", "myPrice"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, price as myPrice from " +
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
                      "order by (myPrice * 6) + 5, price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "myPrice"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, 1+volume*23 as myVol from " +
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
                      "order by (price * 6) + 5, price, myVol";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "myVol"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol as mySymbol from " +
                      "SupportMarketDataBean#length(5) " +
                      "order by price, mySymbol";
                CreateAndSend(env, epl, milestone);
                spv.Symbols.Add("CAT");
                AssertValues(env, spv.Symbols, "mySymbol");
                ClearValues(spv);
                SendEvent(env, "FOX", 10);
                spv.Symbols.Add("FOX");
                AssertValues(env, spv.Symbols, "mySymbol");
                ClearValuesDropStmt(env, spv);
            }
        }

        internal class ResultSetMultipleKeysJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select symbol from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.symbol = two.TheString " +
                          "output every 6 events " +
                          "order by symbol, price";
                var spv = new SymbolPricesVolumes();
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesBySymbolPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by price, symbol, volume";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceSymbol(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, volume*2 from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by price, volume";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "volume*2"));
                ClearValuesDropStmt(env, spv);
            }
        }

        internal class ResultSetSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select symbol from " +
                          "SupportMarketDataBean#length(5) " +
                          "output every 6 events " +
                          "order by price";
                var spv = new SymbolPricesVolumes();
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, price from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Prices, "price");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "price"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, volume from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Volumes, "volume");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "volume"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, volume*2 from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Volumes, "volume*2");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "volume*2"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, volume from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by symbol";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Volumes, "volume");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "volume"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select price from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by symbol";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.Prices, "price");
                AssertOnlyProperties(env, Arrays.AsList("price"));
                ClearValuesDropStmt(env, spv);
            }
        }

        internal class ResultSetSimpleJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select symbol from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.symbol = two.TheString " +
                          "output every 6 events " +
                          "order by price";
                var spv = new SymbolPricesVolumes();
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, price from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Prices, "price");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "price"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, volume from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Volumes, "volume");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "volume"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, volume*2 from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Volumes, "volume*2");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "volume*2"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select symbol, volume from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by symbol";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Volumes, "volume");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "volume"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select price from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by symbol, price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesBySymbolJoin(spv);
                AssertValues(env, spv.Prices, "price");
                AssertOnlyProperties(env, Arrays.AsList("price"));
                ClearValuesDropStmt(env, spv);
            }
        }

        internal class ResultSetWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select * from " +
                          "SupportMarketDataBean#length(5) " +
                          "output every 6 events " +
                          "order by price";
                CreateAndSend(env, epl, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesByPrice(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Prices, "price");
                AssertValues(env, spv.Volumes, "volume");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "id", "volume", "price", "feed"));
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select * from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
                      "order by symbol";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.Symbols, "symbol");
                AssertValues(env, spv.Prices, "price");
                AssertValues(env, spv.Volumes, "volume");
                AssertOnlyProperties(env, Arrays.AsList("symbol", "volume", "price", "feed", "id"));
                ClearValuesDropStmt(env, spv);
            }
        }

        internal class ResultSetWildcardJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select * from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.symbol = two.TheString " +
                          "output every 6 events " +
                          "order by price";
                var spv = new SymbolPricesVolumes();
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertSymbolsJoinWildCard(env, spv.Symbols);
                ClearValuesDropStmt(env, spv);

                epl = "@Name('s0') select * from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "output every 6 events " +
                      "order by symbol, price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesBySymbolJoin(spv);
                AssertSymbolsJoinWildCard(env, spv.Symbols);
                ClearValuesDropStmt(env, spv);
            }
        }

        internal class ResultSetNoOutputClauseView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var spv = new SymbolPricesVolumes();
                var epl = "@Name('s0') select symbol from " +
                          "SupportMarketDataBean#length(5) " +
                          "order by price";
                var listener = new SupportUpdateListener();
                CreateAndSend(env, epl, milestone);
                spv.Symbols.Add("CAT");
                AssertValues(env, spv.Symbols, "symbol");
                ClearValues(spv);
                SendEvent(env, "FOX", 10);
                spv.Symbols.Add("FOX");
                AssertValues(env, spv.Symbols, "symbol");
                ClearValuesDropStmt(env, spv);

                // Set start time
                SendTimeEvent(env, 0);

                epl = "@Name('s0') select symbol from " +
                      "SupportMarketDataBean#time_batch(1 sec) " +
                      "order by price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                SendTimeEvent(env, 1000);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol"));
                ClearValuesDropStmt(env, spv);
            }
        }

        internal class ResultSetNoOutputClauseJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select symbol from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.symbol = two.TheString " +
                          "order by price";
                var spv = new SymbolPricesVolumes();
                var listener = new SupportUpdateListener();
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                spv.Symbols.Add("KGB");
                AssertValues(env, spv.Symbols, "symbol");
                ClearValues(spv);
                SendEvent(env, "DOG", 10);
                spv.Symbols.Add("DOG");
                AssertValues(env, spv.Symbols, "symbol");
                ClearValuesDropStmt(env, spv);

                // Set start time
                SendTimeEvent(env, 0);

                epl = "@Name('s0') select symbol from " +
                      "SupportMarketDataBean#time_batch(1) as one, " +
                      "SupportBeanString#length(100) as two " +
                      "where one.symbol = two.TheString " +
                      "order by price, symbol";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                SendTimeEvent(env, 1000);
                AssertValues(env, spv.Symbols, "symbol");
                AssertOnlyProperties(env, Arrays.AsList("symbol"));
                ClearValuesDropStmt(env, spv);
            }
        }

        internal class SymbolPricesVolumes
        {
            internal IList<double> Prices = new List<double>();
            internal IList<string> Symbols = new List<string>();
            internal IList<long> Volumes = new List<long>();
        }
    }
} // end of namespace