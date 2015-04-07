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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewGroupWinSharedViewStartStop
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener = new SupportUpdateListener();
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestSharedView()
        {
            _epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(SubscriberName string, ValueInt float)");
            string query = "select SubscriberName, avg(ValueInt) "
                    + "from MyEvent.std:groupwin(SubscriberName).win:length(4)"
                    + "group by SubscriberName output snapshot every 1 events";
            string query2 = "select SubscriberName, avedev(ValueInt) "
                    + "from MyEvent.std:groupwin(SubscriberName).win:length(3) "
                    + "group by SubscriberName output snapshot every 1 events";
    
            string[] groups = {
                    "G_A", "G_A", "G_A", "G_A", "G_B", "G_B", "G_B", "G_B",
                    "G_B", "G_B", "G_B", "G_B", "G_B", "G_B", "G_B", "G_B",
                    "G_B", "G_B", "G_B", "G_B", "G_C", "G_C", "G_C", "G_C",
                    "G_D", "G_A", "G_D", "G_D", "G_A", "G_D", "G_D", "G_D",
                    "G_A", "G_A", "G_A", "G_A", "G_C", "G_C", "G_C", "G_C",
                    "G_D", "G_A", "G_D", "G_D", "G_D", "G_A", "G_D", "G_D",
                    "G_D", "G_E" };
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(query, "myquery");
            EPStatement statement2 = _epService.EPAdministrator.CreateEPL(query2, "myquery2");
            statement.AddListener(_listener);
            statement2.AddListener(_listener);
    
            int i = 0;
            foreach(var csv in groups)
            {
                var @event = new object[]{ csv, 0 };

                _epService.EPRuntime.SendEvent(@event, "MyEvent");
                
                i++;

                var stmt = _epService.EPAdministrator.GetStatement("myquery");
                if (i%6 == 0)
                {
                    stmt.Stop();
                }
                else if (i%6 == 4)
                {
                    stmt.Start();
                }
            }
        }
    }
}
