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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestEveryDistinct : SupportBeanConstants
    {
        [Test]
        public void TestExpireSeenBeforeKey()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }
    
            engine.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var expression = "select * from pattern [every-distinct(a.IntPrimitive, 1 sec) a=SupportBean(TheString like 'A%')]";
            var statement = engine.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            engine.EPRuntime.SendEvent(new SupportBean("A1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString".Split(','), new Object[]{"A1"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("A3", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString".Split(','), new Object[]{"A3"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A4", 1));
            engine.EPRuntime.SendEvent(new SupportBean("A5", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
    
            engine.EPRuntime.SendEvent(new SupportBean("A4", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString".Split(','), new Object[]{"A4"});
            engine.EPRuntime.SendEvent(new SupportBean("A5", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString".Split(','), new Object[]{"A5"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A6", 1));
            engine.EPRuntime.SendEvent(new CurrentTimeEvent(1999));
            engine.EPRuntime.SendEvent(new SupportBean("A7", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            engine.EPRuntime.SendEvent(new SupportBean("A7", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString".Split(','), new Object[]{"A7"});

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestEveryDistinctOverFilter()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }
    
            var expression = "select * from pattern [every-distinct(IntPrimitive) a=SupportBean]";
            RunEveryDistinctOverFilter(engine, expression);
    
            expression = "select * from pattern [every-distinct(IntPrimitive,2 minutes) a=SupportBean]";
            RunEveryDistinctOverFilter(engine, expression);
        }
    
        private void RunEveryDistinctOverFilter(EPServiceProvider engine, String expression)
        {
            var statement = engine.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            engine.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("a.TheString"));
    
            engine.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("E3", 2));
            Assert.AreEqual("E3", listener.AssertOneGetNewAndReset().Get("a.TheString"));
    
            engine.EPRuntime.SendEvent(new SupportBean("E4", 3));
            Assert.AreEqual("E4", listener.AssertOneGetNewAndReset().Get("a.TheString"));
    
            engine.EPRuntime.SendEvent(new SupportBean("E5", 2));
            engine.EPRuntime.SendEvent(new SupportBean("E6", 3));
            engine.EPRuntime.SendEvent(new SupportBean("E7", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("E8", 0));
            Assert.AreEqual("E8", listener.AssertOneGetNewAndReset().Get("a.TheString"));
    
            var model = engine.EPAdministrator.CompileEPL(expression);
            Assert.AreEqual(expression, model.ToEPL());
            engine.EPAdministrator.Create(model);
    
            statement.Dispose();
        }
    
        [Test]
        public void TestRepeatOverDistinct()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }
    
            var expression = "select * from pattern [[2] every-distinct(a.IntPrimitive) a=SupportBean]";
            RunRepeatOverDistinct(engine, expression);
    
            expression = "select * from pattern [[2] every-distinct(a.IntPrimitive, 1 hour) a=SupportBean]";
            RunRepeatOverDistinct(engine, expression);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void RunRepeatOverDistinct(EPServiceProvider engine, String expression)
        {
            var statement = engine.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            engine.EPRuntime.SendEvent(new SupportBean("E1", 1));
            engine.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("E3", 2));
            var theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E1", theEvent.Get("a[0].TheString"));
            Assert.AreEqual("E3", theEvent.Get("a[1].TheString"));
    
            engine.EPRuntime.SendEvent(new SupportBean("E4", 3));
            engine.EPRuntime.SendEvent(new SupportBean("E5", 2));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        [Test]
        public void TestEveryDistinctOverRepeat()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }

            var expression = "select * from pattern [every-distinct(a[0].IntPrimitive) [2] a=SupportBean]";
            RunEveryDistinctOverRepeat(engine, expression);
    
            expression = "select * from pattern [every-distinct(a[0].IntPrimitive, a[0].IntPrimitive, 1 hour) [2] a=SupportBean]";
            RunEveryDistinctOverRepeat(engine, expression);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void RunEveryDistinctOverRepeat(EPServiceProvider engine, String expression)
        {
            var statement = engine.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            engine.EPRuntime.SendEvent(new SupportBean("E1", 1));
            engine.EPRuntime.SendEvent(new SupportBean("E2", 1));
            var theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E1", theEvent.Get("a[0].TheString"));
            Assert.AreEqual("E2", theEvent.Get("a[1].TheString"));
    
            engine.EPRuntime.SendEvent(new SupportBean("E3", 1));
            engine.EPRuntime.SendEvent(new SupportBean("E4", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("E5", 2));
            engine.EPRuntime.SendEvent(new SupportBean("E6", 1));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E5", theEvent.Get("a[0].TheString"));
            Assert.AreEqual("E6", theEvent.Get("a[1].TheString"));
        }
    
        [Test]
        public void TestTimerWithinOverDistinct()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }

            // for 10 seconds, look for every distinct A
            var expression = "select * from pattern [(every-distinct(a.IntPrimitive) a=SupportBean) where timer:within(10 sec)]";
            RunTimerWithinOverDistinct(engine, expression);
    
            expression = "select * from pattern [(every-distinct(a.IntPrimitive, 2 days 2 minutes) a=SupportBean) where timer:within(10 sec)]";
            RunTimerWithinOverDistinct(engine, expression);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void RunTimerWithinOverDistinct(EPServiceProvider engine, String expression)
        {
            SendTimer(0, engine);
            var statement = engine.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            engine.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("a.TheString"));
    
            engine.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("E3", 2));
            Assert.AreEqual("E3", listener.AssertOneGetNewAndReset().Get("a.TheString"));
    
            SendTimer(11000, engine);
            engine.EPRuntime.SendEvent(new SupportBean("E4", 3));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("E5", 1));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        [Test]
        public void TestEveryDistinctOverTimerWithin()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }

            var expression = "select * from pattern [every-distinct(a.IntPrimitive) (a=SupportBean where timer:within(10 sec))]";
            RunEveryDistinctOverTimerWithin(engine, expression);
    
            expression = "select * from pattern [every-distinct(a.IntPrimitive, 1 hour) (a=SupportBean where timer:within(10 sec))]";
            RunEveryDistinctOverTimerWithin(engine, expression);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void RunEveryDistinctOverTimerWithin(EPServiceProvider engine, String expression)
        {
            SendTimer(0, engine);
            var statement = engine.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            engine.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("a.TheString"));
    
            engine.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(5000, engine);
            engine.EPRuntime.SendEvent(new SupportBean("E3", 2));
            Assert.AreEqual("E3", listener.AssertOneGetNewAndReset().Get("a.TheString"));
    
            SendTimer(10000, engine);
            engine.EPRuntime.SendEvent(new SupportBean("E4", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("E5", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("E6", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(15000, engine);
            engine.EPRuntime.SendEvent(new SupportBean("E7", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(20000, engine);
            engine.EPRuntime.SendEvent(new SupportBean("E8", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(25000, engine);
            engine.EPRuntime.SendEvent(new SupportBean("E9", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(50000, engine);
            engine.EPRuntime.SendEvent(new SupportBean("E10", 1));
            Assert.AreEqual("E10", listener.AssertOneGetNewAndReset().Get("a.TheString"));
    
            engine.EPRuntime.SendEvent(new SupportBean("E11", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("E12", 2));
            Assert.AreEqual("E12", listener.AssertOneGetNewAndReset().Get("a.TheString"));
    
            engine.EPRuntime.SendEvent(new SupportBean("E13", 2));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        [Test]
        public void TestEveryDistinctOverAnd()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }

            var expression = "select * from pattern [every-distinct(a.IntPrimitive, b.IntPrimitive) (a=SupportBean(TheString like 'A%') and b=SupportBean(TheString like 'B%'))]";
            RunEveryDistinctOverAnd(engine, expression);
    
            expression = "select * from pattern [every-distinct(a.IntPrimitive, b.IntPrimitive, 1 hour) (a=SupportBean(TheString like 'A%') and b=SupportBean(TheString like 'B%'))]";
            RunEveryDistinctOverAnd(engine, expression);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void RunEveryDistinctOverAnd(EPServiceProvider engine, String expression)
        {
            var statement = engine.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            engine.EPRuntime.SendEvent(new SupportBean("A1", 1));
            Assert.IsFalse(listener.IsInvoked);
            engine.EPRuntime.SendEvent(new SupportBean("B1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A1", "B1"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A2", 1));
            engine.EPRuntime.SendEvent(new SupportBean("B2", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("A3", 2));
            engine.EPRuntime.SendEvent(new SupportBean("B3", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A3", "B3"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A4", 1));
            engine.EPRuntime.SendEvent(new SupportBean("B4", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A4", "B4"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A5", 2));
            engine.EPRuntime.SendEvent(new SupportBean("B5", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("A6", 2));
            engine.EPRuntime.SendEvent(new SupportBean("B6", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A6", "B6"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A7", 2));
            engine.EPRuntime.SendEvent(new SupportBean("B7", 20));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        [Test]
        public void TestEveryDistinctOverOr()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }

            var expression = "select * from pattern [every-distinct(coalesce(a.IntPrimitive, 0) + coalesce(b.IntPrimitive, 0)) (a=SupportBean(TheString like 'A%') or b=SupportBean(TheString like 'B%'))]";
            RunEveryDistinctOverOr(engine, expression);
    
            expression = "select * from pattern [every-distinct(coalesce(a.IntPrimitive, 0) + coalesce(b.IntPrimitive, 0), 1 hour) (a=SupportBean(TheString like 'A%') or b=SupportBean(TheString like 'B%'))]";
            RunEveryDistinctOverOr(engine, expression);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void RunEveryDistinctOverOr(EPServiceProvider engine, String expression)
        {
            var statement = engine.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            engine.EPRuntime.SendEvent(new SupportBean("A1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A1", null});
    
            engine.EPRuntime.SendEvent(new SupportBean("B1", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{null, "B1"});
    
            engine.EPRuntime.SendEvent(new SupportBean("B2", 1));
            engine.EPRuntime.SendEvent(new SupportBean("A2", 2));
            engine.EPRuntime.SendEvent(new SupportBean("A3", 2));
            engine.EPRuntime.SendEvent(new SupportBean("B3", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("B4", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{null, "B4"});
    
            engine.EPRuntime.SendEvent(new SupportBean("B5", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{null, "B5"});
    
            engine.EPRuntime.SendEvent(new SupportBean("B6", 3));
            engine.EPRuntime.SendEvent(new SupportBean("A4", 3));
            engine.EPRuntime.SendEvent(new SupportBean("A5", 4));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        [Test]
        public void TestEveryDistinctOverNot()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }

            var expression = "select * from pattern [every-distinct(a.IntPrimitive) (a=SupportBean(TheString like 'A%') and not SupportBean(TheString like 'B%'))]";
            RunEveryDistinctOverNot(engine, expression);
    
            expression = "select * from pattern [every-distinct(a.IntPrimitive, 1 hour) (a=SupportBean(TheString like 'A%') and not SupportBean(TheString like 'B%'))]";
            RunEveryDistinctOverNot(engine, expression);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void RunEveryDistinctOverNot(EPServiceProvider engine, String expression)
        {
            var statement = engine.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            engine.EPRuntime.SendEvent(new SupportBean("A1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString".Split(','), new Object[]{"A1"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("A3", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString".Split(','), new Object[]{"A3"});
    
            engine.EPRuntime.SendEvent(new SupportBean("B1", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("A4", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString".Split(','), new Object[]{"A4"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A5", 1));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        [Test]
        public void TestEveryDistinctOverFollowedBy()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }

            var expression = "select * from pattern [every-distinct(a.IntPrimitive + b.IntPrimitive) (a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%'))]";
            RunEveryDistinctOverFollowedBy(engine, expression);
    
            expression = "select * from pattern [every-distinct(a.IntPrimitive + b.IntPrimitive, 1 hour) (a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B%'))]";
            RunEveryDistinctOverFollowedBy(engine, expression);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void RunEveryDistinctOverFollowedBy(EPServiceProvider engine, String expression)
        {
            var statement = engine.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            engine.EPRuntime.SendEvent(new SupportBean("A1", 1));
            Assert.IsFalse(listener.IsInvoked);
            engine.EPRuntime.SendEvent(new SupportBean("B1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A1", "B1"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A2", 1));
            engine.EPRuntime.SendEvent(new SupportBean("B2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("A3", 10));
            engine.EPRuntime.SendEvent(new SupportBean("B3", -8));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("A4", 2));
            engine.EPRuntime.SendEvent(new SupportBean("B4", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A4", "B4"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A5", 3));
            engine.EPRuntime.SendEvent(new SupportBean("B5", 0));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        [Test]
        public void TestEveryDistinctWithinFollowedBy()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }

            var expression = "select * from pattern [(every-distinct(a.IntPrimitive) a=SupportBean(TheString like 'A%')) -> b=SupportBean(IntPrimitive=a.IntPrimitive)]";
            RunEveryDistinctWithinFollowedBy(engine, expression);
    
            expression = "select * from pattern [(every-distinct(a.IntPrimitive, 2 hours 1 minute) a=SupportBean(TheString like 'A%')) -> b=SupportBean(IntPrimitive=a.IntPrimitive)]";
            RunEveryDistinctWithinFollowedBy(engine, expression);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void RunEveryDistinctWithinFollowedBy(EPServiceProvider engine, String expression)
        {
            var statement = engine.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            engine.EPRuntime.SendEvent(new SupportBean("A1", 1));
            engine.EPRuntime.SendEvent(new SupportBean("B1", 0));
            Assert.IsFalse(listener.IsInvoked);
            engine.EPRuntime.SendEvent(new SupportBean("B2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A1", "B2"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A2", 2));
            engine.EPRuntime.SendEvent(new SupportBean("A3", 3));
            engine.EPRuntime.SendEvent(new SupportBean("A4", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("B3", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A3", "B3"});
    
            engine.EPRuntime.SendEvent(new SupportBean("B4", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("B5", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A2", "B5"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A5", 2));
            engine.EPRuntime.SendEvent(new SupportBean("B6", 2));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("A6", 4));
            engine.EPRuntime.SendEvent(new SupportBean("B7", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A6", "B7"});
        }
    
        [Test]
        public void TestFollowedByWithDistinct()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }

            var expression = "select * from pattern [every-distinct(a.IntPrimitive) a=SupportBean(TheString like 'A%') -> every-distinct(b.IntPrimitive) b=SupportBean(TheString like 'B%')]";
            RunFollowedByWithDistinct(engine, expression);
    
            expression = "select * from pattern [every-distinct(a.IntPrimitive, 1 day) a=SupportBean(TheString like 'A%') -> every-distinct(b.IntPrimitive) b=SupportBean(TheString like 'B%')]";
            RunFollowedByWithDistinct(engine, expression);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void RunFollowedByWithDistinct(EPServiceProvider engine, String expression)
        {
            var statement = engine.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            engine.EPRuntime.SendEvent(new SupportBean("A1", 1));
            engine.EPRuntime.SendEvent(new SupportBean("B1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A1", "B1"});
            engine.EPRuntime.SendEvent(new SupportBean("B2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A1", "B2"});
            engine.EPRuntime.SendEvent(new SupportBean("B3", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("A2", 1));
            engine.EPRuntime.SendEvent(new SupportBean("B4", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A1", "B4"});
    
            engine.EPRuntime.SendEvent(new SupportBean("A3", 2));
            engine.EPRuntime.SendEvent(new SupportBean("B5", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.TheString,b.TheString".Split(','), new Object[]{"A3", "B5"});
            engine.EPRuntime.SendEvent(new SupportBean("B6", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            engine.EPRuntime.SendEvent(new SupportBean("B7", 3));
            var events = listener.GetAndResetLastNewData();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(events, "a.TheString,b.TheString".SplitCsv(),
                new Object[][]
                {
                    new Object[]{ "A1", "B7" },
                    new Object[]{ "A3", "B7" }
                });
        }
    
        [Test]
        public void TestInvalid()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("A", typeof(SupportBean_A));
            config.AddEventType("B", typeof(SupportBean_B));
            var engine = EPServiceProviderManager.GetDefaultProvider(config);
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }

            TryInvalid(engine, "a=A->every-distinct(a.IntPrimitive) B",
                    "Failed to validate pattern every-distinct expression 'a.IntPrimitive': Failed to resolve property 'a.IntPrimitive' to a stream or nested property in a stream [a=A->every-distinct(a.IntPrimitive) B]");
    
            TryInvalid(engine, "every-distinct(dummy) A",
                    "Failed to validate pattern every-distinct expression 'dummy': Property named 'dummy' is not valid in any stream [every-distinct(dummy) A]");
    
            TryInvalid(engine, "every-distinct(2 sec) A",
                    "Every-distinct node requires one or more distinct-value expressions that each return non-constant result values [every-distinct(2 sec) A]");

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestMonthScoped()
        {
            String[] fields = "a.TheString,a.IntPrimitive".Split(',');
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(
                SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SupportUpdateListener listener = new SupportUpdateListener();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            SendCurrentTime(epService, "2002-02-01T09:00:00.000");
            epService.EPAdministrator.CreateEPL("select * from pattern [every-distinct(TheString, 1 month) a=SupportBean]").Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 1});

            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            SendCurrentTimeWithMinus(epService, "2002-03-01T09:00:00.000", 1);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            Assert.IsFalse(listener.IsInvoked);

            SendCurrentTime(epService, "2002-03-01T09:00:00.000");

            epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", 4});

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }

        private void SendCurrentTimeWithMinus(EPServiceProvider epService, String time, long minus) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }

        private void SendCurrentTime(EPServiceProvider epService, String time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }

        public void TryInvalid(EPServiceProvider engine, String statement, String message)
        {
            try
            {
                engine.EPAdministrator.CreatePattern(statement);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        private void SendTimer(long timeInMSec, EPServiceProvider epService)
        {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            var runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
}
