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
            string stmtText = "select a.TheString from pattern [every a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%')] order by a.TheString desc";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmtText);
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B", 3));
    
            EventBean[] received = listener.GetNewDataListFlattened();
            Assert.AreEqual(2, received.Length);
            EPAssertionUtil.AssertPropsPerRow(received, "a.TheString".Split(','), new object[][]
            {
                new object[] {"A2"}, new object[] {"A1"}
            });
    
            // try pattern with output limit
            var listenerThree = new SupportUpdateListener();
            string stmtTextThree = "select a.TheString from pattern [every a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%')] " +
                    "output every 2 events order by a.TheString desc";
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL(stmtTextThree);
            stmtThree.Events += listenerThree.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("A3", 3));
            epService.EPRuntime.SendEvent(new SupportBean("B", 3));
    
            EventBean[] receivedThree = listenerThree.GetNewDataListFlattened();
            Assert.AreEqual(2, receivedThree.Length);
            EPAssertionUtil.AssertPropsPerRow(receivedThree, "a.TheString".Split(','), new object[][]
            {
                new object[] {"A2"}, new object[] {"A1"}
            });
    
            // try grouped time window
            string stmtTextTwo = "select rstream TheString from SupportBean#groupwin(TheString)#time(10) order by TheString desc";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 1));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(11000));
            EventBean[] receivedTwo = listenerTwo.GetNewDataListFlattened();
            Assert.AreEqual(2, receivedTwo.Length);
            EPAssertionUtil.AssertPropsPerRow(receivedTwo, "TheString".Split(','), new object[][]
            {
                new object[] {"A2"}, new object[] {"A1"}
            });
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionIterator(EPServiceProvider epService) {
            string statementString = "select Symbol, TheString, Price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "order by Price";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            SendJoinEvents(epService);
            SendEvent(epService, "CAT", 50);
            SendEvent(epService, "IBM", 49);
            SendEvent(epService, "CAT", 15);
            SendEvent(epService, "IBM", 100);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), new string[]{"Symbol", "TheString", "Price"},
                    new object[][]{
                            new object[] {"CAT", "CAT", 15d},
                            new object[] {"IBM", "IBM", 49d},
                            new object[] {"CAT", "CAT", 50d},
                            new object[] {"IBM", "IBM", 100d},
                    });
    
            SendEvent(epService, "KGB", 75);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), new string[]{"Symbol", "TheString", "Price"},
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
            string statementString = "select Symbol, TheString from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by Price";
            var listener = new SupportUpdateListener();
            var spv = new SymbolPricesVolumes();
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Symbols, "TheString");
            AssertOnlyProperties(listener, Collections.List("Symbol", "TheString"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by TheString, Price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesBySymbolPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol"));
            ClearValuesDropStmt(epService, spv);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionDescending_OM(EPServiceProvider epService) {
            string stmtText = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Price desc";
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("Symbol");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView("length", Expressions.Constant(5)));
            model.OutputLimitClause = OutputLimitClause.Create(6);
            model.OrderByClause = OrderByClause.Create().Add("Price", true);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
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
            AssertValues(listener, spv.Symbols, "Symbol");
    
            statement.Dispose();
        }
    
        private void RunAssertionDescending(EPServiceProvider epService) {
            string statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Price desc";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            var spv = new SymbolPricesVolumes();
            OrderValuesByPriceDesc(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Price desc, Symbol asc";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            CompatExtensions.Reverse(spv.Symbols);
            AssertValues(listener, spv.Symbols, "Symbol");
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Price asc";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Volume from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Symbol desc";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbol(spv);
            CompatExtensions.Reverse(spv.Symbols);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Volumes, "Volume");
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Symbol desc, Price desc";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbolPrice(spv);
            CompatExtensions.Reverse(spv.Symbols);
            CompatExtensions.Reverse(spv.Prices);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Prices, "Price");
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Symbol, Price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbolPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Prices, "Price");
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionExpressions(EPServiceProvider epService) {
            string statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (Price * 6) + 5";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            var spv = new SymbolPricesVolumes();
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (Price * 6) + 5, Price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Price"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, 1+Volume*23 from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (Price * 6) + 5, Price, Volume";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol", "1+Volume*23"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by Volume*Price, Symbol";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionAliasesSimple(EPServiceProvider epService) {
            string statementString = "select Symbol as mySymbol from " +
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
    
            statementString = "select Symbol as mySymbol, Price as myPrice from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by myPrice";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "mySymbol");
            AssertValues(listener, spv.Prices, "myPrice");
            AssertOnlyProperties(listener, Collections.List("mySymbol", "myPrice"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Price as myPrice from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (myPrice * 6) + 5, Price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol", "myPrice"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, 1+Volume*23 as myVol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (Price * 6) + 5, Price, myVol";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol", "myVol"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionExpressionsJoin(EPServiceProvider epService) {
            string statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by (Price * 6) + 5";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            var spv = new SymbolPricesVolumes();
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by (Price * 6) + 5, Price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Prices, "Price");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Price"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, 1+Volume*23 from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by (Price * 6) + 5, Price, Volume";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol", "1+Volume*23"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by Volume*Price, Symbol";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by sum(Price)";
            try {
                CreateAndSend(epService, statementString, listener);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
    
            statementString = "select sum(Price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by sum(Price + 6)";
            try {
                CreateAndSend(epService, statementString, listener);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
    
            statementString = "select sum(Price + 6) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by sum(Price)";
            try {
                CreateAndSend(epService, statementString, listener);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
        }
    
        private void RunAssertionInvalidJoin(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by sum(Price)";
            try {
                CreateAndSend(epService, statementString, listener);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
    
            statementString = "select sum(Price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by sum(Price + 6)";
            try {
                CreateAndSend(epService, statementString, listener);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
    
            statementString = "select sum(Price + 6) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by sum(Price)";
            try {
                CreateAndSend(epService, statementString, listener);
                Assert.Fail();
            } catch (EPStatementException) {
                // expected
            }
        }
    
        private void RunAssertionMultipleKeys(EPServiceProvider epService) {
            string statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by Symbol, Price";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            var spv = new SymbolPricesVolumes();
            OrderValuesBySymbolPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by Price, Symbol, Volume";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPriceSymbol(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Volume*2 from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by Price, Volume";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Volume*2"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionAliases(EPServiceProvider epService) {
            string statementString = "select Symbol as mySymbol from " +
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
    
            statementString = "select Symbol as mySymbol, Price as myPrice from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by myPrice";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "mySymbol");
            AssertValues(listener, spv.Prices, "myPrice");
            AssertOnlyProperties(listener, Collections.List("mySymbol", "myPrice"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Price as myPrice from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (myPrice * 6) + 5, Price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol", "myPrice"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, 1+Volume*23 as myVol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by (Price * 6) + 5, Price, myVol";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol", "myVol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol as mySymbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "order by Price, mySymbol";
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
            string statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by Symbol, Price";
            var listener = new SupportUpdateListener();
            var spv = new SymbolPricesVolumes();
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesBySymbolPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by Price, Symbol, Volume";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceSymbol(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Volume*2 from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by Price, Volume";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Volume*2"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionSimple(EPServiceProvider epService) {
            string statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Price";
            var spv = new SymbolPricesVolumes();
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Prices, "Price");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Price"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Volume from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Volumes, "Volume");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Volume"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Volume*2 from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Volumes, "Volume*2");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Volume*2"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Volume from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Symbol";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Volumes, "Volume");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Volume"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Symbol";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Prices, "Price");
            AssertOnlyProperties(listener, Collections.List("Price"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionSimpleJoin(EPServiceProvider epService) {
            string statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by Price";
            var spv = new SymbolPricesVolumes();
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by Price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Prices, "Price");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Price"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Volume from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by Price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Volumes, "Volume");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Volume"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Volume*2 from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by Price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Volumes, "Volume*2");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Volume*2"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Symbol, Volume from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by Symbol";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Volumes, "Volume");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Volume"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select Price from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by Symbol, Price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesBySymbolJoin(spv);
            AssertValues(listener, spv.Prices, "Price");
            AssertOnlyProperties(listener, Collections.List("Price"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionWildcard(EPServiceProvider epService) {
            string statementString = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Price";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            var spv = new SymbolPricesVolumes();
            OrderValuesByPrice(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Prices, "Price");
            AssertValues(listener, spv.Volumes, "Volume");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Id", "Volume", "Price", "Feed"));
            ClearValuesDropStmt(epService, spv);
    
            statementString = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "output every 6 events " +
                    "order by Symbol";
            CreateAndSend(epService, statementString, listener);
            OrderValuesBySymbol(spv);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertValues(listener, spv.Prices, "Price");
            AssertValues(listener, spv.Volumes, "Volume");
            AssertOnlyProperties(listener, Collections.List("Symbol", "Volume", "Price", "Feed", "Id"));
            ClearValuesDropStmt(epService, spv);
        }
    
    
        private void RunAssertionWildcardJoin(EPServiceProvider epService) {
            string statementString = "select * from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by Price";
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
                    "where one.Symbol = two.TheString " +
                    "output every 6 events " +
                    "order by Symbol, Price";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesBySymbolJoin(spv);
            AssertSymbolsJoinWildCard(listener, spv.Symbols);
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionNoOutputClauseView(EPServiceProvider epService) {
            var spv = new SymbolPricesVolumes();
            string statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "order by Price";
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            spv.Symbols.Add("CAT");
            AssertValues(listener, spv.Symbols, "Symbol");
            ClearValues(spv);
            SendEvent(epService, "FOX", 10);
            spv.Symbols.Add("FOX");
            AssertValues(listener, spv.Symbols, "Symbol");
            ClearValuesDropStmt(epService, spv);
    
            // Set start time
            SendTimeEvent(epService, 0);
    
            statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#time_batch(1 sec) " +
                    "order by Price";
            CreateAndSend(epService, statementString, listener);
            OrderValuesByPrice(spv);
            SendTimeEvent(epService, 1000);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol"));
            ClearValuesDropStmt(epService, spv);
        }
    
        private void RunAssertionNoOutputClauseJoin(EPServiceProvider epService) {
            string statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "order by Price";
            var spv = new SymbolPricesVolumes();
            var listener = new SupportUpdateListener();
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            spv.Symbols.Add("KGB");
            AssertValues(listener, spv.Symbols, "Symbol");
            ClearValues(spv);
            SendEvent(epService, "DOG", 10);
            spv.Symbols.Add("DOG");
            AssertValues(listener, spv.Symbols, "Symbol");
            ClearValuesDropStmt(epService, spv);
    
            // Set start time
            SendTimeEvent(epService, 0);
    
            statementString = "select Symbol from " +
                    typeof(SupportMarketDataBean).FullName + "#time_batch(1) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.Symbol = two.TheString " +
                    "order by Price, Symbol";
            CreateAndSend(epService, statementString, listener);
            SendJoinEvents(epService);
            OrderValuesByPriceJoin(spv);
            SendTimeEvent(epService, 1000);
            AssertValues(listener, spv.Symbols, "Symbol");
            AssertOnlyProperties(listener, Collections.List("Symbol"));
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
