///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestOutputLimitFirstHaving  {
    
        private EPServiceProvider epService;
        private SupportUpdateListener listener;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableExecutionDebug = true;
            config.EngineDefaults.LoggingConfig.IsEnableTimerDebug = false;
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            listener = null;
        }
    
        [Test]
        public void TestHavingNoAvgOutputFirstEvents() {
            String query = "select DoublePrimitive from SupportBean having DoublePrimitive > 1 output first every 2 events";
            EPStatement statement = epService.EPAdministrator.CreateEPL(query);
            statement.Events += listener.Update;
            RunAssertion2Events();
            statement.Dispose();
    
            // test joined
            query = "select DoublePrimitive from SupportBean.std:lastevent(),SupportBean_ST0.std:lastevent() st0 having DoublePrimitive > 1 output first every 2 events";
            statement = epService.EPAdministrator.CreateEPL(query);
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ID", 1));
            statement.Events += listener.Update;
            RunAssertion2Events();
        }
    
        [Test]
        public void TestHavingNoAvgOutputFirstMinutes() {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            String[] fields = "val0".Split(',');
            String query = "select sum(DoublePrimitive) as val0 from SupportBean.win:length(5) having sum(DoublePrimitive) > 100 output first every 2 seconds";
            EPStatement statement = epService.EPAdministrator.CreateEPL(query);
            statement.Events += listener.Update;
    
            SendBeanEvent(10);
            SendBeanEvent(80);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            SendBeanEvent(11);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {101d});
    
            SendBeanEvent(1);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2999));
            SendBeanEvent(1);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
            SendBeanEvent(1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBeanEvent(100);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {114d});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(4999));
            SendBeanEvent(0);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000));
            SendBeanEvent(0);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {102d});
        }
    
        [Test]
        public void TestHavingAvgOutputFirstEveryTwoMinutes()
        {
            String query = "select DoublePrimitive, avg(DoublePrimitive) from SupportBean having DoublePrimitive > 2*Avg(DoublePrimitive) output first every 2 minutes";
            EPStatement statement = epService.EPAdministrator.CreateEPL(query);
            statement.Events += listener.Update;
    
            SendBeanEvent(1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBeanEvent(2);
            Assert.IsFalse(listener.IsInvoked);
        
            SendBeanEvent(9);
            Assert.IsTrue(listener.IsInvoked);
         }
    
    
        private void RunAssertion2Events() {
    
            SendBeanEvent(1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(2);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(9);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(2);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(2);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(2);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(2);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
        }
    
        private void SendBeanEvent(double doublePrimitive) {
            SupportBean b = new SupportBean();
            b.DoublePrimitive = doublePrimitive;
            epService.EPRuntime.SendEvent(b);
        }
    }
    
}
