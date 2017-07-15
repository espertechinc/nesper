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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowSubquery 
    {
        private EPServiceProvider epService;
        private SupportUpdateListener listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Logging.IsEnableQueryPlan = true;
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("ABean", typeof(SupportBean_S0));
            listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            listener = null;
        }
    
        [Test]
        public void TestSubqueryTwoConsumerWindow() {
            string epl =
                "\n create window MyWindowTwo.win:length(1) as (mycount long);" +
                "\n @Name('insert-count') insert into MyWindowTwo select 1L as mycount from SupportBean;" +
                "\n create variable long myvar = 0;" +
                "\n @Name('assign') on MyWindowTwo set myvar = (select mycount from MyWindowTwo);";
            EPServiceProvider engine = EPServiceProviderManager.GetDefaultProvider();
            engine.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            engine.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            engine.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(1L, engine.EPRuntime.GetVariableValue("myvar"));   // if the subquery-consumer executes first, this will be null
        }
    
        [Test]
        public void TestSubqueryLateConsumerAggregation() {
            epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from MyWindow where (select count(*) from MyWindow) > 0");
            stmt.AddListener(listener);
            
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            Assert.IsTrue(listener.IsInvoked);
        }
    }
}
