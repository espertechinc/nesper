///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.time;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientEPServiceProviderMetricsJMX : RegressionExecution {
    
        private static readonly string ENGINE_NAME = typeof(ExecClientEPServiceProviderMetricsJMX).Name;
        private static readonly string FILTER_NAME = "\"com.espertech.esper-" + ENGINE_NAME + "\":type=\"filter\"";
        private static readonly string RUNTIME_NAME = "\"com.espertech.esper-" + ENGINE_NAME + "\":type=\"runtime\"";
        private static readonly string SCHEDULE_NAME = "\"com.espertech.esper-" + ENGINE_NAME + "\":type=\"schedule\"";
        private static readonly string[] ALL = new string[]{FILTER_NAME, RUNTIME_NAME, SCHEDULE_NAME};
    
        public override void Run(EPServiceProvider defaultEPService) {
#if NOT_SUPPORTED_IN_DOTNET
            AssertNoEngineJMX();
    
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.MetricsReporting.JmxEngineMetrics = true;
            EPServiceProvider epService = EPServiceProviderManager.GetProvider(ENGINE_NAME, configuration);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec("2002-05-1T08:00:00.000")));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            epService.EPAdministrator.CreateEPL("select * from pattern [every a=SupportBean(theString like 'A%') -> b=SupportBean(theString like 'B') where timer:Within(a.intPrimitive)]");
            epService.EPRuntime.SendEvent(new SupportBean("A1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 60));
    
            AssertEngineJMX();
    
            epService.Dispose();
    
            AssertNoEngineJMX();
#endif
        }

#if NOT_SUPPORTED_IN_DOTNET
        private void AssertEngineJMX() {
            foreach (string name in ALL) {
                AssertJMXVisible(name);
            }
        }
    
        private void AssertJMXVisible(string name) {
            ManagementFactory.PlatformMBeanServer.GetObjectInstance(new ObjectName(name));
        }
    
        private void AssertNoEngineJMX() {
            foreach (string name in ALL) {
                AssertJMXNotVisible(name);
            }
        }
    
        private void AssertJMXNotVisible(string name) {
            try {
                ManagementFactory.PlatformMBeanServer.GetObjectInstance(new ObjectName(name));
                Assert.Fail();
            } catch (InstanceNotFoundException ex) {
                // expected
            }
        }
#endif
    }
} // end of namespace
