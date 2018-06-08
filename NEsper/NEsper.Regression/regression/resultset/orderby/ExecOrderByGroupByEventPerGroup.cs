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
    public class ExecOrderByGroupByEventPerGroup : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionNoHavingNoJoin(epService);
            RunAssertionHavingNoJoin(epService);
            RunAssertionNoHavingJoin(epService);
            RunAssertionHavingJoin(epService);
            RunAssertionHavingJoinAlias(epService);
            RunAssertionLast(epService);
            RunAssertionLastJoin(epService);
            RunAssertionIteratorGroupByEventPerGroup(epService);
        }
    
        private void RunAssertionNoHavingNoJoin(EPServiceProvider epService) {
            string statementString = "select irstream symbol, sum(price) as mysum from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) " +
                    "group by symbol " +
                    "output every 6 events " +
                    "order by sum(price), symbol";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            TryAssertionNoHaving(epService, statement);
            statement.Dispose();
        }
    
        private void RunAssertionHavingNoJoin(EPServiceProvider epService) {
            string statementString = "select irstream symbol, sum(price) as mysum from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) " +
                    "group by symbol " +
                    "having sum(price) > 0 " +
                    "output every 6 events " +
                    "order by sum(price), symbol";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            TryAssertionHaving(epService, statement);
            statement.Dispose();
        }
    
        private void RunAssertionNoHavingJoin(EPServiceProvider epService) {
            string statementString = "select irstream symbol, sum(price) as mysum from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "group by symbol " +
                    "output every 6 events " +
                    "order by sum(price), symbol";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
    
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
    
            TryAssertionNoHaving(epService, statement);
    
            statement.Dispose();
        }
    
        private void RunAssertionHavingJoin(EPServiceProvider epService) {
            string statementString = "select irstream symbol, sum(price) as mysum from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "group by symbol " +
                    "having sum(price) > 0 " +
                    "output every 6 events " +
                    "order by sum(price), symbol";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
    
            TryAssertionHaving(epService, statement);
    
            statement.Dispose();
        }
    
        private void RunAssertionHavingJoinAlias(EPServiceProvider epService) {
            string statementString = "select irstream symbol, sum(price) as mysum from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "group by symbol " +
                    "having sum(price) > 0 " +
                    "output every 6 events " +
                    "order by mysum, symbol";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
    
            TryAssertionHaving(epService, statement);
    
            statement.Dispose();
        }
    
        private void RunAssertionLast(EPServiceProvider epService) {
            string statementString = "select irstream symbol, sum(price) as mysum from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) " +
                    "group by symbol " +
                    "output last every 6 events " +
                    "order by sum(price), symbol";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
            TryAssertionLast(epService, statement);
            statement.Dispose();
        }
    
        private void RunAssertionLastJoin(EPServiceProvider epService) {
            string statementString = "select irstream symbol, sum(price) as mysum from " +
                    typeof(SupportMarketDataBean).FullName + "#length(20) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "group by symbol " +
                    "output last every 6 events " +
                    "order by sum(price), symbol";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
    
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
    
            TryAssertionLast(epService, statement);
    
            statement.Dispose();
        }
    
        private void RunAssertionIteratorGroupByEventPerGroup(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "sumPrice"};
            string statementString = "select symbol, sum(price) as sumPrice from " +
                    typeof(SupportMarketDataBean).FullName + "#length(10) as one, " +
                    typeof(SupportBeanString).FullName + "#length(100) as two " +
                    "where one.symbol = two.TheString " +
                    "group by symbol " +
                    "order by symbol";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementString);
    
            epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));
            epService.EPRuntime.SendEvent(new SupportBeanString("CMU"));
            epService.EPRuntime.SendEvent(new SupportBeanString("KGB"));
            epService.EPRuntime.SendEvent(new SupportBeanString("DOG"));
    
            SendEvent(epService, "CAT", 50);
            SendEvent(epService, "IBM", 49);
            SendEvent(epService, "CAT", 15);
            SendEvent(epService, "IBM", 100);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields,
                    new object[][]{
                            new object[] {"CAT", 65d},
                            new object[] {"IBM", 149d},
                    });
    
            SendEvent(epService, "KGB", 75);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields,
                    new object[][]{
                            new object[] {"CAT", 65d},
                            new object[] {"IBM", 149d},
                            new object[] {"KGB", 75d},
                    });
    
            statement.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void TryAssertionLast(EPServiceProvider epService, EPStatement statement) {
            string[] fields = "symbol,mysum".Split(',');
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 1);
            SendEvent(epService, "CMU", 2);
            SendEvent(epService, "CAT", 5);
            SendEvent(epService, "CAT", 6);
    
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new object[][]{new object[] {"CMU", 3.0}, new object[] {"IBM", 7.0}, new object[] {"CAT", 11.0}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, fields,
                    new object[][]{new object[] {"CAT", null}, new object[] {"CMU", null}, new object[] {"IBM", null}});
    
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 5);
            SendEvent(epService, "CMU", 5);
            SendEvent(epService, "DOG", 0);
            SendEvent(epService, "DOG", 1);
    
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new object[][]{new object[] {"DOG", 1.0}, new object[] {"CMU", 13.0}, new object[] {"IBM", 14.0}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, fields,
                    new object[][]{new object[] {"DOG", null}, new object[] {"CMU", 3.0}, new object[] {"IBM", 7.0}});
        }
    
        private void TryAssertionNoHaving(EPServiceProvider epService, EPStatement statement) {
            string[] fields = "symbol,mysum".Split(',');
    
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 1);
            SendEvent(epService, "CMU", 2);
            SendEvent(epService, "CAT", 5);
            SendEvent(epService, "CAT", 6);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new object[][]{new object[] {"CMU", 1.0}, new object[] {"CMU", 3.0}, new object[] {"IBM", 3.0}, new object[] {"CAT", 5.0}, new object[] {"IBM", 7.0}, new object[] {"CAT", 11.0}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, fields,
                    new object[][]{new object[] {"CAT", null}, new object[] {"CMU", null}, new object[] {"IBM", null}, new object[] {"CMU", 1.0}, new object[] {"IBM", 3.0}, new object[] {"CAT", 5.0}});
            listener.Reset();
    
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 5);
            SendEvent(epService, "CMU", 5);
            SendEvent(epService, "DOG", 0);
            SendEvent(epService, "DOG", 1);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new object[][]{new object[] {"DOG", 0.0}, new object[] {"DOG", 1.0}, new object[] {"CMU", 8.0}, new object[] {"IBM", 10.0}, new object[] {"CMU", 13.0}, new object[] {"IBM", 14.0}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, fields,
                    new object[][]{new object[] {"DOG", null}, new object[] {"DOG", 0.0}, new object[] {"CMU", 3.0}, new object[] {"IBM", 7.0}, new object[] {"CMU", 8.0}, new object[] {"IBM", 10.0}});
        }
    
        private void TryAssertionHaving(EPServiceProvider epService, EPStatement statement) {
            string[] fields = "symbol,mysum".Split(',');
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 1);
            SendEvent(epService, "CMU", 2);
            SendEvent(epService, "CAT", 5);
            SendEvent(epService, "CAT", 6);
    
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new object[][]{new object[] {"CMU", 1.0}, new object[] {"CMU", 3.0}, new object[] {"IBM", 3.0}, new object[] {"CAT", 5.0}, new object[] {"IBM", 7.0}, new object[] {"CAT", 11.0}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, fields,
                    new object[][]{new object[] {"CMU", 1.0}, new object[] {"IBM", 3.0}, new object[] {"CAT", 5.0}});
            listener.Reset();
    
            SendEvent(epService, "IBM", 3);
            SendEvent(epService, "IBM", 4);
            SendEvent(epService, "CMU", 5);
            SendEvent(epService, "CMU", 5);
            SendEvent(epService, "DOG", 0);
            SendEvent(epService, "DOG", 1);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                    new object[][]{new object[] {"DOG", 1.0}, new object[] {"CMU", 8.0}, new object[] {"IBM", 10.0}, new object[] {"CMU", 13.0}, new object[] {"IBM", 14.0}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, fields,
                    new object[][]{new object[] {"CMU", 3.0}, new object[] {"IBM", 7.0}, new object[] {"CMU", 8.0}, new object[] {"IBM", 10.0}});
        }
    }
} // end of namespace
