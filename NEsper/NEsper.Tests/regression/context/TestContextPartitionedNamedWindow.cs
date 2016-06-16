///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextPartitionedNamedWindow
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType<SupportBean_S1>();
            configuration.EngineDefaults.LoggingConfig.IsEnableExecutionDebug = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestNWFireAndForgetInvalid()
        {
            _epService.EPAdministrator.CreateEPL("create context SegmentedByString partition by TheString from SupportBean");
    
            _epService.EPAdministrator.CreateEPL("context SegmentedByString create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("context SegmentedByString insert into MyWindow select * from SupportBean");
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 0));
    
            var expected = "Error executing statement: Named window 'MyWindow' is associated to context 'SegmentedByString' that is not available for querying without context partition selector, use the ExecuteQuery(epl, selector) method instead [select * from MyWindow]";
            try {
                _epService.EPRuntime.ExecuteQuery("select * from MyWindow");
            }
            catch (EPException ex) {
                Assert.AreEqual(expected, ex.Message);
            }
    
            var prepared = _epService.EPRuntime.PrepareQueryWithParameters("select * from MyWindow");
            try {
                _epService.EPRuntime.ExecuteQuery(prepared);
            }
            catch (EPException ex) {
                Assert.AreEqual(expected, ex.Message);
            }
        }
    }
}
