///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.time;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestEPServiceProviderMetricsJMX  {
    
        private readonly static String FILTER_NAME = "\"com.espertech.esper-default\":type=\"filter\"";
        private readonly static String RUNTIME_NAME = "\"com.espertech.esper-default\":type=\"runtime\"";
        private readonly static String SCHEDULE_NAME = "\"com.espertech.esper-default\":type=\"schedule\"";
        private readonly static String[] ALL = new String[] {FILTER_NAME, RUNTIME_NAME, SCHEDULE_NAME};
    
        [Test]
        public void TestMetricsJMX() {
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.MetricsReporting.JmxEngineMetrics = true;
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTime.ParseDefaultMSec("2002-05-1T8:00:00.000")));
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
    
            epService.EPAdministrator.CreateEPL("select * from pattern [every a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B') where timer:within(a.IntPrimitive)]");
            epService.EPRuntime.SendEvent(new SupportBean("A1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 60));
    
            AssertEngineJMX();
    
            epService.Dispose();
    
            AssertNoEngineJMX();
    
            config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.MetricsReporting.JmxEngineMetrics = false;
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
    
            AssertNoEngineJMX();
    
            epService.Dispose();
        }
    
        private void AssertEngineJMX() {
            foreach (String name in ALL) {
                AssertJMXVisible(name);
            }
        }
    
        private void AssertNoEngineJMX() {
            foreach (String name in ALL) {
                AssertJMXNotVisible(name);
            }
        }
    
        private void AssertJMXVisible(String name) {
            ManagementFactory.PlatformMBeanServer.GetObjectInstance(new ObjectName(name));
        }
    
        private void AssertJMXNotVisible(String name) {
            try {
                ManagementFactory.PlatformMBeanServer.GetObjectInstance(new ObjectName(name));
                Assert.Fail();
            }
            catch (InstanceNotFoundException ex) {
                // expected
            }
        }
    }
}
