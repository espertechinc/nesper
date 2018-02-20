///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.orderby
{
    public class ExecOrderBySimple : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionOrderByMultiDelivery(epService);
            RunAssertionIterator(epService);
            RunAssertionAcrossJoin(epService);
            RunAssertionDescending_OM(epService);
            RunAssertionDescending(epService);
            RunAssertionExpressions(epService);
            RunAssertionAliasesSimple(epService);
            RunAssertionExpressionsJoin(epService);
            RunAssertionMultipleKeys(epService);
            RunAssertionAliases(epService);
            RunAssertionMultipleKeysJoin(epService);
            RunAssertionSimple(epService);
            RunAssertionSimpleJoin(epService);
            RunAssertionWildcard(epService);
            RunAssertionWildcardJoin(epService);
            RunAssertionNoOutputClauseView(epService);
            RunAssertionNoOutputClauseJoin(epService);
            RunAssertionInvalid(epService);
            RunAssertionInvalidJoin(epService);
        }
    
        private void RunAssertionOrderByMultiDelivery(EPServiceProvider epService) {
            // test for QWY-933597 or ESPER-409
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // try pattern
            var listener = new SupportUpdateListener();
            string stmtText = "select a.theString from pattern [every a=SupportBean(theString like 'A%') -> b=SupportBean(theString like 'B%')] order by a.theString desc";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtText);
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B", 3));
    
            EventBean[] received = listener.GetNewDataListFlattened();
            Assert.AreEqual(2, received.Length);
            EPAssertionUtil.AssertPropsPerRow(received, "a.theString".Split(','), new object[][]
            {
                new object[] {"A2"}, new object[] {"A1"}
            });
    
            // try pattern with output limit
            var listenerThree = new SupportUpdateListener();
            string stmtTextThree = "select a.theString from pattern [every a=SupportBean(theString like 'A%') -> b=SupportBean(theString like 'B%')] " +
                    "output every 2 events order by a.theString desc";
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL(stmtTextThree);
            stmtThree.Events += listenerThree.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("A3", 3));
            epService.EPRuntime.SendEvent(new SupportBean("B", 3));
    
            EventBean[] receivedThree = listenerThree.GetNewDataListFlattened();
            Assert.AreEqual(2, receivedThree.Length);
            EPAssertionUtil.AssertPropsPerRow(receivedThree, "a.theString".Split(','), new object[][]
            {
                new object[] {"A2"}, new object[] {"A1"}
            });
    
            // try grouped time window
            string stmtTextTwo = "select rstream theString from SupportBean#Groupwin(theString)#Time(10) order by theString desc";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 1));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(11000));
            EventBean[] receivedTwo = listenerTwo.GetNewDataListFlattened();
            Assert.AreEqual(2, receivedTwo.Length);
            EPAssertionUtil.AssertPropsPerRow(receivedTwo, "theString".Split(','), new object[][]
            {
                new object[] {"A2"}, new object[] {"A1"}
            });
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionIterator(EPServiceProvider epService) {
            string statementString = "select symbol, theString, price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "order by price";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            SendJoinEvents(epService);
            SendEvent(epService, "CAT", 50);
            SendEvent(epService, "IBM", 49);
            SendEvent(epService, "CAT", 15);
            SendEvent(epService, "IBM", 100);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), new string[]{"symbol", "theString", "price"},
                    new object[][]{
                            new object[] {"CAT", "CAT", 15d},
                            new object[] {"IBM", "IBM", 49d},
                            new object[] {"CAT", "CAT", 50d},
                            new object[] {"IBM", "IBM", 100d},
                    });
    
            SendEvent(epService, "KGB", 75);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), new string[]{"symbol", "theString", "price"},
                    new object[][]{
                            new object[] {"CAT", "CAT", 15d},
                            new object[] {"IBM", "IBM", 49d},
                            new object[] {"CAT", "CAT", 50d},
                            new object[] {"KGB", "KGB", 75d},
                            new object[] {"IBM", "IBM", 100d},
                    });
    
            statement.Dispose();
        }
    
        private void RunAssertionAcrossJoin(EPServiceProvider epService) {
            string statementString = "select symbol, theString from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by price";
            var listener = new SupportUpdateListener();
            var spv = new SymbolPricesVolumes();
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Symbols, "theString");
            AssertOnlyProperties(listener, Collections.List("symbol", "theString"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by theString, price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesBySymbolPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol"));
            ClearValuesDropStmt(epService, spv);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionDescending_OM(EPServiceProvider epService) {
            string stmtText = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by price desc";
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("symbol");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView("length", Expressions.Constant(5)));
            model.OutputLimitClause = OutputLimitClause.Create(6);
            model.OrderByClause = OrderByClause.Create().Add("price", true);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            var listener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.Create(model);
            statement.Events += listener.Update;
            SendEvent(epService, "IBM", 2);
            SendEvent(epService, "KGB", 1);
            SendEvent(epService, "CMU", 3);
            SendEvent(epService, "IBM", 6);
            SendEvent(epService, "CAT", 6);
            SendEvent(epService, "CAT", 5);
    
            var spv = new SymbolPricesVolumes();
            OrderValuesByPriceDesc(spv);
            AssertValues(listener, spv.Symbols, "symbol");
    
            statement.Dispose();
        }
    
        private void RunAssertionDescending(EPServiceProvider epService) {
            string statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by price desc";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            var spv = new SymbolPricesVolumes();
            OrderValuesByPriceDesc(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by price desc, symbol asc";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            CompatExtensions.Reverse(spv.Symbols);
            AssertValues(listener, spv.Symbols, "symbol");
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by price asc";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, volume from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by symbol desc";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbol(spv);
            CompatExtensions.Reverse(spv.Symbols);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Volumes, "volume");
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by symbol desc, price desc";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbolPrice(spv);
            CompatExtensions.Reverse(spv.Symbols);
            CompatExtensions.Reverse(spv.Prices);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Prices, "price");
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by symbol, price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbolPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Prices, "price");
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionExpressions(EPServiceProvider epService) {
            string statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (price * 6) + 5";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            var spv = new SymbolPricesVolumes();
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (price * 6) + 5, price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol", "price"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, 1+volume*23 from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (price * 6) + 5, price, volume";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol", "1+volume*23"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by volume*price, symbol";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionAliasesSimple(EPServiceProvider epService) {
            string statementString = "select symbol as mySymbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by mySymbol";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            var spv = new SymbolPricesVolumes();
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Symbols, "mySymbol");
            AssertOnlyProperties(listener, Collections.List("mySymbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol as mySymbol, price as myPrice from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by myPrice";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "mySymbol");
            AssertValues(listener, spv.Prices, "myPrice");
            AssertOnlyProperties(listener, Collections.List("mySymbol", "myPrice"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, price as myPrice from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (myPrice * 6) + 5, price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol", "myPrice"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, 1+volume*23 as myVol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (price * 6) + 5, price, myVol";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol", "myVol"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionExpressionsJoin(EPServiceProvider epService) {
            string statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by (price * 6) + 5";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            var spv = new SymbolPricesVolumes();
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by (price * 6) + 5, price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Prices, "price");
            AssertOnlyProperties(listener, Collections.List("symbol", "price"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, 1+volume*23 from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by (price * 6) + 5, price, volume";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol", "1+volume*23"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by volume*price, symbol";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by sum(price)";
            try {
                CreateAndSend(epService, statementString, listener);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
    
            statementString = "select sum(price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by sum(price + 6)";
            try {
                CreateAndSend(epService, statementString, listener);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
    
            statementString = "select sum(price + 6) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by sum(price)";
            try {
                CreateAndSend(epService, statementString, listener);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
        }
    
        private void RunAssertionInvalidJoin(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by sum(price)";
            try {
                CreateAndSend(epService, statementString, listener);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
    
            statementString = "select sum(price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by sum(price + 6)";
            try {
                CreateAndSend(epService, statementString, listener);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
    
            statementString = "select sum(price + 6) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by sum(price)";
            try {
                CreateAndSend(epService, statementString, listener);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
        }
    
        private void RunAssertionMultipleKeys(EPServiceProvider epService) {
            string statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by symbol, price";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            var spv = new SymbolPricesVolumes();
            OrderValuesBySymbolPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by price, symbol, volume";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPriceSymbol(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, volume*2 from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by price, volume";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol", "volume*2"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionAliases(EPServiceProvider epService) {
            string statementString = "select symbol as mySymbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by mySymbol";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            var spv = new SymbolPricesVolumes();
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Symbols, "mySymbol");
            AssertOnlyProperties(listener, Collections.List("mySymbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol as mySymbol, price as myPrice from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by myPrice";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "mySymbol");
            AssertValues(listener, spv.Prices, "myPrice");
            AssertOnlyProperties(listener, Collections.List("mySymbol", "myPrice"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, price as myPrice from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (myPrice * 6) + 5, price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol", "myPrice"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, 1+volume*23 as myVol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (price * 6) + 5, price, myVol";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol", "myVol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol as mySymbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "order by price, mySymbol";
            CreateAndSend(epService, statementString, listener);
            spv.Symbols.Add("CAT");
            AssertValues(listener, spv.Symbols, "mySymbol");
            ClearValues(spv);
            SendEvent(epService, "FOX", 10);
            spv.Symbols.Add("FOX");
            AssertValues(listener, spv.Symbols, "mySymbol");
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionMultipleKeysJoin(EPServiceProvider epService) {
            string statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by symbol, price";
            var listener = new SupportUpdateListener();
            var spv = new SymbolPricesVolumes();
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesBySymbolPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by price, symbol, volume";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceSymbol(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, volume*2 from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by price, volume";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol", "volume*2"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionSimple(EPServiceProvider epService) {
            string statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by price";
            var spv = new SymbolPricesVolumes();
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Prices, "price");
            AssertOnlyProperties(listener, Collections.List("symbol", "price"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, volume from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Volumes, "volume");
            AssertOnlyProperties(listener, Collections.List("symbol", "volume"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, volume*2 from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Volumes, "volume*2");
            AssertOnlyProperties(listener, Collections.List("symbol", "volume*2"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, volume from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by symbol";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Volumes, "volume");
            AssertOnlyProperties(listener, Collections.List("symbol", "volume"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by symbol";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Prices, "price");
            AssertOnlyProperties(listener, Collections.List("price"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionSimpleJoin(EPServiceProvider epService) {
            string statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by price";
            var spv = new SymbolPricesVolumes();
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Prices, "price");
            AssertOnlyProperties(listener, Collections.List("symbol", "price"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, volume from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Volumes, "volume");
            AssertOnlyProperties(listener, Collections.List("symbol", "volume"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, volume*2 from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Volumes, "volume*2");
            AssertOnlyProperties(listener, Collections.List("symbol", "volume*2"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select symbol, volume from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by symbol";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Volumes, "volume");
            AssertOnlyProperties(listener, Collections.List("symbol", "volume"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by symbol, price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesBySymbolJoin(spv);
            AssertValues(listener, spv.Prices, "price");
            AssertOnlyProperties(listener, Collections.List("price"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionWildcard(EPServiceProvider epService) {
            string statementString = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by price";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            var spv = new SymbolPricesVolumes();
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Prices, "price");
            AssertValues(listener, spv.Volumes, "volume");
            AssertOnlyProperties(listener, Collections.List("symbol", "id", "volume", "price", "feed"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by symbol";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertValues(listener, spv.Prices, "price");
            AssertValues(listener, spv.Volumes, "volume");
            AssertOnlyProperties(listener, Collections.List("symbol", "volume", "price", "feed", "id"));
            ClearValuesDropStmt(epService, spv);
        }
    
    
        private void RunAssertionWildcardJoin(EPServiceProvider epService) {
            string statementString = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by price";
            var spv = new SymbolPricesVolumes();
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertSymbolsJoinWildCard(listener, spv.Symbols);
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "output every 6 events " +
                    "order by symbol, price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesBySymbolJoin(spv);
            AssertSymbolsJoinWildCard(listener, spv.Symbols);
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionNoOutputClauseView(EPServiceProvider epService) {
            var spv = new SymbolPricesVolumes();
            string statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "order by price";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            spv.Symbols.Add("CAT");
            AssertValues(listener, spv.Symbols, "symbol");
            ClearValues(spv);
            SendEvent(epService, "FOX", 10);
            spv.Symbols.Add("FOX");
            AssertValues(listener, spv.Symbols, "symbol");
            ClearValuesDropStmt(epService, spv);
    
            // Set start time
            SendTimeEvent(epService, 0);
    
            statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#Time_batch(1 sec) " +
                    "order by price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            SendTimeEvent(epService, 1000);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionNoOutputClauseJoin(EPServiceProvider epService) {
            string statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "order by price";
            var spv = new SymbolPricesVolumes();
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            spv.Symbols.Add("KGB");
            AssertValues(listener, spv.Symbols, "symbol");
            ClearValues(spv);
            SendEvent(epService, "DOG", 10);
            spv.Symbols.Add("DOG");
            AssertValues(listener, spv.Symbols, "symbol");
            ClearValuesDropStmt(epService, spv);
    
            // Set start time
            SendTimeEvent(epService, 0);
    
            statementString = "select symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#Time_batch(1) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.theString " +
                    "order by price, symbol";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            SendTimeEvent(epService, 1000);
            AssertValues(listener, spv.Symbols, "symbol");
            AssertOnlyProperties(listener, Collections.List("symbol"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void AssertOnlyProperties(SupportUpdateListener listener, IList<string> requiredProperties) {
            EventBean[] events = listener.LastNewData;
            if (events == null || events.Length == 0) {
                return;
            }
            EventType type = events[0].EventType;
            var actualProperties = new List<string>(Collections.List(type.PropertyNames));
            Log.Debug(".assertOnlyProperties actualProperties==" + actualProperties);
            Assert.IsTrue(actualProperties.ContainsAll(requiredProperties));
            actualProperties.RemoveAll(requiredProperties);
            Assert.IsTrue(actualProperties.IsEmpty());
        }
    
        private void AssertSymbolsJoinWildCard(SupportUpdateListener listener, IList<string> symbols) {
            EventBean[] events = listener.LastNewData;
            Log.Debug(".assertValuesMayConvert event type = " + events[0].EventType);
            Log.Debug(".assertValuesMayConvert values: " + symbols);
            Log.Debug(".assertValuesMayConvert events.Length==" + events.Length);
            for (int i = 0; i < events.Length; i++) {
                SupportMarketDataBean theEvent = (SupportMarketDataBean) events[i].Get("one");
                Assert.AreEqual(symbols[i], theEvent.Symbol);
            }
        }
    
        private void AssertValues<T>(SupportUpdateListener listener, IList<T> values, string valueName) {
            EventBean[] events = listener.LastNewData;
            Assert.AreEqual(values.Count, events.Length);
            Log.Debug(".assertValuesMayConvert values: " + values);
            for (int i = 0; i < events.Length; i++) {
                Log.Debug(".assertValuesMayConvert events[" + i + "]==" + events[i].Get(valueName));
                Assert.AreEqual(values[i], events[i].Get(valueName));
            }
        }
    
        private void ClearValuesDropStmt(EPServiceProvider epService, SymbolPricesVolumes spv) {
            epService.EPAdministrator.DestroyAllStatements();
            ClearValues(spv);
        }
    
        private void ClearValues(SymbolPricesVolumes spv) {
            spv.Prices.Clear();
            spv.Volumes.Clear();
            spv.Symbols.Clear();
        }
    
        private void CreateAndSend(EPServiceProvider epService, string statementString, SupportUpdateListener listener) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += listener.Update;
            SendEvent(epService, "IBM", 2);
            SendEvent(epService, "KGB", 1);
            SendEvent(epService, "CMU", 3);
            SendEvent(epService, "IBM", 6);
            SendEvent(epService, "CAT", 6);
            SendEvent(epService, "CAT", 5);
        }
    
        private void OrderValuesByPrice(SymbolPricesVolumes spv) {
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
    
        private void OrderValuesByPriceDesc(SymbolPricesVolumes spv) {
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
    
        private void OrderValuesByPriceJoin(SymbolPricesVolumes spv) {
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
    
        private void OrderValuesByPriceSymbol(SymbolPricesVolumes spv) {
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
    
        private void OrderValuesBySymbol(SymbolPricesVolumes spv) {
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
    
        private void OrderValuesBySymbolJoin(SymbolPricesVolumes spv) {
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
    
        private void OrderValuesBySymbolPrice(SymbolPricesVolumes spv) {
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
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendTimeEvent(EPServiceProvider epService, int millis) {
            var theEvent = new CurrentTimeEvent(millis);
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendJoinEvents(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
        }

        private class SymbolPricesVolumes
        {
            public IList<string> Symbols = new List<string>();
            public IList<double> Prices = new List<double>();
            public IList<long> Volumes = new List<long>();
        }
    }
} // end of namespace
