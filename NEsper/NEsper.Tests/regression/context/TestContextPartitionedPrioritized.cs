///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextPartitionedPrioritized 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private EPServiceProviderSPI _spi;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("SupportBean", typeof(SupportBean));
            configuration.EngineDefaults.ExecutionConfig.IsPrioritized = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _spi = (EPServiceProviderSPI) _epService;
    
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestFirstEventPrioritized() {
            _epService.EPAdministrator.CreateEPL(
                    "create context SegmentedByMessage partition by TheString from SupportBean");
    
            EPStatement statementWithDropAnnotation = _epService.EPAdministrator.CreateEPL(
                    "@Drop @Priority(1) context SegmentedByMessage select 'test1' from SupportBean");
            SupportUpdateListener statementWithDropAnnotationListener = new SupportUpdateListener();
            statementWithDropAnnotation.Events += statementWithDropAnnotationListener.Update;
    
            EPStatement lowPriorityStatement = _epService.EPAdministrator.CreateEPL(
                    "@Priority(0) context SegmentedByMessage select 'test2' from SupportBean");
            SupportUpdateListener lowPriorityStatementListener = new SupportUpdateListener();
            lowPriorityStatement.Events += lowPriorityStatementListener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("test msg",1));
    
            Assert.IsTrue(statementWithDropAnnotationListener.IsInvoked);
            Assert.IsFalse(lowPriorityStatementListener.IsInvoked);
        }
    
    }
}
