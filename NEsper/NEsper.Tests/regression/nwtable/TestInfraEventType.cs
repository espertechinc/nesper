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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.events;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraEventType 
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
        public void TestEventType() {
            RunAssertionType(true);
            RunAssertionType(false);
        }
    
        private void RunAssertionType(bool namedWindow) {
            string eplCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as (c0 int[], c1 int[primitive])" :
                    "create table MyInfra (c0 int[], c1 int[primitive])";
            EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(epService, false, eplCreate);

            object[][] expectedType = new object[][] { new object[] { "c0", typeof(int[]) }, new object[] { "c1", typeof(int[]) } };
            EventTypeAssertionUtil.AssertEventTypeProperties(expectedType, stmt.EventType, EventTypeAssertionEnum.NAME, EventTypeAssertionEnum.TYPE);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    }
}
