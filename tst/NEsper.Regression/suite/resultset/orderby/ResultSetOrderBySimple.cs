///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework; // assertEquals

// assertTrue

namespace com.espertech.esper.regressionlib.suite.resultset.orderby
{
	public class ResultSetOrderBySimple {
	    private static readonly ILog Log = LogManager.GetLogger(typeof(ResultSetOrderBySimple));

	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
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

	    private class ResultSetOrderByMultiDelivery : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            // test for QWY-933597 or ESPER-409
	            var milestone = new AtomicLong();
	            env.AdvanceTime(0);

	            // try pattern
	            var epl = "@name('s0') select a.theString from pattern [every a=SupportBean(theString like 'A%') -> b=SupportBean(theString like 'B%')] order by a.theString desc";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.SendEventBean(new SupportBean("A1", 1));

	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean("A2", 2));
	            env.SendEventBean(new SupportBean("B", 3));

	            env.AssertListener("s0", listener => {
	                var received = listener.NewDataListFlattened;
	                Assert.AreEqual(2, received.Length);
	                EPAssertionUtil.AssertPropsPerRow(received, "a.theString".SplitCsv(), new object[][]{new object[] {"A2"}, new object[] {"A1"}});
	            });

	            env.UndeployAll();

	            // try pattern with output limit
	            epl = "@name('s0') select a.theString from pattern [every a=SupportBean(theString like 'A%') -> b=SupportBean(theString like 'B%')] " +
	                "output every 3 events order by a.theString desc";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.SendEventBean(new SupportBean("A1", 1));
	            env.SendEventBean(new SupportBean("A2", 2));

	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean("A3", 3));
	            env.SendEventBean(new SupportBean("B", 3));

	            env.AssertListener("s0", listener => {
	                var receivedThree = listener.NewDataListFlattened;
	                Assert.AreEqual(3, receivedThree.Length);
	                EPAssertionUtil.AssertPropsPerRow(receivedThree, "a.theString".SplitCsv(), new object[][]{new object[] {"A3"}, new object[] {"A2"}, new object[] {"A1"}});
	            });

	            env.UndeployAll();

	            // try grouped time window
	            epl = "@name('s0') select rstream theString from SupportBean#groupwin(theString)#time(10) order by theString desc";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.AdvanceTime(1000);
	            env.SendEventBean(new SupportBean("A1", 1));
	            env.SendEventBean(new SupportBean("A2", 1));

	            env.MilestoneInc(milestone);

	            env.AdvanceTime(11000);
	            env.AssertListener("s0", listener => {
	                var receivedTwo = listener.NewDataListFlattened;
	                Assert.AreEqual(2, receivedTwo.Length);
	                EPAssertionUtil.AssertPropsPerRow(receivedTwo, "theString".SplitCsv(), new object[][]{new object[] {"A2"}, new object[] {"A1"}});
	            });

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetIterator : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select symbol, theString, price from " +
	                      "SupportMarketDataBean#length(10) as one, " +
	                      "SupportBeanString#length(100) as two " +
	                      "where one.symbol = two.theString " +
	                      "order by price";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendJoinEvents(env, milestone);
	            SendEvent(env, "CAT", 50);

	            env.MilestoneInc(milestone);

	            SendEvent(env, "IBM", 49);
	            SendEvent(env, "CAT", 15);
	            SendEvent(env, "IBM", 100);
	            env.AssertPropsPerRowIterator("s0", new string[]{"symbol", "theString", "price"},
	                new object[][]{
	                    new object[] {"CAT", "CAT", 15d},
	                    new object[] {"IBM", "IBM", 49d},
	                    new object[] {"CAT", "CAT", 50d},
	                    new object[] {"IBM", "IBM", 100d},
	                });

	            env.MilestoneInc(milestone);

	            SendEvent(env, "KGB", 75);
	            env.AssertPropsPerRowIterator("s0", new string[]{"symbol", "theString", "price"},
	                new object[][]{
	                    new object[] {"CAT", "CAT", 15d},
	                    new object[] {"IBM", "IBM", 49d},
	                    new object[] {"CAT", "CAT", 50d},
	                    new object[] {"KGB", "KGB", 75d},
	                    new object[] {"IBM", "IBM", 100d},
	                });

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetAcrossJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select symbol, theString from " +
	                      "SupportMarketDataBean#length(10) as one, " +
	                      "SupportBeanString#length(100) as two " +
	                      "where one.symbol = two.theString " +
	                      "output every 6 events " +
	                      "order by price";
	            var spv = new SymbolPricesVolumes();
	            CreateAndSend(env, epl, milestone);

	            env.MilestoneInc(milestone);

	            SendJoinEvents(env, milestone);
	            OrderValuesByPriceJoin(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.symbols, "theString");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "theString"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by theString, price";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesBySymbolPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol"}));
	            ClearValuesDropStmt(env, spv);

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetDescendingOM : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "select symbol from " +
	                           "SupportMarketDataBean#length(5) " +
	                           "output every 6 events " +
	                           "order by price desc";

	            var model = new EPStatementObjectModel();
	            model.SelectClause = SelectClause.Create("symbol");
	            model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportMarketDataBean)).AddView("length", Expressions.Constant(5)));
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
	            AssertValues(env, spv.symbols, "symbol");

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetDescending : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select symbol from " +
	                      "SupportMarketDataBean#length(5) " +
	                      "output every 6 events " +
	                      "order by price desc";
	            CreateAndSend(env, epl, milestone);
	            var spv = new SymbolPricesVolumes();
	            OrderValuesByPriceDesc(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by price desc, symbol asc";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            spv.symbols.Reverse();
	            AssertValues(env, spv.symbols, "symbol");
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by price asc";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, volume from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by symbol desc";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesBySymbol(spv);
	            spv.symbols.Reverse();
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.volumes, "volume");
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, price from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by symbol desc, price desc";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesBySymbolPrice(spv);
	            spv.symbols.Reverse();
	            spv.prices.Reverse();
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.prices, "price");
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, price from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by symbol, price";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesBySymbolPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.prices, "price");
	            ClearValuesDropStmt(env, spv);
	        }
	    }

	    private class ResultSetExpressions : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select symbol from " +
	                      "SupportMarketDataBean#length(10) " +
	                      "output every 6 events " +
	                      "order by (price * 6) + 5";
	            CreateAndSend(env, epl, milestone);
	            var spv = new SymbolPricesVolumes();
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, price from " +
	                "SupportMarketDataBean#length(10) " +
	                "output every 6 events " +
	                "order by (price * 6) + 5, price";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "price"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, 1+volume*23 from " +
	                "SupportMarketDataBean#length(10) " +
	                "output every 6 events " +
	                "order by (price * 6) + 5, price, volume";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "1+volume*23"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol from " +
	                "SupportMarketDataBean#length(10) " +
	                "output every 6 events " +
	                "order by volume*price, symbol";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesBySymbol(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol"}));
	            ClearValuesDropStmt(env, spv);
	        }
	    }

	    private class ResultSetAliasesSimple : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select symbol as mySymbol from " +
	                      "SupportMarketDataBean#length(5) " +
	                      "output every 6 events " +
	                      "order by mySymbol";
	            var listener = new SupportUpdateListener();
	            CreateAndSend(env, epl, milestone);
	            var spv = new SymbolPricesVolumes();
	            OrderValuesBySymbol(spv);
	            AssertValues(env, spv.symbols, "mySymbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"mySymbol"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol as mySymbol, price as myPrice from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by myPrice";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "mySymbol");
	            AssertValues(env, spv.prices, "myPrice");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"mySymbol", "myPrice"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, price as myPrice from " +
	                "SupportMarketDataBean#length(10) " +
	                "output every 6 events " +
	                "order by (myPrice * 6) + 5, price";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "myPrice"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, 1+volume*23 as myVol from " +
	                "SupportMarketDataBean#length(10) " +
	                "output every 6 events " +
	                "order by (price * 6) + 5, price, myVol";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "myVol"}));
	            ClearValuesDropStmt(env, spv);
	        }
	    }

	    private class ResultSetExpressionsJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select symbol from " +
	                      "SupportMarketDataBean#length(10) as one, " +
	                      "SupportBeanString#length(100) as two " +
	                      "where one.symbol = two.theString " +
	                      "output every 6 events " +
	                      "order by (price * 6) + 5";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            var spv = new SymbolPricesVolumes();
	            OrderValuesByPriceJoin(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, price from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by (price * 6) + 5, price";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesByPriceJoin(spv);
	            AssertValues(env, spv.prices, "price");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "price"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, 1+volume*23 from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by (price * 6) + 5, price, volume";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesByPriceJoin(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "1+volume*23"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by volume*price, symbol";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesBySymbol(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol"}));
	            ClearValuesDropStmt(env, spv);
	        }
	    }

	    private class ResultSetInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var message = "Aggregate functions in the order-by clause must also occur in the select expression";
	            var epl = "@name('s0') select symbol from " +
	                      "SupportMarketDataBean#length(5) " +
	                      "output every 6 events " +
	                      "order by sum(price)";
	            env.TryInvalidCompile(epl, message);

	            epl = "@name('s0') select sum(price) from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by sum(price + 6)";
	            env.TryInvalidCompile(epl, message);

	            epl = "@name('s0') select sum(price + 6) from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by sum(price)";
	            env.TryInvalidCompile(epl, message);

	            epl = "@name('s0') select symbol from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by sum(price)";
	            env.TryInvalidCompile(epl, message);

	            epl = "@name('s0') select sum(price) from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by sum(price + 6)";
	            env.TryInvalidCompile(epl, message);

	            epl = "@name('s0') select sum(price + 6) from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by sum(price)";
	            env.TryInvalidCompile(epl, message);
	        }
	    }

	    private class ResultSetMultipleKeys : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select symbol from " +
	                      "SupportMarketDataBean#length(10) " +
	                      "output every 6 events " +
	                      "order by symbol, price";
	            CreateAndSend(env, epl, milestone);
	            var spv = new SymbolPricesVolumes();
	            OrderValuesBySymbolPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol from " +
	                "SupportMarketDataBean#length(10) " +
	                "output every 6 events " +
	                "order by price, symbol, volume";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPriceSymbol(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, volume*2 from " +
	                "SupportMarketDataBean#length(10) " +
	                "output every 6 events " +
	                "order by price, volume";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "volume*2"}));
	            ClearValuesDropStmt(env, spv);
	        }
	    }

	    private class ResultSetAliases : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select symbol as mySymbol from " +
	                      "SupportMarketDataBean#length(5) " +
	                      "output every 6 events " +
	                      "order by mySymbol";
	            CreateAndSend(env, epl, milestone);
	            var spv = new SymbolPricesVolumes();
	            OrderValuesBySymbol(spv);
	            AssertValues(env, spv.symbols, "mySymbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"mySymbol"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol as mySymbol, price as myPrice from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by myPrice";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "mySymbol");
	            AssertValues(env, spv.prices, "myPrice");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"mySymbol", "myPrice"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, price as myPrice from " +
	                "SupportMarketDataBean#length(10) " +
	                "output every 6 events " +
	                "order by (myPrice * 6) + 5, price";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "myPrice"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, 1+volume*23 as myVol from " +
	                "SupportMarketDataBean#length(10) " +
	                "output every 6 events " +
	                "order by (price * 6) + 5, price, myVol";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "myVol"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol as mySymbol from " +
	                "SupportMarketDataBean#length(5) " +
	                "order by price, mySymbol";
	            CreateAndSend(env, epl, milestone);
	            spv.symbols.Add("CAT");
	            AssertValues(env, spv.symbols, "mySymbol");
	            ClearValues(spv);
	            SendEvent(env, "FOX", 10);
	            spv.symbols.Add("FOX");
	            AssertValues(env, spv.symbols, "mySymbol");
	            ClearValuesDropStmt(env, spv);
	        }
	    }

	    private class ResultSetMultipleKeysJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select symbol from " +
	                      "SupportMarketDataBean#length(10) as one, " +
	                      "SupportBeanString#length(100) as two " +
	                      "where one.symbol = two.theString " +
	                      "output every 6 events " +
	                      "order by symbol, price";
	            var spv = new SymbolPricesVolumes();
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesBySymbolPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by price, symbol, volume";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesByPriceSymbol(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, volume*2 from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by price, volume";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesByPriceJoin(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "volume*2"}));
	            ClearValuesDropStmt(env, spv);
	        }
	    }

	    private class ResultSetSimple : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select symbol from " +
	                      "SupportMarketDataBean#length(5) " +
	                      "output every 6 events " +
	                      "order by price";
	            var spv = new SymbolPricesVolumes();
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, price from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by price";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.prices, "price");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "price"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, volume from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by price";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.volumes, "volume");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "volume"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, volume*2 from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by price";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.volumes, "volume*2");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "volume*2"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, volume from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by symbol";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesBySymbol(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.volumes, "volume");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "volume"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select price from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by symbol";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesBySymbol(spv);
	            AssertValues(env, spv.prices, "price");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"price"}));
	            ClearValuesDropStmt(env, spv);
	        }
	    }

	    private class ResultSetSimpleJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select symbol from " +
	                      "SupportMarketDataBean#length(10) as one, " +
	                      "SupportBeanString#length(100) as two " +
	                      "where one.symbol = two.theString " +
	                      "output every 6 events " +
	                      "order by price";
	            var spv = new SymbolPricesVolumes();
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesByPriceJoin(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, price from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by price";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesByPriceJoin(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.prices, "price");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "price"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, volume from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by price";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesByPriceJoin(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.volumes, "volume");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "volume"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, volume*2 from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by price";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesByPriceJoin(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.volumes, "volume*2");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "volume*2"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select symbol, volume from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by symbol";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesBySymbol(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.volumes, "volume");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "volume"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select price from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by symbol, price";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesBySymbolJoin(spv);
	            AssertValues(env, spv.prices, "price");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"price"}));
	            ClearValuesDropStmt(env, spv);
	        }
	    }

	    private class ResultSetWildcard : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select * from " +
	                      "SupportMarketDataBean#length(5) " +
	                      "output every 6 events " +
	                      "order by price";
	            CreateAndSend(env, epl, milestone);
	            var spv = new SymbolPricesVolumes();
	            OrderValuesByPrice(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.prices, "price");
	            AssertValues(env, spv.volumes, "volume");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "id", "volume", "price", "feed"}));
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select * from " +
	                "SupportMarketDataBean#length(5) " +
	                "output every 6 events " +
	                "order by symbol";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesBySymbol(spv);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertValues(env, spv.prices, "price");
	            AssertValues(env, spv.volumes, "volume");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol", "volume", "price", "feed", "id"}));
	            ClearValuesDropStmt(env, spv);
	        }

	    }

	    private class ResultSetWildcardJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select * from " +
	                      "SupportMarketDataBean#length(10) as one, " +
	                      "SupportBeanString#length(100) as two " +
	                      "where one.symbol = two.theString " +
	                      "output every 6 events " +
	                      "order by price";
	            var spv = new SymbolPricesVolumes();
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesByPriceJoin(spv);
	            AssertSymbolsJoinWildCard(env, spv.symbols);
	            ClearValuesDropStmt(env, spv);

	            epl = "@name('s0') select * from " +
	                "SupportMarketDataBean#length(10) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "output every 6 events " +
	                "order by symbol, price";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesBySymbolJoin(spv);
	            AssertSymbolsJoinWildCard(env, spv.symbols);
	            ClearValuesDropStmt(env, spv);
	        }
	    }

	    private class ResultSetNoOutputClauseView : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var spv = new SymbolPricesVolumes();
	            var epl = "@name('s0') select symbol from " +
	                      "SupportMarketDataBean#length(5) " +
	                      "order by price";
	            var listener = new SupportUpdateListener();
	            CreateAndSend(env, epl, milestone);
	            spv.symbols.Add("CAT");
	            AssertValues(env, spv.symbols, "symbol");
	            ClearValues(spv);
	            SendEvent(env, "FOX", 10);
	            spv.symbols.Add("FOX");
	            AssertValues(env, spv.symbols, "symbol");
	            ClearValuesDropStmt(env, spv);

	            // Set start time
	            SendTimeEvent(env, 0);

	            epl = "@name('s0') select symbol from " +
	                "SupportMarketDataBean#time_batch(1 sec) " +
	                "order by price";
	            CreateAndSend(env, epl, milestone);
	            OrderValuesByPrice(spv);
	            SendTimeEvent(env, 1000);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol"}));
	            ClearValuesDropStmt(env, spv);
	        }
	    }

	    private class ResultSetNoOutputClauseJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select symbol from " +
	                      "SupportMarketDataBean#length(10) as one, " +
	                      "SupportBeanString#length(100) as two " +
	                      "where one.symbol = two.theString " +
	                      "order by price";
	            var spv = new SymbolPricesVolumes();
	            var listener = new SupportUpdateListener();
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            spv.symbols.Add("KGB");
	            AssertValues(env, spv.symbols, "symbol");
	            ClearValues(spv);
	            SendEvent(env, "DOG", 10);
	            spv.symbols.Add("DOG");
	            AssertValues(env, spv.symbols, "symbol");
	            ClearValuesDropStmt(env, spv);

	            // Set start time
	            SendTimeEvent(env, 0);

	            epl = "@name('s0') select symbol from " +
	                "SupportMarketDataBean#time_batch(1) as one, " +
	                "SupportBeanString#length(100) as two " +
	                "where one.symbol = two.theString " +
	                "order by price, symbol";
	            CreateAndSend(env, epl, milestone);
	            SendJoinEvents(env, milestone);
	            OrderValuesByPriceJoin(spv);
	            SendTimeEvent(env, 1000);
	            AssertValues(env, spv.symbols, "symbol");
	            AssertOnlyProperties(env, Arrays.AsList(new string[]{"symbol"}));
	            ClearValuesDropStmt(env, spv);
	        }
	    }

	    private static void AssertOnlyProperties(RegressionEnvironment env, IList<string> requiredProperties) {
	        env.AssertListener("s0", listener => {
	            var events = listener.LastNewData;
	            if (events == null || events.Length == 0) {
	                return;
	            }
	            var type = events[0].EventType;
	            IList<string> actualProperties = new List<string>(Arrays.AsList(type.PropertyNames));
	            Log.Debug(".assertOnlyProperties actualProperties==" + actualProperties);
	            Assert.IsTrue(actualProperties.ContainsAll(requiredProperties));
	            actualProperties.RemoveAll(requiredProperties);
	            Assert.IsTrue(actualProperties.IsEmpty());
	        });
	    }

	    private static void AssertSymbolsJoinWildCard(RegressionEnvironment env, IList<string> symbols) {
	        env.AssertListener("s0", listener => {
	            var events = listener.LastNewData;
	            Log.Debug(".assertValuesMayConvert event type = " + events[0].EventType);
	            Log.Debug(".assertValuesMayConvert values: " + symbols);
	            Log.Debug(".assertValuesMayConvert events.length==" + events.Length);
	            for (var i = 0; i < events.Length; i++) {
	                var theEvent = (SupportMarketDataBean) events[i].Get("one");
	                Assert.AreEqual(symbols[i], theEvent.Symbol);
	            }
	        });
	    }

	    private static void AssertValues<T>(RegressionEnvironment env, IList<T> values, string valueName) {
	        env.AssertListener("s0", listener => {
	            var events = listener.LastNewData;
	            Assert.AreEqual(values.Count, events.Length);
	            Log.Debug(".assertValuesMayConvert values: " + values);
	            for (var i = 0; i < events.Length; i++) {
	                Log.Debug(".assertValuesMayConvert events[" + i + "]==" + events[i].Get(valueName));
	                Assert.AreEqual(values[i], events[i].Get(valueName));
	            }
	        });
	    }

	    private static void ClearValuesDropStmt(RegressionEnvironment env, SymbolPricesVolumes spv) {
	        env.UndeployAll();
	        ClearValues(spv);
	    }

	    private static void ClearValues(SymbolPricesVolumes spv) {
	        spv.prices.Clear();
	        spv.volumes.Clear();
	        spv.symbols.Clear();
	    }

	    private static void CreateAndSend(RegressionEnvironment env, string epl, AtomicLong milestone) {
	        env.CompileDeploy(epl).AddListener("s0");
	        SendEvent(env, "IBM", 2);
	        SendEvent(env, "KGB", 1);
	        SendEvent(env, "CMU", 3);
	        SendEvent(env, "IBM", 6);

	        env.MilestoneInc(milestone);

	        SendEvent(env, "CAT", 6);
	        SendEvent(env, "CAT", 5);
	    }

	    private static void OrderValuesByPrice(SymbolPricesVolumes spv) {
	        spv.symbols.Add("KGB");
	        spv.symbols.Add("IBM");
	        spv.symbols.Add("CMU");
	        spv.symbols.Add("CAT");
	        spv.symbols.Add("IBM");
	        spv.symbols.Add("CAT");
	        spv.prices.Add(1d);
	        spv.prices.Add(2d);
	        spv.prices.Add(3d);
	        spv.prices.Add(5d);
	        spv.prices.Add(6d);
	        spv.prices.Add(6d);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	    }

	    private static void OrderValuesByPriceDesc(SymbolPricesVolumes spv) {
	        spv.symbols.Add("IBM");
	        spv.symbols.Add("CAT");
	        spv.symbols.Add("CAT");
	        spv.symbols.Add("CMU");
	        spv.symbols.Add("IBM");
	        spv.symbols.Add("KGB");
	        spv.prices.Add(6d);
	        spv.prices.Add(6d);
	        spv.prices.Add(5d);
	        spv.prices.Add(3d);
	        spv.prices.Add(2d);
	        spv.prices.Add(1d);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	    }

	    private static void OrderValuesByPriceJoin(SymbolPricesVolumes spv) {
	        spv.symbols.Add("KGB");
	        spv.symbols.Add("IBM");
	        spv.symbols.Add("CMU");
	        spv.symbols.Add("CAT");
	        spv.symbols.Add("CAT");
	        spv.symbols.Add("IBM");
	        spv.prices.Add(1d);
	        spv.prices.Add(2d);
	        spv.prices.Add(3d);
	        spv.prices.Add(5d);
	        spv.prices.Add(6d);
	        spv.prices.Add(6d);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	    }

	    private static void OrderValuesByPriceSymbol(SymbolPricesVolumes spv) {
	        spv.symbols.Add("KGB");
	        spv.symbols.Add("IBM");
	        spv.symbols.Add("CMU");
	        spv.symbols.Add("CAT");
	        spv.symbols.Add("CAT");
	        spv.symbols.Add("IBM");
	        spv.prices.Add(1d);
	        spv.prices.Add(2d);
	        spv.prices.Add(3d);
	        spv.prices.Add(5d);
	        spv.prices.Add(6d);
	        spv.prices.Add(6d);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	    }

	    private static void OrderValuesBySymbol(SymbolPricesVolumes spv) {
	        spv.symbols.Add("CAT");
	        spv.symbols.Add("CAT");
	        spv.symbols.Add("CMU");
	        spv.symbols.Add("IBM");
	        spv.symbols.Add("IBM");
	        spv.symbols.Add("KGB");
	        spv.prices.Add(6d);
	        spv.prices.Add(5d);
	        spv.prices.Add(3d);
	        spv.prices.Add(2d);
	        spv.prices.Add(6d);
	        spv.prices.Add(1d);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	    }

	    private static void OrderValuesBySymbolJoin(SymbolPricesVolumes spv) {
	        spv.symbols.Add("CAT");
	        spv.symbols.Add("CAT");
	        spv.symbols.Add("CMU");
	        spv.symbols.Add("IBM");
	        spv.symbols.Add("IBM");
	        spv.symbols.Add("KGB");
	        spv.prices.Add(5d);
	        spv.prices.Add(6d);
	        spv.prices.Add(3d);
	        spv.prices.Add(2d);
	        spv.prices.Add(6d);
	        spv.prices.Add(1d);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	    }

	    private static void OrderValuesBySymbolPrice(SymbolPricesVolumes spv) {
	        spv.symbols.Add("CAT");
	        spv.symbols.Add("CAT");
	        spv.symbols.Add("CMU");
	        spv.symbols.Add("IBM");
	        spv.symbols.Add("IBM");
	        spv.symbols.Add("KGB");
	        spv.prices.Add(5d);
	        spv.prices.Add(6d);
	        spv.prices.Add(3d);
	        spv.prices.Add(2d);
	        spv.prices.Add(6d);
	        spv.prices.Add(1d);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	        spv.volumes.Add(0L);
	    }

	    private static void SendEvent(RegressionEnvironment env, string symbol, double price) {
	        var bean = new SupportMarketDataBean(symbol, price, 0L, null);
	        env.SendEventBean(bean);
	    }

	    private static void SendTimeEvent(RegressionEnvironment env, int millis) {
	        env.AdvanceTime(millis);
	    }

	    private static void SendJoinEvents(RegressionEnvironment env, AtomicLong milestone) {
	        env.SendEventBean(new SupportBeanString("CAT"));
	        env.SendEventBean(new SupportBeanString("IBM"));
	        env.MilestoneInc(milestone);
	        env.SendEventBean(new SupportBeanString("CMU"));
	        env.SendEventBean(new SupportBeanString("KGB"));
	        env.SendEventBean(new SupportBeanString("DOG"));
	    }

	    private class SymbolPricesVolumes {
		    internal IList<string> symbols = new List<string>();
	        internal IList<double?> prices = new List<double?>();
	        internal IList<long?> volumes = new List<long?>();
	    }
	}
} // end of namespace
