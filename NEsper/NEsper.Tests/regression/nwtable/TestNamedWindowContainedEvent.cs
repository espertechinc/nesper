///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.named;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.bookexample;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowContainedEvent 
    {
        private EPServiceProviderSPI epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Logging.IsEnableQueryPlan = true;
            epService = (EPServiceProviderSPI) EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(OrderBean));
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestInvalid() {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
    
            epService.EPAdministrator.CreateEPL("create window OrderWindow.win:time(30) as OrderBean");
    
            try {
                string epl = "select * from SupportBean unidirectional, OrderWindow[books]";
                epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to validate named window use in join, contained-event is only allowed for named windows when marked as unidirectional [select * from SupportBean unidirectional, OrderWindow[books]]", ex.Message);
            }
    
            try {
                string epl = "select *, (select bookId from OrderWindow[books] where sb.TheString = bookId) " +
                        "from SupportBean sb";
                epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to plan subquery number 1 querying OrderWindow: Failed to validate named window use in subquery, contained-event is only allowed for named windows when not correlated [select *, (select bookId from OrderWindow[books] where sb.TheString = bookId) from SupportBean sb]", ex.Message);
            }
        }
    }
}
