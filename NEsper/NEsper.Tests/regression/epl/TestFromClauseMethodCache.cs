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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestFromClauseMethodCache 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestLRUCache()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            ConfigurationMethodRef methodConfig = new ConfigurationMethodRef();
            methodConfig.SetLRUCache(3);
            config.AddMethodRef(typeof(SupportStaticMethodInvocations).FullName, methodConfig);
            config.AddImport(typeof(SupportStaticMethodInvocations).Namespace);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();

            String joinStatement = "select Id, P00, TheString from " +
                    typeof(SupportBean).FullName + "().win:length(100) as s1, " +
                    " method:SupportStaticMethodInvocations.FetchObjectLog(TheString, IntPrimitive)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
            stmt.Events += _listener.Update;
    
            // set sleep off
            SupportStaticMethodInvocations.GetInvocationSizeAndReset();
    
            // The LRU cache caches per same keys
            String[] fields = new String[] {"Id", "P00", "TheString"};
            SendBeanEvent("E1", 1);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {1, "|E1|", "E1"});
            
            SendBeanEvent("E2", 2);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {2, "|E2|", "E2"});
    
            SendBeanEvent("E3", 3);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {3, "|E3|", "E3"});
            Assert.AreEqual(3, SupportStaticMethodInvocations.GetInvocationSizeAndReset());
    
            // should be cached
            SendBeanEvent("E3", 3);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {3, "|E3|", "E3"});
            Assert.AreEqual(0, SupportStaticMethodInvocations.GetInvocationSizeAndReset());
    
            // should not be cached
            SendBeanEvent("E4", 4);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {4, "|E4|", "E4"});
            Assert.AreEqual(1, SupportStaticMethodInvocations.GetInvocationSizeAndReset());
    
            // should be cached
            SendBeanEvent("E2", 2);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {2, "|E2|", "E2"});
            Assert.AreEqual(0, SupportStaticMethodInvocations.GetInvocationSizeAndReset());
    
            // should not be cached
            SendBeanEvent("E1", 1);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {1, "|E1|", "E1"});
            Assert.AreEqual(1, SupportStaticMethodInvocations.GetInvocationSizeAndReset());

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestExpiryCache()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            ConfigurationMethodRef methodConfig = new ConfigurationMethodRef();
            methodConfig.SetExpiryTimeCache(1, 10);
            config.AddMethodRef(typeof(SupportStaticMethodInvocations).FullName, methodConfig);
            config.AddImport(typeof(SupportStaticMethodInvocations).Namespace);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();

            String joinStatement = "select Id, P00, TheString from " +
                    typeof(SupportBean).FullName + "().win:length(100) as s1, " +
                    " method:SupportStaticMethodInvocations.FetchObjectLog(TheString, IntPrimitive)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
            stmt.Events += _listener.Update;
    
            // set sleep off
            SupportStaticMethodInvocations.GetInvocationSizeAndReset();
    
            SendTimer(1000);
            String[] fields = new String[] {"Id", "P00", "TheString"};
            SendBeanEvent("E1", 1);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {1, "|E1|", "E1"});
    
            SendTimer(1500);
            SendBeanEvent("E2", 2);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {2, "|E2|", "E2"});
    
            SendTimer(2000);
            SendBeanEvent("E3", 3);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {3, "|E3|", "E3"});
            Assert.AreEqual(3, SupportStaticMethodInvocations.GetInvocationSizeAndReset());
    
            // should be cached
            SendBeanEvent("E3", 3);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {3, "|E3|", "E3"});
            Assert.AreEqual(0, SupportStaticMethodInvocations.GetInvocationSizeAndReset());
    
            SendTimer(2100);
            // should not be cached
            SendBeanEvent("E4", 4);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {4, "|E4|", "E4"});
            Assert.AreEqual(1, SupportStaticMethodInvocations.GetInvocationSizeAndReset());
    
            // should be cached
            SendBeanEvent("E2", 2);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {2, "|E2|", "E2"});
            Assert.AreEqual(0, SupportStaticMethodInvocations.GetInvocationSizeAndReset());
    
            // should not be cached
            SendBeanEvent("E1", 1);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {1, "|E1|", "E1"});
            Assert.AreEqual(1, SupportStaticMethodInvocations.GetInvocationSizeAndReset());

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void SendTimer(long timeInMSec)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendBeanEvent(string stringValue, int intPrimitive)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
