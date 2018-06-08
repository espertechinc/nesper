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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.orderby
{
    public class ExecOrderByRowPerEvent : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionIteratorAggregateRowPerEvent(epService);
            RunAssertionAliases(epService);
            RunAssertionAggregateAllJoinOrderFunction(epService);
            RunAssertionAggregateAllOrderFunction(epService);
            RunAssertionAggregateAllSum(epService);
            RunAssertionAggregateAllMaxSum(epService);
            RunAssertionAggregateAllSumHaving(epService);
            RunAssertionAggOrderWithSum(epService);
            RunAssertionAggregateAllJoin(epService);
            RunAssertionAggregateAllJoinMax(epService);
            RunAssertionAggHaving(epService);
        }
    
        private void RunAssertionIteratorAggregateRowPerEvent(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "sumPrice"};
            string statementString = "select symbol, sum(price) as sumPrice from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "order by symbol";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
    
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
    
            SendEvent(epService, "CAT", 50);
            SendEvent(epService, "IBM", 49);
            SendEvent(epService, "CAT", 15);
            SendEvent(epService, "IBM", 100);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields,
                    new object[][]{
                            new object[] {"CAT", 214d},
                            new object[] {"CAT", 214d},
                            new object[] {"IBM", 214d},
                            new object[] {"IBM", 214d},
                    });
    
            SendEvent(epService, "KGB", 75);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields,
                    new object[][]{
                            new object[] {"CAT", 289d},
                            new object[] {"CAT", 289d},
                            new object[] {"IBM", 289d},
                            new object[] {"IBM", 289d},
                            new object[] {"KGB", 289d},
                    });
    
            statement.Dispose();
        }
    
        private void RunAssertionAliases(EPServiceProvider epService) {
            string statementString = "select symbol as mySymbol, sum(price) as mySum from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by mySymbol";
    
            var listener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += listener.Update;
    
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 1);
            SendEvent(epService, "CMU", 2);
            SendEvent(epService, "CAT", 5);
            SendEvent(epService, "CAT", 6);
    
            string[] fields = "mySymbol,mySum".Split(',');
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{
                new object[] {"CAT", 15.0}, 
                new object[] {"CAT", 21.0}, 
                new object[] {"CMU", 8.0}, 
                new object[] {"CMU", 10.0}, 
                new object[] {"IBM", 3.0}, 
                new object[] {"IBM", 7.0}});
    
            statement.Dispose();
        }
    
        private void RunAssertionAggregateAllJoinOrderFunction(EPServiceProvider epService) {
            string statementString = "select symbol, sum(price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "output every 6 events " +
                    "order by volume*sum(price), symbol";
    
            var listener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += listener.Update;
            SendEvent(epService, "IBM", 2);
            SendEvent(epService, "KGB", 1);
            SendEvent(epService, "CMU", 3);
            SendEvent(epService, "IBM", 6);
            SendEvent(epService, "CAT", 6);
            SendEvent(epService, "CAT", 5);
    
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
    
            string[] fields = "symbol".Split(',');
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{
                new object[] {"CAT"}, 
                new object[] {"CAT"}, 
                new object[] {"CMU"}, 
                new object[] {"IBM"}, 
                new object[] {"IBM"}, 
                new object[] {"KGB"}});
    
            statement.Dispose();
        }
    
        private void RunAssertionAggregateAllOrderFunction(EPServiceProvider epService) {
            string statementString = "select symbol, sum(price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by volume*sum(price), symbol";
    
            var listener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += listener.Update;
    
            SendEvent(epService, "IBM", 2);
            SendEvent(epService, "KGB", 1);
            SendEvent(epService, "CMU", 3);
            SendEvent(epService, "IBM", 6);
            SendEvent(epService, "CAT", 6);
            SendEvent(epService, "CAT", 5);
    
            string[] fields = "symbol".Split(',');
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{
                new object[] {"CAT"}, 
                new object[] {"CAT"}, 
                new object[] {"CMU"}, 
                new object[] {"IBM"}, 
                new object[] {"IBM"}, 
                new object[] {"KGB"}});
    
            statement.Dispose();
        }
    
        private void RunAssertionAggregateAllSum(EPServiceProvider epService) {
            string statementString = "select symbol, sum(price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by symbol";
    
            var listener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += listener.Update;
    
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 1);
            SendEvent(epService, "CMU", 2);
            SendEvent(epService, "CAT", 5);
            SendEvent(epService, "CAT", 6);
    
            string[] fields = "symbol,sum(price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{
                new object[] {"CAT", 15.0}, 
                new object[] {"CAT", 21.0}, 
                new object[] {"CMU", 8.0}, 
                new object[] {"CMU", 10.0}, 
                new object[] {"IBM", 3.0}, 
                new object[] {"IBM", 7.0}});
    
            statement.Dispose();
        }
    
        private void RunAssertionAggregateAllMaxSum(EPServiceProvider epService) {
            string statementString = "select symbol, max(sum(price)) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by symbol";
    
            var listener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += listener.Update;
    
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 1);
            SendEvent(epService, "CMU", 2);
            SendEvent(epService, "CAT", 5);
            SendEvent(epService, "CAT", 6);
    
            string[] fields = "symbol,max(sum(price))".Split(',');
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{
                new object[] {"CAT", 15.0}, 
                new object[] {"CAT", 21.0}, 
                new object[] {"CMU", 8.0}, 
                new object[] {"CMU", 10.0}, 
                new object[] {"IBM", 3.0}, 
                new object[] {"IBM", 7.0}});
    
            statement.Dispose();
        }
    
        private void RunAssertionAggregateAllSumHaving(EPServiceProvider epService) {
            string statementString = "select symbol, sum(price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "having sum(price) > 0 " +
                    "output every 6 events " +
                    "order by symbol";
    
            var listener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += listener.Update;
    
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 1);
            SendEvent(epService, "CMU", 2);
            SendEvent(epService, "CAT", 5);
            SendEvent(epService, "CAT", 6);
    
            string[] fields = "symbol,sum(price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{
                new object[] {"CAT", 15.0}, 
                new object[] {"CAT", 21.0}, 
                new object[] {"CMU", 8.0}, 
                new object[] {"CMU", 10.0}, 
                new object[] {"IBM", 3.0}, 
                new object[] {"IBM", 7.0}});
    
            statement.Dispose();
        }
    
        private void RunAssertionAggOrderWithSum(EPServiceProvider epService) {
            string statementString = "select symbol, sum(price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) " +
                    "output every 6 events " +
                    "order by symbol, sum(price)";
    
            var listener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += listener.Update;
    
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 1);
            SendEvent(epService, "CMU", 2);
            SendEvent(epService, "CAT", 5);
            SendEvent(epService, "CAT", 6);
    
            string[] fields = "symbol,sum(price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{
                new object[] {"CAT", 15.0}, 
                new object[] {"CAT", 21.0}, 
                new object[] {"CMU", 8.0}, 
                new object[] {"CMU", 10.0}, 
                new object[] {"IBM", 3.0}, 
                new object[] {"IBM", 7.0}});
    
            statement.Dispose();
        }
    
        private void RunAssertionAggregateAllJoin(EPServiceProvider epService) {
            string statementString = "select symbol, sum(price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "output every 6 events " +
                    "order by symbol, sum(price)";
    
            var listener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += listener.Update;
    
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 1);
            SendEvent(epService, "CMU", 2);
            SendEvent(epService, "CAT", 5);
            SendEvent(epService, "CAT", 6);
    
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
    
            string[] fields = "symbol,sum(price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{
                new object[] {"CAT", 11.0}, 
                new object[] {"CAT", 11.0}, 
                new object[] {"CMU", 21.0}, 
                new object[] {"CMU", 21.0}, 
                new object[] {"IBM", 18.0}, 
                new object[] {"IBM", 18.0}});
    
            statement.Dispose();
        }
    
        private void RunAssertionAggregateAllJoinMax(EPServiceProvider epService) {
            string statementString = "select symbol, max(sum(price)) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "output every 6 events " +
                    "order by symbol";
    
            var listener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += listener.Update;
    
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 1);
            SendEvent(epService, "CMU", 2);
            SendEvent(epService, "CAT", 5);
            SendEvent(epService, "CAT", 6);
    
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
    
            string[] fields = "symbol,max(sum(price))".Split(',');
            EPAssertionUtil.AssertPropsPerRow(
                listener.LastNewData, fields, new object[][] {
                    new object[] {"CAT", 11.0},
                    new object[] {"CAT", 11.0}, 
                    new object[] {"CMU", 21.0}, 
                    new object[] {"CMU", 21.0}, 
                    new object[] {"IBM", 18.0}, 
                    new object[] {"IBM", 18.0}
                });
    
            statement.Dispose();
        }
    
        private void RunAssertionAggHaving(EPServiceProvider epService) {
            string statementString = "select symbol, sum(price) from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "having sum(price) > 0 " +
                    "output every 6 events " +
                    "order by symbol";
            var listener = new SupportUpdateListener();
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            statement.Events += listener.Update;
    
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 1);
            SendEvent(epService, "CMU", 2);
            SendEvent(epService, "CAT", 5);
            SendEvent(epService, "CAT", 6);
    
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
    
            string[] fields = "symbol,sum(price)".Split(',');
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{
                new object[] {"CAT", 11.0}, 
                new object[] {"CAT", 11.0}, 
                new object[] {"CMU", 21.0}, 
                new object[] {"CMU", 21.0}, 
                new object[] {"IBM", 18.0}, 
                new object[] {"IBM", 18.0}});
    
            statement.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
