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
    public class ResultSetOrderBySimple
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResultSetOrderBySimple));

        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithOrderByMultiDelivery(execs);
            WithIterator(execs);
            WithAcrossJoin(execs);
            WithDescendingOM(execs);
            WithDescending(execs);
            WithExpressions(execs);
            WithAliasesSimple(execs);
            WithExpressionsJoin(execs);
            WithMultipleKeys(execs);
            WithAliases(execs);
            WithMultipleKeysJoin(execs);
            WithSimple(execs);
            WithSimpleJoin(execs);
            WithWildcard(execs);
            WithWildcardJoin(execs);
            WithNoOutputClauseView(execs);
            WithNoOutputClauseJoin(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithNoOutputClauseJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetNoOutputClauseJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithNoOutputClauseView(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetNoOutputClauseView());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcardJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetWildcardJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcard(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetWildcard());
            return execs;
        }

        public static IList<RegressionExecution> WithSimpleJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetSimpleJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithMultipleKeysJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetMultipleKeysJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithAliases(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAliases());
            return execs;
        }

        public static IList<RegressionExecution> WithMultipleKeys(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetMultipleKeys());
            return execs;
        }

        public static IList<RegressionExecution> WithExpressionsJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetExpressionsJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithAliasesSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAliasesSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithExpressions(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetExpressions());
            return execs;
        }

        public static IList<RegressionExecution> WithDescending(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetDescending());
            return execs;
        }

        public static IList<RegressionExecution> WithDescendingOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetDescendingOM());
            return execs;
        }

        public static IList<RegressionExecution> WithAcrossJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAcrossJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithIterator(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetIterator());
            return execs;
        }

        public static IList<RegressionExecution> WithOrderByMultiDelivery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetOrderByMultiDelivery());
            return execs;
        }

        private class ResultSetOrderByMultiDelivery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test for QWY-933597 or ESPER-409
                var milestone = new AtomicLong();
                env.AdvanceTime(0);

                // try pattern
                var epl =
"@name('s0') select a.TheString from pattern [every a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%')] Order by a.TheString desc";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("A1", 1));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A2", 2));
                env.SendEventBean(new SupportBean("B", 3));

                env.AssertListener(
                    "s0",
                    listener => {
                        var received = listener.NewDataListFlattened;
                        Assert.AreEqual(2, received.Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            received,
                            "a.TheString".SplitCsv(),
                            new object[][] { new object[] { "A2" }, new object[] { "A1" } });
                    });

                env.UndeployAll();

                // try pattern with output limit
                epl =
                    "@name('s0') select a.TheString from pattern [every a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%')] " +
"output every 3 events Order by a.TheString desc";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("A1", 1));
                env.SendEventBean(new SupportBean("A2", 2));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("A3", 3));
                env.SendEventBean(new SupportBean("B", 3));

                env.AssertListener(
                    "s0",
                    listener => {
                        var receivedThree = listener.NewDataListFlattened;
                        Assert.AreEqual(3, receivedThree.Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            receivedThree,
                            "a.TheString".SplitCsv(),
                            new object[][] { new object[] { "A3" }, new object[] { "A2" }, new object[] { "A1" } });
                    });

                env.UndeployAll();

                // try grouped time window
                epl =
"@name('s0') select rstream TheString from SupportBean#groupwin(TheString)#time(10) Order by TheString desc";
                env.CompileDeploy(epl).AddListener("s0");

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("A1", 1));
                env.SendEventBean(new SupportBean("A2", 1));

                env.MilestoneInc(milestone);

                env.AdvanceTime(11000);
                env.AssertListener(
                    "s0",
                    listener => {
                        var receivedTwo = listener.NewDataListFlattened;
                        Assert.AreEqual(2, receivedTwo.Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            receivedTwo,
                            "TheString".SplitCsv(),
                            new object[][] { new object[] { "A2" }, new object[] { "A1" } });
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select Symbol, TheString, Price from "+
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
"Order by Price";
                env.CompileDeploy(epl).AddListener("s0");

                SendJoinEvents(env, milestone);
                SendEvent(env, "CAT", 50);

                env.MilestoneInc(milestone);

                SendEvent(env, "IBM", 49);
                SendEvent(env, "CAT", 15);
                SendEvent(env, "IBM", 100);
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "Symbol", "TheString", "Price" },
                    new object[][] {
                        new object[] { "CAT", "CAT", 15d },
                        new object[] { "IBM", "IBM", 49d },
                        new object[] { "CAT", "CAT", 50d },
                        new object[] { "IBM", "IBM", 100d },
                    });

                env.MilestoneInc(milestone);

                SendEvent(env, "KGB", 75);
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "Symbol", "TheString", "Price" },
                    new object[][] {
                        new object[] { "CAT", "CAT", 15d },
                        new object[] { "IBM", "IBM", 49d },
                        new object[] { "CAT", "CAT", 50d },
                        new object[] { "KGB", "KGB", 75d },
                        new object[] { "IBM", "IBM", 100d },
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetAcrossJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select Symbol, TheString from "+
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                          "output every 6 events " +
"Order by Price";
                var spv = new SymbolPricesVolumes();
                CreateAndSend(env, epl, milestone);

                env.MilestoneInc(milestone);

                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.symbols, "TheString");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "TheString" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol from "+
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by TheString, Price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesBySymbolPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol"}));
                ClearValuesDropStmt(env, spv);

                env.UndeployAll();
            }
        }

        private class ResultSetDescendingOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "select Symbol from "+
                               "SupportMarketDataBean#length(5) " +
                               "output every 6 events " +
