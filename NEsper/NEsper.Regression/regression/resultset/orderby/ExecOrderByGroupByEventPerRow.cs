///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.orderby
{
    public class ExecOrderByGroupByEventPerRow : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionAliasesAggregationCompile(epService);
            RunAssertionAliasesAggregationOM(epService);
            RunAssertionAliases(epService);
            RunAssertionGroupBySwitch(epService);
            RunAssertionGroupBySwitchJoin(epService);
            RunAssertionLastJoin(epService);
            RunAssertionIteratorGroupByEventPerRow(epService);
            RunAssertionLast(epService);
        }
    
        private void RunAssertionAliasesAggregationCompile(EPServiceProvider epService) {
            string statementString = "select symbol, volume, sum(price) as mySum from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) " +
                    "group by symbol " +
                    "output every 6 events " +
                    "order by sum(price), symbol";
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(statementString);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(statementString, model.ToEPL());
    
            var testListener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.Create(model);
            statement.Events += testListener.Update;
    
            TryAssertionDefault(epService, testListener);
    
            statement.Dispose();
        }
    
        private void RunAssertionAliasesAggregationOM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("symbol", "volume").Add(Expressions.Sum("price"), "mySum");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView(View.Create("length", Expressions.Constant(20))));
            model.GroupByClause = GroupByClause.Create("symbol");
            model.OutputLimitClause = OutputLimitClause.Create(6);
            model.OrderByClause = OrderByClause.Create(Expressions.Sum("price")).Add("symbol", false);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string statementString = "select symbol, volume, sum(price) as mySum from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) " +
                    "group by symbol " +
                    "output every 6 events " +
                    "order by sum(price), symbol";
    
            Assert.AreEqual(statementString, model.ToEPL());
    
            var testListener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.Create(model);
            statement.Events += testListener.Update;
    
            TryAssertionDefault(epService, testListener);
    
            statement.Dispose();
        }
    
        private void RunAssertionAliases(EPServiceProvider epService) {
            string statementString = "select symbol, volume, sum(price) as mySum from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) " +
                    "group by symbol " +
                    "output every 6 events " +
                    "order by mySum, symbol";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            TryAssertionDefault(epService, testListener);
    
            statement.Dispose();
        }
    
        private void RunAssertionGroupBySwitch(EPServiceProvider epService) {
            // Instead of the row-per-group behavior, these should
            // get row-per-event behavior since there are properties
            // in the order-by that are not in the select expression.
            string statementString = "select symbol, sum(price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) " +
                    "group by symbol " +
                    "output every 6 events " +
                    "order by sum(price), symbol, volume";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            TryAssertionDefaultNoVolume(epService, testListener);
    
            statement.Dispose();
        }
    
        private void RunAssertionGroupBySwitchJoin(EPServiceProvider epService) {
            string statementString = "select symbol, sum(price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "group by symbol " +
                    "output every 6 events " +
                    "order by sum(price), symbol, volume";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
    
            TryAssertionDefaultNoVolume(epService, testListener);
    
            statement.Dispose();
        }
    
        private void RunAssertionLast(EPServiceProvider epService) {
            string statementString = "select symbol, volume, sum(price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) " +
                    "group by symbol " +
                    "output last every 6 events " +
                    "order by sum(price)";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            TryAssertionLast(epService, testListener);
    
            statement.Dispose();
        }
    
        private void RunAssertionLastJoin(EPServiceProvider epService) {
            string statementString = "select symbol, volume, sum(price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "group by symbol " +
                    "output last every 6 events " +
                    "order by sum(price)";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            var testListener = new SupportUpdateListener();
            statement.Events += testListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
    
            TryAssertionLast(epService, testListener);
    
            statement.Dispose();
        }
    
        private void TryAssertionLast(EPServiceProvider epService, SupportUpdateListener testListener) {
            SendEvent(epService, "IBM", 101, 3);
            SendEvent(epService, "IBM", 102, 4);
            SendEvent(epService, "CMU", 103, 1);
            SendEvent(epService, "CMU", 104, 2);
            SendEvent(epService, "CAT", 105, 5);
            SendEvent(epService, "CAT", 106, 6);
    
            string[] fields = "symbol,volume,sum(price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(testListener.LastNewData, fields,
                    new object[][]{new object[] {"CMU", 104L, 3.0}, new object[] {"IBM", 102L, 7.0}, new object[] {"CAT", 106L, 11.0}});
            Assert.IsNull(testListener.LastOldData);
    
            SendEvent(epService, "IBM", 201, 3);
            SendEvent(epService, "IBM", 202, 4);
            SendEvent(epService, "CMU", 203, 5);
            SendEvent(epService, "CMU", 204, 5);
            SendEvent(epService, "DOG", 205, 0);
            SendEvent(epService, "DOG", 206, 1);
    
            EPAssertionUtil.AssertPropsPerRow(testListener.LastNewData, fields,
                    new object[][]{new object[] {"DOG", 206L, 1.0}, new object[] {"CMU", 204L, 13.0}, new object[] {"IBM", 202L, 14.0}});
            Assert.IsNull(testListener.LastOldData);
        }
    
    
        private void RunAssertionIteratorGroupByEventPerRow(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "TheString", "sumPrice"};
            string statementString = "select symbol, TheString, sum(price) as sumPrice from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "group by symbol " +
                    "order by symbol";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            SendJoinEvents(epService);
            SendEvent(epService, "CAT", 50);
            SendEvent(epService, "IBM", 49);
            SendEvent(epService, "CAT", 15);
            SendEvent(epService, "IBM", 100);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields,
                    new object[][]{
                            new object[] {"CAT", "CAT", 65d},
                            new object[] {"CAT", "CAT", 65d},
                            new object[] {"IBM", "IBM", 149d},
                            new object[] {"IBM", "IBM", 149d},
                    });
    
            SendEvent(epService, "KGB", 75);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields,
                    new object[][]{
                            new object[] {"CAT", "CAT", 65d},
                            new object[] {"CAT", "CAT", 65d},
                            new object[] {"IBM", "IBM", 149d},
                            new object[] {"IBM", "IBM", 149d},
                            new object[] {"KGB", "KGB", 75d},
                    });
    
            statement.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, long volume, double price) {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendJoinEvents(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
        }
    
        private void TryAssertionDefault(EPServiceProvider epService, SupportUpdateListener testListener) {
            SendEvent(epService, "IBM", 110, 3);
            SendEvent(epService, "IBM", 120, 4);
            SendEvent(epService, "CMU", 130, 1);
            SendEvent(epService, "CMU", 140, 2);
            SendEvent(epService, "CAT", 150, 5);
            SendEvent(epService, "CAT", 160, 6);
    
            string[] fields = "symbol,volume,mySum".Split(',');
            EPAssertionUtil.AssertPropsPerRow(testListener.LastNewData, fields,
                    new object[][]{new object[] {"CMU", 130L, 1.0}, new object[] {"CMU", 140L, 3.0}, new object[] {"IBM", 110L, 3.0},
                            new object[] {"CAT", 150L, 5.0}, 
                        new object[] {"IBM", 120L, 7.0},
                        new object[] {"CAT", 160L, 11.0}});
            Assert.IsNull(testListener.LastOldData);
        }
    
        private void TryAssertionDefaultNoVolume(EPServiceProvider epService, SupportUpdateListener testListener) {
            SendEvent(epService, "IBM", 110, 3);
            SendEvent(epService, "IBM", 120, 4);
            SendEvent(epService, "CMU", 130, 1);
            SendEvent(epService, "CMU", 140, 2);
            SendEvent(epService, "CAT", 150, 5);
            SendEvent(epService, "CAT", 160, 6);
    
            string[] fields = "symbol,sum(price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(testListener.LastNewData, fields,
                    new object[][]{new object[] {"CMU", 1.0}, new object[] {"CMU", 3.0}, new object[] {"IBM", 3.0},
                        new object[] {"CAT", 5.0}, 
                        new object[] {"IBM", 7.0}, 
                        new object[] {"CAT", 11.0}});
            Assert.IsNull(testListener.LastOldData);
        }
    }
} // end of namespace
