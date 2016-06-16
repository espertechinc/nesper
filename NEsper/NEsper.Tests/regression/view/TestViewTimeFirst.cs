///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewTimeFirst 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestMonthScoped() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime("2002-02-01T9:00:00.000");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:firsttime(1 month)");
    
            SendCurrentTime("2002-02-15T9:00:00.000");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            SendCurrentTimeWithMinus("2002-03-01T9:00:00.000", 1);
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
    
            SendCurrentTime("2002-03-01T9:00:00.000");
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));

            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), "TheString".Split(','), new object[][] { new object[] { "E1" }, new object[] { "E2" } });
        }
    
        private void SendCurrentTime(string time) {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private void SendCurrentTimeWithMinus(string time, long minus) {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }
    }
}
