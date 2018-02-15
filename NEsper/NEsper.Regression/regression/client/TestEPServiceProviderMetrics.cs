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
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestEPServiceProviderMetrics
    {
        private const String FILTER_NAME = "\"com.espertech.esper-default\":type=\"filter\"";
        private const String RUNTIME_NAME = "\"com.espertech.esper-default\":type=\"runtime\"";
        private const String SCHEDULE_NAME = "\"com.espertech.esper-default\":type=\"schedule\"";
        private static readonly String[] ALL = new string[] {FILTER_NAME, RUNTIME_NAME, SCHEDULE_NAME};
    
        [Test]
        public void TestMetricsJMX() {
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.MetricsReporting.IsEnableMetricsReporting = true;
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec("2002-05-01T08:00:00.000")));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            epService.EPAdministrator.CreateEPL("select * from pattern [every a=SupportBean(TheString like 'A%') -> b=SupportBean(TheString like 'B') where timer:within(a.IntPrimitive)]");
            epService.EPRuntime.SendEvent(new SupportBean("A1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 60));
    
            AssertEngineJMX();
    
            epService.Dispose();
    
            //AssertNoEngineJMX();
    
            config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.MetricsReporting.IsEnableMetricsReporting = false;
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
    
            //AssertNoEngineJMX();
    
            epService.Dispose();
        }
    
        private void AssertEngineJMX() {
        }
    }
}