"Order by Price desc";

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("Symbol");
                model.FromClause = FromClause.Create(
                    FilterStream.Create(nameof(SupportMarketDataBean)).AddView("length", Expressions.Constant(5)));
                model.OutputLimitClause = OutputLimitClause.Create(6);
                model.OrderByClause = OrderByClause.Create().Add("Price", true);
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
                AssertValues(env, spv.symbols, "Symbol");

                env.UndeployAll();
            }
        }

        private class ResultSetDescending : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select Symbol from "+
                          "SupportMarketDataBean#length(5) " +
                          "output every 6 events " +
"Order by Price desc";
                CreateAndSend(env, epl, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesByPriceDesc(spv);
                AssertValues(env, spv.symbols, "Symbol");
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol from "+
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by Price desc, Symbol asc";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                spv.symbols.Reverse();
                AssertValues(env, spv.symbols, "Symbol");
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol from "+
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by Price asc";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Volume from "+
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by Symbol desc";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbol(spv);
                spv.symbols.Reverse();
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.volumes, "Volume");
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Price from "+
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by Symbol desc, Price desc";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbolPrice(spv);
                spv.symbols.Reverse();
                spv.prices.Reverse();
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.prices, "Price");
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Price from "+
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by Symbol, Price";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbolPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.prices, "Price");
                ClearValuesDropStmt(env, spv);
            }
        }

        private class ResultSetExpressions : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select Symbol from "+
                          "SupportMarketDataBean#length(10) " +
                          "output every 6 events " +
"Order by (Price * 6) + 5";
                CreateAndSend(env, epl, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol"}));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Price from "+
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
"Order by (Price * 6) + 5, Price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Price" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, 1+Volume*23 from "+
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
"Order by (Price * 6) + 5, Price, Volume";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "1+Volume*23" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol from "+
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
"Order by Volume*Price, Symbol";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol"}));
                ClearValuesDropStmt(env, spv);
            }
        }

        private class ResultSetAliasesSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select Symbol as mySymbol from "+
                          "SupportMarketDataBean#length(5) " +
                          "output every 6 events " +
"Order by mySymbol";
                var listener = new SupportUpdateListener();
                CreateAndSend(env, epl, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.symbols, "mySymbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "mySymbol" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol as mySymbol, Price as myPrice from "+
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by myPrice";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "mySymbol");
                AssertValues(env, spv.prices, "myPrice");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "mySymbol", "myPrice" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Price as myPrice from "+
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
"Order by (myPrice * 6) + 5, Price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "myPrice" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, 1+Volume*23 as myVol from "+
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
"Order by (Price * 6) + 5, Price, myVol";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "myVol" }));
                ClearValuesDropStmt(env, spv);
            }
        }

        private class ResultSetExpressionsJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select Symbol from "+
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                          "output every 6 events " +
"Order by (Price * 6) + 5";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol"}));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Price from "+
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by (Price * 6) + 5, Price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.prices, "Price");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Price" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, 1+Volume*23 from "+
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by (Price * 6) + 5, Price, Volume";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "1+Volume*23" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol from "+
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by Volume*Price, Symbol";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol"}));
                ClearValuesDropStmt(env, spv);
            }
        }

        private class ResultSetInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var message = "Aggregate functions in the Order-by clause must also occur in the select expression";
                var epl = "@name('s0') select Symbol from "+
                          "SupportMarketDataBean#length(5) " +
                          "output every 6 events " +
