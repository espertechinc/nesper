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
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestPrioritizedExec
    {
        private EPServiceProvider _epService;
    
        [Test]
        public void TestWithPrioritizedExec()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.ExecutionConfig.IsPrioritized = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddEventType(typeof(Event));
    
            SendTimeEvent("2002-05-1 10:00:00.000");
            
            const string epl = "\n @Name('ctx') create context RuleActivityTime as start (0, 9, *, *, *) end (0, 17, *, *, *);" +
                               "\n @Name('window') context RuleActivityTime create window EventsWindow.std:firstunique(productID) as Event;" +
                               "\n @Name('variable') create variable boolean IsOutputTriggered_2 = false;" +
                               "\n @Name('A') insert into EventsWindow select * from Event(not exists (select * from EventsWindow));" +
                               "\n @Name('B') insert into EventsWindow select * from Event(not exists (select * from EventsWindow));" +
                               "\n @Name('C') insert into EventsWindow select * from Event(not exists (select * from EventsWindow));" +
                               "\n @Name('D') insert into EventsWindow select * from Event(not exists (select * from EventsWindow));" +
                               "\n @Name('out') context RuleActivityTime select * from EventsWindow";
    
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            _epService.EPAdministrator.GetStatement("out").Events +=
                new SupportUpdateListener().Update;
    
            _epService.EPRuntime.SendEvent(new Event("A1"));
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
        
        private void SendTimeEvent(String time) {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeHelper.ParseDefaultMSec(time)));
        }

        public class Event
        {
            public Event(String productId)
            {
                ProductID = productId;
            }

            public string ProductID { get; private set; }
        }
    }
}
