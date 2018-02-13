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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.bean.SupportBeanConstants;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternOperatorEveryDistinct : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportBean_B));
    
            RunAssertionExpireSeenBeforeKey(epService);
            RunAssertionEveryDistinctOverFilter(epService);
            RunAssertionRepeatOverDistinct(epService);
            RunAssertionTimerWithinOverDistinct(epService);
            RunAssertionEveryDistinctOverRepeat(epService);
            RunAssertionEveryDistinctOverTimerWithin(epService);
            RunAssertionEveryDistinctOverAnd(epService);
            RunAssertionEveryDistinctOverOr(epService);
            RunAssertionEveryDistinctOverNot(epService);
            RunAssertionEveryDistinctOverFollowedBy(epService);
            RunAssertionEveryDistinctWithinFollowedBy(epService);
            RunAssertionFollowedByWithDistinct(epService);
            RunAssertionInvalid(epService);
            RunAssertionMonthScoped(epService);
        }
    
        private void RunAssertionExpireSeenBeforeKey(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            string expression = "select * from pattern [every-Distinct(a.intPrimitive, 1 sec) a=SupportBean(theString like 'A%')]";
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString".Split(','), new Object[]{"A1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("A3", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString".Split(','), new Object[]{"A3"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A4", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A5", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
    
            epService.EPRuntime.SendEvent(new SupportBean("A4", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString".Split(','), new Object[]{"A4"});
            epService.EPRuntime.SendEvent(new SupportBean("A5", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString".Split(','), new Object[]{"A5"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A6", 1));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1999));
            epService.EPRuntime.SendEvent(new SupportBean("A7", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            epService.EPRuntime.SendEvent(new SupportBean("A7", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString".Split(','), new Object[]{"A7"});
    
            statement.Dispose();
        }
    
        private void RunAssertionEveryDistinctOverFilter(EPServiceProvider epService) {
            string expression = "select * from pattern [every-Distinct(intPrimitive) a=SupportBean]";
            RunEveryDistinctOverFilter(epService, expression);
    
            expression = "select * from pattern [every-Distinct(intPrimitive,2 minutes) a=SupportBean]";
            RunEveryDistinctOverFilter(epService, expression);
        }
    
        private void RunEveryDistinctOverFilter(EPServiceProvider epService, string expression) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("a.theString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            Assert.AreEqual("E3", listener.AssertOneGetNewAndReset().Get("a.theString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            Assert.AreEqual("E4", listener.AssertOneGetNewAndReset().Get("a.theString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E6", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E7", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E8", 0));
            Assert.AreEqual("E8", listener.AssertOneGetNewAndReset().Get("a.theString"));
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(expression);
            Assert.AreEqual(expression, model.ToEPL());
            epService.EPAdministrator.Create(model);
    
            statement.Dispose();
        }
    
        private void RunAssertionRepeatOverDistinct(EPServiceProvider epService) {
            string expression = "select * from pattern [[2] every-Distinct(a.intPrimitive) a=SupportBean]";
            RunRepeatOverDistinct(epService, expression);
    
            expression = "select * from pattern [[2] every-Distinct(a.intPrimitive, 1 hour) a=SupportBean]";
            RunRepeatOverDistinct(epService, expression);
        }
    
        private void RunRepeatOverDistinct(EPServiceProvider epService, string expression) {
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E1", theEvent.Get("a[0].theString"));
            Assert.AreEqual("E3", theEvent.Get("a[1].theString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionEveryDistinctOverRepeat(EPServiceProvider epService) {
            string expression = "select * from pattern [every-Distinct(a[0].intPrimitive) [2] a=SupportBean]";
            RunEveryDistinctOverRepeat(epService, expression);
    
            expression = "select * from pattern [every-Distinct(a[0].intPrimitive, a[0].intPrimitive, 1 hour) [2] a=SupportBean]";
            RunEveryDistinctOverRepeat(epService, expression);
        }
    
        private void RunEveryDistinctOverRepeat(EPServiceProvider epService, string expression) {
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E1", theEvent.Get("a[0].theString"));
            Assert.AreEqual("E2", theEvent.Get("a[1].theString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E6", 1));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E5", theEvent.Get("a[0].theString"));
            Assert.AreEqual("E6", theEvent.Get("a[1].theString"));
    
            statement.Dispose();
        }
    
        private void RunAssertionTimerWithinOverDistinct(EPServiceProvider epService) {
            // for 10 seconds, look for every distinct A
            string expression = "select * from pattern [(every-Distinct(a.intPrimitive) a=SupportBean) where timer:Within(10 sec)]";
            RunTimerWithinOverDistinct(epService, expression);
    
            expression = "select * from pattern [(every-Distinct(a.intPrimitive, 2 days 2 minutes) a=SupportBean) where timer:Within(10 sec)]";
            RunTimerWithinOverDistinct(epService, expression);
        }
    
        private void RunTimerWithinOverDistinct(EPServiceProvider epService, string expression) {
    
            SendTimer(0, epService);
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("a.theString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            Assert.AreEqual("E3", listener.AssertOneGetNewAndReset().Get("a.theString"));
    
            SendTimer(11000, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionEveryDistinctOverTimerWithin(EPServiceProvider epService) {
            string expression = "select * from pattern [every-Distinct(a.intPrimitive) (a=SupportBean where timer:Within(10 sec))]";
            RunEveryDistinctOverTimerWithin(epService, expression);
    
            expression = "select * from pattern [every-Distinct(a.intPrimitive, 1 hour) (a=SupportBean where timer:Within(10 sec))]";
            RunEveryDistinctOverTimerWithin(epService, expression);
        }
    
        private void RunEveryDistinctOverTimerWithin(EPServiceProvider epService, string expression) {
    
            SendTimer(0, epService);
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("a.theString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(5000, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            Assert.AreEqual("E3", listener.AssertOneGetNewAndReset().Get("a.theString"));
    
            SendTimer(10000, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(15000, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E7", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(20000, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E8", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(25000, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E9", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(50000, epService);
            epService.EPRuntime.SendEvent(new SupportBean("E10", 1));
            Assert.AreEqual("E10", listener.AssertOneGetNewAndReset().Get("a.theString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E11", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E12", 2));
            Assert.AreEqual("E12", listener.AssertOneGetNewAndReset().Get("a.theString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E13", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionEveryDistinctOverAnd(EPServiceProvider epService) {
            string expression = "select * from pattern [every-Distinct(a.intPrimitive, b.intPrimitive) (a=SupportBean(theString like 'A%') and b=SupportBean(theString like 'B%'))]";
            RunEveryDistinctOverAnd(epService, expression);
    
            expression = "select * from pattern [every-Distinct(a.intPrimitive, b.intPrimitive, 1 hour) (a=SupportBean(theString like 'A%') and b=SupportBean(theString like 'B%'))]";
            RunEveryDistinctOverAnd(epService, expression);
        }
    
        private void RunEveryDistinctOverAnd(EPServiceProvider epService, string expression) {
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean("B1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A1", "B1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A2", 1));
            epService.EPRuntime.SendEvent(new SupportBean("B2", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("A3", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B3", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A3", "B3"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A4", 1));
            epService.EPRuntime.SendEvent(new SupportBean("B4", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A4", "B4"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A5", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B5", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("A6", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B6", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A6", "B6"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A7", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B7", 20));
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionEveryDistinctOverOr(EPServiceProvider epService) {
            string expression = "select * from pattern [every-Distinct(coalesce(a.intPrimitive, 0) + coalesce(b.intPrimitive, 0)) (a=SupportBean(theString like 'A%') or b=SupportBean(theString like 'B%'))]";
            RunEveryDistinctOverOr(epService, expression);
    
            expression = "select * from pattern [every-Distinct(coalesce(a.intPrimitive, 0) + coalesce(b.intPrimitive, 0), 1 hour) (a=SupportBean(theString like 'A%') or b=SupportBean(theString like 'B%'))]";
            RunEveryDistinctOverOr(epService, expression);
        }
    
        private void RunEveryDistinctOverOr(EPServiceProvider epService, string expression) {
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A1", null});
    
            epService.EPRuntime.SendEvent(new SupportBean("B1", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{null, "B1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("B2", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("A3", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B3", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("B4", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{null, "B4"});
    
            epService.EPRuntime.SendEvent(new SupportBean("B5", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{null, "B5"});
    
            epService.EPRuntime.SendEvent(new SupportBean("B6", 3));
            epService.EPRuntime.SendEvent(new SupportBean("A4", 3));
            epService.EPRuntime.SendEvent(new SupportBean("A5", 4));
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionEveryDistinctOverNot(EPServiceProvider epService) {
            string expression = "select * from pattern [every-Distinct(a.intPrimitive) (a=SupportBean(theString like 'A%') and not SupportBean(theString like 'B%'))]";
            RunEveryDistinctOverNot(epService, expression);
    
            expression = "select * from pattern [every-Distinct(a.intPrimitive, 1 hour) (a=SupportBean(theString like 'A%') and not SupportBean(theString like 'B%'))]";
            RunEveryDistinctOverNot(epService, expression);
        }
    
        private void RunEveryDistinctOverNot(EPServiceProvider epService, string expression) {
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString".Split(','), new Object[]{"A1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("A3", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString".Split(','), new Object[]{"A3"});
    
            epService.EPRuntime.SendEvent(new SupportBean("B1", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("A4", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString".Split(','), new Object[]{"A4"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A5", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionEveryDistinctOverFollowedBy(EPServiceProvider epService) {
            string expression = "select * from pattern [every-Distinct(a.intPrimitive + b.intPrimitive) (a=SupportBean(theString like 'A%') -> b=SupportBean(theString like 'B%'))]";
            RunEveryDistinctOverFollowedBy(epService, expression);
    
            expression = "select * from pattern [every-Distinct(a.intPrimitive + b.intPrimitive, 1 hour) (a=SupportBean(theString like 'A%') -> b=SupportBean(theString like 'B%'))]";
            RunEveryDistinctOverFollowedBy(epService, expression);
        }
    
        private void RunEveryDistinctOverFollowedBy(EPServiceProvider epService, string expression) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean("B1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A1", "B1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A2", 1));
            epService.EPRuntime.SendEvent(new SupportBean("B2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("A3", 10));
            epService.EPRuntime.SendEvent(new SupportBean("B3", -8));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("A4", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B4", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A4", "B4"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A5", 3));
            epService.EPRuntime.SendEvent(new SupportBean("B5", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionEveryDistinctWithinFollowedBy(EPServiceProvider epService) {
            string expression = "select * from pattern [(every-Distinct(a.intPrimitive) a=SupportBean(theString like 'A%')) -> b=SupportBean(intPrimitive=a.intPrimitive)]";
            RunEveryDistinctWithinFollowedBy(epService, expression);
    
            expression = "select * from pattern [(every-Distinct(a.intPrimitive, 2 hours 1 minute) a=SupportBean(theString like 'A%')) -> b=SupportBean(intPrimitive=a.intPrimitive)]";
            RunEveryDistinctWithinFollowedBy(epService, expression);
        }
    
        private void RunEveryDistinctWithinFollowedBy(EPServiceProvider epService, string expression) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("B1", 0));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean("B2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A1", "B2"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("A3", 3));
            epService.EPRuntime.SendEvent(new SupportBean("A4", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("B3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A3", "B3"});
    
            epService.EPRuntime.SendEvent(new SupportBean("B4", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("B5", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A2", "B5"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A5", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B6", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("A6", 4));
            epService.EPRuntime.SendEvent(new SupportBean("B7", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A6", "B7"});
    
            statement.Dispose();
        }
    
        private void RunAssertionFollowedByWithDistinct(EPServiceProvider epService) {
            string expression = "select * from pattern [every-Distinct(a.intPrimitive) a=SupportBean(theString like 'A%') -> every-Distinct(b.intPrimitive) b=SupportBean(theString like 'B%')]";
            RunFollowedByWithDistinct(epService, expression);
    
            expression = "select * from pattern [every-Distinct(a.intPrimitive, 1 day) a=SupportBean(theString like 'A%') -> every-Distinct(b.intPrimitive) b=SupportBean(theString like 'B%')]";
            RunFollowedByWithDistinct(epService, expression);
        }
    
        private void RunFollowedByWithDistinct(EPServiceProvider epService, string expression) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("B1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A1", "B1"});
            epService.EPRuntime.SendEvent(new SupportBean("B2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A1", "B2"});
            epService.EPRuntime.SendEvent(new SupportBean("B3", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("A2", 1));
            epService.EPRuntime.SendEvent(new SupportBean("B4", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A1", "B4"});
    
            epService.EPRuntime.SendEvent(new SupportBean("A3", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B5", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.theString,b.theString".Split(','), new Object[]{"A3", "B5"});
            epService.EPRuntime.SendEvent(new SupportBean("B6", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("B7", 3));
            EventBean[] events = listener.GetAndResetLastNewData();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(events, "a.theString,b.theString".Split(','),
                    new Object[][]{new object[] {"A1", "B7"}, new object[] {"A3", "B7"}});
    
            statement.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            TryInvalid(epService, "a=A->every-Distinct(a.intPrimitive) B",
                    "Failed to validate pattern every-distinct expression 'a.intPrimitive': Failed to resolve property 'a.intPrimitive' to a stream or nested property in a stream [a=A->every-Distinct(a.intPrimitive) B]");
    
            TryInvalid(epService, "every-Distinct(dummy) A",
                    "Failed to validate pattern every-distinct expression 'dummy': Property named 'dummy' is not valid in any stream [every-Distinct(dummy) A]");
    
            TryInvalid(epService, "every-Distinct(2 sec) A",
                    "Every-distinct node requires one or more distinct-value expressions that each return non-constant result values [every-Distinct(2 sec) A]");
        }
    
        private void RunAssertionMonthScoped(EPServiceProvider epService) {
            string[] fields = "a.theString,a.intPrimitive".Split(',');
            var listener = new SupportUpdateListener();
    
            SendCurrentTime(epService, "2002-02-01T09:00:00.000");
            epService.EPAdministrator.CreateEPL("select * from pattern [every-Distinct(theString, 1 month) a=SupportBean]").AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            SendCurrentTimeWithMinus(epService, "2002-03-01T09:00:00.000", 1);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            Assert.IsFalse(listener.IsInvoked);
    
            SendCurrentTime(epService, "2002-03-01T09:00:00.000");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 4});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void SendCurrentTimeWithMinus(EPServiceProvider epService, string time, long minus) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }
    
        private void SendCurrentTime(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private static void TryInvalid(EPServiceProvider epService, string statement, string message) {
            try {
                epService.EPAdministrator.CreatePattern(statement);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void SendTimer(long timeInMSec, EPServiceProvider epService) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