"Order by sum(Price)";
                env.TryInvalidCompile(epl, message);

                epl = "@name('s0') select sum(Price) from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by sum(Price + 6)";
                env.TryInvalidCompile(epl, message);

                epl = "@name('s0') select sum(Price + 6) from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by sum(Price)";
                env.TryInvalidCompile(epl, message);

                epl = "@name('s0') select Symbol from "+
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by sum(Price)";
                env.TryInvalidCompile(epl, message);

                epl = "@name('s0') select sum(Price) from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by sum(Price + 6)";
                env.TryInvalidCompile(epl, message);

                epl = "@name('s0') select sum(Price + 6) from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by sum(Price)";
                env.TryInvalidCompile(epl, message);
            }
        }

        private class ResultSetMultipleKeys : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select Symbol from "+
                          "SupportMarketDataBean#length(10) " +
                          "output every 6 events " +
"Order by Symbol, Price";
                CreateAndSend(env, epl, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesBySymbolPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol"}));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol from "+
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
"Order by Price, Symbol, Volume";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPriceSymbol(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol"}));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Volume*2 from "+
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
"Order by Price, Volume";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Volume*2" }));
                ClearValuesDropStmt(env, spv);
            }
        }

        private class ResultSetAliases : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select Symbol as mySymbol from "+
                          "SupportMarketDataBean#length(5) " +
                          "output every 6 events " +
"Order by mySymbol";
                CreateAndSend(env, epl, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.symbols, "mySymbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "mySymbol" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol as mySymbol, Price as myPrice from "+
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by myPrice";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "mySymbol");
                AssertValues(env, spv.prices, "myPrice");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "mySymbol", "myPrice" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Price as myPrice from "+
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
"Order by (myPrice * 6) + 5, Price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "myPrice" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, 1+Volume*23 as myVol from "+
                      "SupportMarketDataBean#length(10) " +
                      "output every 6 events " +
"Order by (Price * 6) + 5, Price, myVol";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "myVol" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol as mySymbol from "+
                      "SupportMarketDataBean#length(5) " +
"Order by Price, mySymbol";
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

        private class ResultSetMultipleKeysJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select Symbol from "+
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                          "output every 6 events " +
"Order by Symbol, Price";
                var spv = new SymbolPricesVolumes();
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesBySymbolPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol"}));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol from "+
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by Price, Symbol, Volume";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceSymbol(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol"}));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Volume*2 from "+
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by Price, Volume";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Volume*2" }));
                ClearValuesDropStmt(env, spv);
            }
        }

        private class ResultSetSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select Symbol from "+
                          "SupportMarketDataBean#length(5) " +
                          "output every 6 events " +
"Order by Price";
                var spv = new SymbolPricesVolumes();
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol"}));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Price from "+
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by Price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.prices, "Price");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Price" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Volume from "+
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by Price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.volumes, "Volume");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Volume" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Volume*2 from "+
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by Price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.volumes, "Volume*2");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Volume*2" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Volume from "+
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by Symbol";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.volumes, "Volume");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Volume" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Price from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by Symbol";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.prices, "Price");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Price" }));
                ClearValuesDropStmt(env, spv);
            }
        }

        private class ResultSetSimpleJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select Symbol from "+
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                          "output every 6 events " +
"Order by Price";
                var spv = new SymbolPricesVolumes();
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol"}));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Price from "+
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by Price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.prices, "Price");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Price" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Volume from "+
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by Price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.volumes, "Volume");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Volume" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Volume*2 from "+
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by Price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.volumes, "Volume*2");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Volume*2" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Symbol, Volume from "+
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by Symbol";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.volumes, "Volume");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Volume" }));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select Price from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by Symbol, Price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesBySymbolJoin(spv);
                AssertValues(env, spv.prices, "Price");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Price" }));
                ClearValuesDropStmt(env, spv);
            }
        }

        private class ResultSetWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select * from " +
                          "SupportMarketDataBean#length(5) " +
                          "output every 6 events " +
"Order by Price";
                CreateAndSend(env, epl, milestone);
                var spv = new SymbolPricesVolumes();
                OrderValuesByPrice(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.prices, "Price");
                AssertValues(env, spv.volumes, "Volume");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Id", "Volume", "Price", "Feed"}));
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select * from " +
                      "SupportMarketDataBean#length(5) " +
                      "output every 6 events " +
"Order by Symbol";
                CreateAndSend(env, epl, milestone);
                OrderValuesBySymbol(spv);
                AssertValues(env, spv.symbols, "Symbol");
                AssertValues(env, spv.prices, "Price");
                AssertValues(env, spv.volumes, "Volume");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol", "Volume", "Price", "Feed", "Id" }));
                ClearValuesDropStmt(env, spv);
            }
        }

        private class ResultSetWildcardJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select * from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                          "output every 6 events " +
"Order by Price";
                var spv = new SymbolPricesVolumes();
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                AssertSymbolsJoinWildCard(env, spv.symbols);
                ClearValuesDropStmt(env, spv);

                epl = "@name('s0') select * from " +
                      "SupportMarketDataBean#length(10) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
                      "output every 6 events " +
