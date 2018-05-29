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
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeIterator : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            RunAssertionPatternNoWindow(epService);
            RunAssertionPatternWithWindow(epService);
            RunAssertionOrderByWildcard(epService);
            RunAssertionOrderByProps(epService);
            RunAssertionFilter(epService);
            RunAssertionGroupByRowPerGroupOrdered(epService);
            RunAssertionGroupByRowPerGroup(epService);
            RunAssertionGroupByRowPerGroupHaving(epService);
            RunAssertionGroupByComplex(epService);
            RunAssertionGroupByRowPerEventOrdered(epService);
            RunAssertionGroupByRowPerEvent(epService);
            RunAssertionGroupByRowPerEventHaving(epService);
            RunAssertionAggregateAll(epService);
            RunAssertionAggregateAllOrdered(epService);
            RunAssertionAggregateAllHaving(epService);
            RunAssertionRowForAll(epService);
            RunAssertionRowForAllHaving(epService);
        }
    
        private void RunAssertionPatternNoWindow(EPServiceProvider epService) {
            // Test for Esper-115
            string cepStatementString = "@IterableUnbound select * from pattern " +
                    "[every ( addressInfo = " + typeof(SupportBean).FullName + "(TheString='address') " +
                    "-> txnWD = " + typeof(SupportBean).FullName + "(TheString='txn') ) ] " +
                    "where addressInfo.IntBoxed = txnWD.IntBoxed";
            EPStatement epStatement = epService.EPAdministrator.CreateEPL(cepStatementString);
    
            var myEventBean1 = new SupportBean();
            myEventBean1.TheString = "address";
            myEventBean1.IntBoxed = 9001;
            epService.EPRuntime.SendEvent(myEventBean1);
            Assert.IsFalse(epStatement.HasFirst());
    
            var myEventBean2 = new SupportBean();
            myEventBean2.TheString = "txn";
            myEventBean2.IntBoxed = 9001;
            epService.EPRuntime.SendEvent(myEventBean2);
            Assert.IsTrue(epStatement.HasFirst());
    
            IEnumerator<EventBean> itr = epStatement.GetEnumerator();
            Assert.IsTrue(itr.MoveNext());
            EventBean theEvent = itr.Current;
            Assert.AreEqual(myEventBean1, theEvent.Get("addressInfo"));
            Assert.AreEqual(myEventBean2, theEvent.Get("txnWD"));
    
            epStatement.Dispose();
        }
    
        private void RunAssertionPatternWithWindow(EPServiceProvider epService) {
            string cepStatementString = "select * from pattern " +
                    "[every ( addressInfo = " + typeof(SupportBean).FullName + "(TheString='address') " +
                    "-> txnWD = " + typeof(SupportBean).FullName + "(TheString='txn') ) ]#lastevent " +
                    "where addressInfo.IntBoxed = txnWD.IntBoxed";
            EPStatement epStatement = epService.EPAdministrator.CreateEPL(cepStatementString);
    
            var myEventBean1 = new SupportBean();
            myEventBean1.TheString = "address";
            myEventBean1.IntBoxed = 9001;
            epService.EPRuntime.SendEvent(myEventBean1);
    
            var myEventBean2 = new SupportBean();
            myEventBean2.TheString = "txn";
            myEventBean2.IntBoxed = 9001;
            epService.EPRuntime.SendEvent(myEventBean2);
    
            IEnumerator<EventBean> itr = epStatement.GetEnumerator();
            Assert.IsTrue(itr.MoveNext());
            EventBean theEvent = itr.Current;
            Assert.AreEqual(myEventBean1, theEvent.Get("addressInfo"));
            Assert.AreEqual(myEventBean2, theEvent.Get("txnWD"));
    
            epStatement.Dispose();
        }
    
        private void RunAssertionOrderByWildcard(EPServiceProvider epService) {
            string stmtText = "select * from " + typeof(SupportMarketDataBean).FullName + "#length(5) order by symbol, volume";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            object eventOne = SendEvent(epService, "SYM", 1);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{eventOne}, stmt.GetEnumerator());
    
            object eventTwo = SendEvent(epService, "OCC", 2);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{eventTwo, eventOne}, stmt.GetEnumerator());
    
            object eventThree = SendEvent(epService, "TOC", 3);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{eventTwo, eventOne, eventThree}, stmt.GetEnumerator());
    
            object eventFour = SendEvent(epService, "SYM", 0);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{eventTwo, eventFour, eventOne, eventThree}, stmt.GetEnumerator());
    
            object eventFive = SendEvent(epService, "SYM", 10);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{eventTwo, eventFour, eventOne, eventFive, eventThree}, stmt.GetEnumerator());
    
            object eventSix = SendEvent(epService, "SYM", 4);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new object[]{eventTwo, eventFour, eventSix, eventFive, eventThree}, stmt.GetEnumerator());
    
            stmt.Dispose();
        }
    
        private void RunAssertionOrderByProps(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "volume"};
            string stmtText = "select symbol, volume from " + typeof(SupportMarketDataBean).FullName + "#length(3) order by symbol, volume";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, "SYM", 1);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 1L}});
    
            SendEvent(epService, "OCC", 2);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"OCC", 2L}, new object[] {"SYM", 1L}});
    
            SendEvent(epService, "SYM", 0);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"OCC", 2L}, new object[] {"SYM", 0L}, new object[] {"SYM", 1L}});
    
            SendEvent(epService, "OCC", 3);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"OCC", 2L}, new object[] {"OCC", 3L}, new object[] {"SYM", 0L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionFilter(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "vol"};
            string stmtText = "select symbol, volume * 10 as vol from " + typeof(SupportMarketDataBean).FullName + "#length(5)" +
                    " where volume < 0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, "SYM", 100);
            Assert.IsFalse(stmt.HasFirst());
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendEvent(epService, "SYM", -1);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", -10L}});
    
            SendEvent(epService, "SYM", -6);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", -10L}, new object[] {"SYM", -60L}});
    
            SendEvent(epService, "SYM", 1);
            SendEvent(epService, "SYM", 16);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", -10L}, new object[] {"SYM", -60L}});
    
            SendEvent(epService, "SYM", -9);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", -10L}, new object[] {"SYM", -60L}, new object[] {"SYM", -90L}});
    
            SendEvent(epService, "SYM", 2);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", -60L}, new object[] {"SYM", -90L}});
    
            SendEvent(epService, "SYM", 3);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", -90L}});
    
            SendEvent(epService, "SYM", 4);
            SendEvent(epService, "SYM", 5);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", -90L}});
            SendEvent(epService, "SYM", 6);
            Assert.IsFalse(stmt.HasFirst());
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupByRowPerGroupOrdered(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "sumVol"};
            string stmtText = "select symbol, sum(volume) as sumVol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "group by symbol " +
                    "order by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, "SYM", 100);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 100L}});
    
            SendEvent(epService, "OCC", 5);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"OCC", 5L}, new object[] {"SYM", 100L}});
    
            SendEvent(epService, "SYM", 10);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"OCC", 5L}, new object[] {"SYM", 110L}});
    
            SendEvent(epService, "OCC", 6);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"OCC", 11L}, new object[] {"SYM", 110L}});
    
            SendEvent(epService, "ATB", 8);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"ATB", 8L}, new object[] {"OCC", 11L}, new object[] {"SYM", 110L}});
    
            SendEvent(epService, "ATB", 7);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"ATB", 15L}, new object[] {"OCC", 11L}, new object[] {"SYM", 10L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupByRowPerGroup(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "sumVol"};
            string stmtText = "select symbol, sum(volume) as sumVol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "group by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, "SYM", 100);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 100L}});
    
            SendEvent(epService, "SYM", 10);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 110L}});
    
            SendEvent(epService, "TAC", 1);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 110L}, new object[] {"TAC", 1L}});
    
            SendEvent(epService, "SYM", 11);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 121L}, new object[] {"TAC", 1L}});
    
            SendEvent(epService, "TAC", 2);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 121L}, new object[] {"TAC", 3L}});
    
            SendEvent(epService, "OCC", 55);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 21L}, new object[] {"TAC", 3L}, new object[] {"OCC", 55L}});
    
            SendEvent(epService, "OCC", 4);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"TAC", 3L}, new object[] {"SYM", 11L}, new object[] {"OCC", 59L}});
    
            SendEvent(epService, "OCC", 3);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 11L}, new object[] {"TAC", 2L}, new object[] {"OCC", 62L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupByRowPerGroupHaving(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "sumVol"};
            string stmtText = "select symbol, sum(volume) as sumVol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "group by symbol having sum(volume) > 10";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, "SYM", 100);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 100L}});
    
            SendEvent(epService, "SYM", 5);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 105L}});
    
            SendEvent(epService, "TAC", 1);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 105L}});
    
            SendEvent(epService, "SYM", 3);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 108L}});
    
            SendEvent(epService, "TAC", 12);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 108L}, new object[] {"TAC", 13L}});
    
            SendEvent(epService, "OCC", 55);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"TAC", 13L}, new object[] {"OCC", 55L}});
    
            SendEvent(epService, "OCC", 4);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"TAC", 13L}, new object[] {"OCC", 59L}});
    
            SendEvent(epService, "OCC", 3);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"TAC", 12L}, new object[] {"OCC", 62L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupByComplex(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "msg"};
            string stmtText = "insert into Cutoff " +
                    "select symbol, (Convert.ToString(count(*)) || 'x1000.0') as msg " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#groupwin(symbol)#length(1) " +
                    "where price - volume >= 1000.0 group by symbol having count(*) = 1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("SYM", -1, -1L, null));
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("SYM", 100000d, 0L, null));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", "1x1000.0"}});
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("SYM", 1d, 1L, null));
            Assert.IsFalse(stmt.HasFirst());
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupByRowPerEventOrdered(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "price", "sumVol"};
            string stmtText = "select symbol, price, sum(volume) as sumVol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "group by symbol " +
                    "order by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, "SYM", -1, 100);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", -1d, 100L}});
    
            SendEvent(epService, "TAC", -2, 12);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"SYM", -1d, 100L}, new object[] {"TAC", -2d, 12L}});
    
            SendEvent(epService, "TAC", -3, 13);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"SYM", -1d, 100L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}});
    
            SendEvent(epService, "SYM", -4, 1);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"SYM", -1d, 101L}, new object[] {"SYM", -4d, 101L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}});
    
            SendEvent(epService, "OCC", -5, 99);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"OCC", -5d, 99L}, new object[] {"SYM", -1d, 101L}, new object[] {"SYM", -4d, 101L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}});
    
            SendEvent(epService, "TAC", -6, 2);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"OCC", -5d, 99L}, new object[] {"SYM", -4d, 1L}, new object[] {"TAC", -2d, 27L}, new object[] {"TAC", -3d, 27L}, new object[] {"TAC", -6d, 27L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupByRowPerEvent(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "price", "sumVol"};
            string stmtText = "select symbol, price, sum(volume) as sumVol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "group by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, "SYM", -1, 100);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", -1d, 100L}});
    
            SendEvent(epService, "TAC", -2, 12);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"SYM", -1d, 100L}, new object[] {"TAC", -2d, 12L}});
    
            SendEvent(epService, "TAC", -3, 13);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"SYM", -1d, 100L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}});
    
            SendEvent(epService, "SYM", -4, 1);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"SYM", -1d, 101L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}, new object[] {"SYM", -4d, 101L}});
    
            SendEvent(epService, "OCC", -5, 99);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"SYM", -1d, 101L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}, new object[] {"SYM", -4d, 101L}, new object[] {"OCC", -5d, 99L}});
    
            SendEvent(epService, "TAC", -6, 2);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"TAC", -2d, 27L}, new object[] {"TAC", -3d, 27L}, new object[] {"SYM", -4d, 1L}, new object[] {"OCC", -5d, 99L}, new object[] {"TAC", -6d, 27L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupByRowPerEventHaving(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "price", "sumVol"};
            string stmtText = "select symbol, price, sum(volume) as sumVol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "group by symbol having sum(volume) > 20";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, "SYM", -1, 100);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", -1d, 100L}});
    
            SendEvent(epService, "TAC", -2, 12);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"SYM", -1d, 100L}});
    
            SendEvent(epService, "TAC", -3, 13);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"SYM", -1d, 100L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}});
    
            SendEvent(epService, "SYM", -4, 1);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"SYM", -1d, 101L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}, new object[] {"SYM", -4d, 101L}});
    
            SendEvent(epService, "OCC", -5, 99);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"SYM", -1d, 101L}, new object[] {"TAC", -2d, 25L}, new object[] {"TAC", -3d, 25L}, new object[] {"SYM", -4d, 101L}, new object[] {"OCC", -5d, 99L}});
    
            SendEvent(epService, "TAC", -6, 2);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"TAC", -2d, 27L}, new object[] {"TAC", -3d, 27L}, new object[] {"OCC", -5d, 99L}, new object[] {"TAC", -6d, 27L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionAggregateAll(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "sumVol"};
            string stmtText = "select symbol, sum(volume) as sumVol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, "SYM", 100);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 100L}});
    
            SendEvent(epService, "TAC", 1);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 101L}, new object[] {"TAC", 101L}});
    
            SendEvent(epService, "MOV", 3);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 104L}, new object[] {"TAC", 104L}, new object[] {"MOV", 104L}});
    
            SendEvent(epService, "SYM", 10);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"TAC", 14L}, new object[] {"MOV", 14L}, new object[] {"SYM", 14L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionAggregateAllOrdered(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "sumVol"};
            string stmtText = "select symbol, sum(volume) as sumVol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
                    " order by symbol asc";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, "SYM", 100);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 100L}});
    
            SendEvent(epService, "TAC", 1);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 101L}, new object[] {"TAC", 101L}});
    
            SendEvent(epService, "MOV", 3);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"MOV", 104L}, new object[] {"SYM", 104L}, new object[] {"TAC", 104L}});
    
            SendEvent(epService, "SYM", 10);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"MOV", 14L}, new object[] {"SYM", 14L}, new object[] {"TAC", 14L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionAggregateAllHaving(EPServiceProvider epService) {
            var fields = new string[]{"symbol", "sumVol"};
            string stmtText = "select symbol, sum(volume) as sumVol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) having sum(volume) > 100";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, "SYM", 100);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, "TAC", 1);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 101L}, new object[] {"TAC", 101L}});
    
            SendEvent(epService, "MOV", 3);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"SYM", 104L}, new object[] {"TAC", 104L}, new object[] {"MOV", 104L}});
    
            SendEvent(epService, "SYM", 10);
            Assert.IsFalse(stmt.HasFirst());
    
            stmt.Dispose();
        }
    
        private void RunAssertionRowForAll(EPServiceProvider epService) {
            var fields = new string[]{"sumVol"};
            string stmtText = "select sum(volume) as sumVol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {null}});
    
            SendEvent(epService, 100);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {100L}});
    
            SendEvent(epService, 50);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {150L}});
    
            SendEvent(epService, 25);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {175L}});
    
            SendEvent(epService, 10);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {85L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionRowForAllHaving(EPServiceProvider epService) {
            var fields = new string[]{"sumVol"};
            string stmtText = "select sum(volume) as sumVol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) having sum(volume) > 100";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, 100);
            Assert.IsFalse(stmt.HasFirst());
    
            SendEvent(epService, 50);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {150L}});
    
            SendEvent(epService, 25);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {175L}});
    
            SendEvent(epService, 10);
            Assert.IsFalse(stmt.HasFirst());
    
            stmt.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price, long volume) {
            epService.EPRuntime.SendEvent(new SupportMarketDataBean(symbol, price, volume, null));
        }
    
        private SupportMarketDataBean SendEvent(EPServiceProvider epService, string symbol, long volume) {
            var theEvent = new SupportMarketDataBean(symbol, 0, volume, null);
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private void SendEvent(EPServiceProvider epService, long volume) {
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("SYM", 0, volume, null));
        }
    }
} // end of namespace
