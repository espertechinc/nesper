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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowIndex 
    {
        private EPServiceProviderSPI epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            epService = (EPServiceProviderSPI) EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
        }
        
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestUniqueIndexUniqueView() {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatement stmtWindow = epService.EPAdministrator.CreateEPL("create window MyWindowOne#unique(TheString) as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowOne select * from SupportBean");
            epService.EPAdministrator.CreateEPL("create unique index I1 on MyWindowOne(TheString)");
    
            epService.EPRuntime.SendEvent(new SupportBean("E0", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
            epService.EPRuntime.SendEvent(new SupportBean("E0", 5));

            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtWindow.GetEnumerator(), "TheString,IntPrimitive".Split(','), new object[][] { new object[] { "E0", 5 }, new object[] { "E1", 4 }, new object[] { "E2", 3 } });
        }
    }
}