"Order by Symbol, Price";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesBySymbolJoin(spv);
                AssertSymbolsJoinWildCard(env, spv.symbols);
                ClearValuesDropStmt(env, spv);
            }
        }

        private class ResultSetNoOutputClauseView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var spv = new SymbolPricesVolumes();
                var epl = "@name('s0') select Symbol from "+
                          "SupportMarketDataBean#length(5) " +
"Order by Price";
                var listener = new SupportUpdateListener();
                CreateAndSend(env, epl, milestone);
                spv.symbols.Add("CAT");
                AssertValues(env, spv.symbols, "Symbol");
                ClearValues(spv);
                SendEvent(env, "FOX", 10);
                spv.symbols.Add("FOX");
                AssertValues(env, spv.symbols, "Symbol");
                ClearValuesDropStmt(env, spv);

                // Set start time
                SendTimeEvent(env, 0);

                epl = "@name('s0') select Symbol from "+
                      "SupportMarketDataBean#time_batch(1 sec) " +
"Order by Price";
                CreateAndSend(env, epl, milestone);
                OrderValuesByPrice(spv);
                SendTimeEvent(env, 1000);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol"}));
                ClearValuesDropStmt(env, spv);
            }
        }

        private class ResultSetNoOutputClauseJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select Symbol from "+
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
"Order by Price";
                var spv = new SymbolPricesVolumes();
                var listener = new SupportUpdateListener();
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                spv.symbols.Add("KGB");
                AssertValues(env, spv.symbols, "Symbol");
                ClearValues(spv);
                SendEvent(env, "DOG", 10);
                spv.symbols.Add("DOG");
                AssertValues(env, spv.symbols, "Symbol");
                ClearValuesDropStmt(env, spv);

                // Set start time
                SendTimeEvent(env, 0);

                epl = "@name('s0') select Symbol from "+
                      "SupportMarketDataBean#time_batch(1) as one, " +
                      "SupportBeanString#length(100) as two " +
"where one.Symbol = two.TheString "+
"Order by Price, Symbol";
                CreateAndSend(env, epl, milestone);
                SendJoinEvents(env, milestone);
                OrderValuesByPriceJoin(spv);
                SendTimeEvent(env, 1000);
                AssertValues(env, spv.symbols, "Symbol");
                AssertOnlyProperties(env, Arrays.AsList(new string[] { "Symbol"}));
                ClearValuesDropStmt(env, spv);
            }
        }

        private static void AssertOnlyProperties(
            RegressionEnvironment env,
            IList<string> requiredProperties)
        {
            env.AssertListener(
                "s0",
                listener => {
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

        private static void AssertSymbolsJoinWildCard(
            RegressionEnvironment env,
            IList<string> symbols)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var events = listener.LastNewData;
                    Log.Debug(".assertValuesMayConvert event type = " + events[0].EventType);
                    Log.Debug(".assertValuesMayConvert values: " + symbols);
                    Log.Debug(".assertValuesMayConvert events.length==" + events.Length);
                    for (var i = 0; i < events.Length; i++) {
                        var theEvent = (SupportMarketDataBean)events[i].Get("one");
                        Assert.AreEqual(symbols[i], theEvent.Symbol);
                    }
                });
        }

        private static void AssertValues<T>(
            RegressionEnvironment env,
            IList<T> values,
            string valueName)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var events = listener.LastNewData;
                    Assert.AreEqual(values.Count, events.Length);
                    Log.Debug(".assertValuesMayConvert values: " + values);
                    for (var i = 0; i < events.Length; i++) {
                        Log.Debug(".assertValuesMayConvert events[" + i + "]==" + events[i].Get(valueName));
                        Assert.AreEqual(values[i], events[i].Get(valueName));
                    }
                });
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
            spv.prices.Clear();
            spv.volumes.Clear();
            spv.symbols.Clear();
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

        private static void OrderValuesByPriceDesc(SymbolPricesVolumes spv)
        {
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

        private static void OrderValuesByPriceJoin(SymbolPricesVolumes spv)
        {
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

        private static void OrderValuesByPriceSymbol(SymbolPricesVolumes spv)
        {
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

        private static void OrderValuesBySymbol(SymbolPricesVolumes spv)
        {
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

        private static void OrderValuesBySymbolJoin(SymbolPricesVolumes spv)
        {
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

        private static void OrderValuesBySymbolPrice(SymbolPricesVolumes spv)
        {
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

        private class SymbolPricesVolumes
        {
            internal IList<string> symbols = new List<string>();
            internal IList<double?> prices = new List<double?>();
            internal IList<long?> volumes = new List<long?>();
        }
    }
} // end of namespace